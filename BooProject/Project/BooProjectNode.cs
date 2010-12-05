//
//   Copyright © 2010 Michael Feingold
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Hill30.BooProject.DesignTime;
using Microsoft.VisualStudio.Project;
using System.Windows.Forms;
using System.Drawing;
using EnvDTE;
using Microsoft.VisualStudio.TextManager.Interop;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Hill30.BooProject.AST;
using Hill30.BooProject.LanguageService;
using Hill30.BooProject.Project.ProjectProperties;
using Microsoft.VisualStudio.Shell.Interop;

namespace Hill30.BooProject.Project
{
    [ComVisible(true)]
    public interface IProjectManager
    {
        void Compile();
        IFileNode GetFileNode(string path);
    }

    [ComVisible(true)]
    public sealed class BooProjectNode : ProjectNode, IProjectManager
    {
        #region Enum for image list
        internal enum ProjectIcons
        {
            Project = 0,
            File = 1,
        }
        #endregion

        private static readonly ImageList imageList;
        private static int imageOffset;
        private VSProject vsProject;

        static BooProjectNode()
        {
            imageList = new ImageList { ColorDepth = ColorDepth.Depth24Bit, ImageSize = new Size(16, 16) };

            imageList.Images.AddStrip(GetIcon("BooProjectNode"));
            imageList.Images.AddStrip(GetIcon("BooFileNode"));
            imageList.TransparentColor = Color.Magenta;
        }

        static Bitmap GetIcon(string name)
        {
            return new Bitmap(
                typeof(BooProjectNode).Assembly.GetManifestResourceStream(
                    "Hill30.BooProject.Resources." + name + ".bmp")
                );
        }

        public BooProjectNode()
//            :base()
        {
            OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            OleServiceProvider.AddService(typeof(SVsDesignTimeAssemblyResolution), this, false);

            SupportsProjectDesigner = true;
            CanProjectDeleteItems = true;
            imageOffset = InitializeImageList();
        }

        private int InitializeImageList()
        {
            var result = ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                ImageHandler.AddImage(img);
            }
            return result;
        }

        internal const string ProjectName = "BooProject";

        public override Guid ProjectGuid
        {
            get { return typeof(BooProjectFactory).GUID; }
        }

        public override string ProjectType
        {
            get { return ProjectName; }
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            var result = new Guid[1];
            result[0] = typeof(ProjectProperties.Application).GUID;
            return result;
        }

        protected override Guid[] GetConfigurationDependentPropertyPages()
        {
            var result = new Guid[1];
            result[0] = typeof(ProjectProperties.Build).GUID;
            return result;
        }

        /// <summary>
        /// Return an imageindex
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public override int ImageIndex
        {
            get
            {
                return imageOffset + (int)ProjectIcons.Project;
            }
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new BooProjectNodeProperties(this);
        }

        /// <summary>
        /// Creates the file node.
        /// </summary>
        /// <param name="item">The project element item.</param>
        /// <returns></returns>
        public override FileNode CreateFileNode(ProjectElement item)
        {
            var node = new BooFileNode(this, item);

            node.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            node.OleServiceProvider.AddService(typeof(ProjectItem), node.ServiceCreator, false);
            node.OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            SubmitForCompile(node);
            return node;
        }

        public override bool IsCodeFile(string fileName)
        {
            if (System.IO.Path.GetExtension(fileName) == ".boo")
                return true;
            return base.IsCodeFile(fileName);
        }

        public override bool Navigate(VsTextBuffer buffer, int line, int column)
        {
            var source = (BooSource)GlobalServices.LanguageService.GetOrCreateSource((IVsTextLines)buffer);
            var pos = source.MapPosition(line, column);
            return base.Navigate(buffer, pos.Line, pos.Column);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && resolver != null)
                resolver.Dispose();
            base.Dispose(disposing);
        }

        public void AddTask(ErrorTask task)
        {
            TaskProvider.Tasks.Add(task);
        }

        public void RemoveTask(ErrorTask task)
        {
            TaskProvider.Tasks.Remove(task);
        }

        #region Properties

        public static int ImageOffset { get { return imageOffset; } }

        // ReSharper disable InconsistentNaming
        internal VSProject VSProject
        // ReSharper restore InconsistentNaming
        {
            get { return vsProject ?? (vsProject = new OAVSProject(this)); }
        }
        #endregion

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(VSProject) == serviceType)
            {
                service = VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                service = GetAutomationObject();
            }
            return service;
        }

        IEnumerable<BooFileNode> GetFileEnumerator(HierarchyNode parent)
        {
            for (var node = parent.FirstChild; node != null; node = node.NextSibling)
                if (node is FolderNode)
                    foreach (var file in GetFileEnumerator(node))
                        yield return file;
                else
                    if (node is BooFileNode && IsCodeFile(node.Url))
                        yield return (BooFileNode)node;
                    else
                        continue;
        }

        #region IProjectManager Members

        private BooCompiler compiler;
        private ResolvingUnit resolver;

        private readonly List<BooFileNode> compileList = new List<BooFileNode>();

        internal void SubmitForCompile(BooFileNode file)
        {
            if (IsCodeFile(file.Url) && file.ItemNode.ItemName == "Compile")
                lock (compileList)
                {
                    compileList.Add(file);
                }
        }

        public void Compile()
        {
            lock (this)
            {
                if (compiler == null)
                {
                    var pipeline = CompilerPipeline.GetPipeline("compile");
                    pipeline.BreakOnErrors = false;
                    compiler = new BooCompiler(new CompilerParameters(true) { Pipeline = pipeline });
                    resolver = new ResolvingUnit(this);
                }
            }

            List<BooFileNode> localCompileList;
            lock (compileList)
            {
                localCompileList = new List<BooFileNode>(compileList);
                compileList.Clear();
            }
            if (localCompileList.Count == 0)
                return;

            compiler.Parameters.Input.Clear();
            compiler.Parameters.References.Clear();
            compiler.Parameters.References.Add(resolver);

            var results = new Dictionary<string, Tuple<BooFileNode, CompileResults>>();
            foreach (var file in GetFileEnumerator(this))
                if (localCompileList.Contains(file))
                {
                    var result = new CompileResults(file);
                    var input = file.GetCompilerInput(result);
                    results.Add(input.Name, new Tuple<BooFileNode, CompileResults>(file, result));
                    compiler.Parameters.Input.Add(input);
                }
                else
                    compiler.Parameters.References.Add(file.CompileUnit);

            CompilerStepEventHandler handler = 
                (sender, args) => 
            {
                if (args.Step == ((CompilerPipeline)sender)[0])
                    CompileResults.MapParsedNodes(results, args.Context);
            };

            compiler.Parameters.Pipeline.AfterStep += handler;
            CompileResults.MapCompleted(results, compiler.Run());
            compiler.Parameters.Pipeline.AfterStep -= handler;
            foreach (var item in results.Values)
                item.Item1.SetCompilerResults(item.Item2);
        }

        public IFileNode GetFileNode(string path)
        {
            return GetFileEnumerator(this).FirstOrDefault(file => file.Url == path);
        }

        #endregion
    }
}

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
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Microsoft.VisualStudio.Project;
using System.Windows.Forms;
using System.Drawing;
using EnvDTE;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Hill30.BooProject.AST;

namespace Hill30.BooProject.Project
{
    [ComVisible(true)]
    public interface IProjectManager
    {
        void Compile();

        IFileNode GetFileNode(string path);

        

        BooCompiler CreateCompiler();

        void AddTask(ErrorTask task);

        void RemoveTask(ErrorTask task);

        void NavigateTo(ErrorTask errorTask);
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
        {
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

            return node;
        }

        public override bool IsCodeFile(string fileName)
        {
            if (System.IO.Path.GetExtension(fileName) == ".boo")
                return true;
            return base.IsCodeFile(fileName);
        }

        //public override bool Navigate(VsTextBuffer buffer, int line, int column)
        //{
        //    var languageService = (LanguageService.BooLanguageService)GetService(typeof(LanguageService.BooLanguageService));
        //    var source = (LanguageService.BooSource)languageService.GetOrCreateSource((IVsTextLines)buffer);
        //    var pos = source.MapPosition(line, column);
        //    return base.Navigate(buffer, pos.Line, pos.Column);
        //}

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
                    if (node is BooFileNode)
                        yield return (BooFileNode)node;
                    else
                        continue;
        }

        #region IProjectManager Members

        BooCompiler compiler;

        public void Compile()
        {
            lock (this)
            {
                if (compiler == null)
                {
                    var pipeline = CompilerPipeline.GetPipeline("compile");
                    pipeline.BreakOnErrors = false;
                    compiler = new BooCompiler(new CompilerParameters(true) { Pipeline = pipeline });
                }
            }

            compiler.Parameters.Input.Clear();

            var results = new Dictionary<string, Tuple<BooFileNode, CompileResults>>();
            foreach (var file in GetFileEnumerator(this))
                if (file.NeedsCompilation)
                {
                    var result = new CompileResults();
                    var input = file.GetCompilerInput(result);
                    results.Add(input.Name, new Tuple<BooFileNode, CompileResults>(file, result));
                    compiler.Parameters.Input.Add(input);
                }
                //else
                //    compiler.Parameters.References.Add(file.CompilerResults.CompileUnit);

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
            foreach (var file in GetFileEnumerator(this))
                if (file.Url == path)
                    return file;
            return null;
        }


        public BooCompiler CreateCompiler()
        {
            var pipeline = CompilerPipeline.GetPipeline("compile");
            pipeline.BreakOnErrors = false;
            return new BooCompiler(new CompilerParameters(true) { Pipeline = pipeline });
        }

        public void AddTask(ErrorTask task)
        {
            TaskProvider.Tasks.Add(task);
        }

        public void RemoveTask(ErrorTask task)
        {
            TaskProvider.Tasks.Remove(task);
        }

        public void NavigateTo(ErrorTask task)
        {
            Navigate(task.Document, task.Line, task.Column);
        }

        #endregion

    }
}

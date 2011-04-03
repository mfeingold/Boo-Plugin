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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Hill30.Boo.ASTMapper;
using Hill30.BooProject.Compilation;
using Hill30.BooProject.LanguageService;
using Hill30.BooProject.Project.ProjectProperties;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using VSLangProj;
using CompilerManager = Hill30.BooProject.Compilation.CompilerManager;

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
        private readonly CompilerManager compilerManager;

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
// ReSharper disable AssignNullToNotNullAttribute
                typeof(BooProjectNode).Assembly.GetManifestResourceStream(
                    "Hill30.BooProject.Resources." + name + ".bmp")
// ReSharper restore AssignNullToNotNullAttribute
                );
        }

        public BooProjectNode()
        {
            OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            OleServiceProvider.AddService(typeof(SVsDesignTimeAssemblyResolution), this, false);

            SupportsProjectDesigner = true;
            CanProjectDeleteItems = true;
            imageOffset = InitializeImageList();
            compilerManager = new CompilerManager(this);
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

        internal const string PROJECT_NAME = "BooProject";

        public override Guid ProjectGuid
        {
            get { return typeof(BooProjectFactory).GUID; }
        }

        public override string ProjectType
        {
            get { return PROJECT_NAME; }
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
            result[0] = typeof(Build).GUID;
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

        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            var result = base.CreateReferenceContainerNode();
            result.OnChildAdded += result_OnChildAdded;
            result.OnChildRemoved += result_OnChildRemoved;
            return result;
        }

        void result_OnChildAdded(object sender, HierarchyNodeEventArgs e)
        {
            compilerManager.AddReference((ReferenceNode)e.Child);
        }

        void result_OnChildRemoved(object sender, HierarchyNodeEventArgs e)
        {
            compilerManager.RemoveReference((ReferenceNode)e.Child);
        }

        public override int OnAggregationComplete()
        {
            compilerManager.Initialize();
            return base.OnAggregationComplete();
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
            compilerManager.SubmitForCompile(node);
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
            if (disposing)
                compilerManager.Dispose();
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

        public static IEnumerable<BooFileNode> GetFileEnumerator(HierarchyNode parent)
        {
            for (var node = parent.FirstChild; node != null; node = node.NextSibling)
                if (node is FolderNode)
                    foreach (var file in GetFileEnumerator(node))
                        yield return file;
                else
                    if (node is BooFileNode && parent.ProjectMgr.IsCodeFile(node.Url))
                        yield return (BooFileNode)node;
                    else
                        continue;
        }


        public void SubmitForCompile(BooFileNode file)
        {
            compilerManager.SubmitForCompile(file);
        }

        #region IProjectManager Members

        public void Compile()
        {
            compilerManager.Compile();
        }

        public IFileNode GetFileNode(string path)
        {
            return GetFileEnumerator(this).FirstOrDefault(file => file.Url == path);
        }

        #endregion
    }
}

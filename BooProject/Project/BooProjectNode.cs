using System;
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Microsoft.VisualStudio.Project;
using System.Windows.Forms;
using System.Drawing;
using EnvDTE;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.Project
{
    [ComVisible(true)]
    public interface IProjectManager
    {
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

        public override bool Navigate(VsTextBuffer buffer, int line, int column)
        {
            var languageService = GetService(typeof(LanguageService.Service)) as LanguageService.Service;
            var source = (LanguageService.BooSource)languageService.GetOrCreateSource((IVsTextLines)buffer);
            return base.Navigate(buffer, line-1, source.MapPosition(line, column));
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

        #region IProjectManager Members

        public BooCompiler CreateCompiler()
        {
            return new BooCompiler(
                new CompilerParameters(true)
                {
                    Pipeline = CompilerPipeline.GetPipeline("compile")
                }
            );
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

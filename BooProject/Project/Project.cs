using System;
using Microsoft.VisualStudio.Project;
using System.Windows.Forms;
using System.Drawing;
using EnvDTE;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;

namespace Hill30.BooProject.Project
{
    public sealed class Project : ProjectNode
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

        static Project()
        {
            imageList = new ImageList {ColorDepth = ColorDepth.Depth24Bit, ImageSize = new Size(16, 16)};

            imageList.Images.AddStrip(GetIcon("BooProjectNode"));
            imageList.Images.AddStrip(GetIcon("BooFileNode"));
            imageList.TransparentColor = Color.Magenta;
        }

        static Bitmap GetIcon(string name)
        {
            return new Bitmap(
                typeof(Project).Assembly.GetManifestResourceStream(
                    "Hill30.BooProject.Resources." + name + ".bmp")            
                );
        }

        public Project()
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
            get { return typeof(Factory).GUID; }
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
            var node = new File(this, item);

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

    }
}

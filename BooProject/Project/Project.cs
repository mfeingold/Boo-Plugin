using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using System.Windows.Forms;
using System.Drawing;
using EnvDTE;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;

namespace Hill30.BooProject.Project
{
    public class Project : ProjectNode
    {
        #region Enum for image list
        internal enum ProjectIcons
        {
            Project = 0,
            File = 1,
        }
        #endregion

        private static ImageList imageList;
        internal static int imageOffset;
        private VSLangProj.VSProject vsProject;

        static Project()
        {
            imageList = new ImageList();

            imageList.ColorDepth = ColorDepth.Depth24Bit;
            imageList.ImageSize = new Size(16, 16);
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
            InitializeImageList();
        }

        private void InitializeImageList()
        {
            imageOffset = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
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
            Guid[] result = new Guid[1];
            result[0] = typeof(ProjectProperties.Application).GUID;
            return result;
        }

        protected override Guid[] GetConfigurationDependentPropertyPages()
        {
            Guid[] result = new Guid[1];
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
            File node = new File(this, item);

            node.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            node.OleServiceProvider.AddService(typeof(ProjectItem), node.ServiceCreator, false);
            node.OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);

            return node;
        }

        #region Properties

        public static int ImageOffset { get { return imageOffset; } }

        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (vsProject == null)
                {
                    vsProject = new OAVSProject(this);
                }

                return vsProject;
            }
        }
        #endregion

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(VSLangProj.VSProject) == serviceType)
            {
                service = this.VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                service = this.GetAutomationObject();
            }
            return service;
        }

    }
}

using System;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Hill30.BooProject.Project.Automation;

namespace Hill30.BooProject.Project
{
    public class BooFileNode : FileNode
    {
        #region Fields
        private BooOAFileItem automationObject;
        #endregion

        public BooFileNode(ProjectNode root, ProjectElement e)
			: base(root, e)
		{
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
                if (System.IO.Path.GetExtension(FileName) == ".boo")
                    return BooProjectNode.ImageOffset + (int)BooProjectNode.ProjectIcons.File;
                return base.ImageIndex;
            }
        }

        #region Overriden implementation
        /// <summary>
        /// Gets the automation object for the file node.
        /// </summary>
        /// <returns></returns>
        public override object GetAutomationObject()
        {
            if (automationObject == null)
            {
                automationObject = new BooOAFileItem(ProjectMgr.GetAutomationObject() as OAProject, this);
            }

            return automationObject;
        }
        #endregion



        #region Private implementation
        internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
        {
            get { return CreateServices; }
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(EnvDTE.ProjectItem) == serviceType)
            {
                service = GetAutomationObject();
            }
            return service;
        }
        #endregion
    }
}

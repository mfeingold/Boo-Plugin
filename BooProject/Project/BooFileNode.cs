using System;
using Microsoft.VisualStudio.Project;

namespace Hill30.BooProject.Project
{
    public class BooFileNode : FileNode
    {
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

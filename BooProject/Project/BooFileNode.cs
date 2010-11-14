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

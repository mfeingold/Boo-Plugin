using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using VSLangProj;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hill30.BooProject.Project.ProjectProperties
{
    [CLSCompliant(false), ComVisible(true)]
    public class BooProjectNodeProperties : ProjectNodeProperties
    {
        public BooProjectNodeProperties(ProjectNode node)
			: base(node)
		{
		}

        [Browsable(false)]
        public References DesignTimeReferences
        {
            get
            {
                return null;
            }
        }
    }
}

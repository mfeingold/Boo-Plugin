using System;
using Microsoft.VisualStudio.Project;
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

        // this is a remnant of my struggle with the cross project type resolution. I will have to revisit it 
        // in the future 
        //[Browsable(false)]
        //public References DesignTimeReferences
        //{
        //    get
        //    {
        //        return null;
        //    }
        //}
    }
}

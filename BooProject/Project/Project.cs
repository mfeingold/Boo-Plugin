using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;

namespace Hill30.BooProject.Project
{
    public class Project : ProjectNode
    {
        internal const string ProjectName = "BooProject";

        public override Guid ProjectGuid
        {
            get { return typeof(Factory).GUID; }
        }

        public override string ProjectType
        {
            get { return ProjectName; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project.Automation;
using System.Runtime.InteropServices;

namespace Hill30.BooProject.Project.Automation
{
    [ComVisible(true)]
    public class BooOAFileItem : OAFileItem
    {
        public BooOAFileItem(OAProject oaProject, BooFileNode fileNode) 
            : base(oaProject, fileNode)
        {
        }
    }
}

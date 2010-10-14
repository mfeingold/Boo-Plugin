using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project.Automation;

namespace Hill30.BooProject.Project.Automation
{
    [ComVisible(true)]
    public class BooOAProject : OAProject
    {
        public BooOAProject(BooProjectNode projectNode) 
            : base(projectNode)
        {
        }
    }
}

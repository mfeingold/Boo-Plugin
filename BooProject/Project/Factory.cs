using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Hill30.BooProject.Project
{
    public class Factory : ProjectFactory
    {
        BooProjectPackage package;
        public Factory(BooProjectPackage package)
            : base(package)
        {
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            var result = new Project();
            result.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return result;
        }
    }
}

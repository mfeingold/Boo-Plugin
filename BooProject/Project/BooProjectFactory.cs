using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.Win32;
using System.IO;

namespace Hill30.BooProject.Project
{
    public class BooProjectFactory : ProjectFactory
    {
        BooProjectPackage package;
        public BooProjectFactory(BooProjectPackage package)
            : base(package)
        {
            if (Environment.GetEnvironmentVariable("BooBinPath", EnvironmentVariableTarget.User) == null)
                BuildEngine.SetGlobalProperty("BooBinPath", Path.GetDirectoryName(GetType().Assembly.Location) + @"\Boo_files");
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            var result = new BooProjectNode();
            result.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return result;
        }

    }
}

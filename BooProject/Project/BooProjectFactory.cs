using System;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.IO;

namespace Hill30.BooProject.Project
{
    public class BooProjectFactory : ProjectFactory
    {
        readonly BooProjectPackage package;
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
            result.SetSite((IOleServiceProvider)((IServiceProvider)package).GetService(typeof(IOleServiceProvider)));
            return result;
        }

    }
}

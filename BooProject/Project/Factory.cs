using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.Win32;

namespace Hill30.BooProject.Project
{
    public class Factory : ProjectFactory
    {
        BooProjectPackage package;
        public Factory(BooProjectPackage package)
            : base(package)
        {
            if (Environment.GetEnvironmentVariable("BooBinPath", EnvironmentVariableTarget.User) == null)
               BuildEngine.SetGlobalProperty("BooBinPath", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\VisualStudio\10.0\Extensions\Hill30\BooLanguage\1.0");
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

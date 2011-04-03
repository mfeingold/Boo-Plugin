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
using System.IO;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Hill30.BooProject.Project
{
    public class BooProjectFactory : ProjectFactory
    {
        readonly BooProjectPackage package;
        public BooProjectFactory(BooProjectPackage package)
            : base(package)
        {
            if (Environment.GetEnvironmentVariable("BooBinPath", EnvironmentVariableTarget.User) == null)
                BuildEngine.SetGlobalProperty("BooBinPath", GlobalServices.BinPath);
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

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
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Boo.Lang.Compiler.IO;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.Project
{
    public class BooFileNode : FileNode
    {
        private CompileResults.CompileResults results;

        public BooFileNode(ProjectNode root, ProjectElement e)
			: base(root, e)
		{
            results = new CompileResults.CompileResults();
		}

        public CompileResults.CompileResults CompilerResults
        {
            get { return results; }
        }

        public ICompilerInput PrepareForCompilation()
        {
            IVsHierarchy hier;
            uint itemId;
            IntPtr docData;
            uint cookie;

            string source;
            if (ErrorHandler.Succeeded(
                GlobalServices.RDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, Url, out hier, out itemId, out docData,
                                                   out cookie))
                && docData != IntPtr.Zero
                )
            {
                var doc = Marshal.GetObjectForIUnknown(docData);
                Marshal.Release(docData);

                var lines = doc as IVsTextLines;
                if (lines == null)
                {
                    var bufferProvider = doc as IVsTextBufferProvider;
                    if (bufferProvider != null)
                    {
                        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                        System.Diagnostics.Debug.Assert(lines != null, "IVsTextLines does not implement IVsTextBuffer");
                    }
                }
                source = File.ReadAllText(lines.);
            }
            else
                source = File.ReadAllText(Url);

            results.Initialize(Url, source);
            return new StringInput(Url, source);
        }

        /// <summary>
        /// Return an imageindex
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public override int ImageIndex
        {
            get
            {
                if (System.IO.Path.GetExtension(FileName) == ".boo")
                    return BooProjectNode.ImageOffset + (int)BooProjectNode.ProjectIcons.File;
                return base.ImageIndex;
            }
        }

        #region Private implementation
        internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
        {
            get { return CreateServices; }
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(EnvDTE.ProjectItem) == serviceType)
            {
                service = GetAutomationObject();
            }
            return service;
        }
        #endregion
    }
}

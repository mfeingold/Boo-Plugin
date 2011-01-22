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
using System.ComponentModel.Composition;
using Hill30.BooProject.LanguageService;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.ComponentModelHost;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Design;

namespace Hill30.BooProject
{
    public class GlobalServices
    {

        public static readonly IVsRunningDocumentTable RDT = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));

        public static readonly BooLanguageService LanguageService = (BooLanguageService)Package.GetGlobalService(typeof (BooLanguageService));

        public static readonly DynamicTypeService TypeService = (DynamicTypeService)Package.GetGlobalService(typeof(DynamicTypeService));

        public static IFileNode GetFileNodeForBuffer(ITextBuffer buffer) { return handler.GetFileNodeForBufferImpl(buffer); }

        public static IProjectManager GetProjectManagerForFile(string filePath) { return handler.GetProjectManagerForFileImpl(filePath); }

        public static IFileNode GetFileNodeForView(IVsTextView view) { return handler.GetFileNodeForViewImpl(view);}

        public static ITextBuffer GetBufferForVsBuffer(IVsTextBuffer buffer) { return handler.GetBufferForVsBufferImpl(buffer); }

        private static readonly GlobalServices handler = new GlobalServices();

        private GlobalServices()
        {
            ((IComponentModel)Package.GetGlobalService(typeof(SComponentModel))).DefaultCompositionService.SatisfyImportsOnce(this);
            documentFactoryService.TextDocumentCreated += documentFactoryService_TextDocumentCreated;
            documentFactoryService.TextDocumentDisposed += documentFactoryService_TextDocumentDisposed;
        }

// ReSharper disable InconsistentNaming
        void documentFactoryService_TextDocumentCreated(object sender, TextDocumentEventArgs e)
// ReSharper restore InconsistentNaming
        {
            var fileNode = GetFileNodeForFileImpl(e.TextDocument.FilePath);
            if (fileNode != null)
                fileNode.Bind(e.TextDocument.TextBuffer);
        }

// ReSharper disable InconsistentNaming
        void documentFactoryService_TextDocumentDisposed(object sender, TextDocumentEventArgs e)
// ReSharper restore InconsistentNaming
        {
            var fileNode = GetFileNodeForFileImpl(e.TextDocument.FilePath);
            if (fileNode != null)
                fileNode.Bind(null);
        }

        [Import]
        private IVsEditorAdaptersFactoryService bufferAdapterService;

        [Import]
        private ITextDocumentFactoryService documentFactoryService;

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider serviceProvider;

        private IFileNode GetFileNodeForBufferImpl(ITextBuffer buffer)
        {
            var doc = buffer.Properties[typeof(ITextDocument)] as ITextDocument;
            if (doc == null)
                return null;

            return GetFileNodeForFileImpl(doc.FilePath);
        }
                    
        private IFileNode GetFileNodeForFileImpl(string filePath)
        {
            var projectManager = GetProjectManagerForFileImpl(filePath);
            if (projectManager == null)
                return null;

            return projectManager.GetFileNode(filePath);
        }

        private IFileNode GetFileNodeForViewImpl(IVsTextView view)
        {
            IVsTextLines lines;
            if (ErrorHandler.Failed(view.GetBuffer(out lines)))
                return null;

            var buffer = lines as IVsTextBuffer;
            if (buffer == null)
                return null;

            return GetFileNodeForBuffer(bufferAdapterService.GetDocumentBuffer(buffer));
        }

        private ITextBuffer GetBufferForVsBufferImpl(IVsTextBuffer buffer)
        {
            return (ITextBuffer)LanguageService.Invoke(new Action(()=> bufferAdapterService.GetDocumentBuffer(buffer)), new object[] {});
        }

        private IProjectManager GetProjectManagerForFileImpl(string filePath)
        {
            var hier = new RunningDocumentTable(serviceProvider).GetHierarchyItem(filePath);

            if (hier == null)
                return null;

            object value;

            if (ErrorHandler.Failed(hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeName, out value)))
                return null;

            if (value.ToString() != BooProjectNode.PROJECT_NAME)
                return null;

            if (ErrorHandler.Failed(hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Root, out value)))
                return null;

            var pointer = new IntPtr((int)value);
            try
            {
                return Marshal.GetObjectForIUnknown(pointer) as IProjectManager;
            }
            finally
            {
                Marshal.Release(pointer);
            }
        }

     }
}

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
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Hill30.BooProject.LanguageService.Colorizer;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooLanguageService : Microsoft.VisualStudio.Package.LanguageService, IOleComponent
    {
        [Import]
        public IVsEditorAdaptersFactoryService BufferAdapterService { get; private set; }

        [Import]
        public IClassificationTypeRegistryService ClassificationTypeRegistry { get; private set; }

        internal static void Register(IServiceContainer container)
        {
            // Proffer the service.
            var langService = new BooLanguageService();
            langService.SetSite(container);

            ((IComponentModel)langService.GetService(typeof(SComponentModel))).DefaultCompositionService.SatisfyImportsOnce(langService);

            container.AddService(typeof(BooLanguageService), langService, true);
            langService.Start();

            // as stupid as it looks this timer is necessary to keep the parsing thread alive
            // without it the parsing thread will be shut down after 10 sec of inactivity, which means that
            // the next parsing request will trigger creating of a different parsing thread which will break 
            // the type resolver supplied by DynamicTypeService
            langService.keepAliveTimer = new System.Threading.Timer(langService.KeepAlive, langService, 7000, 7000);
        }

// ReSharper disable UnaccessedField.Local
        private System.Threading.Timer keepAliveTimer;
// ReSharper restore UnaccessedField.Local

        private void KeepAlive(object langService)
        {
            using( 
                BeginParse
                    (
                        new ParseRequest(0,0, new TokenInfo(), null, null, ParseReason.None, null, null, false)
                        , null
                    )
                .AsyncWaitHandle
                )
            {
            }
        }

        internal static void Stop(IServiceContainer container)
        {
            var service = container.GetService(typeof(BooLanguageService))
                                      as BooLanguageService;
            if (service == null || service.mComponentId == 0)
                return;

            var mgr = container.GetService(typeof(SOleComponentManager))
                                       as IOleComponentManager;
            if (mgr != null)
            {
                mgr.FRevokeComponent(service.mComponentId);
            }
            service.mComponentId = 0;
        }

        public override string GetFormatFilterList()
        {
            return "Boo files(*.boo)|*.boo";
        }

        private LanguagePreferences mPreferences;

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (mPreferences == null)
            {
                mPreferences = new LanguagePreferences(Site, typeof(BooLanguageService).GUID, Name );
                mPreferences.Init();
            }
            return mPreferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return new Boo.ASTMapper.Scanner.Scanner(() => GetLanguagePreferences().TabSize);
        }

        public override string Name
        {
            get { return Constants.LanguageName; }
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            return new BooSource(this, buffer, GetColorizer(buffer)) {LastParseTime = 0};
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            var source = GetSource(req.View) as BooSource;

            if (source != null && req.Reason == ParseReason.Check)
                source.Compile(req);

            return new BooAuthoringScope(source);
        }

        public override int GetItemCount(out int count)
        {
            count = Formats.ColorableItems.Length - 1;
            return VSConstants.S_OK;
        }

        public override int GetColorableItem(int index, out IVsColorableItem item)
        {
            item = Formats.ColorableItems[index];
            return VSConstants.S_OK;
        }

        public override TypeAndMemberDropdownBars CreateDropDownHelper(IVsTextView forView)
        {
            var fileNode = GlobalServices.GetFileNodeForView(forView);
            if (fileNode == null)
                return null;
            return new BooTypeAndMemberDropdownBars(this, fileNode);
        }

        private void Start()
        {
            // Register a timer to call our language service during
            // idle periods.
            var mgr = Site.GetService(typeof(SOleComponentManager)) as IOleComponentManager;

            if (mComponentId == 0 && mgr != null)
            {
                var crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 100;
                mgr.FRegisterComponent(this, crinfo, out mComponentId);
            }
        }

        private uint mComponentId;

        #region IOleComponent Members

        public int FDoIdle(uint grfidlef)
        {
            bool bPeriodic = (grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;
            // Use typeof(TestLanguageService) because we need to
            // reference the GUID for our language service.
            var service = GetService(typeof(BooLanguageService))
                                      as BooLanguageService;
            if (service != null)
            {
                service.OnIdle(bPeriodic);
            }
            return 0;
        }

        public int FContinueMessageLoop(uint uReason,
                                        IntPtr pvLoopData,
                                        MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved,
                              uint message,
                              IntPtr wParam,
                              IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic,
                                       int fSameComponent,
                                       OLECRINFO[] pcrinfo,
                                       int fHostIsActivating,
                                       OLECHOSTINFO[] pchostinfo,
                                       uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadId)
        {
        }

        public void OnEnterState(uint uStateId, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }

        #endregion
    }
}

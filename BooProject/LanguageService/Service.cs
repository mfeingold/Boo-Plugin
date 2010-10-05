using System;
using Boo.Lang.Compiler.Steps;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;

namespace Hill30.BooProject.LanguageService
{
    public class Service : Microsoft.VisualStudio.Package.LanguageService, IOleComponent
    {
        
        internal static void Register(IServiceContainer container)
        {
            // Proffer the service.
            var langService = new Service();
            langService.SetSite(container);
            container.AddService(typeof(Service),
                                        langService,
                                        true);
            langService.Start();
        }

        internal static void Stop(IServiceContainer container)
        {
            var service = container.GetService(typeof(Service))
                                      as Service;
            if (service == null || service.mComponentID == 0)
                return;

            var mgr = container.GetService(typeof(SOleComponentManager))
                                       as IOleComponentManager;
            if (mgr != null)
            {
                mgr.FRevokeComponent(service.mComponentID);
            }
            service.mComponentID = 0;
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
                mPreferences = new LanguagePreferences(Site, typeof(Service).GUID, Name );
                mPreferences.Init();
            }
            return mPreferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return new Scanner();
        }

        public override string Name
        {
            get { return Constants.LanguageName; }
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            var s = base.CreateSource(buffer);
            s.LastParseTime = 0;
            return s;
        }

        public class BooAuthoringScope : AuthoringScope
        {
            public override string GetDataTipText(int line, int col, out TextSpan span)
            {
                span = new TextSpan();
                return null;
            }

            public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
            {
                return null;
            }

            public override Methods GetMethods(int line, int col, string name)
            {
                return null;
            }

            public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
            {
                span = new TextSpan();
                return null;
            }
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            var compiler = new BooCompiler(
                new CompilerParameters(true)
                        {
                            Pipeline = CompilerPipeline.GetPipeline("compile")
                        }
                );
            var context = compiler.Run(BooParser.ParseString("code", req.Text));

            return new BooAuthoringScope();
        }

        private void Start()
        {
            // Register a timer to call our language service during
            // idle periods.
            var mgr = Site.GetService(typeof(SOleComponentManager)) as IOleComponentManager;

            if (mComponentID == 0 && mgr != null)
            {
                var crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                mgr.FRegisterComponent(this, crinfo, out mComponentID);
            }
        }

        private uint mComponentID;

        #region IOleComponent Members

        public int FDoIdle(uint grfidlef)
        {
            bool bPeriodic = (grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;
            // Use typeof(TestLanguageService) because we need to
            // reference the GUID for our language service.
            var service = GetService(typeof(Service))
                                      as Service;
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

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
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

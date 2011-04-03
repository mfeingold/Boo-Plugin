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
using Hill30.Boo.ASTMapper;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public sealed class BooSource : Source
    {
        private readonly IFileNode fileNode;
        private readonly IProjectManager projectManager;

        public BooSource(BooLanguageService service, IVsTextLines buffer, Microsoft.VisualStudio.Package.Colorizer colorizer)
            : base(service, buffer, colorizer)
        {
            projectManager = GlobalServices.GetProjectManagerForFile(GetFilePath());
            fileNode = projectManager.GetFileNode(GetFilePath());
            fileNode.ShowMessages();
            fileNode.Recompiled +=
                (sender, eventArgs) => service.Invoke(
                    new Action<BooLanguageService>(SynchronizeDropDowns), 
                    new object[] {service}
                                           );
        }

        static void SynchronizeDropDowns(BooLanguageService service)
        {
            service.SynchronizeDropdowns();
        }

        internal void Compile(ParseRequest req)
        {
            projectManager.Compile();
        }

        public override void OnChangeLineText(TextLineChange[] lineChange, int last)
        {
            base.OnChangeLineText(lineChange, last);
            fileNode.SubmitForCompile();
        }

        internal string GetDataTipText(int line, int col, out TextSpan span)
        {
            var token = fileNode.GetMappedToken(line, col);
            if (token == null)
            {
                span = new TextSpan();
                return "";
            }
            return token.GetDataTiptext(out span);
        }

        internal Declarations GetDeclarations(int line, int col, TokenInfo info, ParseReason reason)
        {
            var token = fileNode.GetAdjacentMappedToken(line, col);
            if (token == null)
                return new BooDeclarations();
            return token.GetDeclarations(info, reason);
        }

        internal string Goto(int line, int col, out TextSpan span)
        {
            var token = fileNode.GetMappedToken(line, col);
            if (token == null)
            {
                span = new TextSpan();
                return null;
            }
            return token.Goto(out span);
        }

        public override CommentInfo GetCommentFormat()
        {
            return new CommentInfo {BlockStart = "/*", BlockEnd = "*/", UseLineComments = false};
        }

        public CompileResults.BufferPoint MapPosition(int line, int column)
        {
            return fileNode.MapPosition(line, column);
        }

        public override void Dispose()
        {
            fileNode.HideMessages();
            base.Dispose();
        }

    }
}

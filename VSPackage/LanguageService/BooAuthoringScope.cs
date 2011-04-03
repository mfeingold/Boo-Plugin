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

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    /// <summary>
    /// Implements the QuickInfo/ CodeCompletion/Goto functionality
    /// </summary>
    /// <remarks>
    /// The methods in this class are responsible for locating appropriate mapped nodes
    /// based on the given coordinates an delegating the response generation to the appropriate methods
    /// of the located node(s).
    /// </remarks>
    // TODO: hide goto declaration menu item
    // TODO: change the Declarations property to GetDeclarations method to support different parse reasons
    // TODO: implement the GetMethods
    // TODO: implement goto to a file from another project(in another language) 
    public class BooAuthoringScope : AuthoringScope
    {
        private readonly BooSource source;

        public BooAuthoringScope(BooSource source)
        {
            this.source = source;
        }

        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            return source.GetDataTipText(line, col, out span);
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            return source.GetDeclarations(line, col, info, reason);
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            return null;
        }

        public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            return source.Goto(line, col, out span);
        }
    }

}

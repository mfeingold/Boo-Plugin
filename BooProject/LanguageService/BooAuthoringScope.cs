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

using System.Linq;
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
            var nodes = source.GetNodes(line, col, node=>node.QuickInfoTip != null);
            var tip = "";
            span = new TextSpan();
            foreach (var node in nodes)
            {
                span = span.Union(node.TextSpan);
                if (tip != "")
                    tip += '\n';
                tip += node.QuickInfoTip;
            }
            return tip;
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            var node = source.GetAdjacentNodes(line, col, n=>n.Declarations.GetCount() > 0).FirstOrDefault();
            if (node == null)
                return new BooDeclarations();
            return node.Declarations;
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            return null;
        }

        public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            var node = source.GetNodes(line, col, n=>n.DeclarationNode != null).FirstOrDefault();
            if (node != null)
            {
                if (node.DeclarationNode != null)
                {
                    span = node.DeclarationNode.TextSpan;
                    return node.DeclarationNode.LexicalInfo.FullPath;
                }
            }
            span = new TextSpan();
            return null;
        }
    }

}

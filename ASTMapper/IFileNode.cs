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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Hill30.Boo.ASTMapper.AST;
using Hill30.Boo.ASTMapper.AST.Nodes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace Hill30.Boo.ASTMapper
{
    [ComVisible(true)]
    public interface IFileNode
    {
        event EventHandler Recompiled;
        
        MappedToken GetMappedToken(int line, int col);

        MappedToken GetAdjacentMappedToken(int line, int col);

        IEnumerable<MappedTypeDefinition> Types { get; }

        CompileResults.BufferPoint MapPosition(int line, int column);

        IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans);

        IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span);

        void HideMessages();

        void ShowMessages();

        void Bind(ITextBuffer textBuffer);

        void SubmitForCompile();
    }
}

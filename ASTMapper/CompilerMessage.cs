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
using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Hill30.Boo.ASTMapper
{
    public class CompilerMessage
    {
        private ErrorTask task;

        public CompilerMessage(LexicalInfo lexicalInfo, string code, string message, TaskErrorCategory errorCategory)
        {
            LexicalInfo = lexicalInfo;
            Code = code;
            Message = message;
            ErrorCategory = errorCategory;
        }
        
        public LexicalInfo LexicalInfo { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public TaskErrorCategory ErrorCategory { get; private set; }
        public virtual IVsHierarchy FileNode { get { return null; } }

        public void Show(Action<ErrorTask> exposer, Action<ErrorTask> navigator)
        {
            task = new ErrorTask
            {
                Document = LexicalInfo.FileName,
                ErrorCategory = ErrorCategory,
                Line = LexicalInfo.Line-1,
                Column = LexicalInfo.Column-1,
                Priority = TaskPriority.High,
                Text = Message,
                HierarchyItem = FileNode,
                Category = TaskCategory.CodeSense
            };
            task.Navigate += 
                (sender, e) => 
                    {
                        var errortask = sender as ErrorTask;
                        if (errortask != null)
                            navigator(errortask);
                    }
                ;
            exposer(task);
        }

        public void Hide(Action<ErrorTask> remover)
        {
            if (task != null)
                remover(task);
            task = null;
        }

    }
}

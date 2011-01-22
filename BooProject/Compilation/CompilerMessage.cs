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
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Shell;

namespace Hill30.BooProject.Compilation
{
    public class CompilerMessage
    {
        private ErrorTask task;
        private readonly BooFileNode fileNode;
        private readonly BooProjectNode projectManager;

        public CompilerMessage(BooFileNode fileNode, LexicalInfo lexicalInfo, string code, string message, TaskErrorCategory errorCategory)
        {
            this.fileNode = fileNode;
            projectManager = (BooProjectNode) fileNode.ProjectMgr;
            LexicalInfo = lexicalInfo;
            Code = code;
            Message = message;
            ErrorCategory = errorCategory;
        }
        
        public LexicalInfo LexicalInfo { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public TaskErrorCategory ErrorCategory { get; private set; }

        public void Show()
        {
            task = new ErrorTask
            {
                Document = LexicalInfo.FileName,
                ErrorCategory = ErrorCategory,
                Line = LexicalInfo.Line-1,
                Column = LexicalInfo.Column-1,
                Priority = TaskPriority.High,
                Text = Message,
                HierarchyItem = fileNode,
                Category = TaskCategory.CodeSense
            };
            task.Navigate += TaskNavigate;
            projectManager.AddTask(task);
        }

        private void TaskNavigate(object sender, EventArgs e)
        {
            var errortask = sender as ErrorTask;
            if (errortask != null)
                projectManager.Navigate(errortask.Document, errortask.Line, errortask.Column);
        }

        public void Hide()
        {
            if (task != null)
                projectManager.RemoveTask(task);
            task = null;
        }

    }
}

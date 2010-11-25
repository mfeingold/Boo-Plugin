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
using Boo.Lang.Compiler;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Hill30.BooProject.Project;

namespace Hill30.BooProject.LanguageService.TaskItems
{
    public class Collection
    {
        readonly List<ErrorTask> tasks = new List<ErrorTask>();
        readonly IProjectManager projectManager;
        readonly IVsHierarchy hier;
        private CompilerWarningCollection warnings = new CompilerWarningCollection();
        private CompilerErrorCollection errors = new CompilerErrorCollection();

        public Collection(IProjectManager projectManager, IVsHierarchy hier)
        {
            this.projectManager = projectManager;
            this.hier = hier;
        }

        public CompilerErrorCollection Errors { get { return errors; } }
        public CompilerWarningCollection Warnings { get { return warnings; } }

        internal void CreateMessages(CompilerErrorCollection compilerErrors, CompilerWarningCollection compilerWarnings)
        {
            errors = compilerErrors;
            warnings = compilerWarnings;
            foreach (var error in compilerErrors)
            {
                var task = 
                    new ErrorTask
                    {
                        Document = error.LexicalInfo.FileName,
                        ErrorCategory = TaskErrorCategory.Error,
                        Line = error.LexicalInfo.Line,
                        Column = error.LexicalInfo.Column,
                        Priority = TaskPriority.High,
                        Text = error.Code + ' ' + error.Message,
                        HierarchyItem = hier,
                        Category = TaskCategory.CodeSense
                    };
                task.Navigate += TaskNavigate;
                tasks.Add(task);
                projectManager.AddTask(task);
            }
            foreach (var warning in compilerWarnings)
            {
                var task =
                    new ErrorTask
                    {
                        Document = warning.LexicalInfo.FileName,
                        ErrorCategory = TaskErrorCategory.Warning,
                        Line = warning.LexicalInfo.Line,
                        Column = warning.LexicalInfo.Column,
                        Priority = TaskPriority.High,
                        Text = warning.Code + ' ' + warning.Message,
                        HierarchyItem = hier,
                        Category = TaskCategory.CodeSense
                    };
                task.Navigate += TaskNavigate;
                tasks.Add(task);
                projectManager.AddTask(task);
            }
        }

        private void TaskNavigate(object sender, EventArgs e)
        {
            projectManager.NavigateTo((ErrorTask)sender);
        }

        internal void Clear()
        {
            foreach (var task in tasks)
                projectManager.RemoveTask(task);
            errors.Clear();
            warnings.Clear();
        }
    }
}

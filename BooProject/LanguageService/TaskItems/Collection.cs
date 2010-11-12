using System;
using System.Collections.Generic;
using Boo.Lang.Compiler;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Hill30.BooProject.Project;

namespace Hill30.BooProject.LanguageService.TaskItems
{
    public class Collection : IDisposable
    {
        readonly List<ErrorTask> tasks = new List<ErrorTask>();
        readonly IProjectManager projectManager;
        readonly IVsHierarchy hier;

        public Collection(IProjectManager projectManager, IVsHierarchy hier)
        {
            this.projectManager = projectManager;
            this.hier = hier;
        }

        public void CreateErrorMessages(CompilerErrorCollection errors)
        {
            foreach (var error in errors)
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
        }

        private void TaskNavigate(object sender, EventArgs e)
        {
            projectManager.NavigateTo((ErrorTask)sender);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (var task in tasks)
                projectManager.RemoveTask(task);
        }

        #endregion
    }
}

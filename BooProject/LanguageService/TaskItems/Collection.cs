using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Hill30.BooProject.Project;
using Hill30.BooProject.LanguageService.NodeMapping;
using System.IO;

namespace Hill30.BooProject.LanguageService.TaskItems
{
    public class Collection : IDisposable
    {
        List<ErrorTask> tasks = new List<ErrorTask>();
        IProjectManager projectManager;
        IVsHierarchy hier;
        Mapper mapper;


        public Collection(IProjectManager projectManager, IVsHierarchy hier, Mapper mapper)
        {
            this.projectManager = projectManager;
            this.hier = hier;
            this.mapper = mapper;
        }

        public void CreateErrorMessages(CompilerErrorCollection errors)
        {
            foreach (var error in errors)
            {
                var column = mapper.MapPosition(error.LexicalInfo.Line, error.LexicalInfo.Column);
                ErrorTask task = new ErrorTask() 
                    {
                        Document = error.LexicalInfo.FileName,
                        ErrorCategory = TaskErrorCategory.Error,
                        Line = error.LexicalInfo.Line,
                        Column = 10,// column-1,
                        Priority = TaskPriority.High,
                        Text = error.Code + ' ' + error.Message,
                        HierarchyItem = hier,
                        Category = TaskCategory.CodeSense
                    };
                task.Navigate += task_Navigate;
                this.tasks.Add(task);
                projectManager.AddTask(task);
            }
        }

        void task_Navigate(object sender, EventArgs e)
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

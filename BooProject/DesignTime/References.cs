using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;

namespace Hill30.BooProject.DesignTime
{
    [ComVisible(true)]
    public class DesignReferences : References
    {
        private OAReferences projRefs;
        public DesignReferences(OAReferences projRefs)
        {
            this.projRefs = projRefs;
        }

        #region References Members

        public Reference Add(string bstrPath)
        {
            return projRefs.Add(bstrPath);
        }

        public Reference AddActiveX(string bstrTypeLibGuid, int lMajorVer = 0, int lMinorVer = 0, int lLocaleId = 0, string bstrWrapperTool = "")
        {
            return projRefs.AddActiveX(bstrTypeLibGuid, lMajorVer, lMinorVer, lLocaleId, bstrWrapperTool);
        }

        public Reference AddProject(EnvDTE.Project pProject)
        {
            return projRefs.AddProject(pProject);
        }

        public EnvDTE.Project ContainingProject
        {
            get { return projRefs.ContainingProject; }
        }

        public int Count
        {
            get { return projRefs.Count; }
        }

        public EnvDTE.DTE DTE
        {
            get { return projRefs.DTE; }
        }

        public Reference Find(string bstrIdentity)
        {
            return projRefs.Find(bstrIdentity);
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return projRefs.GetEnumerator();
        }

        public Reference Item(object index)
        {
            return projRefs.Item(index);
        }

        public object Parent
        {
            get { return projRefs.Parent; }
        }

        #endregion
    }
}

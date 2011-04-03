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
using Microsoft.VisualStudio.Project;
using System.Runtime.Versioning;
using Hill30.BooProject.Project.Attributes;
using System.IO;
using Microsoft.VisualStudio;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

using LocDisplayNameAttribute = Hill30.BooProject.Project.Attributes.LocDisplayNameAttribute;

namespace Hill30.BooProject.Project.ProjectProperties
{
	public class Application : SettingsPage
	{

        #region Fields
        private string assemblyName;
        private OutputType outputType;
        private string defaultNamespace;
        private string startupObject;
        private string applicationIcon;
        private FrameworkName targetFrameworkMoniker;
        private bool allowUnsafe;
        private bool useDuckTyping;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Explicitly defined default constructor.
        /// </summary>
        public Application()
        {
            Name = Resources.GetString(Resources.ApplicationCaption);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets Assembly Name.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.AssemblyName)]
        [LocDisplayName(Resources.AssemblyName)]
        [ResourcesDescriptionAttribute(Resources.AssemblyNameDescription)]
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets OutputType.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Application)]
        [LocDisplayName(Resources.OutputType)]
        [ResourcesDescriptionAttribute(Resources.OutputTypeDescription)]
        public OutputType OutputType
        {
            get { return outputType; }
            set { outputType = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets Default Namespace.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Application)]
        [LocDisplayName(Resources.DefaultNamespace)]
        [ResourcesDescriptionAttribute(Resources.DefaultNamespaceDescription)]
        public string DefaultNamespace
        {
            get { return defaultNamespace; }
            set { defaultNamespace = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets Startup Object.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Application)]
        [LocDisplayName(Resources.StartupObject)]
        [ResourcesDescriptionAttribute(Resources.StartupObjectDescription)]
        public string StartupObject
        {
            get { return startupObject; }
            set { startupObject = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets Application Icon.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Application)]
        [LocDisplayName(Resources.ApplicationIcon)]
        [ResourcesDescriptionAttribute(Resources.ApplicationIconDescription)]
        public string ApplicationIcon
        {
            get { return applicationIcon; }
            set { applicationIcon = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets Assembly Name.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.AllowUnsafe)]
        [ResourcesDescriptionAttribute(Resources.AllowUnsafeDescription)]
        public bool AllowUnsafe
        {
            get { return allowUnsafe; }
            set { allowUnsafe = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets Assembly Name.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.UseDuckTyping)]
        [ResourcesDescriptionAttribute(Resources.UseDuckTypingDescription)]
        public bool UseDuckTyping
        {
            get { return useDuckTyping; }
            set { useDuckTyping = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets the path to the project file.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Project)]
        [LocDisplayName(Resources.ProjectFile)]
        [ResourcesDescriptionAttribute(Resources.ProjectFileDescription)]
        public string ProjectFile
        {
            get { return Path.GetFileName(ProjectMgr.ProjectFile); }
        }

        /// <summary>
        /// Gets the path to the project folder.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Project)]
        [LocDisplayName(Resources.ProjectFolder)]
        [ResourcesDescriptionAttribute(Resources.ProjectFolderDescription)]
        public string ProjectFolder
        {
            get { return Path.GetDirectoryName(ProjectMgr.ProjectFolder); }
        }

        /// <summary>
        /// Gets the output file name depending on current OutputType.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Project)]
        [LocDisplayName(Resources.OutputFile)]
        [ResourcesDescriptionAttribute(Resources.OutputFileDescription)]
        public string OutputFile
        {
            get
            {
                switch (outputType)
                {
                    case OutputType.Exe:
                    case OutputType.WinExe:
                        {
                            return assemblyName + ".exe";
                        }

                    default:
                        {
                            return assemblyName + ".dll";
                        }
                }
            }
        }

        /// <summary>
        /// Gets or sets Target Platform PlatformType.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.Project)]
        [LocDisplayName(Resources.TargetFrameworkMoniker)]
        [ResourcesDescriptionAttribute(Resources.TargetFrameworkMonikerDescription)]
        [PropertyPageTypeConverter(typeof(FrameworkNameConverter))]
        public FrameworkName TargetFrameworkMoniker
        {
            get { return targetFrameworkMoniker; }
            set { targetFrameworkMoniker = value; IsDirty = true; }
        }

        #endregion

        #region Overriden Implementation

        /// <summary>
        /// Bind properties.
        /// </summary>
        protected override void BindProperties()
        {
            if (ProjectMgr == null)
            {
                return;
            }

            assemblyName = ProjectMgr.GetProjectProperty("AssemblyName", true);

            var temp = ProjectMgr.GetProjectProperty("OutputType", false);

            if (!string.IsNullOrEmpty(temp))
            {
                try
                {
                    outputType = (OutputType)Enum.Parse(typeof(OutputType), temp);
                }
                catch (ArgumentException)
                {
                }
            }

            defaultNamespace = ProjectMgr.GetProjectProperty("RootNamespace", false);
            startupObject = ProjectMgr.GetProjectProperty("StartupObject", false);
            applicationIcon = ProjectMgr.GetProjectProperty("ApplicationIcon", false);
            Boolean.TryParse(ProjectMgr.GetProjectProperty("AllowUnsafeBlocks", false), out allowUnsafe);
            Boolean.TryParse(ProjectMgr.GetProjectProperty("Ducky", false), out useDuckTyping);

            try
            {
                targetFrameworkMoniker = ProjectMgr.TargetFrameworkMoniker;
            }
            catch (ArgumentException)
            {
            }
        }

        /// <summary>
        /// Apply Changes on project node.
        /// </summary>
        /// <returns>E_INVALIDARG if internal ProjectMgr is null, otherwise applies changes and return S_OK.</returns>
        protected override int ApplyChanges()
        {
            if (ProjectMgr == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            var propertyPageFrame = (IVsPropertyPageFrame)ProjectMgr.Site.GetService((typeof(SVsPropertyPageFrame)));
            bool reloadRequired = ProjectMgr.TargetFrameworkMoniker != targetFrameworkMoniker;

            ProjectMgr.SetProjectProperty("AssemblyName", assemblyName);
            ProjectMgr.SetProjectProperty("OutputType", outputType.ToString());
            ProjectMgr.SetProjectProperty("RootNamespace", defaultNamespace);
            ProjectMgr.SetProjectProperty("StartupObject", startupObject);
            ProjectMgr.SetProjectProperty("ApplicationIcon", applicationIcon);
            ProjectMgr.SetProjectProperty("AllowUnsafeBlocks", allowUnsafe.ToString());
            ProjectMgr.SetProjectProperty("Ducky", useDuckTyping.ToString());

            if (reloadRequired)
            {
                if (MessageBox.Show(SR.GetString(SR.ReloadPromptOnTargetFxChanged), SR.GetString(SR.ReloadPromptOnTargetFxChangedCaption), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ProjectMgr.TargetFrameworkMoniker = targetFrameworkMoniker;
                }
            }

            IsDirty = false;

            if (reloadRequired)
            {
                // This prevents the property page from displaying bad data from the zombied (unloaded) project
                propertyPageFrame.HideFrame();
                propertyPageFrame.ShowFrame(GetType().GUID);
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}

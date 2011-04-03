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
using System.Linq;
using Hill30.BooProject.Project.Attributes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

using LocDisplayNameAttribute = Hill30.BooProject.Project.Attributes.LocDisplayNameAttribute;

namespace Hill30.BooProject.Project.ProjectProperties
{
    public class Build : SettingsPage
    {
        #region Fields
        private string conditionalSymbols;
        private bool defineDebug;
        private bool defineTrace;
        private string outputPath;
        #endregion Fields


        #region Constructors
        /// <summary>
        /// Explicitly defined default constructor.
        /// </summary>
        public Build()
        {
            Name = Resources.GetString(Resources.BuildCaption);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Conditional Symblos.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.ConditionalSymbols)]
        [ResourcesDescriptionAttribute(Resources.ConditionalSymbolsDescription)]
        public string ConditionalSymbols
        {
            get { return conditionalSymbols; }
            set { conditionalSymbols = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets whether Debug Symbol is set.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.DefineDebug)]
        [ResourcesDescriptionAttribute(Resources.DefineDebugDescription)]
        public bool DefineDebug
        {
            get { return defineDebug; }
            set { defineDebug = value; IsDirty = true; }
        }

        /// <summary>
        /// Gets or sets whether Trace Symbol is set.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.DefineTrace)]
        [ResourcesDescriptionAttribute(Resources.DefineTraceDescription)]
        public bool DefineTrace
        {
            get { return defineTrace; }
            set { defineTrace = value; IsDirty = true; }
        }

        [ResourcesCategoryAttribute(Resources.OutputCaption)]
        [LocDisplayName(Resources.OutputPath)]
        [SRDescriptionAttribute(Resources.OutputPathDescription)]
        public string OutputPath
        {
            get { return outputPath; }
            set { outputPath = value; IsDirty = true; }
        }

        #endregion

        protected override void BindProperties()
        {
            if (ProjectMgr == null)
            {
                return;
            }

            var tempConditionalSymbols = GetConfigProperty("DefineConstants")
                .Where(c => Char.IsLetterOrDigit(c) || c == '_' || c == ';').Aggregate("", (current, c) => current + c);

            tempConditionalSymbols = ParseSymbol(tempConditionalSymbols, "DEBUG", out defineDebug);

            tempConditionalSymbols = ParseSymbol(tempConditionalSymbols, "TRACE", out defineTrace);

            conditionalSymbols = tempConditionalSymbols;

            outputPath = GetConfigProperty("OutputPath");

        }

        private static string ParseSymbol(string symbols, string symbol, out bool flag)
        {
            var result = symbols;
            flag = false;
            if (result.Contains(";"+ symbol +";"))
            {
                flag = true;
                result = result.Replace(";"+ symbol +";", ";");
            }

            if (result.StartsWith(symbol + ";"))
            {
                flag = true;
                result = result.Substring(symbol.Length + 1);
            }

            if (result.EndsWith(";" + symbol))
            {
                flag = true;
                result = result.Substring(0, result.Length - symbol.Length - 1);
            }

            if (result == symbol)
            {
                flag = true;
                result = "";
            }

            return result;
        }

        protected override int ApplyChanges()
        {
            if (ProjectMgr == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            string tempConditionalSymbols;
            if (defineTrace)
                tempConditionalSymbols = defineDebug ? "DEBUG;TRACE" : "TRACE";
            else
                if (defineDebug)
                    tempConditionalSymbols = "DEBUG";
                else
                    tempConditionalSymbols = "";
            if (conditionalSymbols != "")
                if (tempConditionalSymbols != "")
                    tempConditionalSymbols = tempConditionalSymbols + ";" + conditionalSymbols;
                else
                    tempConditionalSymbols = conditionalSymbols;
            SetConfigProperty("DefineConstants", tempConditionalSymbols);
            SetConfigProperty("OutputPath", outputPath);

            IsDirty = false;
            return VSConstants.S_OK;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Project;

using LocDisplayNameAttribute = Hill30.BooProject.Project.Attributes.LocDisplayNameAttribute;
using Hill30.BooProject.Project.Attributes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

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
            this.Name = Resources.GetString(Resources.BuildCaption);
        }
        #endregion

        #region Properties

        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.ConditionalSymbols)]
        [ResourcesDescriptionAttribute(Resources.ConditionalSymbolsDescription)]
        /// <summary>
        /// Gets or sets Assembly Name.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        public string ConditionalSymbols
        {
            get { return this.conditionalSymbols; }
            set { this.conditionalSymbols = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.DefineDebug)]
        [ResourcesDescriptionAttribute(Resources.DefineDebugDescription)]
        /// <summary>
        /// Gets or sets Assembly Name.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        public bool DefineDebug
        {
            get { return this.defineDebug; }
            set { this.defineDebug = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(Resources.GeneralCaption)]
        [LocDisplayName(Resources.DefineTrace)]
        [ResourcesDescriptionAttribute(Resources.DefineTraceDescription)]
        /// <summary>
        /// Gets or sets Assembly Name.
        /// </summary>
        /// <remarks>IsDirty flag was switched to true.</remarks>
        public bool DefineTrace
        {
            get { return this.defineTrace; }
            set { this.defineTrace = value; this.IsDirty = true; }
        }

        [ResourcesCategoryAttribute(Resources.OutputCaption)]
        [LocDisplayName(Resources.OutputPath)]
        [SRDescriptionAttribute(Resources.OutputPathDescription)]
        public string OutputPath
        {
            get { return this.outputPath; }
            set { this.outputPath = value; this.IsDirty = true; }
        }

        #endregion

        protected override void BindProperties()
        {
            if (this.ProjectMgr == null)
            {
                return;
            }

            var conditionalSymbols = "";
            foreach (var c in this.GetConfigProperty("DefineConstants"))
                if (Char.IsLetterOrDigit(c) || c == '_' || c == ';')
                    conditionalSymbols += c;

            conditionalSymbols = parse_symbol(conditionalSymbols, "DEBUG", out defineDebug);

            conditionalSymbols = parse_symbol(conditionalSymbols, "TRACE", out defineTrace);

            this.conditionalSymbols = conditionalSymbols;

            outputPath = this.GetConfigProperty("OutputPath");

        }

        private string parse_symbol(string symbols, string symbol, out bool flag)
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
            if (this.ProjectMgr == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            string conditional_symbols;
            if (defineTrace)
                if (defineDebug)
                    conditional_symbols = "DEBUG;TRACE";
                else
                    conditional_symbols = "TRACE";
            else
                if (defineDebug)
                    conditional_symbols = "DEBUG";
                else
                    conditional_symbols = "";
            if (this.conditionalSymbols != "")
                if (conditional_symbols != "")
                    conditional_symbols = conditional_symbols + ";" + this.conditionalSymbols;
                else
                    conditional_symbols = this.conditionalSymbols;
            this.SetConfigProperty("DefineConstants", conditional_symbols);
            this.SetConfigProperty("OutputPath", outputPath);

            this.IsDirty = false;
            return VSConstants.S_OK;
        }
    }
}

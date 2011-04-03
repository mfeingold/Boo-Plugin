using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Hill30.Boo.MSBuildUtilities
{
    public class Booc : ToolTask
    {
        #region Task properties

        public string[] AdditionalLibPaths { get; set; }

        /// <summary>
        /// Allows to compile unsafe code.
        /// </summary>
        public bool AllowUnsafeBlocks { get; set; }

        private bool? checkForOverflowUnderflow;

        /// <summary>
        /// Gets/sets if integer overlow checking is enabled.
        /// </summary>
        public bool CheckForOverflowUnderflow
        {
            get { return checkForOverflowUnderflow ?? true; }
            set { checkForOverflowUnderflow = value; }
        }

        /// <summary>
        /// Gets/sets the culture.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets/sets the conditional compilation symbols.
        /// </summary>
        public string DefineSymbols { get; set; }

        public bool DelaySign { get; set; }

        /// <summary>
        /// Gets/sets a comma-separated list of warnings that should be disabled.
        /// </summary>
        public string DisabledWarnings { get; set; }

        /// <summary>
        /// Gets/sets if we want to use ducky mode.
        /// </summary>
        public bool Ducky { get; set; }

        public bool EmitDebugInformation { get; set; }

        /// <summary>
        /// If set to true the task will output warnings and errors with full file paths
        /// </summary>
        public bool GenerateFullPaths { get; set; }

        public string KeyContainer { get; set; }

        public string KeyFile { get; set; }

        public bool NoConfig { get; set; }

        public bool NoLogo { get; set; }

        /// <summary>
        /// Gets/sets if we want to link to the standard libraries or not.
        /// </summary>
        public bool NoStandardLib { get; set; }

        /// <summary>
        /// Gets/sets a comma-separated list of optional warnings that should be enabled.
        /// </summary>
        public string OptionalWarnings { get; set; }

        [Output]
        public ITaskItem OutputAssembly { get; set; }
        
        /// <summary>
        /// Gets/sets a specific pipeline to add to the compiler process.
        /// </summary>
        public string Pipeline { get; set; }

        /// <summary>
        ///Specifies target platform.
        /// </summary>
        public string Platform { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        [Required]
        public ITaskItem[] ResponseFiles { get; set; }

        [Required]
        public ITaskItem[] Resources { get; set; }

        [Required]
        public ITaskItem[] Sources { get; set; }

        /// <summary>
        /// Gets/sets the source directory.
        /// </summary>
        public string SourceDirectory { get; set; }

        /// <summary>
        /// Gets/sets whether strict mode is enabled.
        /// </summary>
        public bool Strict { get; set; }

        public string TargetType { get; set; }

        public string TargetFramework { get; set; }

        public bool TreatWarningsAsErrors { get; set; }

        public bool Utf8Output { get; set; }

        /// <summary>
        /// Gets/sets the verbosity level.
        /// </summary>
        public string Verbosity { get; set; }

        /// <summary>
        /// Gets/sets a comma-separated list of warnings that should be treated as errors.
        /// </summary>
        public string WarningsAsErrors { get; set; }

        /// <summary>
        /// Gets/sets if we want to use whitespace agnostic mode.
        /// </summary>
        public bool WhiteSpaceAgnostic { get; set; }

        #endregion

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(Path.GetDirectoryName(GetType().Assembly.CodeBase.Substring(8)), ToolName);
        }

        protected override string ToolName
        {
            get { return "boocNET35.exe"; }
        }

        protected override string GenerateCommandLineCommands()
        {
            var commandLine = new CommandLineBuilder();

            commandLine.AppendSwitchIfNotNull("-t:", TargetType.ToLower());
            commandLine.AppendSwitchIfNotNull("-o:", OutputAssembly);
            commandLine.AppendSwitchIfNotNull("-c:", Culture);
            commandLine.AppendSwitchIfNotNull("-srcdir:", SourceDirectory);
            commandLine.AppendSwitchIfNotNull("-keyfile:", KeyFile);
            commandLine.AppendSwitchIfNotNull("-keycontainer:", KeyContainer);
            commandLine.AppendSwitchIfNotNull("-p:", Pipeline);
            commandLine.AppendSwitchIfNotNull("-define:", DefineSymbols);
            commandLine.AppendSwitchIfNotNull("-lib:", AdditionalLibPaths, ",");
            commandLine.AppendSwitchIfNotNull("-nowarn:", DisabledWarnings);
            commandLine.AppendSwitchIfNotNull("-warn:", OptionalWarnings);
            commandLine.AppendSwitchIfNotNull("-platform:", Platform);
		
		if (TreatWarningsAsErrors)
			commandLine.AppendSwitch("-warnaserror"); // all warnings are errors
		else
			commandLine.AppendSwitchIfNotNull("-warnaserror:", WarningsAsErrors); // only specific warnings are errors
		
		if (NoLogo)
		    commandLine.AppendSwitch("-nologo");

		if (NoConfig)
		    commandLine.AppendSwitch("-noconfig");

		if (NoStandardLib)
		    commandLine.AppendSwitch("-nostdlib");

		if (DelaySign)
		    commandLine.AppendSwitch("-delaysign");

		if (WhiteSpaceAgnostic)
		    commandLine.AppendSwitch("-wsa");

		if (Ducky)
		    commandLine.AppendSwitch("-ducky");

		if (Utf8Output)
		    commandLine.AppendSwitch("-utf8");

		if (Strict)
		    commandLine.AppendSwitch("-strict");

		if (AllowUnsafeBlocks)
		    commandLine.AppendSwitch("-unsafe");

        commandLine.AppendSwitch(EmitDebugInformation ? "-debug+" : "-debug-");

        commandLine.AppendSwitch(CheckForOverflowUnderflow ? "-checked+" : "-checked-");

		foreach (var rsp in ResponseFiles)
			commandLine.AppendSwitchIfNotNull("@", rsp.ItemSpec);				

		foreach (var reference in References)
			commandLine.AppendSwitchIfNotNull("-r:", reference.ItemSpec);
				
		foreach (var resource in Resources)
			commandLine.AppendSwitchIfNotNull("-resource:", resource.ItemSpec);
		
		if (!String.IsNullOrEmpty(Verbosity) )
            switch (Verbosity.ToLower())
            {
                case "normal":
                    break;
                case "warning":
                    commandLine.AppendSwitch("-v");
                    break;
                case "info":
                    commandLine.AppendSwitch("-vv");
                    break;
                case "verbose":
                    commandLine.AppendSwitch("-vvv");
                    break;
                default:
                    Log.LogErrorWithCodeFromResources(
                        "Vbc.EnumParameterHasInvalidValue",
                        "Verbosity",
                        Verbosity,
                        "Normal, Warning, Info, Verbose");
                    break;
            }

            commandLine.AppendFileNamesIfNotNull(Sources, " ");

            return commandLine.ToString();
        }
    }
}

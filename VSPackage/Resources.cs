using System;
using System.Reflection;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace Hill30.BooProject
{
	/// <summary>
	/// This class represent resource storage and management functionality.
	/// </summary>
	internal sealed class Resources
	{
		#region Constants
// ReSharper disable InconsistentNaming
		internal const string Application = "Application";
		internal const string ApplicationCaption = "ApplicationCaption";
        internal const string BuildCaption = "BuildCaption";
        internal const string GeneralCaption = "GeneralCaption";
		internal const string AssemblyName = "AssemblyName";
		internal const string AssemblyNameDescription = "AssemblyNameDescription";
		internal const string OutputType = "OutputType";
		internal const string OutputTypeDescription = "OutputTypeDescription";
		internal const string DefaultNamespace = "DefaultNamespace";
		internal const string DefaultNamespaceDescription = "DefaultNamespaceDescription";
		internal const string StartupObject = "StartupObject";
		internal const string StartupObjectDescription = "StartupObjectDescription";
		internal const string ApplicationIcon = "ApplicationIcon";
		internal const string ApplicationIconDescription = "ApplicationIconDescription";
		internal const string Project = "Project";
		internal const string ProjectFile = "ProjectFile";
		internal const string ProjectFileDescription = "ProjectFileDescription";
		internal const string ProjectFolder = "ProjectFolder";
		internal const string ProjectFolderDescription = "ProjectFolderDescription";
		internal const string OutputFile = "OutputFile";
		internal const string OutputFileDescription = "OutputFileDescription";
		internal const string TargetFrameworkMoniker = "TargetFrameworkMoniker";
		internal const string TargetFrameworkMonikerDescription = "TargetFrameworkMonikerDescription";
		internal const string NestedProjectFileAssemblyFilter = "NestedProjectFileAssemblyFilter";
        internal const string ConditionalSymbols = "ConditionalSymbols";
        internal const string ConditionalSymbolsDescription = "ConditionalSymbolsDescription";
        internal const string DefineDebug = "DefineDebug";
        internal const string DefineDebugDescription = "DefineDebugDescription";
        internal const string DefineTrace = "DefineTrace";
        internal const string DefineTraceDescription = "DefineTraceDescription";
        internal const string AllowUnsafe = "AllowUnsafe";
        internal const string AllowUnsafeDescription = "AllowUnsafeDescription";
        internal const string UseDuckTyping = "UseDuckTyping";
        internal const string UseDuckTypingDescription = "UseDuckTypingDescription";
        internal const string OutputCaption = "Output";
        internal const string OutputPath = "OutputPath";
        internal const string OutputPathDescription = "OutputPathDescription";
	    internal const string ToolWindowTitle = "ToolWindowTitle";
        internal const string CanNotCreateWindow = "CanNotCreateWindow";
// ReSharper restore InconsistentNaming

        //internal const string MsgFailedToLoadTemplateFile = "Failed to add template file to project";
		#endregion Constants

        #region Fields
        private static Resources loader;
        private readonly ResourceManager resourceManager;
        private static Object internalSyncObjectInstance;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Internal explicitly defined default constructor.
        /// </summary>
        internal Resources()
        {
            resourceManager = new System.Resources.ResourceManager("Hill30.BooProject.Resources",
                Assembly.GetExecutingAssembly());
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the internal sync. object.
        /// </summary>
        private static Object InternalSyncObject
        {
            get
            {
                if (internalSyncObjectInstance == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref internalSyncObjectInstance, o, null);
                }
                return internalSyncObjectInstance;
            }
        }
        /// <summary>
        /// Gets information about a specific culture.
        /// </summary>
        private static CultureInfo Culture
        {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }

        /// <summary>
        /// Gets convenient access to culture-specific resources at runtime.
        /// </summary>
        public static ResourceManager ResourceManager
        {
            get
            {
                return GetLoader().resourceManager;
            }
        }
        #endregion Properties

        #region Public Implementation
        /// <summary>
        /// Provide access to resource string value.
        /// </summary>
        /// <param name="name">Received string name.</param>
        /// <param name="args">Arguments for the String.Format method.</param>
        /// <returns>Returns resources string value or null if error occured.</returns>
        public static string GetString(string name, params object[] args)
        {
            Resources resourcesInstance = GetLoader();
            if (resourcesInstance == null)
            {
                return null;
            }
            string res = resourcesInstance.resourceManager.GetString(name, Resources.Culture);

            if (args != null && args.Length > 0)
            {
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }
        /// <summary>
        /// Provide access to resource string value.
        /// </summary>
        /// <param name="name">Received string name.</param>
        /// <returns>Returns resources string value or null if error occured.</returns>
        public static string GetString(string name)
        {
            Resources resourcesInstance = GetLoader();

            if (resourcesInstance == null)
            {
                return null;
            }
            return resourcesInstance.resourceManager.GetString(name, Resources.Culture);
        }

        /// <summary>
        /// Provide access to resource object value.
        /// </summary>
        /// <param name="name">Received object name.</param>
        /// <returns>Returns resources object value or null if error occured.</returns>
        public static object GetObject(string name)
        {
            Resources resourcesInstance = GetLoader();

            if (resourcesInstance == null)
            {
                return null;
            }
            return resourcesInstance.resourceManager.GetObject(name, Resources.Culture);
        }
        #endregion Methods

        #region Private Implementation
        private static Resources GetLoader()
        {
            if (loader == null)
            {
                lock (InternalSyncObject)
                {
                    if (loader == null)
                    {
                        loader = new Resources();
                    }
                }
            }
            return loader;
        }
        #endregion

    }
}
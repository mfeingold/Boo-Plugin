using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Shell.Design;

namespace Hill30.BooProject.DesignTime
{
    public class ResolvingUnit: ICompileUnit, IDisposable
    {
        private readonly InternalCompileUnit internalCompileUnit = new InternalCompileUnit(new CompileUnit());
        private INamespace rootNamespace;
        private IDisposable context;
        private ITypeResolutionService typeResolver;

        public ResolvingUnit(BooProjectNode projectManager)
        {
            context = GlobalServices.TypeService.GetContextTypeResolver(projectManager);
            typeResolver = GlobalServices.TypeService.GetTypeResolutionService(projectManager);
//            typeResolver.ReferenceAssembly(new AssemblyName("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
//            var t = typeResolver.GetAssembly(new AssemblyName("System"));
            //var ts = GlobalServices.TypeService.GetTypeDiscoveryService(projectManager).GetTypes(typeof(object), true);
            var t = typeResolver.GetAssembly(new AssemblyName("System"));
          
            //var list = new System.Collections.ArrayList(ts);
            //var types = GlobalServices.TypeService.GetTypeDiscoveryService(projectManager).GetTypes(typeof(Object), false);
            //typeResolver.ReferenceAssembly(new AssemblyName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
        }

        #region ICompileUnit Members

        public INamespace  RootNamespace
        {
            get { return rootNamespace ?? (rootNamespace = new ResovingNamespace(typeResolver)); }
        }

        #endregion

        #region IEntity Members

        public EntityType  EntityType
        {
            get { return internalCompileUnit.EntityType; }
        }

        public string  FullName
        {
            get { return internalCompileUnit.FullName; }
        }

        public string  Name
        {
            get { return internalCompileUnit.Name; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            context.Dispose();
        }

        #endregion
    }
}

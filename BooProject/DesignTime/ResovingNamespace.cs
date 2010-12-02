using System.Collections.Generic;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.TypeSystem;

namespace Hill30.BooProject.DesignTime
{
    class ResovingNamespace : AbstractNamespace
    {
        private System.ComponentModel.Design.ITypeResolutionService typeResolver;
        private readonly ResovingNamespace parent;
        private readonly string name;

        public ResovingNamespace(System.ComponentModel.Design.ITypeResolutionService typeResolver)
        {
            this.typeResolver = typeResolver;
        }

        private ResovingNamespace(ResovingNamespace parent, string name)
        {
            typeResolver = parent.typeResolver;
            this.parent = parent;
            this.name = name;
        }

        public override string Name { get { return name; } }

        public override INamespace ParentNamespace { get { return parent; } }

        public override IEnumerable<IEntity> GetMembers()
        {
            yield break;
        }

        public override bool Resolve(ICollection<IEntity> resultingSet, string name, EntityType typesToConsider)
        {
            if (base.Resolve(resultingSet, name, typesToConsider))
                return true;
            var type = typeResolver.GetType((FullName == null ? "" : FullName + '.') + name);
            if (type != null)
            {
                resultingSet.Clear();
                resultingSet.Add(CompilerParameters.SharedTypeSystemProvider.Map(type));
                return true;
            }
            if ((typesToConsider & EntityType.Namespace) == EntityType.Namespace && resultingSet.Count == 0)
            {
                resultingSet.Add(new ResovingNamespace(this, name));
                return true;
            }
            return false;
        }
    }
}

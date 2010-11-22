
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Colorizer;
using Boo.Lang.Compiler.TypeSystem;
namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedAttribute : MappedNode
    {
        private string quickInfoTip;
        
        public MappedAttribute(BufferMap bufferMap, Attribute node)
            : base(bufferMap, node, node.Name.Length)
        {
        }

        public override string Format
        {
            get
            {
                return Formats.BooType;
            }
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.TypeReference; }
        }

        protected override void ResolveImpl()
        {
            try
            {
                var type = TypeSystemServices.GetType(Node);
                if (type is Error)
                    return;
                quickInfoTip = "class " + type.FullName;
            }
            catch 
            {
                return;
            }
        }
    }
}

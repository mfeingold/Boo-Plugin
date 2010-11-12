using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Hill30.BooProject.LanguageService.Mapping;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Diagnostics;
using Hill30.BooProject.LanguageService.Mapping.Nodes;

namespace Hill30.BooProject.LanguageService
{
    public class BooTypeAndMemberDropdownBars : TypeAndMemberDropdownBars
    {
        private readonly BooLanguageService service;
        private readonly BooSource source;
        private List<MappedTypeDefinition> types = new List<MappedTypeDefinition>();
        private bool recompiled;

        public BooTypeAndMemberDropdownBars(BooLanguageService service, BooSource source)
            :base(service)
        {
            this.service = service;
            this.source = source;
            Debug.Assert(source != null, "No Source available");
            source.Recompiled += SourceRecompiled;
        }

        void SourceRecompiled(object sender, EventArgs e)
        {
            recompiled = true;
            types = new List<MappedTypeDefinition>(source.GetTypes());
            service.Invoke(new Action(service.SynchronizeDropdowns), new object[]{});
        }

        public override bool OnSynchronizeDropdowns(
            Microsoft.VisualStudio.Package.LanguageService languageService, 
            IVsTextView textView, 
            int line, 
            int col, 
            ArrayList dropDownTypes, 
            ArrayList dropDownMembers, 
            ref int selectedType, 
            ref int selectedMember)
        {
            // TODO: Navigation logic: assign selectedType/selectedMember based on line/col

            if (!recompiled)
                return false;

            recompiled = false;
            dropDownTypes.Clear();
            dropDownMembers.Clear();
            foreach (var node in types)
            {
                var type = node.Node;
                var name = node.Node.FullName;
                if (node.Node.NodeType == NodeType.Module)
                    name = "<module> " + name;
                dropDownTypes.Add(
                    new DropDownMember(
                        name,
                        node.TextSpan,
                        BooDeclarations.GetIconForNode(type),
                        DROPDOWNFONTATTR.FONTATTR_PLAIN));
                foreach (var member in type.Members)
                {
                    string memberName = member.FullName;
                    var span = new TextSpan();
                    var entity = TypeSystemServices.GetEntity(member);
                    LexicalInfo location;
                    switch (entity.EntityType)
                    {
                        case EntityType.Event:
                            location = ((InternalEvent)entity).Event.LexicalInfo;
                            break;
                        case EntityType.Field:
                            location = ((InternalField)entity).Field.LexicalInfo;
                            break;
                        case EntityType.Method:
                            location = ((InternalMethod)entity).Method.LexicalInfo;
                            break;
                        case EntityType.Property:
                            location = ((InternalProperty)entity).Property.LexicalInfo;
                            break;
                        default:
                            continue;
                    }
                    var declarationNode = source.GetNodes(location, n => n.Type == MappedNodeType.VariableDefinition).FirstOrDefault();
                    if (declarationNode != null)
                        span = declarationNode.TextSpan;

                    dropDownMembers.Add(new DropDownMember(
                                            memberName,
                                            span,
                                            BooDeclarations.GetIconForNode(member),
                                            DROPDOWNFONTATTR.FONTATTR_PLAIN));
                }
            }
            return true;
        }
    }
}

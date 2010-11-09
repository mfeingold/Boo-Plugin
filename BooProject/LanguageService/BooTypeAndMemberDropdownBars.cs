using System;
using System.Collections;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Diagnostics;

namespace Hill30.BooProject.LanguageService
{
    public class BooTypeAndMemberDropdownBars : TypeAndMemberDropdownBars
    {
        private BooLanguageService booLanguageService;
        private readonly BooSource source;

        public BooTypeAndMemberDropdownBars(BooLanguageService booLanguageService, BooSource source)
            :base(booLanguageService)
        {
            // TODO: Complete member initialization
            this.booLanguageService = booLanguageService;
            this.source = source;
            Debug.Assert(source != null, "No Source available");
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
            dropDownTypes.Clear();
            dropDownMembers.Clear();
            foreach (var node in source.GetTypes())
            {
                var type = (TypeDefinition) node.Node;
                var name = type.FullName;
                if (node.Node.NodeType == NodeType.Module)
                    name = "<module> " + name;
                dropDownTypes.Add(
                    new DropDownMember(
                        name,
                        new TextSpan(),
                        GetIconForNode(type),
                        DROPDOWNFONTATTR.FONTATTR_PLAIN));
                foreach (var member in type.Members)
                {
                    string memberName = member.FullName;
                    //try
                    //{
                    //    memberName = TypeSystemServices.GetType(member).FullName;
                    //}
                    //catch (Exception)
                    //{
                    //}
                    dropDownMembers.Add(new DropDownMember(
                                            memberName,
                                            new TextSpan(),
                                            GetIconForNode(member),
                                            DROPDOWNFONTATTR.FONTATTR_PLAIN));
                }
            }
            return true;
        }

        public static int GetIconForNode(TypeMember node)
        {
            var result = int.MinValue;
            switch (node.NodeType)
            {
                case NodeType.ClassDefinition:
                    result = CLASS_ICONS;
                    break;
                case NodeType.EnumDefinition:
                    result = ENUM_ICONS;
                    break;
                case NodeType.StructDefinition:
                    result = STRUCT_ICONS;
                    break;
                case NodeType.InterfaceDefinition:
                    result = INTERFACE_ICONS;
                    break;

                case NodeType.EnumMember:
                    result = ENUM_MEMBER_ICONS;
                    break;
                case NodeType.Method:
                case NodeType.Constructor:
                case NodeType.Destructor:
                    result = METHOD_ICONS;
                    break;
                case NodeType.Property:
                    result = PROPERTY_ICONS;
                    break;
                case NodeType.Field:
                    result = FIELD_ICONS;
                    break;
                case NodeType.Event:
                    result = EVENT_ICONS;
                    break;
            }

            if (node.IsPublic)
                result += ICON_PUBLIC;
            if (node.IsPrivate)
                result += ICON_PRIVATE;
            if (node.IsInternal)
                result += ICON_INTERNAL;
            else if (node.IsProtected) // if it is internal protected, only the internal icon is shown
                result += ICON_PROTECTED;
            return result;
        }

// ReSharper disable InconsistentNaming

        const int CLASS_ICONS = 0;
        const int CONST_ICONS = 6;
        const int DELEGATE_ICONS = 12;
        const int ENUM_ICONS = 18;
        const int ENUM_MEMBER_ICONS = 24;
        const int EVENT_ICONS = 30;
        const int FIELD_ICONS = 42;
        const int INTERFACE_ICONS = 48;
        const int METHOD_ICONS = 72;
        const int PROPERTY_ICONS = 102;
        const int STRUCT_ICONS = 108;

        const int ICON_PUBLIC = 0;
        const int ICON_INTERNAL = 1;
        const int ICON_DIAMOND = 2;
        const int ICON_PROTECTED = 3;
        const int ICON_PRIVATE = 4;
        const int ICON_REFERENCE = 5;

// ReSharper restore InconsistentNaming

    }
}

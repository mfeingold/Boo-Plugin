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

using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Ast;

namespace Hill30.Boo.ASTMapper
{
    /// <summary>
    /// Represents a collection of items to be shown in member selection dropdowns
    /// </summary>
    /// <remarks>
    /// The actual list is based on the list of members of the IType object passed in when the collection is created.
    /// The member list is further filtered and formatted.
    /// Filtering: only methods, properties, fields and events are included. Also access modifiers are taken into consideration
    /// Formatting: 3 items displayed include name, description and icon. 
    /// Name: is a short name of the member for overloaded methods only one item is included in the list.
    /// Standard VS icons are selected based on the member type and access modifiers. 
    /// Description consists of Full name of the member including the signature. For overloaded methods this will be a description 
    /// of the first method of the group plus the number of other overloads
    /// </remarks>
    
    // TODO: method description does not include parameter description
    // TODO: double check the list of members to be included
    // TODO: double check filtering of the private members
    // TODO: implement filtering for the protected members
    
    public class BooDeclarations : Declarations
    {
        private class Declaration
        {
            public string DisplayText { get; set; }
            public string Description { get; set; }
            public int ImageIndex { get; set; }
            public int Count { get; set; }
        }

        private readonly SortedList<string, Declaration> list = new SortedList<string, Declaration>();

        public BooDeclarations()
        {
        }

        public BooDeclarations(Node context, IType varType, bool instance)
        {
            if (varType != null)
                foreach (var member in varType.GetMembers())
                {
                    switch (member.EntityType)
                    {
                        case EntityType.Method:
                            FormatMethod(context, (IMethod)member, instance);
                            break;
                        case EntityType.Property:
                            FormatProperty(context, (IProperty)member, instance);
                            break;
                        case EntityType.Event:
                            FormatEvent(context, (IEvent)member, instance);
                            break;
                        case EntityType.Field:
                            FormatField(context, (IField)member, instance);
                            break;
                    }
                }
            //for (var i = 1000; i < 1300; i += 6)
            //    list.Add("a" + i, new Declaration { DisplayText = "a" + i, ImageIndex = i - 1000 });
        }

        private static bool IsContextPrivate(Node context, IType type)
        {
            if (context == null)
                return false;
            if (context.NodeType == NodeType.ClassDefinition && TypeSystemServices.GetType(context) == type)
                    return true;
            if (context.NodeType == NodeType.StructDefinition && TypeSystemServices.GetType(context) == type)
                return true;
            return IsContextPrivate(context.ParentNode, type);
        }

        private static bool IsContextProtected(Node context, IType type)
        {
            if (context == null)
                return false;
            if (context.NodeType == NodeType.ClassDefinition && TypeSystemServices.GetType(context) == type)
                return true;
            if (context.NodeType == NodeType.StructDefinition && TypeSystemServices.GetType(context) == type)
                return true;
            return IsContextProtected(context.ParentNode, type);
        }

        private void FormatField(Node context, IField field, bool instance)
        {
            if (field.IsStatic == instance)
                return;
            if (
                field.IsPublic ||
                field.IsInternal && (field is InternalField) ||
                field.IsProtected && IsContextProtected(context, field.DeclaringType) ||
                field.IsPrivate && IsContextPrivate(context, field.DeclaringType)
                )
            {

                var name = field.Name;
                var description = name + " as " + field.Type;

                list.Add(name,
                    new Declaration
                    {
                        DisplayText = name,
                        Description = description,
                        ImageIndex = GetIconForNode(NodeType.Field, field.IsPublic, field.IsInternal, field.IsProtected, field.IsPrivate)
                    });
            }
        }

        private void FormatEvent(Node context, IEvent @event, bool instance)
        {
            if (@event.IsStatic == instance)
                return;
            if (
                @event.IsPublic
                || IsContextPrivate(context, @event.Type)
                )
            {

                var name = @event.Name;
                var description = name + " as " + @event.Type;

                list.Add(name,
                    new Declaration
                    {
                        DisplayText = name,
                        Description = description,
                        // Hmm... if it is not public - is it protected? or internal? let us make it private
                        ImageIndex = GetIconForNode(NodeType.Event, @event.IsPublic, /* @event.IsInternal */ false, /*@event.IsProtected*/ false, !@event.IsPublic)
                    });
            }
        }

        private void FormatProperty(Node context, IProperty property, bool instance)
        {
            if (property.IsStatic == instance)
                return;
            if (
                property.IsPublic ||
                property.IsInternal && (property is InternalField) ||
                property.IsProtected && IsContextProtected(context, property.DeclaringType) ||
                property.IsPrivate && IsContextPrivate(context, property.DeclaringType)
                )
            {
                var name = property.Name;
                var description = name + " as " + property.Type;

                if (property.IsExtension)
                    description = "(extension) " + description;

                list.Add(name,
                    new Declaration
                        {
                            DisplayText = name,
                            Description = description,
                            ImageIndex = GetIconForNode(NodeType.Property, property.IsPublic, property.IsInternal, property.IsProtected, property.IsPrivate)
                        });
            }
        }

        private void FormatMethod(Node context, IMethod method, bool instance)
        {
            if (method.IsAbstract)
                return;
            if (method.IsSpecialName)
                return;
            if (method.IsStatic == instance)
                return;
            if (
                method.IsPublic ||
                method.IsInternal && (method is InternalField) ||
                method.IsProtected && IsContextProtected(context, method.DeclaringType) ||
                method.IsPrivate && IsContextPrivate(context, method.DeclaringType)
                )
            {

                var name = method.Name;
                Declaration declaration;
                if (list.TryGetValue(name, out declaration))
                {
                    declaration.Count++;
                    return;
                }

                var description = name + " as " + method.ReturnType;
                if (method.IsExtension)
                    description = "(extension) " + description;

                list.Add(name, new Declaration
                    {
                        DisplayText = name,
                        Description = description,
                        ImageIndex = GetIconForNode(NodeType.Method, method.IsPublic, method.IsInternal, method.IsProtected, method.IsPrivate)
                    });
            }
        }

        public override int GetCount()
        {
            return list.Count();
        }

        public override string GetDescription(int index)
        {
            if (list.Values[index].Count == 0)
                return list.Values[index].Description;
            return list.Values[index].Description + " (+" + list.Values[index].Count + " overload(s))";
        }

        public override string GetDisplayText(int index)
        {
            return list.Values[index].DisplayText;
        }

        public override int GetGlyph(int index)
        {
            return list.Values[index].ImageIndex;
        }

        public override string GetName(int index)
        {
            return list.Keys[index];
        }

        public static int GetIconForNode(TypeMember node)
        {
            return GetIconForNode(node.NodeType, node.IsPublic, node.IsInternal, node.IsProtected, node.IsPrivate);
        }

        public static int GetIconForNode(NodeType type, bool isPublic, bool isInternal, bool isProtected, bool isPrivate)
        {
            var result = int.MinValue;
            switch (type)
            {
                case NodeType.Module:
                    result = MODULE_ICONS;
                    break;
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

            if (isPublic)
                result += ICON_PUBLIC;
            if (isPrivate)
                result += ICON_PRIVATE;
            if (isInternal)
                result += ICON_INTERNAL;
            else if (isProtected) // if it is internal protected, only the internal icon is shown
                result += ICON_PROTECTED;
            return result;
        }

        // ReSharper disable InconsistentNaming

        const int CLASS_ICONS = 0;
        //const int CONST_ICONS = 6;
        //const int DELEGATE_ICONS = 12;
        const int ENUM_ICONS = 18;
        const int ENUM_MEMBER_ICONS = 24;
        const int EVENT_ICONS = 30;
        const int FIELD_ICONS = 42;
        const int INTERFACE_ICONS = 48;
        const int METHOD_ICONS = 72;
        const int MODULE_ICONS = 84;
        const int PROPERTY_ICONS = 102;
        const int STRUCT_ICONS = 108;

        const int ICON_PUBLIC = 0;
        const int ICON_INTERNAL = 1;
        //const int ICON_DIAMOND = 2;
        const int ICON_PROTECTED = 3;
        const int ICON_PRIVATE = 4;
        //const int ICON_REFERENCE = 5;

        // ReSharper restore InconsistentNaming

    }
}

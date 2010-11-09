using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService
{
    public class BooDeclarations : Declarations
    {
        public class Declaration
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
            for (var i = 1000; i < 1300; i += 6)
                list.Add("a" + i, new Declaration {DisplayText="a" + i, ImageIndex = i - 1000});
        }

        private bool IsPrivate(Node context, IType type)
        {
            if (context == null)
                return false;
            if (context.NodeType == NodeType.ClassDefinition && TypeSystemServices.GetType(context) == type)
                    return true;
            return IsPrivate(context.ParentNode, type);
        }

        private void FormatField(Node context, IField field, bool instance)
        {
            if (field.IsStatic == instance)
                return;

            var name = field.Name;
            var description = name + " as " + field.Type;
            var icon = FIELD_ICONS;
            if (field.IsPrivate)
            {
                if (!IsPrivate(context, field.Type))
                    return;
                icon += ICON_PRIVATE;
            }
            if (field.IsProtected)
                icon += ICON_PROTECTED;

            list.Add(name, new Declaration { DisplayText = name, Description = description, ImageIndex = icon } );
        }

        private void FormatEvent(Node context, IEvent @event, bool instance)
        {
            if (@event.IsStatic == instance)
                return;

            var name = @event.Name;
            var description = name + " as " + @event.Type;
            var icon = EVENT_ICONS;
            if (!@event.IsPublic)
                icon += ICON_PROTECTED;

            list.Add(name, new Declaration { DisplayText = name, Description = description, ImageIndex = icon });
        }

        private void FormatProperty(Node context, IProperty property, bool instance)
        {
            if (property.IsStatic == instance)
                return;

            var name = property.Name;
            var description = name + " as " + property.Type;
            var icon = PROPERTY_ICONS;
            if (property.IsPrivate)
            {
                if (!IsPrivate(context, property.Type))
                    return;
                icon += ICON_PRIVATE;
            }
            if (property.IsProtected)
                icon += ICON_PROTECTED;
            if (property.IsExtension)
                description = "(extension) " + description;

            list.Add(name, new Declaration { DisplayText = name, Description = description, ImageIndex = icon });
        }

        private void FormatMethod(Node context, IMethod method, bool instance)
        {
            if (method.IsAbstract)
                return;
            if (method.IsSpecialName)
                return;
            if (method.IsStatic == instance)
                return;

            var name = method.Name;
            Declaration declaration;
            if (list.TryGetValue(name, out declaration))
            {
                declaration.Count++;
                return;
            }

            var description = name + " as " + method.ReturnType;
            var icon = METHOD_ICONS;
            if (method.IsPrivate)
            {
                if (!IsPrivate(context, method.ReturnType))
                    return;
                icon += ICON_PRIVATE;
            }
            if (method.IsProtected)
                icon += ICON_PROTECTED;
            if (method.IsExtension)
                description = "(extension) " + description;

            list.Add(name, new Declaration { DisplayText = name, Description = description, ImageIndex = icon });
        }

        const int CLASS_ICONS = 0;
        const int DELEGATE_ICONS = 12;
        const int ENUM_ICONS = 18;
        const int EVENT_ICONS = 30;
        const int FIELD_ICONS = 42;
        const int INTERFACE_ICONS = 48;
        const int METHOD_ICONS = 72;
        const int PROPERTY_ICONS = 102;

        const int ICON_PUBLIC = 0;
        const int ICON_INTERNAL = 1;
        const int ICON_DIAMOND = 2;
        const int ICON_PROTECTED = 3;
        const int ICON_PRIVATE = 4;
        const int ICON_REFERENCE = 5;

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

    }
}

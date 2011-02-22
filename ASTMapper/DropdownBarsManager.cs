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

// TODO: module members
// TODO: double check what's to be included in the type list (delegates?)
// TODO: double check the filtering of the member list
// TODO: double check the nested type order
// TODO: method siganture does not include parameter definitions

using System;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.TextManager.Interop;
using Hill30.Boo.ASTMapper.AST.Nodes;

namespace Hill30.Boo.ASTMapper
{
    public class DropdownBarsManager
    {
        private readonly IFileNode file;

        public DropdownBarsManager(IFileNode file)
        {
            this.file = file;
        }

        private static void ProcessMembers(TypeDefinition type, Action<TypeMember> process)
        {
            foreach (var member in type.Members)
            {
                switch (member.NodeType)
                {
                    case NodeType.Event:
                    case NodeType.Field:
                    case NodeType.Property:
                    case NodeType.Method:
                    case NodeType.Constructor:
                    case NodeType.Destructor:
                        break;
                    default:
                        continue;
                }
                process(member);
            }
        }

        public class DropDownItem
        {
            public DropDownItem(TypeMember member, TextSpan textSpan)
            {
                Name = member.FullName;
                TextSpan = textSpan;
                IconId = BooDeclarations.GetIconForNode(member);
            }

            public DropDownItem(MappedTypeDefinition type)
            {
                Name = type.Node.NodeType == NodeType.Module
                            ? "<Module>"
                            : type.TypeNode.FullName;
                TextSpan = type.TextSpan;
                IconId = BooDeclarations.GetIconForNode(type.TypeNode);
            }

            public string Name { get; private set; }

            public TextSpan TextSpan { get; private set; }

            public int IconId { get; private set; }
        }

        public List<DropDownItem> GetTypesDropdown()
        {
            return file.Types.Select(node => new DropDownItem(node)).ToList();
        }

        public List<DropDownItem> GetMembersDropdown()
        {
            var result = new List<DropDownItem>();
            foreach (var node in file.Types)
                ProcessMembers(node.TypeNode,
                    member => result.Add(new DropDownItem(member, member.GetTextSpan(file))));

            return result;
        }


        public void SelectCurrent(
            int line,
            int col,
            ref int selectedType,
            ref int selectedMember,
            Action<int, DROPDOWNFONTATTR> memberHighlight)
        {

            var sType = -1;
            var mIndex = -1;
            var sm = -1;
            TypeDefinition selectedTypeNode = null;
            foreach (var type in file.Types)
            {

                sType++;
                if (type.TextSpan.Contains(line, col))
                {
                    selectedTypeNode = type.TypeNode;
                    selectedType = sType;
                }
                ProcessMembers(type.TypeNode,
                    member =>
                    {
// ReSharper disable AccessToModifiedClosure
                        mIndex++;
// ReSharper restore AccessToModifiedClosure
                        if (member.GetTextSpan(file).Contains(line, col))
                            sm = mIndex;
                    }
                    );
            }
            selectedMember = sm;

            mIndex = -1;
            foreach (var type in file.Types)
            {
                ProcessMembers(type.TypeNode,
                    member =>
                    {
// ReSharper disable AccessToModifiedClosure
                        mIndex++;
// ReSharper restore AccessToModifiedClosure
                        memberHighlight(mIndex, DROPDOWNFONTATTR.FONTATTR_GRAY);
                        if (member.ParentNode == selectedTypeNode)
                            memberHighlight(mIndex, DROPDOWNFONTATTR.FONTATTR_PLAIN);
                        if (mIndex == sm)
                            memberHighlight(mIndex, DROPDOWNFONTATTR.FONTATTR_BOLD);
                    });
            }
        }
    }
}

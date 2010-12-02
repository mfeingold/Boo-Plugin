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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Diagnostics;
using Hill30.BooProject.Project;

namespace Hill30.BooProject.LanguageService
{
    /// <summary>
    /// Manages the dropdown lists in the code editor navigation bar
    /// </summary>
    /// <remarks>
    /// The lists are rebuilt in response to the recompiled event fired by the Source object.
    /// The list of types is a flat list of all type/struct/enum definitions present in the source file
    /// The member list includes members of all types from the type list as well as members of the module
    /// Selection of a type/type member from the list positions caret on the appropriate point in the source
    /// The members of the currently selected type are shown in regular font. Members of all other types are shown in grey
    /// Currently selected member is shown in bold.
    /// As caret is moved through the source code, the highlighting of the member list is modified accordingly
    /// The selected type is the last type on the type list containing the current caret position. This implies
    /// that the nested types are listed with outer type first
    /// </remarks>
    
    // TODO: module members
    // TODO: double check what's to be included in the type list (delegates?)
    // TODO: double check the filtering of the member list
    // TODO: double check the nested type order
    // TODO: method siganture does not include parameter definitions

    public class BooTypeAndMemberDropdownBars : TypeAndMemberDropdownBars
    {
        private BooLanguageService service;
        private bool recompiled;
        private IFileNode fileNode;

        public BooTypeAndMemberDropdownBars(BooLanguageService service, IFileNode fileNode)
            : base(service)
        {
            this.service = service;
            this.fileNode = fileNode;
            fileNode.Recompiled += SourceRecompiled;
        }

        void SourceRecompiled(object sender, EventArgs e)
        {
            recompiled = true;
            service.Invoke(new Action(service.SynchronizeDropdowns), new object[]{});
        }

        private void ProcessMembers(TypeDefinition type, Action<TypeMember> process)
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
            if (recompiled)
            {
                recompiled = false;
                dropDownTypes.Clear();
                dropDownMembers.Clear();
                foreach (var node in fileNode.Types)
                {
                    var type = node.TypeNode;
                    var name = node.TypeNode.FullName;
                    ProcessMembers(type,
                        member => dropDownMembers.Add(new DropDownMember(
                                                          member.FullName,
                                                          member.GetTextSpan(fileNode),
                                                          BooDeclarations.GetIconForNode(member),
                                                          DROPDOWNFONTATTR.FONTATTR_GRAY
                                                          )));

                    if (node.Node.NodeType == NodeType.Module)
                        continue;
                    dropDownTypes.Add(
                        new DropDownMember(
                            name,
                            node.TextSpan,
                            BooDeclarations.GetIconForNode(type),
                            DROPDOWNFONTATTR.FONTATTR_PLAIN));

                }
            }

            var sType = -1;
            var mIndex = -1;
            var sm = -1;
            TypeDefinition selectedTypeNode = null;
            foreach (var type in fileNode.Types)
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
                            mIndex++;
                            if (member.GetTextSpan(fileNode).Contains(line, col))
                                sm = mIndex;
                        }
                    );
            }
            selectedMember = sm;

            if (dropDownMembers.Count == 0)
                return true;

            mIndex = -1;
            foreach (var type in fileNode.Types)
            {
                ProcessMembers(type.TypeNode,
                    member =>
                        {
                            mIndex++;
                            ((DropDownMember)dropDownMembers[mIndex]).FontAttr = DROPDOWNFONTATTR.FONTATTR_GRAY;
                            if (member.ParentNode == selectedTypeNode)
                                ((DropDownMember)dropDownMembers[mIndex]).FontAttr = DROPDOWNFONTATTR.FONTATTR_PLAIN;
                            if (mIndex == sm)
                                ((DropDownMember)dropDownMembers[mIndex]).FontAttr = DROPDOWNFONTATTR.FONTATTR_BOLD;
                        }
                    );
            }

            return true;
        }
    }
}

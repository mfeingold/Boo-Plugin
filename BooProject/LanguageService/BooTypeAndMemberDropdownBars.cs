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
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Hill30.Boo.ASTMapper;

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
    
    public class BooTypeAndMemberDropdownBars : TypeAndMemberDropdownBars
    {
        private readonly BooLanguageService service;
        private bool isDirty;
        private readonly DropdownBarsManager barManager;

        public BooTypeAndMemberDropdownBars(BooLanguageService service, IFileNode fileNode)
            : base(service)
        {
            this.service = service;
            isDirty = true;
            fileNode.Recompiled += SourceRecompiled;
            barManager = new DropdownBarsManager(fileNode);
        }

        void SourceRecompiled(object sender, EventArgs e)
        {
            isDirty = true;
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
            if (isDirty)
            {
                isDirty = false;
                dropDownTypes.Clear();
                dropDownMembers.Clear();
                foreach (var member in barManager.GetMembersDropdown())
                    dropDownMembers.Add(new DropDownMember(
                                                          member.Name,
                                                          member.TextSpan,
                                                          member.IconId,
                                                          DROPDOWNFONTATTR.FONTATTR_GRAY
                                                          ));

                foreach (var type in barManager.GetTypesDropdown())
                    dropDownTypes.Add(
                        new DropDownMember(
                            type.Name,
                            type.TextSpan,
                            type.IconId,
                            DROPDOWNFONTATTR.FONTATTR_PLAIN));

            }

            barManager.SelectCurrent(
                line,
                col,
                ref selectedType,
                ref selectedMember,
                (mIndex, attr) =>
                    ((DropDownMember)dropDownMembers[mIndex]).FontAttr = attr);
           
            return true;
        }
    }
}

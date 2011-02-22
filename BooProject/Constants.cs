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

namespace Hill30.BooProject
{
    static class Constants
    {
        public const string LanguageName = "Visual Boo";

        public const string GuidBooProjectPkgString = "3ed37b82-3194-4ca8-96b4-d4e3feb8a35d";
        public const string GuidBooProjectCmdSetString = "ec06ff5c-8707-4572-9d77-348e88d3eebf";

        public static readonly Guid GuidBooProjectCmdSet = new Guid(GuidBooProjectCmdSetString);
    };

    public static class PkgCmdIDList
    {
        public const uint cmdidBooISh = 0x100;
    };
}
// Guids.cs
// MUST match guids.h
using System;

namespace Hill30.BooProject
{
    static class Constants
    {
        public const string LanguageName = "Visual Boo";

        public const string guidBooProjectPkgString = "3ed37b82-3194-4ca8-96b4-d4e3feb8a35d";
        public const string guidBooProjectCmdSetString = "ec06ff5c-8707-4572-9d77-348e88d3eebf";

        public static readonly Guid guidBooProjectCmdSet = new Guid(guidBooProjectCmdSetString);
    };
}
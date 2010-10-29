using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;

namespace Hill30.BooProject.LanguageService
{
    public class BooDeclarations : Declarations
    {
        public BooDeclarations(IEnumerable<Tuple<string, string, int, string>> list = null )
        {
            if (list == null)
                list = new List<Tuple<string, string, int, string>>();
            this.list = new List<Tuple<string, string, int, string>>(list);
        }

        private readonly List<Tuple<string, string, int, string>> list;

        public override int GetCount()
        {
            return list.Count();
        }

        public override string GetDescription(int index)
        {
            return list[index].Item1;
        }

        public override string GetDisplayText(int index)
        {
            return list[index].Item2;
        }

        public override int GetGlyph(int index)
        {
            return list[index].Item3;
        }

        public override string GetName(int index)
        {
            return list[index].Item4;
        }

        public List<Tuple<string, string, int, string>> List { get { return list; }}
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Svg
{
    public class FontFamilyLookupArgs : EventArgs
    {
        public FontFamily FontFamily { get; set; }
        public string Name { get; private set; }

        public FontFamilyLookupArgs(string name)
        {
            Name = name;
        }
    }
}

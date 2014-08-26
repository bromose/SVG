using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Svg
{
    [TypeConverter(typeof(EnumBaseConverter<SvgCoordinateUnits>))]
    public enum SvgGradientSpreadMethod
    {
        Pad,
        Reflect,
        Repeat
    }
}
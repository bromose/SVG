using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(SvgPatternUnitsConverter))]
    public enum SvgPatternUnits
    {
        /// <summary>
        ///  the user coordinate system for attributes ‘x’, ‘y’, ‘width’ and ‘height’ is established using the bounding box of the element to which the 
        ///  pattern is applied (see Object bounding box units) and then applying the transform specified by attribute ‘patternTransform’.
        /// </summary>
        ObjectBoundingBox,
        /// <summary>
        ///  ‘x’, ‘y’, ‘width’ and ‘height’ represent values in the coordinate system that results from taking the current user coordinate system in 
        ///  place at the time when the ‘pattern’ element is referenced (i.e., the user coordinate system for the element referencing the ‘pattern’ 
        ///  element via a ‘fill’ or ‘stroke’ property) and then applying the transform specified by attribute ‘patternTransform’.
        /// </summary>
        UserSpaceOnUse
    }
}

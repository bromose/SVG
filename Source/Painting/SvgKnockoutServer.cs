using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg
{
    /// <summary>
    /// A paint server to handle colors that paint a contextual background
    /// </summary>
    public sealed class SvgKnockoutServer : SvgPaintServer
    {
        public SvgKnockoutServer(string name)
        {
            Name = name ?? "";
        }

        public string Name { get; private set; }

        public override Brush GetBrush(SvgVisualElement styleOwner, SvgRenderer renderer, float opacity)
        {
            return (Brush)(renderer.KnockoutBrush ?? Brushes.White).Clone();
        }

        public override string ToString()
        {
            return Name;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgColourServer>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgKnockoutServer;
            newObj.Name = this.Name;
            return newObj;

        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

}

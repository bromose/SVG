using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Svg
{
    public sealed class SvgColourServer : SvgPaintServer
    {
        public SvgColourServer() : this(Color.Black, null)
        {
        }

        public SvgColourServer(Color colour, string name)
        {
            this._colour = colour;
            Name = name;
        }

        private Color _colour;

        public Color Colour
        {
            get { return this._colour; }
            //set { this._colour = value; }
        }
        public string Name { get; private set; }

        public override Brush GetBrush(SvgVisualElement styleOwner, SvgRenderer renderer, float opacity)
        {
            //is none?
            if (this == SvgPaintServer.None) 
                return new SolidBrush(Color.Transparent);


            int alpha = (int)((opacity * (this.Colour.A/255.0f) ) * 255);
            Color colour = Color.FromArgb(alpha, this.Colour);

            return new SolidBrush(colour);
        }

        public override string ToString()
        {
            //if(this == SvgPaintServer.None)
            //    return "none";
            //else if(this == SvgColourServer.NotSet)
            //    return "";
            if (Name != null)
                return Name;
        	
            Color c = this.Colour;

            // Return the name if it exists
            if (c.IsKnownColor)
            {
                return c.Name;
            }

            // Return the hex value
            return String.Format("#{0}", c.ToArgb().ToString("x").Substring(2));
        }

		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgColourServer>();
		}

		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgColourServer;
			newObj._colour = this.Colour;
            newObj.Name = this.Name;
			return newObj;

		}

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var objColor = obj as SvgColourServer;
            if (objColor == null)
                return false;

            return ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return _colour.GetHashCode();
        }
    }
}

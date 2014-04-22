using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Svg
{
    public class SvgRenderer : IDisposable
    {
        private Graphics _innerGraphics;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgRenderer"/> class.
        /// </summary>
        protected SvgRenderer()
        {
        }
        /// <summary>
        /// Creates a new <see cref="SvgRenderer"/> from the specified <see cref="Image"/>.
        /// </summary>
        /// <param name="image"><see cref="Image"/> from which to create the new <see cref="SvgRenderer"/>.</param>
        public static SvgRenderer FromImage(Image image)
        {
            SvgRenderer renderer = new SvgRenderer();
            renderer._innerGraphics = Graphics.FromImage(image);
            return renderer;
        }
        /// <summary>
        /// Creates a new <see cref="SvgRenderer"/> from the specified <see cref="Graphics"/>.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> to create the renderer from.</param>
        public static SvgRenderer FromGraphics(Graphics graphics)
        {
            SvgRenderer renderer = new SvgRenderer();
            renderer._innerGraphics = graphics;
            return renderer;
        }

        public virtual void DrawImageUnscaled(Image image, Point location)
        {
            this._innerGraphics.DrawImageUnscaled(image, location);
        }

        public virtual void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
        {
            _innerGraphics.DrawImage(image, destRect, srcRect, graphicsUnit);
        }

        public virtual void SetClip(Region region)
        {
            this._innerGraphics.SetClip(region, CombineMode.Complement);
        }

        public virtual Region Clip
        {
            get { return this._innerGraphics.Clip; }
            set { this._innerGraphics.Clip = value; }
        }

        public virtual void FillPath(Brush brush, GraphicsPath path)
        {
            this._innerGraphics.FillPath(brush, path);
        }

        public virtual void DrawPath(Pen pen, GraphicsPath path)
        {
            this._innerGraphics.DrawPath(pen, path);
        }

        public virtual void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            this._innerGraphics.TranslateTransform(dx, dy, order);
        }

        public void TranslateTransform(float dx, float dy)
        {
            this.TranslateTransform(dx, dy, MatrixOrder.Append);
        }

        public virtual void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            this._innerGraphics.ScaleTransform(sx, sy, order);
        }

        public void ScaleTransform(float sx, float sy)
        {
            this.ScaleTransform(sx, sy, MatrixOrder.Append);
        }

        public virtual SmoothingMode SmoothingMode
        {
            get { return this._innerGraphics.SmoothingMode; }
            set { this._innerGraphics.SmoothingMode = value; }
        }

        public virtual PixelOffsetMode PixelOffsetMode
        {
            get { return this._innerGraphics.PixelOffsetMode; }
            set { this._innerGraphics.PixelOffsetMode = value; }
        }

        public virtual CompositingQuality CompositingQuality
        {
            get { return this._innerGraphics.CompositingQuality; }
            set { this._innerGraphics.CompositingQuality = value; }
        }

        public virtual TextRenderingHint TextRenderingHint
        {
            get { return this._innerGraphics.TextRenderingHint; }
            set { this._innerGraphics.TextRenderingHint = value; }
        }

        public virtual int TextContrast
        {
            get { return this._innerGraphics.TextContrast; }
            set { this._innerGraphics.TextContrast = value; }
        }

        public virtual Matrix Transform
        {
            get { return this._innerGraphics.Transform; }
            set { this._innerGraphics.Transform = value; }
        }

        public virtual void Save()
        {
            this._innerGraphics.Save();
        }

        public virtual SizeF MeasureString(string text, Font font)
        {
            var ff = font.FontFamily;
            float lineSpace = ff.GetLineSpacing(font.Style);
            float ascent = ff.GetCellAscent(font.Style);
            float baseline = font.GetHeight(this._innerGraphics) * ascent / lineSpace;

            StringFormat format = StringFormat.GenericTypographic;
            format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });
            Region[] r = this._innerGraphics.MeasureCharacterRanges(text, font, new Rectangle(0, 0, 1000, 1000), format);
            RectangleF rect = r[0].GetBounds(this._innerGraphics);

            return new SizeF(rect.Width, baseline);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_innerGraphics != null)
                this._innerGraphics.Dispose();
        }

        ~SvgRenderer()
        {
            Dispose(false);
        }
    }
}
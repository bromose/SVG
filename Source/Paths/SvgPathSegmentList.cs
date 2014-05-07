using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace Svg.Pathing
{
    [TypeConverter(typeof(SvgPathBuilder))]
    public sealed class SvgPathSegmentList : IList<SvgPathSegment>
    {
        internal SvgPath _owner;
        private List<SvgPathSegment> _segments;

        public SvgPathSegmentList()
        {
            this._segments = new List<SvgPathSegment>();
        }

        public SvgPathSegment Last
        {
            get { return this._segments[this._segments.Count - 1]; }
        }

        public int IndexOf(SvgPathSegment item)
        {
            return this._segments.IndexOf(item);
        }

        public void Insert(int index, SvgPathSegment item)
        {
            this._segments.Insert(index, item);
            if (this._owner != null)
            {
                this._owner.OnPathUpdated();
            }
        }

        public void RemoveAt(int index)
        {
            this._segments.RemoveAt(index);
            if (this._owner != null)
            {
                this._owner.OnPathUpdated();
            }
        }

        public SvgPathSegment this[int index]
        {
            get { return this._segments[index]; }
            set { this._segments[index] = value; this._owner.OnPathUpdated(); }
        }

        public void Add(SvgPathSegment item)
        {
            this._segments.Add(item);
            if (this._owner != null)
            {
                this._owner.OnPathUpdated();
            }
        }

        public void Add(GraphicsPath path)
        {
            var pathData = path.PathData;
            for (int i = 0; i < pathData.Types.Length; i++)
            {
                PointF last = i > 0 ? pathData.Points[i - 1] : pathData.Points[0];
                PointF pt = pathData.Points[i];
                byte bType = pathData.Types[i];
                if (bType == 0)
                {
                    //buffer.Append("M").Append(pt.X.ToString("#.000")).Append(",").Append(pt.Y.ToString("#.000")).Append(" ");
                    _segments.Add(new SvgMoveToSegment(pt));
                }
                if (bType.ContainsMask((byte)PathPointType.Bezier))
                {
                    PointF pt1 = pathData.Points[++i];
                    if (pathData.Types.Length > i + 1 && pathData.Types[i + 1].ContainsMask((byte)PathPointType.Bezier3))
                    {
                        PointF pt2 = pathData.Points[++i];
                        //buffer.Append("C").Append(pt.X.ToString("#.000")).Append(",").Append(pt.Y.ToString("#.000")).Append(" ").Append(pt1.X.ToString("#.000")).Append(",").Append(pt1.Y.ToString("#.000")).Append(" ").Append(pt2.X.ToString("#.000")).Append(",").Append(pt2.Y.ToString("#.000")).Append(" ");
                        _segments.Add(new SvgCubicCurveSegment(last, pt, pt1, pt2));
                    }
                    else
                    {
                        //buffer.Append("Q").Append(pt.X.ToString("#.000")).Append(",").Append(pt.Y.ToString("#.000")).Append(" ").Append(pt1.X.ToString("#.000")).Append(",").Append(pt1.Y.ToString("#.000")).Append(" ");
                        _segments.Add(new SvgQuadraticCurveSegment(last, pt, pt1));
                    }
                }
                else if (bType.ContainsMask((byte)PathPointType.Line))
                {
                    //buffer.Append("L").Append(pt.X.ToString("#.000")).Append(",").Append(pt.Y.ToString("#.000")).Append(" ");
                    _segments.Add(new SvgLineSegment(last, pt));
                }
                if (bType.ContainsMask((byte)PathPointType.CloseSubpath))
                {
                    //buffer.Append("z");
                    _segments.Add(new SvgClosePathSegment());
                }
            }
        }

        public void Clear()
        {
            this._segments.Clear();
        }

        public bool Contains(SvgPathSegment item)
        {
            return this._segments.Contains(item);
        }

        public void CopyTo(SvgPathSegment[] array, int arrayIndex)
        {
            this._segments.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this._segments.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(SvgPathSegment item)
        {
            bool removed = this._segments.Remove(item);

            if (removed)
            {
                if (this._owner != null)
                {
                    this._owner.OnPathUpdated();
                }
            }

            return removed;
        }

        public IEnumerator<SvgPathSegment> GetEnumerator()
        {
            return this._segments.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._segments.GetEnumerator();
        }

        #region Edits
        public void Offset(float dx, float dy)
        {
            foreach (var seg in _segments)
            {
                seg.EditOffset(dx, dy);
            }
        }
        public void Scale(float scale)
        {
            foreach (var seg in _segments)
            {
                seg.EditScale(scale);
            }
        }
        #endregion
    }
}
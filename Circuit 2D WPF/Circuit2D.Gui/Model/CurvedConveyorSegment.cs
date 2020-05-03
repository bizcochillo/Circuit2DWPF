using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Circuit2D.Gui.Model
{
    class CurvedConveyorSegment : ConveyorSegmentBase
    {
        public double A0 { get; set; }
        public double B0 { get; set; }
        public double A1 { get; set; }
        public double B1 { get; set; }

        public Point P0Top => GetTopPoint(P0, Alpha0 - Math.PI / 2.0);
        public Point P0Bottom => GetBottomPoint(P0, Alpha0 - Math.PI / 2.0);

        public Point P1Top => GetTopPoint(P1, Alpha1 - Math.PI / 2.0);
        public Point P1Bottom => GetBottomPoint(P1, Alpha1 - Math.PI / 2.0);

        public Point Solution => new Point(
            (B1 - B0) / (A0 - A1),
            A0 * ((B1 - B0) / (A0 - A1)) + B0);

        public SweepDirection Direction
        {
            get
            {
                var det = (P0.X - Solution.X) * (P1.Y - Solution.Y) - (P1.X - Solution.X) * (P0.Y - Solution.Y);
                if (det < 0) return SweepDirection.Counterclockwise;
                return SweepDirection.Clockwise;
            }
        }

        public int Radius => Distance(Solution, P0);
        public double Alpha { get; set; }
        public double Beta => Alpha - Math.PI / 2.0;

        public double Alpha0 { get; set; }
        public double Alpha1 { get; set; }

        public bool IsValid()
        {
            return IsNumber(A0) && IsNumber(A1) && IsNumber(B0) && IsNumber(B1);
        }

        private bool IsNumber(double number)
        {
            return !double.IsNaN(number) && !double.IsInfinity(number);
        }

        private int Distance(Point p1, Point p2)
        {
            return (int)Math.Round(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)));
        }

        public override PathFigure GetPath()
        {
            return GetRoundedPathFigure(P0, P1, Radius, Direction);
        }

        private PathFigure GetRoundedPathFigure(Point p1, Point p2, int radius, SweepDirection sweepDirection)
        {
            var arc = new ArcSegment
            {
                Point = p2,
                Size = new Size(radius, radius),
                SweepDirection = sweepDirection
            };

            var figure = new PathFigure { StartPoint = p1 };
            figure.Segments.Add(arc);

            return figure;
        }
    }
}

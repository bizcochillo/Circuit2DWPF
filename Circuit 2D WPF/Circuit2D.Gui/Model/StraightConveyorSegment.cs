using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Circuit2D.Gui.Model
{
    class StraightConveyorSegment : ConveyorSegmentBase
    {
        public Point SourceP0 { get; set; }
        public Point SourceP1 { get; set; }

        public Point P0Top => GetTopPoint(P0, Beta);
        public Point P0Bottom => GetBottomPoint(P0, Beta);

        public Point P1Top => GetTopPoint(P1, Beta);
        public Point P1Bottom => GetBottomPoint(P1, Beta);

        public double Alpha => Math.Atan2(SourceP1.Y - SourceP0.Y, SourceP1.X - SourceP0.X);
        public double Beta => Alpha - Math.PI / 2;

        public double A => (SourceP1.X - SourceP0.X) / (SourceP0.Y - SourceP1.Y);

        public override PathFigure GetPath()
        {
            return new PathFigure(P0, new List<PathSegment> { new LineSegment(P1, false) }, false);
        }
    }
}

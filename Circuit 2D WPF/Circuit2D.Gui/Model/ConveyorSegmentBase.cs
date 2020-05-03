using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Circuit2D.Gui.Model
{
    class ConveyorSegmentBase
    {
        private const int CONVEYOR_SEGMENT_HEIGHT = 15;

        public Point P0 { get; set; }
        public Point P1 { get; set; }

        public Point GetTopPoint(Point p, double angle)
        {
            return new Point
            (
                p.X + CONVEYOR_SEGMENT_HEIGHT * Math.Cos(angle),
                p.Y + CONVEYOR_SEGMENT_HEIGHT * Math.Sin(angle)
            );
        }

        public Point GetBottomPoint(Point p, double angle)
        {
            return new Point
            (
                p.X - CONVEYOR_SEGMENT_HEIGHT * Math.Cos(angle),
                p.Y - CONVEYOR_SEGMENT_HEIGHT * Math.Sin(angle)
            );
        }

        public virtual PathFigure GetPath()
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Circuit2D.Gui.Model.Interfaces
{
    public class BezierLineBasicInfoDto
    {
        public Point Start;
        public Point End;
        public List<Point> ControlPoints;

        public void AddControlPoint(Point p)
        {
            if (ControlPoints == null)
                ControlPoints = new List<Point>();
            ControlPoints.Add(p);
        }
    }
}

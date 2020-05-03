using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Circuit2D.Gui.Model.Interfaces
{
    public interface IDrawingPath
    {
        void AddPoint(Point p);
        void Animate();
    }
}

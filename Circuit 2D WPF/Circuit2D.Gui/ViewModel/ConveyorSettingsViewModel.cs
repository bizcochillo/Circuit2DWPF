using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Circuit2D.Gui.Model;
using Circuit2D.Gui.Model.Interfaces;
using Prism.Commands;

namespace Circuit2D.Gui.ViewModel
{
    class ConveyorSettingsViewModel
    {
        public ConveyorSettingsViewModel() { }

        public ConveyorSettingsViewModel(ICanvas canvas)
        {
            canvas.Clear();

            DrawingPath = new ConveyorDrawingPath(canvas);
            DrawingPath.IsMarkingEnabled = true;
            DrawingPath.IsRoundedSegmentsEnabled = true;
            canvas.SetDrawingPath(DrawingPath);
            SendPackageCommand = new DelegateCommand(Go);
        }

        public ConveyorDrawingPath DrawingPath { get; set; }
        public DelegateCommand SendPackageCommand { get; set; }

        private void Go()
        {
            DrawingPath.Animate();
        }
    }
}

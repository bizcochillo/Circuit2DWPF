using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Circuit2D.Gui.Model;
using Circuit2D.Gui.Model.Interfaces;
using Circuit2D.Gui.ViewModel;
using Prism.Commands;

namespace Circuit2D.Gui.ViewModel
{
    public class VehiclesSettingsViewModel: ViewModelBase
    {
        public BezierDrawingPath DrawingPath { get; }
        private IForklift _forklift;
        
        public VehiclesSettingsViewModel()
        {
            GoCommand = new DelegateCommand(Go);
        }

        public VehiclesSettingsViewModel(ICanvas canvas) : this()
        {
            canvas.Clear();
            _forklift = canvas.FindElementByType<IForklift>();
            DrawingPath = new BezierDrawingPath(canvas)
            {
                IsMarkingEnabled = true,
                IsBezierPathEnabled = true
            };

            SelectedVehicle = VehicleType.Forklift;
            canvas.SetDrawingPath(DrawingPath);
        }

        public DelegateCommand GoCommand { get; set; }

        private void Go()
        {
            DrawingPath.Animate();
        }

        private ObservableCollection<VehicleType> _vehicles = new ObservableCollection<VehicleType>
        {
            VehicleType.Forklift,
            VehicleType.Truck
        };

        public ObservableCollection<VehicleType> Vehicles
        {
            get => _vehicles;
            set
            {
                _vehicles = value;
                NotifyPropertyChanged();
            }
        }

        private VehicleType _selectedVehicle;
        public VehicleType SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                _selectedVehicle = value;
                DrawingPath.SetTruckType(_selectedVehicle);
                NotifyPropertyChanged();
            }
        }

    }
}



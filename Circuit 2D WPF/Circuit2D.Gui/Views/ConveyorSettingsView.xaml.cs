using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Circuit2D.Gui.Model.Interfaces;
using Circuit2D.Gui.ViewModel;
using Prism.Ioc;

namespace Circuit2D.Gui.Views
{
    /// <summary>
    /// Interaction logic for ConveyorSettingsView.xaml
    /// </summary>
    public partial class ConveyorSettingsView : UserControl, ISettingsView
    {
        public ConveyorSettingsView(IContainerExtension container)
        {
            InitializeComponent();
            DataContext = new ConveyorSettingsViewModel(container.Resolve<ICanvas>());
        }
    }
}

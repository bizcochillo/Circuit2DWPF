using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Circuit2D.Gui.Model.Interfaces;
using CommonServiceLocator;
using Prism.Ioc;
using Prism.Regions;

namespace Circuit2D.Gui.Views
{
    /// <summary>
    /// Interaction logic for SideMenuView.xaml
    /// </summary>
    public partial class SideMenuView : UserControl
    {
        private IRegionManager _regionManager;
        private IContainerExtension _container;
        private ISettingsView _configurationViewActive;

        public SideMenuView()
        {
            InitializeComponent();
            _regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            _container = ServiceLocator.Current.GetInstance<IContainerExtension>();
        }

        private void BtnVehicles_Click(object sender, RoutedEventArgs e)
        {
            ActivateView(_container.Resolve<VehiclesSettingsView>());
        }

        private void BtnPackage_Click(object sender, RoutedEventArgs e)
        {
            ActivateView(_container.Resolve<ConveyorSettingsView>());
        }

        private void ActivateView(ISettingsView newView)
        {
            // Get the region where the configuration panel will be load in. 
            IRegion detailsRegion = _regionManager.Regions["DetailsRegion"];

            // Disable previous panel. 
            if (_configurationViewActive != null) detailsRegion.Deactivate(_configurationViewActive);

            // Set active panel. 
            _configurationViewActive = newView;

            // Add new view to the region. 
            detailsRegion.Add(_configurationViewActive);
        }
    }
}

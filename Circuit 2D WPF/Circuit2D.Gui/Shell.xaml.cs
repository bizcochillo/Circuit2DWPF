using System.Windows;
using Circuit2D.Gui.Model;
using Circuit2D.Gui.Model.Interfaces;
using Prism.Ioc;

namespace Circuit2D.Gui
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        public Shell(IContainerExtension container)
        {
            InitializeComponent();
            container.RegisterInstance<ICanvas>(new WpfCanvas(DesignRegionCanvas));
        }
    }
}

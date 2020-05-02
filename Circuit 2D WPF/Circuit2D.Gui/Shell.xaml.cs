using System.Windows;
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
        }
    }
}

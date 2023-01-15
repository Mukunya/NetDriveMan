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

namespace NetworkDriveManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //Hide();
        }

        private void TrayIconClick(object sender, MouseButtonEventArgs e)
        {
            Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel= true;
            base.OnClosing(e);
        }

        private void MenuClick(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void TitleMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}

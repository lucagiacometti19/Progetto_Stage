using DevExpress.Map;
using DevExpress.Map.Native;
using DevExpress.Xpf.Map;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Progetto
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel main;
        public MainWindow()
        {
            InitializeComponent();
            main = new MainViewModel();
            DataContext = main;
        }

        private void MapControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MapControl controlMap = sender as MapControl;
            Point position = e.GetPosition(controlMap);
            GeoPoint point = vector.ScreenToGeoPoint(position);
            main.CreateMapPushpin(point);
        }
    }
}

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

namespace QAMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool bool_Playing = false;
        public MainWindow()
        {
            InitializeComponent();

            Window_Main.Title = "QAMP " + typeof(MainWindow).Assembly.GetName().Version.ToString();
        }

        private void Handle_Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (!bool_Playing)
            {
                bool_Playing = true;
                Image_Play.Source = new BitmapImage(new Uri("Image\\Pause.png", UriKind.RelativeOrAbsolute));

                Window_Main.Title = "PLAYING";
            }
            else
            {
                bool_Playing = false;
                Image_Play.Source = new BitmapImage(new Uri("Image\\Play.png", UriKind.RelativeOrAbsolute));

                Window_Main.Title = "PAUSED";
            }
        }

        private void Handle_Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            bool_Playing = false;
            Image_Play.Source = new BitmapImage(new Uri("Image\\Play.png", UriKind.RelativeOrAbsolute));

            Window_Main.Title = "STOPPED";
        }
    }
}

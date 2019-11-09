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

using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using Un4seen.Bass;

namespace QAMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string string_Media = "Audio\\Leave A Message.wav";
        private int int_Stream = 0;
        private SYNCPROC SYNCPROC_Media = null;

        private long long_Duration_Bytes = 0;
        private double double_Duration_Seconds = 0;

        private long long_Position_Bytes = 0;
        private double double_Position_Seconds = 0;

        private bool bool_Playing = false;
        private bool bool_Sliding = false;

        Line Line_Horizontal_Axis = new Line();

        Line Line_Vertical_Axis = new Line();
        double double_Vertical_Axis_Margin_Left = 0;

        public MainWindow()
        {
            InitializeComponent();

            Window_Main.Title = "QAMP " + typeof(MainWindow).Assembly.GetName().Version.ToString();

            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(250);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Handle_Window_Main_ContentRendered(object Sender, EventArgs E)
        {

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {

                int_Stream = Bass.BASS_StreamCreateFile(string_Media, 0L, 0L, BASSFlag.BASS_DEFAULT);
                if (int_Stream != 0)
                {
                    SYNCPROC_Media = new SYNCPROC(Handle_Media_End);

                    if ((Bass.BASS_ChannelSetSync(int_Stream, BASSSync.BASS_SYNC_END, 0, SYNCPROC_Media, IntPtr.Zero)) == 0)
                    {
                        Window_Main.Title = "ERROR: Error establishing BASS_SYNC_END on file stream.";
                    }
                    else
                    {
                        long_Duration_Bytes = Bass.BASS_ChannelGetLength(int_Stream);
                        double_Duration_Seconds = Bass.BASS_ChannelBytes2Seconds(int_Stream, long_Duration_Bytes);

                        Slider_Control.Minimum = 0;
                        Slider_Control.Maximum = double_Duration_Seconds;

                        Window_Main.Title = "Stream created.";

                        Create_Vertical_Axis();
                        Button_Play.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
                else
                {
                    Window_Main.Title = "ERROR: " + Bass.BASS_ErrorGetCode().ToString();
                }
            }
        }

        private void Handle_Window_Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //StackPanel_Graph.Children.Remove(Line_Horizontal_Axis);
            //Create_Horizontal_Axis();

            Update_Vertical_Axis();
        }

        private void Handle_Window_Main_Closing(object Sender, System.ComponentModel.CancelEventArgs E)
        {
            Bass.BASS_StreamFree(int_Stream);
            Bass.BASS_Free();
        }

        private void Create_Horizontal_Axis()
        {
            Line_Horizontal_Axis.Stroke = System.Windows.Media.Brushes.WhiteSmoke;

            Line_Horizontal_Axis.X1 = 0;
            Line_Horizontal_Axis.Y1 = StackPanel_Graph.ActualHeight / 2;

            Line_Horizontal_Axis.X2 = StackPanel_Graph.ActualWidth;
            Line_Horizontal_Axis.Y2 = StackPanel_Graph.ActualHeight / 2;

            Line_Horizontal_Axis.StrokeThickness = .5;

            StackPanel_Graph.Children.Add(Line_Horizontal_Axis);
        }

        private void Create_Vertical_Axis()
        {
            Line_Vertical_Axis.Stroke = System.Windows.Media.Brushes.WhiteSmoke;

            Line_Vertical_Axis.X1 = 0;
            Line_Vertical_Axis.Y1 = 0;

            Line_Vertical_Axis.X2 = 0;
            Line_Vertical_Axis.Y2 = StackPanel_Graph.ActualHeight;

            Line_Vertical_Axis.StrokeThickness = .5;

            StackPanel_Graph.Children.Add(Line_Vertical_Axis);
        }

        private void Update_Vertical_Axis()
        {
            double_Vertical_Axis_Margin_Left = ((Slider_Control.Value / Slider_Control.Maximum) * (StackPanel_Graph.ActualWidth));
            Line_Vertical_Axis.Margin = new Thickness(double_Vertical_Axis_Margin_Left, 0, 0, 0);
        }

        private void Handle_Button_Play_Click(object Sender, RoutedEventArgs E)
        {
            if (!bool_Playing)
            {
                bool_Playing = true;
                Image_Play.Source = new BitmapImage(new Uri("Image\\Pause.png", UriKind.RelativeOrAbsolute));

                Bass.BASS_ChannelPlay(int_Stream, false); 
                Window_Main.Title = "PLAYING";
            }
            else
            {
                bool_Playing = false;
                Image_Play.Source = new BitmapImage(new Uri("Image\\Play.png", UriKind.RelativeOrAbsolute));

                Bass.BASS_ChannelPause(int_Stream); 
                Window_Main.Title = "PAUSED";
            }
        }

        private void Handle_Button_Stop_Click(object Sender, RoutedEventArgs E)
        {
            bool_Playing = false;
            Image_Play.Source = new BitmapImage(new Uri("Image\\Play.png", UriKind.RelativeOrAbsolute));

            Bass.BASS_ChannelStop(int_Stream);
            Bass.BASS_ChannelSetPosition(int_Stream, 0); 
            Window_Main.Title = "STOPPED";
        }

        private void Handle_Media_End(int Handle, int Channel, int Data, IntPtr User)
        {
            bool_Playing = false;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            Image_Play.Source = new BitmapImage(new Uri("Image\\Play.png", UriKind.RelativeOrAbsolute))
            ));

            Bass.BASS_ChannelStop(int_Stream);

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            Window_Main.Title = "STOPPED (Media End Position Reached)"
            ));
        }

        private void Slider_Control_DragStarted(object Sender, DragStartedEventArgs E)
        {
            bool_Sliding = true;

            Window_Main.Title = "SLIDING STARTED";
        }

        private void Slider_Control_DragCompleted(object Sender, DragCompletedEventArgs E)
        {
            bool_Sliding = false;
            Bass.BASS_ChannelSetPosition(int_Stream, Bass.BASS_ChannelSeconds2Bytes(int_Stream, Slider_Control.Value));
            Window_Main.Title = "SLIDING COMPLETED";
        }

        private void Slider_Control_ValueChanged(object Sender, RoutedPropertyChangedEventArgs<double> E)
        {

            TextBlock_TimeCode.Text = TimeSpan.FromSeconds(Slider_Control.Value).ToString(@"hh\:mm\:ss");
            Update_Vertical_Axis();
        }

        private void Timer_Tick(object Sender, EventArgs E)
        {
            if ((int_Stream != 0) && (!bool_Sliding))
            {
                long_Position_Bytes = Bass.BASS_ChannelGetPosition(int_Stream);
                double_Position_Seconds = Bass.BASS_ChannelBytes2Seconds(int_Stream, long_Position_Bytes);

                Slider_Control.Value = double_Position_Seconds;
            }
        }
    }
}

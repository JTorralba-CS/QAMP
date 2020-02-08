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

        private int int_Level_Stereo = 0;

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

                        Slider_Axis.Minimum = 0;
                        Slider_Axis.Maximum = double_Duration_Seconds;

                        Window_Main.Title = "Stream created.";

                        Audio_Level();

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
            Canvas_Lines.Children.Clear();
            if (int_Stream != 0)
            {
                Audio_Level();
            }
        }

        private void Handle_Window_Main_Closing(object Sender, System.ComponentModel.CancelEventArgs E)
        {
            Bass.BASS_StreamFree(int_Stream);
            Bass.BASS_Free();
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

            if (Slider_Control.Value < Slider_Control.Maximum)
            {
                Bass.BASS_ChannelSetPosition(int_Stream, Bass.BASS_ChannelSeconds2Bytes(int_Stream, Slider_Control.Value));
            }
            else
            {
                Bass.BASS_ChannelSetPosition(int_Stream, Bass.BASS_ChannelSeconds2Bytes(int_Stream, Slider_Control.Maximum - 0.125));
            }
            Window_Main.Title = "SLIDING COMPLETED";
        }

        private void Slider_Control_ValueChanged(object Sender, RoutedPropertyChangedEventArgs<double> E)
        {
            TextBlock_TimeCode.Text = TimeSpan.FromSeconds(Slider_Control.Value).ToString(@"hh\:mm\:ss");

            Slider_Axis.Value = Slider_Control.Value;
        }

        private void Audio_Level()
        {
            int Stream;
            int NumFrames;

            Stream = Bass.BASS_StreamCreateFile(string_Media, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
            if (Stream == 0) throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            long TrackLengthInBytes = Bass.BASS_ChannelGetLength(Stream);
            long FrameLengthInBytes = Bass.BASS_ChannelSeconds2Bytes(Stream, 0.02D);
            NumFrames = (int)Math.Round(1f * TrackLengthInBytes / FrameLengthInBytes);

            SolidColorBrush LeftFill = new SolidColorBrush();
            LeftFill.Color = Colors.White;

            SolidColorBrush LeftOutline = new SolidColorBrush();
            LeftOutline.Color = Colors.LightGray;

            Polyline LeftPolyLine = new Polyline();
            LeftPolyLine.StrokeThickness = 1;
            LeftPolyLine.Fill = LeftFill;
            LeftPolyLine.StrokeLineJoin = PenLineJoin.Round;
            LeftPolyLine.Opacity = .75;

            PointCollection LeftPolyPoints = new PointCollection();

            SolidColorBrush RightFill = new SolidColorBrush();
            RightFill.Color = Colors.Red;

            SolidColorBrush RightOutline = new SolidColorBrush();
            RightOutline.Color = Colors.LightGray;

            Polyline RightPolyLine = new Polyline();
            RightPolyLine.StrokeThickness = 1;
            RightPolyLine.Fill = RightFill;
            RightPolyLine.StrokeLineJoin = PenLineJoin.Round;
            RightPolyLine.Opacity = .75;

            PointCollection RightPolyPoints = new PointCollection();

            LeftPolyPoints.Add(new System.Windows.Point((0 * (StackPanel_Graph.ActualWidth - 8) / NumFrames) - 4, StackPanel_Graph.ActualHeight / 2));
            RightPolyPoints.Add(new System.Windows.Point((0 * (StackPanel_Graph.ActualWidth - 8) / NumFrames) - 4, StackPanel_Graph.ActualHeight / 2));

            for (int i = 0; i < NumFrames; i++)
            {
                int_Level_Stereo = Bass.BASS_ChannelGetLevel(Stream);

                if (int_Level_Stereo == -1)
                {
                    int_Level_Stereo = 0;
                }
                double double_Level_Left = Utils.HighWord32(int_Level_Stereo) / (32768 / (StackPanel_Graph.ActualHeight / 2));
                double double_Level_Right = Utils.LowWord32(int_Level_Stereo) / (32768 / (StackPanel_Graph.ActualHeight / 2));

                LeftPolyPoints.Add(new System.Windows.Point((i * (StackPanel_Graph.ActualWidth - 8) / NumFrames) - 4, StackPanel_Graph.ActualHeight / 2 - double_Level_Left));

                RightPolyPoints.Add(new System.Windows.Point((i * (StackPanel_Graph.ActualWidth - 8) / NumFrames) - 4, StackPanel_Graph.ActualHeight / 2 + double_Level_Left));
            }

            RightPolyPoints.Add(new System.Windows.Point((NumFrames * (StackPanel_Graph.ActualWidth - 8) / NumFrames) - 4, StackPanel_Graph.ActualHeight / 2));
            RightPolyLine.Points = RightPolyPoints;
            Canvas_Lines.Children.Add(RightPolyLine);

            LeftPolyPoints.Add(new System.Windows.Point((NumFrames * (StackPanel_Graph.ActualWidth - 8) / NumFrames) - 4, StackPanel_Graph.ActualHeight / 2));
            LeftPolyLine.Points = LeftPolyPoints;
            Canvas_Lines.Children.Add(LeftPolyLine);
        }

        private void Timer_Tick(object Sender, EventArgs E)
        {
            if ((int_Stream != 0) && (!bool_Sliding))
            {
                if (Slider_Control.Clicked)
                {
                    if (Slider_Control.Position < Slider_Control.Maximum)
                    {
                        Bass.BASS_ChannelSetPosition(int_Stream, Slider_Control.Position);
                    }
                    else
                    {
                        Bass.BASS_ChannelSetPosition(int_Stream, Slider_Control.Position - 0.125);
                    }
                    

                    switch (Slider_Control.Button)
                    {
                        case "LEFT":
                            if (bool_Playing == false)
                            {
                                Button_Play.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                            }
                            break;
                        case "RIGHT":
                            if (bool_Playing == true)
                            {
                                Button_Play.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                            }
                            break;
                        default:
                            break;
                    };

                    TextBlock_DEBUG.Text = "SLIDER_" + Slider_Control.Button + "_BUTTON_CLICKED";

                    Slider_Control.Clicked = false;
                    Slider_Control.Button = null;
                }

                long_Position_Bytes = Bass.BASS_ChannelGetPosition(int_Stream);
                double_Position_Seconds = Bass.BASS_ChannelBytes2Seconds(int_Stream, long_Position_Bytes);

                Slider_Control.UpdateValue(double_Position_Seconds);
            }
        }
    }
}

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

        private bool bool_Playing = false;
        private bool bool_Sliding = false;

        public MainWindow()
        {
            InitializeComponent();

            Window_Main.Title = "QAMP " + typeof(MainWindow).Assembly.GetName().Version.ToString();
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

                        Window_Main.Title = "Stream created.";

                        Button_Play.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
                else
                {
                    Window_Main.Title = "ERROR: " + Bass.BASS_ErrorGetCode().ToString();
                }
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

            Window_Main.Title = "SLIDING COMPLETED";
        }

        private void Slider_Control_ValueChanged(object Sender, RoutedPropertyChangedEventArgs<double> E)
        {
        }
    }
}

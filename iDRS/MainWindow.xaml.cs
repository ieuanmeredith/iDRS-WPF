using irsdkSharp;
using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace iDRS
{
    public enum DrsStatus
    {
        NotAvailable = 0,
        Approaching = 1,
        Enabled = 2,
        On = 3
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _connectionStatus;

        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                if (string.Equals(value, _connectionStatus))
                    return;
                _connectionStatus = value;
                OnPropertyChanged("ConnectionStatus");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private WaveOutEvent shortOutputDevice;
        private WaveOutEvent longOutputDevice;
        private WaveOutEvent doubleOutputDevice;

        private AudioFileReader shortBeep;
        private AudioFileReader longBeep;
        private AudioFileReader doubleBeep;

        private static IRacingSDK sdk;

        private DrsStatus drsStatus = DrsStatus.NotAvailable;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            PlayDoubleBeep();

            sdk = new IRacingSDK();
            Task.Run(() => Loop());
            Console.ReadLine();
        }

        private void OnShortPlaybackStopped(object sender, StoppedEventArgs args)
        {
            shortOutputDevice.Dispose();
            shortOutputDevice = null;
            shortBeep.Dispose();
            shortBeep = null;
        }

        private void OnLongPlaybackStopped(object sender, StoppedEventArgs args)
        {
            longOutputDevice.Dispose();
            longOutputDevice = null;
            longBeep.Dispose();
            longBeep = null;
        }

        private void OnDoublePlaybackStopped(object sender, StoppedEventArgs args)
        {
            doubleOutputDevice.Dispose();
            doubleOutputDevice = null;
            doubleBeep.Dispose();
            doubleBeep = null;
        }

        private void Loop()
        {
            while (true)
            {
                var currentlyConnected = sdk.IsConnected();

                ConnectionStatus = currentlyConnected ? "Connected" : "Disconnected";

                if (currentlyConnected)
                {
                    var newStatus = (DrsStatus)sdk.GetData("DRS_Status");

                    switch (newStatus)
                    {
                        case DrsStatus.NotAvailable:
                            if (drsStatus != newStatus)
                            {
                                drsStatus = newStatus;
                            }
                            break;
                        case DrsStatus.Approaching:
                            if (drsStatus == DrsStatus.NotAvailable)
                            {
                                drsStatus = newStatus;
                                PlayDoubleBeep();
                            }
                            break;
                        case DrsStatus.Enabled:
                            if (drsStatus == DrsStatus.Approaching)
                            {
                                drsStatus = newStatus;
                                PlayLongBeep();
                            }
                            break;
                        case DrsStatus.On:
                            if (drsStatus == DrsStatus.Enabled)
                            {
                                drsStatus = newStatus;
                                PlayShortBeep();
                            }
                            break;
                    }
                   
                    Thread.Sleep(15);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void PlayShortBeep()
        {
            if (shortOutputDevice == null)
            {
                shortOutputDevice = new WaveOutEvent();
                shortOutputDevice.PlaybackStopped += OnShortPlaybackStopped;
            }
            if (shortBeep == null)
            {
                shortBeep = new AudioFileReader($"{System.AppDomain.CurrentDomain.BaseDirectory}/Assets/beep-07.mp3");
                shortOutputDevice.Init(shortBeep);
            }
            shortOutputDevice.Volume = 1;
            shortOutputDevice.Play();
        }

        private void PlayLongBeep()
        {
            if (longOutputDevice == null)
            {
                longOutputDevice = new WaveOutEvent();
                longOutputDevice.PlaybackStopped += OnLongPlaybackStopped;
            }
            if (longBeep == null)
            {
                longBeep = new AudioFileReader($"{System.AppDomain.CurrentDomain.BaseDirectory}/Assets/beep-02.mp3");
                longOutputDevice.Init(longBeep);
            }
            longOutputDevice.Volume = 0.1f;
            longOutputDevice.Play();
        }

        private void PlayDoubleBeep()
        {
            if (doubleOutputDevice == null)
            {
                doubleOutputDevice = new WaveOutEvent();
                doubleOutputDevice.PlaybackStopped += OnDoublePlaybackStopped;
            }
            if (doubleBeep == null)
            {
                doubleBeep = new AudioFileReader($"{System.AppDomain.CurrentDomain.BaseDirectory}/Assets/beep-24.mp3");
                doubleOutputDevice.Init(doubleBeep);
            }
            doubleOutputDevice.Volume = 1;
            doubleOutputDevice.Play();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

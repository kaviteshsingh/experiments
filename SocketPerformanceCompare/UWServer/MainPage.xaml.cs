using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.System.Threading;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.System.Threading.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Net;
using System.Net.Sockets;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWServer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion



        public MainPage()
        {
            this.InitializeComponent();

            DgPacketList = new ObservableCollection<string>();
            SimpleSocketPacketList = new ObservableCollection<string>();

            DgPackets = 0;
            SimpleSocketPackets = 0;

            DataContext = this;
        }

        static readonly Object _lockObject = new Object();

        private int _dgPackets;

        public int DgPackets
        {
            get { return _dgPackets; }
            set
            {
                _dgPackets = value;
                OnPropertyChanged("DgPackets");
            }
        }

        private DatagramSocket _dgSocket;

        public DatagramSocket DgSocket
        {
            get { return _dgSocket; }
            set { _dgSocket = value; }
        }

        private ObservableCollection<string> _dgPacketList;

        public ObservableCollection<string> DgPacketList
        {
            get { return _dgPacketList; }
            set { _dgPacketList = value; }
        }



        public async void SetupDgListener()
        {
            if(simpleSocket !=null)
            {               
                simpleSocket.Dispose();
                simpleSocket = null;

                SimpleSocketPacketList.Clear();
                SimpleSocketPackets = 0;                
            }

            if(DgSocket == null)
            {
                try
                {
                    DgSocket = new DatagramSocket();

                    DgSocket.MessageReceived += SocketListener_MessageReceived;

                    await DgSocket.BindServiceNameAsync("50000");
                    Debug.WriteLine("DatagramSocket:: On 50000 port.");
                }
                catch(Exception Ex)
                {
                    DgSocket = null;
                    Debug.WriteLine(Ex.Message + " " + Ex.StackTrace);
                    throw Ex;
                }
            }
        }

        private async void SocketListener_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            //int tpr = 0;
            //Debug.WriteLine("{0}::{1}::{2}::{3}",
            //tpr, args.LocalAddress.ToString(), args.RemoteAddress.ToString(), args.RemotePort.ToString());

            uint datalen = args.GetDataReader().UnconsumedBufferLength;

            DataReader reader = args.GetDataReader();
            byte[] dataBuffer = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(dataBuffer);


            //Debug.WriteLine("{0}::{1}", datalen, Encoding.ASCII.GetString(dataBuffer));
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock(_lockObject)
                {
                    DgPackets++;
                    DgPacketList.Add(Encoding.ASCII.GetString(dataBuffer));
                }
            });
        }

        private void btDatagramServer_Click(object sender, RoutedEventArgs e)
        {
            SetupDgListener();
        }





        private ObservableCollection<string> _simpleSocketPacketList;

        public ObservableCollection<string> SimpleSocketPacketList
        {
            get { return _simpleSocketPacketList; }
            set { _simpleSocketPacketList = value; }
        }


        private int _simpleSocketPackets;

        public int SimpleSocketPackets
        {
            get { return _simpleSocketPackets; }
            set { _simpleSocketPackets = value; OnPropertyChanged("SimpleSocketPackets"); }
        }

        private Socket simpleSocket;

        public Socket SimpleSocket
        {
            get { return simpleSocket; }
            set { simpleSocket = value; }
        }


        private void btSocketServer_Click(object sender, RoutedEventArgs e)
        {
            if(_dgSocket != null)
            {                
                _dgSocket.Dispose();
                _dgSocket = null;

                DgPackets = 0;
                DgPacketList.Clear();

            }


            if(simpleSocket == null)
            {
                SetupSimpleSocketListener();
                Debug.WriteLine("btWorkItemServer_Click:: Launched WorkItem"); 
            }
        }


        public void SetupSimpleSocketListener()
        {

            try
            {
                IPAddress LocalIp = IPAddress.Any;
                IPEndPoint localEndPoint = new IPEndPoint(LocalIp, 50000);
                SimpleSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

                byte[] buffer = new Byte[1024];
                SimpleSocket.Bind(localEndPoint);
                Debug.WriteLine("Bind Complete");

                SocketAsyncEventArgs sockarg = new SocketAsyncEventArgs();
                sockarg.Completed += Sockarg_Completed;
                sockarg.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                sockarg.SetBuffer(buffer, 0, buffer.Length);

                Debug.WriteLine("SocketAsyncEventArgs Complete");

                if(!SimpleSocket.ReceiveFromAsync(sockarg))
                {
                    Sockarg_Completed(this, sockarg);
                }

                //Debug.WriteLine("StartSocketServer:: Exit");
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }



        }

        private async void Sockarg_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Debug.WriteLine("{0}::{1}", e.BytesTransferred, e.Count);

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock(_lockObject)
                {
                    SimpleSocketPackets++;
                    SimpleSocketPacketList.Add(Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred));
                }
            });

            //Debug.WriteLine("Sockarg_Completed:: Complete");


            if(e.SocketError == System.Net.Sockets.SocketError.Success)
            {
                if(!SimpleSocket.ReceiveFromAsync(e))
                {
                    Sockarg_Completed(this, e);
                } 
            }
            else
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    lock(_lockObject)
                    {
                        SimpleSocketPackets = 0;
                        SimpleSocketPacketList.Clear();
                    }
                });
            }
        }
    }
}

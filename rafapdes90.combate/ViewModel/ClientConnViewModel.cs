using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using rafapdes90.combate.Model;
using rafapdes90.combate.View;

namespace rafapdes90.combate.ViewModel
{
    /// <inheritdoc />
    /// <summary>
    /// Inicia conexão a um socket remoto.
    /// </summary>
    public class ClientConnViewModel : ViewModelBase
    {
        private string _ipBox1 = string.Empty;
        public string IpBox1
        {
            get => _ipBox1;
            set => Set(() => this.IpBox1, ref _ipBox1, value);
        }

        private string _ipBox2 = string.Empty;
        public string IpBox2
        {
            get => _ipBox2;
            set => Set(() => this.IpBox2, ref _ipBox2, value);
        }

        private string _ipBox3 = string.Empty;
        public string IpBox3
        {
            get => _ipBox3;
            set => Set(() => this.IpBox3, ref _ipBox3, value);
        }

        private string _ipBox4 = string.Empty;
        public string IpBox4
        {
            get => _ipBox4;
            set => Set(() => this.IpBox4, ref _ipBox4, value);
        }

        private string _porta = string.Empty;
        public string Porta
        {
            get => _porta;
            set => Set(() => this.Porta, ref _porta, value);
        }

        private bool _connectButtonIsEnabled = true;
        public bool ConnectButtonIsEnabled
        {
            get => _connectButtonIsEnabled;
            set => Set(() => ConnectButtonIsEnabled, ref _connectButtonIsEnabled, value);
        }

        private string _connectButtonContent = "Conectar";
        public string ConnectButtonContent
        {
            get => _connectButtonContent;
            set => Set(() => this.ConnectButtonContent, ref _connectButtonContent, value);
        }

        private string EnderecoIp
        {
            get => this.IpBox1 + "." + this.IpBox2 + "." + this.IpBox3 + "." + this.IpBox4;
            set
            {
                this.IpBox1 = value;
                this.IpBox2 = value;
                this.IpBox3 = value;
                this.IpBox4 = value;
            }
        }

        public ICommand RequestConnectionCommand { get; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public ClientConnViewModel()
        {
            RequestConnectionCommand = new RelayCommand(RequestConnectionMethod);
            Messenger.Default.Register<NotificationMessage>(this, "Toggle_Client", ToggleClient);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);
        }

        private async void RequestConnectionMethod()
        {
            var serverConn = ServiceLocator.Current.GetInstance<ServerConnViewModel>();
            serverConn.SelfHost.Close();
            Console.WriteLine(serverConn.SelfHost.State.ToString());
            Console.WriteLine(serverConn.SelfHost.ChannelDispatchers.First().Listener?.Uri);

            Messenger.Default.Send(new NotificationMessage(string.Empty), "Toggle_Server");
            this.ConnectButtonContent = "Conectando..";

            await Task.Run(() =>
            {
                Socket sender = null;

                try
                {
                    // Create one SocketPermission for socket access restrictions
                    var socketPermission = new SocketPermission(
                        NetworkAccess.Connect,    // Connection permission
                        TransportType.Tcp,        // Defines transport types
                        string.Empty,                       // Gets the IP addresses
                        SocketPermission.AllPorts // Specifies all ports
                    );

                    // Ensures the code to have permission to access a Socket
                    if (!socketPermission.IsUnrestricted())
                    {
                        socketPermission.Demand();
                    }

                    // Creates a network endpoint
                    IPAddress ipAddr = IPAddress.Parse(this.EnderecoIp);
                    var ipEndPoint = new IPEndPoint(ipAddr, int.Parse(this.Porta));

                    // Create one Socket object to setup Tcp connection
                    sender = new Socket(
                            AddressFamily.InterNetwork, // Specifies the addressing scheme
                            SocketType.Stream, // The type of socket 
                            ProtocolType.Tcp // Specifies the protocols 
                        )
                    { NoDelay = false }; // Using the Nagle algorithm

                    // Establishes a connection to a remote host
                    sender.Connect(ipEndPoint);

                    Messenger.Default.Send(new GenericMessage<Socket>(sender), "ClientConn_Handler");
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"{0}", e.Message);

                    Messenger.Default.Send(new NotificationMessage(string.Empty), "Toggle_Server");
                    sender?.Close();
                    this.ConnectButtonContent = "Conectar";
                }
            });
        }

        private void ToggleClient(NotificationMessage notificationMessage)
        {
            this.ConnectButtonIsEnabled = !this.ConnectButtonIsEnabled;
            this.EnderecoIp = string.Empty;
            this.Porta = string.Empty;
        }

        private void ResetAll()
        {
            this.EnderecoIp = string.Empty;
            this.Porta = string.Empty;
            this.ConnectButtonIsEnabled = true;
            this.ConnectButtonContent = "Conectar";
        }

        private async void NetConnLost(NotificationMessage notificationMsg)
        {
            ResetAll();

            bool isClient = bool.Parse(notificationMsg.Notification);
            if (isClient)
            {
                MainViewModel main = ServiceLocator.Current.GetInstance<MainViewModel>();
                main.MensagemFinal = "O outro jogador desistiu!";
                await Task.Run(() =>
                    Application.Current.Dispatcher.Invoke(() =>
                        ((MainWindow)Application.Current.MainWindow)?.CallDesistir()));
            }
        }
    }
}

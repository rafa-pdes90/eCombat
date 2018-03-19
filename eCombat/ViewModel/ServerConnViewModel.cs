using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;

namespace eCombat.ViewModel
{
    /// <inheritdoc />
    /// <summary>
    /// Inicializa um socket em modo listen, divulga infos de IP:Port no ConnectWindow e começa a aceitar conexões.
    /// </summary>
    public class ServerConnViewModel : ViewModelBase
    {
        private string _ipPortInfo = string.Empty;
        public string IpPortInfo
        {
            get => _ipPortInfo;
            set => Set(() => IpPortInfo, ref _ipPortInfo, value);
        }

        private bool _ipPortTextBoxIsEnabled = false;
        public bool IpPortTextBoxIsEnabled
        {
            get => _ipPortTextBoxIsEnabled;
            set => Set(() => IpPortTextBoxIsEnabled, ref _ipPortTextBoxIsEnabled, value);
        }

        private bool _listenButtonIsEnabled = true;
        public bool ListenButtonIsEnabled
        {
            get => _listenButtonIsEnabled;
            set => Set(() => ListenButtonIsEnabled, ref _listenButtonIsEnabled, value);
        }

        private string _listenButtonContent = "Solicitar";
        public string ListenButtonContent
        {
            get => _listenButtonContent;
            set => Set(() => ListenButtonContent, ref _listenButtonContent, value);
        }

        private Socket SListener { get; set; }
        public bool AcceptingConnection { get; private set; } = false;
        public ICommand ToggleListeningCommand { get; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the ServerConnViewModel class.
        /// </summary>
        public ServerConnViewModel()
        {
            ToggleListeningCommand = new RelayCommand(ListenConnectionMethod);
            Messenger.Default.Register<NotificationMessage>(this, "Toggle_Server", ToggleServer);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);
        }

        public ServiceHost SelfHost { get; set; }
        public GameMasterClient GameMaster { get; set; }
        private async void ListenConnectionMethod()
        {
            if (this.ListenButtonContent != "Cancelar")
            {
                try
                {
                    /*
                    var localBaseAddress = new Uri("net.tcp://" + GetLocalIp());
                    SelfHost = new ServiceHost(typeof(CombateSvc), localBaseAddress);
                    */
                    this.SelfHost = new ServiceHost(typeof(CombateSvc));
                    this.SelfHost.Open();

                    /*
                    IChannelListener channelListener = SelfHost.ChannelDispatchers[1].Listener;
                    if (channelListener != null)
                    {
                        Uri myUri = channelListener.Uri;
                        var myResolveCriteria = new ResolveCriteria(new EndpointAddress(myUri));
                        ResolveResponse proxyResponse = discoveryClient.Resolve(myResolveCriteria);
                        EndpointDiscoveryMetadata myMetadata = proxyResponse.EndpointDiscoveryMetadata;
                        foreach (var uriDoida in myMetadata.ListenUris)
                            Console.WriteLine(uriDoida);
                        Console.WriteLine(myMetadata.Address);
                        string myPublicName = myMetadata.Extensions.First(x => x.Name.LocalName == "Name").Value;
                        Console.WriteLine(myPublicName);
                    }
                    */

                    this.GameMaster = new GameMasterClient("Server_IGameMaster");
                    await this.GameMaster.IntroduceToGameMasterAsync(0);
                    await this.GameMaster.DoWorkAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    this.SelfHost.Abort();
                }
            }

            Messenger.Default.Send(new NotificationMessage(string.Empty), "Toggle_Client");
            if (this.ListenButtonContent == "Cancelar")
            {
                ResetAll();
                return;
            }
            this.ListenButtonContent = "Cancelar";

            await Task.Run(() =>
            {
                try
                {
                    // Creates one SocketPermission object for access restrictions
                    var socketPermission = new SocketPermission(
                        NetworkAccess.Accept, // Allowed to accept connections
                        TransportType.Tcp, // Defines transport types
                        string.Empty, // The IP addresses of local host
                        SocketPermission.AllPorts // Specifies all ports
                    );

                    // Ensures the code to have permission to access a Socket
                    if (!socketPermission.IsUnrestricted())
                    {
                        socketPermission.Demand();
                    }

                    // Descobre o próprio IP(v4) na rede local
                    IPAddress ipAddr = GetLocalIp();

                    // Creates a network endpoint
                    var ipEndPoint = new IPEndPoint(ipAddr, 0);

                    // Create one Socket object to listen the incoming connection
                    this.SListener = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp
                    );

                    // Associates a Socket with a local endpoint
                    this.SListener.Bind(ipEndPoint);

                    // Atualiza IpPortInfo
                    this.IpPortInfo = IPAddress.Parse(((IPEndPoint)this.SListener.LocalEndPoint).Address.ToString()) +
                                      ":" +
                                      ((IPEndPoint)this.SListener.LocalEndPoint).Port;
                    this.IpPortTextBoxIsEnabled = true;

                    // Places a Socket in a listening state and specifies the maximum
                    // Length of the pending connections queue
                    this.SListener.Listen(0);
                    this.AcceptingConnection = true;

                    // Begins an asynchronous operation to accept an attempt
                    var aCallback = new AsyncCallback(AcceptCallback);
                    this.SListener.BeginAccept(aCallback, this.SListener);
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"{0}", e.Message);
                    ResetAll();
                }
            });
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get Listening Socket object
                var listener = (Socket)ar.AsyncState;

                // Create a new socket
                Socket handler = listener.EndAccept(ar);

                if (this.AcceptingConnection)
                {
                    // Using the Nagle algorithm
                    handler.NoDelay = false;

                    this.AcceptingConnection = false;
                    Messenger.Default.Send(new GenericMessage<Socket>(handler), "ServerConn_Handler");
                }
                else
                {
                    Messenger.Default.Send(new GenericMessage<Socket>(handler), "ServerConn_Reject");
                }

                // Begins an asynchronous operation to accept an attempt
                var aCallback = new AsyncCallback(AcceptCallback);
                listener.BeginAccept(aCallback, listener);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"{0}", e.Message);
            }
        }

        private static IPAddress GetLocalIp()
        {
            IPAddress localIp = Array.Find(
                Dns.GetHostEntry(string.Empty).AddressList,
                x => x.AddressFamily == AddressFamily.InterNetwork
                     && x.ToString().Split('.').Last() != "1");

            return localIp;
        }

        private void ToggleServer(NotificationMessage notificationMessage)
        {
            this.ListenButtonIsEnabled = !this.ListenButtonIsEnabled;
            this.IpPortInfo = string.Empty;
            this.IpPortTextBoxIsEnabled = false;
        }

        private void ResetAll()
        {
            this.IpPortInfo = string.Empty;
            this.IpPortTextBoxIsEnabled = false;
            if (this.SListener != null)
            {
                this.SListener.Close();
                this.SListener = null;
            }
            this.AcceptingConnection = false;
            this.ListenButtonIsEnabled = true;
            this.ListenButtonContent = "Solicitar";
        }

        private async void NetConnLost(NotificationMessage notificationMsg)
        {
            ResetAll();

            bool isClient = bool.Parse(notificationMsg.Notification);
            if (!isClient)
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

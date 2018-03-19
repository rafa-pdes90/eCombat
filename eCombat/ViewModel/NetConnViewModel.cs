using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;

namespace eCombat.ViewModel
{
    /// <inheritdoc />
    /// <summary>
    /// Recebe o handler socket e inicializa a comunicação.
    /// <para></para>
    /// Possui os métodos de envio e recebimento, tanto síncronos quanto async.
    /// </summary>
    public class NetConnViewModel : ViewModelBase
    {
        private int MaxMsgSize { get; } = 150;
        private bool IsClient { get; set; }

        public Socket Handler { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public NetConnViewModel()
        {
            Messenger.Default.Register<GenericMessage<Socket>>(this, "ServerConn_Handler", ServerConnSet);
            Messenger.Default.Register<GenericMessage<Socket>>(this, "ServerConn_Reject", ServerConnReject);
            Messenger.Default.Register<GenericMessage<Socket>>(this, "ClientConn_Handler", ClientConnSet);
        }

        private void NetConnSet()
        {
            try
            {
                // Receiving byte array 
                var buffer = new byte[this.MaxMsgSize];
                var obj = new object[2];

                // Continues to asynchronously receive data
                obj[0] = buffer;
                obj[1] = this.Handler;
                this.Handler.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, obj);
            }
            catch (SocketException e)
            {
                Console.WriteLine(@"{0} Error code: {1}.", e.Message, e.ErrorCode);
                Messenger.Default.Send(new NotificationMessage(this.IsClient.ToString()), "NetConn_Lost");
            }

            Messenger.Default.Send(new GenericMessage<NetConnViewModel>(this), "NetConn_Set");
        }

        private void ClientConnSet(GenericMessage<Socket> genericMsg)
        {
            this.Handler = genericMsg.Content;

            string convite;
            do
            {
                Receive(this.Handler, out convite);
                switch (convite)
                {
                    case "GAME?":
                        Send(this.Handler, "GAME!");
                        this.IsClient = true;
                        Messenger.Default.Send(new GenericMessage<bool>(true), "PlayerIsClient_Set");
                        NetConnSet();
                        break;
                    case ".":
                        this.Handler.Shutdown(SocketShutdown.Receive);
                        this.Handler.Close();
                        return;
                    default:
                        convite = null;
                        break;
                }
            } while (convite == null);
        }

        private void ServerConnSet(GenericMessage<Socket> genericMsg)
        {
            this.Handler = genericMsg.Content;

            Send(this.Handler, "GAME?");

            string retorno;
            do
            {
                Receive(this.Handler, out retorno);
                if (retorno == "GAME!")
                {
                    this.IsClient = false;
                    Messenger.Default.Send(new GenericMessage<bool>(false), "PlayerIsClient_Set");
                    NetConnSet();
                }
                else
                {
                    retorno = null;
                }
            } while (retorno == null);
        }

        private void ServerConnReject(GenericMessage<Socket> genericMsg)
        {
            Socket tempHandler = genericMsg.Content;

            Send(tempHandler, ".");
            tempHandler.Shutdown(SocketShutdown.Receive);
            tempHandler.Close();
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Fetch a user-defined object that contains information
                var obj = (object[])ar.AsyncState;

                // Received byte array
                var buffer = (byte[])obj[0];

                // A Socket to handle remote host communication.
                var handler = (Socket)obj[1];

                // Received message
                string content = string.Empty;

                // The number of bytes received.
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    content += Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Continues to asynchronously receive data
                    var buffernew = new byte[this.MaxMsgSize];
                    obj[0] = buffernew;
                    obj[1] = handler;
                    handler.BeginReceive(buffernew, 0, buffernew.Length, SocketFlags.None, ReceiveCallback, obj);

                    Messenger.Default.Send(new NotificationMessage(content), "NetMsg_In");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(@"{0} Error code: {1}.", e.Message, e.ErrorCode);
                Messenger.Default.Send(new NotificationMessage(this.IsClient.ToString()), "NetConn_Lost");
            }
        }

        // ReSharper disable once UnusedMember.Local
        public void SendCallback(IAsyncResult ar)
        {
            try
            {
                // A Socket which has sent the data to remote host
                var handler = (Socket)ar.AsyncState;

                // The number of bytes sent to the Socket
                // ReSharper disable once UnusedVariable
                int bytesSend = handler.EndSend(ar);
            }
            catch (SocketException e)
            {
                Console.WriteLine(@"{0} Error code: {1}.", e.Message, e.ErrorCode);
                Messenger.Default.Send(new NotificationMessage(this.IsClient.ToString()), "NetConn_Lost");
            }
        }

        public void Receive(Socket tempHandler, out string content)
        {
            content = null;
            var bytes = new byte[this.MaxMsgSize];

            try
            {
                // Get reply from the server.
                int bytesRead = tempHandler.Receive(bytes);
                content = Encoding.UTF8.GetString(bytes, 0, bytesRead);
            }
            catch (SocketException e)
            {
                Console.WriteLine(@"{0} Error code: {1}.", e.Message, e.ErrorCode);
                Messenger.Default.Send(new NotificationMessage(this.IsClient.ToString()), "NetConn_Lost");
            }
        }

        public void Send(Socket tempHandler, string content)
        {
            byte[] msg = Encoding.UTF8.GetBytes(content);

            try
            {
                // Blocks until send returns.
                // ReSharper disable once UnusedVariable
                int bytesSend = tempHandler.Send(msg);
            }
            catch (SocketException e)
            {
                Console.WriteLine(@"{0} Error code: {1}.", e.Message, e.ErrorCode);
                Messenger.Default.Send(new NotificationMessage(this.IsClient.ToString()), "NetConn_Lost");
            }
        }
    }
}

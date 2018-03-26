using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;

namespace eCombat.ViewModel
{
    class NetMsgViewModel : ViewModelBase
    {
        private NetConnViewModel NetConn { get; } = ServiceLocator.Current.GetInstance<NetConnViewModel>();

        public NetMsgViewModel()
        {
            Messenger.Default.Register<NotificationMessage>(this, "NetMsg_In", NetMsgIn);
        }

        private void NetMsgIn(NotificationMessage notificationMessage)
        {
            string[] novaMsg = notificationMessage.Notification.Split(null, 2);
            var msgContent = new NotificationMessage(novaMsg[1]);

            switch (novaMsg[0])
            {
                case "c":
                    Messenger.Default.Send(msgContent, "Chat_In");
                    break;
                case "f":
                    Messenger.Default.Send(msgContent, "Feedback_In");
                    break;
                case "m":
                    Messenger.Default.Send(msgContent, "Move_In");
                    break;
                case "g":
                    MainViewModel main = ServiceLocator.Current.GetInstance<MainViewModel>();
                    main.MensagemFinal = "O outro jogador desistiu!";
                    Application.Current.Dispatcher.Invoke(() =>
                            ((MainWindow)Application.Current.MainWindow)?.CallDesistir());
                    break;
                default:
                    Console.WriteLine(novaMsg[1]);
                    break;
            }
        }

        public void NetMsgAsync(string message)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(message);
            // Sends data asynchronously to a connected Socket 
            //this.NetConn.Handler.BeginSend(byteData, 0, byteData.Length, 0, this.NetConn.SendCallback, this.NetConn.Handler);
        }

        public void NetMsgSend(string message)
        {
            //this.NetConn.Send(this.NetConn.Handler, message);
        }
    }
}

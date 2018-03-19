using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ChatMsgViewModel : ViewModelBase
    {
        private string _playerColor;
        public string PlayerColor
        {
            get => _playerColor;
            set => Set(() => PlayerColor, ref _playerColor, value);
        }

        private string _opponentColor;
        public string OpponentColor
        {
            get => _opponentColor;
            set => Set(() => OpponentColor, ref _opponentColor, value);
        }

        private NetMsgViewModel NetMsg { get; } = ServiceLocator.Current.GetInstance<NetMsgViewModel>();
        public ObservableCollection<ChatMsg> ChatList { get; } = new ObservableCollection<ChatMsg>();

        public ChatMsgViewModel()
        {
            Messenger.Default.Register<NotificationMessage>(this, "Chat_In", ChatIn);
            Messenger.Default.Register<NotificationMessage>(this, "Chat_Async", ChatAsync);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);
            Messenger.Default.Register<GenericMessage<bool>>(this, "PlayerIsClient_Set", PlayerIsClientSet);
        }

        private void PlayerIsClientSet(GenericMessage<bool> genericMessage)
        {
            bool playerIsClient = genericMessage.Content;

            Application.Current.Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow)?.SetPlayerColor(playerIsClient));
            PlayerColor = playerIsClient ? "CornflowerBlue" : "Orange";
            OpponentColor = playerIsClient ? "Orange" : "CornflowerBlue";
        }

        private void ChatIn(NotificationMessage notificationMessage)
        {
            string[] msg = notificationMessage.Notification.Split(null, 2);

            var recebida = new ChatMsg(int.Parse(msg[0]), "r", msg[1]);
            Application.Current.Dispatcher.Invoke(() => this.ChatList.Add(recebida));
        }

        private void ChatAsync(NotificationMessage notificationMessage)
        {
            int msgId = ChatList.Count + 1;
            string msg = "c" + " " + msgId + " " + notificationMessage.Notification;

            NetMsg.NetMsgAsync(msg);

            var enviada = new ChatMsg(msgId, "s", notificationMessage.Notification);
            this.ChatList.Add(enviada);
        }

        private void ResetAll()
        {
            Application.Current.Dispatcher.Invoke(() => this.ChatList.Clear());
        }

        private void NetConnLost(NotificationMessage notificationMsg)
        {
            ResetAll();
        }
    }
}

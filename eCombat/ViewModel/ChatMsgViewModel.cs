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
        /// <summary>
        /// The <see cref="OpponentName" /> property's name.
        /// </summary>
        public const string OpponentNamePropertyName = "OpponentName";

        private string _opponentName = "";

        /// <summary>
        /// Sets and gets the OpponentName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string OpponentName
        {
            get => _opponentName;
            set => Set(() => OpponentName, ref _opponentName, value);
        }

        //private NetMsgViewModel NetMsg { get; } = ServiceLocator.Current.GetInstance<NetMsgViewModel>();
        public ObservableCollection<ChatMsg> ChatList { get; } = new ObservableCollection<ChatMsg>();

        public ChatMsgViewModel()
        {
            Messenger.Default.Register<string>(this, "OpponentName", SetOpponentName);


            Messenger.Default.Register<NotificationMessage>(this, "Chat_In", ChatIn);
            Messenger.Default.Register<NotificationMessage>(this, "Chat_Async", ChatAsync);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);
        }

        private void SetOpponentName(string name)
        {
            this.OpponentName = name;
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

            //NetMsg.NetMsgAsync(msg);

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

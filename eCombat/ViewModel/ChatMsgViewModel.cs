using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;
using GalaSoft.MvvmLight.CommandWpf;

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

        /// <summary>
        /// The <see cref="SendTextContent" /> property's name.
        /// </summary>
        public const string SendTextContentPropertyName = "SendTextContent";

        private string _sendTextContent = string.Empty;

        /// <summary>
        /// Sets and gets the SendTextContent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SendTextContent
        {
            get => _sendTextContent;
            set => Set(() => SendTextContent, ref _sendTextContent, value);
        }

        public ObservableCollection<ChatMsg> ChatList { get; } = new ObservableCollection<ChatMsg>();

        public ICommand SendButtonCommand { get; }

        public ChatMsgViewModel()
        {
            Messenger.Default.Register<string>(this, "OpponentName", SetOpponentName);
            Messenger.Default.Register<ChatMsg>(this, "Chat_In", ChatIn);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);

            SendButtonCommand = new RelayCommand(SendButtonMethod);
        }

        private void SetOpponentName(string name)
        {
            this.OpponentName = name;
        }

        private void SendButtonMethod()
        {
            if (this.SendTextContent == "") return;

            GameMaster.Client.WriteMessageToChatAsync(this.SendTextContent);

            this.SendTextContent = string.Empty;
        }

        private void ChatIn(ChatMsg chatMessage)
        {
            Application.Current.Dispatcher.Invoke(() => this.ChatList.Add(chatMessage));
        }

        private void ResetAll()
        {
            Application.Current.Dispatcher.Invoke(() => this.SendTextContent = string.Empty);
            Application.Current.Dispatcher.Invoke(() => this.ChatList.Clear());
        }

        private void NetConnLost(NotificationMessage notificationMsg)
        {
            ResetAll();
        }
    }
}

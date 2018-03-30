using System.Collections.ObjectModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;

namespace eCombat.ViewModel
{
    public class ChatViewModel : ViewModelBase
    {
        public SelfPlayer SelfPlayer => SelfPlayer.Instance;
        public Opponent Opponent => Opponent.Instance;

        /// <summary>
        /// The <see cref="TypingBoxText" /> property's name.
        /// </summary>
        public const string TypingBoxTextPropertyName = "TypingBoxText";

        private string _typingBoxText;

        /// <summary>
        /// Sets and gets the TypingBoxText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TypingBoxText
        {
            get => this._typingBoxText;
            set => Set(() => this.TypingBoxText, ref this._typingBoxText, value);
        }

        /// <summary>
        /// The <see cref="TypingBoxTextLimit" /> property's name.
        /// </summary>
        public const string TypingBoxTextLimitPropertyName = "TypingBoxTextLimit";

        private int _typingBoxTextLimit;

        /// <summary>
        /// Sets and gets the TypingBoxTextLimit property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int TypingBoxTextLimit
        {
            get => this._typingBoxTextLimit;
            set => Set(() => this.TypingBoxTextLimit, ref this._typingBoxTextLimit, value, true);
        }

        /// <summary>
        /// The <see cref="TypingBoxWatermark" /> property's name.
        /// </summary>
        public const string TypingBoxWatermarkPropertyName = "TypingBoxWatermark";

        private string _typingBoxWatermark;

        /// <summary>
        /// Sets and gets the TypingBoxWatermark property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TypingBoxWatermark
        {
            get => this._typingBoxWatermark;
            set => Set(() => this.TypingBoxWatermark, ref this._typingBoxWatermark, value);
        }

        /// <summary>
            /// The <see cref="ChatMsgList" /> property's name.
            /// </summary>
        public const string ChatMsgListPropertyName = "ChatMsgList";

        private ObservableCollection<ChatMsg> _chatMsgList;

        /// <summary>
        /// Sets and gets the ChatMsgList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ChatMsg> ChatMsgList
        {
            get => this._chatMsgList;
            set => Set(() => this.ChatMsgList, ref this._chatMsgList, value);
        }


        private RelayCommand _sendCommand;

        /// <summary>
        /// Gets the SendCommand.
        /// </summary>
        public RelayCommand SendCommand
        {
            get
            {
                return this._sendCommand
                       ?? (this._sendCommand = new RelayCommand(
                           () =>
                           {
                               GameMaster.Client.WriteMessageToChat(this.TypingBoxText);

                               this.TypingBoxText = string.Empty;
                           },
                           () => !string.IsNullOrEmpty(this.TypingBoxText)));
            }
        }


        public ChatViewModel()
        {
            Messenger.Default.Register<PropertyChangedMessage<int>>(this,
                x => this.TypingBoxWatermark = x.NewValue + " characters");
            
            Messenger.Default.Register<ChatMsg>(this, "NewChatMsg", AddToChatMsgList);
            Messenger.Default.Register<char>(this, "HardReset", x => HardReset());

            HardReset();
        }

        private void HardReset()
        {
            this.ChatMsgList = new ObservableCollection<ChatMsg>();
            this.TypingBoxText = string.Empty;
            this.TypingBoxTextLimit = 140;
        }

        private void AddToChatMsgList(ChatMsg chatMessage)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.ChatMsgList.Add(chatMessage));
        }
    }
}

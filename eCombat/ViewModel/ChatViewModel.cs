using System.Collections.ObjectModel;
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
        /// The <see cref="PostText" /> property's name.
        /// </summary>
        public const string PostTextPropertyName = "PostText";

        private string _postText;

        /// <summary>
        /// Sets and gets the PostText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PostText
        {
            get => this._postText;
            set => Set(() => this.PostText, ref this._postText, value);
        }

        /// <summary>
        /// The <see cref="PostSizeLimit" /> property's name.
        /// </summary>
        public const string PostSizeLimitPropertyName = "PostSizeLimit";

        private int _postSizeLimit;

        /// <summary>
        /// Sets and gets the PostSizeLimit property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int PostSizeLimit
        {
            get => this._postSizeLimit;
            set => Set(() => this.PostSizeLimit, ref this._postSizeLimit, value, true);
        }

        public ObservableCollection<ChatMsg> ChatMsgList { get; set; }

        private RelayCommand _postCommand;

        /// <summary>
        /// Gets the PostCommand.
        /// </summary>
        public RelayCommand PostCommand =>
            this._postCommand ?? (this._postCommand = new RelayCommand(PostMethod,
                () => !string.IsNullOrEmpty(this.PostText)));
        

        public ChatViewModel()
        {
            Messenger.Default.Register<ChatMsg>(this, "NewChatMsg", AddToChatMsgList);
            Messenger.Default.Register<int>(this, "HardReset", x => HardReset());

            this.ChatMsgList = new ObservableCollection<ChatMsg>();

            HardReset();
        }

        private void HardReset()
        {
            this.PostText = string.Empty;
            this.PostSizeLimit = 140;
            this.ChatMsgList.Clear();
        }

        private void AddToChatMsgList(ChatMsg chatMessage)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.ChatMsgList.Add(chatMessage));
        }

        private void PostMethod()
        {
            GameMaster.Client.WriteMessageToChat(this.PostText);

            this.PostText = string.Empty;
        }
    }
}

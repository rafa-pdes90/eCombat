using System.Windows;
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
        /// <summary>
        /// The <see cref="WelcomeMessageContent" /> property's name.
        /// </summary>
        public const string WelcomeMessageContentPropertyName = "WelcomeMessageContent";

        private string _welcomeMessageContent = "Welcome to eCombat!";
        
        /// <summary>
        /// Sets and gets the WelcomeMessage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeMessageContent
        {
            get => _welcomeMessageContent;
            set => Set(() => WelcomeMessageContent, ref _welcomeMessageContent, value);
        }

        /// <summary>
        /// The <see cref="NicknameContent" /> property's name.
        /// </summary>
        public const string NicknameContentPropertyName = "NicknameContent";

        private string _nicknameContent = "Enter a Nickname";

        /// <summary>
        /// Sets and gets the NicknameLabel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NicknameContent
        {
            get => _nicknameContent;
            set => Set(() => NicknameContent, ref _nicknameContent, value);
        }

        /// <summary>
        /// The <see cref="NicknameTextIsEnabled" /> property's name.
        /// </summary>
        public const string NicknameTextIsEnabledPropertyName = "NicknameTextIsEnabled";

        private bool _nicknameTextIsEnabled = true;

        /// <summary>
        /// Sets and gets the NicknameTextIsEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool NicknameTextIsEnabled
        {
            get => _nicknameTextIsEnabled;
            set => Set(() => NicknameTextIsEnabled, ref _nicknameTextIsEnabled, value);
        }

        /// <summary>
        /// The <see cref="NicknameText" /> property's name.
        /// </summary>
        public const string NicknameTextPropertyName = "NicknameText";

        private string _nicknameText = "";

        /// <summary>
        /// Sets and gets the Nickname property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NicknameText
        {
            get => _nicknameText;
            set => Set(() => NicknameText, ref _nicknameText, value);
        }

        /// <summary>
        /// The <see cref="RequestOrCancelMatchIsEnabled" /> property's name.
        /// </summary>
        public const string RequestOrCancelMatchIsEnabledPropertyName = "RequestOrCancelMatchIsEnabled";

        private bool _requestorCancelMatchIsEnabled = false;

        /// <summary>
        /// Sets and gets the RequestOrCancelMatchIsEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool RequestOrCancelMatchIsEnabled
        {
            get => _requestorCancelMatchIsEnabled;
            set => Set(() => RequestOrCancelMatchIsEnabled, ref _requestorCancelMatchIsEnabled, value);
        }

        /// <summary>
        /// The <see cref="RequestOrCancelMatchContent" /> property's name.
        /// </summary>
        public const string RequestOrCancelMatchContentPropertyName = "RequestOrCancelMatchContent";

        private string _requestOrCancelMatchContent = "Conectando...";

        /// <summary>
        /// Sets and gets the RequestOrCancelMatchContent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RequestOrCancelMatchContent
        {
            get => _requestOrCancelMatchContent;
            set => Set(() => RequestOrCancelMatchContent, ref _requestOrCancelMatchContent, value);
        }

        /// <summary>
        /// The <see cref="RequestOrCancelMatchLoadingVisibility" /> property's name.
        /// </summary>
        public const string RequestOrCancelMatchLoadingVisibilityPropertyName = "RequestOrCancelMatchLoadingVisibility";

        private Visibility _requestOrCancelMatchLoadingVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the RequestOrCancelMatchLoadingVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility RequestOrCancelMatchLoadingVisibility
        {
            get => _requestOrCancelMatchLoadingVisibility;
            set => Set(() => RequestOrCancelMatchLoadingVisibility, ref _requestOrCancelMatchLoadingVisibility, value);
        }

        private RelayCommand _requestOrCancelMatchCommand;

        /// <summary>
        /// Gets the RequestOrCancelMatchCommand.
        /// </summary>
        public RelayCommand RequestOrCancelMatchCommand
        {
            get
            {
                return _requestOrCancelMatchCommand
                    ?? (_requestOrCancelMatchCommand = new RelayCommand(async () =>
                           {
                               if (!this.RequestOrCancelMatchIsEnabled || this.NicknameText.Length == 0) return;

                               if (this.RequestOrCancelMatchLoadingVisibility.Equals(Visibility.Collapsed))
                               {
                                   this.NicknameTextIsEnabled = false;
                                   this.RequestOrCancelMatchLoadingVisibility = Visibility.Visible;
                                   this.RequestOrCancelMatchContent = "Cancel";

                                   Messenger.Default.Send(true, "SetIsConnecting");

                                   await GameMaster.Client.FaceMatchAsync(NicknameText);
                               }
                               else
                               {
                                   this.NicknameTextIsEnabled = true;
                                   this.RequestOrCancelMatchLoadingVisibility = Visibility.Collapsed;
                                   this.RequestOrCancelMatchContent = "Ready to Play!";

                                   Messenger.Default.Send(false, "SetIsConnecting");

                                   GameMaster.Client.CancelMatch();
                               }
                           }));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public NetConnViewModel()
        {
            Messenger.Default.Register<bool>(this, "ChangeRequestOrCancelState", ChangeRequestOrCancelState);
            Messenger.Default.Register<int>(this, "RunRequestOrCancelCommand",
                token => RunRequestOrCancelCommand());
        }

        private void ChangeRequestOrCancelState(bool value)
        {
            this.RequestOrCancelMatchIsEnabled = value;

            Messenger.Default.Send(this.NicknameText, "PlayerName");
        }

        private async void RunRequestOrCancelCommand()
        {
            this.RequestOrCancelMatchIsEnabled = true;

            Messenger.Default.Send(true, "SetIsConnecting");

            await GameMaster.Client.FaceMatchAsync(NicknameText);
        }
    }
}

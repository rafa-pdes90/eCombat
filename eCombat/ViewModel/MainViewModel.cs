using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;

namespace eCombat.ViewModel
{

    /// <inheritdoc />
    /// <summary>
    /// Janela principal.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="IsConnecting" /> property's name.
        /// </summary>
        public const string IsConnectingPropertyName = "IsConnecting";

        private bool _isConnecting = false;

        /// <summary>
        /// Sets and gets the IsConnecting property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsConnecting
        {
            get => _isConnecting;
            set => Set(() => IsConnecting, ref _isConnecting, value);
        }

        /// <summary>
        /// The <see cref="IsPlaying" /> property's name.
        /// </summary>
        public const string IsPlayingPropertyName = "IsPlaying";

        private bool _isPlaying = false;

        /// <summary>
        /// Sets and gets the IsPlaying property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(() => IsPlaying, ref _isPlaying, value);
        }

        private RelayCommand _giveUpCommand;

        /// <summary>
        /// Gets the GiveUpCommand.
        /// </summary>
        public RelayCommand GiveUpCommand =>
            this._giveUpCommand ?? (this._giveUpCommand = new RelayCommand(GiveUpMethod,
                () => this.IsPlaying));

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<int>(this, "StartNewMatch", token => StartNewMatch());
            Messenger.Default.Register<bool>(this, "SetIsConnecting", SetIsConnecting);
            Messenger.Default.Register<string>(this, "SetPlayerName", SetPlayerName);
            Messenger.Default.Register<string>(this, "SetOpponentName", SetOpponentName);
            Messenger.Default.Register<bool>(this, "SetPlayersOrder", SetPlayersOrder);
            Messenger.Default.Register<bool>(this, "EndMatch", EndMatch);
            Messenger.Default.Register<bool>(this, "FlagCaptured", SetWinner);
            Messenger.Default.Register<int>(this, "HardReset", x => HardReset());

            HardReset();
        }

        private void HardReset()
        {
            this.IsConnecting = true;
            this.IsPlaying = false;
        }

        private void StartNewMatch()
        {
            this.IsConnecting = false;
            this.IsPlaying = true;
        }

        private void SetIsConnecting(bool isConnecting)
        {
            this.IsConnecting = isConnecting;
        }

        private void SetPlayerName(string name)
        {
            SelfPlayer.Instance.Name = name;
        }

        private void SetOpponentName(string name)
        {
            Opponent.Instance.Name = name;
        }

        private void SetPlayersOrder(bool isPlayer2)
        {
            SelfPlayer.Instance.IsPlayer2 = isPlayer2;
            Opponent.Instance.IsPlayer2 = !isPlayer2;
        }

        private void GiveUpMethod()
        {
            this.IsPlaying = false;

            Messenger.Default.Send("GiveUp", "EndMatchResult");

            GameMaster.Client.CancelMatch();
        }
        private void SetWinner(bool IsWinner)
        {
            this.IsPlaying = false;
            string finResult = IsWinner ? "Victory" : "Defeat";
            Messenger.Default.Send(finResult, "EndMatchResult");
        }

        private void EndMatch(bool isWorthPoints)
        {
            if (this.IsPlaying)
            {
                this.IsPlaying = false;

                string finResult = isWorthPoints ? "LeftWin" : "Cancelled";
                Messenger.Default.Send(finResult, "EndMatchResult");
            }

            Messenger.Default.Send(0, "LoadFin");
        }
    }
}

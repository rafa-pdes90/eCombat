using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;
using eCombat.View;

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

        /// <summary>
        /// The <see cref="PlayerName" /> property's name.
        /// </summary>
        public const string PlayerNamePropertyName = "PlayerName";

        private string _playerName = "";

        /// <summary>
        /// Sets and gets the PlayerName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PlayerName
        {
            get => _playerName;
            set => Set(() => PlayerName, ref _playerName, value);
        }

        /// <summary>
        /// The <see cref="IsOpponentTurn" /> property's name.
        /// </summary>
        public const string IsOpponentTurnPropertyName = "IsOpponentTurn";

        private bool _isOpponentTurn;

        /// <summary>
        /// Sets and gets the IsOpponentTurn property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOpponentTurn
        {
            get => _isOpponentTurn;
            set => Set(() => IsOpponentTurn, ref _isOpponentTurn, value);
        }
        
        public ObservableCollection<BoardPiece> OpponentList { get; set; }
        public ObservableCollection<BoardPiece> UnitList { get; set; }

        public ICommand DesistirPartidaCommand { get; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<bool>(this, "SetIsConnecting", SetIsConnecting);
            Messenger.Default.Register<string>(this, "PlayerName", SetPlayerName);
            Messenger.Default.Register<bool>(this, "SetPlayersColors", SetPlayersColors);

            DesistirPartidaCommand = new RelayCommand(DesistirPartidaMethod);
        }

        private void SetIsConnecting(bool isConnecting)
        {
            this.IsConnecting = isConnecting;
        }

        private void SetPlayerName(string name)
        {
            this.PlayerName = name;
        }

        private void SetPlayersColors(bool isPlayer2)
        {
            SelfPlayer.Instance.IsPlayer2 = isPlayer2;
            Opponent.Instance.IsPlayer2 = !isPlayer2;
        }

        private void DesistirPartidaMethod()
        {
            this.IsPlaying = false;
            GameMaster.Client.CancelMatch();

            Task.Run(() => Messenger.Default.Send(0, "ResetAll"));

            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.LoadDialogWindow(new Fin("You gave up!?")));
        }
    }
}

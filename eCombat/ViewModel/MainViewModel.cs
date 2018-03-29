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
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random _local;

        public static Random ThisThreadsRandom =>
            _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

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

        /// <summary>
        /// The <see cref="PlayerColor" /> property's name.
        /// </summary>
        public const string PlayerColorPropertyName = "PlayerColor";

        private SolidColorBrush _playerColor;

        /// <summary>
        /// Sets and gets the PlayerColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush PlayerColor
        {
            get => _playerColor;
            set => Set(() => PlayerColor, ref _playerColor, value);
        }

        /// <summary>
        /// The <see cref="OpponentColor" /> property's name.
        /// </summary>
        public const string OpponentColorPropertyName = "OpponentColor";

        private SolidColorBrush _opponentColor;

        /// <summary>
        /// Sets and gets the OpponentColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SolidColorBrush OpponentColor
        {
            get => _opponentColor;
            set => Set(() => OpponentColor, ref _opponentColor, value);
        }
        
        public List<BoardPiece> EnemyList { get; set; }
        public List<BoardPiece> UnitList { get; set; }
        public ObservableCollection<string> LogList { get; } = new ObservableCollection<string>();

        public ICommand DesistirPartidaCommand { get; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<bool>(this, "SetIsConnecting", SetIsConnecting);
            Messenger.Default.Register<string>(this, "PlayerName", SetPlayerName);
            Messenger.Default.Register<bool>(this, "EvalPlayersColors", EvalPlayersColors);
            Messenger.Default.Register<bool>(this, "EvalMatchTurn", EvalMatchTurn);
            Messenger.Default.Register<string>(this, "LogIn", LogIn);

            DesistirPartidaCommand = new RelayCommand(DesistirPartidaMethod);

            this.EnemyList = Army.GetEnemyList();
            this.UnitList = Army.GetUnitlist();
            this.UnitList.Shuffle();
        }

        private void LogIn(string logEntry)
        {
            Application.Current.Dispatcher.Invoke(() =>
                this.LogList.Add(logEntry));
        }

        private void SetIsConnecting(bool isConnecting)
        {
            this.IsConnecting = isConnecting;
        }

        private void SetPlayerName(string name)
        {
            this.PlayerName = name;
        }

        private void EvalPlayersColors(bool player2Color)
        {
            if (player2Color)
            {
                this.PlayerColor = Brushes.CornflowerBlue;
                this.OpponentColor = Brushes.Orange;
            }
            else
            {
                this.PlayerColor = Brushes.Orange;
                this.OpponentColor = Brushes.CornflowerBlue;
            }
        }

        private void EvalMatchTurn(bool isOpponentTurn)
        {
            this.IsOpponentTurn = isOpponentTurn;

            Messenger.Default.Send(isOpponentTurn ? "It's the opponent's turn!" : "It's your turn!", "LogIn");
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

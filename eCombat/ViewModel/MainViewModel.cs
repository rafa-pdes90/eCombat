using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using eCombat.Model;

namespace eCombat.ViewModel
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
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
            Messenger.Default.Register<string>(this, "PlayerName", SetPlayerName);
            Messenger.Default.Register<bool>(this, "SetPlayersColors", SetPlayersColors);
            Messenger.Default.Register<bool>(this, "EvalMatchTurn", EvalMatchTurn);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);

            DesistirPartidaCommand = new RelayCommand(DesistirPartidaMethod);

            this.EnemyList = Army.GetEnemyList();
            this.UnitList = Army.GetUnitlist();
            this.UnitList.Shuffle();
        }

        private void SetPlayerName(string name)
        {
            this.PlayerName = name;
        }

        private void SetPlayersColors(bool player2Color)
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
        }

        private void DesistirPartidaMethod()
        {
            //NetMsg.NetMsgSend("g bye");
            NetConnViewModel netConn = ServiceLocator.Current.GetInstance<NetConnViewModel>();
            //netConn.Handler.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            Messenger.Default.Send("Você desistiu!?", "SetEndMatchMessage");

            Application.Current.Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow)?.CallDesistir());
        }

        private void ResetAll()
        {
        }

        private void NetConnLost(NotificationMessage notificationMsg)
        {
            Messenger.Default.Send("O outro jogador desistiu!", "SetEndMatchMessage");
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.CallDesistir());
            ResetAll();
        }
    }
}

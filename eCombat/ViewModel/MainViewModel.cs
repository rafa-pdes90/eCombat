using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using eCombat.Model;
using eCombat.View;

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







        private string _mensagemFinal;
        public string MensagemFinal
        {
            get => _mensagemFinal;
            set => Set(() => MensagemFinal, ref _mensagemFinal, value);
        }

        private string _sendTextContent = string.Empty;
        public string SendTextContent
        {
            get => _sendTextContent;
            set => Set(() => this.SendTextContent, ref _sendTextContent, value);
        }

        public string FeedbackReceived { get; set; }
        public List<BoardPiece> EnemyList { get; set; }
        public List<BoardPiece> UnitList { get; set; }

        public ICommand SendButtonCommand { get; }
        public ICommand DesistirPartidaCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ExitCommand { get; }

        public ObservableCollection<string> LogList { get; } = new ObservableCollection<string>();
        //private NetMsgViewModel NetMsg { get; } = ServiceLocator.Current.GetInstance<NetMsgViewModel>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<string>(this, "PlayerName", SetPlayerName);
            Messenger.Default.Register<bool>(this, "IsPlayer1", SetGameTurn);

            SendButtonCommand = new RelayCommand(SendButtonMethod);
            DesistirPartidaCommand = new RelayCommand(DesistirPartidaMethod);
            ResetCommand = new RelayCommand(ResetMethod);
            ExitCommand = new RelayCommand(ExitMethod);
            Messenger.Default.Register<GenericMessage<bool>>(this, "PlayerIsClient_Set", PlayerIsClientSet);
            Messenger.Default.Register<GenericMessage<NetConnViewModel>>(this, "NetConn_Set", NetConnSet);
            Messenger.Default.Register<NotificationMessage>(this, "NetConn_Lost", NetConnLost);
            Messenger.Default.Register<NotificationMessage>(this, "Move_In", MoveIn);
            Messenger.Default.Register<NotificationMessage>(this, "Feedback_In", FeedbackIn);
            this.EnemyList = Army.GetEnemyList();
            this.UnitList = Army.GetUnitlist();
            this.UnitList.Shuffle();
        }

        private void SetPlayerName(string name)
        {
            this.PlayerName = name;
        }

        private void SetGameTurn(bool isSelfTurn)
        {
            this.IsOpponentTurn = !isSelfTurn;

            if (isSelfTurn)
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

        private void MoveIn(NotificationMessage notificationMessage)
        {
            string[] coords = notificationMessage.Notification.Split();
            int origemY = int.Parse(coords[0]);
            int origemX = int.Parse(coords[1]);
            int destinoY = int.Parse(coords[2]);
            int destinoX = int.Parse(coords[3]);
            string powerLevel = coords[4];
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.MoveTheEnemy(origemY, origemX, destinoY, destinoX, powerLevel));

            this.IsOpponentTurn = false;
        }

        private void FeedbackIn(NotificationMessage notificationMessage)
        {
            this.FeedbackReceived = notificationMessage.Notification;
        }

        private void PlayerIsClientSet(GenericMessage<bool> genericMessage)
        {
            IsOpponentTurn = !genericMessage.Content;
        }

        private void NetConnSet(GenericMessage<NetConnViewModel> genericMessage)
        {
            Application.Current.Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow)?.StartNewMatch());
        }

        private void SendButtonMethod()
        {
            if (this.SendTextContent == "") return;
            Messenger.Default.Send(new NotificationMessage(this.SendTextContent), "Chat_Async");
            this.SendTextContent = string.Empty;
            ((MainWindow)Application.Current.MainWindow)?.ChatScrollToEnd();
        }

        public void FinishTurn(int origemY, int origemX, int destinoY, int destinoX, string powerLevel)
        {
            string moveMsg = "m " + origemY + " " + origemX + " " + destinoY + " " + destinoX + " " + powerLevel;
            //NetMsg.NetMsgAsync(moveMsg);
        }

        public void SendDefenderFeedback(string powerLevel)
        {
            string fbMsg = "f " + powerLevel;
            //NetMsg.NetMsgSend(fbMsg);
        }

        private void DesistirPartidaMethod()
        {
            //NetMsg.NetMsgSend("g bye");
            NetConnViewModel netConn = ServiceLocator.Current.GetInstance<NetConnViewModel>();
            netConn.Handler.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            this.MensagemFinal = "Voc� desistiu!?";
            Application.Current.Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow)?.CallDesistir());
        }

        private void ResetMethod()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void ExitMethod()
        {
            Application.Current.Shutdown();
        }

        private void ResetAll()
        {
            Application.Current.Dispatcher.Invoke(() => this.SendTextContent = string.Empty);
        }

        private void NetConnLost(NotificationMessage notificationMsg)
        {
            ResetAll();
        }
    }
}

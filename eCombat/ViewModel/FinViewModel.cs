using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using eCombat.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;

namespace eCombat.ViewModel
{
    public class FinViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="EndMatchMessage" /> property's name.
        /// </summary>
        public const string EndMatchMessagePropertyName = "EndMatchMessage";

        private string _endMatchMessage = "Fin";

        /// <summary>
        /// Sets and gets the EndMatchMessage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string EndMatchMessage
        {
            get => _endMatchMessage;
            set => Set(() => EndMatchMessage, ref _endMatchMessage, value);
        }

        public ICommand ResetCommand { get; }
        public ICommand ExitCommand { get; }

        public FinViewModel()
        {
            ResetCommand = new RelayCommand(ResetMethod);
            ExitCommand = new RelayCommand(ExitMethod);

            Messenger.Default.Register<string>(this, "EndMatchResult", SetEndMatchMessage);
        }

        private static void ResetMethod()
        {
            Messenger.Default.Send(0, "HardReset");
            Messenger.Default.Send(0, "SoftReset");

            Messenger.Default.Send(0, "FinClose");

            Task.Run(() => Messenger.Default.Send(0, "RunRequestOrCancelCommand"));

            Messenger.Default.Send(0, "OpenConnection");
        }

        private static void ExitMethod()
        {
            Messenger.Default.Send(0, "FinishGame");
        }

        private void SetEndMatchMessage(string result)
        {
            switch (result)
            {
                case "Victory":
                    this.EndMatchMessage = "You win! Congratulations!";
                    break;
                case "Defeat":
                    this.EndMatchMessage = "You lose! Better luck next time!";
                    break;
                case "LeftWin":
                    this.EndMatchMessage = "The opponent surrendered! You win!";
                    break;
                case "Cancelled":
                    this.EndMatchMessage = "The match has been cancelled.";
                    break;
                case "GiveUp":
                    this.EndMatchMessage = "You gave up!? What a pity!";
                    break;
                default:
                    this.EndMatchMessage = "Fin";
                    break;
            }
        }
    }
}

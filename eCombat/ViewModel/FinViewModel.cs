using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace eCombat.ViewModel
{
    public class FinViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="EndMatchMessage" /> property's name.
        /// </summary>
        public const string EndMatchMessagePropertyName = "EndMatchMessage";

        private string _endMatchMessage;

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

            Messenger.Default.Register<string>(this, "SetEndMatchMessage", SetEndMatchMessage);
        }

        private void SetEndMatchMessage(string message)
        {
            this.EndMatchMessage = message;
        }

        private static void ResetMethod()
        {
            //TODO
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private static void ExitMethod()
        {
            //TODO
            Application.Current.Shutdown();
        }
    }
}
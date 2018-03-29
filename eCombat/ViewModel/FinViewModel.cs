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
        }

        private static void ResetMethod()
        {
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.DialogWindow.Close());

            Task.Run(() => Messenger.Default.Send(0, "RunRequestOrCancelCommand"));

            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.LoadDialogWindow(new ConnectionWindow()));
        }

        private static void ExitMethod()
        {
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.DialogWindow.Close());
        }
    }
}

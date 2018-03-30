using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace eCombat.ViewModel
{
    public class LogViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="LogList" /> property's name.
        /// </summary>
        public const string LogListPropertyName = "LogList";

        private ObservableCollection<string> _logList;

        /// <summary>
        /// Sets and gets the LogList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> LogList
        {
            get => this._logList;
            set => Set(() => this.LogList, ref this._logList, value);
        }


        public LogViewModel()
        {
            Messenger.Default.Register<string>(this, "NewLogEntry", AddToLogList);
            Messenger.Default.Register<char>(this, "SoftReset", x => SoftReset());

            SoftReset();
        }

        private void SoftReset()
        {
            this.LogList = new ObservableCollection<string>();
        }

        private void AddToLogList(string logEntry)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.LogList.Add(logEntry));
        }
    }
}

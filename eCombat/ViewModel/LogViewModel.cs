using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace eCombat.ViewModel
{
    public class LogViewModel : ViewModelBase
    {
        private ObservableCollection<string> _logList;

        public ObservableCollection<string> LogList
        {
            get => this._logList;
            set => Set(() => this.LogList, ref this._logList, value);
        }

        public LogViewModel()
        {
            Messenger.Default.Register<string>(this, "NewLogEntry", AddToLogList);
            Messenger.Default.Register<int>(this, "SoftReset", x => SoftReset());

            this.LogList = new ObservableCollection<string>();

            SoftReset();
        }

        private void SoftReset()
        {
            this.LogList.Clear();
        }

        private void AddToLogList(string logEntry)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.LogList.Add(logEntry));
        }
    }
}

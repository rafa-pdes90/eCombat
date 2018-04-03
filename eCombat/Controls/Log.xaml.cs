using System.Linq;
using System.Windows.Controls;
using eCombat.ViewModel;

namespace eCombat.Controls
{
    /// <summary>
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : UserControl
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        public LogViewModel Vm => (LogViewModel)this.DataContext;

        public Log()
        {
            InitializeComponent();
        }

        private void LogScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            string newLogEntry = this.Vm.LogList.LastOrDefault();

            if (newLogEntry == null) return;

            double pseudoEnd = this.LogScrollViewer.ExtentHeight;
            this.LogScrollViewer.ScrollToVerticalOffset(pseudoEnd);
        }
    }
}

using System.Linq;
using System.Windows.Controls;
using eCombat.ViewModel;

namespace eCombat.Controls
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        public ChatViewModel Vm => (ChatViewModel)this.DataContext;

        public Chat()
        {
            InitializeComponent();
        }

        private void ChatScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Math.Abs(e.VerticalChange) > 0) return;
            
            ChatMsg newChatMessage = this.Vm.ChatMsgList.LastOrDefault();

            if (newChatMessage == null || !newChatMessage.IsSelfMessage) return;

            double pseudoEnd = this.ChatScrollViewer.ExtentHeight;
            this.ChatScrollViewer.ScrollToVerticalOffset(pseudoEnd);
        }
    }
}

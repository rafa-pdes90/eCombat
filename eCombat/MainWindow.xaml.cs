using System;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using MahApps.Metro.Controls;
using eCombat.Model;
using eCombat.View;
using eCombat.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace eCombat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        private MainViewModel Vm => (MainViewModel)this.DataContext;

        private bool KeepOn { get; set; }
        private MetroWindow DialogWindow { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            this.KeepOn = false;
            this.DialogWindow = null;

            Messenger.Default.Register<int>(this, "StartNewMatch", token => StartNewMatch());
            Messenger.Default.Register<int>(this, "LoadFin", x => LoadFin());
            Messenger.Default.Register<int>(this, "FinClose", x => FinClose());
            Messenger.Default.Register<int>(this, "OpenConnection", x => OpenConnection());
            Messenger.Default.Register<int>(this, "FinishGame", x => FinishGame());
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Vm.IsConnecting)
            {
                this.Vm.IsPlaying = false;
                GameMaster.Client.CancelMatch();
            }

            GameMaster.Client.LeaveGame();

            ViewModelLocator.Cleanup();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            LoadDialogWindow(new ConnectionWindow());

            if (!this.KeepOn) return;

            this.KeepOn = false;
        }

        public void LoadDialogWindow(MetroWindow dialog)
        {
            this.DialogWindow = dialog;

            this.Effect = new BlurEffect();

            dialog.Owner = this;
            dialog.ShowDialog();

            if (this.KeepOn)
            {
                this.Effect = null;
            }
            else
            {
                this.Close();
            }
        }

        public void StartNewMatch()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.KeepOn = true;
                this.DialogWindow.Close();
            });
        }

        private void LoadFin()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                LoadDialogWindow(new Fin());
            });
        }

        private void FinClose()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.KeepOn = true;
                this.DialogWindow.Close();
            });
        }

        private void OpenConnection()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                LoadDialogWindow(new ConnectionWindow());
            });
        }

        private void FinishGame()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                this.DialogWindow.Close();
            });
        }
    }
}

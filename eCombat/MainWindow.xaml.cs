using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using eCombat.Model;
using eCombat.View;
using eCombat.ViewModel;
using GalaSoft.MvvmLight.Messaging;

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

        public MetroWindow DialogWindow { get; set; }

        private bool KeepOn { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            this.DialogWindow = null;
            this.KeepOn = false;

            Messenger.Default.Register<int>(this, "ResetAll", token => ResetAll());
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
            this.Vm.IsConnecting = false;
            this.Vm.IsPlaying = true;
            this.KeepOn = true;
            this.DialogWindow.Close();
        }

        public void EvalPrematureMatchEnd(bool isWorthPoints)
        {
            if (!this.Vm.IsPlaying) return;

            this.Vm.IsPlaying = false;

            Task.Run(() => Messenger.Default.Send(0, "ResetAll"));

            LoadDialogWindow(isWorthPoints
                ? new Fin("The opponent has given up! You win!")
                : new Fin("The match has been cancelled."));
        }

        private void ResetAll()
        {
            this.KeepOn = false;
        }
    }
}

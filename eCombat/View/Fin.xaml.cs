using CommonServiceLocator;
using eCombat.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MahApps.Metro.Controls;

namespace eCombat.View
{
    /// <summary>
    /// Interaction logic for Fin.xaml
    /// </summary>
    public partial class Fin : MetroWindow
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        public FinViewModel Vm => (FinViewModel)this.DataContext;

        public Fin()
        {
            InitializeComponent();
        }
    }
}

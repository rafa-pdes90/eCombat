using CommonServiceLocator;
using eCombat.ViewModel;
using MahApps.Metro.Controls;

namespace eCombat.View
{
    /// <summary>
    /// Interaction logic for Fin.xaml
    /// </summary>
    public partial class Fin : MetroWindow
    {
        private static FinViewModel Vm { get; }

        static Fin()
        {
            Vm = ServiceLocator.Current.GetInstance<FinViewModel>();
        }

        public Fin(string headerMessage)
        {
            InitializeComponent();

            Vm.EndMatchMessage = headerMessage;
        }
    }
}

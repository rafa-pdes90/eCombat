/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:eCombat"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace eCombat.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<NetConnViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PlayBoardViewModel>();
            SimpleIoc.Default.Register<ChatViewModel>();
            SimpleIoc.Default.Register<LogViewModel>();
            SimpleIoc.Default.Register<FinViewModel>(true);
        }

        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<NetConnViewModel>();
            SimpleIoc.Default.Unregister<MainViewModel>();
            SimpleIoc.Default.Unregister<PlayBoardViewModel>();
            SimpleIoc.Default.Unregister<ChatViewModel>();
            SimpleIoc.Default.Unregister<LogViewModel>();
            SimpleIoc.Default.Unregister<FinViewModel>();
        }

        /// <summary>
        /// Gets the NetConnViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public NetConnViewModel NetConnVm
        {
            get { return ServiceLocator.Current.GetInstance<NetConnViewModel>(); }
        }

        /// <summary>
        /// Gets the MainViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel MainVm
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        /// <summary>
        /// Gets the PlayBoardViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PlayBoardViewModel PlayBoardVm
        {
            get { return ServiceLocator.Current.GetInstance<PlayBoardViewModel>(); }
        }

        /// <summary>
        /// Gets the ChatViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ChatViewModel ChatVm
        {
            get { return ServiceLocator.Current.GetInstance<ChatViewModel>(); }
        }

        /// <summary>
        /// Gets the LogViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public LogViewModel LogVm
        {
            get { return ServiceLocator.Current.GetInstance<LogViewModel>(); }
        }

        /// <summary>
        /// Gets the FinViewModel property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public FinViewModel FinVm
        {
            get { return ServiceLocator.Current.GetInstance<FinViewModel>(); }
        }
    }
}

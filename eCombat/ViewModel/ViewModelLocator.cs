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
using GalaSoft.MvvmLight;
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
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<NetConnViewModel>();
            SimpleIoc.Default.Register<ClientConnViewModel>();
            SimpleIoc.Default.Register<ServerConnViewModel>();
            SimpleIoc.Default.Register<NetMsgViewModel>();
            SimpleIoc.Default.Register<ChatMsgViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public ClientConnViewModel ClientConn
        {
            get { return ServiceLocator.Current.GetInstance<ClientConnViewModel>(); }
        }

        public ServerConnViewModel ServerConn
        {
            get { return ServiceLocator.Current.GetInstance<ServerConnViewModel>(); }
        }

        public ChatMsgViewModel ChatMsg
        {
            get { return ServiceLocator.Current.GetInstance<ChatMsgViewModel>(); }
        }

        public static void Cleanup()
        {
            SimpleIoc.Default.Unregister<MainViewModel>();
            SimpleIoc.Default.Unregister<NetConnViewModel>();
            SimpleIoc.Default.Unregister<ClientConnViewModel>();
            SimpleIoc.Default.Unregister<ServerConnViewModel>();
            SimpleIoc.Default.Unregister<NetMsgViewModel>();
            SimpleIoc.Default.Unregister<ChatMsgViewModel>();
        }
    }
}
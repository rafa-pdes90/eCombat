using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using eCombat.Model;
using eCombat.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GameServer;

namespace eCombat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        private NetConnViewModel NetConn { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var splashScreen = new SplashScreen("/Resources/SplashScreen.png");
            splashScreen.Show(false, true);

            var dummy0 = new ViewModelLocator();
            this.NetConn = ServiceLocator.Current.GetInstance<NetConnViewModel>();
            var dummy1 = ServiceLocator.Current.GetInstance<FinViewModel>();

            Task.Run(() => ConnectToGameMaster());

            // base.OnStartup(e); // BSOD, DO NOT USE! //

            // Create the startup window
            var wnd = new MainWindow();

            splashScreen.Close(TimeSpan.FromMilliseconds(2000));

            // Show the window
            wnd.Show();
        }

        private void ConnectToGameMaster()
        {
            while (true)
            {
                var result = MessageBoxResult.None;

                try
                {
                    GameMasterSvcClient dummy = GameMaster.Client;
                    break;
                }
                catch (FaultException<GameMasterSvcFault> f)
                {
                    string fMsg = "Error on game server while " + f.Detail.Operation +
                                  ". Reason: " + f.Detail.Reason + "\n\r\n\rRetry?";
                    result = MessageBox.Show(fMsg, "Service error",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                }
                catch (Exception e)
                {
                    switch (e)
                    {
                        case EndpointNotFoundException _:
                            result = MessageBox.Show(
                                "The Proxy Server couldn't be reached.\n\r\n\rRetry?",
                                "Connection error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                            break;
                        case CommunicationObjectAbortedException _:
                            result = MessageBox.Show(
                                "The Game Server couldn't be reached.\n\r\n\rRetry?",
                                "Connection refused", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                            break;
                        case CommunicationException _:
                            result = MessageBox.Show(
                                "There was an error registering to the Proxy Server.\n\r\n\rRetry?",
                                "Connection refused", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            break;
                        default:
                            this.Dispatcher.Invoke(() => ShowUnknownException(e));
                            break;
                    }
                }

                if (result == MessageBoxResult.None) break;

                if (result == MessageBoxResult.OK) continue;

                this.Dispatcher.Invoke(this.Shutdown);
                return;
            }

            this.NetConn.RequestOrCancelMatchIsEnabled = true;
            this.NetConn.RequestOrCancelMatchLoadingVisibility = Visibility.Collapsed;
            this.NetConn.RequestOrCancelMatchContent = "Ready to Play!";
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            switch (e.Exception)
            {
                case CommunicationException _:
                    GameMaster.Client.Abort();
                    //TODO Auto retry connect to server again, with an option to cancel
                    ConnectToGameMaster();
                    this.NetConn.RequestOrCancelMatchCommand.Execute(null);
                    break;
                default:
                    ShowUnknownException(e.Exception);
                    break;
            }

            e.Handled = true;
        }

        private void ShowUnknownException(Exception e)
        {
            MessageBox.Show("An unhandled exception just occurred:\n\r\n\r" + e,
                "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown();
        }
    }
}

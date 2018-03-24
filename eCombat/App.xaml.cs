using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using eCombat.Model;
using GameServer;

namespace eCombat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var splashScreen = new SplashScreen("/Resources/SplashScreen.png");
            splashScreen.Show(false, false);

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
                try
                {
                    GameMasterSvcClient dummy = GameMaster.Client;
                    break;
                }
                catch (Exception e)
                {
                    var result = MessageBoxResult.None;

                    switch (e)
                    {
                        case FaultException<GameMasterSvcFault> f:
                            Console.WriteLine(@"GameMasterSvcFault while " + f.Detail.Operation + @". Reason: " + f.Detail.Reason);
                            GameMaster.Client.Abort();
                            return;
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

                    if (result == MessageBoxResult.OK) continue;

                    if (result == MessageBoxResult.None) break;

                    this.Dispatcher.Invoke(this.Shutdown);
                    return;
                }
            }

            //TODO Turn connection window button on
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            switch (e)
            {
                default:
                    ShowUnknownException(e.Exception);
                    break;
            }

            e.Handled = true;
        }

        private void ShowUnknownException(Exception e)
        {
            string errorMsg = "";
            string[] exceptionMsg = e.ToString().Split(new[] { ':', ')' },
                StringSplitOptions.RemoveEmptyEntries);

            if (exceptionMsg.Length == 1)
            {
                errorMsg += ":\n\r" + exceptionMsg[0];
            }
            else
            {
                errorMsg += ":\n\r" + exceptionMsg[1];

                if (exceptionMsg.Length > 2)
                {
                    errorMsg += ":\n\r" + exceptionMsg[2] + ")";
                }
            }

            MessageBox.Show("An unhandled exception just occurred" + errorMsg + ".",
                "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown();
        }
    }
}

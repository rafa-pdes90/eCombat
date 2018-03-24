using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace eCombat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var splashScreen = new SplashScreen("/Resources/SplashScreen.png");
            splashScreen.Show(false, true);

            // base.OnStartup(e); // BSOD, DO NOT USE! //

            // Create the startup window
            var wnd = new MainWindow();
            
            splashScreen.Close(TimeSpan.FromMilliseconds(2000));

            // Show the window
            wnd.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            switch (e)
            {
                default:
                    string errorMsg = "";
                    string[] exceptionMsg = e.Exception.ToString().Split(new char[] {':', '\n', '\r'},
                        StringSplitOptions.RemoveEmptyEntries);

                    int i = 1;
                    while (i < exceptionMsg.Length && i <= 3)
                    {
                        switch (i)
                        {
                            case 1:
                                errorMsg += "\n\r" + exceptionMsg[i];
                                break;
                            case 2:
                                if (e.Exception.InnerException != null)
                                {
                                    errorMsg += "\n\r\n\r" + exceptionMsg[i];
                                }
                                break;
                            case 3:
                                int endTrim = exceptionMsg[i].Length - 8;
                                errorMsg += "\n\r\n\r" + exceptionMsg[i].Substring(3, endTrim);
                                break;
                        }

                        i++;
                    }

                    MessageBox.Show("An unhandled exception just occurred: " + errorMsg, 
                        "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Shutdown();
                    break;
            }
            e.Handled = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace eCombat
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CombateSvc" in both code and config file together.
    public class CombateSvc : ICombateSvc
    {
        public void StartMatch(string opponentName, string opponentId, bool isPlayer1)
        {
            Console.WriteLine(@"Starting game against " + opponentId + @" - " + opponentName);
            Console.WriteLine(isPlayer1 ? @"It's your turn!" : @"It' the opponent turn!");
            Console.WriteLine();

            Messenger.Default.Send(opponentName, "OpponentName");
            Messenger.Default.Send(opponentId, "OpponentId");
            Messenger.Default.Send(isPlayer1, "IsPlayer1");

            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.StartNewMatch());
        }

        public void CancelMatch() { }
    }
}

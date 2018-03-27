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
        public void StartMatch(string opponentName, string opponentId, bool isOpponentTurn)
        {
            Messenger.Default.Send(false, "RequestOrCancelState");

            Console.WriteLine(@"Starting game against " + opponentId + @" - " + opponentName);
            Console.WriteLine(isOpponentTurn ? @"It' the opponent turn!" : @"It's your turn!");
            Console.WriteLine();

            Messenger.Default.Send(opponentName, "OpponentName");
            Messenger.Default.Send(opponentId, "OpponentId");
            Messenger.Default.Send(isOpponentTurn, "SetPlayersColors");
            Messenger.Default.Send(isOpponentTurn, "EvalMatchTurn");

            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.StartNewMatch());
        }

        public void CancelMatch(bool isWorthPoints)
        {
            //TODO
        }

        public void MoveBoardPiece(int srcX, int srcY, int destX, int destY, bool isOpponentTurn)
        {
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.MoveBoardPiece(srcX, srcY, destX, destY));

            Messenger.Default.Send(isOpponentTurn, "EvalMatchTurn");
        }

        public void AttackBoardPiece(int srcX, int srcY, int destX, int destY,
            string attackerPowerLevel, string defenderPowerLevel, bool isOpponentTurn)
        {
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.AttackBoardPiece(
                    srcX, srcY, destX, destY, attackerPowerLevel, defenderPowerLevel));

            Messenger.Default.Send(isOpponentTurn, "EvalMatchTurn");
        }

        public string ShowPowerLevel(int srcX, int srcY)
        {
            string powerLevel = null;

            Application.Current.Dispatcher.Invoke(() =>
                powerLevel = ((MainWindow) Application.Current.MainWindow)?.ShowPowerLevel(srcX, srcY));

            return powerLevel;
        }

        public void WriteMessageToChat(ChatMsg chatMessage, bool isSelfMessage)
        {
            chatMessage.IsSelfMessage = isSelfMessage;
            Messenger.Default.Send(chatMessage, "Chat_In");
            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.ChatScrollToEnd());
        }
    }
}

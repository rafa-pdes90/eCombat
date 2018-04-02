using System.Threading.Tasks;
using System.Windows;
using eCombat.Model;
using GalaSoft.MvvmLight.Messaging;

namespace eCombat
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CombateSvc" in both code and config file together.
    public class CombateSvc : ICombateSvc
    {
        public void StartMatch(string opponentName, string opponentId, bool isOpponentTurn)
        {
            Messenger.Default.Send("A game against " + opponentName + " has started!", "NewLogEntry");
            Messenger.Default.Send(false, "ChangeRequestOrCancelState");
            Messenger.Default.Send(opponentName, "OpponentName");
            Messenger.Default.Send(opponentId, "OpponentId");
            Messenger.Default.Send(isOpponentTurn, "SetPlayersColors");
            Messenger.Default.Send(0, "LoadBoard");
            Messenger.Default.Send(isOpponentTurn, "SetMatchTurn");

            Application.Current.Dispatcher.Invoke(() =>
                ((MainWindow)Application.Current.MainWindow)?.StartNewMatch());
        }

        public void EndMatch(bool isWorthPoints)
        {
            Task.Run(() =>
                Application.Current.Dispatcher.Invoke(() =>
                    ((MainWindow)Application.Current.MainWindow)?.EvalPrematureMatchEnd(isWorthPoints)));
        }

        public void MoveBoardPiece(int srcX, int srcY, int destX, int destY, bool isOpponentTurn)
        {
            Messenger.Default.Send(new[] {srcX, srcY, destX, destY}, "MoveBoardPiece");

            Messenger.Default.Send(isOpponentTurn, "SetMatchTurn");
        }

        public void AttackBoardPiece(int srcX, int srcY, int destX, int destY,
            string attackerPowerLevel, string defenderPowerLevel, bool isOpponentTurn)
        {
            Messenger.Default.Send(new object[] {srcX, srcY, destX, destY, attackerPowerLevel, defenderPowerLevel},
                "AttackBoardPiece");

            Messenger.Default.Send(isOpponentTurn, "SetMatchTurn");
        }

        public string ShowPowerLevel(int srcX, int srcY)
        {
            return Board.Layout[srcX, srcY].PieceOnTop.PowerLevel;
        }

        public void WriteMessageToChat(ChatMsg chatMessage, bool isSelfMessage)
        {
            chatMessage.IsSelfMessage = isSelfMessage;

            Messenger.Default.Send(chatMessage, "NewChatMsg");
        }

        public void Ping() { }
    }
}

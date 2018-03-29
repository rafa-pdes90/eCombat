using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GameMasterSvc" in both code and config file together.
    public class GameMasterSvc : IGameMasterSvc
    {
        private Player SessionPlayer { get; set; }

        public void EnterGame(string clientId)
        {
            try
            {
                EndpointDiscoveryMetadata probedMetadata = GameMaster.Probe<ICombateSvc>(clientId);

                if (probedMetadata == null)
                {
                    var fault = new GameMasterSvcFault("probing by Id", "invalid Id.", "ITGM 01");
                    throw new FaultException<GameMasterSvcFault>(fault, "invalid Id");
                }

                this.SessionPlayer = new Player(this, clientId, probedMetadata);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TargetInvocationException _:
                        Console.WriteLine("Fault exception because of unreachable proxy");
                        var fault = new GameMasterSvcFault("probing by URI", "unable to connect to the proxy and query it.", "ITGM 00");
                        throw new FaultException<GameMasterSvcFault>(fault);
                    case FaultException _:
                        Console.WriteLine("Fault exception because of " + e.Message);
                        throw;
                    default:
                        Console.WriteLine(e);
                        break;
                }
                Console.WriteLine();
            }
        }

        public void LeaveGame()
        {
            this.SessionPlayer.EndGame();

            this.SessionPlayer = null;
        }

        public void FaceMatch(string displayName)
        {
            if (this.SessionPlayer.CurrentMatch != null) return;

            Task.Run(() =>
                this.SessionPlayer.SeekMatch(displayName));
        }

        public void CancelMatch()
        {
            if (this.SessionPlayer.CurrentMatch == null) return;

            Task.Run(() =>
                this.SessionPlayer.EndCurrentMatch());
        }

        public void MoveBoardPiece(int srcX, int srcY, int destX, int destY)
        {
            if (this.SessionPlayer.CurrentMatch == null) return;

            Task.Run(() =>
                this.SessionPlayer.RelayOriginalAndMirroredMove(srcX, srcY, destX, destY));
        }

        public void AttackBoardPiece(int srcX, int srcY, int destX, int destY, string attackerPowerLevel)
        {
            if (this.SessionPlayer.CurrentMatch == null) return;

            Task.Run(() => 
                this.SessionPlayer.RelayOriginalAndMirroredAttack(srcX, srcY, destX, destY, attackerPowerLevel));
        }

        public void WriteMessageToChat(string message)
        {
            if (this.SessionPlayer.CurrentMatch == null) return;

            Task.Run(() =>
                this.SessionPlayer.RelayTaggedMessage(message));
        }
    }
}

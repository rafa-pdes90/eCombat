using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GameMasterSvc" in both code and config file together.
    public class GameMasterSvc : IGameMasterSvc
    {
        public Player SessionPlayer { get; set; }

        public void MeetTheGameMaster(string clientId)
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

        public void IntroduceToGameMaster(string displayName)
        {
            Task.Run(() =>
                this.SessionPlayer.SeekMatch(displayName));
        }

        public void CancelMatch()
        {
            Task.Run(() =>
                this.SessionPlayer.FinishCurrentMatch());
        }

        public void MoveBoardPiece(int srcX, int srcY, int destX, int destY)
        {
            Task.Run(() =>
                this.SessionPlayer.MakeOriginalAndMirroredMove(srcX, srcY, destX, destY));
        }

        public void AttackBoardPiece(int srcX, int srcY, int destX, int destY,
            string attackerPowerLevel)
        {
            Task.Run(() => 
                this.SessionPlayer.MakeOriginalAndMirroredAttack(srcX, srcY, destX, destY, attackerPowerLevel));
        }

        public void WriteMessageToChat(string message)
        {

        }
    }
}

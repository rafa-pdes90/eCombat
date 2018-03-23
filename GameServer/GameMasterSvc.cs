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
                    var fault = new GMFault("probing by Id", "invalid Id.", "ITGM 01");
                    throw new FaultException<GMFault>(fault, "invalid Id");
                }

                this.SessionPlayer = new Player(this, clientId, probedMetadata);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TargetInvocationException _:
                        Console.WriteLine("Fault exception because of unreachable proxy");
                        var fault = new GMFault("probing by URI", "unable to connect to and query the proxy.", "ITGM 00");
                        throw new FaultException<GMFault>(fault);
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
            Task.Run(() => this.SessionPlayer.SeekMatch(displayName));
        }
    }
}

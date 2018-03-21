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
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GameMaster" in both code and config file together.
    public class GameMaster : IGameMaster
    {
        private static GameManager _theHouse;

        private string PlayerClientId { get; set; }
        private CombateSvcClient PlayerClient { get; set; }
        private GameInfo CurrentGame { get; set; }

        static GameMaster()
        {
            _theHouse = new GameManager();
        }

        public string IntroduceToGameMaster(Uri clientUri)
        {
            try
            {
                EndpointDiscoveryMetadata probedMetadata = GMHelper.ProbeByUri(clientUri);

                if (probedMetadata == null)
                {
                    var fault = new GMFault("probing by Uri", "invalid Uri.", "ITGM 01");
                    throw new FaultException<GMFault>(fault);
                }

                var testCriteria = new FindCriteria(typeof(ICombateSvc));
                if (testCriteria.IsMatch(probedMetadata))
                {
                    Task.Run(() =>
                        this.PlayerClient = GMHelper.NewClient(probedMetadata));

                    this.PlayerClientId = probedMetadata.Extensions.First(x => x.Name.LocalName == "Id").Value;
                    Task.Run(() =>
                    {
                        lock (_theHouse)
                        {
                             CurrentGame = GMHelper.NewGame(this, ref _theHouse);
                        }

                        if (CurrentGame == null) return;
                        CurrentGame.Player1.PlayerClient.DoWorkAsync();
                        CurrentGame.Player2.PlayerClient.DoWorkAsync();
                    });

                    return this.PlayerClientId;
                }
                else
                {
                    var fault = new GMFault("probing by Uri", "Uri belongs to another service type.", "ITGM 02");
                    throw new FaultException<GMFault>(fault);
                }
            }
            catch (TargetInvocationException)
            {
                var fault = new GMFault("probing by URI", "unable to connect to and query the proxy.", "ITGM 00");
                throw new FaultException<GMFault>(fault);
            }
        }

        public void DoWork()
        {
            Console.WriteLine(@"Testing GameServer");
            PlayerClient.Close();
        }
    }
}

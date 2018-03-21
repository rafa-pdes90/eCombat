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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GameMaster" in both code and config file together.
    public class GameMaster : IGameMaster
    {
        private static GameManager _theHouse;

        private static readonly Mutex Locker;

        private string DisplayName { get; set; }
        private string PlayerClientId { get; set; }
        private CombateSvcClient PlayerClient { get; set; }
        private GameInfo CurrentGame { get; set; }

        static GameMaster()
        {
            _theHouse = new GameManager();
            Locker = new Mutex();
        }

        public string IntroduceToGameMaster(Uri clientUri, string name)
        {
            try
            {
                EndpointDiscoveryMetadata probedMetadata = GMHelper.ProbeByUri(clientUri);

                if (probedMetadata == null)
                {
                    var fault = new GMFault("probing by Uri", "invalid Uri.", "ITGM 01");
                    throw new FaultException<GMFault>(fault, "invalid Uri");
                }

                var testCriteria = new FindCriteria(typeof(ICombateSvc));
                if (testCriteria.IsMatch(probedMetadata))
                {
                    this.PlayerClientId = probedMetadata.Extensions.First(x => x.Name.LocalName == "Id").Value;
                    Console.WriteLine("Player " + this.PlayerClientId + " has logged in");

                    Task.Run(async () =>
                    {
                        await InitGame(name, probedMetadata);
                    });

                    return this.PlayerClientId;
                }
                else
                {
                    var fault = new GMFault("probing by Uri", "Uri belongs to another service type.", "ITGM 02");
                    throw new FaultException<GMFault>(fault, "service mismatch");
                }
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
                return null;
            }
        }


        #region unserviced methods

        private async Task InitGame(string name, EndpointDiscoveryMetadata probedMetadata)
        {
            this.DisplayName = name;
            this.PlayerClient = GMHelper.NewClient(probedMetadata);

            Locker.WaitOne();
            while (_theHouse.WaitingPlayers.Count > 0)
            {
                GameMaster waitingPlayer = _theHouse.WaitingPlayers.Dequeue();
                try
                {
                    await Task.WhenAll(
                        waitingPlayer.PlayerClient.StartGameAsync(this.DisplayName, this.PlayerClientId, true),
                        PlayerClient.StartGameAsync(waitingPlayer.DisplayName, waitingPlayer.PlayerClientId, false));

                    lock (_theHouse)
                    {
                        CurrentGame = GMHelper.NewGame(waitingPlayer, this, ref _theHouse);
                    }
                    waitingPlayer.CurrentGame = CurrentGame;
                    Console.WriteLine("Game #" + CurrentGame.Id + " has started between players " +
                                      waitingPlayer.PlayerClientId + " and " + this.PlayerClientId);

                    Locker.ReleaseMutex();
                    return;
                }
                catch (EndpointNotFoundException)
                {
                    Console.WriteLine("Player " + waitingPlayer.PlayerClientId + " couldn't be reached");
                    waitingPlayer.PlayerClient.Abort();
                }
            }

            _theHouse.WaitingPlayers.Enqueue(this);
            CurrentGame = null;
            Locker.ReleaseMutex();
            Console.WriteLine("Player " + this.PlayerClientId + " is waiting for an opponent");
        }

        #endregion
    }
}

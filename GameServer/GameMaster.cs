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

        public string IntroduceToGameMaster(string clientId, string name)
        {
            try
            {
                EndpointDiscoveryMetadata probedMetadata = GMHelper.ProbeById<ICombateSvc>(clientId);

                if (probedMetadata == null)
                {
                    var fault = new GMFault("probing by Id", "invalid Id.", "ITGM 01");
                    throw new FaultException<GMFault>(fault, "invalid Id");
                }

                this.PlayerClientId = clientId;
                Console.WriteLine("Player " + name + "(" + clientId + ") has logged in");
                Console.WriteLine();

                Task.Run(async () =>
                {
                    await InitGame(name, probedMetadata);
                });

                return clientId;
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
                    Console.WriteLine();

                    Locker.ReleaseMutex();
                    return;
                }
                catch (EndpointNotFoundException)
                {
                    Console.WriteLine("Player " + waitingPlayer.PlayerClientId + " couldn't be reached");
                    Console.WriteLine();
                    waitingPlayer.PlayerClient.Abort();
                }
            }

            _theHouse.WaitingPlayers.Enqueue(this);
            CurrentGame = null;
            Locker.ReleaseMutex();
            Console.WriteLine("Player " + this.PlayerClientId + " is waiting for an opponent");
            Console.WriteLine();
        }

        #endregion
    }
}

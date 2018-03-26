using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    public sealed class Player
    {
        private static Queue<Player> WaitingPlayers { get; set; }

        public Semaphore Locker { get; private set; }

        private GameMasterSvc GameSession { get; set; }
        private string ClientId { get; set; }
        private ICombateSvcChannel Client { get; set; }
        private EndpointDiscoveryMetadata RemoteMetadata { get; set; }
        private string DisplayName { get; set; }
        private MatchInfo CurrentGame { get; set; }

        static Player()
        {
            WaitingPlayers = new Queue<Player>();
        }

        ~Player()
        {
            this.Locker.Close();
            this.GameSession = null;
            
            try
            {
                this.Client?.Close();
            }
            catch (Exception)
            {
                this.Client?.Abort();
            }

            //TODO CurrentGame
            CurrentGame = null;
        }

        public Player(GameMasterSvc gameSession, string clientId, EndpointDiscoveryMetadata playerMetadata)
        {
            this.Locker = new Semaphore(1, 1);
            this.GameSession = gameSession;
            this.ClientId = clientId;

            Locker.WaitOne();
            Task.Run(() => Init(playerMetadata));
        }

        private void Init(EndpointDiscoveryMetadata playerMetadata)
        {
            this.RemoteMetadata = playerMetadata;
            this.Client = null;
            this.DisplayName = null;
            this.CurrentGame = null;

            Console.WriteLine("Player " + "(" + this.ClientId + ") has logged in");
            Console.WriteLine();
            this.Locker.Release();
        }

        private void InvokeStartMatch(string opponentName, string opponentId, bool isPlayer1)
        {
            try
            {
                this.Client = GameMaster.NewClient(this.RemoteMetadata);
                this.Client.StartMatch(opponentName, opponentId, isPlayer1);
            }
            catch (EndpointNotFoundException)
            {
                Console.WriteLine("Player " + this.ClientId + " couldn't be reached");
                Console.WriteLine();

                this.GameSession.SessionPlayer = null;
                throw;
            }
        }

        private void InvokeCancelMatch()
        {
            try
            {
                this.Client.CancelMatch();
            }
            catch (EndpointNotFoundException)
            {
                Console.WriteLine("Player " + this.ClientId + " couldn't be reached");
                Console.WriteLine();

                this.GameSession.SessionPlayer = null;
                throw;
            }
        }

        /// <summary>
        /// Seek and starts a match or put player on the waiting list
        /// </summary>
        /// <exception cref="EndpointNotFoundException"></exception>
        /// <param name="displayName"></param>
        public void SeekMatch(string displayName)
        {
            if (this.CurrentGame != null) return;

            this.Locker.WaitOne();

            this.DisplayName = displayName;

            lock (WaitingPlayers)
            {
                while (WaitingPlayers.Count > 0)
                {
                    Player waitingPlayer = WaitingPlayers.Peek();

                    waitingPlayer.Locker.WaitOne();

                    try
                    {
                        waitingPlayer.InvokeStartMatch(this.DisplayName, this.ClientId, true);
                    }
                    catch (EndpointNotFoundException)
                    {
                        WaitingPlayers.Dequeue();
                        continue;
                    }

                    try
                    {
                        this.InvokeStartMatch(waitingPlayer.DisplayName, waitingPlayer.ClientId, false);
                    }
                    catch (EndpointNotFoundException)
                    {
                        try
                        {
                            waitingPlayer.InvokeCancelMatch();
                            waitingPlayer.Locker.Release();
                        }
                        catch (EndpointNotFoundException)
                        {
                            WaitingPlayers.Dequeue();
                        }

                        return;
                    }

                    WaitingPlayers.Dequeue();

                    this.CurrentGame = GameMaster.NewMatch(waitingPlayer, this);
                    this.Locker.Release();

                    waitingPlayer.CurrentGame = CurrentGame;
                    waitingPlayer.Locker.Release();

                    Console.WriteLine("Game #" + CurrentGame.Id + " has started between players " +
                                      waitingPlayer.ClientId + " and " + this.ClientId);
                    Console.WriteLine();
                    
                    return;
                }

                this.Locker.Release();

                WaitingPlayers.Enqueue(this);
            }

            Console.WriteLine("Player " + this.ClientId + " is waiting for an opponent");
            Console.WriteLine();
        }
    }
}

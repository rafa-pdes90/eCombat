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
        private MatchInfo CurrentMatch { get; set; }

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

            //TODO CurrentMatch
            CurrentMatch = null;
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
            this.DisplayName = "";
            this.CurrentMatch = null;

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

        private void InvokeCancelMatch(bool isWorthPoints)
        {
            try
            {
                this.Client.CancelMatch(isWorthPoints);
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
            if (this.CurrentMatch != null) return;

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
                            waitingPlayer.InvokeCancelMatch(false);
                        }
                        catch (EndpointNotFoundException)
                        {
                            WaitingPlayers.Dequeue();
                        }

                        waitingPlayer.Locker.Release();
                        return;
                    }

                    WaitingPlayers.Dequeue();

                    this.CurrentMatch = GameMaster.NewMatch(waitingPlayer, this);
                    this.Locker.Release();

                    waitingPlayer.CurrentMatch = CurrentMatch;
                    waitingPlayer.Locker.Release();

                    Console.WriteLine("Game #" + CurrentMatch.Id + " has started between players " +
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

        public void FinishCurrentMatch()
        {
            lock (WaitingPlayers)
            {
                this.Locker.WaitOne();

                if (this.CurrentMatch == null)
                {
                    if (WaitingPlayers.Count == 0)
                    {
                        this.Locker.Release();
                        return;
                    }

                    Console.WriteLine("Player " + this.ClientId + " is not waiting anymore");
                    Console.WriteLine();

                    WaitingPlayers.Dequeue();
                }
                else
                {
                    Console.WriteLine("Player " + this.ClientId + " has quit the match");
                    Console.WriteLine();

                    Player opponent = this.CurrentMatch.GetOpponent(this);

                    opponent.Locker.WaitOne();

                    try
                    {
                        opponent.InvokeCancelMatch(this.CurrentMatch.MoveCount >= 2);
                        opponent.CurrentMatch = null;
                    }
                    catch
                    {
                        //ignored
                    }

                    opponent.Locker.Release();

                    try
                    {
                        this.InvokeCancelMatch(this.CurrentMatch.MoveCount >= 2);
                        this.CurrentMatch = null;
                    }
                    catch
                    {
                        //ignored
                    }
                }

                this.Locker.Release();
            }
        }

        private void TryToRunIt(Action tryAction, Player opponent)
        {
            try
            {
                tryAction();
            }
            catch (EndpointNotFoundException)
            {
                if (opponent.Client.State == CommunicationState.Faulted)
                {
                    Console.WriteLine("Player " + opponent.ClientId + " couldn't be reached");
                    Console.WriteLine();

                    opponent.GameSession.SessionPlayer = null;

                    try { this.InvokeCancelMatch(true); }
                    catch
                    {
                        // ignored
                    }
                }
                else
                {
                    Console.WriteLine("Player " + this.ClientId + " couldn't be reached");
                    Console.WriteLine();

                    this.GameSession.SessionPlayer = null;

                    try { opponent.InvokeCancelMatch(true); }
                    catch
                    {
                        //ignored
                    }
                }
            }
        }

        public void MakeOriginalAndMirroredMove(int srcX, int srcY, int destX, int destY)
        {
            int mirroredSrcX = Math.Abs(srcX - 9);
            int mirroredSrcY = Math.Abs(srcY - 9);
            int mirroredDestX = Math.Abs(destX - 9);
            int mirroredDestY = Math.Abs(destY - 9);

            Player opponent = this.CurrentMatch.GetOpponent(this);

            var tryAction = new Action(async () =>
            {
                await Task.WhenAll(this.Client.MoveBoardPieceAsync(srcX, srcY, destX, destY),
                    opponent.Client.MoveBoardPieceAsync(mirroredSrcX, mirroredSrcY, mirroredDestX, mirroredDestY));
            });

            TryToRunIt(tryAction, opponent);

            this.CurrentMatch.MoveCount++;
        }

        public void MakeOriginalAndMirroredAttack(int srcX, int srcY, int destX, int destY,
            int attackerPowerLevel)
        {
            int mirroredSrcX = Math.Abs(srcX - 9);
            int mirroredSrcY = Math.Abs(srcY - 9);
            int mirroredDestX = Math.Abs(destX - 9);
            int mirroredDestY = Math.Abs(destY - 9);

            Player opponent = this.CurrentMatch.GetOpponent(this);

            var tryAction = new Action(async () =>
            {
                int defenderPowerLevel = opponent.Client.ShowPowerLevel(mirroredDestX, mirroredDestY);

                await Task.WhenAll(this.Client.AttackBoardPieceAsync(srcX, srcY, destX, destY,
                        attackerPowerLevel, defenderPowerLevel),
                    opponent.Client.AttackBoardPieceAsync(mirroredSrcX, mirroredSrcY, mirroredDestX, mirroredDestY,
                        attackerPowerLevel, defenderPowerLevel));
            });

            TryToRunIt(tryAction, opponent);

            this.CurrentMatch.MoveCount++;
        }
    }
}

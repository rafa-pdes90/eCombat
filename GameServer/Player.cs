using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Threading.Tasks;
using eCombat;

namespace GameServer
{
    public sealed class Player
    {
        private static Queue<Player> WaitingPlayers { get; }

        private Semaphore Locker { get; }
        private string ClientId { get; set; }
        private GameMasterSvc GameSession { get; set; }
        private EndpointDiscoveryMetadata RemoteMetadata { get; set; }
        private ICombateSvcChannel Client { get; set; }
        private string DisplayName { get; set; }
        private MatchInfo CurrentMatch { get; set; }
        private Timer Pinger { get; set; }

        static Player()
        {
            WaitingPlayers = new Queue<Player>();
        }

        private static void TryToRunIt(Action tryAction)
        {
            try
            {
                tryAction();
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        ~Player()
        {
            //TODO
            this.Locker.Close();
            
            try
            {
                this.Client?.Close();
            }
            catch (Exception)
            {
                this.Client?.Abort();
            }
            
            this.CurrentMatch = null;
        }

        public Player(GameMasterSvc gameSession, string clientId, EndpointDiscoveryMetadata playerMetadata)
        {
            this.Locker = new Semaphore(1, 1);

            this.Locker.WaitOne();

            Task.Run(() => Init(gameSession, clientId, playerMetadata));
        }

        private void Init(GameMasterSvc gameSession, string clientId, EndpointDiscoveryMetadata playerMetadata)
        {
            Console.WriteLine("Player " + "(" + clientId + ") has logged in");
            Console.WriteLine();

            this.ClientId = clientId;
            this.GameSession = gameSession;
            this.RemoteMetadata = playerMetadata;
            this.Client = null;
            this.DisplayName = null;
            this.CurrentMatch = null;

            this.Locker.Release();
        }

        public void EndGame()
        {
            //TODO
        }

        private void PingPlayer(object param)
        {
            this.Locker.WaitOne();

            if (this.Client == null)
            {
                this.Locker.Release();
                return;
            }

            try
            {
                this.Client.Ping();
            }
            catch (Exception)
            {
                Console.WriteLine("Player " + this.ClientId + " couldn't be reached");
                Console.WriteLine();

                this.Pinger.Dispose();

                if (this.CurrentMatch != null)
                {
                    Player opponent = this.CurrentMatch.GetOpponent(this);

                    opponent.Locker.WaitOne();

                    lock (this.CurrentMatch.Winner)
                    {
                        if (this.CurrentMatch.Winner == "0")
                        {
                            this.CurrentMatch.SetWinner(opponent);

                            opponent.InvokeEndMatch();
                        }
                    }

                    opponent.Locker.Release();

                    this.CurrentMatch = null;
                }

                this.Client.Abort();

                this.Locker.Release();

                Task.Run(() => this.GameSession.LeaveGame());
            }

            this.Locker.Release();
        }

        private void InvokeStartMatch(string opponentName, string opponentId, bool isPlayer2)
        {
            try
            {
                this.Client = GameMaster.NewClient(this.RemoteMetadata);
                this.Client.StartMatch(opponentName, opponentId, isPlayer2);
            }
            catch (Exception)
            {
                Console.WriteLine("Player " + this.ClientId + " couldn't be reached");
                Console.WriteLine();

                this.Client.Abort();

                Task.Run(() => this.GameSession.LeaveGame());

                throw;
            }

            this.Pinger = new Timer(PingPlayer, null, 0, 15000); // 15s
        }

        private void InvokeCancelMatch()  // Use if this is on WaitingPlayers queue
        {
            try
            {
                this.Client.EndMatch(this.CurrentMatch.MoveCount >= 2);
                this.Pinger.Dispose();
                this.Client.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Player " + this.ClientId + " couldn't be reached");
                Console.WriteLine();

                this.Client.Abort();
                this.Client = null;

                Task.Run(() => this.GameSession.LeaveGame());

                throw;
            }

            this.Client = null;
        }

        private void InvokeEndMatch() // Use if this is not on WaitingPlayers queue
        {
            var tryAction = new Action(() =>
            {
                this.Client.EndMatch(this.CurrentMatch.MoveCount >= 2);
                this.Pinger.Dispose();
                this.Client.Close();
                this.Client = null;
            });

            TryToRunIt(tryAction);
        }

        public void SeekMatch(string displayName)
        {
            lock (WaitingPlayers)
            {
                this.Locker.WaitOne();

                if (this.CurrentMatch != null)
                {
                    this.Locker.Release();
                    return;
                }

                this.DisplayName = displayName;

                while (WaitingPlayers.Count > 0)
                {
                    Player waitingPlayer = WaitingPlayers.Peek();

                    waitingPlayer.Locker.WaitOne();

                    try
                    {
                        waitingPlayer.InvokeStartMatch(this.DisplayName, this.ClientId, false);
                    }
                    catch (Exception)
                    {
                        WaitingPlayers.Dequeue();
                        waitingPlayer.Locker.Release();

                        continue;
                    }

                    try
                    {
                        this.InvokeStartMatch(waitingPlayer.DisplayName, waitingPlayer.ClientId, true);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            waitingPlayer.InvokeCancelMatch();
                        }
                        catch (Exception)
                        {
                            WaitingPlayers.Dequeue();
                        }
                        waitingPlayer.Locker.Release();

                        this.Locker.Release();
                        return;
                    }

                    WaitingPlayers.Dequeue();

                    this.CurrentMatch = GameMaster.NewMatch(waitingPlayer, this);
                    this.Locker.Release();

                    waitingPlayer.CurrentMatch = this.CurrentMatch;
                    waitingPlayer.Locker.Release();

                    Console.WriteLine("Game #" + CurrentMatch.Id + " has started between players " +
                                      waitingPlayer.ClientId + " and " + this.ClientId);
                    Console.WriteLine();
                    
                    return;
                }

                WaitingPlayers.Enqueue(this);

                this.Locker.Release();
            }

            Console.WriteLine("Player " + this.ClientId + " is waiting for an opponent");
            Console.WriteLine();
        }

        public void CancelCurrentMatch()
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

                    opponent.InvokeEndMatch();
                    opponent.CurrentMatch = null;

                    opponent.Locker.Release();

                    this.InvokeEndMatch();
                    this.CurrentMatch = null;
                }

                this.Locker.Release();
            }
        }

        public void RelayOriginalAndMirroredMove(int srcX, int srcY, int destX, int destY)
        {
            this.Locker.WaitOne();

            if (this.CurrentMatch == null)
            {
                this.Locker.Release();
                return;
            }

            int mirroredSrcX = Math.Abs(srcX - 9);
            int mirroredSrcY = Math.Abs(srcY - 9);
            int mirroredDestX = Math.Abs(destX - 9);
            int mirroredDestY = Math.Abs(destY - 9);

            Player opponent = this.CurrentMatch.GetOpponent(this);

            var tryAction = new Action(async () =>
            {
                await Task.WhenAll(
                    this.Client.MoveBoardPieceAsync(srcX, srcY, destX, destY, true),
                    opponent.Client.MoveBoardPieceAsync(
                        mirroredSrcX, mirroredSrcY, mirroredDestX, mirroredDestY, false));
            });

            TryToRunIt(tryAction);

            this.CurrentMatch.MoveCount++;

            this.Locker.Release();
        }

        public void RelayOriginalAndMirroredAttack(int srcX, int srcY, int destX, int destY,
            string attackerPowerLevel)
        {
            this.Locker.WaitOne();

            if (this.CurrentMatch == null)
            {
                this.Locker.Release();
                return;
            }

            int mirroredSrcX = Math.Abs(srcX - 9);
            int mirroredSrcY = Math.Abs(srcY - 9);
            int mirroredDestX = Math.Abs(destX - 9);
            int mirroredDestY = Math.Abs(destY - 9);

            string defenderPowerLevel = null;

            Player opponent = this.CurrentMatch.GetOpponent(this);

            var tryAction = new Action(async () =>
            {
                defenderPowerLevel = opponent.Client.ShowPowerLevel(mirroredDestX, mirroredDestY);

                await Task.WhenAll(
                    this.Client.AttackBoardPieceAsync(srcX, srcY, destX, destY,
                        attackerPowerLevel, defenderPowerLevel, true),
                    opponent.Client.AttackBoardPieceAsync(mirroredSrcX, mirroredSrcY, mirroredDestX, mirroredDestY,
                        attackerPowerLevel, defenderPowerLevel, false));
            });

            TryToRunIt(tryAction);

            this.CurrentMatch.MoveCount++;

            if (defenderPowerLevel == "*")
            {
                opponent.Locker.WaitOne();

                lock (this.CurrentMatch.Winner)
                {
                    this.CurrentMatch.SetWinner(this);
                }

                opponent.InvokeEndMatch();
                opponent.CurrentMatch = null;

                opponent.Locker.Release();

                this.InvokeEndMatch();
                this.CurrentMatch = null;
            }

            this.Locker.Release();
        }

        public void RelayTaggedMessage(string message)
        {
            this.Locker.WaitOne();

            if (this.CurrentMatch == null)
            {
                this.Locker.Release();
                return;
            }

            var msg = new ChatMsg
            {
                MsgId = this.CurrentMatch.MsgCount,
                MsgContent = message
            };

            Player opponent = this.CurrentMatch.GetOpponent(this);

            var tryAction = new Action(async () =>
            {
                await Task.WhenAll(
                    this.Client.WriteMessageToChatAsync(msg, true),
                    opponent.Client.WriteMessageToChatAsync(msg, false));
            });

            TryToRunIt(tryAction);

            this.CurrentMatch.MsgCount++;

            this.Locker.Release();
        }
    }
}

using System;
using System.ServiceModel;
using GameServer;

namespace eCombat.Model
{
    public static class GameMaster
    {
        private static string _myId;

        public static string MyId
        {
            get => SelfHost.State != CommunicationState.Opened ? null : _myId;

            private set => _myId = value;
        }

        public static string OpponentId { get; set; }

        private static CustomHost _selfHost;

        private static CustomHost SelfHost
        {
            get
            {
                if (_selfHost != null && _selfHost.State != CommunicationState.Closed)
                {
                    return _selfHost;
                }

                _selfHost = new CustomHost(typeof(CombateSvc));

                try
                {
                    MyId = _selfHost.CustomOpen("ICombateSvc");
                }
                catch (Exception)
                {
                    _selfHost.Abort();
                    throw;
                }

                return _selfHost;
            }
        }

        private static GameMasterSvcClient _client;

        public static GameMasterSvcClient Client
        {
            get
            {
                if (SelfHost.State != CommunicationState.Opened) return null;

                if (_client != null && _client.State != CommunicationState.Closed)
                {
                    return _client;
                }

                _client = new GameMasterSvcClient("Server_IGameMasterSvc");

                try
                {
                    _client.EnterGame(MyId);
                }
                catch (FaultException<GameMasterSvcFault>)
                {
                    _client.Abort();
                    throw;
                }
                catch (Exception)
                {
                    _client.Abort();
                    throw new CommunicationObjectAbortedException();
                }

                return _client;
            }
        }
    }
}

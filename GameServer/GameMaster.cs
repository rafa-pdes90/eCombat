using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Xml.Linq;

namespace GameServer
{
    public sealed class MatchInfo
    {
        public int Id { get; set; }
        public Player Player1 { get; }
        public Player Player2 { get; }
        public int MoveCount { get; set; }
        public int MsgCount { get; set; }
        public string Winner { get; private set; }

        public MatchInfo(Player p1, Player p2)
        {
            this.Player1 = p1;
            this.Player2 = p2;
            this.Winner = "0";
            this.MoveCount = 0;
            this.MsgCount = 0;
        }

        /// <summary>
        /// Gets the opponent to player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Player GetOpponent(Player player)
        {
            return player.Equals(this.Player1) ? this.Player2 : this.Player1;
        }

        /// <summary>
        /// Set player as the winner
        /// </summary>
        /// <param name="player"></param>
        public void SetWinner(Player player)
        {
            this.Winner = player.Equals(this.Player1) ? "1" : "2";
        }
    }

    public sealed class GameManager
    {
        public Dictionary<int, MatchInfo> MatchList { get; }
        public int MatchCount { get; private set; }

        public GameManager()
        {
            this.MatchList = new Dictionary<int, MatchInfo>();
            this.MatchCount = 0;
        }

        /// <summary>
        /// Updates newMatch with an Id and adds it to MatchList
        /// </summary>
        /// <param name="newMatch"></param>
        public void AddToMatchList(MatchInfo newMatch)
        {
            newMatch.Id = MatchCount;
            MatchList.Add(this.MatchCount++, newMatch);
        }
    }
    
    public static class GameMaster
    {
        public static Timer ProbeClientReseter { get; private set; }

        private static GameManager Manager { get; set; }
        private static DiscoveryClient ProbeClient { get; set; }
        private static ChannelFactory<ICombateSvcChannel> SvcFactory { get; set; }

        /// <summary>
        /// Needs to be called before using any other method or all will return null
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="probeEndpoint"></param>
        /// <param name="svcFactory"></param>
        public static void Init(DiscoveryEndpoint probeEndpoint, ChannelFactory<ICombateSvcChannel> svcFactory)
        {
            Manager = new GameManager();
            
            ProbeClient = new DiscoveryClient(probeEndpoint);

            ProbeClientReseter = new Timer(ReloadProbeClient, probeEndpoint, 900000, 900000); // 15min.

            SvcFactory = svcFactory;
        }
        
        /// <summary>
        /// Reloads ProbeClient
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <param name="param"></param>
        private static void ReloadProbeClient(object param)
        {
            lock (ProbeClient)
            {
                ProbeClient = new DiscoveryClient((DiscoveryEndpoint)param);
            }
        }

        /// <summary>
        /// Initializes a new client
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <param name="clientMetadata"></param>
        /// <returns></returns>
        public static ICombateSvcChannel NewClient(EndpointDiscoveryMetadata clientMetadata)
        {
            // Check to see if the endpoint has a listenUri and if it differs from the Address URI
            if (clientMetadata.ListenUris.Count > 0 &&
                clientMetadata.Address.Uri != clientMetadata.ListenUris[0])
            {
                return SvcFactory.CreateChannel
                    (clientMetadata.Address, clientMetadata.ListenUris[0]);
            }

            return SvcFactory.CreateChannel(clientMetadata.Address);
        }

        /// <summary>
        /// Initializes a new game and updates the game manager
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static MatchInfo NewMatch(Player p1, Player p2)
        {
            var newMatch = new MatchInfo(p1, p2);

            lock (Manager)
            {
                Manager.AddToMatchList(newMatch);
            }

            return newMatch;
        }

        /// <summary>
        /// Queries the proxy for a service
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="System.Reflection.TargetInvocationException"></exception>
        /// <param name="serviceType"></param>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        private static EndpointDiscoveryMetadata Probe(Type serviceType = null, string serviceId = null)
        {
            FindCriteria svcSearch = serviceType == null ? new FindCriteria() : new FindCriteria(serviceType);

            if (serviceId != null)
            {
                var searchExtension = new XElement("Id", serviceId);
                svcSearch.Extensions.Add(searchExtension);
            }

            FindResponse searchResponse;

            lock (ProbeClient)
            {
                searchResponse = ProbeClient.Find(svcSearch);
            }

            return searchResponse.Endpoints.FirstOrDefault();
        }

        /// <summary>
        /// Queries the proxy for a service
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="System.Reflection.TargetInvocationException"></exception>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata Probe(string serviceId = null)
        {
            return Probe(null, serviceId);
        }

        /// <summary>
        /// Queries the proxy for a service
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="System.Reflection.TargetInvocationException"></exception>
        /// <typeparam name="TType"></typeparam>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata Probe<TType>(string serviceId = null)
        {
            return Probe(typeof(TType), serviceId);
        }

        /// <summary>
        /// Queries the proxy for a service
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="System.Reflection.TargetInvocationException"></exception>
        /// <param name="serviceUri"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata Probe(Uri serviceUri)
        {
            var svcSearch = new ResolveCriteria(new EndpointAddress(serviceUri));

            ResolveResponse searchResponse;
            lock (ProbeClient)
            {
                searchResponse = ProbeClient.Resolve(svcSearch);
            }

            return searchResponse.EndpointDiscoveryMetadata;
        }
    }
}

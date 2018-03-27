using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public sealed class MatchInfo
    {
        public int Id { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int Winner { get; set; }
        public int MoveCount { get; set; }

        public MatchInfo(Player p1, Player p2)
        {
            this.Player1 = p1;
            this.Player2 = p2;
            this.Winner = 0;
        }

        public Player GetOpponent(Player player)
        {
            return player.Equals(this.Player1) ? this.Player2 : this.Player1;
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

        public void AddToMatchList(MatchInfo newMatch)
        {
            newMatch.Id = MatchCount;
            MatchList.Add(this.MatchCount++, newMatch);
        }
    }

    public static class GameMaster
    {
        private static GameManager Manager { get; set; }

        // ReSharper disable once NotAccessedField.Local
        private static Timer _probeClientReseter;
        private static DiscoveryClient ProbeClient { get; set; }
        private static ChannelFactory<ICombateSvcChannel> SvcFactory { get; set; }

        public static void Init(Uri probeUri, Binding probeBinding, ChannelFactory<ICombateSvcChannel> svcFactory)
        {
            Manager = new GameManager();
            
            var probeEndpoint = new DiscoveryEndpoint(probeBinding, new EndpointAddress(probeUri));
            ProbeClient = new DiscoveryClient(probeEndpoint);
            _probeClientReseter = new Timer(ReloadProbeClient, probeEndpoint, 900000, 900000); // 15min.
            SvcFactory = svcFactory;
        }

        private static void ReloadProbeClient(object param)
        {
            // Create a DiscoveryClient that points to the DiscoveryProxy
            lock (ProbeClient)
            {
                ProbeClient = new DiscoveryClient((DiscoveryEndpoint)param);
            }
        }
        
        /// <summary>
        /// Initializes a new client
        /// </summary>
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
        
        private static EndpointDiscoveryMetadata Probe(string serviceId, Type serviceType)
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
        /// Query the proxy for all services or a specific one by its Id
        /// </summary>
        /// <exception cref="TargetInvocationException"></exception>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata Probe(string serviceId = null)
        {
            return Probe(serviceId, null);
        }

        /// <summary>
        /// Query the proxy for all TType services or a specific TType one by its Id
        /// </summary>
        /// <exception cref="TargetInvocationException"></exception>
        /// <typeparam name="TType"></typeparam>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata Probe<TType>(string serviceId = null)
        {
            return Probe(serviceId, typeof(TType));
        }

        /// <summary>
        /// Query the proxy for a specific service by its URI
        /// </summary>
        /// <exception cref="TargetInvocationException"></exception>
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

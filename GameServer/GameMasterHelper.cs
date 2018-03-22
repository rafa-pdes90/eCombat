using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer
{
    public class GameManager
    {
        public Queue<GameMaster> WaitingPlayers { get; set; }
        public Dictionary<int, GameInfo> GameList { get; }
        public int GameCount { get; private set; }

        public GameManager()
        {
            this.WaitingPlayers = new Queue<GameMaster>();
            this.GameList = new Dictionary<int, GameInfo>();
            this.GameCount = 0;
        }

        public void AddToGameList(GameInfo newGame)
        {
            newGame.Id = GameCount;
            GameList.Add(this.GameCount++, newGame);
        }
    }

    public class GameInfo
    {
        public int Id { get; set; }
        public GameMaster Player1 { get; set; }
        public GameMaster Player2 { get; set; }
        public int Winner { get; set; }

        public GameInfo(GameMaster p1, GameMaster p2)
        {
            this.Player1 = p1;
            this.Player2 = p2;
            this.Winner = 0;
        }
    }

    public class GMHelper
    {
        private static DiscoveryClient ProbeClient { get; set; }
        private static Type SvcBindingType { get; set; }
        private static SecurityMode SvcSecurityMode { get; set; }

        public static void Init(Uri probeUri, Binding probeBinding, Type svcBindingType, SecurityMode svcSecurityMode)
        {
            // Create a DiscoveryClient that points to the DiscoveryProxy
            var discoveryEndpoint = new DiscoveryEndpoint(probeBinding, new EndpointAddress(probeUri));
            ProbeClient = new DiscoveryClient(discoveryEndpoint);
            SvcBindingType = svcBindingType;
            SvcSecurityMode = svcSecurityMode;
        }
        
        /// <summary>
        /// Initializes a new client
        /// </summary>
        /// <param name="clientMetadata"></param>
        /// <returns></returns>
        public static CombateSvcClient NewClient(EndpointDiscoveryMetadata clientMetadata)
        {
            var clientBinding = (Binding)Activator.CreateInstance(SvcBindingType, SvcSecurityMode);
            var client = new CombateSvcClient(clientBinding, clientMetadata.Address);

            // Check to see if the endpoint has a listenUri and if it differs from the Address URI
            if (clientMetadata.ListenUris.Count > 0 &&
                clientMetadata.Address.Uri != clientMetadata.ListenUris[0])
            {
                client.Endpoint.Behaviors.Add(new ClientViaBehavior(clientMetadata.ListenUris[0]));
            }

            return client;
        }

        /// <summary>
        /// Initializes a new game and updates the game manager
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="house"></param>
        /// <returns></returns>
        public static GameInfo NewGame(GameMaster p1, GameMaster p2, ref GameManager house)
        {
            var newGame = new GameInfo(p1, p2);
            house.AddToGameList(newGame);

            return newGame;
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

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
        public GameMaster WaitingPlayer { get; set; }
        public Dictionary<int, GameInfo> GameList { get; }
        public int GameCount { get; private set; }

        public GameManager()
        {
            this.WaitingPlayer = null;
            this.GameList = new Dictionary<int, GameInfo>();
            this.GameCount = 0;
        }

        public void AddToGameList(GameInfo newGame)
        {
            GameList.Add(this.GameCount++, newGame);
        }
    }

    public class GameInfo
    {
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
        /// <param name="newPlayer"></param>
        /// <param name="house"></param>
        /// <returns></returns>
        public static GameInfo NewGame(GameMaster newPlayer, ref GameManager house)
        {
            if (house.WaitingPlayer == null)
            {
                house.WaitingPlayer = newPlayer;
                return null;
            }
            
            var newGame = new GameInfo(house.WaitingPlayer, newPlayer);
            house.AddToGameList(newGame);

            house.WaitingPlayer = null;
            return newGame;
        }

        /// <summary>
        /// Try to find service by its serviceUri
        /// </summary>
        /// <exception cref="TargetInvocationException"></exception>
        /// <param name="serviceUri"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata ProbeByUri(Uri serviceUri)
        {
            var svcSearch = new ResolveCriteria(new EndpointAddress(serviceUri));

            ResolveResponse searchResponse;
            lock (ProbeClient)
            {
                searchResponse = ProbeClient.Resolve(svcSearch);
            }

            return searchResponse.EndpointDiscoveryMetadata;
        }

        /// <summary>
        /// Try to find a service of TType by its Id
        /// </summary>
        /// <exception cref="TargetInvocationException"></exception>
        /// <typeparam name="TType"></typeparam>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata ProbeById<TType>(string serviceId)
        {
            var svcSearch = new FindCriteria(typeof(TType));
            var searchExtension = new XElement("Id", serviceId);
            svcSearch.Extensions.Add(searchExtension);
            
            FindResponse searchResponse;
            lock (ProbeClient)
            {
                searchResponse = ProbeClient.Find(svcSearch);
            }
            
            return searchResponse.Endpoints.FirstOrDefault();
        }

        /*
        static void FindServiceAsync<TType>()
        {
            DiscoveryClient dc = new DiscoveryClient(new UdpDiscoveryEndpoint());
            dc.FindCompleted += new EventHandler<FindCompletedEventArgs>(discoveryClient_FindCompleted);
            dc.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(discoveryClient_FindProgressChanged);
            dc.FindAsync(new FindCriteria(typeof(TType)));
        }
        static void discoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            Console.WriteLine("Found service at: " + e.EndpointDiscoveryMetadata.Address);
        }

        static void async discoveryClient_FindCompleted(object sender, FindCompletedEventArgs e)
        {
            if (e.Result.Endpoints.Count > 0)
            {
                EndpointAddress ep = e.Result.Endpoints[0].Address;
                var client = new CalculatorServiceClient();

                // Connect to the discovered service endpoint  
                client.Endpoint.Address = ep;
                Console.WriteLine("Invoking CalculatorService at {0}", ep);

                await client.DoWorkAsync();

                client.Close(); //?
            }
            else
                Console.WriteLine("No matching endpoints found");
        }
        */
    }
}

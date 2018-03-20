using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer
{
    public class GMHelper
    {
        private static DiscoveryClient ProbeClient { get; set; }
        public static Type CombateSvcBinding { get; set; }
        public static SecurityMode CombateSvcSecurity { get; set; }

        public static void Init(Uri probeUri, Binding probeBinding, Type combateSvcBinding,SecurityMode combateSvcSecurity)
        {
            // Create a DiscoveryClient that points to the DiscoveryProxy
            var discoveryEndpoint = new DiscoveryEndpoint(probeBinding, new EndpointAddress(probeUri));
            ProbeClient = new DiscoveryClient(discoveryEndpoint);
            CombateSvcBinding = combateSvcBinding;
            CombateSvcSecurity = combateSvcSecurity;
        }

        /// <summary>
        /// Try to find a service of TType by its ID
        /// </summary>
        /// <exception cref="TargetInvocationException"></exception>
        /// <typeparam name="TType"></typeparam>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static EndpointDiscoveryMetadata ProbeById<TType>(string serviceId)
        {
            var svcSearch = new FindCriteria(typeof(TType));
            var searchExtension = new XElement("ID", serviceId);
            svcSearch.Extensions.Add(searchExtension);
            
            FindResponse searchResponse;
            lock (ProbeClient)
            {
                searchResponse = ProbeClient.Find(svcSearch);
            }
            
            return searchResponse.Endpoints.FirstOrDefault();
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

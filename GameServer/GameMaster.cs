using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Text;
using System.Xml.Linq;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GameMaster" in both code and config file together.
    public class GameMaster : IGameMaster
    {
        public void DoWork()
        {
            Console.WriteLine(@"Teste GameServer");

            // Create a DiscoveryClient that points to the DiscoveryProxy  
            var probeBinding = new NetTcpBinding(SecurityMode.None);
            var probeUri = new Uri("net.tcp://localhost:8001/Probe/");
            var discoveryEndpoint = new DiscoveryEndpoint(probeBinding, new EndpointAddress(probeUri));
            var discoveryClient = new DiscoveryClient(discoveryEndpoint);

            try
            {
                // Find IeCombatSvc endpoints
                var eCombatSvcSearch = new FindCriteria(typeof(IeCombatSvc));
                var searchExtension = new XElement("Name", "eCombatSvc0");
                eCombatSvcSearch.Extensions.Add(searchExtension);

                FindResponse searchResponse;
                lock (discoveryClient)
                {
                    searchResponse = discoveryClient.Find(eCombatSvcSearch);
                }

                // Check to see if endpoints were found, if so then invoke the service.
                EndpointDiscoveryMetadata discoveredEndpoint = searchResponse.Endpoints.FirstOrDefault();
                if (discoveredEndpoint == null) return;

                // Check to see if the endpoint has a listenUri and if it differs from the Address URI
                if (discoveredEndpoint.ListenUris.Count > 0 && discoveredEndpoint.Address.Uri != discoveredEndpoint.ListenUris[0])
                {
                    // Since the service is using a unique ListenUri, it needs to be invoked at the correct ListenUri 
                    InvokeeCombatService(discoveredEndpoint.Address, discoveredEndpoint.ListenUris[0]);
                }
                else
                {
                    // Endpoint was found, however it doesn't have a unique ListenUri, hence invoke the service with only the Address URI
                    InvokeeCombatService(discoveredEndpoint.Address, null);
                }
            }
            catch (TargetInvocationException)
            {
                Console.WriteLine("This client was unable to connect to and query the proxy. Ensure that the proxy is up and running.");
            }
        }

        private static async void InvokeeCombatService(EndpointAddress endpointAddress, Uri viaUri)
        {
            // Create a client  
            var client = new eCombatSvcClient(new NetTcpBinding(SecurityMode.None), endpointAddress);

            // if viaUri is not null then add the approprate ClientViaBehavior.
            if (viaUri != null)
            {
                client.Endpoint.Behaviors.Add(new ClientViaBehavior(viaUri));
            }

            try
            {
                // Call the Add service operation.  
                await client.DoWorkAsync();

                // Closing the client gracefully closes the connection and cleans up resources  
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                client.Abort();
            }
        }
    }
}

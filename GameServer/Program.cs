using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var selfHost = new ServiceHost(typeof(GameMaster));
            selfHost.Open();

            /*
            // Create a DiscoveryClient that points to the DiscoveryProxy  
            var probeEndpointAddress = new Uri("net.tcp://localhost:8001/Probe/");
            var discoveryEndpoint = new DiscoveryEndpoint(new NetTcpBinding(SecurityMode.None), new EndpointAddress(probeEndpointAddress));
            var discoveryClient = new DiscoveryClient(discoveryEndpoint);
            IChannelListener channelListener = SelfHost.ChannelDispatchers[1].Listener;
            if (channelListener != null)
            {
                Uri myUri = channelListener.Uri;
                var myResolveCriteria = new ResolveCriteria(new EndpointAddress(myUri));
                ResolveResponse proxyResponse = discoveryClient.Resolve(myResolveCriteria);
                EndpointDiscoveryMetadata myMetadata = proxyResponse.EndpointDiscoveryMetadata;
                foreach (var uriDoida in myMetadata.ListenUris)
                    Console.WriteLine(uriDoida);
                Console.WriteLine(myMetadata.Address);
                string myPublicName = myMetadata.Extensions.First(x => x.Name.LocalName == "Name").Value;
                Console.WriteLine(myPublicName);
            }
            */

            Console.WriteLine("Game Server started.");
            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to terminate the server.");
            Console.WriteLine();
            Console.ReadLine();
        }
    }
}

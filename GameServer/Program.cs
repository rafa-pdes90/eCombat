using System;
using System.ServiceModel;

namespace GameServer
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var selfHost = new ServiceHost(typeof(GameMaster));
            try
            {
                selfHost.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();

                selfHost.Abort();
                return;
            }

            Console.WriteLine("Game Server started.");
            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to terminate the server.");
            Console.WriteLine();
            Console.ReadLine();

            selfHost.Close();
        }

        /*
        static void FindServiceAsync()
        {
            DiscoveryClient dc = new DiscoveryClient(new UdpDiscoveryEndpoint());
            dc.FindCompleted += new EventHandler<FindCompletedEventArgs>(discoveryClient_FindCompleted);
            dc.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(discoveryClient_FindProgressChanged);
            dc.FindAsync(new FindCriteria(typeof(ICombateSvc)));
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
                var client = new CombateSvcClient();

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

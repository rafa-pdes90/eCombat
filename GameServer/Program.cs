using System;
using System.ServiceModel;

namespace GameServer
{
    internal class Program
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            var proxyUri = new Uri("net.tcp://localhost:8001/Probe");
            var proxyBinding = new NetTcpBinding(SecurityMode.None);
            var svcBinding = new NetTcpBinding(SecurityMode.None);
            var svcFactory = new ChannelFactory<ICombateSvcChannel>(svcBinding);

            GameMaster.Init(proxyUri, proxyBinding, svcFactory);

            var selfHost = new ServiceHost(typeof(GameMasterSvc));
            try
            {
                selfHost.Open();
                Console.WriteLine("Game Server started.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case CommunicationObjectFaultedException _:
                    case EndpointNotFoundException _:
                        Console.WriteLine("The Discovery Proxy couldn't be reached.");
                        break;
                    case TimeoutException _:
                        Console.WriteLine("The Discovery Proxy is unresponsive.");
                        break;
                    case CommunicationException _:
                        Console.WriteLine("The Discovery Proxy refused the connection.");
                        break;
                    default:
                        Console.WriteLine(e);
                        break;
                }
                Console.WriteLine();

                selfHost.Abort();
            }

            Console.WriteLine("Press <ENTER> to terminate the server.");
            Console.WriteLine();
            Console.ReadLine();

            try
            {
                selfHost.Close();
            }
            catch (Exception)
            {
                selfHost.Abort();
            }
        }
    }
}

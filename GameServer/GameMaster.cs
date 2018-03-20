using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Text;
using System.Xml.Linq;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GameMaster" in both code and config file together.
    public class GameMaster : IGameMaster
    {
        private EndpointDiscoveryMetadata ClientMetadata { get; set; }
        public static CombateSvcClient Player { get; set; }

        public void IntroduceToGameMaster(Uri clientUri)
        {
            try
            {
                EndpointDiscoveryMetadata probedMetadata = GMHelper.ProbeByUri(clientUri);

                var testCriteria = new FindCriteria(typeof(ICombateSvc));
                if (testCriteria.IsMatch(probedMetadata))
                {
                    var clientBinding = (Binding)Activator.CreateInstance(GMHelper.CombateSvcBinding);
                    var client = new CombateSvcClient(new NetTcpBinding(SecurityMode.None), probedMetadata.Address);

                    // Check to see if the endpoint has a listenUri and if it differs from the Address URI
                    if (probedMetadata.ListenUris.Count > 0 && probedMetadata.Address.Uri != probedMetadata.ListenUris[0])
                    {
                        client.Endpoint.Behaviors.Add(new ClientViaBehavior(probedMetadata.ListenUris[0]));
                    }

                    this.ClientMetadata = probedMetadata;
                    Player = client;
                }
                else
                {

                }
            }
            catch (TargetInvocationException e)
            {
                Console.WriteLine(e);
            }
        }

        public void DoWork()
        {
            Console.WriteLine(@"Testing GameServer");

            try
            {
                // Call the Add service operation.  
                Player.DoWork();

                // Closing the client gracefully closes the connection and cleans up resources  
                Player.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                Player.Abort();
            }
        }
    }
}

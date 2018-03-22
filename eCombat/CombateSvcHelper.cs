using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Xml.Linq;

namespace eCombat
{
    public class CustomHost : ServiceHost
    {
        public CustomHost(Type serviceType) : base(serviceType)
        {
        }

        public string CustomOpen(string parents)
        {
            var behavior = new EndpointDiscoveryBehavior();
            var parentsElem = new XElement("Parents", parents);
            
            behavior.Extensions.Add(parentsElem);

            ServiceEndpoint endpoint =
                Description.Endpoints.First(x => x.Name == "CombateSvcEndpoint");
            while (true)
            {
                string serviceId = Guid.NewGuid().ToString();
                var id = new XElement("Id", serviceId);
                behavior.Extensions.Add(id);
                endpoint.EndpointBehaviors.Add(behavior);

                try
                {
                    Open();
                    return serviceId;
                }
                catch (Exception e)
                {
                    switch (e)
                    {
                        case CommunicationException _:
                            behavior.Extensions.Remove(id);
                            break;
                        default:
                            throw;
                    }
                }
            }
        }
    }
}

using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Xml.Linq;

namespace eCombat.Model
{
    public class CustomHost : ServiceHost
    {
        public CustomHost(Type serviceType) : base(serviceType)
        {
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="CommunicationObjectFaultedException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <param name="contract"></param>
        /// <returns></returns>
        public string CustomOpen(string contract)
        {
            return CustomOpen(contract, base.DefaultOpenTimeout);
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="CommunicationObjectFaultedException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <param name="timeout"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        public string CustomOpen(string contract, TimeSpan timeout)
        {
            string serviceId = Guid.NewGuid().ToString();
            var id = new XElement("Id", serviceId);
            var contractElem = new XElement("Contract", contract);
            var behavior = new EndpointDiscoveryBehavior();

            behavior.Extensions.Add(id);
            behavior.Extensions.Add(contractElem);

            Description.Endpoints.First(x => x.Name == "CombateSvcEndpoint").EndpointBehaviors.Add(behavior);
            
            base.Open(timeout);

            return serviceId;
        }
    }
}

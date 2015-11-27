
namespace RelaySamples
{
    using System;
    using System.ServiceModel;
    using Microsoft.ServiceBus;
    using System.Threading.Tasks;

    // This is an all-in-one Relay service that can be exposed through the Service Bus
    // Relay. As the Service Bus client is based on WCF, the Program class carries two 
    // attributes. [ServiceContract] declares the contract and you find that echoed in
    // the client project. [ServiceBehavior] tells the WCF runtime that we want to host
    // the service out of a singleton instance.
    [ServiceContract(Namespace = "", Name = "echo"), 
     ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    class Program : ITcpListenerSample
    {
        public async Task Run(string listenAddress, string listenToken)
        {
            // The host for our service is a regular WCF service host. You can use 
            // all extensibility options of WCF and you can also host non-Relay
            // endpoints alongside the Relay endpoints on this host
            using (ServiceHost host = new ServiceHost(this))
            {
                // Now we're adding the service endpoint with a listen address on Service Bus
                // and using the NetTcpRelayBinding, which is a variation of the regular
                // NetTcpBinding of WCF with the difference that this one listens on the
                // Service Bus Relay service.                
                // Since the Service Bus Relay requires Authorization, we then also add the 
                // SAS token provider to the endpoint.
                host.AddServiceEndpoint(GetType(), new NetTcpRelayBinding(), listenAddress)
                    .EndpointBehaviors.Add(
                        new TransportClientEndpointBehavior(
                            TokenProvider.CreateSharedAccessSignatureTokenProvider(listenToken)));

                // once open returns, the service is open for business. Not async for legibility.
                host.Open();
                Console.WriteLine("Service listening at address {0}", listenAddress);
                Console.WriteLine("Press [Enter] to close the listener and exit.");
                Console.ReadLine();
                host.Close();
            }
        }

        [OperationContract]
        async Task<string> Echo(string input)
        {
            Console.WriteLine("\tCall received with input \"{0}\"", input);
            return input;
        }
    }
}
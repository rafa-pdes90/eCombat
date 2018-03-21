﻿using System;
using System.ServiceModel;

namespace GameServer
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var proxyUri = new Uri("net.tcp://localhost:8001/Probe");
            var proxyBinding = new NetTcpBinding(SecurityMode.None);
            Type svcBindingType = typeof(NetTcpBinding);
            const SecurityMode svcSecurityMode = SecurityMode.None;
            GMHelper.Init(proxyUri, proxyBinding, svcBindingType, svcSecurityMode);

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
    }
}

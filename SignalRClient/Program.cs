using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;


namespace SignalRClient
{
    class Program
    {
        static void Main(string[] args)
        {


            var connection = new HubConnection("http://127.0.0.1:8088/");
            var myHub = connection.CreateHubProxy("MyHub");

            var connect = connection.Start();
            connect.Wait();

            myHub.Invoke<string>("Send", "test message");

            
            Console.ReadKey();

            //connection.Start().ContinueWith(task => {
            //    if (task.IsFaulted)
            //    {
            //        Console.WriteLine("There was an error opening the connection:{0}", task.Exception.GetBaseException());
            //    }
            //    else
            //    {
            //        Console.WriteLine("Connected");
                    
            //    }

            //}).Wait();
        }
    }
}

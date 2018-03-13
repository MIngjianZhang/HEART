using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UWPClient
{
    static void Main(string[] args)
    {
        try
        {
            int port = 1337;
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.43.221"), port);
            TcpClient client = new TcpClient();
            //client.NoDelay = true;
            //if (client.NoDelay == true)
            //    Console.WriteLine("Nodelay set to true ");
            Console.WriteLine("Connecting...\n");
            client.Connect(ip);
            Console.WriteLine("Connected!\n");
            using (NetworkStream ns = client.GetStream())
            {
                using (StreamReader sr = new StreamReader(ns))
                {
                    StreamWriter sw = new StreamWriter(ns);
                    sw.WriteLine((10.0)*Double.Parse(args[0])+(-10.0)*Double.Parse(args[1]));
                    sw.Flush();
                    DateTime localDate = DateTime.Now;
                    Console.WriteLine("Time is ~~~ " + localDate);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

}
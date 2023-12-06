using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Sockets;



namespace Taurus
{
    internal class Program
    {
        public static RouterSocket server = new RouterSocket("@tcp://*:5556");
        static void Main(string[] args)
        {
            DetectorManager dm = new DetectorManager();
            // server loop
            while (true)
            {
                var clientMessage = server.ReceiveMultipartMessage();
                
                if (clientMessage[2].ConvertToString() == "PreAcq")
                {
                    dm.PreAcq(clientMessage[0]);
                }
            }
        }
        static void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }
        static void Client_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            bool hasmore_b;
            e.Socket.ReceiveFrameString(out hasmore_b);
            if (hasmore_b)
            {
                string result = e.Socket.ReceiveFrameString(out hasmore_b);
                Console.WriteLine("REPLY {0}", result);
            }
        }
    }
}

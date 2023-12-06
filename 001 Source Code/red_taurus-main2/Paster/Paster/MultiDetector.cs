using NetMQ.Sockets;
using NetMQ;
using System.Text;
using static System.Windows.Forms.AxHost;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Paster
{
    public partial class MultiDetector : Form
    {
        private NetMQPoller poller = new NetMQPoller();
        private List<string> detectorList = new List<string>();
        private List<DealerSocket> socketList = new List<DealerSocket>();

        //static PublisherSocket pubSocket = new PublisherSocket();

        static void pub_sever_open()
        {
            using (var pubSocket = new PublisherSocket())
            {
                pubSocket.Bind("tcp://*:5555");
                {
                    Thread.Sleep(1000); // Delay time for subscribers

                    var fileBytes = File.ReadAllBytes("C:\\VSI\\Parm_config.txt");
                    pubSocket.SendFrame(fileBytes);
                    Debug.WriteLine("Send");
                }
            }

        }
        public MultiDetector()
        {
            InitializeComponent();

        }

        private void PrintFrames(string operationType, NetMQMessage message)
        {
            for (int i = 0; i < message.FrameCount; i++)
            {
                Console.WriteLine("{0} Socket : Frame[{1}] = {2}", operationType, i,
                    message[i].ConvertToString());
            }
        }
        private void Client_ReceiveReady(object? sender, NetMQSocketEventArgs e)
        {

            bool hasmore_b;
            //e.Socket.ReceiveFrameString(out hasmore_b);
            string result = e.Socket.ReceiveFrameString(out hasmore_b);
            result = e.Socket.ReceiveFrameString(out hasmore_b);
            if (result == "AckImage")
            {
                int nWidth = 0;
                int nHeight = 0;
                int nBytesPerPixel = 0;
                byte[] m_nSendaddr = e.Socket.ReceiveFrameBytes();
                Array.Reverse(m_nSendaddr);
                int ipaddr = BitConverter.ToInt32(m_nSendaddr);


                if (BitConverter.IsLittleEndian)
                {
                    byte[] bytes = e.Socket.ReceiveFrameBytes();
                    Array.Reverse(bytes);
                    nWidth = BitConverter.ToInt32(bytes);
                    bytes = e.Socket.ReceiveFrameBytes();
                    Array.Reverse(bytes);
                    nHeight = BitConverter.ToInt32(bytes);
                    bytes = e.Socket.ReceiveFrameBytes();
                    Array.Reverse(bytes);
                    nBytesPerPixel = BitConverter.ToInt32(bytes);
                }


                Debug.WriteLine('1');

                byte[] ImgData = e.Socket.ReceiveFrameBytes();
                using (MemoryStream ms = new MemoryStream(ImgData))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        int numPixels = nWidth * nHeight;
                        ushort[] pixelData = new ushort[numPixels];

                        for (int i = 0; i < numPixels; i++)
                        {
                            // Read 16-bit pixel data (little endian)
                            byte lowByte = reader.ReadByte();
                            byte highByte = reader.ReadByte();
                            ushort pixelValue = (ushort)((highByte << 8) | lowByte);

                            // Store pixel data in array
                            pixelData[i] = pixelValue;
                        }

                        // Convert 16-bit pixel data to 8-bit grayscale image
                        byte[] imageData = new byte[numPixels];
                        for (int i = 0; i < numPixels; i++)
                        {
                            imageData[i] = (byte)(pixelData[i] / 256);
                        }

                        // Create Bitmap object from grayscale image data
                        Bitmap bitmap = new Bitmap(nWidth, nHeight, PixelFormat.Format8bppIndexed);
                        ColorPalette palette = bitmap.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            palette.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        bitmap.Palette = palette;

                        Rectangle rect = new Rectangle(0, 0, nWidth, nHeight);
                        BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);
                        bitmap.UnlockBits(bmpData);

                        // Save image as TIFF file
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string filePath = string.Format(".\\Image\\Image{0}_{1}.tif", ipaddr, timestamp);
                        bitmap.Save(filePath, ImageFormat.Tiff);

                        string logFilePath = "C:\\VSI\\logs\\btnPreAcqClickLog.txt";

                        DateTime currentTime = DateTime.Now;
                        string curTime = currentTime.ToString("yyyy-mm-dd HH:mm:ss.ffff");
                        string logFinishMessage = $"Finish : {curTime}\n";
                        File.AppendAllText(logFilePath, logFinishMessage);
                    }
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string filePath = "C:/VSI/Detectorlist.txt";
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {

                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4 && parts[0] == "addr" && parts[2] == "port")
                {
                    string detectorName = string.Format("proxy_{0}", detectorList.Count);
                    detectorList.Add(detectorName);

                    DealerSocket client = new DealerSocket();
                    client.Options.Identity = Encoding.Unicode.GetBytes(detectorName ?? string.Empty);
                    client.Connect(String.Format("tcp://{0}:{1}", parts[1], parts[3]));
                    client.ReceiveReady += Client_ReceiveReady;
                    poller.Add(client);
                    socketList.Add(client);
                }
            }

            poller.RunAsync();
        }

        // origin working code

        private void btnPreAcq_Click(object sender, EventArgs e)
        {
            //thread.sleep(1000); //start delay
            while (true)
            {
                for (int i = 0; i < detectorList.Count; i++)
                {
                    string logfilepath = "C:\\VSI\\logs\\btnPreAcqClickLog.txt";
                    string logstartmessage = $"start : {DateTime.Now}";
                    File.AppendAllText(logfilepath, logstartmessage);

                    var messagetoserver = new NetMQMessage();
                    // run gun or oddy run gun  
                    messagetoserver.AppendEmptyFrame();
                    messagetoserver.Append("PreAcq");
                    socketList[i].SendMultipartMessage(messagetoserver);

                    Debug.WriteLine("Currnet Thread: " + i);
                    Thread.Sleep(2000);
                }
                //thread.sleep(60000); // delay time per cycle
            }
        }

        //private void btnPreAcq_Click(object sender, EventArgs e)
        //{
        //    for (int i = 0; i < 2; i++)
        //    {
        //        string logFilePath = "C:\\VSI\\logs\\btnPreAcqClickLog.txt";
        //        DateTime currentTime = DateTime.Now;
        //        string curTime = currentTime.ToString("yyyy-mm-dd HH:mm:ss.ffff");
        //        string logStartMessage = $"Start : {curTime}\n";
        //        File.AppendAllText(logFilePath, logStartMessage);
        //        var messageToServer = new NetMQMessage();
        //        // run gun or oddy run gun
        //        messageToServer.AppendEmptyFrame();
        //        messageToServer.Append("PreAcq");
        //        socketList[i].SendMultipartMessage(messageToServer);
        //        Thread.Sleep(1000);
        //    }

        //}

        private void MultiDetector_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (DealerSocket socket in socketList)
            {
                socket.Close();
            }

            poller.StopAsync();
            poller.Dispose();
        }

        private void txtIPAddr_TextChanged(object sender, EventArgs e)
        {

        }


        static void config_sender_sever_open()
        {
            using (var pubSocket = new PublisherSocket())
            {
                pubSocket.Bind("tcp://*:5555");
                while (true)
                {
                    Thread.Sleep(1000); // Delay time for subscribers

                    var fileBytes = File.ReadAllBytes("C:\\VSI\\Parm_config.txt");
                    pubSocket.SendFrame(fileBytes);
                }
            }

        }

        private void btnServer_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(pub_sever_open));
            t.Start(); // sending parm config to LattePandas
        }
    }
}
using iDetector;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace Taurus
{
    internal class DetectorManager: EventReceiver
    {
        private static int m_nId = -1;
        private int m_nStatus = 0;
        private NetMQ.NetMQFrame m_nSenderAddr = null;
        private IRayImage CurImage;

        //private DispatcherTimer ConnectStateTimer;
        private static int CorrectionOpt = 0;

        static SerialPort port = new SerialPort(Get_parm("arduino_port"), 9600);

        


        static string Get_parm(string mode) // TODO : automatically get port from setting
        {
            string filePath = @"C:\\VSI2\\TaurusConfig.txt";
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                if (mode == "arduino_port")
                {
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("arduino_device_port"))
                        {
                            string[] values = line.Split('=');
                            string devicePort = values[1].Trim();
                            Console.WriteLine("Arduino port: " + devicePort + "\nConnected");
                            return devicePort;
                        }
                    }
                    Console.WriteLine("device_port value not found.");
                    return null;
                }
                else if (mode == "gun_time")
                {
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("gun_time"))
                        {
                            string[] values = line.Split('=');
                            string gunTime = values[1].Trim();
                            Console.WriteLine("Gun Time: " + gunTime);
                            return gunTime;
                        }
                    }
                    Console.WriteLine("gun time value not found.");
                    return null;
                }
                else { return null; }
                
            }
            else
            {
                Console.WriteLine("File not exist. Need check");
                return null;
            }
        }

        static void on_recv_5v()
        {
            port.Open();
            port.Write("g");
            string gun_time = Get_parm("gun_time");
            Thread.Sleep(int.Parse(gun_time));
            port.Write("n");
            port.Close();

        }

        public DetectorManager()
        {
            m_nId = Detector.CreateDetector("C:\\VSI2", this);
            string txt;
            if (m_nId > 0)
            {
                Connect();
                txt = "Create successfully.";
            }
            else
                txt = "Create failed.";
            Console.WriteLine(txt);
        }

        ~DetectorManager()
        {
            Detector.DestroyDetector(m_nId);
            m_nId = 0;
        }

        public void Connect()
        {
            Detector d = Detector.DetectorList[m_nId];
            string txt;
            if (d == null)
            {
                txt = "invalid detector id";
            }
            else
            {
                int nResult = d.Connect();
                txt = (nResult == 0 ? "connecting..." : "connect fail.");
            }
            Console.WriteLine(txt);
        }

        public void MonitorChannelStateChange()
        {
            Task.Factory.StartNew( state =>
            {
                string[] arrSdkState = { "Unknown", "Ready", "Busy", "Sleeping" };
                string[] arrConnState = { "Unknown", "Hardbreak", "NotConnected", "LowRate", "OK" };
                IRayVariant var = new IRayVariant();
                IRayVariant connState = new IRayVariant();

                bool bFlag = false;
                while (true)
                {
                    if (m_nId > 0)
                    {
                        int retCode = SdkInterface.GetAttr(m_nId, SdkInterface.Attr_State, ref var);
                        if (SdkInterface.Err_OK != retCode) var.val.nVal = 0;

                        retCode = SdkInterface.GetAttr(m_nId, SdkInterface.Attr_ConnState, ref connState);
                        if (SdkInterface.Err_OK != retCode) var.val.nVal = 0;
                    }
                    
                    Console.WriteLine(arrSdkState[var.val.nVal]);
                    Console.WriteLine(arrConnState[connState.val.nVal]);
                    // ready
                    m_nStatus = var.val.nVal;

                    Thread.Sleep(1000);
                }
            }, string.Format("client {0}", 0), TaskCreationOptions.LongRunning);
        }

        public void PreAcq(NetMQ.NetMQFrame sender_addr)
        {
            Thread t = new Thread(new ThreadStart(on_recv_5v));
            if(m_nStatus == 1)
            {
                m_nSenderAddr = sender_addr;
                Detector d = Detector.DetectorList[m_nId];
                if (d == null) return;

                int nRet = d.PrepAcquire();

                if (SdkInterface.Err_TaskPending != nRet && SdkInterface.Err_OK != nRet)
                {
                    Console.WriteLine(String.Format("PrepAcquire failed. err:{0}", nRet));
                }
                else
                {   
                    t.Start();
                    Console.WriteLine("PrepAcquire...");
                }
            }
        }
        private int GetEthernetIPNumber()
        {
            NetworkInterface ethernetInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                x.OperationalStatus == OperationalStatus.Up);

            IPAddress ipAddress = ethernetInterface?.GetIPProperties().UnicastAddresses
                .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;

            if (ipAddress != null)
            {
                string[] parts = ipAddress.ToString().Split('.');
                if (parts.Length == 4 && int.TryParse(parts[3], out int ipNumber))
                {
                    return ipNumber;
                }
            }

            return -1;
        }

        private void SendImage(IRayImage image)
        {
            int ipNumber = GetEthernetIPNumber();
            var nWidth = image.nWidth;
            var nHeight = image.nHeight;
            var nBytesPerPixel = image.nBytesPerPixel;
            var nImgSize = nWidth * nHeight * nBytesPerPixel;
            byte[] ImgData = null;

            if ((0 != nImgSize) && (IntPtr.Zero != image.pData))
            {
                ImgData = new byte[nImgSize];

                try
                {
                    Marshal.Copy(image.pData, ImgData, 0, nImgSize);
                }
                catch (Exception)
                {
                    Console.WriteLine("Save Image failed.");
                }
            }

            if (image.propList.nItemCount > 0)
            {
                IRayVariantMapItem[] Params = new IRayVariantMapItem[image.propList.nItemCount];

                SdkParamConvertor<IRayVariantMapItem>.IntPtrToStructArray(image.propList.pItems, ref Params);
                //Params is availabe now.
            }

            var messageToClient = new NetMQMessage();
            messageToClient.Append(m_nSenderAddr);
            messageToClient.AppendEmptyFrame();
            messageToClient.Append("AckImage");
            messageToClient.Append(ipNumber);
            messageToClient.Append(nWidth);
            messageToClient.Append(nHeight);
            messageToClient.Append(nBytesPerPixel);
            messageToClient.Append(ImgData);
            Program.server.SendMultipartMessage(messageToClient);

            return;
        }

        void EventReceiver.SdkCallbackHandler(int nDetectorID, int nEventID, int nEventLevel,
                       IntPtr pszMsg, int nParam1, int nParam2, int nPtrParamLen, IntPtr pParam)
        {
            bool processed = true;

            switch (nEventID)
            {
                case SdkInterface.Evt_TaskResult_Succeed:
                    {
                        switch (nParam1)
                        {
                            case SdkInterface.Cmd_Connect:

                                Console.WriteLine("Connect succeed!");
                                //PreAcq();
                                MonitorChannelStateChange();
                                break;
                            case SdkInterface.Cmd_ReadUserROM:
                                Console.WriteLine("Read ram succeed!");
                                break;
                            case SdkInterface.Cmd_WriteUserROM:
                                Console.WriteLine("Write ram succeed!");
                                break;
                            case SdkInterface.Cmd_Clear:
                                Console.WriteLine("Cmd_Clear Ack succeed");
                                break;
                            case SdkInterface.Cmd_ClearAcq:
                                Console.WriteLine("Cmd_ClearAcq Ack succeed.");
                                break;
                            case SdkInterface.Cmd_StartAcq:
                                Console.WriteLine("Cmd_StartAcq Ack succeed.");
                                break;
                            case SdkInterface.Cmd_StopAcq:
                                Console.WriteLine("Cmd_StopAcq Ack succeed.");
                                break;
                            case SdkInterface.Cmd_ForceSingleAcq:
                                Console.WriteLine("Cmd_ForceSingleAcq Ack succeed.");
                                break;
                            case SdkInterface.Cmd_Disconnect:
                                Console.WriteLine("Cmd_Disconnect Ack succeed.");
                                break;
                            case SdkInterface.Cmd_ReadTemperature:
                                Console.WriteLine("Cmd_ReadTemperature Ack Succeed.");
                                //UpdateTemperature();
                                break;
                            case SdkInterface.Cmd_SetCorrectOption:
                                Console.WriteLine("Cmd_SetCorrectOption Ack Succeed.");
                                break;
                            case SdkInterface.Cmd_SetCaliSubset:
                                Console.WriteLine("Cmd_SetCaliSubset Ack Succeed.");
                                break;
                            default:
                                processed = false;
                                break;
                        }
                    }
                    break;
                case SdkInterface.Evt_TaskResult_Failed:
                    switch (nParam1)
                    {
                        case SdkInterface.Cmd_Connect:
                            {
                                switch (nParam2)
                                {
                                    case SdkInterface.Err_DetectorRespTimeout:
                                        Console.WriteLine("FPD no response!");
                                        break;
                                    case SdkInterface.Err_FPD_Busy:
                                        Console.WriteLine("FPD busy!");
                                        break;
                                    case SdkInterface.Err_ProdInfoMismatch:
                                        Console.WriteLine("Init failed!");
                                        break;
                                    case SdkInterface.Err_ImgChBreak:
                                        Console.WriteLine("Image Chanel isn't ok!");
                                        break;
                                    case SdkInterface.Err_CommDeviceNotFound:
                                        Console.WriteLine("Cannot find device!");
                                        break;
                                    case SdkInterface.Err_CommDeviceOccupied:
                                        Console.WriteLine("Device is beeing occupied!");
                                        break;
                                    case SdkInterface.Err_CommParamNotMatch:
                                        Console.WriteLine("Param error, please check IP address!");
                                        break;
                                    default:
                                        Console.WriteLine("Connect failed!");
                                        break;
                                }
                            }
                            break;
                        case SdkInterface.Cmd_ReadUserROM:
                            Console.WriteLine("Read ram failed!");
                            break;
                        case SdkInterface.Cmd_WriteUserROM:
                            Console.WriteLine("Write ram failed!");
                            break;
                        case SdkInterface.Cmd_StartAcq:
                            Console.WriteLine("Cmd_StartAcq Ack failed.");
                            break;
                        case SdkInterface.Cmd_StopAcq:
                            Console.WriteLine("Cmd_StopAcq Ack failed.");
                            break;
                        case SdkInterface.Cmd_Disconnect:
                            Console.WriteLine("Cmd_Disconnect Ack failed.");
                            break;
                        case SdkInterface.Cmd_ReadTemperature:
                            Console.WriteLine("Cmd_ReadTemperature Ack failed.");
                            break;
                        case SdkInterface.Cmd_SetCorrectOption:
                            Console.WriteLine("Cmd_SetCorrectOption Ack failed.");
                            break;
                        case SdkInterface.Cmd_ClearAcq:
                            Console.WriteLine("Cmd_ClearAcq Ack failed.");
                            break;
                        case SdkInterface.Cmd_SetCaliSubset:
                            Console.WriteLine("Cmd_SetCaliSubset Ack failed.");
                            break;
                        default:
                            processed = false;
                            Console.WriteLine("Failed!");
                            break;
                    }
                    break;

                case SdkInterface.Evt_Exp_Prohibit:
                    Console.WriteLine("Evt_Exp_Prohibit.");
                    break;
                case SdkInterface.Evt_Exp_Enable:
                    Console.WriteLine("Evt_Exp_Enable.");
                    break;
                case SdkInterface.Evt_Image:
                case SdkInterface.Evt_Prev_Image:
                    {
                        Console.WriteLine("Got Image");
                        CurImage = new IRayImage();
                        CurImage = (IRayImage)Marshal.PtrToStructure(pParam, typeof(IRayImage));

                        SendImage(CurImage);
                    }
                    break;
                default:
                    processed = false;
                    break;
            }

            if (!processed)
            {
                string msg = "unprocessed msg:" + nEventID.ToString() + ",nParam1:" + nParam1.ToString();
                Console.WriteLine(msg);
            }

            return;
        }
    }
}

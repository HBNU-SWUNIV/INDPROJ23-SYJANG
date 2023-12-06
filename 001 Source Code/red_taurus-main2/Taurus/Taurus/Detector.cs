using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iDetector;
using System.Runtime.InteropServices;


namespace Taurus
{
    struct AttrResult
    {      
        public int nVal;   
        public float  fVal;
        public String strVal;
    }
    class Detector
    {
        public int DetectorID
        {
            get { return m_nDetectorID; }
        }

        private int m_nDetectorID;
        private SdkInterface.SdkCallbackHandler handler;
        private List<EventReceiver> m_receivers = new List<EventReceiver>();

        public static Dictionary<int, Detector> DetectorList = new Dictionary<int,Detector>();

        void SdkCb(int nDetectorID, int nEventID, int nEventLevel,
            IntPtr pszMsg, int nParam1, int nParam2, int nPtrParamLen, IntPtr pParam)
        {
            m_receivers.ForEach(
                delegate(EventReceiver receiver)
                {
                     receiver.SdkCallbackHandler(nDetectorID, nEventID, nEventLevel, pszMsg, nParam1, nParam2, nPtrParamLen, pParam);
                }
            );
        
        }
        
        public static int CreateDetector(string strWorkDir, EventReceiver r)
        {
            Detector d= new Detector();

            
            int nResult = SdkInterface.Create(strWorkDir, d.handler, ref d.m_nDetectorID);

            if (nResult == SdkInterface.Err_OK)
            {
                DetectorList.Add(d.m_nDetectorID, d);
                d.RegisterHandler(r);
                return d.m_nDetectorID;
            }
            else
            {
                return -1;
            }            
        }

        public static void DestroyDetector(int id)
        {
            Detector d = DetectorList[id];
            if (d != null)
            {
                SdkInterface.Destroy(id);
                DetectorList.Remove(id);
            }
            return;
        }

        public int Connect()
        {
            int nResult = Invoke(SdkInterface.Cmd_Connect);
            if (nResult != SdkInterface.Err_TaskPending && nResult != SdkInterface.Err_OK)
            {
                return 1;
            }

            return 0;
        }
        public int Disconnect()
        {
            return Invoke(SdkInterface.Cmd_Disconnect);
        }
        public int SingleAcquire()
        {
            return Invoke(SdkInterface.Cmd_ForceSingleAcq);
        }
        public int PrepAcquire()
        {
            return Invoke(SdkInterface.Cmd_ClearAcq);
        }
        public int StartAcquire()
        {
            return Invoke(SdkInterface.Cmd_StartAcq);
        }
        public int StopAcquire()
        {
            return Invoke(SdkInterface.Cmd_StopAcq);
        }
        public int ReadTemperature()
        {
            return Invoke(SdkInterface.Cmd_ReadTemperature);
        }
        public int SetCorrectionOption(int nValue)
        {
            return Invoke(SdkInterface.Cmd_SetCorrectOption, nValue);
        }
        public int SetAttr(int nId, int nValue)
        {
            IRayVariant var = new IRayVariant();
            var.vt = IRAY_VAR_TYPE.IVT_INT;
            var.val.nVal = nValue;
            return SdkInterface.SetAttr(m_nDetectorID, nId, ref var);
        }
        public int SetAttr(int nId, float fValue)
        {
            IRayVariant var = new IRayVariant();
            var.vt = IRAY_VAR_TYPE.IVT_FLT;
            var.val.fVal = fValue;
            return SdkInterface.SetAttr(m_nDetectorID, nId, ref var);
        }
        public int SetAttr(int nId, String str)
        {
            IRayVariant var = new IRayVariant();
            var.vt = IRAY_VAR_TYPE.IVT_FLT;
            var.val.strVal = str;
            return SdkInterface.SetAttr(m_nDetectorID, nId, ref var);
        }
        public int GetAttr(int nId, ref AttrResult result)
        {
            IRayVariant var = new IRayVariant();
            
            int nRet = SdkInterface.GetAttr(m_nDetectorID, nId, ref var);

            if (nRet == SdkInterface.Err_OK)
            {
                if (var.vt == IRAY_VAR_TYPE.IVT_INT)
                {
                    result.nVal = var.val.nVal;
                }
                else if (var.vt == IRAY_VAR_TYPE.IVT_FLT)
                {
                    result.fVal = var.val.fVal;
                }
                else if (var.vt == IRAY_VAR_TYPE.IVT_STR)
                {
                    result.strVal = var.val.strVal;
                }
            }
            return nRet;
        }

        public int Invoke(int cmdId)
        {
            return SdkInterface.Invoke(m_nDetectorID, cmdId, null, 0);
        }
        public int Invoke(int cmdId, int nValue)
        {
            IRayCmdParam[] param = new IRayCmdParam[1];
            param[0].pt = IRAY_PARAM_TYPE.IPT_VARIANT;
            param[0].var.vt = IRAY_VAR_TYPE.IVT_INT;
            param[0].var.val.nVal = nValue;
            return SdkInterface.Invoke(m_nDetectorID, cmdId, param, 1);
        }
        public int Invoke(int cmdId, float fValue)
        {
            IRayCmdParam[] param = new IRayCmdParam[1];
            param[0].pt = IRAY_PARAM_TYPE.IPT_VARIANT;
            param[0].var.vt = IRAY_VAR_TYPE.IVT_FLT;
            param[0].var.val.fVal = fValue;
            return SdkInterface.Invoke(m_nDetectorID, cmdId, param, 1);
        }
        public int Invoke(int cmdId, String strValue)
        {
            IRayCmdParam[] param = new IRayCmdParam[1];
            param[0].pt = IRAY_PARAM_TYPE.IPT_VARIANT;
            param[0].var.vt = IRAY_VAR_TYPE.IVT_STR;
            param[0].var.val.strVal = strValue;
            return SdkInterface.Invoke(m_nDetectorID, cmdId, param, 1);
        }

        public String GetErrorInfo(int nErrorCode)
        {
            ErrorInfo info = new ErrorInfo();
            SdkInterface.GetErrInfo(nErrorCode, ref info);
            return info.strDescription;
        }
        public int SetCaliSubSet(String strSubDir)
        {
            return Invoke(SdkInterface.Cmd_SetCaliSubset, strSubDir);
        }

        public void RegisterHandler(EventReceiver r)
        {
            if (r != null && !m_receivers.Contains(r))
            {
                m_receivers.Add(r);
            }

            return;
        }

        public void UnRegisterHandler(EventReceiver r)
        { 
            m_receivers.Remove(r);
            return ;
        }

        public const int OFFSETMASK = (int)(Enm_CorrectOption.Enm_CorrectOp_SW_PreOffset | Enm_CorrectOption.Enm_CorrectOp_SW_PostOffset | Enm_CorrectOption.Enm_CorrectOp_HW_PreOffset | Enm_CorrectOption.Enm_CorrectOp_HW_PostOffset);
        public const int GAINMASK = (int)(Enm_CorrectOption.Enm_CorrectOp_SW_Gain | Enm_CorrectOption.Enm_CorrectOp_HW_Gain);
        public const int DEFECTMASK = (int)(Enm_CorrectOption.Enm_CorrectOp_SW_Defect | Enm_CorrectOption.Enm_CorrectOp_HW_Defect);

        private Detector()
        {
            handler = new SdkInterface.SdkCallbackHandler(SdkCb);
        }
        ~Detector()
        { 
        
        }        
    }
}

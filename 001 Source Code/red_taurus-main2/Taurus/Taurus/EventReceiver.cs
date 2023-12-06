using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iDetector;

namespace Taurus
{
    interface EventReceiver
    {
        void SdkCallbackHandler(int nDetectorID, int nEventID, int nEventLevel,
IntPtr pszMsg, int nParam1, int nParam2, int nPtrParamLen, IntPtr pParam);

    }
}

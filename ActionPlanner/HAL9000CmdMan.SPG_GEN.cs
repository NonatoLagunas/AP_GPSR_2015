using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Robotics.API;
using Robotics.Controls;

namespace ActionPlanner
{
    public partial class HAL9000CmdMan
    {
        #region SPG_GEN Commands 25/03/15
        public void SPG_GEN_say(string strToSay)
        {
            this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say].IsResponseReceived = false;
            this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say].Command.Parameters = strToSay;
            this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say].Command);
        }

        public bool SPG_GEN_say(string strToSay, int timeOut_ms)
        {
            this.SPG_GEN_say(strToSay);
            return this.WaitForResponse(JustinaCommands.SP_GEN_say, timeOut_ms);
        }

        public bool SPG_GEN_asay(string strToSay, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.SP_GEN_asay, strToSay);
            return this.WaitForResponse(JustinaCommands.SP_GEN_asay, timeOut_ms);
        }

        public bool SPG_GEN_aplay(string song, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.SP_GEN_aplay, song);
            return this.WaitForResponse(JustinaCommands.SP_GEN_aplay, timeOut_ms);
        }

        public bool SPG_GEN_shutup(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.SP_GEN_shutup, "");
            return this.WaitForResponse(JustinaCommands.SP_GEN_shutup, timeOut_ms);
        }

        public bool SPG_GEN_voice(string voice, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.SP_GEN_voice, voice);
            return this.WaitForResponse(JustinaCommands.SP_GEN_voice, timeOut_ms);
        }
        #endregion
    }
}

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
        #region LANG_UND Commands 09/07/15
        public bool process_string(string stringToProcess, int timeOut_ms)
        {
            string conceptualDependencyString;
            this.SetupAndSendCommand(JustinaCommands.LANG_UND_processstring, stringToProcess);
            if (!this.WaitForResponse(JustinaCommands.LANG_UND_processstring, timeOut_ms)) return false;
            conceptualDependencyString = this.justinaCmdAndResp[(int)JustinaCommands.LANG_UND_processstring].Response.Parameters;
            return true;
        }
        #endregion
    }
}

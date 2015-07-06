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
        #region BLK Commands 25/03/15
        public bool BLK_alive(string module, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.BLK_alive, module);
            return this.WaitForResponse(JustinaCommands.BLK_alive, timeOut_ms);
        }
        #endregion  
    }
}

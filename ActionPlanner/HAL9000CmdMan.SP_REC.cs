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
        #region SP_REC Commands 25/03/15
        public bool SP_REC_grammar(string grammarFilePath, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.SP_REC_grammar, grammarFilePath);
            return this.WaitForResponse(JustinaCommands.SP_REC_grammar, timeOut_ms);
        }
        #endregion
    }
}

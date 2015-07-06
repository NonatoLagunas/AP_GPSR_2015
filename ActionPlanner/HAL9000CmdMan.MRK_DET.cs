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
        #region MarkerDetector Commands 25/03/15
        public bool VISION_findmarker(out string command, int timeout_ms)
        {
            command = string.Empty;

            this.SetupAndSendCommand(JustinaCommands.VISION_findmarker, "params");
            if (!this.WaitForResponse(JustinaCommands.VISION_findmarker, timeout_ms)) return false;

            try
            {
                command = this.justinaCmdAndResp[(int)JustinaCommands.VISION_findmarker].Response.Parameters;
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findmarker");
                return false;
            }
            return true;
        }

        public bool VISION_findwaving(double headAngle, out double xFall, out double zFall, int timeout_ms)
        {
            xFall = 0;
            zFall = 0;
            this.SetupAndSendCommand(JustinaCommands.VISION_findwaving, headAngle.ToString());
            if (!this.WaitForResponse(JustinaCommands.VISION_findwaving, timeout_ms)) return false;

            try
            {
                char[] delimiters = { ' ' };
                string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.VISION_findwaving].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                double.TryParse(parts[0], out xFall);
                double.TryParse(parts[1], out zFall);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findwaving");
                return false;
            }

            if (xFall == 0 && zFall == 0)
                return false;

            return true;
        }

        public void VISION_findfall(bool start, double headAngle)
        {
            string command;

            if (start)
                command = "start " + ((int)headAngle).ToString();
            //command = "start " + headAngle.ToString();
            else
                command = "stop";

            this.SetupAndSendCommand(JustinaCommands.VISION_findfall, command);
        }
        public bool VISION_findmarkermultiplelanguages(out string language, out string command, int timeout_ms)
        {
            language = string.Empty;
            command = string.Empty;

            this.SetupAndSendCommand(JustinaCommands.VISION_findmarkermultiplelanguages, String.Empty);
            if (!this.WaitForResponse(JustinaCommands.VISION_findmarkermultiplelanguages, timeout_ms)) return false;

            try
            {
                char[] delimiters = { ' ' };
                string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.VISION_findmarkermultiplelanguages].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                language = parts[0];
                if (!language.Equals("0"))
                    command = parts[1];
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findmarkermultiplelanguages");
                return false;
            }
            return true;
        }
        #endregion
    }
}

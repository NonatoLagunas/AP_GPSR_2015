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
	partial class HAL9000CmdMan
    {
        #region HEAD Commands 25/03/15
        public bool HEAD_followskeleton(int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.HEAD_followskeleton, "");
			return this.WaitForResponse(JustinaCommands.HEAD_followskeleton, timeOut_ms);
		}

		public void HEAD_lookat(double pan, double tilt)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Command.Parameters = pan.ToString("0.0000") +
				" " + tilt.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Command);
		}

		public bool HEAD_lookat(double pan, double tilt, int timeOut_ms)
		{
			this.HEAD_lookat(pan, tilt);
			return this.WaitForResponse(JustinaCommands.HEAD_lookat, timeOut_ms);
		}

		public bool HEAD_lookat(out double pan, out double tilt, int timeOut_ms)
		{
			pan = 0;
			tilt = 0;
			this.SetupAndSendCommand(JustinaCommands.HEAD_lookat, "");
			if (!this.WaitForResponse(JustinaCommands.HEAD_lookat, timeOut_ms)) return false;

			char[] delimiters = { ' ' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

			try
			{
				pan = double.Parse(parts[0]);
				tilt = double.Parse(parts[1]);
			}
			catch
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Can't parse response from hd_lookat");
				return false;
			}

			return true;
		}

		public void HEAD_lookatrel(double pan, double tilt)
		{
			this.SetupAndSendCommand(JustinaCommands.HEAD_lookatrel, pan.ToString("0.0000") + " " + tilt.ToString("0.0000"));
		}

		public bool HEAD_lookatrel(double pan, double tilt, int timeout_ms)
		{
			this.HEAD_lookatrel(pan, tilt);
			return this.WaitForResponse(JustinaCommands.HEAD_lookat, timeout_ms);
		}

		public void HEAD_search(string whatToSearch, int timeInSeconds)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search].Command.Parameters = whatToSearch + " " + timeInSeconds.ToString();
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search].Command);
		}

		public void HEAD_show(string emotion)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show].Command.Parameters = emotion;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show].Command);
		}

		public bool HEAD_stopfollowskeleton(int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.HEAD_stopfollowskeleton, "");
			return this.WaitForResponse(JustinaCommands.HEAD_stopfollowskeleton, timeOut_ms);
		}

		public bool HEAD_show(string emotion, int timeOut_ms)
		{
			this.HEAD_show(emotion);
			return this.WaitForResponse(JustinaCommands.HEAD_show, timeOut_ms);
        }
        #endregion
    }
}

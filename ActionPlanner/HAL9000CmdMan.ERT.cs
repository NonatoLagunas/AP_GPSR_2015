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
        #region ERT Commands 25/03/15
        /// <summary>
		/// Request the Emergency Reporting Tool to add a fire mark at the specified position
		/// </summary>
		/// <param name="x">The x coordinate on the map where the fire was located</param>
		/// <param name="y">The y coordinate on the map where the fire was located</param>
		/// <param name="imagePath">The path of an image file which is a picture of the fire source</param>
		/// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
		/// <returns>true if the command executed successfully, false otherwise</returns>
		public bool ERT_addfire(double x, double y, string imagePath, int timeOut_ms)
		{
			// ert_addfire "x y imagepath"
			this.SetupAndSendCommand(JustinaCommands.ERT_addfire, x.ToString("0.00") + " " + y.ToString("0.00") + " " + imagePath);
			return this.WaitForResponse(JustinaCommands.ERT_addfire, timeOut_ms);
		}

		/// <summary>
		/// Request the Emergency Reporting Tool to add a event mark at the specified position
		/// </summary>
		/// <param name="x">The x coordinate on the map where the event was located</param>
		/// <param name="y">The y coordinate on the map where the event was located</param>
		/// <param name="imagePath">The path of an image file which is a picture of the event</param>
		/// <param name="timeout">The maximum amount of time to wait for an execution response</param>
		/// <returns>true if the command executed successfully, false otherwise</returns>
		public bool ERT_addlocation(double x, double y, string imagePath, int timeOut_ms)
		{
			return this.ERT_addlocation(x, y, null, imagePath, timeOut_ms);
		}

		/// <summary>
		/// Request the Emergency Reporting Tool to add a event mark at the specified position
		/// </summary>
		/// <param name="x">The x coordinate on the map where the event was located</param>
		/// <param name="y">The y coordinate on the map where the event was located</param>
		/// <param name="comments">Additional information about the event</param>
		/// <param name="imagePath">The path of an image file which is a picture of the event</param>
		/// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
		/// <returns>true if the command executed successfully, false otherwise</returns>
		public bool ERT_addlocation(double x, double y, string commentaries, string imagePath, int timeOut_ms)
		{
			// ert_addfire "x y \"comments about the location\" imagepath"
			// The comments must be between escaped double quotes and are optional
			commentaries = String.IsNullOrEmpty(commentaries) ? " " : " \\\"" + commentaries + "\\\" ";
			this.SetupAndSendCommand(JustinaCommands.ERT_addlocation, x.ToString("0.00") + " " + y.ToString("0.00") + commentaries + imagePath);
			return this.WaitForResponse(JustinaCommands.ERT_addlocation, timeOut_ms);
		}

		/// <summary>
		/// Request the Emergency Reporting Tool to add a person mark at the specified position
		/// </summary>
		/// <param name="x">The x coordinate on the map where the person was located</param>
		/// <param name="y">The y coordinate on the map where the person was located</param>
		/// <param name="status">The status of the found person (accepted values are { Dead, Injured, Fine, FineAndKnowsExitWay })</param>
		/// <param name="imagePath">The path of an image file which is a picture of the person</param>
		/// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
		/// <returns>true if the command executed successfully, false otherwise</returns>
		public bool ERT_addperson(double x, double y, string status, string imagePath, int timeOut_ms)
		{
			// ert_addfire "x y personStatus imagepath"
			// personStatus = { Dead, Injured, Fine, FineAndKnowsExitWay }
			this.SetupAndSendCommand(JustinaCommands.ERT_addperson, x.ToString("0.00") + " " + y.ToString("0.00") + " " + status + " " + imagePath);
			return this.WaitForResponse(JustinaCommands.ERT_addperson, timeOut_ms);
		}

		/// <summary>
		/// Request the Emergency Reporting Tool to build the report
		/// </summary>
		/// <param name="timeout">The maximum amount of time to wait for an execution response</param>
		/// <returns>true if the command executed successfully, false otherwise</returns>
		public bool ERT_buildreport(int timeOut_ms)
		{
			// ert_buildreport
			// This command does not require parameters
			this.SetupAndSendCommand(JustinaCommands.ERT_buildreport, " ");
			return this.WaitForResponse(JustinaCommands.ERT_buildreport, timeOut_ms);
		}

        /// <summary>
        /// Request the Emergency Reporting Tool to build the report
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait for an execution response</param>
        /// <returns>nothing, async command</returns>
        public bool ERT_buildreport()
        {
            // ert_buildreport
            // This command does not require parameters
            this.SetupAndSendCommand(JustinaCommands.ERT_buildreport, " ");
            return true;
        }
        #endregion
    }
}

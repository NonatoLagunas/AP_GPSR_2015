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
        #region PRS_FND Commands 25/03/15
        /// <summary>
        /// Deletes all known faces from the PersonFinder database
        /// </summary>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_amnesia(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_amnesia, String.Empty);
            return this.WaitForResponse(JustinaCommands.PRS_FND_amnesia, timeOut_ms);
        }

        /// <summary>
        /// Looks for a human with the specified name
        /// </summary>
        /// <param name="humanName">The name of the human to find, use null to find any human</param>
        /// <param name="pan">When this method returns, contains the pan angle relative to the FOV of the face found in the image source being used for face recognition</param>
        /// <param name="tilt">When this method returns, contains the tilt angle relative to the FOV of the face found in the image source being used for face recognition</param>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_findhuman(ref string humanName, out double pan, out double tilt, int timeOut_ms)
        {
            char[] delimiters = { ' ' };

            pan = 0;
            tilt = 0;
            if (String.IsNullOrEmpty(humanName)) humanName = "human";
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_findhuman, humanName);
            if (!this.WaitForResponse(JustinaCommands.PRS_FND_findhuman, timeOut_ms))
                return false;
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_findhuman].Response.Parameters.Split(
                delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return false;
            humanName = parts[0];
            Double.TryParse(parts[1], out pan);
            Double.TryParse(parts[2], out tilt);
            return true;
        }

        /// <summary>
        /// Deletes all known faces with the specified name from the PersonFinder database.
        /// </summary>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_forgethuman(string humanName, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_forgethuman, humanName);
            return this.WaitForResponse(JustinaCommands.PRS_FND_forgethuman, timeOut_ms);
        }

        /// <summary>
        /// Request the list of names of the known persons and the number of trained patterns
        /// </summary>
        /// <param name="names">When this method returns contains a list of key/pair values with the names
        /// of the known persons and the number of trained patterns for each face</param>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_knownnames(out SortedList<string, int> names, int timeOut_ms)
        {
            string name;
            int patterns;
            string parameters;
            char[] delimiters = { ' ' };

            names = null;
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_knownnames, String.Empty);
            if (!this.WaitForResponse(JustinaCommands.PRS_FND_knownnames, timeOut_ms))
                return false;

            parameters = this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_knownnames].Response.Parameters;
            if (String.IsNullOrEmpty(parameters))
                return true;
            string[] parts = parameters.Split(
                delimiters, StringSplitOptions.RemoveEmptyEntries);
            if ((parts.Length % 2) != 0)
                return false;
            names = new SortedList<string, int>(parts.Length / 2);
            for (int i = 0; i < parts.Length; i += 2)
            {
                name = parts[i].Replace("\"", String.Empty);
                if (!Int32.TryParse(parts[i + 1], out patterns))
                    continue;
                if (!names.ContainsKey(name))
                    names.Add(name, patterns);
            }
            return true;
        }

        /// <summary>
        /// Asociates the detected face to a name.
        /// </summary>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_rememberhuman(string humanName, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_rememberhuman, humanName);
            return this.WaitForResponse(JustinaCommands.PRS_FND_rememberhuman, timeOut_ms);
        }

        /// <summary>
        /// Request the Person Finder to use the specified image resolution when capturing images for face recognition.
        /// </summary>
        /// <param name="width">The width of the input image. Values over 800 may increase accuracy but higly reduce performance</param>
        /// <param name="height">The width of the input image. Values over 600 may increase accuracy but higly reduce performance</param>
        /// <param name="fps">The capture framerate in frames per second. Values 30 higly reduce performance</param>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_resolution(int width, int height, int fps, int timeOut_ms)
        {
            string parameters = width.ToString() + "x" + height.ToString() + "@" + fps.ToString();
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_resolution, parameters);
            return this.WaitForResponse(JustinaCommands.PRS_FND_resolution, timeOut_ms);
        }

        /// <summary>
        /// Request the Person Finder application to close
        /// </summary>
        public void PRS_FND_shutdown()
        {
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_shutdown, String.Empty);
        }

        /// <summary>
        /// Enables or disables the PersonFinder capture thread to reduce CPU usage
        /// </summary>
        /// <param name="enable">true to put the thread to sleep, false to awaken it</param>
        public void PRS_FND_sleep(bool enable)
        {
            string parameters = enable ? "enable" : "disable";
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_sleep, parameters);
        }

        /// <summary>
        /// Enables or disables the PersonFinder capture thread to reduce CPU usage
        /// </summary>
        /// <param name="enable">true to put the thread to sleep, false to awaken it</param>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        public bool PRS_FND_sleep(bool enable, int timeOut_ms)
        {
            this.PRS_FND_sleep(enable);
            return this.WaitForResponse(JustinaCommands.PRS_FND_sleep, timeOut_ms);
        }

        /// <summary>
        /// Request the Person Finder to change to the specified input source
        /// </summary>
        /// <param name="sourceName">The name of the image source. Valid values are 'camera', 'kinect', 'pipe' and 'file'</param>
        /// <param name="token">An optional parameter that gives additional information to the source like the camera index, pipe or file name, etc.</param>
        /// <param name="timeOut_ms">The maximum amount of time to wait for an execution response</param>
        /// <returns>true if the command executed successfully, false otherwise</returns>
        public bool PRS_FND_source(string sourceName, string token, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.PRS_FND_source, sourceName + " " + token);
            return this.WaitForResponse(JustinaCommands.PRS_FND_source, timeOut_ms);
        }
        #endregion
    }
}

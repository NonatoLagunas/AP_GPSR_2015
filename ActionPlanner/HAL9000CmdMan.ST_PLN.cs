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
        #region ST_PLN Commands 25/03/15
        public void ST_PLN_alignhuman(string parameters)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_alignhuman, parameters);
        }

        public bool ST_PLN_alignhuman(string parameters, int timeOut_ms)
        {
            this.ST_PLN_alignhuman(parameters);
            return this.WaitForResponse(JustinaCommands.ST_PLN_alignhuman, timeOut_ms);
        }

        public bool ST_PLN_alignedge(double distance, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_alignedge, distance.ToString("0.000"));
            return this.WaitForResponse(JustinaCommands.ST_PLN_alignedge, timeOut_ms);
        }

        public void ST_PLN_cleantable()
        {
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane].IsResponseReceived = false;
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane].Command.Parameters = "";
            this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane].Command);
        }

        public bool ST_PLN_cleantable(int timeOut_ms)
        {
            this.ST_PLN_cleantable();
            return this.WaitForResponse(JustinaCommands.ST_PLN_cleanplane, timeOut_ms);
        }

        public bool ST_PLN_deliverobject(string armUsed, int timeOut_ms)
        {
            if (armUsed != "left" && armUsed != "right")
                armUsed = "left";
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_deliverobject, armUsed);
            return this.WaitForResponse(JustinaCommands.ST_PLN_deliverobject, timeOut_ms);
        }

        public void ST_PLN_dopresentation()
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_dopresentation, "");
        }

        public bool ST_PLN_dopresentation(int timeOut_ms)
        {
            this.ST_PLN_dopresentation();
            return this.WaitForResponse(JustinaCommands.ST_PLN_dopresentation, timeOut_ms);
        }

        public void ST_PLN_drop(string armToUse)
        {
            SetupAndSendCommand(JustinaCommands.ST_PLN_drop, armToUse);
        }

        public bool ST_PLN_drop(string armToUse, int timetOut_ms)
        {
            this.ST_PLN_drop(armToUse);
            return this.WaitForResponse(JustinaCommands.ST_PLN_drop, timetOut_ms);
        }

        public void ST_PLN_findhuman(string humanName, string devices)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_findhuman, humanName + " " + devices);
        }

        public bool ST_PLN_findhuman(string humanName, string devices, int timeOut_ms, out string foundHumanName)
        {
            foundHumanName = "";
            this.ST_PLN_findhuman(humanName, devices);
            if (!this.WaitForResponse(JustinaCommands.ST_PLN_findhuman, timeOut_ms)) return false;
            foundHumanName = this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findhuman].Response.Parameters;
            return true;
        }

        public void ST_PLN_findobject(string objectName, string devices)
        {
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].IsResponseReceived = false;
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].Command.Parameters = devices + " " + objectName;
            this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].Command);
        }

        public bool ST_PLN_fashionfind_object(string objectsToFind, int timeOut_ms, out List<string> foundObjects)
        {

            foundObjects = new List<string>();

            this.SetupAndSendCommand(JustinaCommands.ST_PLN_fashion_findobject, objectsToFind);

            if (!this.WaitForResponse(JustinaCommands.ST_PLN_fashion_findobject, timeOut_ms))
                return false;

            char[] delimiters = { ' ' };
            foundObjects.AddRange(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_fashion_findobject].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
            return true;
        }


        public bool ST_PLN_findobject(string objectName, string devices, int timeOut_ms)
        {
            this.ST_PLN_findobject(objectName, devices);
            return this.WaitForResponse(JustinaCommands.ST_PLN_findobject, timeOut_ms);
        }

        public bool ST_PLN_findobject(string objectName, string devices, int timeOut_ms, out List<string> foundObjects)
        {
            foundObjects = new List<string>();
            this.ST_PLN_findobject(objectName, devices);
            if (!this.WaitForResponse(JustinaCommands.ST_PLN_findobject, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            foundObjects.AddRange(this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
            return true;
        }

        public bool ST_PLN_findobject(string objectsToFind, int timeOut_ms, out List<string> foundObjects)
        {
            return this.ST_PLN_findobject(objectsToFind, "", timeOut_ms, out foundObjects);
        }

        public void ST_PLN_greethuman(string devices)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_greethuman, devices);
        }

        public bool ST_PLN_greethuman(string devices, int timeOut_ms)
        {
            this.ST_PLN_greethuman(devices);
            return this.WaitForResponse(JustinaCommands.ST_PLN_greethuman, timeOut_ms);
        }

        public void ST_PLN_rememberhuman(string humanName, string devices)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_rememberhuman, devices + " " + humanName);
        }

        public bool ST_PLN_rememberhuman(string humanName, string devices, int timeOut_ms)
        {
            this.ST_PLN_rememberhuman(humanName, devices);
            return this.WaitForResponse(JustinaCommands.ST_PLN_rememberhuman, timeOut_ms);
        }

        public void ST_PLN_seehand(string objectToBeRecognized)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_seehand, objectToBeRecognized);
        }

        public bool ST_PLN_seehand(string objectToBeRecognized, int timeOut_ms)
        {
            this.ST_PLN_seehand(objectToBeRecognized);
            return this.WaitForResponse(JustinaCommands.ST_PLN_seehand, timeOut_ms);
        }

        public bool ST_PLN_startlearn(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_startlearn, "");
            return this.WaitForResponse(JustinaCommands.ST_PLN_startlearn, timeOut_ms);
        }

        public bool ST_PLN_stoplearn(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_stoplearn, "");
            return this.WaitForResponse(JustinaCommands.ST_PLN_stoplearn, timeOut_ms);
        }

        public bool ST_PLN_takehandover(string objsToBeHandled, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_takehandover, objsToBeHandled);
            return this.WaitForResponse(JustinaCommands.ST_PLN_takehandover, timeOut_ms);
        }

        public void ST_PLN_takeobject()
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, "");
        }

        public bool ST_PLN_takeobject(int timeOut_ms)
        {
            this.ST_PLN_takeobject();
            return this.WaitForResponse(JustinaCommands.ST_PLN_take, timeOut_ms);
        }

        public void ST_PLN_takeobject(string objectToTake)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, objectToTake);
        }

        public bool ST_PLN_takeobject(string objectToTake, int timeOut_ms)
        {
            this.ST_PLN_takeobject(objectToTake);
            return this.WaitForResponse(JustinaCommands.ST_PLN_take, timeOut_ms);
        }

        public bool ST_PLN_takeobject(string objectToTake, string armToUse, out string leftArm, out string rightArm, int timeOut_ms)
        {
            leftArm = "";
            rightArm = "";
            if (armToUse == "")
                this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, objectToTake);
            else this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, objectToTake + " " + armToUse);

            if (!this.WaitForResponse(JustinaCommands.ST_PLN_take, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_take].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            leftArm = parts[0];
            rightArm = parts[1];
            return true;
        }

        public bool ST_PLN_takeobject(string objectsToTake, out List<string> takenObjects, int timeOut_ms)
        {
            takenObjects = new List<string>();
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_take, objectsToTake);
            if (!this.WaitForResponse(JustinaCommands.ST_PLN_take, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_take].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
                takenObjects.Add(s);
            if (takenObjects.Count < 1) return false;
            return true;
        }

        public bool ST_PLN_taketable(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_taketable, " ");
            return this.WaitForResponse(JustinaCommands.ST_PLN_taketable, timeOut_ms);

        }

        public bool ST_PLN_pointatobject(double x, double y, double z, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_pointatobject, x.ToString("0.000") + " " + y.ToString("0.000") + " " + z.ToString("0.000"));
            return this.WaitForResponse(JustinaCommands.ST_PLN_pointatobject, timeOut_ms);

        }

        public bool ST_PLN_takexyz(double x, double y, double z, out bool isUsingRightArm, int timeout_ms)
        {
            isUsingRightArm = true;
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_takexyz, string.Format("{0} {1} {2}", x, y, z));
            if (!this.WaitForResponse(JustinaCommands.ST_PLN_takexyz, timeout_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_takexyz].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                if (parts[0] != "empty")
                {
                    isUsingRightArm = false;
                    return true;
                }

                if (parts[1] != "empty")
                {
                    isUsingRightArm = true;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public bool ST_PLN_dropbox(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_dropbox, "");
            return this.WaitForResponse(JustinaCommands.ST_PLN_dropbox, timeOut_ms);
        }

        public bool ST_PLN_pickbox(int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_pickbox, "");
            return this.WaitForResponse(JustinaCommands.ST_PLN_pickbox, timeOut_ms);
        }

        public bool ST_PLN_shakehand(string hand, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ST_PLN_shakehand, hand);
            return this.WaitForResponse(JustinaCommands.ST_PLN_shakehand, timeOut_ms);
        }
        #endregion
    }
}

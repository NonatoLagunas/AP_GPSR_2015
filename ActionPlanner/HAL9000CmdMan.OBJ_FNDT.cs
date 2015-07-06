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
        #region OBJ_FNDT Commands 25/03/15
        public bool OBJ_FNDT_find(string objToFind, out double x, out double y, out double z, int timeOut_ms)
        {
            x = 0;
            y = 0;
            z = 0;
            string[] parts;
            char[] delimiters = { ' ' };
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_find, objToFind);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_find, timeOut_ms)) return false;
            parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_find].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                x = double.Parse(parts[0]) / 1000;
                y = double.Parse(parts[1]) / 1000;
                z = double.Parse(parts[2]) / 1000;
            }
            catch { return false; }
            return true;
        }

        public bool OBJ_FNDT_find(string objToFind, out List<string> foudnObj, int timeOut_ms)
        {
            foudnObj = new List<string>();
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_find, objToFind);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_find, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_find].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < parts.Length; i += 4)
                    foudnObj.Add(parts[i]);
            }
            catch { return false; }
            return true;
        }

        public bool OBJ_FNDT_findontable(string objstring, out List<string> foudnObj, int timeOut_ms)
        {
            foudnObj = new List<string>();
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findontable, objstring);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findontable, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findontable].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < parts.Length; i += 4)
                    foudnObj.Add(parts[i]);
            }
            catch { return false; }
            return true;
        }

        public bool OBJ_FNDT_findedgetuned(double angleRads, out double x0, out double y0, out double z0, out double x1,
            out double y1, out double z1, int timeOut_ms)
        {
            x0 = 0;
            y0 = 0;
            z0 = 0;
            x1 = 1;
            y1 = 0;
            z1 = 0;
            //El -3 es correción dada por los de visión para ver bien el plano de la mesa
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgetuned, (angleRads * 180 / Math.PI - 3).ToString("0.00"));
            //Thread.Sleep(1000);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgetuned, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgetuned].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x0 = double.Parse(parts[0]);
                y0 = double.Parse(parts[1]);
                z0 = double.Parse(parts[2]);
                x1 = double.Parse(parts[3]);
                y1 = double.Parse(parts[4]);
                z1 = double.Parse(parts[5]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_findedgereloaded(double angleRads, out double x0, out double y0, out double z0, int timeOut_ms)
        {
            x0 = 0;
            y0 = 0;
            z0 = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgereloaded, (angleRads * 180 / Math.PI - 3).ToString("0.00"));
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgereloaded, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgereloaded].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x0 = double.Parse(parts[0]) / 1000;
                y0 = double.Parse(parts[1]) / 1000;
                z0 = double.Parse(parts[2]) / 1000;

            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgereloaded");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_findgolemhand(double angleRads, out double x0, out double y0, out double z0, int timeOut_ms)
        {
            x0 = 0;
            y0 = 0;
            z0 = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findgolemhand, (angleRads * 180 / Math.PI - 3).ToString("0.00"));
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findgolemhand, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findgolemhand].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x0 = double.Parse(parts[0]);
                y0 = double.Parse(parts[1]);
                z0 = double.Parse(parts[2]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgereloaded");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_findedge(double angleRads, out double x0, out double y0, out double z0, out double x1,
            out double y1, out double z1, int timeOut_ms)
        {
            x0 = 0;
            y0 = 0;
            z0 = 0;
            x1 = 1;
            y1 = 0;
            z1 = 0;
            //El -3 es correción dada por los de visión para ver bien el plano de la mesa
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedge, (angleRads * 180 / Math.PI - 3).ToString("0.00"));
            //Thread.Sleep(1000);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedge, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedge].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x0 = double.Parse(parts[0]) / 1000;
                y0 = double.Parse(parts[1]) / 1000;
                z0 = double.Parse(parts[2]) / 1000;
                x1 = double.Parse(parts[3]) / 1000;
                y1 = double.Parse(parts[4]) / 1000;
                z1 = double.Parse(parts[5]) / 1000;
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedge");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_findhandgrip(double angleRads, out double posx, out double posy, out double posz, int timeOut_ms)
        {
            posx = 0;
            posy = 0;
            posz = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findhandgrip, (angleRads * 180 / Math.PI).ToString("0.0000"));
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findhandgrip, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findhandgrip].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                posx = double.Parse(parts[0]);
                posy = double.Parse(parts[1]);
                posz = double.Parse(parts[2]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000CmdMan: Cannot parse response from " + JustinaCommands.OBJ_FNDT_findhandgrip);
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_sethumantracker(string name, string options, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_sethumantracker, name + " " + options);
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_sethumantracker, timeOut_ms);
        }

        public bool OBJ_FNDT_findandtrackhuman(string name, int timeOut)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findandtrackhuman, name);
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_findandtrackhuman, timeOut);
        }

        public bool OBJ_FNDT_locatehuman(out double x, out double y, out double z, int timeOut)
        {
            x = 0;
            y = 0; z = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_locatehuman, "");
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_locatehuman, timeOut)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_locatehuman].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                x = double.Parse(parts[0]);
                y = double.Parse(parts[1]);
                z = double.Parse(parts[2]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000CmdMan.-> Cannot parse response from oft_locatehuman");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_tracking(string options, string name, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_tracking, options + " " + name);
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_tracking, timeOut_ms);
        }

        public bool OBJ_FNDT_findhuman(string name, out double x, out double y, out double z, int timeOut_ms)
        {
            x = 0; y = 0; z = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findhuman, name);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findhuman, timeOut_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findhuman].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                x = double.Parse(parts[1]);
                y = double.Parse(parts[2]);
                z = double.Parse(parts[3]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000CmdMan.-> Cannot parse response from oft_findhuman");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_remember(int frames, int timeout_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_remember, frames.ToString());
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_remember, timeout_ms);

        }

        public bool OBJ_FNDT_trainshirt(string name, int timeout_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_trainshirt, name);
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_trainshirt, timeout_ms);
        }

        public bool OBJ_FNDT_testshirt(string name, out double x, out double y, out double z, int timeout_ms)
        {
            string responseName;
            x = 0; y = 0; z = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_testshirt, "");
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_testshirt, timeout_ms)) return false;
            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_testshirt].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                responseName = parts[0];
                if (!responseName.Substring(0, responseName.IndexOf('_')).Equals(name))
                    return false;
                x = double.Parse(parts[1]);
                y = double.Parse(parts[2]);
                z = double.Parse(parts[3]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000CmdMan.-> Cannot parse response from oft_testshirt");
                return false;
            }
            return true;

        }

        public bool OBJ_FNDT_detectsmoke(int frames, int timeout_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_detectsmoke, frames.ToString());
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_detectsmoke, timeout_ms);
        }

        public bool OBJ_FNDT_takepicture(string path, int timeout_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_takepicture, path);
            return this.WaitForResponse(JustinaCommands.OBJ_FNDT_takepicture, timeout_ms);
        }

        public bool OBJ_FNDT_findedgereturns(double angleRads, out double x0, out double y0, out double z0, out double x1, out double y1, out double z1, int timeOut_ms)
        {
            x0 = 0;
            y0 = 0;
            z0 = 0;
            x1 = 1;
            y1 = 0;
            z1 = 0;
            //El -3 es correción dada por los de visión para ver bien el plano de la mesa
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgereturns, (angleRads * 180 / Math.PI).ToString("0.00"));
            //Thread.Sleep(1000);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgereturns, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgereturns].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x0 = double.Parse(parts[0]) / 1000;
                y0 = double.Parse(parts[1]) / 1000;
                z0 = double.Parse(parts[2]) / 1000;
                x1 = double.Parse(parts[3]) / 1000;
                y1 = double.Parse(parts[4]) / 1000;
                z1 = double.Parse(parts[5]) / 1000;
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_findedgefastandfurious(double angleRads, double height, out double x0, out double y0, out double z0, out double x1, out double y1, out double z1, int timeOut_ms)
        {
            x0 = 0;
            y0 = 0;
            z0 = 0;
            x1 = 1;
            y1 = 0;
            z1 = 0;
            //El -3 es correción dada por los de visión para ver bien el plano de la mesa
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findedgefastandfurious, (angleRads * 180 / Math.PI - 3).ToString("0.00") + " " + (height).ToString("0.00"));
            //Thread.Sleep(1000);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findedgefastandfurious, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgefastandfurious].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x0 = double.Parse(parts[0]) / 1000;
                y0 = double.Parse(parts[1]) / 1000;
                z0 = double.Parse(parts[2]) / 1000;
                x1 = double.Parse(parts[3]) / 1000;
                y1 = double.Parse(parts[4]) / 1000;
                z1 = double.Parse(parts[5]) / 1000;
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_gestodi(out int dir, int timeout_ms)
        {
            dir = 0;
            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_gestodi, string.Empty);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_gestodi, timeout_ms)) return false;

            try
            {
                dir = int.Parse(this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_gestodi].Response.Parameters);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findedgetuned");
                return false;
            }
            return true;
        }

        public bool OBJ_FNDT_findpaddle(out string command, int timeout_ms)
        {
            command = "";

            this.SetupAndSendCommand(JustinaCommands.OBJ_FNDT_findpaddle, string.Empty);
            if (!this.WaitForResponse(JustinaCommands.OBJ_FNDT_findpaddle, timeout_ms)) return false;

            try
            {
                command = this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findpaddle].Response.Parameters;
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from oft_findpaddle");
                return false;
            }
            return true;
        }
        #endregion
    }
}

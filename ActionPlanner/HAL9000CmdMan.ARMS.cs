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
        #region ARMS Commands 25/03/15
        public void ARMS_ra_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
        {
            string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") +
                " " + roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") +
                " " + elbow.ToString("0.00");
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_abspos, parameters);
        }

        public bool ARMS_ra_abspos(double x, double y, double z, double roll, double pitch, double yaw,
            double elbow, int timeOut_ms)
        {
            this.ARMS_ra_abspos(x, y, z, roll, pitch, yaw, elbow);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_abspos, timeOut_ms);
        }

        public bool ARMS_ra_abspos(out double x, out double y, out double z, out double roll, out double pitch,
            out double yaw, out double elbow, int timeOut_ms)
        {
            x = 0;
            y = 0;
            z = 0;
            roll = 0;
            pitch = 0;
            yaw = 0;
            elbow = 0;
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_abspos, "");
            if (!this.WaitForResponse(JustinaCommands.ARMS_ra_abspos, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_abspos].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x = double.Parse(parts[0]);
                y = double.Parse(parts[1]);
                z = double.Parse(parts[2]);
                roll = double.Parse(parts[3]);
                pitch = double.Parse(parts[4]);
                yaw = double.Parse(parts[5]);
                elbow = double.Parse(parts[6]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000CmdMan: Can't parse response from ra_abspos");
                return false;
            }
            return true;
        }

        public void ARMS_ra_artpos(double q1, double q2, double q3, double q4, double q5, double q6, double q7)
        {
            string parameters = q1.ToString("0.000") + " " + q2.ToString("0.000") + " " + q3.ToString("0.000") + " " +
                q4.ToString("0.000") + " " + q5.ToString("0.000") + " " + q6.ToString("0.000") + " " + q7.ToString("0.000");
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_artpos, parameters);
        }

        public bool ARMS_ra_artpos(double q1, double q2, double q3, double q4, double q5, double q6, double q7, int timeOut_ms)
        {
            this.ARMS_ra_artpos(q1, q2, q3, q4, q5, q6, q7);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_artpos, timeOut_ms);
        }

        public bool ARMS_ra_artpos(out double p0, out double p1, out double p2, out double p3, out double p4, out double p5, out double p6, int timeOut_ms)
        {
            p0 = 0;
            p1 = 0;
            p2 = 0;
            p3 = 0;
            p4 = 0;
            p5 = 0;
            p6 = 0;

            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_artpos, " ");
            if (!this.WaitForResponse(JustinaCommands.ARMS_ra_artpos, timeOut_ms)) return false;
            char[] delimiters = { ' ', '\t' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_artpos].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 7) return false;
            try
            {
                p0 = double.Parse(parts[0]);
                p1 = double.Parse(parts[1]);
                p2 = double.Parse(parts[2]);
                p3 = double.Parse(parts[3]);
                p4 = double.Parse(parts[4]);
                p5 = double.Parse(parts[5]);
                p6 = double.Parse(parts[6]);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void ARMS_ra_goto(string predefinedPosition)
        {
            if (predefinedPosition == "navigation")
                ARMS_ra_closegrip(3000);
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_goto, predefinedPosition);
            //this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto].IsResponseReceived = false;
            //this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto].Command.Parameters = predefinedPosition;
            //this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto].Command);
        }

        public bool ARMS_ra_goto(string predefinedPosition, int timeOut_ms)
        {
            this.ARMS_ra_goto(predefinedPosition);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_goto, timeOut_ms);
        }

        public void ARMS_goto(string predefinedPosition)
        {
            if (predefinedPosition == "navigation")
            {
                ARMS_la_closegrip(3000);
                ARMS_ra_closegrip(3000);
            }
            this.SetupAndSendCommand(JustinaCommands.ARMS_goto, predefinedPosition);

        }

        public bool ARMS_goto(string predefinedPosition, int timeOut_ms)
        {
            this.ARMS_goto(predefinedPosition);
            return this.WaitForResponse(JustinaCommands.ARMS_goto, timeOut_ms);
        }

        public void ARMS_ra_move(string predefinedMovement)
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_move, predefinedMovement);
            //this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move].IsResponseReceived = false;
            //this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move].Command.Parameters = predefinedMovement;
            //this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move].Command);
        }

        public bool ARMS_ra_move(string predefinedMovement, int timeOut_ms)
        {
            this.ARMS_ra_move(predefinedMovement);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_move, timeOut_ms);
        }

        public void ARMS_la_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow)
        {
            string parameters = x.ToString("0.00") + " " + y.ToString("0.00") + " " + z.ToString("0.00") +
                " " + roll.ToString("0.00") + " " + pitch.ToString("0.00") + " " + yaw.ToString("0.00") +
                " " + elbow.ToString("0.00");
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_abspos, parameters);
        }

        public bool ARMS_la_abspos(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
        {
            this.ARMS_la_abspos(x, y, z, roll, pitch, yaw, elbow);
            return this.WaitForResponse(JustinaCommands.ARMS_la_abspos, timeOut_ms);
        }

        public bool ARMS_la_abspos(out double x, out double y, out double z, out double roll, out double pitch,
            out double yaw, out double elbow, int timeOut_ms)
        {
            x = 0;
            y = 0;
            z = 0;
            roll = 0;
            pitch = 0;
            yaw = 0;
            elbow = 0;
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_abspos, "");
            if (!this.WaitForResponse(JustinaCommands.ARMS_la_abspos, timeOut_ms)) return false;

            char[] delimiters = { ' ' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_abspos].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                x = double.Parse(parts[0]);
                y = double.Parse(parts[1]);
                z = double.Parse(parts[2]);
                roll = double.Parse(parts[3]);
                pitch = double.Parse(parts[4]);
                yaw = double.Parse(parts[5]);
                elbow = double.Parse(parts[6]);
            }
            catch
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000CmdMan: Can't parse response from la_abspos");
                return false;
            }
            return true;
        }

        public void ARMS_la_artpos(double q1, double q2, double q3, double q4, double q5, double q6, double q7)
        {
            string parameters = q1.ToString("0.000") + " " + q2.ToString("0.000") + " " + q3.ToString("0.000") + " " +
                q4.ToString("0.000") + " " + q5.ToString("0.000") + " " + q6.ToString("0.000") + " " + q7.ToString("0.000");
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_artpos, parameters);
        }

        public bool ARMS_la_artpos(double q1, double q2, double q3, double q4, double q5, double q6, double q7, int timeOut_ms)
        {
            this.ARMS_la_artpos(q1, q2, q3, q4, q5, q6, q7);
            return this.WaitForResponse(JustinaCommands.ARMS_la_artpos, timeOut_ms);
        }

        public bool ARMS_la_artpos(out double p0, out double p1, out double p2, out double p3, out double p4, out double p5, out double p6, int timeOut_ms)
        {
            p0 = 0;
            p1 = 0;
            p2 = 0;
            p3 = 0;
            p4 = 0;
            p5 = 0;
            p6 = 0;

            this.SetupAndSendCommand(JustinaCommands.ARMS_la_artpos, "");

            if (!this.WaitForResponse(JustinaCommands.ARMS_la_artpos, timeOut_ms)) return false;
            char[] delimiters = { ' ', '\t' };
            string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_artpos].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 7) return false;
            try
            {
                p0 = double.Parse(parts[0]);
                p1 = double.Parse(parts[1]);
                p2 = double.Parse(parts[2]);
                p3 = double.Parse(parts[3]);
                p4 = double.Parse(parts[4]);
                p5 = double.Parse(parts[5]);
                p6 = double.Parse(parts[6]);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void ARMS_la_goto(string predefinedPosition)
        {
            if (predefinedPosition == "navigation")
                ARMS_la_closegrip(3000);

            this.SetupAndSendCommand(JustinaCommands.ARMS_la_goto, predefinedPosition);
        }

        public bool ARMS_la_goto(string predefinedPosition, int timeOut_ms)
        {
            this.ARMS_la_goto(predefinedPosition);
            return this.WaitForResponse(JustinaCommands.ARMS_la_goto, timeOut_ms);
        }

        public void ARMS_la_move(string predefinedMovement)
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_move, predefinedMovement);
        }

        public bool ARMS_la_move(string predefinedMovement, int timeOut_ms)
        {
            this.ARMS_la_move(predefinedMovement);
            return this.WaitForResponse(JustinaCommands.ARMS_la_move, timeOut_ms);
        }

        public void ARMS_ra_closegrip()
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_closegrip, "");
        }

        public bool ARMS_ra_closegrip(int timeOut_ms)
        {
            this.ARMS_ra_closegrip();
            return this.WaitForResponse(JustinaCommands.ARMS_ra_closegrip, timeOut_ms);
        }

        public void ARMS_ra_opengrip()
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_opengrip, "");
        }

        public bool ARMS_ra_opengrip(int timeOut_ms)
        {
            this.ARMS_ra_opengrip();
            return this.WaitForResponse(JustinaCommands.ARMS_ra_opengrip, timeOut_ms);
        }

        public bool ARMS_ra_opengrip(int percentage, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_opengrip, percentage.ToString());
            return this.WaitForResponse(JustinaCommands.ARMS_ra_opengrip, timeOut_ms);
        }

        public void ARMS_la_closegrip()
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_closegrip, "");
        }

        public bool ARMS_la_closegrip(int timeOut_ms)
        {
            this.ARMS_la_closegrip();
            return this.WaitForResponse(JustinaCommands.ARMS_la_closegrip, timeOut_ms);
        }

        public void ARMS_la_opengrip()
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_opengrip, "");
        }

        public bool ARMS_la_opengrip(int timeOut_ms)
        {
            this.ARMS_la_opengrip();
            return this.WaitForResponse(JustinaCommands.ARMS_la_opengrip, timeOut_ms);
        }

        public bool ARMS_la_opengrip(int percentage, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_la_opengrip, percentage.ToString());
            return this.WaitForResponse(JustinaCommands.ARMS_la_opengrip, timeOut_ms);
        }

        public bool ARMS_ra_reachable(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
        {
            string parameters = x.ToString("0.0000") + " " + y.ToString("0.0000") + " " + z.ToString("0.0000") +
                " " + roll.ToString("0.0000") + " " + pitch.ToString("0.0000") + " " + yaw.ToString("0.0000") +
                " " + elbow.ToString("0.0000");
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_reachable, parameters);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_reachable, timeOut_ms);
        }

        public bool ARMS_la_reachable(double x, double y, double z, double roll, double pitch, double yaw, double elbow, int timeOut_ms)
        {
            string parameters = x.ToString("0.0000") + " " + y.ToString("0.0000") + " " + z.ToString("0.0000") +
                " " + roll.ToString("0.0000") + " " + pitch.ToString("0.0000") + " " + yaw.ToString("0.0000") +
                " " + elbow.ToString("0.0000");
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_reachable, parameters);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_reachable, timeOut_ms);
        }
        public bool ARMS_ra_hand_move(string position, int timeOut_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.ARMS_ra_hand_move, position);
            return this.WaitForResponse(JustinaCommands.ARMS_ra_hand_move, timeOut_ms);
        }
        #endregion
    }
}
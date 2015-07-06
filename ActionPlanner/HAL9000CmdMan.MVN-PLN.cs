using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Robotics.API;
using Robotics.Controls;
using Robotics.Mathematics;

namespace ActionPlanner
{
	partial class HAL9000CmdMan
    {
        #region MVN_PLN Commands 25/03/15
        public bool MVN_PLN_calculatepath(double x, double y, out double[] nodes, int timeOut_ms)
		{
			nodes = null;
			string[] nodesString;
			char[] delimiters = { ' ' };
			string parameters = x.ToString() + " " + y.ToString();
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_calculatepath, parameters);
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_calculatepath, timeOut_ms)) return false;
			nodesString = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_calculatepath].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			if (!this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_calculatepath].Response.Success)
				return false;
			try
			{
				nodes = new double[nodesString.Length];
				for (int i = 0; i < nodesString.Length; ++i)
					nodes[i] = double.Parse(nodesString[i]);
			}
			catch { return false; }
			return true;

		}

		public bool MVN_PLN_fixhuman(string id, int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_fixhuman, id);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_fixhuman, timeOut_ms);
		}

		public void MVN_PLN_getclose()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].Command.Parameters = UseFloorLaser ? " true" : "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose].Command);
		}

		public bool MVN_PLN_getclose(int timeOut_ms)
		{
			this.MVN_PLN_getclose();
			return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
		}

		public void MVN_PLN_getclose(string location)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, location + (UseFloorLaser ? " true" : ""));
		}

		public bool MVN_PLN_getclose(string location, int timeOut_ms)
		{
			if (this.UseLocalization)
			{
				this.MVN_PLN_getclose(location);
				this.LocalizeWithVisionWhileMoving();
				return this.JustinaCmdAndResps[(int)JustinaCommands.MVN_PLN_getclose].Response.Success;
			}
			else
			{
				this.MVN_PLN_getclose(location);
				return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
			}
		}

		public bool MVN_PLN_getclose(double x, double y, int timeOut_ms)
		{
			string parameters = x.ToString() + " " + y.ToString() + (UseFloorLaser ? " true" : "");

			if (this.UseLocalization)
			{
				this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, parameters);
				this.LocalizeWithVisionWhileMoving();
				return this.JustinaCmdAndResps[(int)JustinaCommands.MVN_PLN_getclose].Response.Success;
			}
			else
			{
				this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, parameters);
				return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
			}
		}
        public bool MVN_PLN_getclose(double x, double y, double angle, int timeOut_ms)
        {
            string parameters = x.ToString() + " " + y.ToString() + " " + angle.ToString();// +(UseFloorLaser ? " true" : "");

            if (this.UseLocalization)
            {
                this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, parameters);
                this.LocalizeWithVisionWhileMoving();
                return this.JustinaCmdAndResps[(int)JustinaCommands.MVN_PLN_getclose].Response.Success;
            }
            else
            {
                this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, parameters);
                return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
            }
        }
		public bool MVN_PLN_getclosef(double x, double y, int timeOut_ms)
		{
			string parameters = x.ToString() + " " + y.ToString() + (UseFloorLaser ? " true" : "");

			if (this.UseLocalization)
			{
				this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclosef, parameters);
				this.LocalizeWithVisionWhileMoving();
				return this.JustinaCmdAndResps[(int)JustinaCommands.MVN_PLN_getclosef].Response.Success;
			}
			else
			{
				this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclosef, parameters);
				return this.WaitForResponse(JustinaCommands.MVN_PLN_getclosef, timeOut_ms);
			}
		}

		private void LocalizeWithVisionWhileMoving()
		{
			double headPan = -1.5708;
			double headTilt = -1.1;
			int headCounter = 0;

			Vector3 P0 = new Vector3();
			Vector3 P1 = new Vector3();
			double x0, y0, z0, x1, y1, z1;

			while (!this.IsResponseReceived(JustinaCommands.MVN_PLN_getclose))
			{
				if (this.InGoodRegionForLoc != 0)
				{
					if (++headCounter == 1)
					{
						if (this.NearObjForLocDirection > 1.5708) this.NearObjForLocDirection = 1.5708;
						if (this.NearObjForLocDirection < -1.5708) this.NearObjForLocDirection = -1.5708;
						headPan = this.NearObjForLocDirection;
						this.HEAD_lookat(headPan, headTilt, 2000);
						Thread.Sleep(200);
					}
					else if (headCounter == 100)
					{
						headPan = 0;
						this.HEAD_lookat(headPan, headTilt, 2000);
						Thread.Sleep(500);
						headCounter = 0;
					}
					else if (headCounter % 5 == 0)
					{
						if (this.NearObjForLocDirection > 1.5708) this.NearObjForLocDirection = 1.5708;
						if (this.NearObjForLocDirection < -1.5708) this.NearObjForLocDirection = -1.5708;
						headPan = this.NearObjForLocDirection;
						this.HEAD_lookat(headPan, headTilt, 2000);
						Thread.Sleep(100);
					}
					if (!this.OBJ_FNDT_findedgetuned(headTilt, out x0, out y0, out z0, out x1, out y1,
						out z1, 2000))
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot get line with vision module");
					else
					{
						P0 = new Vector3(x0, y0, z0);
						P1 = new Vector3(x1, y1, z1);
						P0 = this.TransHeadToRobot(P0, headPan);
						P1 = this.TransHeadToRobot(P1, headPan);
						if (!this.MVN_PLN_updatelinelmarks(P0, P1, 1000))
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> No response from mvn-pln for updating lmarks");
					}
				}
				else headCounter = 0;
			}

			this.HEAD_lookat(0, 0, 5000);
		}

		private Vector3 TransHeadToRobot(Vector3 vectorInKinectCoords, double headPan)
		{
			//Transforms a coordinate in the kinect system to robot system considering only the head pan. 
			//Vision returns the coordinates considering the kinect in an horizontal position, i.e.
			//it corrects the head tilt using the kinect accelerometers.
			Vector3 temp = new Vector3(vectorInKinectCoords.Z, -vectorInKinectCoords.X, vectorInKinectCoords.Y);
			//The order Z,-X, Y is to transform from kinect system to head system

			//1.75 is the Jutina's height
			//0.1 is the horizontal distance between Justinas kinematic center (point between tire axis)
			// and the front of kinect
			HomogeneousM R = new HomogeneousM(headPan, 1.75, 0.1, 0);
			return R.Transform(temp);
		}

		public bool MVN_PLN_getclose(double x0, double y0, double z0, double x1, double y1, double z1, int timeOut_ms)
		{
			string parameters = "line " + x0.ToString("0.000") + " " + y0.ToString("0.000") + " " +
				z0.ToString("0.000") + " " + x1.ToString("0.000") + " " + y1.ToString("0.000") + " " + z1.ToString("0.000");
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_getclose, parameters);

			return this.WaitForResponse(JustinaCommands.MVN_PLN_getclose, timeOut_ms);
		}
		public void MVN_PLN_goto(string goalRoom)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = goalRoom;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public bool MVN_PLN_goto(string goalRoom, int timeOut_ms)
		{
			this.MVN_PLN_goto(goalRoom);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_goto, timeOut_ms);
		}

		public void MVN_PLN_goto(RegionType regionType, string goalRegion)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = regionType.ToString().ToLower() + " " + goalRegion;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public bool MVN_PLN_goto(RegionType regionType, string goalRegion, int timeOut_ms)
		{
			this.MVN_PLN_goto(regionType, goalRegion);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_goto, timeOut_ms);
		}

		public void MVN_PLN_goto(double x, double y, double angle)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = "xy" + " " +
				x.ToString("0.0000") + " " + y.ToString("0.0000") + " " + angle.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public void MVN_PLN_goto(double x, double y)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command.Parameters = "xy" + " " +
				x.ToString("0.0000") + " " + y.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto].Command);
		}

		public void MVN_PLN_move(double distance, double angle, double time)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command.Parameters = distance.ToString("0.0000") +
				" " + angle.ToString("0.0000") + " " + time.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command);
		}

		public bool MVN_PLN_move(double distance, double angle, double time, int timeOut_ms)
		{
			this.MVN_PLN_move(distance, angle, time);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms);
		}

		public void MVN_PLN_move(double distance, double angle)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command.Parameters = distance.ToString("0.0000") +
				" " + angle.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command);
		}

		public bool MVN_PLN_move(double distance, double angle, int timeOut_ms)
		{
			this.MVN_PLN_move(distance, angle);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms);
		}

		public bool MVN_PLN_move(double distance, double angle, out double x, out double y, int timeOut_ms)
		{
			x = 0;
			y = 0;
			string[] parts;
			char[] delimiters = { ' ' };
			this.MVN_PLN_move(distance, angle);
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms)) return false;

			parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				x = double.Parse(parts[0]);
				y = double.Parse(parts[1]);
			}
			catch { return false; }
			return true;
		}

		public void MVN_PLN_move(double distance)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command.Parameters = distance.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move].Command);
		}

		public bool MVN_PLN_move(double distance, int timeOut_ms)
		{
			this.MVN_PLN_move(distance);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_move, timeOut_ms);
		}

		public void MVN_PLN_obstacle(string options)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_obstacle, options);
		}

		public bool MVN_PLN_obstacle(string options, int timeOut_ms)
		{
			this.MVN_PLN_obstacle(options);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_obstacle, timeOut_ms);
		}

		public void MVN_PLN_position()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command);
		}

		public void MVN_PLN_position(double x, double y, double angle)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command.Parameters =
				x.ToString("0.0000") + " " + y.ToString("0.0000") + " " + angle.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command);
		}

		public bool MVN_PLN_position(double x, double y, int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_position, x.ToString("0.000") + " " + y.ToString("0.000"));
			return this.WaitForResponse(JustinaCommands.MVN_PLN_position, timeOut_ms);
		}

		public void MVN_PLN_position(double x, double y)
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command.Parameters =
				x.ToString("0.0000") + " " + y.ToString("0.0000");
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Command);
		}

		public bool MVN_PLN_position(out double x, out double y, out double angle, int timeOut_ms)
		{
			x = 0;
			y = 0;
			angle = 0;
			string[] parts;
			char[] delimiters = { ' ' };
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_position, "");
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_position, timeOut_ms)) return false;
			parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				x = double.Parse(parts[0]);
				y = double.Parse(parts[1]);
				angle = double.Parse(parts[2]);
			}
			catch { return false; }
			return true;
		}

		public bool MVN_PLN_position(string location, out double x, out double y, out double angle, int timeOut_ms)
		{
			x = 0;
			y = 0;
			angle = 0.0;
			string[] parts;
			char[] delimiters = { ' ' };
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_position, location);
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_position, timeOut_ms)) return false;
			parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			if (!this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position].Response.Success)
				return false;
			try
			{
				x = double.Parse(parts[1]);
				y = double.Parse(parts[2]);
				angle = double.Parse(parts[3]);
			}
			catch { return false; }
			return true;
		}

		public void MVN_PLN_report()
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_report, "");
		}

		public bool MVN_PLN_report(int timeOut_ms)
		{
			this.MVN_PLN_report();
			return this.WaitForResponse(JustinaCommands.MVN_PLN_report, timeOut_ms);
		}

		public void MVN_PLN_selfterminate()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_selfterminate].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_selfterminate].Command.Parameters = "";
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_selfterminate].Command);
			this.TerminateProcess("MotionPlannerRevolutions.exe", 1000);
		}

		public bool MVN_PLN_sethumanpos(double x, double y, int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_sethumanpos, x.ToString("0.000") + " " +
				y.ToString("0.000"));
			return this.WaitForResponse(JustinaCommands.MVN_PLN_sethumanpos, timeOut_ms);
		}

		public void MVN_PLN_setspeeds(byte ls, byte rs)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_setspeeds, ls.ToString() + " " + rs.ToString());
		}

		public bool MVN_PLN_setspeeds(byte ls, byte rs, int timeOut_ms)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_setspeeds, ls.ToString() + " " + rs.ToString());
			return this.WaitForResponse(JustinaCommands.MVN_PLN_setspeeds, timeOut_ms);
		}

		public void MVN_PLN_startfollowhuman()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startfollowhuman].IsResponseReceived = false;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startfollowhuman].Command);
		}

		public void MVN_PLN_startfollowhuman(string personToFollow)
		{
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_startfollowhuman, personToFollow);
		}

		public void MVN_PLN_stopfollowhuman()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopfollowhuman].IsResponseReceived = false;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopfollowhuman].Command);
		}

		public bool MVN_PLN_distancetogoal(string location, out double distance, int timeout_ms)
		{
			distance = double.MaxValue;
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_distancetogoal, location);
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_distancetogoal, timeout_ms)) return false;

			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_distancetogoal].Response.Parameters.Split();

			try
			{ distance = double.Parse(parts[0]); }
			catch
			{ return false; }

			return true;
		}

		public bool MVN_PLN_distancetogoal(double x, double y, out double distance, int timeout_ms)
		{
			distance = double.MaxValue;
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_distancetogoal, x.ToString() + " " + y.ToString());

			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_distancetogoal, timeout_ms)) return false;
			if (!this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_distancetogoal].Response.Success) return false;

			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_distancetogoal].Response.Parameters.Split();

			try
			{ distance = double.Parse(parts[0]); }
			catch
			{ return false; }

			return true;
		}

		public bool MVN_PLN_updatelinelmarks(double x0, double y0, double z0, double x1, double y1, double z1, int timeOut_ms)
		{
			string parameters = x0.ToString("0.000") + " " + y0.ToString("0.000") + " " + z0.ToString("0.000") + " " +
				x1.ToString("0.000") + " " + y1.ToString("0.000") + " " + z1.ToString("0.000");
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_updatelinelmarks, parameters);
			return this.WaitForResponse(JustinaCommands.MVN_PLN_updatelinelmarks, timeOut_ms);
		}

		public bool MVN_PLN_updatelinelmarks(Vector3 P0, Vector3 P1, int timeOut_ms)
		{
			return this.MVN_PLN_updatelinelmarks(P0.X, P0.Y, P0.Z, P1.X, P1.Y, P1.Z, timeOut_ms);
		}

		public bool MVN_PLN_findlegs(string options, out List<Vector3> piernas, int timeOut_ms)
		{
			piernas = new List<Vector3>();
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_findlegs, options);
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_findlegs, timeOut_ms)) return false;
			char[] delimiters = { ' ' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_findlegs].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				for (int i = 0; i < parts.Length; i += 3)
				{
					Vector3 pi = new Vector3();
					pi.X = double.Parse(parts[i]);
					pi.Y = double.Parse(parts[i + 1]);
					pi.Z = double.Parse(parts[i + 2]);
					piernas.Add(pi);
				}
			}
			catch { return false; }
			return true;
		}
		public bool MVN_PLN_infolaser(int options, double anguloMenor, double anguloMayor, out Vector4 punto, int timeOut_ms)
		{
			punto = new Vector4();
			string cad = options.ToString("0") + " " + anguloMenor.ToString("0.000") + " " + anguloMayor.ToString("0.000");
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_infolaser, cad);
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_infolaser, timeOut_ms)) return false;
			char[] delimiters = { ' ' };
			string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_infolaser].Response.Parameters.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				if (parts.Length > 0)
				{
					punto.Z = double.Parse(parts[0]);
					punto.W = double.Parse(parts[1]);
					punto.X = double.Parse(parts[2]);
					punto.Y = double.Parse(parts[3]);
				}
			}
			catch { return false; }
			return true;
		}
		public bool MVN_PLN_basebusy(out int ocupado, int timeOut_ms)
		{
			ocupado = 1;
			this.SetupAndSendCommand(JustinaCommands.MVN_PLN_basebusy, "1");
			if (!this.WaitForResponse(JustinaCommands.MVN_PLN_basebusy, timeOut_ms)) return false;
			try
			{
				ocupado = int.Parse(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_basebusy].Response.Parameters);
			}
			catch { return false; }
			return true;
		}


        public bool MVN_PLN_addlocation(string location, string orientation, int timeout_ms)
        {
            this.SetupAndSendCommand(JustinaCommands.MVN_PLN_addlocation, location + " " + orientation);
            if (!this.WaitForResponse(JustinaCommands.MVN_PLN_addlocation, timeout_ms)) return false;

            //string[] parts = this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_addLocation].Response.Parameters.Split();

            return true;
        }

		public void MVN_PLN_startsavingpath()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startsavingpath].IsResponseReceived = false;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startsavingpath].Command);
		}

		public void MVN_PLN_stopsavingpath()
		{
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopsavingpath].IsResponseReceived = false;
			this.SendCommand(this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopsavingpath].Command);
		}
		public bool UseLocalization { get; set; }

		public int InGoodRegionForLoc { get; set; }

		public double NearObjForLocDirection { get; set; }
        #endregion
    }
}

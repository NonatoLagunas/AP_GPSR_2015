using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.StateMachines;
using Robotics.Controls;
using Robotics.Mathematics;
using System.Threading;

namespace ActionPlanner.ComplexActions
{
    class SM_TakeTray
    {

        #region enums
        enum States
        {
			LineTable,
			TakeTray,
			FinalState
        }

        /// <summary>
        /// Final states that indicate whether the execution of this SM was successful.
        /// </summary>
        public enum FinalStates
        {
            /// <summary>
            /// This SM is still running
            /// </summary>
            StillRunning,
            /// <summary>
            /// The execution of this SM was successful.
            /// </summary>
            OK,
            /// <summary>
            /// The execution of this SM was NOT successful.
            /// </summary>
            Failed
        }
        #endregion

        #region Variables

        private HAL9000Brain brain;
        private HAL9000CmdMan cmdMan;
		private double distance;
		private double height;
		private double headPan;
		private double headTilt;
		private int percent;
		private Vector3 larmPoint;
		private Vector3 rarmPoint;


   
        
     
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
	

        #endregion

        #region Constructor


		public SM_TakeTray(HAL9000Brain brain, HAL9000CmdMan cmdMan, double height) 
		
		{
            SM = new FunctionBasedStateMachine();
            this.brain = brain;
            this.cmdMan = cmdMan;
			distance = 0;
			this.height = height;
			headPan = 0;
			headTilt = -1.1;
			percent = 50;
            this.finalState = FinalStates.StillRunning;

    		SM.AddState(new FunctionState((int)States.LineTable,LineTable));
            SM.AddState(new FunctionState((int)States.TakeTray, TakeTray));
            SM.AddState(new FunctionState((int)States.FinalState, FinalState,true));
			SM.SetFinalState((int)States.FinalState);
        }
        #endregion
		
		#region Estado : LineTable

		int LineTable(int currentState, object o)
        {
			double x0, y0, z0, x1, y1, z1;
			this.cmdMan.HEAD_lookat(headPan,headTilt);
			while(!this.cmdMan.OBJ_FNDT_findedgereturns(headTilt, out x0, out y0, out z0, out x1, out y1, out z1, 5000));
		    height = (y0+y1)/2;
			distance = (z0+z1)/2;
			headTilt=0; 
			this.cmdMan.HEAD_lookat(headPan,headTilt);
			this.cmdMan.MVN_PLN_move(distance+0.1,10000);
			return (int)States.TakeTray;
        }
        #endregion

		#region Estado : TakeTray

		int TakeTray(int currentState, object o)
        {
			double x0, y0, z0, x1, y1, z1;
		    headTilt=-1.1; 
            this.cmdMan.HEAD_lookat(headPan,headTilt);
		    while(!this.cmdMan.OBJ_FNDT_findedgefastandfurious(headTilt,height, out x0, out y0, out z0, out x1, out y1, out z1, 5000));
			larmPoint = new Vector3(x0, y0, z0);
			rarmPoint = new Vector3(x1, y1, z1);

			larmPoint = TransHeadToRobot(larmPoint, headPan);
			rarmPoint = TransHeadToRobot(rarmPoint, headPan);

			larmPoint.Y -= 0.01;
			//rarmPoint.Y += 0.04;

			larmPoint = TransRobotToLeftArm(larmPoint);
			rarmPoint = TransRobotToRightArm(rarmPoint);

			this.cmdMan.ARMS_goto("navigation", 7000);

			//Poner Apertura
			this.cmdMan.ARMS_la_opengrip(percent,3000);
			this.cmdMan.ARMS_ra_opengrip(percent,3000);

			//this.cmdMan.ARMS_la_abspos(larmPoint.X - 0.05, larmPoint.Y  , larmPoint.Z, 1.4168, 0.2183, 1.4358, 0, 20000);
			//this.cmdMan.ARMS_ra_abspos(rarmPoint.X - 0.05, rarmPoint.Y  , rarmPoint.Z, 1.4168, 0.2183, 1.4358, 0, 20000);
			this.cmdMan.ARMS_la_abspos(larmPoint.X - 0.05, larmPoint.Y, larmPoint.Z, 1.5708, 0.0, 1.5708, 0, 20000);
			this.cmdMan.ARMS_ra_abspos(rarmPoint.X - 0.05, rarmPoint.Y, rarmPoint.Z, 1.5708, 0.0, 1.5708, 0, 20000);

			Thread.Sleep(2000);

			this.cmdMan.ARMS_la_closegrip();
			this.cmdMan.ARMS_ra_closegrip();
			
			//Hacer prueba para poner los brasos mas atras

			//this.cmdMan.ARMS_la_artpos(out lp0, out lp1, out lp2, out lp3, out lp4, out lp5, out lp6, 10000);
			//Thread.Sleep(3000);
			//this.cmdMan.ARMS_ra_artpos(out rp0, out rp1, out rp2, out rp3, out rp4, out rp5, out rp6, 10000);

			//this.cmdMan.ARMS_la_artpos(lp0, lp1, lp2, lp3, lp4, lp5, lp6, 10000);
			//this.cmdMan.ARMS_ra_artpos(rp0, rp1, rp2, rp3 + 0.2, rp4, rp5, rp6, 10000);

			this.cmdMan.ARMS_goto("charola", 10000);
			return (int)States.FinalState;
		
        }
        #endregion
        
		#region Estado: FinalState
        int FinalState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("head to 0,0");
            cmdMan.HEAD_lookat(0, 0, 10000);

			this.finalState = FinalStates.OK;
            return currentState;
        }
        #endregion

        #region Run State Machine
        public FinalStates Execute()
        {
            while (this.brain.Status.IsRunning && this.brain.Status.IsExecutingPredefinedTask && !SM.Finished)
            {
                if (this.brain.Status.IsPaused)
                {
                    Thread.Sleep((int)this.brain.Status.BrainWaveType);
                    continue;
                }
                SM.RunNextStep();
            }
            return this.finalState;
        }
        #endregion

		#region methods
		public Vector3 TransRobotToLeftArm(Vector3 vectorRobot)
		{
			//return new Vector3(vectorRobot.Y, 1.35 - vectorRobot.Z, vectorRobot.X - 0.15);
			return new Vector3(1.35 - vectorRobot.Z, vectorRobot.X, -vectorRobot.Y + 0.17);
		}

		public Vector3 TransRobotToRightArm(Vector3 vectorRobot)
		{
			//return new Vector3(vectorRobot.Y, 1.14 - vectorRobot.Z, vectorRobot.X + 0.15);
			return new Vector3(1.35 - vectorRobot.Z, vectorRobot.X, -vectorRobot.Y - 0.17);
		}

		public Vector3 TransHeadToRobot(Vector3 vectorInKinectCoords, double headPan)
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
		#endregion
	}
}

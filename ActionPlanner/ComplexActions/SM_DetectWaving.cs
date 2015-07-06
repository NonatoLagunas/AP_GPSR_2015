using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;

namespace ActionPlanner.ComplexActions
{
	class SM_DetectWaving
	{
		#region Status Enums
		/// <summary>
		/// States of the "DetectEmergency" State Machine
		/// </summary>
		private enum States
		{
			/// <summary>
			/// initial state of a DetectEmergency
			/// </summary>
			InitialState,
			/// <summary>
			/// Search for an emergency.
			/// </summary>
			SearchWaving,
			/// <summary>
			/// Final State.
			/// </summary>
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
			/// Requests were not found.
			/// </summary>
			WavingNotFound
		}
		#endregion

		#region Variables

		private readonly HAL9000Brain brain;
		private readonly HAL9000CmdMan cmdMan;
		private FunctionBasedStateMachine SM;
		private FinalStates finalState;
        private double headAngle;
        private double headPan;

		#endregion

		/// <summary>
		/// Creates a state machine to enter the arena up to a location known by the motion planner.
		/// </summary>
		/// <param name="brain">A HAL9000Brain instance</param>
		/// <param name="location">A location known by the motion planner where the robot should try to go when entering the arena.</param>
		public SM_DetectWaving(HAL9000Brain brain, HAL9000CmdMan cmdMan, double headAngle, double headPan)
		{
			this.brain = brain;
			this.cmdMan = cmdMan;

			this.headAngle = headAngle;
            this.headPan = headPan;

			SM = new FunctionBasedStateMachine();
			SM.AddState(new FunctionState((int)States.InitialState, new SMStateFuncion(InitialState)));
			SM.AddState(new FunctionState((int)States.SearchWaving, new SMStateFuncion(SearchWaving)));
			SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState), true));

			SM.SetFinalState((int)States.FinalState);
			finalState = FinalStates.StillRunning;
		}


		#region Class methods
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

		#region State functions

		private int InitialState(int currentState, object o)
		{
			return (int)(States.SearchWaving);
		}

		private int SearchWaving(int currentState, object o)
		{
			double xFall, zFall;
			/*send command to find waving*/
			//emergencyFound = false;

			//if (!emergencyFound)
            Thread.Sleep(1000);
            if (!cmdMan.VISION_findwaving(headAngle, out xFall, out zFall, 10000))
				finalState = FinalStates.WavingNotFound;
			else
			{
                double distance, angle;
                //double robotX, robotY, robotAngle;
                //double goalX, goalY;

                //this.cmdMan.MVN_PLN_position(out robotX, out robotY, out robotAngle, 1000);
				distance = ((Math.Sqrt(Math.Pow(xFall, 2) + Math.Pow(zFall, 2))));
				
				angle = Math.Atan2(-xFall, zFall);
                //angle = 1.5708-robotAngle;

                //goalX = robotX + (xFall * Math.Cos(angle) - zFall * Math.Sin(angle));
                //goalY = robotY + (xFall * Math.Sin(angle) + zFall * Math.Cos(angle));
                
                /*if(!cmdMan.MVN_PLN_getclose(goalX,goalY,10000))
                    if (!cmdMan.MVN_PLN_getclose(goalX, goalY, 10000))
                        cmdMan.MVN_PLN_getclose(goalX, goalY, 10000);
                */
                

				if (!cmdMan.MVN_PLN_move((3*distance)/5, angle+headPan, 10000))
                    if (!cmdMan.MVN_PLN_move((3*distance)/5, angle + headPan, 10000))
                        cmdMan.MVN_PLN_move((3*distance)/5, angle + headPan, 10000);
				 
				//getclose to waving
				finalState = FinalStates.OK;
			}

			return (int)States.FinalState;
		}

		private int FinalState(int currentState, object o)
		{
			return currentState;
		}

		#endregion
	}
}

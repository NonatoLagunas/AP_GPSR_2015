using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;

namespace ActionPlanner.ComplexActions
{
	class SM_DetectPersonFall
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
			SearchFall,
			/// <summary>
			/// Go to the emergency location.
			/// </summary>
			GoToFall,
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
			FallNotDetected,
			NoHumanFallDetected,
			FallNotReached
		}
		#endregion

		#region Variables

		private readonly HAL9000Brain brain;
		private readonly HAL9000CmdMan cmdMan;
		private FunctionBasedStateMachine SM;
		private FinalStates finalState;
		private System.Diagnostics.Stopwatch timer;
		private bool fallDetected;

		#endregion

		/// <summary>
		/// Creates a state machine to enter the arena up to a location known by the motion planner.
		/// </summary>
		/// <param name="brain">A HAL9000Brain instance</param>
		/// <param name="location">A location known by the motion planner where the robot should try to go when entering the arena.</param>
		public SM_DetectPersonFall(HAL9000Brain brain, HAL9000CmdMan cmdMan)
		{
			this.brain = brain;
			this.cmdMan = cmdMan;
			timer = new System.Diagnostics.Stopwatch();
			fallDetected = false;

			SM = new FunctionBasedStateMachine();
			SM.AddState(new FunctionState((int)States.InitialState, new SMStateFuncion(InitialState)));
			SM.AddState(new FunctionState((int)States.SearchFall, new SMStateFuncion(SearchFall)));
			SM.AddState(new FunctionState((int)States.GoToFall, new SMStateFuncion(GoToFall)));
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
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> InitialState of person fall recognition.");
			brain.lastPersonFallDetected.Clear();
			return (int)(States.SearchFall);
		}

		private int SearchFall(int currentState, object o)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SearchFall state of SM_DetectPersonFall reached.");
			
			//double headAngle=-Math.PI/6;			
            double headAngle = (-0.5 * 3.1416) / 180;
			cmdMan.HEAD_lookat(0.0,headAngle, 5000);
			//headAngle = (headAngle * 180) / Math.PI;
			headAngle = -0.5;
            brain.lastPersonFallDetected.Clear();
            Thread.Sleep(5000);
			//enviar comando para que comienze a detectar caidas
			cmdMan.VISION_findfall(true, headAngle);
			
			timer.Start();			
			while (timer.Elapsed.Seconds < 25 && brain.lastPersonFallDetected.Count==0);
			timer.Stop();
			
			//enviar comando para que deje de detectar caidas
			cmdMan.VISION_findfall(false, 0);

			if (brain.lastPersonFallDetected.Count > 0)
				fallDetected = true;
			else
				fallDetected = false;

			if (!fallDetected)
			{
				finalState = FinalStates.FallNotDetected;
				return (int)States.FinalState;
			}

			return (int)States.GoToFall;
		}

		private int GoToFall(int currentState, object o)
		{
			bool notHumanFall;
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> GoToFall state of SM_DetectPersonFall reached.");
			if(brain.GetCloseToHumanFall(out notHumanFall, 10000))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Getclose to human success.");
				finalState = FinalStates.OK;
				return (int)States.FinalState;
			}
			else
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Getclose to human FAILED");
				if (!notHumanFall)
					finalState = FinalStates.FallNotReached;
				else
				{
					//brain.SayAsync("I have detected a fall in that direction.");
					//point at the direction of the fall

					//Thread.Sleep(1000);
					finalState = FinalStates.NoHumanFallDetected;
				}

				return (int)States.FinalState;
			}
		}

		private int FinalState(int currentState, object o)
		{
			return currentState;
		}

		#endregion
	}
}

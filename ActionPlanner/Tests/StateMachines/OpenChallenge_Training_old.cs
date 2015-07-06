using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;
using Robotics.Mathematics;
using ActionPlanner.ComplexActions;

namespace ActionPlanner.Tests.StableTests
{
	class OpenChallenge_Training
	{
		#region  State enums

		private enum States
		{
			CallRobot,
			TrainFace,
			DeliverSheet,
			TrainVoice,
			ReceiveSheet,
			AdjustTable,
			GotoTray,
			TakeTray,
			GotoTable,
			TrayOnTable,
			FinalState

		}
		public enum FinalStates
		{
			StillRunning,
			OK,
			Failed
		}

		#endregion
		#region Variables
		private readonly HAL9000Brain brain;
		private readonly HAL9000CmdMan cmdMan;
		private FunctionBasedStateMachine SM;
		private FinalStates finalState;
		private string LocationTray;
		private string LocationTable;
		private double height;
		private string nameCallPerson;
		private double angleCallPerson;
		private int voiceRecognitionAttempts;
		private double distance;
		private double headPan;
		private double headTilt;
		private int percent;
		private List<string> alreadyKnownPersons;
		private string[] defaultNames;
		private int personCounter;
		private bool usingRightArm;
		private Vector3 larmPoint;
		private Vector3 rarmPoint;
		#endregion
		#region Constructor
		public OpenChallenge_Training(HAL9000Brain brain, HAL9000CmdMan cmdMan)
		{
			this.brain = brain;
			this.cmdMan = cmdMan;
			this.brain.UseFloorLaser = false;
			height = 0;
			LocationTable = "table";
			LocationTray = "tray";
			percent = 50;
			distance = 0;
			headPan = 0;
			headTilt = -1.1;
			voiceRecognitionAttempts = 0;
			alreadyKnownPersons = new List<string>();
			defaultNames = new string[] {"John", "James", "William", "Michael"};
			personCounter = 0;
			usingRightArm = true;

			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Executing Open Challenge");
			this.brain.Status.TestBeingExecuted = "Open Challenge";
			this.brain.OnStatusChanged(new HAL9000StatusArgs(this.brain.Status));
			this.finalState = FinalStates.StillRunning;

			SM = new FunctionBasedStateMachine();
			SM.AddState(new FunctionState((int)States.CallRobot, CallRobot));
			SM.AddState(new FunctionState((int)States.TrainFace, TrainFace));
			SM.AddState(new FunctionState((int)States.DeliverSheet, DeliverSheet));
			SM.AddState(new FunctionState((int)States.TrainVoice, TrainVoice));
			SM.AddState(new FunctionState((int)States.ReceiveSheet, ReceiveSheet));
			SM.AddState(new FunctionState((int)States.AdjustTable, AdjustTable));
			SM.AddState(new FunctionState((int)States.GotoTray, GotoTray));
			SM.AddState(new FunctionState((int)States.TakeTray, TakeTray));
			SM.AddState(new FunctionState((int)States.GotoTable, GotoTable));
			SM.AddState(new FunctionState((int)States.TrayOnTable, TrayOnTable));
			SM.AddState(new FunctionState((int)States.FinalState, FinalState, true));

			SM.SetFinalState((int)States.FinalState);

		}

		#endregion

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


		#region State Functions


		private int CallRobot(int currentState, object o)
		{
			
			TextBoxStreamWriter.DefaultLog.WriteLine("Waiting for a person to call the robot...");
			//string[] expectedStrigs = null; ;
			string order;
			if (!this.cmdMan.SPK_identify(2, out this.nameCallPerson, out this.angleCallPerson, 10000))
			{
				//Thread.Sleep(2000);
				return currentState;
			}

			string recognizedPerson = "human";

			if (this.nameCallPerson == "unknown")
			{
				if (voiceRecognitionAttempts < 1)
				{
					voiceRecognitionAttempts++;
					TextBoxStreamWriter.DefaultLog.WriteLine("Could not recognize person's voice, trying again.");
					return currentState;
				}
				else
				{
					this.cmdMan.HEAD_lookat(this.angleCallPerson, 0.0, 5000);
					double pan, tilt;
					//if (this.cmdMan.ST_PLN_findhuman("human", "hdtilt", 60000, out recognizedPerson) && recognizedPerson != "unknown")
					if (this.cmdMan.PRS_FND_findhuman(ref recognizedPerson, out pan, out tilt, 60000) && recognizedPerson != "unknown")
					{
						this.cmdMan.HEAD_lookatrel(pan, tilt, 5000);
						brain.SayAsync("Hello " + recognizedPerson);
						TextBoxStreamWriter.DefaultLog.WriteLine("Person recognized with eyes: " + recognizedPerson);
					}
					else
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("Person was not recognized, neither by voice or face.");
						brain.SayAsync("Hello human, I do not know you. Would you like me to remember you?");
						voiceRecognitionAttempts = 0;
						if (brain.WaitForUserConfirmation())
							return (int)States.TrainFace;
						return currentState;
					}
				}
			}
			else
			{
				if (this.angleCallPerson > 0.17)
					this.brain.SayAsync("Hello " + nameCallPerson + " I heard you in my left side.");
				else if (this.angleCallPerson < -0.17)
					this.brain.SayAsync("Hello " + nameCallPerson + " I heard you in my rigth side.");
				else
					this.brain.SayAsync("Hello " + nameCallPerson + " I heard you front of me.");

				Thread.Sleep(3000);

				//this.cmdMan.MVN_PLN_move(0, this.angleCallPerson, 3000);
				this.cmdMan.HEAD_lookat(this.angleCallPerson, 0);
			}

			voiceRecognitionAttempts = 0;
			this.brain.SayAsync("Do you need something?");
			
			order = this.brain.WaitForHumanOrders("", 7000, true, "table","no");
			if (order == null || order == String.Empty || order== "no")
			{
			//	this.cmdMan.HEAD_lookat(0, 0);
			//	this.cmdMan.MVN_PLN_move(0, -this.angleCallPerson, 3000);
				return currentState;
			}
			else
			{
				this.cmdMan.HEAD_lookat(0, 0);
				return (int)States.AdjustTable;
			}
		}

		private int TrainFace(int currentState, object o)
		{
			string[] knownNames = new string[brain.KnownPersons.Keys.Count];
            brain.KnownPersons.Keys.CopyTo(knownNames, 0);
			SM_AssociateNameAndFace nameAndFace = new SM_AssociateNameAndFace(brain, cmdMan, false, knownNames, defaultNames[personCounter], alreadyKnownPersons);
			personCounter = (personCounter + 1) % defaultNames.Length;

			if (nameAndFace.Execute() != SM_AssociateNameAndFace.FinalStates.OK)
				return currentState;

			alreadyKnownPersons.Add(nameAndFace.Name);

			return (int)States.DeliverSheet;
		}

		private int DeliverSheet(int currentState, object o)
		{
			brain.SayAsync("I will need you to read the paragraph in this sheet.");
			//this.cmdMan.ST_PLN_deliverobject("object", 10000);
			Thread.Sleep(800);
			if (usingRightArm)
			{
				this.cmdMan.ARMS_ra_goto("deliver");
				brain.SayAsync("Please take it from my hand, I will open my gripper.");
				Thread.Sleep(1500);
				this.cmdMan.ARMS_ra_opengrip();
				Thread.Sleep(2000);
				this.cmdMan.ARMS_ra_closegrip();
				Thread.Sleep(1500);
				this.cmdMan.ARMS_ra_goto("standby", 10000);
			}
			else
			{
				this.cmdMan.ARMS_la_goto("deliver");
				brain.SayAsync("Please take it from my hand, I will open my gripper.");
				Thread.Sleep(1500);
				this.cmdMan.ARMS_la_opengrip();
				Thread.Sleep(2000);
				this.cmdMan.ARMS_la_closegrip();
				Thread.Sleep(1500);
				this.cmdMan.ARMS_la_goto("standby", 10000);
			}

			return (int)States.TrainVoice;
		}

		private int TrainVoice(int currentState, object o)
		{
			//Revisar si el brazo regresa a navigation
			brain.SayAsync("You will have 20 seconds to read as much as you can. if you finish, try to keep speaking until the 20 seconds are up.");
			//In case brain.SayAsync is really asynchronous:
			this.cmdMan.SPG_GEN_say("You can start read-ing in 3. 2. 1. NOW!", 10000);

			if (!this.cmdMan.SPK_train(alreadyKnownPersons[alreadyKnownPersons.Count - 1], 20, 60000))
				brain.SayAsync("I could not hear you well, I will remember only your face.");

			return (int)States.ReceiveSheet;
		}

		private int ReceiveSheet(int currentState, object o)
		{
			brain.SayAsync("Please give me back the paper sheet.");
			this.cmdMan.HEAD_lookat(0.0, -1.0, 10000);
			double x, y, z;
			this.cmdMan.OBJ_FNDT_findgolemhand(-1.0, out x, out y, out z, 5000);
			//this.cmdMan.ARMS_goto("navigation", 10000);
			this.cmdMan.ST_PLN_takexyz(x, y, z, out usingRightArm, 60000);

			if (usingRightArm)
				this.cmdMan.ARMS_ra_goto("standby", 10000);
			else
				this.cmdMan.ARMS_la_goto("standby", 10000);

			return (int)States.CallRobot;
		}

		private int AdjustTable(int currentState, object o)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("Getting close to table...");
			this.brain.SayAsync("I am getting close to the table.");
			this.brain.GetCloseToTable(LocationTable, out height, 120000);
			TextBoxStreamWriter.DefaultLog.WriteLine("Taking the table...");
			this.brain.SayAsync("I am taking the table.");
			if (!this.cmdMan.ST_PLN_taketable(100000))
				return currentState;
			this.brain.SayAsync("Table is now aligned.");
			this.cmdMan.MVN_PLN_move(-0.3, 4000);
			return (int)States.GotoTray;
		}

		private int GotoTray(int currentState, object o)
		{
			this.brain.SayAsync("I am getting close to the tray.");
			TextBoxStreamWriter.DefaultLog.WriteLine("Getting close to tray location...");
			if (!this.brain.GetCloseToTable(LocationTray, out height, 120000))
				return currentState;
			return (int)States.TakeTray;
		}

		private int TakeTray(int currentState, object o)
		{
			this.brain.SayAsync("I will take the tray.");
			TextBoxStreamWriter.DefaultLog.WriteLine("Taking the tray...");
			//Revisar en la MS TakeTray que los brazos se acomoden hacia atras
			SM_TakeTray SMTakeTray = new SM_TakeTray(brain, cmdMan, height);
			SMTakeTray.Execute();
			//ExecuteTakeTray();
			this.cmdMan.MVN_PLN_move(-0.3, 4000);
			return (int)States.GotoTable;
		}

		private int GotoTable(int currentState, object o)
		{
			this.brain.SayAsync("I'm going back to the table.");
			TextBoxStreamWriter.DefaultLog.WriteLine("Getting back to the table...");
			if (!this.brain.GetCloseToTable(LocationTable, 120000))
				return currentState;
			return (int)States.TrayOnTable;
		}

		private int TrayOnTable(int currentState, object o)
		{
			this.brain.SayAsync("I will drop the tray.");
			TextBoxStreamWriter.DefaultLog.WriteLine("Dropping the tray...");
			this.cmdMan.ARMS_ra_opengrip(4000);
			this.cmdMan.ARMS_la_opengrip(4000);
			return (int)States.FinalState;
		}

		private int FinalState(int currentState, object o)
		{
			this.brain.SayAsync("I have finished"); 

			return currentState;

		}

		#endregion


	}

}

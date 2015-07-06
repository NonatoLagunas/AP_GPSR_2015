using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;

namespace ActionPlanner.ComplexActions
{
    class SM_AssociateNameAndFace
    {
        #region Status Enums
        /// <summary>
        /// States of the "FindAndRememberUnknownHuman" State Machine
        /// </summary>
        private enum States
        {   
            InitialState,
            FindHuman,
            AskForName,
            TrainShirt,
            TrainFace,
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
            /// A human face could not be found.
            /// </summary>
            HumanNotFound,
			/// <summary>
			/// A human face was found but could not be learned.
			/// </summary>
			HumanNotLearned
        }
        #endregion

        #region Variables

        private readonly HAL9000Brain brain;
        private readonly HAL9000CmdMan cmdMan;
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
        private int attemptCounter;
        private string defaultName;
        private string[] knownPersons;
        private List<string> rejectedNames;
        private string foundHuman;
        private double x;
        private double y;
        private double angle;
		private bool usePanAndTilt;
		private bool trainShirt;
        
        #endregion

		#region Constructors
		/// <summary>
        /// Creates a state machine to enter the arena up to a location known by the motion planner.
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        /// <param name="location">A location known by the motion planner where the robot should try to go when entering the arena.</param>
        public SM_AssociateNameAndFace(HAL9000Brain brain, HAL9000CmdMan cmdMan, string [] knownPersons, string defaultName, bool usePanAndTilt, List<string> rejectedNames, bool trainPersonsShirt)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;
            this.attemptCounter = 0;
            this.defaultName = defaultName;
            this.knownPersons = knownPersons;
			this.rejectedNames = rejectedNames;
			this.usePanAndTilt = usePanAndTilt;
			this.trainShirt = trainPersonsShirt;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, new SMStateFuncion(InitialState)));
            SM.AddState(new FunctionState((int)States.AskForName, new SMStateFuncion(AskForName)));
            SM.AddState(new FunctionState((int)States.FindHuman, new SMStateFuncion(FindHuman)));
            SM.AddState(new FunctionState((int)States.TrainFace, new SMStateFuncion(TrainFace)));
            SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState), true));

            SM.SetFinalState((int)States.FinalState);
            finalState = FinalStates.StillRunning;

        }

		public SM_AssociateNameAndFace(HAL9000Brain brain, HAL9000CmdMan cmdMan, string [] knownPersons, string defaultName, List<string> rejectedNames) : this(brain, cmdMan, knownPersons, defaultName, true, rejectedNames, false)
        {
        }

		public SM_AssociateNameAndFace(HAL9000Brain brain, HAL9000CmdMan cmdMan, string[] knownPersons, string defaultName, bool usePanAndTilt, List<string> rejectedNames)
			: this(brain, cmdMan, knownPersons, defaultName, usePanAndTilt, rejectedNames, false)
		{
		}

		public SM_AssociateNameAndFace(HAL9000Brain brain, HAL9000CmdMan cmdMan, string[] knownPersons, string defaultName, List<string> rejectedNames, bool trainPersonsShirt)
			: this(brain, cmdMan, knownPersons, defaultName, true, rejectedNames, trainPersonsShirt)
		{
		}

		#endregion

		#region Properties
		public double X
        {
            get { return x; }
        }

        public double Y
        {
            get { return y; }
        }

        public double Angle
        {
            get { return angle; }
        }

        public string Name
        {
            get { return foundHuman; }
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

        #region State functions

        private int InitialState(int currentState, object o)
        {
			return (int)(States.FindHuman);
        }

        private int FindHuman(int currentState, object o)
        {
            string foundHuman;
            //Thread.Sleep(3500);

            if (this.cmdMan.ST_PLN_findhuman("human", "hdtilt hdpan", 90000, out foundHuman))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Found human: " + foundHuman +
                    ". Sending <alignhuman> command");
                //this.cmdMan.ST_PLN_alignhuman("human", 20000);
                this.cmdMan.SPG_GEN_shutup(2000);
                Thread.Sleep(500);
                this.cmdMan.SPG_GEN_say("Hello human, I am the robot justina. Please speak louder.", 15000);
                attemptCounter = 0;
                return (int)States.AskForName;
            }

            if (attemptCounter < 2)
            {
                attemptCounter++;
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot find human");
                this.cmdMan.SPG_GEN_shutup(2000);
                Thread.Sleep(100);
                this.cmdMan.SPG_GEN_say("please look straight to my webcam.", 10000);
                //this.cmdMan.HEAD_lookat(0, -.3, 4000);
                return currentState;
            }
            attemptCounter = 0;
            return (int)States.AskForName;

        }

        private int AskForName(int currentState, object o)
        {
            foundHuman = brain.WaitForHumanOrders("Please, tell me your name in the form: robot, my name is:", 15000, true, knownPersons);
			if ((foundHuman == null || foundHuman == "") && attemptCounter < 2)
			{
				attemptCounter++;
				return currentState;
			}
			attemptCounter = 0;
            if (rejectedNames.Contains(foundHuman))
            {
                brain.SayAsync("Sorry the name " + foundHuman + " is already in my system");
                return currentState;
            }
            if (foundHuman == "" || foundHuman == null)
            {
                foundHuman = defaultName;
                brain.SayAsync("Sorry, I cannot understand your name. You will be " + foundHuman + " for the remainder of the test");
                Thread.Sleep(1500);
			}
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Confirm \"yes\" received");
			brain.SayAsync("O.K. I am going to remember your face. Please look straight to my web camera.");
            Thread.Sleep(500);
			//if(trainShirt)
			//    return (int)States.TrainShirt;
            return (int)States.TrainFace;
        }

        private int TrainShirt(int currentState, object o)
        {
            brain.SayAsync("Im going to remember your clothes.");
            cmdMan.HEAD_lookat(0.0, -0.3);
            Thread.Sleep(1000);
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Remembering shirt...");

            brain.SayAsync("Human, please stand one meter in front of me.");
            Thread.Sleep(1000);
			if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_front", 3000))
				if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_front", 3000))
					cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_front", 3000);

            brain.SayAsync("Now, please turn to your left side.");
            Thread.Sleep(2000);

			if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_left", 3000))
				if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_left", 3000))
					cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_left", 3000);

            brain.SayAsync("Now, turn your back towards me.");
            Thread.Sleep(2000);

			if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_back", 3000))
				if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_back", 3000))
					cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_back", 3000);

            brain.SayAsync("Now, please turn to your right side.");
            Thread.Sleep(2000);

			if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_right", 3000))
				if (!cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_right", 3000))
					cmdMan.OBJ_FNDT_trainshirt(foundHuman + "_right", 3000);

            brain.SayAsync("Thank you.");
            Thread.Sleep(500);
            return (int)States.TrainFace;
            //return (int)States.FinalState;
        }


        private int TrainFace(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Sending <remember_human> command");
			this.cmdMan.MVN_PLN_position(out x, out y, out angle, 2000);
			string person;

			if (this.cmdMan.ST_PLN_findhuman("human", "hdtilt hdpan", 90000, out person))
			{
				if (this.cmdMan.ST_PLN_rememberhuman(foundHuman, "", 60000))
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Face of " + foundHuman + " remembered succesfully");
					brain.SayAsync("I have remembered your face.");
                    Thread.Sleep(500);
				}
				else
				{
					if (attemptCounter < 2)
					{
						attemptCounter++;
						return currentState;
					}
					else
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot remember the face of " + foundHuman);
						//brain.SayAsync("I cannot remember your face. I will continue with the test");
                        //return (int)States.TrainShirt;
						this.finalState = FinalStates.HumanNotLearned;
						return (int)States.FinalState;
					}
				}
			}
			else
			{
				if (attemptCounter < 2)
				{
					attemptCounter++;
					return currentState;
				}
				else
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot find human.");
					this.finalState = FinalStates.HumanNotFound;
					//return (int)States.TrainShirt;
					return (int)States.FinalState;
				}
			}

			attemptCounter = 0;
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumandRoutine.-> Desactivating reco human");
            this.cmdMan.PRS_FND_sleep(true);
			this.finalState = FinalStates.OK;
            return (int)States.FinalState;
        }

        private int FinalState(int currentState, object o)
        {
            return currentState;
        }

        #endregion
    }
}

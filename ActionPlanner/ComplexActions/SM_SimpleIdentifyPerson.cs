using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;

namespace ActionPlanner.ComplexActions
{
    class SM_SimpleIdentifyPerson
    {
          #region Status Enums
        /// <summary>
        /// States of the "FindAndRememberUnknownHuman" State Machine
        /// </summary>
        private enum States
        {   
            InitialState,
            SpeachToHuman,
            FindHuman,
            AskForName,
            AsociateName,
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

        private readonly HAL9000Brain brain;
        private readonly HAL9000CmdMan cmdMan;
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
		private bool startIdentifyPerson;
        private string defaultName;
        private string[] knownPersons;
        private List<string> rejectedNames;
        private string foundHuman;
        private double x;
        private double y;
        private double angle;
        
        #endregion

        /// <summary>
        /// Creates a state machine to enter the arena up to a location known by the motion planner.
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        /// <param name="location">A location known by the motion planner where the robot should try to go when entering the arena.</param>
		public SM_SimpleIdentifyPerson(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool startIdentifyPerson, string[] knownPersons, string defaultName, List<string> rejectedNames)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;
			this.startIdentifyPerson = startIdentifyPerson;
            this.defaultName = defaultName;
            this.knownPersons = knownPersons;
			this.rejectedNames = rejectedNames;

            SM = new FunctionBasedStateMachine();
			SM.AddState(new FunctionState((int)States.InitialState, new SMStateFuncion(InitialState)));
			SM.AddState(new FunctionState((int)States.SpeachToHuman, new SMStateFuncion(SpeachToHuman)));
            SM.AddState(new FunctionState((int)States.FindHuman, new SMStateFuncion(FindHuman)));
            SM.AddState(new FunctionState((int)States.AskForName, new SMStateFuncion(AskForName)));
            SM.AddState(new FunctionState((int)States.AsociateName, new SMStateFuncion(AsociateName)));
            SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState), true));

            SM.SetFinalState((int)States.FinalState);
            finalState = FinalStates.StillRunning;

        }

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


		/// <summary>
		/// //
		/// </summary>
		/// <param name="currentState"></param>
		/// <param name="o"></param>
		/// <returns></returns>
		/// 
		// estoy modificando esta parte y cambien el need to call por startIdentifyPerson
        private int InitialState(int currentState, object o)
        {
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> startIdentifyPerson: " + startIdentifyPerson);
			if (startIdentifyPerson)
				return (int)(States.SpeachToHuman);
            else
                return currentState;
        }

		private int SpeachToHuman(int currentState, object o)
        {
            this.cmdMan.SPG_GEN_say("hello human, please get close to me, to begin the recognition process");
            Thread.Sleep(10000);
            return (int)States.FindHuman;
        }

        private int FindHuman(int currentState, object o)
        {
            string foundHuman;

            if (this.cmdMan.ST_PLN_findhuman("human", "hdtilt hdpan", 90000, out foundHuman))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Found human: " + foundHuman +
                    ". Sending <alignhuman> command");
                //this.cmdMan.ST_PLN_alignhuman("human", 20000);
                this.cmdMan.SPG_GEN_shutup(2000);
                Thread.Sleep(500);
                this.cmdMan.SPG_GEN_say("Hellow human, you looks good", 15000);
            }
            else
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot find human");
                this.cmdMan.SPG_GEN_shutup(2000);
                Thread.Sleep(500);
                this.cmdMan.SPG_GEN_say("Hello human i cannot find your face, please get in front of me.", 10000);
                this.cmdMan.HEAD_lookat(0, 0, 4000);
            }
            return (int)States.AskForName;
        }
        private int AskForName(int currentState, object o)
        {
            //d this.cmdMan.SPG_GEN_say("Please tell your name", 10000);
            foundHuman = brain.WaitForHumanOrders("Please tell me your name", 60000, true, knownPersons);
            if (rejectedNames.Contains(foundHuman))
            {
                brain.SayAsync("Sorry the name + " + foundHuman + " is already in my system");
                return currentState;
            }
            if (foundHuman == null)
            {
                foundHuman = defaultName;
                this.cmdMan.SPG_GEN_say("I cannot understand your name. You will be " + foundHuman + "creative, for the remainder of the test");
            }
            //rejectedNames.Add(foundHuman);
            return (int)States.AsociateName;
        }
        private int AsociateName(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Confirm \"yes\" received");
            brain.SayAsync("O.K. I am going to remember your face. Please look straight forward to my eyes");
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Sending <remember_human> command");
            if (this.cmdMan.ST_PLN_rememberhuman(foundHuman, "", 60000))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Face of " + foundHuman + " remembered succesfully");
                this.cmdMan.SPG_GEN_say("I have remembered your face.", 10000);
                this.cmdMan.HEAD_lookat(0, 0, 4000);
                this.cmdMan.MVN_PLN_position(out x, out y, out angle, 2000);
            }
            else
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot remember the face of " + foundHuman);
                this.cmdMan.SPG_GEN_say("I cannot remember your face. I will continue with the test", 12000);
                this.cmdMan.HEAD_lookat(0, 0, 4000);
            }
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

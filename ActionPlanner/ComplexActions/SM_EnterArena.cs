using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;

namespace ActionPlanner.ComplexActions
{
    class SM_EnterArena
    {
        #region Status Enums
        /// <summary>
        /// States of the "Enter Arena" State Machine
        /// </summary>
        private enum States
        {
            /// <summary>
            /// Decides whether to try to open the door or just wait for it to be opened.
            /// </summary>
            InitialState,
            /// <summary>
            /// Waits for the door to be opened.
            /// </summary>
            WaitForDoorToBeOpened,
            /// <summary>
            /// Enters the arena to "frontentrance" or a specified location.
            /// </summary>
            EnterArena,
            /// <summary>
            /// Empty final state of this state machine
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
        private string location;
        private byte attemptCounter;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a state machine to enter the arena up to a location known by the motion planner.
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        /// <param name="location">A location known by the motion planner where the robot should try to go when entering the arena.</param>
        /// <param name="tryToOpenDoor">A boolean indicating whether the robot should try to open the door on its own or ask for help directly.</param>
        public SM_EnterArena(HAL9000Brain brain, HAL9000CmdMan cmdMan, string location, bool tryToOpenDoor)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;
            this.location = location;
            this.finalState = FinalStates.StillRunning;
            this.attemptCounter = 0;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, new SMStateFuncion(InitialState)));
            SM.AddState(new FunctionState((int)States.WaitForDoorToBeOpened, new SMStateFuncion(WaitForDoorToBeOpened)));
            SM.AddState(new FunctionState((int)States.EnterArena, new SMStateFuncion(EnterArena)));
            SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState), true));

            SM.SetFinalState((int)States.FinalState);

        }

        /// <summary>
        /// Creates a state machine to enter the arena up to a location known by the motion planner called "frontentrance".
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        /// <param name="tryToOpenDoor">A boolean indicating whether the robot should try to open the door on its own or ask for help directly.</param>
        public SM_EnterArena(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool tryToOpenDoor) : this(brain, cmdMan, "frontentrance", tryToOpenDoor) { }

        /// <summary>
        /// Creates a state machine to enter the arena up to location known by the motion planner. It will not try to open the door by itself and will ask for help from the beginning instead.
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        public SM_EnterArena(HAL9000Brain brain, HAL9000CmdMan cmdMan, string location) : this(brain, cmdMan, location, false) { }

        /// <summary>
        /// Creates a state machine to enter the arena up to a location known by the motion planner called "frontentrance". It will not try to open the door by itself and will ask for help from the beginning instead.
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        public SM_EnterArena(HAL9000Brain brain, HAL9000CmdMan cmdMan) : this(brain, cmdMan, "frontentrance", false) { }
        #endregion

        #region Class methods
        public FinalStates Execute()
        {
			TextBoxStreamWriter.DefaultLog.WriteLine("Executing: SM_EnterArena");
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
        /// Determines whether it should jump to state trying to open the door or state waiting for the door to be opened.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        private int InitialState(int currentState, object o)
        {
            return (int)(States.WaitForDoorToBeOpened);
        }


        private int WaitForDoorToBeOpened(int currentState, object o)
        {
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> Saying 'I am waiting ...'");
			this.brain.SayAsync("I am waiting for the door to be opened");
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> Waiting for the door is opened");
						
			while (this.cmdMan.MVN_PLN_obstacle("door", 2000)) 
				Thread.Sleep(500);

            if (this.cmdMan.MVN_PLN_obstacle("door", 2000))
                return (int)States.WaitForDoorToBeOpened;
			
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> The door is open");
			this.brain.SayAsync("I can see now that the door is open");

            return (int)States.EnterArena;
        }

        private int EnterArena(int currentState, object o)
        {
			this.cmdMan.ARMS_goto("standby", 8000);
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Trying getclose to: " + location);
            if (this.cmdMan.MVN_PLN_getclose(location, 300000))
            {
                this.finalState = FinalStates.OK;
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Entered the arena");
                return (int)States.FinalState;
            }

            if (attemptCounter < 3)
            {
                attemptCounter++;
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot arrive to entrance location, trying again.");
                return (int)States.EnterArena;
            }

            this.finalState = FinalStates.Failed;
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot arrive to entrance location, SM was NOT successful.");
            return (int)States.FinalState;
        }

        private int FinalState(int currentState, object o)
        {
            return (int)States.FinalState;
        }

        #endregion
    }
}

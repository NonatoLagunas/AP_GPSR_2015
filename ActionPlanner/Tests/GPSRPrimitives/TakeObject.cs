using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.StateMachines;
using Robotics.Controls;
using ActionPlanner.ComplexActions;
using ActionPlanner.Tests.ConfigurationFiles;

namespace ActionPlanner.Tests.StateMachines
{
    public class TakeObject
    {

        #region Enums
        /// <summary>
        ///  States of the "Speech Recognition & Audio Detection Test" state machine
        /// </summary>
        private enum States
        {
            /// <summary>
            ///  Configuration state (variables setup)
            /// </summary>
            InitialState,
            /// <summary>
            /// Search and take obejct phase
            /// </summary>
            SearchAndTakeObject,
            /// <summary>
            /// Search and take object failed phase
            /// </summary>
            SATO_Failed,
            /// <summary>
            /// Search and take object suceeded phase
            /// </summary>
            SATO_Succeeded,
            /// <summary>
            ///  Final state of this SM
            /// </summary>
            FinalState
        }

        /// <summary>
        /// Indicates the STATUS of this SM.
        /// </summary>
        public enum Status
        {
            // TODO: Add the status you need here

            /// <summary>
            /// This SM is ready for execution
            /// </summary>
            Ready,
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
        /// <summary>
        /// Stores the HAL9000Brain instance
        /// </summary>
        private HAL9000Brain brain;
        /// <summary>
        /// Stores the HAL9000CmdMan instance
        /// </summary>
        private HAL9000CmdMan cmdMan;
        /// <summary>
        /// The state machine that executes the test
        /// </summary>
        private FunctionBasedStateMachine SM;
        /// <summary>
        /// Stores the state where the state machine stops
        /// </summary>
        private Status finalStatus;
        /// <summary>
        /// Stores all the configuration variables for the "GPSR Test" state machine
        /// </summary>
        private GPSR_WORLD SMConfiguration;
        /// <summary>
        /// Stores the string representation of the object to take
        /// </summary>
        private string objectToTake;
        private string SATO_succeeded;
        private string SATO_failed;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates the SM of the primite action: Take-Object
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        /// <param name="objectToTake">Name of the object to take. Default: empty string.</param>
        public TakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, GPSR_WORLD SMConfiguration, string objectToTake="")
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            finalStatus = Status.Ready;

            this.SMConfiguration = SMConfiguration;
            this.objectToTake = objectToTake;

            SATO_succeeded = "I got it.";
            SATO_failed = "I can't take the object.";

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));

            SM.AddState(new FunctionState((int)States.SearchAndTakeObject, SearchAndTakeObject));
            SM.AddState(new FunctionState((int)States.SATO_Failed, SATO_Failed));
            SM.AddState(new FunctionState((int)States.SATO_Succeeded, SATO_Succeeded));

            SM.AddState(new FunctionState((int)States.FinalState, FinalState, true));

            SM.SetFinalState((int)States.FinalState);
        }
        #endregion

        #region Class Methods
        /// <summary>
        /// Executes the state machine
        /// </summary>
        /// <returns>The obtained STATUS when the state machine stops</returns>
        public Status Execute()
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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> TakeObject primitive execution finished.");
            return this.finalStatus;
        }
        #endregion

        #region States Methods
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing TakeObject primitive.");

            // TODO: Change the next status
            return (int)States.SearchAndTakeObject;
        }

        private int SearchAndTakeObject(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SearchAndTakeObject state reached.");

            SM_SearchAndTakeObject sm = new SM_SearchAndTakeObject(this.brain, this.cmdMan, false, new string[] { this.objectToTake , ""}, 2, false);
            SM_SearchAndTakeObject.FinalStates SATO_FinalState =  sm.Execute();
            if(SATO_FinalState == SM_SearchAndTakeObject.FinalStates.OK)
            {
                SMConfiguration.ARMS_usedArm = sm.ArmsOrder[0];
                return (int)States.SATO_Succeeded;
            }
            else
            {
                return (int)States.SATO_Failed;
            }
        }

        private int SATO_Failed(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SATO_Failed state reached.");

            cmdMan.SPG_GEN_say(SATO_failed, 3000);
            finalStatus = Status.Failed;
            // TODO: Change the next status
            return (int)States.FinalState;
        }

        private int SATO_Succeeded(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SATO_Succeeded state reached.");

            cmdMan.SPG_GEN_say(SATO_succeeded, 3000);
            finalStatus = Status.OK;
            // TODO: Change the next status
            return (int)States.FinalState;
        }

        /// <summary>
        /// Final state of this SM
        /// </summary>
        private int FinalState(int currentState, object o)
        {
            return currentState;
        }
        #endregion
    }
}

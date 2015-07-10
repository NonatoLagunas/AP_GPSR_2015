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
    public class BringObject
    {

        #region Enums
        /// <summary>
        ///  States of the "BringObject primitive" state machine
        /// </summary>
        private enum States
        {
            /// <summary>
            ///  Configuration state (variables setup)
            /// </summary>
            InitialState,
            /// <summary>
            /// Deliver the object in hand 
            /// </summary>
            DeliverObject,
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
        private string objectName;
        private string foundPersonMessage;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates the SM of the primite action: Take-Object
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        /// <param name="objectToTake">Name of the object to take. Default: empty string.</param>
        public BringObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, GPSR_WORLD SMConfiguration, string objectName="")
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            finalStatus = Status.Ready;

            this.SMConfiguration = SMConfiguration;
            this.objectName = objectName;

            foundPersonMessage = "Human please get close to me and take the object.";

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            
            SM.AddState(new FunctionState((int)States.DeliverObject, DeliverObject));

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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing BringObject primitive.");

            // TODO: Change the next status
            return (int)States.DeliverObject;
        }

        private int DeliverObject(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> DeliverObject state reached.");
            //TODO:
            //deliver the object
            if (SMConfiguration.bringTohuman)
                cmdMan.ST_PLN_deliverobject(SMConfiguration.ARMS_usedArm, 15000);

            SMConfiguration.ARMS_usedArm = "";
            //return the arm to the original position
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

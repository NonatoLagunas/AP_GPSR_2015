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
    public class GPSR
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
            /// The robot enters to the arena
            /// </summary>
            EnterArena,
            /// <summary>
            /// The robot aproach to the operator
            /// </summary>
            NavigateToOperator,
            /// <summary>
            /// Listening mode.
            /// </summary>
            WaitForCommand,
            /// <summary>
            /// Command confirmation (expected yes or no from the user)
            /// </summary>
            ConfirmComand,
            /// <summary>
            /// extract the primitives from the conceptual dependency
            /// </summary>
            ParseCommand,
            /// <summary>
            /// execute all the primitives
            /// </summary>
            PerformCommand,
            /// <summary>
            /// the robot leaves the arena
            /// </summary>
            LeaveArena,
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
        /// Stores all the configuration variables for the "Speech Recognition & Audio Detection Test" state machine
        /// </summary>
        private GPSR_WORLD SMConfiguration;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a state machine for this test.
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        public GPSR (HAL9000Brain brain, HAL9000CmdMan cmdMan)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            finalStatus = Status.Ready;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            SM.AddState(new FunctionState((int)States.EnterArena, EnterArena));
            SM.AddState(new FunctionState((int)States.NavigateToOperator, NavigateToOperator));
            SM.AddState(new FunctionState((int)States.WaitForCommand, WaitForCommand));
            SM.AddState(new FunctionState((int)States.ConfirmComand, ConfirmComand));
            SM.AddState(new FunctionState((int)States.ParseCommand, ParseCommand));
            SM.AddState(new FunctionState((int)States.PerformCommand, PerformCommand));
            SM.AddState(new FunctionState((int)States.PerformCommand, LeaveArena));
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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> GPSR SM execution finished.");
            return this.finalStatus;
        }
        #endregion

        #region States Methods
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing GPSR test.");

            //Load the WORLD configuration for this test
            SMConfiguration = new GPSR_WORLD();

            // TODO: Change the next status
            return (int)States.EnterArena;
        }
        /// <summary>
        /// the robot enters to the arena (to an especificated MVN-PLN map location)
        /// </summary>
        /// <returns>NavigateToOperator state if the robot enters the arena, EnterArena otherwise</returns>
        private int EnterArena(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> EnterArena state reached.");

            SM_EnterArena sm = new SM_EnterArena(this.brain, this.cmdMan, SMConfiguration.MVNPLN_entranceLocation, false);
            SM_EnterArena.FinalStates final =  sm.Execute();
            if (final == SM_EnterArena.FinalStates.OK)
            {
                return (int)States.NavigateToOperator;
            }
            else
            {
                return (int)States.EnterArena;
            }            
        }
        /// <summary>
        /// the robot navigate to the operator location
        /// </summary>
        /// <returns>WaitForCommand state if the navigation primiive was executed succesfully. NavigateToOperator otherwise.</returns>
        private int NavigateToOperator(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> NavigateToOperator state reached.");

            NavigateTo sm_navigation = new NavigateTo(this.brain, this.cmdMan, SMConfiguration, SMConfiguration.MVNPLN_operatorLocation);
            NavigateTo.Status navig_status = sm_navigation.Execute();
            if (navig_status == NavigateTo.Status.OK)
            {
                //if the robot reach the location then ask for a command
                cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_waitforcomman, 5000);
                Thread.Sleep(1000);
                brain.RecognizedSentences.Clear();

                return (int)States.WaitForCommand;
            }
            else
            {
                return (int)States.NavigateToOperator;
            }
        }
        /// <summary>
        /// the robot is in listening state, waiting for a command from the user
        /// </summary>
        /// <returns>ConfirmComand state</returns>
        private int WaitForCommand(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> WaitForCommand state reached.");

            if (brain.RecognizedSentences.Count > 0)
            {
                SMConfiguration.sentenceDequeued = brain.RecognizedSentences.Dequeue();

                cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_didyousay, 2000);
                cmdMan.SPG_GEN_say(SMConfiguration.sentenceDequeued,2000);
                Thread.Sleep(500);
                brain.RecognizedSentences.Clear();

                return (int)States.ConfirmComand;
            }
            else
            {
                return (int)States.WaitForCommand;
            }
        }
        /// <summary>
        /// the robot ask for a confimation for the command recognized on WaitForCommand state
        /// </summary>
        /// <returns>WaitForCommand state if the confirmation is NO, ParseCommand state if the confirmation is YES</returns>
        private int ConfirmComand(int currentState, object o)
        {
            return (int)States.WaitForCommand;
            //return (int)States.ParseCommand;
        }
        /// <summary>
        /// Get the command parsing from the language understanding module
        /// </summary>
        /// <returns>PerformCommand state </returns>
        private int ParseCommand(int currentState, object o)
        {
            return (int)States.PerformCommand;
        }
        /// <summary>
        /// Executes all the primitives SM in order to perform the entire task
        /// </summary>
        /// <returns>LeaveArena state</returns>
        private int PerformCommand(int currentState, object o)
        {
            return (int)States.LeaveArena;
        }
        /// <summary>
        /// The robot leaves the arena
        /// </summary>
        /// <returns>FinalState state to finish this state</returns>
        private int LeaveArena(int currentState, object o) 
        {
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

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
    public class NavigateTo
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
            /// Send the arms to the navigation position
            /// </summary>
            RiseArms,
            /// <summary>
            /// Move the robot to a MVNPLN position
            /// </summary>
            Navigate,
            /// <summary>
            /// The navigation succeeded
            /// </summary>
            Navigate_Succeeded,
            /// <summary>
            /// the navigation failed
            /// </summary>
            Navigate_Failed,
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
        /// Stores the MVN_PNL name of the location to reach
        /// </summary>
        private string locationToReach;
        private string navigSucceded;
        private string navigFailed;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates the SM of the primite action: Take-Object
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        /// <param name="objectToTake">Name of the object to take. Default: empty string.</param>
        public NavigateTo(HAL9000Brain brain, HAL9000CmdMan cmdMan, GPSR_WORLD SMConfiguration, string locationToReach = "")
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            finalStatus = Status.Ready;

            this.SMConfiguration = SMConfiguration;
            this.locationToReach = locationToReach;

            navigSucceded = "I reach the location.";
            navigFailed = "I can't reach the location.";

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            
            SM.AddState(new FunctionState((int)States.RiseArms, RiseArms));
            SM.AddState(new FunctionState((int)States.Navigate, Navigate));
            SM.AddState(new FunctionState((int)States.Navigate_Succeeded, Navigate_Succeeded));
            SM.AddState(new FunctionState((int)States.Navigate_Failed, Navigate_Failed));

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool moveActiveArms(string position, int timeout)
        {
            bool finalStatus = false;
            if (SMConfiguration.ARMS_ArmsEnable.left && SMConfiguration.ARMS_ArmsEnable.right)
                return this.cmdMan.ARMS_goto(position, timeout);
            if (SMConfiguration.ARMS_ArmsEnable.left)
                return this.cmdMan.ARMS_la_goto(position, timeout);
            if (SMConfiguration.ARMS_ArmsEnable.right)
                return this.cmdMan.ARMS_ra_goto(position, timeout);

            return finalStatus;
        }
        private bool tryNavigation(string locationToReach)
        {
            bool success=false;
            if (!cmdMan.MVN_PLN_getclose(locationToReach, 50000))
            {
                if (!cmdMan.MVN_PLN_getclose(locationToReach, 50000))
                {
                    if (cmdMan.MVN_PLN_getclose(locationToReach, 50000))
                        success = true;
                }
                else
                    success = true;
            }
            else
                success = true;
            
                        
            return success;
        }
        private bool tryNavigationToTable(string locationToReach)
        {
            bool success = false;
            if (!brain.GetCloseToTable(locationToReach, 50000))
            {
                if (!brain.GetCloseToTable(locationToReach, 50000))
                {
                    if (brain.GetCloseToTable(locationToReach, 50000))
                        success = true;
                }
                else
                    success = true;
            }
            else
                success = true;


            return success;
        }
        #endregion

        #region States Methods
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing NavigateTo primitive.");

            // TODO: Change the next status
            return (int)States.RiseArms;
        }
        /// <summary>
        /// the robot tries to rise his arms to navigation position (to not disturb the laser)
        /// </summary>
        /// <returns>the navigate state, even if the arms not respond</returns>
        private int RiseArms(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> RiseArms state reached.");
            if (SMConfiguration.ARMS_usedArm == "")
            {
                if (!moveActiveArms(SMConfiguration.ARMS_navigation, 10000))
                    if (!moveActiveArms(SMConfiguration.ARMS_navigation, 10000))
                        moveActiveArms(SMConfiguration.ARMS_navigation, 10000);
            }
            else
            {
                if (!moveActiveArms(SMConfiguration.ARMS_navigObject, 10000))
                    if (!moveActiveArms(SMConfiguration.ARMS_navigObject, 10000))
                        moveActiveArms(SMConfiguration.ARMS_navigObject, 10000);
            }

            return (int)States.Navigate;
        }
        /// <summary>
        /// the robot tries to reach a predef MVN-PLN map position. If the location is a table then the robot tries to align to it.
        /// </summary>
        /// <returns>Navigate_SUcceeded state if the robot reach the position, Navigate_Failed otherwise</returns>
        private int Navigate(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Navigate state reached.");
            bool navigationSucceeded=false;
            //try to get the kind of location
            switch (SMConfiguration.getKindLocation(locationToReach))
            {
                case 0:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location is kind of standby.");
                    navigationSucceeded = tryNavigation(locationToReach);
                    break;
                case 1:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location is kind of room.");
                    navigationSucceeded = tryNavigation(locationToReach);
                    break;
                case 2:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location is kind of table.");
                    navigationSucceeded = tryNavigationToTable(locationToReach);
                    break;
                case 3:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location is kind of human location.");
                    navigationSucceeded = tryNavigation(locationToReach);
                    break;
                default:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location is kind of UNKNOWN.");
                    navigationSucceeded = tryNavigation(locationToReach);
                    break;
            }

            if (navigationSucceeded)
            {
                return (int)States.Navigate_Succeeded;
            }
            else
            {
                return (int)States.Navigate_Failed;
            }
        }

        private int Navigate_Succeeded(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Navigate_Succeeded state reached.");
            //return the arms to home position
            if (!(SMConfiguration.ARMS_usedArm == ""))
                cmdMan.ARMS_goto(SMConfiguration.ARMS_home, 10000);
            cmdMan.SPG_GEN_say(navigSucceded, 3000);
            finalStatus = Status.OK;
            // TODO: Change the next status
            return (int)States.FinalState;
        }

        private int Navigate_Failed(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Navigation_Failed state reached.");

            cmdMan.SPG_GEN_say(navigFailed, 3000);
            finalStatus = Status.Failed;
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

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
	public class DefaultStateMachineSM
	{

        #region Enums        
        /// <summary>
        ///  States of the "DefaultStateMachine" state machine
        /// </summary>
        private enum States
        {
            /// <summary>
            ///  Configuration state (variables setup)
            /// </summary>
            InitialState,
            /// <summary>
            ///  Tho robot enters to the arena
            /// </summary>
            EnterArena,
            /// <summary>
            ///  The robot goes to a predefined location in the map (in front of a table) and aligns with a table
            /// </summary>
            GetCloseToTable,
            /// <summary>
            ///  The robot search for an object and take it
            /// </summary>
            SearchAndTakeObject,
            /// <summary>
            ///  The robot drops an object 
            /// </summary>
            DropObject,
            /// <summary>
            ///  The robot leaves the arena
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
        /// Stores all the configuration variables for this state machine
        /// </summary>
        private DefaultStateMachine_WORLD SMConfiguration;
        /// <summary>
        /// Counts the number of times the robot tries to perform an action
        /// </summary>
        private int attemptCounter;
        /// <summary>
        /// Flag to know if the robot already has an object in his hand
        /// </summary>
        private bool objectTaken;
        /// <summary>
        /// Stores which arm the robot uses to grasp an object
        /// </summary>
        private string[] armsOrder;
        /// <summary>
        /// Stores a MVN_PLN-location
        /// </summary>
        private string locationToReach;
		#endregion

		#region Constructors
        /// <summary>
        /// Creates a state machine to navigate to a predefined MVN-PLN location.
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        public DefaultStateMachineSM(HAL9000Brain brain, HAL9000CmdMan cmdMan)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            finalStatus = Status.Ready;
            attemptCounter = 0;
            objectTaken = false;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            SM.AddState(new FunctionState((int)States.EnterArena, EnterArena));
            SM.AddState(new FunctionState((int)States.GetCloseToTable, GetCloseToTable));
            SM.AddState(new FunctionState((int)States.DropObject, DropObject));
            SM.AddState(new FunctionState((int)States.LeaveArena, LeaveArena));
            SM.AddState(new FunctionState((int)States.SearchAndTakeObject, SearchAndTakeObject));
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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Test Finished");
            return this.finalStatus;
        }
        #endregion

        #region States
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> State reached InitialState");

            this.finalStatus = Status.StillRunning;

            //Load the WORLD configuration for this test
            SMConfiguration = new DefaultStateMachine_WORLD();

            return (int)States.EnterArena;
        }

        /// <summary>
        /// The robot enters to the arena an goes to the MVN_PLN EntranceLocation
        /// </summary>
        private int EnterArena(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> State reached EnterArena");

            SM_EnterArena.FinalStates enterArena_status;
            SM_EnterArena enterArena = new SM_EnterArena(brain, cmdMan, SMConfiguration.EntranceLocation);
            enterArena_status = enterArena.Execute();

            if (enterArena_status == SM_EnterArena.FinalStates.OK)
            {
                if (!cmdMan.ARMS_goto(SMConfiguration.ArmsNavigationPosition, 3000))
                    if (!cmdMan.ARMS_goto(SMConfiguration.ArmsNavigationPosition, 3000))
                        cmdMan.ARMS_goto(SMConfiguration.ArmsNavigationPosition, 3000);

                return (int)States.GetCloseToTable;
            }
            else
                return (int)States.EnterArena;
        }

        /// <summary>
        /// The robot navigates to a predefined location in MVN_PPLN, then aligns with a table using vision
        /// the MVN_PLN location must be in front of a table
        /// </summary>
        /// <returns>The next state: searchAndTakeObject if the robot doesn't have an object in his hand,
        /// DropObject if the robot has an object in his hand</returns>
        private int GetCloseToTable(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> State reached GetCloseToTable");

            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Going to location: " + SMConfiguration.ObjectTableLocation);

            if (objectTaken)
                locationToReach = SMConfiguration.DropTableLocation;
            else
                locationToReach = SMConfiguration.ObjectTableLocation;

            if (brain.GetCloseToTable(locationToReach, 15000))
            {
                cmdMan.SPG_GEN_asay(SMConfiguration.TableArrivedMessage, 2000);
                attemptCounter = 0;
                if(objectTaken)
                    return (int)States.DropObject;
                else
                    return (int)States.SearchAndTakeObject;
            }

            if (attemptCounter < 3)
            {
                attemptCounter++;
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't reach location, trying again.");
                return (int)States.GetCloseToTable;
            }

            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't reach location, will try to continue with the test.");
            attemptCounter = 0;
            if (objectTaken)
                return (int)States.DropObject;
            else
                return (int)States.SearchAndTakeObject;
        }

        /// <summary>
        /// The robot tries to find an object and take it
        /// </summary>
        private int SearchAndTakeObject(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> State reached SearchAndTakeObject");

            SM_SearchAndTakeObject smSearchAndTakeObject = new SM_SearchAndTakeObject(brain, cmdMan, true, new string[] { "objects", "objects" }, false, 1, false);
            SM_SearchAndTakeObject.FinalStates searchAndTake_finalState= smSearchAndTakeObject.Execute();

            armsOrder = smSearchAndTakeObject.ArmsOrder;

            if (searchAndTake_finalState == SM_SearchAndTakeObject.FinalStates.OK)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Object succesfully taken.");
                attemptCounter = 0;
                objectTaken = true;
                cmdMan.MVN_PLN_move(-0.35, 60000);

                if (!this.cmdMan.ARMS_goto(SMConfiguration.ArmsObjectTakenPosition, 10000))
                    if (!this.cmdMan.ARMS_goto(SMConfiguration.ArmsObjectTakenPosition, 10000))
                        this.cmdMan.ARMS_goto(SMConfiguration.ArmsObjectTakenPosition, 10000);

                return (int)States.GetCloseToTable;
            }

            if (attemptCounter < 3)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't find and take the object. Trying again.");
                attemptCounter++;
                return (int)States.SearchAndTakeObject;
            }
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't find and take the object. Will try to continue with the test.");
            attemptCounter = 0;
            return (int)States.GetCloseToTable;
        }

        /// <summary>
        /// The robot drops the object
        /// </summary>
        private int DropObject(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> State reached DropObject");

            cmdMan.ST_PLN_drop(armsOrder[0], 30000);

            cmdMan.MVN_PLN_move(-0.35, 60000);

            if (cmdMan.ARMS_goto(SMConfiguration.ArmsNavigationPosition, 10000))
                if (cmdMan.ARMS_goto(SMConfiguration.ArmsNavigationPosition, 10000))
                    cmdMan.ARMS_goto(SMConfiguration.ArmsNavigationPosition, 10000);

            return (int)States.LeaveArena;
        }

        /// <summary>
        /// The robot leaves the arena
        /// </summary>
        private int LeaveArena(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> State reached LeaveArena");

            if(!cmdMan.MVN_PLN_getclose(SMConfiguration.LeaveLocation, 15000))
                if (!cmdMan.MVN_PLN_getclose(SMConfiguration.LeaveLocation, 15000))
                    cmdMan.MVN_PLN_getclose(SMConfiguration.LeaveLocation, 15000);

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

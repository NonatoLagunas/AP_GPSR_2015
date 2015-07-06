using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;

namespace ActionPlanner.ComplexActions
{
    class SM_WaitForRequest
    {
        #region Status Enums
        /// <summary>
        /// States of the "SearchAndGoToRequest" State Machine
        /// </summary>
        private enum States
        {
            /// <summary>
            /// initial state of a SearchAndGoToRequestsm
            /// </summary>
            InitialState,
            /// <summary>
            /// Search for a human request.
            /// </summary>
            SearchRequest,
            /// <summary>
            /// Go to the request location.
            /// </summary>
            GoToRequest,
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
            RequestNotFound,
			/// <summary>
			/// Human's position could not be reached.
			/// </summary>
			HumanNotReachable
        }
        #endregion

        #region Variables

        private readonly HAL9000Brain brain;
        private readonly HAL9000CmdMan cmdMan;
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
		private byte attemptCounter;
        private string requestId;

        #endregion

        /// <summary>
        /// Creates a state machine to enter the arena up to a location known by the motion planner.
        /// </summary>
        /// <param name="brain">A HAL9000Brain instance</param>
        /// <param name="location">A location known by the motion planner where the robot should try to go when entering the arena.</param>
        public SM_WaitForRequest(HAL9000Brain brain, HAL9000CmdMan cmdMan)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;
			this.attemptCounter = 0;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, new SMStateFuncion(InitialState)));
            SM.AddState(new FunctionState((int)States.SearchRequest, new SMStateFuncion(SearchRequest)));
            SM.AddState(new FunctionState((int)States.GoToRequest, new SMStateFuncion(GoToRequest)));
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
			brain.lastRecoGestures.Clear();
			cmdMan.HEAD_lookat(0.0, -0.2, 5000);
            brain.SayAsync("I am looking for requests, please raise your hand above your head.");
            return (int)(States.SearchRequest);
        }

        private int SearchRequest(int currentState, object o)
        {
            bool gestureFound = false;
            for (int i = 0; i < 18; ++i)
            {
                Thread.Sleep(500);
                if (gestureFound = brain.FindSpecificGesture("request", out requestId))
                {
					cmdMan.MVN_PLN_fixhuman(requestId, 4000);
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Gesture found.");
                    brain.SayAsync("I found a request.");
                    break;
                }
            }
            if (!gestureFound)
            {
				if (attemptCounter > 7)
				{
					attemptCounter = 0;
					finalState = FinalStates.RequestNotFound;
					return (int)States.FinalState;
				}
				cmdMan.MVN_PLN_move(0, -Math.PI / 4, 4000);
                if (attemptCounter % 2 == 1)
					brain.SayAsync("I am looking for requests, please raise your hand above your head.");
				attemptCounter++;
                return currentState;
            }
            // Si encontro humano, pasa al siguiente estado
            attemptCounter = 0;
            return (int)States.GoToRequest;
        }

        private int GoToRequest(int currentState, object o)
        {
            if (attemptCounter < 3)
            {
                if (cmdMan.MVN_PLN_getclose("human " + requestId, 60000))
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Getclose to human success at attempt" + attemptCounter);
                    finalState = FinalStates.OK;
                    return (int)States.FinalState;
                }
                else
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Getclose to human FAILED at attempt" + attemptCounter);
                    attemptCounter++;
                    return currentState;
                }
            }
            attemptCounter = 0;
			finalState = FinalStates.HumanNotReachable;
            return (int)States.FinalState;
        }

        private int FinalState(int currentState, object o)
        {
            return currentState;
        }

        #endregion
    }
}

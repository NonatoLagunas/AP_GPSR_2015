using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.StateMachines;
using Robotics.Controls;
using System.Threading;

namespace ActionPlanner.ComplexActions
{
    class SM_SearchAndTakeObject
    {

        #region enums
        enum States
        {
           SearchCloseObjects, SetObjectToTake, TakeObject, FinalState
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

        private HAL9000Brain brain;
        private HAL9000CmdMan cmdMan;
		private String ObjectToFind;
		private String SayObjectName;
        private string foundObject;
        private byte foundObjectsCount;
        private int attemptCounter;
		private int searchAttemptCounter;
        private bool anyObject;
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
		List<string> objectsFound;
		private int objectFoundIndex;
		private bool useTakeHandOver;

        #endregion

        #region Constructor
        public SM_SearchAndTakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string ObjectToFind, bool useHandOver) {
            SM = new FunctionBasedStateMachine();
            this.brain = brain;
            this.cmdMan = cmdMan;
			this.attemptCounter = 0;
			this.searchAttemptCounter = 0;
            this.finalState = FinalStates.StillRunning;

			this.objectsFound = new List<string>();
			this.useTakeHandOver = useHandOver;

            this.anyObject = anyObject;
            if (anyObject)
				this.ObjectToFind = "objects";
            else
                this.ObjectToFind = ObjectToFind;

            this.foundObject = ObjectToFind;

            SM.AddState(new FunctionState((int)States.SearchCloseObjects, new SMStateFuncion(SearchCloseObjects)));
			SM.AddState(new FunctionState((int)States.SetObjectToTake, new SMStateFuncion(SetObjectToTake)));
            SM.AddState(new FunctionState((int)States.TakeObject, new SMStateFuncion(TakeObject)));
            SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState)));

            SM.SetFinalState((int)States.FinalState);
        }

		public SM_SearchAndTakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string ObjectToFind) : this(brain, cmdMan, anyObject, ObjectToFind, true) { }
        #endregion

        #region Properties

        public string FoundObject
        {
            get { return this.foundObject; }
        }

        public byte FoundObjectsCount
        {
            get { return this.foundObjectsCount; }
        }

        #endregion

        #region State functions
        int SearchCloseObjects(int currentState, object o)
        {
            if (cmdMan.ST_PLN_fashionfind_object(ObjectToFind, 180000, out objectsFound))
            {
				attemptCounter = 0;
                TextBoxStreamWriter.DefaultLog.WriteLine("Objects found.");
				return (int)States.SetObjectToTake;
            }

			if (attemptCounter < 2)
			{
				attemptCounter++;
				TextBoxStreamWriter.DefaultLog.WriteLine("No objects were found, trying again");
				this.brain.SayAsync("I did not find the " + ObjectToFind + ". I will try again");
				//this.cmdMan.MVN_PLN_move(-0.25, 0, 0, 5000);
				Thread.Sleep(2500);
				return currentState;
			}

			this.brain.SayAsync("I did not find " + (anyObject?"any":"") + ObjectToFind);
            TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object NOT found, SM was not successful.");
			attemptCounter = 0;
            this.finalState = FinalStates.Failed;
            return (int)States.FinalState;
        }

		int SetObjectToTake(int currentState, object o)
		{
			objectFoundIndex = 0;
			this.foundObjectsCount = (byte)objectsFound.Count;

			if (this.foundObjectsCount == 0)
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("No reachable objects were found, SM was not successful.");
				this.finalState = FinalStates.Failed;
				return (int)States.FinalState;
			}

			if (!anyObject)
			{
				if (objectsFound.Contains(ObjectToFind))
				{
					objectFoundIndex = objectsFound.IndexOf(ObjectToFind);
					TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object found.");
					return (int)States.TakeObject;
				}

				if (searchAttemptCounter < 2)
				{
					searchAttemptCounter++;
					TextBoxStreamWriter.DefaultLog.WriteLine("object NOT found, trying again");
					this.brain.SayAsync("I did not find the " + ObjectToFind + ". I will try again");
					//this.cmdMan.MVN_PLN_move(-0.25, 0, 0, 5000);
					Thread.Sleep(2500);
					return (int)States.SearchCloseObjects;
				}

				TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object NOT found, test failed.");
				this.finalState = FinalStates.Failed;
				return (int)States.FinalState;
			}

			ObjectToFind = objectsFound[0];
			foundObject = objectsFound[0];
			attemptCounter = 0;
			TextBoxStreamWriter.DefaultLog.WriteLine("Some Object found.");
			return (int)States.TakeObject;
		}

        int TakeObject(int currentState, object o)
        {
			if (ObjectToFind.Length >= 7 && ObjectToFind.Substring(0, 7).ToLower().Equals("unknown"))
				SayObjectName = "unknown object";
			else
				SayObjectName = ObjectToFind;

			brain.SayAsync("I will take the " + SayObjectName);

			Thread.Sleep(5000);
			if (cmdMan.ST_PLN_takeobject(ObjectToFind, 180000))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("object taken");
			}
			else
			{
				if (useTakeHandOver)
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("cant take object, using takehandover");
					brain.SayAsync(" I cant reach the " + SayObjectName);
					cmdMan.ST_PLN_takehandover(ObjectToFind, 40000);
				}
				else
				{
					brain.SayAsync("I cant reach the " + SayObjectName + ", I will look for another object.");
					objectsFound.RemoveAt(objectFoundIndex);
					TextBoxStreamWriter.DefaultLog.WriteLine("Cant take the object, trying to take another one.");
					return (int)States.SetObjectToTake;
				}
			}
			
            this.finalState = FinalStates.OK;
            return (int)States.FinalState; 
        }

        int FinalState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("head to 0,0");
            cmdMan.HEAD_lookat(0, 0, 10000);
			cmdMan.MVN_PLN_move(-0.4, 0.0, 10000);
            return currentState;
        }
        #endregion

        #region Run State Machine
        public FinalStates Execute()
        {
			TextBoxStreamWriter.DefaultLog.WriteLine("Executing: SM_SearchAndTakeObject");
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
    }
}

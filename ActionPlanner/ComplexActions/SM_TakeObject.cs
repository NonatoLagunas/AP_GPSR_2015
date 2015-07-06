using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.StateMachines;
using Robotics.Controls;
using System.Threading;

namespace ActionPlanner.ComplexActions
{
    class SM_TakeObject
    {

        #region enums
        enum States
        {
			GetCloseObjectLocation,
			SearchCloseObjects, 
			TakeObject, 
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

        private HAL9000Brain brain;
        private HAL9000CmdMan cmdMan;
		private String ObjectToFind;
		private String SayObjectName;
        private string foundObject;
        private byte foundObjectsCount;
        private String objectLocation;
        private int attemptCounter;
        private bool anyObject;
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
		private bool succesGetClose;

        #endregion

        #region Constructor
        
		
		public SM_TakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string ObjectToFind, string objectLocation, bool succesGetClose) 
		
		{
            SM = new FunctionBasedStateMachine();
            this.brain = brain;
            this.cmdMan = cmdMan;
            this.objectLocation = objectLocation;
            attemptCounter = 0;
            this.finalState = FinalStates.StillRunning;

            this.anyObject = anyObject;
            if (anyObject)
                this.ObjectToFind = ObjectToFind;
            else
                this.ObjectToFind = "objects";

            this.foundObject = ObjectToFind;
			this.succesGetClose = succesGetClose;

            SM.AddState(new FunctionState((int)States.GetCloseObjectLocation, new SMStateFuncion(GetCloseObjectLocation)));
            SM.AddState(new FunctionState((int)States.SearchCloseObjects, new SMStateFuncion(SearchCloseObjects)));
            SM.AddState(new FunctionState((int)States.TakeObject, new SMStateFuncion(TakeObject)));
            SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState)));

            SM.SetFinalState((int)States.FinalState);
        }
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

        #region Estado : _GetCloseObjectLocation
       
		int GetCloseObjectLocation(int currentState, object o)
        {
            cmdMan.HEAD_lookat(0, 0, 5000);

			TextBoxStreamWriter.DefaultLog.WriteLine("Getting close to: " + this.objectLocation);

            if (cmdMan.MVN_PLN_getclose(this.objectLocation, 600000))
            {
                cmdMan.SPG_GEN_asay("i have arrived to the " + this.objectLocation, 10000);
                TextBoxStreamWriter.DefaultLog.WriteLine("Location reached: " + this.objectLocation);
                this.attemptCounter = 0;
                return (int)States.SearchCloseObjects;
            }

            if (attemptCounter < 3)
            {
                this.attemptCounter++;
                TextBoxStreamWriter.DefaultLog.WriteLine("Cant get close to the specified location, trying again.");
				/// para interface
				cmdMan.SPG_GEN_asay("I can't get close to the" + this.objectLocation, 10000);
                return currentState;
            }

			//this.brain.SayAsync("I could not get close to the specified location");
            this.attemptCounter = 0;
            TextBoxStreamWriter.DefaultLog.WriteLine("Cant get close to the specified location, will try to continue with the test.");
            return (int)States.SearchCloseObjects;
        }
        #endregion
        
        #region Estado : SearchCloseObjects
       
		int SearchCloseObjects(int currentState, object o)
        {
			List<string> objectsFound = new List<string>();

            if (cmdMan.ST_PLN_fashionfind_object(ObjectToFind, 90000, out objectsFound))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("Objects found.");
                this.foundObjectsCount = (byte)objectsFound.Count;
                if (!anyObject)
                {
                    if (objectsFound.Contains(ObjectToFind))
                    {
                        TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object found.");
                        attemptCounter = 0;
                        return (int)States.TakeObject;
                    }

                    if (attemptCounter < 3)
                    {
                        attemptCounter++;
                        TextBoxStreamWriter.DefaultLog.WriteLine("object NOT found, trying again");
                        this.brain.SayAsync("I did not find the " + ObjectToFind + ". I will try again");
                        //this.cmdMan.MVN_PLN_move(-0.25, 0, 0, 5000);
                        Thread.Sleep(2500);
                        return currentState;
                    }

                    TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object NOT found, will try to continue the test.");
                    attemptCounter = 0;
                    this.finalState = FinalStates.Failed;
                    return (int)States.FinalState;

                    /*foreach (string obj in objectsFound)
                    {
                        //cmdMan.SPG_GEN_say(obj);

                        if (obj == objectToBring)
                        {
                            TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object founded");
                            nextStage = 70;
                            founded = true;
                        }
                    }*/
                }
                else
                {
                    ObjectToFind = objectsFound[0];
                    foundObject = objectsFound[0];
                    attemptCounter = 0;
                    TextBoxStreamWriter.DefaultLog.WriteLine("Some Object found.");
                    return (int)States.TakeObject;
                }
            }
            else
            {
				if (attemptCounter < 3)
                {
                    attemptCounter++;
                    TextBoxStreamWriter.DefaultLog.WriteLine("No objects were found, trying again");
                    this.brain.SayAsync("I did not find the " + ObjectToFind + ". I will try again");
                    this.cmdMan.MVN_PLN_move(-0.25, 0, 0, 5000);
                    Thread.Sleep(2500);
                    return currentState;
                }

				this.brain.SayAsync("I did not find " + (anyObject?"any":"") + ObjectToFind + " in " + objectLocation);
                TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object NOT found, SM was not successful.");
                attemptCounter = 0;
                this.finalState = FinalStates.Failed;
                return (int)States.FinalState;
            }
        }
        #endregion
        
        #region Estado: TakeObject
        int TakeObject(int currentState, object o)
        {
			if (ObjectToFind.Substring(0, 7).ToLower().Equals("unknown"))
				SayObjectName = "unknown object";
			else
				SayObjectName = ObjectToFind;

			if(this.succesGetClose)
			{
				brain.SayAsync("I will take the " + SayObjectName);
				
				Thread.Sleep(5000);
				if (cmdMan.ST_PLN_takeobject(ObjectToFind, 105000))
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("object taken");
				}
				else
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("cant take object, using takehandover");
					brain.SayAsync(" I cant take the " + SayObjectName);
					cmdMan.ST_PLN_takehandover(ObjectToFind, 40000);
				}
			}
			else
			{
				Thread.Sleep(5000);
				TextBoxStreamWriter.DefaultLog.WriteLine("Getclose not succesfull, using takehandover");
				brain.SayAsync("I can't take the " + SayObjectName);
				cmdMan.ST_PLN_takehandover(ObjectToFind, 40000);
			}
			
            this.finalState = FinalStates.OK;
            return (int)States.FinalState; 
        }
        #endregion

        #region Estado: FinalState
        int FinalState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("head to 0,0");
            cmdMan.HEAD_lookat(0, 0, 10000);

            /*if (cmdMan.MVN_PLN_getclose(frontentrance, 120000))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("Location reached" + objectLocation);
                nextStage = 90;/////
                break;
            }
            TextBoxStreamWriter.DefaultLog.WriteLine("Cant reach to " + objectLocation);
            nextStage = 90;////Si no puede reachear la posición se sigue con lo siguiente
            break;*/
            return currentState;
        }
        #endregion

        #region Run State Machine
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
    }
}

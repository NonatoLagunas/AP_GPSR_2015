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
			/// The object was not taken
			/// </summary>
			FoundButNotTaken,
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
        private String [] objectsToFind;
		private String SayObjectName;
        private string[] foundObjects;
        private string[] armsOrder;
		private byte foundObjectsCount;
		private byte totalFoundObjects;
        private int attemptCounter;
		private int searchAttemptCounter;
        private bool anyObject;
        private FunctionBasedStateMachine SM;
        private FinalStates finalState;
		List<string> objectsFound;
		private int objectFoundIndex;
		private bool useTakeHandOver;
        private int searchAttempts;
        private bool twoArms;
        private int takeCounter;
        private bool firstObjectTaken;
        private string armToUse;

        private bool laTaken;
        #endregion

        #region Constructor
        public SM_SearchAndTakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string [] objectsToFind, bool useHandOver, int searchAttempts = 2, bool twoArms=false) {
            SM = new FunctionBasedStateMachine();
            this.brain = brain;
            this.cmdMan = cmdMan;
			this.attemptCounter = 0;
			this.searchAttemptCounter = 0;
            this.searchAttempts = searchAttempts;
            this.finalState = FinalStates.StillRunning;
            this.twoArms = twoArms;
            this.takeCounter = 0;
            this.firstObjectTaken=false;
            /**********************/
            this.laTaken = false;
            this.armToUse = "";

			this.objectsFound = new List<string>();
			this.useTakeHandOver = useHandOver;

            this.objectsToFind = new string[2];
            this.foundObjects = new string[2];
            this.armsOrder = new string[2];
            this.foundObjects[0] = "";
            this.foundObjects[1] = "";

            this.armsOrder[0] = "";
            this.armsOrder[1] = "";

            this.anyObject = anyObject;
            if (anyObject)
            {
                //this.ObjectToFind = "objects";
                this.objectsToFind[0] = "objects";
                this.objectsToFind[1] = "";
            }
            else
            {
                this.objectsToFind[0] = objectsToFind[0];
                this.objectsToFind[1] = objectsToFind[1];
            }

            //this.objectsToTake = new string[2];
            //this.objectsToTake[0] = "";
            //this.objectsToTake[1] = "";
            //this.foundObjects = new string[2];
            //this.foundObjects[0] = ObjectToFind;
            //this.foundObjects[1] = "";

			//objectsToTake = new string[2];

            SM.AddState(new FunctionState((int)States.SearchCloseObjects, new SMStateFuncion(SearchCloseObjects)));
			SM.AddState(new FunctionState((int)States.SetObjectToTake, new SMStateFuncion(SetObjectToTake)));
            SM.AddState(new FunctionState((int)States.TakeObject, new SMStateFuncion(TakeObject)));
            SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState)));

            SM.SetFinalState((int)States.FinalState);
        }

        public SM_SearchAndTakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string [] objectsToFind) : this(brain, cmdMan, anyObject, objectsToFind, true) { }
        public SM_SearchAndTakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string[] objectsToFind, int searchAttempts) : this(brain, cmdMan, anyObject, objectsToFind, true, searchAttempts) { }
        public SM_SearchAndTakeObject(HAL9000Brain brain, HAL9000CmdMan cmdMan, bool anyObject, string[] objectsToFind, int searchAttempts, bool twoArms) : this(brain, cmdMan, anyObject, objectsToFind, true, searchAttempts, twoArms) { }
        #endregion

        #region Properties

        public string[] FoundObject
        {
            get { return this.foundObjects; }
        }

        public string[] ArmsOrder
        {
            get { return this.armsOrder; }
        }

        public byte FoundObjectsCount
        {
			get { return this.totalFoundObjects; }
        }

        #endregion

        #region State functions
        int SearchCloseObjects(int currentState, object o)
        {
            if (cmdMan.ST_PLN_fashionfind_object(objectsToFind[takeCounter], 270000, out objectsFound))
            {
				attemptCounter = 0;
				TextBoxStreamWriter.DefaultLog.WriteLine("Objects found.");
				this.totalFoundObjects = (byte)objectsFound.Count;
				return (int)States.SetObjectToTake;
            }
            /***28-05-2013*////
			if (attemptCounter < searchAttempts)
			{
				attemptCounter++;
				TextBoxStreamWriter.DefaultLog.WriteLine("No objects were found, trying again");
                this.brain.SayAsync("I did not find the " + objectsToFind[takeCounter] + ". I will try again");
				//this.cmdMan.MVN_PLN_move(-0.25, 0, 0, 5000);
				Thread.Sleep(2500);
				return currentState;
			}

            this.brain.SayAsync("I did not find " + (anyObject ? "any " : "the ") + objectsToFind[takeCounter]);
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
                if (!firstObjectTaken)
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("No reachable objects were found, SM was not successful.");
                    this.finalState = FinalStates.Failed;
                    return (int)States.FinalState;
                }
                else
                {
                    this.finalState = FinalStates.OK;
                    return (int)States.FinalState; 
                }
			}

			if (!anyObject)
			{
                if (objectsFound.Contains(objectsToFind[takeCounter]))
				{
                    objectFoundIndex = objectsFound.IndexOf(objectsToFind[takeCounter]);
					TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object found.");
					return (int)States.TakeObject;
				}
                ///******28-05-2013*******////////////
				if (searchAttemptCounter < searchAttempts )
				{
					searchAttemptCounter++;
					TextBoxStreamWriter.DefaultLog.WriteLine("object NOT found, trying again");
                    this.brain.SayAsync("I did not find the " + objectsToFind[takeCounter] + ". I will try again");
					//this.cmdMan.MVN_PLN_move(-0.25, 0, 0, 5000);
					Thread.Sleep(2500);
					return (int)States.SearchCloseObjects;
				}

				TextBoxStreamWriter.DefaultLog.WriteLine("Requested Object NOT found, test failed.");
				this.finalState = FinalStates.Failed;
				return (int)States.FinalState;
			}
			objectsToFind[takeCounter] = objectsFound[0];

			attemptCounter = 0;
			TextBoxStreamWriter.DefaultLog.WriteLine("Some Object found.");
			return (int)States.TakeObject;
		}

        int TakeObject(int currentState, object o)
        {
			string leftArm="",rightArm="";
            if (firstObjectTaken)
                armToUse = (laTaken) ? "right" : "left";
            else
                armToUse = "";

            armToUse = "left";

            if (objectsToFind[takeCounter].Length >= 7 && objectsToFind[takeCounter].Substring(0, 7).ToLower().Equals("unknown"))
				SayObjectName = "unknown object";
			else
                SayObjectName = objectsToFind[takeCounter];


			//TEMP
			
			//brain.SayAsync("I will take the object");

			//Thread.Sleep(10000);
            //reemplazar por bnuevo comando para forzar a que tome un objeto con un brazo dado
            //if (cmdMan.ST_PLN_takeobject(objectsToFind[takeCounter], armToUse, out string, 180000))
			
            //if (cmdMan.ST_PLN_takeobject(objectsToFind[takeCounter], 180000))
			cmdMan.ST_PLN_takeobject(objectsToFind[takeCounter], armToUse, out leftArm, out rightArm, 180000);
			if (!(leftArm == "empty" && rightArm == "empty"))
			{
				brain.SayAsync("I got the " + SayObjectName);
                TextBoxStreamWriter.DefaultLog.WriteLine("object taken: " + objectsToFind[takeCounter]);
                foundObjects[takeCounter] = objectsToFind[takeCounter];
				//arctualizar la bandera del brazo cupado laTaken}
				laTaken = (leftArm == "empty") ? false : true;
				if (!firstObjectTaken)
					armsOrder[0] = (laTaken) ? "left" : "right";
				else
					armsOrder[1] = (armsOrder[0] == "right") ? "left" : "right";

				TextBoxStreamWriter.DefaultLog.WriteLine(objectsToFind[takeCounter] + " using the " + ((!firstObjectTaken) ? armsOrder[0] : armsOrder[1]) + " arm");
			}
			else
			{
				if (useTakeHandOver)
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("cant take object, using takehandover");
					brain.SayAsync(" I cant reach the " + SayObjectName);
                    //brain.SayAsync("I will point at the " + objectsToFind[takeCounter] + ".");
                    //TODO: point at object
                    cmdMan.ST_PLN_takehandover(objectsToFind[takeCounter], 40000);
                    foundObjects[takeCounter] = objectsToFind[takeCounter];

					//arctualizar la bandera del brazo cupado laTaken
					laTaken = (leftArm == "empty") ? false : true;
                    if (!firstObjectTaken)
                        armsOrder[0] = (laTaken) ? "left" : "right";
                    else
						armsOrder[1] = (armsOrder[0] == "right") ? "left" : "right";

				}
				else
				{
					finalState = FinalStates.FoundButNotTaken;
					foundObjects[takeCounter] = "";
					//TEMP
					//brain.SayAsync("I cant reach the " + SayObjectName + ", I will look for another object.");
					//brain.SayAsync("I cant reach the object, I will look for another object.");
					TextBoxStreamWriter.DefaultLog.WriteLine("Cant take the object, trying to take another one.");
					objectsFound.RemoveAt(0);
					return (int)States.SetObjectToTake;
				}
            }

            if (!firstObjectTaken&&twoArms)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("First object Taken.");
                /*despues de la primer ejecucion checar con que brazo tomó el objeto y actualizar la bandera laTaken(true, toma con el brazo izq, else si no )*/
                //laTaken=false
                takeCounter++;
				firstObjectTaken = true;
				objectsFound.RemoveAt(0);
                return (int)States.SetObjectToTake; 
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

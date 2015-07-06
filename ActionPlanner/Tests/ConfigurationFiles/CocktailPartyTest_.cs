using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.StateMachines;
using Robotics.Controls;
using ActionPlanner.ComplexActions;

namespace ActionPlanner.Test
{

    class CocktailPartyTest
    {

        private struct order
        {
            public string personName;
            public string objectName;
            public double x;
            public double y;
            public double angle;
			public bool wasAbleToRememberFace;
        }

        #region Enums
        private enum States
        {
			EnterArena,
			GoToPartyRoom,
            WaitForRequest,
            NoGestureRecognized,
            IdentifyPerson,
            WaitForOrder,
            WaitForConfirmation,
			EnqueueOrder,
			GoToDrinksLocation,
            SearchAndTakeObject,
            GoToHumanLocation,
            DeliverDrink,
            LeaveArena,
            FinalState
        }

        public enum FinalStates
        {
            StillRunning,
            OK,
            Failed
        }
        #endregion

        #region Variables
        private readonly HAL9000Brain brain;
        private readonly HAL9000CmdMan cmdMan;
        private FinalStates finalState;
        bool tryToOpenDoor;
        string entranceLocation;
		string partyroomLocation;
        string drinksLocation;
        string exitLocation;
		string defaultDrink;
        string[] defaultNames;
        bool gestureFailed;
        FunctionBasedStateMachine SM;
        order currentOrder;
        //Queue<order> orders;
		int ordersCount;
        List<string> rejectedNames;
        int attemptCounter;
        #endregion

        #region Constructors

        public CocktailPartyTest(HAL9000Brain brain, HAL9000CmdMan cmdMan)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Executing Cocktail Party Test");
			this.brain.Status.TestBeingExecuted = "Cocktail Party";
			this.brain.OnStatusChanged(new HAL9000StatusArgs(this.brain.Status));

            this.finalState = FinalStates.StillRunning;
            tryToOpenDoor = false;
            entranceLocation = "frontentrance";
			partyroomLocation = "partyroom";
            drinksLocation = "drinks";
            exitLocation = "exit";
			defaultDrink = "mangojuice";
            attemptCounter = 0;
            gestureFailed = false;
            defaultNames = new string[] { "Alan", "Albert", "Angel" };
            SM = new FunctionBasedStateMachine();
			//orders = new Queue<order>(3);
			ordersCount = 0;
			rejectedNames = new List<string>(3);

			SM.AddState(new FunctionState((int)States.EnterArena, new SMStateFuncion(EnterArena)));
			SM.AddState(new FunctionState((int)States.GoToPartyRoom, new SMStateFuncion(GoToPartyRoom)));
			SM.AddState(new FunctionState((int)States.WaitForRequest, new SMStateFuncion(WaitForRequest)));
            SM.AddState(new FunctionState((int)States.IdentifyPerson, new SMStateFuncion(IdentifyPerson)));
            SM.AddState(new FunctionState((int)States.WaitForOrder, new SMStateFuncion(WaitForOrder)));
            SM.AddState(new FunctionState((int)States.NoGestureRecognized, new SMStateFuncion(NoGestureRecognized)));
            SM.AddState(new FunctionState((int)States.WaitForConfirmation, new SMStateFuncion(WaitForConfirmation)));
			SM.AddState(new FunctionState((int)States.EnqueueOrder, new SMStateFuncion(EnqueueOrder)));
			SM.AddState(new FunctionState((int)States.GoToDrinksLocation, new SMStateFuncion(GoToDrinksLocation)));
			SM.AddState(new FunctionState((int)States.SearchAndTakeObject, new SMStateFuncion(SearchAndTakeObject)));
            SM.AddState(new FunctionState((int)States.GoToHumanLocation, new SMStateFuncion(GoToHumanLocation)));
            SM.AddState(new FunctionState((int)States.DeliverDrink, new SMStateFuncion(DeliverDrink)));
            SM.AddState(new FunctionState((int)States.LeaveArena, new SMStateFuncion(LeaveArena)));
			SM.AddState(new FunctionState((int)States.FinalState, new SMStateFuncion(FinalState)));

            SM.SetFinalState((int)States.FinalState);
        }

        #endregion

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

        private int EnterArena(int currentState, object o)
        {
            SM_EnterArena enterArenaSM = new SM_EnterArena(brain, cmdMan, entranceLocation, tryToOpenDoor);
            if(enterArenaSM.Execute() != SM_EnterArena.FinalStates.OK)
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot enter the arena, trying to continue the test.");

            return (int)States.GoToPartyRoom;
        }

		private int GoToPartyRoom(int currentState, object o)
		{
			if (attemptCounter < 2)
			{
				this.cmdMan.ARMS_goto("standby", 8000);
				if (cmdMan.MVN_PLN_getclose(partyroomLocation, 180000))
				{
					attemptCounter = 0;
					TextBoxStreamWriter.DefaultLog.WriteLine("Arrived to party room.");
					return (int)States.WaitForRequest;
				}

				attemptCounter++;
				TextBoxStreamWriter.DefaultLog.WriteLine("Could not get close to party room, trying again.");
				return currentState;
			}

			attemptCounter = 0;
			TextBoxStreamWriter.DefaultLog.WriteLine("Could not get close to party room. Will continue with the test.");
			return (int)States.WaitForRequest;
		}

        private int WaitForRequest(int currentState, object o)
        {
            SM_WaitForRequest waitForRequestSM = new SM_WaitForRequest(brain, cmdMan);

            if (waitForRequestSM.Execute() == SM_WaitForRequest.FinalStates.OK)
                return (int)States.IdentifyPerson;

            return (int)States.NoGestureRecognized;
        }

        private int NoGestureRecognized(int currentState, object o)
        {
            gestureFailed = true;
            return (int)States.IdentifyPerson;
        }

        private int IdentifyPerson(int currentState, object o)
        {
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Reached state 'IdentifyPerson'");
            string[] knownNames = new string[brain.KnownPersons.Keys.Count];
            brain.KnownPersons.Keys.CopyTo(knownNames, 0);
			SM_IdentifyPerson identifyPersonSM = new SM_IdentifyPerson(brain, cmdMan, gestureFailed, knownNames, defaultNames[ordersCount], rejectedNames);
			SM_IdentifyPerson.FinalStates smFinalState = identifyPersonSM.Execute();

			if (smFinalState == SM_IdentifyPerson.FinalStates.Failed)
				currentOrder.wasAbleToRememberFace = false;
			else if (smFinalState == SM_IdentifyPerson.FinalStates.OK)
				currentOrder.wasAbleToRememberFace = true;
			else return currentState;

			gestureFailed = false;

			currentOrder.personName = identifyPersonSM.Name;
			rejectedNames.Add(currentOrder.personName);
			currentOrder.x = identifyPersonSM.X;
			currentOrder.y = identifyPersonSM.Y;
			currentOrder.angle = identifyPersonSM.Angle;

            brain.recognizedSentences.Clear();
            return (int)States.WaitForOrder;
        }

        private int WaitForOrder(int currentState, object o)
        {
			if (attemptCounter % 30 == 0)
			{
				brain.SayAsync("What would you like to order " + currentOrder.personName + "?");
			}

            SentenceImperative sentence;
            if (brain.FindUserComplexCommand(out sentence))
            {
                if (sentence.ActionClass == VerbType.Bring)
                {
                    currentOrder.objectName = sentence.DirectObject;
                    TextBoxStreamWriter.DefaultLog.WriteLine("recognized request : Bring me the " + currentOrder.objectName);

                    brain.SayAsync("Would you like me to bring you the " + currentOrder.objectName + "?");
					attemptCounter = 0;
                    return (int)States.WaitForConfirmation;
                }
            }
            Thread.Sleep(500);
			attemptCounter++;
			if (attemptCounter == 89)
			{
				attemptCounter = 0;
				brain.SayAsync("I did not understand your order.");
				currentOrder.objectName = defaultDrink;
				TextBoxStreamWriter.DefaultLog.WriteLine("No order was recognized, default object: " + currentOrder.objectName);
				brain.SayAsync("I will bring you the " + currentOrder.objectName);
				return (int)States.EnqueueOrder;
			}
            return currentState;
        }

        private int WaitForConfirmation(int currentState, object o)
        {
            if (!this.brain.WaitForUserConfirmation())
            {
                brain.recognizedSentences.Clear();
                return (int)States.WaitForOrder;
            }

			return (int)States.EnqueueOrder;
        }

		private int EnqueueOrder(int currentState, object o)
		{
			//orders.Enqueue(currentOrder);

			//if (orders.Count < 3)
			//    return (int)States.WaitForRequest;

			brain.SayAsync("I will get your beverage.");
			return (int)States.GoToDrinksLocation;
		}

		private int GoToDrinksLocation(int currentState, object o)
		{
			cmdMan.HEAD_lookat(0, 0, 10000);
			TextBoxStreamWriter.DefaultLog.WriteLine("Going to drinks location.");

			this.cmdMan.ARMS_goto("standby", 8000);
			if (brain.GetCloseToTable(drinksLocation, 180000))
			{
				brain.SayAsync("i have arrived to the drinks location");
				this.attemptCounter = 0;
				return (int)States.SearchAndTakeObject;
			}

			if (attemptCounter < 3)
			{
				attemptCounter++;
				TextBoxStreamWriter.DefaultLog.WriteLine("Cant reach drinks location, trying again");
				return currentState;
			}

			TextBoxStreamWriter.DefaultLog.WriteLine("Cant reach drinks location, will try to continue with the test");
			return (int)States.SearchAndTakeObject;
		}

		private int SearchAndTakeObject(int currentState, object o)
        {
            //currentOrder = orders.Dequeue();

            SM_SearchAndTakeObject searchAndTakeObjectSM = new SM_SearchAndTakeObject(brain, cmdMan, false, currentOrder.objectName);
			if (searchAndTakeObjectSM.Execute() != SM_SearchAndTakeObject.FinalStates.OK)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("Could not fetch order: " + currentOrder.objectName + " for " + currentOrder.personName);
				brain.SayAsync("I could not get the order for " + currentOrder.personName);
				cmdMan.MVN_PLN_move(-0.4, 250000);
				if (ordersCount < 3)
				{
					attemptCounter = 0;
					ordersCount++;
					return (int)States.GoToPartyRoom;
				}
                return (int)States.LeaveArena;
			}
			cmdMan.MVN_PLN_move(-0.4, 250000);
            return (int)States.GoToHumanLocation;
        }

        private int GoToHumanLocation(int currentState, object o)
		{
			//this.cmdMan.ARMS_goto("standby", 8000);
			if (!cmdMan.MVN_PLN_getclose(currentOrder.x, currentOrder.y, 240000))
				if (!cmdMan.MVN_PLN_getclose(currentOrder.x, currentOrder.y, 240000))
					cmdMan.MVN_PLN_getclose(currentOrder.x, currentOrder.y, 240000);

			double x, y, angle;
			cmdMan.MVN_PLN_position(out x, out y, out angle, 2000);
			cmdMan.MVN_PLN_move(0.0, currentOrder.angle - angle, 60000);

            string foundHuman;

			if (currentOrder.wasAbleToRememberFace)
			{
				if (cmdMan.ST_PLN_findhuman(currentOrder.personName, "hdtilt hdpan", 7000, out foundHuman))
				{
					brain.SayAsync("Here is your " + currentOrder.objectName + ", " + currentOrder.personName);
				}
				else
				{
					//cmdMan.HEAD_lookat(0.0, 0.0);
					brain.SayAsync(currentOrder.personName + ", please stand in front of me to take your " + currentOrder.objectName);
				}
			}
			else
			{
				if (cmdMan.ST_PLN_findhuman("human", "hdtilt hdpan", 7000, out foundHuman))
				{
					brain.SayAsync("Here is your " + currentOrder.objectName);
				}
				else
				{
					//cmdMan.HEAD_lookat(0.0, 0.0);
					brain.SayAsync("Please stand in front of me to take your " + currentOrder.objectName);
				}
			}

            TextBoxStreamWriter.DefaultLog.WriteLine("Got back to " + currentOrder.personName + "'s position.");
            return (int)States.DeliverDrink;
        }

        private int DeliverDrink(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("delivering object");
            cmdMan.ST_PLN_deliverobject("object", 30000);
            Thread.Sleep(5000);

			//this.cmdMan.ARMS_ra_goto("home");
			this.cmdMan.ARMS_goto("standby", 8000);

			if (ordersCount < 3)
			{
				attemptCounter = 0;
				ordersCount++;
				return (int)States.GoToPartyRoom;
			}

            return (int)States.LeaveArena;
        }

        private int LeaveArena(int currentState, object o)
        {

			this.cmdMan.ARMS_goto("standby", 8000);
            if (cmdMan.MVN_PLN_getclose(exitLocation, 120000))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("Location reached: " + exitLocation);
                this.finalState = FinalStates.OK;
                return (int)States.FinalState;
            }

            if (attemptCounter < 3)
            {
                attemptCounter++;
                TextBoxStreamWriter.DefaultLog.WriteLine("Cant reach location: " + exitLocation + ", trying again");
                return currentState;
            }

            TextBoxStreamWriter.DefaultLog.WriteLine("Cant reach location: " + exitLocation + ", SM execution was NOT successful");
            this.finalState = FinalStates.Failed;
            return (int)States.FinalState;
        }

        private int FinalState(int currentState, object o)
        {
            return currentState;
        }

        #endregion
    }
}

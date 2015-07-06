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
    public class Restaurant
    {

        #region Enums
        /// <summary>
        ///  States of the state machine
        /// </summary>
        private enum States
        {
            /// <summary>
            ///  Configuration state (variables setup)
            /// </summary>
            InitialState,
            /// <summary>
            /// State in which the robot starts to follow a human (guide phase)
            /// </summary>
            FollowHuman,
            /// <summary>
            /// Listening state, the robot waits for a voice command (indicating the location of a table)
            /// </summary>
            WaitForLocationCommand,
            /// <summary>
            /// State in which the robot waits for a confirmation (it could be a table location or the kitchen location confirmation)
            /// </summary>
            WaitForLocationConfirmation,
            /// <summary>
            /// Listening state, the robot waits for a voice command (indicating the direction of a table)
            /// </summary>
            WaitForTableDirectionCommand,
            /// <summary>
            /// State in which the robot asks for the table direction confirmation
            /// </summary>
            WaitForTableDirectionConfirmation,
            /// <summary>
            /// Listening state, the robot tries to recognize an ordering command (take an order for a certain table)
            /// </summary>
            WaitForOrderingCommand,
            /// <summary>
            /// State in which the robot waits for a confirmation (table number to take an order from)
            /// </summary>
            WaitForOrderTableConfirmation,
            /// <summary>
            /// State in which the robot goes to a table to take an order
            /// </summary>
            GoToOrderTable,
            /// <summary>
            /// State in which the robot take an order
            /// </summary>
            TakeOrder,
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
        /// Stores all the configuration variables for this state machine
        /// </summary>
        private Restaurant_WORLD SMConfiguration;
        /// <summary>
        /// true if a table location command was recognized
        /// </summary>
        private bool tableCommandRecognized;
        /// <summary>
        /// true if a kitchen location command was recognized
        /// </summary>
        private bool kitchenCommandRecognized;
        /// <summary>
        /// stores the number (text) of the table in which the robot is located
        /// </summary>
        private string currentTableNumber;
        /// <summary>
        /// stores the direction (text) of a table right/left
        /// </summary>
        private string currentTableDirection;
        /// <summary>
        /// Stores a list of MVN_PLN locations (one for each table to visit)
        /// </summary>
        private Queue<MapLocation> tablesToVisit;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a state machine for the test.
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        public Restaurant(HAL9000Brain brain, HAL9000CmdMan cmdMan)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            tableCommandRecognized = false;
            kitchenCommandRecognized = false;
            tablesToVisit = new Queue<MapLocation>();

            finalStatus = Status.Ready;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            SM.AddState(new FunctionState((int)States.FollowHuman, FollowHuman));
            SM.AddState(new FunctionState((int)States.WaitForLocationCommand, WaitForLocationCommand));
            SM.AddState(new FunctionState((int)States.WaitForLocationConfirmation, WaitForLocationConfirmation));
            SM.AddState(new FunctionState((int)States.WaitForTableDirectionCommand, WaitForTableDirectionCommand));
            SM.AddState(new FunctionState((int)States.WaitForTableDirectionConfirmation, WaitForTableDirectionConfirmation));
            SM.AddState(new FunctionState((int)States.WaitForOrderingCommand, WaitForOrderingCommand));
            SM.AddState(new FunctionState((int)States.WaitForOrderTableConfirmation, WaitForOrderTableConfirmation));
            SM.AddState(new FunctionState((int)States.GoToOrderTable, GoToOrderTable));
            SM.AddState(new FunctionState((int)States.TakeOrder, TakeOrder));
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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Restaurant SM execution finished.");
            return this.finalStatus;
        }
        #endregion

        #region States Methods
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing Restaurant test.");

            //Load the WORLD configuration for this test
            SMConfiguration = new Restaurant_WORLD();

            finalStatus = Status.StillRunning;

            // TODO: Change the next status
            return (int)States.FollowHuman;
        }

        /// <summary>
        /// The robot start to follow a human (guide phase).
        /// </summary>
        private int FollowHuman(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> FollowHuman state reached.");

            //start saving the robot's path (hansel & gretel)
            cmdMan.MVN_PLN_startsavingpath();

            cmdMan.HEAD_lookat(SMConfiguration.HeadHomePan, SMConfiguration.HeadHomeTilt, 10000);
            //Thread.Sleep(500);
            //cmdMan.HEAD_lookat(SMConfiguration.HeadFollowPan, SMConfiguration.HeadFollowTilt, 10000);

            //send the arms to a position above the laser
            cmdMan.ARMS_goto(SMConfiguration.ArmsFollowPosition);

            brain.SayAsync(SMConfiguration.StartFollowMessage);
            //start to follow a human
            cmdMan.MVN_PLN_startfollowhuman();

            brain.recognizedSentences.Clear();
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Waiting for location command");
            return (int)States.WaitForLocationCommand;
        }
        /// <summary>
        /// The robot is executing the follow-human command. In this state, the human indicates to the robot
        /// if a point is the location of a table or the kitchen location
        /// </summary>
        /// <returns>current state when the </returns>
        int WaitForLocationCommand(int currentState, object o)
        {
            //Verify if the robot hear a command during the following phase
            tableCommandRecognized = false;
            kitchenCommandRecognized = false;
            if (brain.recognizedSentences.Count > 0)
            {
                //the robot heard a command so, get it
                string sentence = brain.recognizedSentences.Dequeue();
                //string sentence = "robot orgering table one";
                //verify if the heard command contains the keywords for a table location commands
                if (SMConfiguration.findKeywordsInSentence(sentence, SMConfiguration.TableLocationKeywords))
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table location advice detected.");
                    //verify if the heard command contains the number of the table (the numbers are stored in a keywords list)
                    List<string> keywordsFound;
                    if (SMConfiguration.findKeywordsInSentence(sentence,SMConfiguration.TableNumberKeywords,out keywordsFound))
                    {
                        //the command has a table number, a table location command was recognized
                        tableCommandRecognized = true;
                        currentTableNumber = keywordsFound[0];
                        //confirm if the recognized commnad is correct
                        brain.SayAsync(SMConfiguration.buildTableConfirmationMessage(currentTableNumber));
                        brain.recognizedSentences.Clear();
                        return (int)States.WaitForLocationConfirmation;
                    }
                }
                //verify if the heard command contains the keywords for the kitchen location
                if (SMConfiguration.findKeywordsInSentence(sentence,SMConfiguration.KitchenLocationKeywords))
                {
                    //a kitchen location command was recognized
                    kitchenCommandRecognized = true;
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Kitchen location reached.");
                    //confirm if the recognized commnad is correct
                    brain.SayAsync(SMConfiguration.KitchenConfirmationMessage);
                    brain.recognizedSentences.Clear();
                    return (int)States.WaitForLocationConfirmation;
                }
                //Command not recognized try to recognized another command
                brain.recognizedSentences.Clear();
                Thread.Sleep(500);
                return currentState;
            }
            //No command detected, keep listening
            Thread.Sleep(500);
            return currentState;
        }

        /// <summary>
        /// The robot waits for an user voice confirmation of a location command (table or kitchen)
        /// </summary>
        /// <returns></returns>
        private int WaitForLocationConfirmation(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Waiting for location confirmation");
            //waits for an user voice confirmation
            if (this.brain.WaitForUserConfirmation())
            {
                cmdMan.MVN_PLN_stopfollowhuman();
                cmdMan.MVN_PLN_stopsavingpath();
                cmdMan.HEAD_lookat(SMConfiguration.HeadHomePan, SMConfiguration.HeadHomeTilt, 1000);
                cmdMan.ARMS_goto(SMConfiguration.ArmsHomePosition, 10000);

                if (tableCommandRecognized)
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table location confirmed");

                    //Ask for the direction of the table
                    brain.SayAsync(SMConfiguration.TableDirectionAskingMessage);
                    brain.recognizedSentences.Clear();
                    Thread.Sleep(100);
                    return (int)States.WaitForTableDirectionCommand;
                }
                if(kitchenCommandRecognized)
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Kitchen location confrimed");
                    brain.SayAsync(SMConfiguration.OrderAskingMessage);
                    brain.recognizedSentences.Clear();
                    Thread.Sleep(1000);
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Waiting for ordering command.");
                    return (int)States.WaitForOrderingCommand;
                }
            }
            brain.SayAsync("Ok no.");
            //Thread.Sleep(500);
            brain.recognizedSentences.Clear();
            return (int)States.WaitForLocationCommand;
            //return (int)States.FollowHuman;
        }

        /// <summary>
        /// The robot waits for the table-direction command. i.e. "robot, the table is at your right side"
        /// </summary>
        private int WaitForTableDirectionCommand(int currentState, object o)
        {
            //Verify if the robot hear a voice command
            if (brain.recognizedSentences.Count > 0)
            {
                //the robot heard a voice command so, get it
                string sentence = brain.recognizedSentences.Dequeue();
                //string sentence = "robot the table is at your right/left";
                //verify if the heard command contains the keywords for a table direction command
                List<string> keywordsFound;
                if (SMConfiguration.findKeywordsInSentence(sentence, SMConfiguration.TableDirectionKeywords, out keywordsFound))
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table direction command recognized.");                    
                    //the direction of the table is stored in keywordsFound[0]
                    currentTableDirection = keywordsFound[0];
                    //confirm if the recognized commnad is correct
                    brain.SayAsync(SMConfiguration.buildTableDirectionConfirmationMessage(currentTableDirection));
                    brain.recognizedSentences.Clear();
                    return (int)States.WaitForTableDirectionConfirmation;
                }
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table direction command not recognized.");
            }
            return currentState;
        }
        
        /// <summary>
        /// The robot waits for the table-direction confirmation
        /// </summary>
        private int WaitForTableDirectionConfirmation(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Waiting for direction confirmation");
            //waits for an user voice confirmation
            if (this.brain.WaitForUserConfirmation())
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table direction confirmed");
                brain.SayAsync(SMConfiguration.OkMessage);
                //the location and direction of the table is confirmed, register the location in MVNPLN
                cmdMan.MVN_PLN_addlocation(SMConfiguration.addTableLocation(currentTableNumber),currentTableDirection,10000);
                //back to the follow-human state
                return (int)States.FollowHuman;
            }
            //the human says no, ask again for the direction of the table
            brain.SayAsync(SMConfiguration.TableDirectionAskingMessage);
            brain.recognizedSentences.Clear();
            Thread.Sleep(100);
            return (int)States.WaitForTableDirectionCommand;
        }

        /// <summary>
        /// The robot waits for an ordering command to attending certain table
        /// </summary>
        private int WaitForOrderingCommand(int currentState, object o)
        {
            //listening commands.....
            if (brain.recognizedSentences.Count > 0)
            {
                //the robot heard a command so, get it
                string sentence = brain.recognizedSentences.Dequeue();
                //string sentence = "robot table one/two/three";

                //verify if the heard command contains the number of the table (the numbers are stored in a keywords list)
                List<string> keywordsFound;
                if (SMConfiguration.findKeywordsInSentence(sentence, SMConfiguration.TableNumberKeywords, out keywordsFound))
                {
                    //the command has a table number
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table number detected.");
                    currentTableNumber = keywordsFound[0];
                    //confirm if the recognized commnad is correct
                    brain.SayAsync(SMConfiguration.buildTableConfirmationMessage(currentTableNumber));
                    brain.recognizedSentences.Clear();
                    return (int)States.WaitForOrderTableConfirmation;
                }
            }
            return currentState;
        }
        /// <summary>
        /// The robot waits for the table number confirmation (and then go to that table to take an order)
        /// </summary>
        private int WaitForOrderTableConfirmation(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Waiting for table number confirmation");
            //waits for an user voice confirmation
            if (this.brain.WaitForUserConfirmation())
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Table number confirmed");
                brain.SayAsync(SMConfiguration.OkMessage);
                tablesToVisit.Enqueue(SMConfiguration.getTableLocation(currentTableNumber));
                return (int)States.GoToOrderTable;               
            }
            brain.SayAsync(SMConfiguration.OrderAskingMessage);
            Thread.Sleep(500);
            brain.recognizedSentences.Clear();
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Waiting for ordering command.");
            return (int)States.WaitForOrderingCommand;
        }
        /// <summary>
        /// The robot navigates to a given table to take an order, if there is no more tables to visit then 
        /// the robot goes to the kitchen to take the food/drinks/etc
        /// </summary>
        private int GoToOrderTable(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> GoToOrderTable state rached.");
            //TODO:Launch a thread to detect request while navigating

            if(tablesToVisit.Count>0)
            {
                //obtain the current table MVN_PLN location to visit
                MapLocation tableLocation = tablesToVisit.Dequeue();
                
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Going to: " + tableLocation.Name);

                //navigate to the current table
                brain.SayAsync(SMConfiguration.builTableVisitMessage(tableLocation.Alias));
                if(!cmdMan.MVN_PLN_getclose(tableLocation.Name, 20000))
                    if(!cmdMan.MVN_PLN_getclose(tableLocation.Name, 20000))
                        cmdMan.MVN_PLN_getclose(tableLocation.Name, 20000);

                //TODO: ask for an order
                return (int)States.FinalState;
            }
            MapLocation kitchenLocation = SMConfiguration.MvnPlnKitchenLocation;
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Going to: " + SMConfiguration.MvnPlnKitchenLocation.Name);
            cmdMan.SPG_GEN_asay(SMConfiguration.GoingToKitchenMessage, 5000);
            //TODO: go to the kitchen to grasp object
            return (int)States.FinalState;
        }
        /// <summary>
        /// the robot is on the table (in front of some people) and ask for an order
        /// </summary>
        private int TakeOrder(int currentState, object o)
        {
            return currentState;
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

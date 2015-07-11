using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
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
        private int attemptCounter;
        private int attemptLimit;
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

            attemptCounter = 0;
            attemptLimit = 3;

            finalStatus = Status.Ready;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            SM.AddState(new FunctionState((int)States.EnterArena, EnterArena));
            SM.AddState(new FunctionState((int)States.NavigateToOperator, NavigateToOperator));
            SM.AddState(new FunctionState((int)States.WaitForCommand, WaitForCommand));
            SM.AddState(new FunctionState((int)States.ConfirmComand, ConfirmComand));
            SM.AddState(new FunctionState((int)States.ParseCommand, ParseCommand));
            SM.AddState(new FunctionState((int)States.PerformCommand, PerformCommand));
            SM.AddState(new FunctionState((int)States.LeaveArena, LeaveArena));
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
        private bool process_string(string cmd, string sentence)
        {
            string result="";
            //create a file to save the sentence to process
            StreamWriter sw = new StreamWriter(SMConfiguration.sentenceFilePath);
            sw.Write(sentence); // write this program to a file
            sw.Close();

            //configure the python program to execute
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
            start.FileName = SMConfiguration.pythonPath; //the python path
            start.Arguments = string.Format("{0} {1}", cmd, SMConfiguration.sentenceFilePath);//the python code and the arguments
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true; 
            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))//execute the program
            {
                using (System.IO.StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();//read the output
                }
            }
            //stores the result in a LOG file 
            StreamWriter swlog = new StreamWriter(SMConfiguration.LOGFilePath);
            swlog.Write(result); // write this program to a file
            swlog.Close();

            //split the result (separate by line ends)
            char[] delimiters = { '\n' };
            string[] lines = result.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            //get the last line (the one which have the response to Action Planner)
            if (lines.Length > 0)
            {
                //split the actions (separate by pipes |)
                SMConfiguration.setOfActions = lines[lines.Length-1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            else
                return false;//the command was not executed
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
                attemptCounter = 0;
                return (int)States.NavigateToOperator;
            }
            else
            {
                attemptCounter++;
                //the robot cannot entered the arena
                if (attemptCounter < attemptLimit)
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot enter to the arena at attempt " + attemptCounter + ". Try again.");
                    return (int)States.EnterArena;
                }
                else
                {
                    //reset the attempt counter
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot enter to the arena. Try to continue with the test.");
                    attemptCounter = 0;
                    return (int)States.NavigateToOperator;
                }
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
                brain.recognizedSentences.Clear();

                attemptCounter = 0;
                return (int)States.WaitForCommand;
            }
            else
            {
                attemptCounter++;
                //the robot cannot navigate to
                if (attemptCounter < attemptLimit)
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> connot navigate to te aperator at attempt: " + attemptCounter + ". Trying again.");
                    return (int)States.NavigateToOperator;
                }
                else
                {
                    //reset the attempt counter
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot navigate to the operator. Try to continue with the test.");
                    attemptCounter = 0;
                    return (int)States.WaitForCommand;
                }
            }
        }
        /// <summary>
        /// the robot is in listening state, waiting for a command from the user
        /// </summary>
        /// <returns>ConfirmComand state</returns>
        private int WaitForCommand(int currentState, object o)
        {

            if (brain.recognizedSentences.Count > 0)
            {
                SMConfiguration.sentenceToParse = brain.recognizedSentences.Dequeue();

                cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_didyousay, 2000);
                cmdMan.SPG_GEN_say(SMConfiguration.sentenceToParse, 2000);
                Thread.Sleep(500);
                brain.recognizedSentences.Clear();

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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> ConfirmComand state reached.");
            if (!this.brain.WaitForUserConfirmation())
            {
                //if the confirmation was NO then the robot ask for a new command
                cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_repeatCommand, 5000);
                Thread.Sleep(1000);
                brain.recognizedSentences.Clear();
                return (int)States.WaitForCommand;
            }
            else
            {
                //if the confirmation was YES then parse the response from the language understanding module
                return (int)States.ParseCommand;
            }
        }
        /// <summary>
        /// Get the command parsing from the language understanding module
        /// </summary>
        /// <returns>PerformCommand state if the LANG_UND module parse correctly the sentence. LeaveArena otherwise</returns>
        private int ParseCommand(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> ParseCommand state reached.");

            if (process_string(SMConfiguration.commandPath, SMConfiguration.sentenceToParse))
            {
                //the string was succesfully parsed
                return (int)States.PerformCommand;
            }
            else
            {
                cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_sentenceNotParsed, 5000);
                finalStatus = Status.Failed;
                return (int)States.LeaveArena;
            }


            /*if (cmdMan.process_string(SMConfiguration.sentenceToParse, out SMConfiguration.setOfActions, 5000))
            {
                //the string was succesfully parsed
            }
            else
            {
                //the string was not parsed
                cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_sentenceNotParsed, 5000);
                return (int)States.LeaveArena;
            }*/
        }
        /// <summary>
        /// Executes all the primitives SM in order to perform the entire task
        /// </summary>
        /// <returns>LeaveArena state</returns>
        private int PerformCommand(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> PerformCommand state reached.");
            
            //exxecute all the primitives (one by one)
            foreach (string action in SMConfiguration.setOfActions)
            {
                char[] delimiters = {' '};
                //get the parameters for each primitive
                string clean_action = action.Replace('\r',' ');
                string[] parameters = clean_action.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                //the first parameter allways is the primitive to execute
                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "answer_question":
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SM to Execute: AnswerQuestion.");
                            //no parameters
                            AnswerQuestion sm_aq = new AnswerQuestion(this.brain, this.cmdMan, this.SMConfiguration);
                            AnswerQuestion.Status finalStatus_aq = sm_aq.Execute();
                            break;
                        case "bring_object":
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SM to Execute: Bringobject.");
                            //1 parameter (object name)
                            BringObject sm_bo = new BringObject(this.brain, this.cmdMan, this.SMConfiguration, parameters[1]);
                            BringObject.Status finalStatus_bo = sm_bo.Execute();
                            break;
                        case "navigate_to":
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SM to Execute: NavigateTo.");
                            //1 parameter (location name/class)
                            NavigateTo sm_nt = new NavigateTo(this.brain, this.cmdMan, this.SMConfiguration, parameters[1]);
                            NavigateTo.Status finalStatus_nt = sm_nt.Execute();
                            break;
                        case "take_object":
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SM to Execute: TakeObject.");
                            //1 parameter (object name)
                            TakeObject sm_to = new TakeObject(this.brain, this.cmdMan, this.SMConfiguration, parameters[1]);
                            TakeObject.Status finalStatus_to = sm_to.Execute();
                            break;
                        case "tell_phrase":
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SM to Execute: TellPhrase.");
                            //1 parameter (kind of phrase to say)
                            TellPhrase sm_tp = new TellPhrase(this.brain, this.cmdMan, this.SMConfiguration, parameters[1]);
                            TellPhrase.Status finalStatus_tp = sm_tp.Execute();
                            break;
                        case "find_person":
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SM to Execute: FindPerson.");
                            //no parameters
                            //FindPerson sm_fp = new FindPerson(this.brain, this.cmdMan, this.SMConfiguration);
                            //FindPerson.Status finalStatus_fp = sm_fp.Execute();
                            break;
                        default:
                            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> The primitive " + parameters[0] + " was not found.");
                            break;
                    }
                }
            }
            finalStatus = Status.OK;
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> All primitives were executed.");
            //TODO:
            //Descomponer las siguientes dependencias conceptuales:
            /*
             * # take [object] from [location] and deliver to [person]
             *      navigate_to [location]
             *      search_and_find [object]
             *      navigate_to [person_location]
             *      bring
             * # take [object] from [location] and deliver to [me]
             *      navigate_to [location]
             *      search_and_find [object]
             *      navigate_to [me]
             *      bring
             * # take [object] from [location1] and deliver to [location2]
             *      navigate_to [location1]
             *      search_and_take [object]
             *      navigate_to [location2]
             *      bring
             * # go to [location] and find [person]
             *      navigate_to [location]
             *      find [person]
             * # find person in [location] and talk [phrase]
             *      navigate_to [location]
             *      find [person]
             *      tellphrase [phrase]
             */
            return (int)States.LeaveArena;
        }
        /// <summary>
        /// The robot leaves the arena
        /// </summary>
        /// <returns>FinalState state to finish this state</returns>
        private int LeaveArena(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> LeaveArena state reached.");

            cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_leavingArena, 5000);

            NavigateTo sm_navigation = new NavigateTo(this.brain, this.cmdMan, SMConfiguration, SMConfiguration.MVNPLN_exitLocation);
            NavigateTo.Status navig_status = sm_navigation.Execute();
            if (navig_status == NavigateTo.Status.OK)
            {
                attemptCounter = 0;
                return (int)States.FinalState;
            }
            else
            {
                attemptCounter++;
                //the robot cannot navigate to exit
                if (attemptCounter < attemptLimit)
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot navigate to the exit at attempt: " + attemptCounter + ". Trying again.");
                    return (int)States.LeaveArena;
                }
                else
                {
                    //reset the attempt counter
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot navigate to the exit. Try to continue with the test.");
                    attemptCounter = 0;
                    return (int)States.FinalState;
                }
            }
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

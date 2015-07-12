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
    public class AnswerQuestion
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
            /// Detect a question from the operator
            /// </summary>
            StateQuestion,
            /// <summary>
            /// Answer the operator's question
            /// </summary>
            StateAnswer,
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
        int attemptCounter;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates the SM of the primite action: Take-Object
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        /// <param name="objectToTake">Name of the object to take. Default: empty string.</param>
        public AnswerQuestion(HAL9000Brain brain, HAL9000CmdMan cmdMan, GPSR_WORLD SMConfiguration)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;

            finalStatus = Status.Ready;
            attemptCounter = 0;

            this.SMConfiguration = SMConfiguration;
            
            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));

            SM.AddState(new FunctionState((int)States.StateQuestion, StateQuestion));
            SM.AddState(new FunctionState((int)States.StateAnswer, StateAnswer));

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
        #endregion

        #region States Methods
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing AnswerQuestion primitive.");

            // TODO: Change the next status
            return (int)States.StateQuestion;
        }

        private int StateQuestion(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing AnswerQuestion primitive.");

            this.cmdMan.SPG_GEN_say("Make me a question.", 2000);
            this.brain.recognizedSentences.Clear();
            Thread.Sleep(1000);
            
            return (int)States.StateAnswer;
        }

        private int StateAnswer(int currentState, object o)
        {
            if (attemptCounter >= 15)
            {
                this.brain.SayAsync("Human I can't hear you. I will trie to continue with the test.");
                Thread.Sleep(2000);
                finalStatus = Status.Failed;
                return (int)States.FinalState;

            }

            if (this.brain.recognizedSentences.Count > 0)
            {
                SentenceImperative foundAction = this.brain.languageProcessor.Understand(this.brain.recognizedSentences.Dequeue());
                if (foundAction != null)
                {
                    if (foundAction.ActionClass == VerbType.Say)
                    {
                        TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Answering \"" + foundAction.DirectObject + "\"");
                        this.cmdMan.SPG_GEN_say(foundAction.DirectObject, 10000);
                        this.brain.recognizedSentences.Clear();

                        finalStatus = Status.OK;
                        return (int)States.FinalState;

                    }
                    else
                        this.brain.recognizedSentences.Clear();
                }
            }
            attemptCounter++;
            Thread.Sleep(1000);
            return (int)States.StateAnswer;
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

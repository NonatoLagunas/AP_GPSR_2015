using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.StateMachines;
using Robotics.Mathematics;
using Robotics.Controls;
using ActionPlanner.ComplexActions;
using ActionPlanner.Tests.ConfigurationFiles;

namespace ActionPlanner.Tests.StateMachines
{
    public class RoboZoo
    {

        #region Enums
        /// <summary>
        ///  States of the "RoboZoo" state machine
        /// </summary>
        private enum States
        {
            /// <summary>
            ///  Configuration state (variables setup)
            /// </summary>
            InitialState,
            /// <summary>
            /// The robot tries to find a marker in front of him
            /// </summary>
            SearchMarker,
            /// <summary>
            /// The marker was not found
            /// </summary>
            MarkerNotFound,
            /// <summary>
            /// The marker was found
            /// </summary>
            MarkerFound,
            /// <summary>
            /// The robot performs the presentation action
            /// </summary>
            PerformPresentation,
            /// <summary>
            /// the robot performs the dance show
            /// </summary>
            PerformDance,
            /// <summary>
            /// the robot performs the hipnotize show
            /// </summary>
            PerformHipnotize,            
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
        private RoboZoo_WORLD SMConfiguration;
        /// <summary>
        /// stores the attempts of the searching phase
        /// </summary>
        private int searchAttempt;
        /// <summary>
        /// rise this flag when you want to stop all the threads
        /// </summary>
        private bool stopAllThreads;
        /// <summary>
        /// this flag raises up at the finish of the hypno speech
        /// </summary>
        private bool hypnoSpeechFinished;
        /// <summary>
        /// this flags becomes true when you want to stop the dance show
        /// </summary>
        private bool stopDance;
        /// <summary>
        /// Launch the corresponding function to perform the async head movements in the searching phase
        /// </summary>
        private Thread t_HEADSearchMovements;
        /// <summary>
        /// Launch the thread function to perform the async movements of the robot's ARMS  in the search phase
        /// </summary>
        private Thread t_ARMSSearchMovements;
        /// <summary>
        /// Launch the thread to speech a phrase in the search phase
        /// </summary>
        private Thread t_SPGENSearchPhrases;
        /// <summary>
        /// Launch the thread to speech a phrase in the hypno phase
        /// </summary>
        private Thread t_SPGENHypnoSpeech;
        /// <summary>
        /// Launch a thread for the arms movements in the hypno phase
        /// </summary>
        private Thread t_ARMSHypnoMovements;
        /// <summary>
        /// Launch a thread for the head movements in the hypno phase
        /// </summary>
        private Thread t_HEADHypnoMovements;
        /// <summary>
        /// Launch a thread for the arms movements in the dance phase
        /// </summary>
        private Thread t_ARMSDanceMovements;
        /// <summary>
        /// Launch a thread for the head movements in the dance phase
        /// </summary>
        private Thread t_HEADDanceMovements;
        /// <summary>
        /// Launch a thread for the head movements in the dance phase
        /// </summary>
        private Thread t_BASEDanceMovements;
        /// <summary>
        /// stores a random number representing a head pan/tilt position
        /// </summary>
        private Random headRandPos;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a state machine for the RoboZoo test.
        /// </summary>
        /// <param name="brain">HAL9000Brain instance</param>
        /// <param name="cmdMan">HAL9000CmdMan instance</param>
        public RoboZoo(HAL9000Brain brain, HAL9000CmdMan cmdMan)
        {
            this.brain = brain;
            this.cmdMan = cmdMan;


            t_HEADSearchMovements = new Thread(new ThreadStart(HEADSearchMovements));
            t_ARMSSearchMovements = new Thread(new ThreadStart(ARMSSearchMovements));
            t_SPGENSearchPhrases = new Thread(new ThreadStart(SPGENSearchPhrases));
            t_BASEDanceMovements = new Thread(new ThreadStart(BASEDanceMovements));
            t_ARMSDanceMovements = new Thread(new ThreadStart(ARMSDanceMovements));
            t_HEADDanceMovements = new Thread(new ThreadStart(HEADDanceMovements));
            t_SPGENHypnoSpeech = new Thread(new ThreadStart(SPGENHypnoSpeech));
            t_ARMSHypnoMovements = new Thread(new ThreadStart(ARMSHypnoMovements));
            t_HEADHypnoMovements = new Thread(new ThreadStart(HEADHypnoMovements));

            headRandPos = new Random();
            searchAttempt = 0;

            stopAllThreads = false;
            hypnoSpeechFinished  = false;
            stopDance = false;

            finalStatus = Status.Ready;

            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.InitialState, InitialState));
            SM.AddState(new FunctionState((int)States.SearchMarker, SearchMarker));
            SM.AddState(new FunctionState((int)States.MarkerFound, MarkerFound));
            SM.AddState(new FunctionState((int)States.MarkerNotFound, MarkerNotFound));
            SM.AddState(new FunctionState((int)States.PerformPresentation, PerformPresentation));
            SM.AddState(new FunctionState((int)States.PerformDance, PerformDance));
            SM.AddState(new FunctionState((int)States.PerformHipnotize, PerformHipnotize));
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
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> RoboZoo SM execution finished.");

            //Stop the head search movement
            stopAllThreads = true;

            //finish shearch async movements
            if (t_HEADSearchMovements.IsAlive)
                t_HEADSearchMovements.Join();
            if (t_ARMSSearchMovements.IsAlive)
                t_ARMSSearchMovements.Join();
            if (t_SPGENSearchPhrases.IsAlive)
                t_SPGENSearchPhrases.Join();
            //finish hypno async movements
            if (t_SPGENHypnoSpeech.IsAlive)
                t_SPGENHypnoSpeech.Join();
            if(t_ARMSHypnoMovements.IsAlive)
                t_ARMSHypnoMovements.Join();
            if (t_HEADHypnoMovements.IsAlive)
                t_HEADHypnoMovements.Join();
            //finish dance async movements
            if (t_BASEDanceMovements.IsAlive)
                t_BASEDanceMovements.Join();
            if (t_ARMSDanceMovements.IsAlive)
                t_ARMSDanceMovements.Join();
            if (t_HEADDanceMovements.IsAlive)
                t_HEADDanceMovements.Join();

            return this.finalStatus;
        }
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
        private bool moveOneArm(string position, int side, int timeout)
        {
            bool finalStatus = false;
            switch(side)
            {
                case 0: //right arm
                    if(SMConfiguration.ARMS_ArmsEnable.right)
                        finalStatus = this.cmdMan.ARMS_ra_goto(position,timeout);
                    break;
                case 1: //left arm
                    if(SMConfiguration.ARMS_ArmsEnable.left)
                        finalStatus = this.cmdMan.ARMS_la_goto(position,timeout);
                    break;
                default:    //any arm
                    if(SMConfiguration.ARMS_ArmsEnable.right)
                        finalStatus = this.cmdMan.ARMS_ra_goto(position,timeout);
                    else if(SMConfiguration.ARMS_ArmsEnable.left)
                        finalStatus = this.cmdMan.ARMS_la_goto(position,timeout);
                    break;
            }

            return finalStatus;
        }
        /// <summary>
        /// performs the movements of the robot's arms in the search phase
        /// </summary>
        private void ARMSSearchMovements()
        {
            int positionCounter = 0;
            //execute until command detection or thread stoping
            while (!SMConfiguration.commandDetected && !stopAllThreads)
            {
                //get the values of the head positions
                string armPosition = SMConfiguration.ARMS_SearchMovements[positionCounter++];
                //move the enable arm
                moveActiveArms(armPosition, 10000);
                /*if (SMConfiguration.ARMS_ArmsEnable.left && SMConfiguration.ARMS_ArmsEnable.right)
                    this.cmdMan.ARMS_goto(armPosition, 10000);
                if (SMConfiguration.ARMS_ArmsEnable.left)
                    this.cmdMan.ARMS_la_goto(armPosition, 10000);
                if (SMConfiguration.ARMS_ArmsEnable.right)
                    this.cmdMan.ARMS_ra_goto(armPosition, 10000);*/

                //incremen/reset the counter (if the counter reachs the lenght of the positions array then reset)
                positionCounter = (positionCounter == SMConfiguration.ARMS_SearchMovements.Length) ? 0 : positionCounter;
            }
            //return the arms to home
            moveActiveArms(SMConfiguration.ARMS_home, 10000);
        }
        /// <summary>
        /// performs the movements of the robot's head in the search phase
        /// </summary>
        private void HEADSearchMovements()
        {
            int movementCounter = 0;
            //execute until command detection or thread stoping
            while (!SMConfiguration.commandDetected && !stopAllThreads)
            {
                //get the values of the head positions
                double pan = SMConfiguration.HEAD_SearchMovements[movementCounter].pan;
                double tilt = SMConfiguration.HEAD_SearchMovements[movementCounter].tilt;
                movementCounter++;
                //move the head
                this.cmdMan.HEAD_lookat(pan, tilt, 10000);
                //incremen/reset the counter (if the counter reachs the lenght of the positions array then reset)
                movementCounter = (movementCounter == SMConfiguration.HEAD_SearchMovements.Length) ? 0 : movementCounter;
                //sleep the thread a while
                Thread.Sleep(2000);
            }
            //return head to 0,0
            this.cmdMan.HEAD_lookat(0.0, 0.0, 3000);
        }
        /// <summary>
        /// asyncronously plays the robot's messages for the search phase
        /// </summary>
        private void SPGENSearchPhrases()
        {
            //execute until command detection or thread stoping
            while (!SMConfiguration.commandDetected && !stopAllThreads)
            {
                //send the phrase to the spgen
                this.cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_searchp1,3000);
                //sleep the thread a while
                Thread.Sleep(5000);
            }
            this.cmdMan.SPG_GEN_shutup(3000);
        }
        /// <summary>
        /// asyncronously plays the robot's messages for the hypno phase
        /// </summary>
        private void SPGENHypnoSpeech()
        {
            int speechCounter = 0;
            //execute until command detection or thread stoping
            while (speechCounter < SMConfiguration.SPGEN_HypnoPhrases.Length && !stopAllThreads)
            {
                //play the message and inc
                this.cmdMan.SPG_GEN_say(SMConfiguration.SPGEN_HypnoPhrases[speechCounter++], 5000);
                //sleep the thread a while
                Thread.Sleep(2500);
            }
            this.cmdMan.SPG_GEN_shutup(1000);
            hypnoSpeechFinished = true;
        }
        /// <summary>
        /// async execution of the arms movements for the hypno phase
        /// </summary>
        private void ARMSHypnoMovements()
        {
            while (!hypnoSpeechFinished && !stopAllThreads)
            {
                //move left arm if it is enabled
                moveOneArm(SMConfiguration.ARMS_hypnol,1,10000);
                moveOneArm(SMConfiguration.ARMS_hypnor,1,10000);
            }
            //move the arms to home
            moveActiveArms(SMConfiguration.ARMS_home, 10000);
        }
        /// <summary>
        /// async execution for the head movements for the hypno phase
        /// </summary>
        private void HEADHypnoMovements()
        {
            int movementCounter = 0;
            //execute until command detection or thread stoping
            while (!hypnoSpeechFinished && !stopAllThreads)
            {
                //get the values of the head positions
                double pan = SMConfiguration.HEAD_HypnoMovements[movementCounter].pan;
                double tilt = SMConfiguration.HEAD_HypnoMovements[movementCounter].tilt;
                movementCounter++;
                //move the head
                this.cmdMan.HEAD_lookat(pan, tilt, 10000);
                //incremen/reset the counter (if the counter reachs the lenght of the positions array then reset)
                movementCounter = (movementCounter == SMConfiguration.HEAD_HypnoMovements.Length) ? 0 : movementCounter;
            }
            //return head to 0,0
            this.cmdMan.HEAD_lookat(0.0, 0.0, 3000);
        }
        /// <summary>
        /// perform the async movements of the base for the dance show
        /// </summary>
        private void BASEDanceMovements()
        {
            int baseAngleCounter = 0;
            while (!stopDance && !stopAllThreads)
            {
                //get the values of the head positions
                int angle = SMConfiguration.BASE_danceAngles[baseAngleCounter++];
                //move the base
                this.cmdMan.MVN_PLN_getclose(0.0, 0.0, MathUtil.ToRadians(angle), 10000);
                //incremen/reset the counter (if the counter reachs the lenght of the positions array then reset)
                baseAngleCounter = (baseAngleCounter == SMConfiguration.BASE_danceAngles.Length) ? 0 : baseAngleCounter;
            }
            //move the base to home
            this.cmdMan.MVN_PLN_getclose(0.0, 0.0, MathUtil.ToRadians(0), 10000);

        }
        /// <summary>
        /// perform the async movements of the arms for the dance show
        /// </summary>
        private void ARMSDanceMovements()
        {
            int positionCounter = 0;
            //execute until command detection or thread stoping
            while (!stopDance && !stopAllThreads)
            {
                //get the values of the head positions
                string armPosition = SMConfiguration.ARMS_DanceMovements[positionCounter++];
                //move the enable arm
                moveActiveArms(armPosition, 10000);                

                //incremen/reset the counter (if the counter reachs the lenght of the positions array then reset)
                positionCounter = (positionCounter == SMConfiguration.ARMS_DanceMovements.Length) ? 0 : positionCounter;
            }
            //return the arms to home
            moveActiveArms(SMConfiguration.ARMS_home, 10000);
        }
        /// <summary>
        /// perform the async movements of the head for the dance show
        /// </summary>
        private void HEADDanceMovements()
        {
            while (!stopDance && !stopAllThreads)
            {
                this.cmdMan.HEAD_lookat(-1 + headRandPos.NextDouble(), -1 + 2 * headRandPos.NextDouble(), 1000);
                Thread.Sleep(700);
            }
            this.cmdMan.HEAD_lookat(SMConfiguration.HEAD_lookToFace.pan, SMConfiguration.HEAD_lookToFace.tilt, 3000);
        }
        #endregion

        #region States Methods
        /// <summary>
        /// Configuration state
        /// </summary>
        private int InitialState(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Initializing RoboZoo test.");

            //Load the WORLD configuration for this test
            SMConfiguration = new RoboZoo_WORLD();
            searchAttempt = 0;


            //initialize the threads fos async robot's movements
            t_HEADSearchMovements = new Thread(new ThreadStart(HEADSearchMovements));
            t_ARMSSearchMovements = new Thread(new ThreadStart(ARMSSearchMovements));
            t_SPGENSearchPhrases = new Thread(new ThreadStart(SPGENSearchPhrases));
             
            
            //Launch the threads to move asynchronous the robot's head
            t_HEADSearchMovements.Start();
            while (!t_HEADSearchMovements.IsAlive);
            //Launch the thread to move asynchronous the robot's arms
            t_ARMSSearchMovements.Start();
            while (!t_ARMSSearchMovements.IsAlive) ;
            //Launch the thread to asynchronously play a message
            t_SPGENSearchPhrases.Start();
            while (!t_SPGENSearchPhrases.IsAlive) ;
            
            return (int)States.SearchMarker;
        }

        /// <summary>
        /// State in which Justina search for a marker/paddle located in front of her
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="o"></param>
        /// <returns>MarkerFound if a marker/paddle was found, MarkerNotFound elsewhere</returns>
        private int SearchMarker(int currentState, object o)
        {

            if(searchAttempt==0)
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SearchMarker state reached.");        

            //trying to execute the findmarker command
            if(cmdMan.OBJ_FNDT_findpaddle(out SMConfiguration.markerCommand, 10000))
            {
                //findmarker command executed succesfully                
                if(SMConfiguration.markerCommand=="")   //check if a marker was found
                    SMConfiguration.markerFound=false;  //marker not found
                else
                    SMConfiguration.markerFound=true;   //marker found
            }

            searchAttempt++;

            //jump to the next state
            if(SMConfiguration.markerFound)
                return (int)States.MarkerFound;
            else
                return (int)States.MarkerNotFound;
        }
        private int MarkerNotFound(int currentState, object o)
        {
            return (int)States.SearchMarker;
        }
        private int MarkerFound(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> MarkerFound state reached.");
            //jump to the perform state of the detected command
            int nextState;
            switch (SMConfiguration.generateIntegerCommand(SMConfiguration.markerCommand))
            {
                case (int)RoboZoo_WORLD.Commands.Dance:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Dance command detected.");
                    SMConfiguration.commandDetected = true;
                    nextState=(int)States.PerformDance;
                    break;
                case (int)RoboZoo_WORLD.Commands.Presentation:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Presentation command detected.");
                    SMConfiguration.commandDetected = true;
                    nextState=(int)States.PerformPresentation;
                    break;
                case (int)RoboZoo_WORLD.Commands.Hypnotize:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Hypnotize command detected.");
                    SMConfiguration.commandDetected = true;
                    nextState = (int)States.PerformHipnotize;
                    break;
                case -1:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> The string command " + SMConfiguration.markerCommand + " does not have a integer representation.");
                    nextState = (int)States.SearchMarker;
                    break;
                default:
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Integer representation for " + SMConfiguration.markerCommand + " not parsed (verify your code).");
                    nextState = (int)States.SearchMarker;
                    break;
            }

            if (SMConfiguration.commandDetected)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Stoping threads.");
                //Stop the head search movement
                if (t_HEADSearchMovements.IsAlive)
                    t_HEADSearchMovements.Join();
                //Stop the arms search movement
                if (t_ARMSSearchMovements.IsAlive)
                    t_ARMSSearchMovements.Join();
                //Stop the spgen search movement
                if (t_SPGENSearchPhrases.IsAlive)
                    t_SPGENSearchPhrases.Join();

                cmdMan.SPG_GEN_say(SMConfiguration.getDetectedCommandPhrase(), 3000);
            }

            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Jump to the next state.");
            return nextState;
        }
        private int PerformPresentation(int currentState, object o)
        {           
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000-> PerformPresentation state reached.");
            //select the arm to use
            int armToUse = -1; //0 for right, 1 for left, others numbers for any arm

            //introduction
            this.cmdMan.SPG_GEN_say("Hello. I am the Robot Justina and I was built in the National University of Mexico", 15000);
            this.cmdMan.SPG_GEN_say("I would like to show you my design", 10000);
            moveOneArm("heilHitler", armToUse, 10000);
            //head show
            this.cmdMan.SPG_GEN_say("I have a mecha-tronic head",10000);
            moveOneArm("showHead", armToUse, 10000);
            this.cmdMan.HEAD_lookat(0.7, 0.0, 10000);
            this.cmdMan.HEAD_lookat(0.0, 0.5, 10000);
            this.cmdMan.HEAD_lookat(-0.7, 0.0, 10000);
            this.cmdMan.HEAD_lookat(0.0, -0.5, 10000);
            this.cmdMan.HEAD_lookat(0, 0, 10000);
            moveOneArm("heilHitler", armToUse, 10000);
            //kinect show
            this.cmdMan.SPG_GEN_say("I have a kinect system",10000);
            moveOneArm("heilHitler", armToUse, 10000);
            Thread.Sleep(1000);
            //arms show
            moveActiveArms("showArm", 12000);
            this.cmdMan.SPG_GEN_say("I have two arms", 2000);
            this.cmdMan.SPG_GEN_say("These are an anthropomophic seven degree of freedom manipulators",2000);
            Thread.Sleep(1000);
            moveActiveArms("home", 12000);
            //laser show
            this.cmdMan.SPG_GEN_say("I have a laser sensor",2000);
            moveOneArm("showLaser", armToUse, 10000);
            Thread.Sleep(1000);
            moveActiveArms("home", 12000);
            //outting
            this.cmdMan.SPG_GEN_say("Finally, I have a differential pair mobile base for navigating", 15000);
            cmdMan.MVN_PLN_move(0, Math.PI / 4, 4000);
            cmdMan.MVN_PLN_move(0, -Math.PI / 4, 4000);
            this.cmdMan.SPG_GEN_say("Thank you very much for your attention");

            return (int)States.InitialState;
        }
        private int PerformDance(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000-> PerformDance state reached.");

            t_BASEDanceMovements = new Thread(new ThreadStart(BASEDanceMovements));
            t_ARMSDanceMovements = new Thread(new ThreadStart(ARMSDanceMovements));
            t_HEADDanceMovements = new Thread(new ThreadStart(HEADDanceMovements));

            stopDance = false;
            //play the song
            this.cmdMan.SPG_GEN_aplay("justina_baila_1.mp3", 1000);
            //TODO:

            //Launch the thread to move asynchronous the robot's arms 
            t_ARMSDanceMovements.Start();
            while (!t_ARMSDanceMovements.IsAlive) ;
            //Launch the threads to move asynchronous the robot's head
            t_HEADDanceMovements.Start();
            while (!t_HEADDanceMovements.IsAlive) ;
            //Launch the thread to asynchronously move the robot's base
            t_BASEDanceMovements.Start();
            while (!t_BASEDanceMovements.IsAlive) ;

            //wait a while
            Thread.Sleep(10000);

            //finish threads
            stopDance = true;
            if (t_ARMSDanceMovements.IsAlive)
                t_ARMSDanceMovements.Join();
            if (t_HEADDanceMovements.IsAlive)
                t_HEADDanceMovements.Join();
            if (t_BASEDanceMovements.IsAlive)
                t_BASEDanceMovements.Join();

            //send arms and head to home
            moveActiveArms(SMConfiguration.ARMS_home, 10000);
            this.cmdMan.HEAD_lookat(SMConfiguration.HEAD_lookToFace.pan, SMConfiguration.HEAD_lookToFace.tilt, 2000);

            return (int)States.InitialState;
        }
        private int PerformHipnotize(int currentState, object o)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000-> PerformHypnotize state reached.");

            t_SPGENHypnoSpeech = new Thread(new ThreadStart(SPGENHypnoSpeech));
            t_ARMSHypnoMovements = new Thread(new ThreadStart(ARMSHypnoMovements));
            t_HEADHypnoMovements = new Thread(new ThreadStart(HEADHypnoMovements));

            //to allow threads execution
            hypnoSpeechFinished = false;

            //introduction to hypno show
            cmdMan.SPG_GEN_shutup(1000);
            this.brain.SayAsync("I will try to hipnotize you.");
            Thread.Sleep(1000);
            this.brain.SayAsync("Look straight to my pendulum.");
            Thread.Sleep(1000);

            //move both arms to navigation 
            moveActiveArms(SMConfiguration.ARMS_navigation, 10000);
            //move left arm to hypno
            moveOneArm(SMConfiguration.ARMS_hypnoc, 1, 10000);            

            //Launch the thread to move asynchronous the robot's arms (hypno left and right)
            t_ARMSHypnoMovements.Start();
            while (!t_ARMSHypnoMovements.IsAlive);
            //Launch the threads to move asynchronous the robot's head ("circular" movement)
            t_HEADHypnoMovements.Start();
            while (!t_HEADHypnoMovements.IsAlive);
            //Launch the thread to asynchronously play the speech
            t_SPGENHypnoSpeech.Start();
            while (!t_SPGENHypnoSpeech.IsAlive);

            if(t_SPGENHypnoSpeech.IsAlive)
                t_SPGENHypnoSpeech.Join();
            if (t_HEADHypnoMovements.IsAlive)
                t_HEADHypnoMovements.Join();
            if (t_ARMSHypnoMovements.IsAlive)
                t_ARMSHypnoMovements.Join();

            //send the arms back to navigation and home
            moveActiveArms(SMConfiguration.ARMS_navigation, 10000);
            moveActiveArms(SMConfiguration.ARMS_home, 10000);

            //look at the human face
            cmdMan.HEAD_lookat(SMConfiguration.HEAD_lookToFace.pan,SMConfiguration.HEAD_lookToFace.tilt, 2000);

            this.brain.SayAsync("now you are under my control!!!");
            Thread.Sleep(1000);
            this.brain.SayAsync("I command you to give me a token!");
            Thread.Sleep(2000);

            return (int)States.InitialState;
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

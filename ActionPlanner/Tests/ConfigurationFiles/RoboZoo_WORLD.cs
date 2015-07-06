using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner.Tests.ConfigurationFiles
{
    class RoboZoo_WORLD
    {
        private Dictionary<string, int> commandList;
        /// <summary>
        /// represent a struct to indicate which arm will be use (lef, right or both)
        /// </summary>
        public struct ArmsEnable
        {
            public bool left;
            public bool right;
        }
        /// <summary>
        /// stores the head position of the robot
        /// </summary>
        public struct HeadPositions
        {
            public double pan, tilt;
            public HeadPositions(double pan, double tilt)
            {
                this.pan = pan;
                this.tilt = tilt;
            }
        }
        /// <summary>
        /// list of the integer representations for the detectet-sting command
        /// </summary>
        public enum Commands
        {
            Dance,
            Hypnotize,
            Presentation
        }
        /// <summary>
        /// this flag is true when a marker was detected by the marker detection program
        /// </summary>
        public bool markerFound;
        /// <summary>
        /// this flag is true when a command was correctly parsed and false when not
        /// </summary>
        public bool commandDetected;
        /// <summary>
        /// this variable stores the string representation of the detected command 
        /// </summary>
        public string markerCommand;
        /// <summary>
        /// stores the integer representation of the detected command
        /// </summary>
        private int intMarkerCommand;
        /// <summary>
        /// this variable stores the positions of the head when the robot searchs a marker
        /// </summary>
        public HeadPositions[] HEAD_SearchMovements;
        /// <summary>
        /// stores the positions of the robot's head for the hypno phase
        /// </summary>
        public HeadPositions[] HEAD_HypnoMovements;
        /// <summary>
        /// stores the head (pan and tilt) positions to look at the human's face
        /// </summary>
        public HeadPositions HEAD_lookToFace;
        /// <summary>
        /// stores the arms positions for the search phase
        /// </summary>
        public string[] ARMS_SearchMovements;
        /// <summary>
        /// stores the phrases for the hypno phase
        /// </summary>
        public string[] SPGEN_HypnoPhrases;
        /// <summary>
        /// store the arms movements positions for the dance phace
        /// </summary>
        public string[] ARMS_DanceMovements;
        /// <summary>
        /// stores the configuration of the enable arm (right, left or both)
        /// </summary>
        public ArmsEnable ARMS_ArmsEnable;
        /// <summary>
        /// stores the ARMS position for home
        /// </summary>
        public string ARMS_home;        
        /// <summary>
        /// stores the ARMS position for showArm
        /// </summary>
        public string ARMS_showarm;
        /// <summary>
        /// stores one phrase for the search phrase
        /// </summary>
        public string SPGEN_searchp1;
        /// <summary>
        /// stores the ARMS position for hypno left
        /// </summary>
        public string ARMS_hypnol;
        /// <summary>
        /// stores the ARMS position for hypno right
        /// </summary>
        public string ARMS_hypnor;
        /// <summary>
        /// stores the ARMS position for hypno center
        /// </summary>
        public string ARMS_hypnoc;
        /// <summary>
        /// stores the ARMS positions for navigation position
        /// </summary>
        public string ARMS_navigation;
        /// <summary>
        /// stores the base angles for the dance show
        /// </summary>
        public int[] BASE_danceAngles;
        /// <summary>
        /// Default constructor
        /// </summary>
        public RoboZoo_WORLD()
        {
            // TODO: initialize the class-properties here
            markerFound = false;
            commandDetected = false;
            markerCommand = "";
            intMarkerCommand=-1;
            commandList = new Dictionary<string, int>(3);
            commandList.Add("bailar", (int)Commands.Dance);
            commandList.Add("hipnotizar", (int)Commands.Hypnotize);
            commandList.Add("presentar", (int)Commands.Presentation);

            //initialize spgen phrases
            SPGEN_searchp1 = "Hello humans. Please, put a marker in front of my eyes.";
            //initialize arms positions
            ARMS_home = "home";
            ARMS_showarm = "dance1";
            ARMS_hypnol = "hipnol";
            ARMS_hypnor = "hipnor";
            ARMS_hypnoc = "hipno";
            ARMS_navigation = "navigation";

            //initialize heads positions
            HEAD_lookToFace.pan = 0.0;
            HEAD_lookToFace.tilt = -0.3;
            //initialize the head movements for the search phase
            HEAD_SearchMovements = new HeadPositions[10];
            HEAD_SearchMovements[0] = new HeadPositions(0.0, 0.0);
            HEAD_SearchMovements[1] = new HeadPositions(-0.4, 0.0);
            HEAD_SearchMovements[2] = new HeadPositions(0.0, 0.0);
            HEAD_SearchMovements[3] = new HeadPositions(0.4, 0.0);
            HEAD_SearchMovements[4] = new HeadPositions(0.0, 0.0);
            HEAD_SearchMovements[5] = new HeadPositions(0.0, -0.3);
            HEAD_SearchMovements[6] = new HeadPositions(-0.4, -0.3);
            HEAD_SearchMovements[7] = new HeadPositions(0.0, -0.3);
            HEAD_SearchMovements[8] = new HeadPositions(0.4, -0.3);
            HEAD_SearchMovements[9] = new HeadPositions(0.0, -0.3);
            //initialize the head movements for the hypno phase
            HEAD_HypnoMovements = new HeadPositions[9];
            HEAD_HypnoMovements[0] = new HeadPositions(0.0, -0.6);
            HEAD_HypnoMovements[1] = new HeadPositions(-0.3, -0.4);
            HEAD_HypnoMovements[2] = new HeadPositions(-0.6, -0.2);
            HEAD_HypnoMovements[3] = new HeadPositions(-0.3, 0.0);
            HEAD_HypnoMovements[4] = new HeadPositions(0.0, 0.2);
            HEAD_HypnoMovements[5] = new HeadPositions(0.3, 0.0);
            HEAD_HypnoMovements[6] = new HeadPositions(0.6, -0.2);
            HEAD_HypnoMovements[7] = new HeadPositions(0.3, -0.4);
            HEAD_HypnoMovements[8] = new HeadPositions(0.0, -0.6);
            //initialize the base movements for the dance phase
            BASE_danceAngles = new int[5];
            BASE_danceAngles[0] = 30;
            BASE_danceAngles[1] = -10;
            BASE_danceAngles[2] = 30;
            BASE_danceAngles[3] = 50;
            BASE_danceAngles[4] = 0;
            //initialize the arms dance movements
            ARMS_DanceMovements = new string[6];
            ARMS_DanceMovements[0]="baila1";
            ARMS_DanceMovements[1] ="baila2";
            ARMS_DanceMovements[2] ="baila3";
            ARMS_DanceMovements[3] ="baila4";
            ARMS_DanceMovements[4] ="baila1";
            ARMS_DanceMovements[5] ="baila2";
            
            //initialize the arms movement for the search phase
            ARMS_SearchMovements = new string[2] {ARMS_home,ARMS_showarm};

            //enable the ARM(s) to use (left, right)
            ARMS_ArmsEnable.left = true;
            ARMS_ArmsEnable.right = true;

            //initialize the phrases for the hypno phase
            SPGEN_HypnoPhrases = new string[5];
            SPGEN_HypnoPhrases[0] = "You can feel yourself relaxing now.";
            SPGEN_HypnoPhrases[1] = "You can feel a heavy, relaxed feeling coming over you.";
            SPGEN_HypnoPhrases[2] = "And as I continue talking, that heavy relaxed feeling will grow stronger and stronger.";
            SPGEN_HypnoPhrases[3] = "That feeling carries you into a deep, peaceful state of hypnosis.";
            SPGEN_HypnoPhrases[4] = "you are mine now!!";
        }

        // TODO: Create all the classs-methods you need here
        /// <summary>
        /// Tries to get the integer representation of an string marker/paddle command
        /// </summary>
        /// <param name="stringCommand">The string representation of the command</param>
        /// <returns>The integer representation of the given command, -1 if the integer representation does not exists</returns>
        public int generateIntegerCommand(string stringCommand)
        {
            int command = -1;

            if (!commandList.TryGetValue(stringCommand, out command))
                command = -1;

            intMarkerCommand = command;

            return command;
        }
        public string getDetectedCommandPhrase()
        {
            string speech="";
            switch (intMarkerCommand)
            {
                case (int)Commands.Dance:
                    speech = "Dance command detected.";
                    break;
                case (int)Commands.Hypnotize:
                    speech = "Hypnotize command detected.";
                    break;
                case (int)Commands.Presentation:
                    speech = "Presentation command detected.";
                    break;
                default:
                    speech = "Not parsed command detected.";
                    break;
            }
            return speech;
        }
    }
}
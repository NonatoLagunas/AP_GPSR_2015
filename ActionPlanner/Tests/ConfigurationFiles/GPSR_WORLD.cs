using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner.Tests.ConfigurationFiles
{
    public class GPSR_WORLD
    {
        /// <summary>
        /// stores a set of MVN_PLN locations an its type. the key is the unique name of the location and the value (int) is the kind of location 
        /// (0-standbylocation, 1-room, 2-table, 3-human)
        /// </summary>
        private Dictionary<string, int> mapLocation;
        /// <summary>
        /// stores a set of targets to bring an object
        /// </summary>
        private Dictionary<string, string> bringTargets;
        /// <summary>
        /// represent a struct to indicate which arm will be use (lef, right or both)
        /// </summary>
        public struct ArmsEnable
        {
            public bool left;
            public bool right;
        }
        /// <summary>
        /// stores the configuration of the enable arm (right, left or both)
        /// </summary>
        public ArmsEnable ARMS_ArmsEnable;
        /// <summary>
        /// stores the ARMS positions for navigation position
        /// </summary>
        public string ARMS_navigation;
        /// <summary>
        /// stores the ARMS positions for home position
        /// </summary>
        public string ARMS_home;
        /// <summary>
        /// stores the ARMS positions for navigation with object in hand
        /// </summary>
        public string ARMS_navigObject;
        /// <summary>
        /// stores the ARMS positions for drop position
        /// </summary>
        public string ARMS_drop;
        /// <summary>
        /// stores the name of the arm used to take an object
        /// </summary>
        public string ARMS_usedArm;
        public string SPGEN_waitforcomman;
        public string SPGEN_repeatCommand;
        public string SPGEN_didyousay;
        public string SPGEN_sentenceNotParsed;
        public string SPGEN_leavingArena;     
        public string MVNPLN_entranceLocation;
        public string MVNPLN_operatorLocation;
        public string MVNPLN_exitLocation;
        public string MVNPLN_kitchenLocation;
        public string MVNPLN_livingroomLocation;
        public bool bringTohuman;
        public string sentenceToParse;
        public string[] setOfActions;
        public string pythonPath;
        public string commandPath;
        public string sentenceFilePath;
        public string LOGFilePath;
        /// <summary>
        /// Default constructor
        /// </summary>
        public GPSR_WORLD()
        {
            pythonPath="python.exe";
            commandPath = "Z:/LANG_UND_PLANZW/cfr_parser_nobb.py";
            sentenceFilePath = "Z:/LANG_UND_PLANZW/stringToProcess";
            LOGFilePath = "Z:/LANG_UND_PLANZW/LOG";
            //initialize SPGEN messages
            SPGEN_sentenceNotParsed = "I understand your command but I am not able to perform it. I'm so sorry!";
            SPGEN_leavingArena = "I'm leaving the arena.";
            SPGEN_waitforcomman = "Hello human! I'm waiting for a command.";
            SPGEN_repeatCommand = "Ok. Please repeat the command.";
            SPGEN_didyousay = "Did you say:";
            //initialize MVN-PLN location
            MVNPLN_entranceLocation = "entrancelocation";
            MVNPLN_operatorLocation = "gpsrLoc";
            MVNPLN_exitLocation = "exit";
            MVNPLN_kitchenLocation = "kitchen";
            MVNPLN_livingroomLocation = "loc2";

            //initialize arms positions
            ARMS_drop = "drop";
            ARMS_navigation = "standby";
            ARMS_navigObject = "navigation";
            ARMS_home = "home";
            ARMS_usedArm = "";

            bringTohuman = false;

            //enable the ARM(s) to use (left, right)
            ARMS_ArmsEnable.left = true;
            ARMS_ArmsEnable.right = true;

            //initialize Bring targets dictionary PARA TRADUCIR EL PARAMETRO A PUNTO DE MOTION
            bringTargets = new Dictionary<string, string>(4);
            bringTargets.Add("me_location", MVNPLN_operatorLocation);
            bringTargets.Add("living_room", MVNPLN_livingroomLocation);
            bringTargets.Add("kitchen", MVNPLN_kitchenLocation);
            bringTargets.Add("exit", MVNPLN_exitLocation);

            //initialize MVN_PLN dictionary DE QUE TIPO ES LA LOCATION
            mapLocation = new Dictionary<string, int>(5);
            mapLocation.Add(MVNPLN_operatorLocation, 0);
            mapLocation.Add(MVNPLN_livingroomLocation, 1);
            mapLocation.Add(MVNPLN_kitchenLocation, 1);
            mapLocation.Add(MVNPLN_entranceLocation, 0);
            mapLocation.Add(MVNPLN_exitLocation, 0);
        }
        // TODO: Create all the classs-methods you need here
        /// <summary>
        /// search the kind of the location in the location list
        /// </summary>
        /// <param name="locationName">the name of the location to search</param>
        /// <returns>the kind of the location (0 by d</returns>
        public int getKindLocation(string locationName)
        {
            int kind=-1;

            if (!mapLocation.TryGetValue(locationName, out kind))
                kind = -1;

            return kind;
        }
        public string getTargetDefaultLocation(string target)
        {
            string defaultLocation="";
            bringTargets.TryGetValue(target, out defaultLocation);
            return defaultLocation;
        }
        public string generateDeliverPhrase(string objectName, string personName="")
        {
            return "Here is your " + objectName + " " + personName;
        }
    }
}

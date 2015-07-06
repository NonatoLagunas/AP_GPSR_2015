using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner.Tests.ConfigurationFiles
{
    class Restaurant_WORLD
    {
        #region Structs and Enums
        /// <summary>
        /// Struct to store the parameters value of the head movement's
        /// </summary>
        public struct headPosition
        {
            public double pan;
            public double tilt;
        }        
        #endregion

        #region Keywords
        /// <summary>
        /// Stores all the keywords to detect a table location command
        /// </summary>
        List<string> tableLocationKeywords;
        /// <summary>
        /// Stores all the keywords to detect a table number in a table location command.
        /// </summary>
        List<string> tableNumberKeywords;
        /// <summary>
        /// Stores all the keywords to detect a kitchen location command
        /// </summary>
        List<string> kitchenLocationKeywords;
        /// <summary>
        /// Stores all the keywords to detect a table direction command
        /// </summary>
        List<string> tableDirectionKeywords;
        #endregion

        #region ARMS Parameters Values
        /// <summary>
        /// Stores the position of the arms used in the following phase.
        /// </summary>
        private string armsFollowPosition;
        /// <summary>
        /// Stores the HOME position of the arms 
        /// </summary>
        private string armsHomePosition;
        #endregion

        #region SP-GEN Messages
        /// <summary>
        /// Stores the message the robot plays when it starts to follow a person.
        /// </summary>
        private string startFollowMessage;
        /// <summary>
        /// Stores the message the robot plays when it recognizes a kitchen location command.
        /// </summary>
        private string kitchenConfirmationMessage;
        /// <summary>
        /// Stores the message the robot plays to ask for a orderig-phase command
        /// </summary>
        private string orderAskingMessage;
        /// <summary>
        /// Stores the message the robot plays to ask for the direction of a table (left or right)
        /// </summary>
        private string tableDirectionAskingMessage;
        /// <summary>
        /// Stores the message for the OK phrase
        /// </summary>
        private string okMessage;
        /// <summary>
        /// Stores the message for a table number confirmation received
        /// </summary>
        private string goinToTakeOrderMessage;
        /// <summary>
        /// stores the message the robot plays when it starts to navigate to the kitchen
        /// </summary>
        private string goingToKitchenMessage;
        #endregion

        #region HEAD Parameters Values
        /// <summary>
        /// Stores the head parameter values for the head-follow position
        /// </summary>
        private headPosition headFollowPosition;
        /// <summary>
        /// Stores the head parameter values for the head-home position
        /// </summary>
        private headPosition headHomePosition;
        #endregion

        #region MVN-PLN maps position
        private Dictionary<string, MapLocation> tableNumber_mapPosition;
        private MapLocation mvnplnKitchenLocation;
        #endregion
        /// <summary>
        /// Default constructor
        /// </summary>
        public Restaurant_WORLD()
        {
            startFollowMessage = "Ok, lets go.";
            kitchenConfirmationMessage = "I heard: here is the kitchen, is that correct?";
            orderAskingMessage = "Ok, tell me. to which table should I go to take an order?";
            tableDirectionAskingMessage = "Please tell me if the table is at my left or my right side.";
            okMessage = "ok";
            goinToTakeOrderMessage = "I am going to take an order to table";
            goingToKitchenMessage = "I am going to the kitchen.";

            mvnplnKitchenLocation = new MapLocation("kitchenTable");

            armsFollowPosition = "standby";
            armsHomePosition = "home";

            tableLocationKeywords = new List<string>(new [] {"table", "ordering"});
            tableNumberKeywords = new List<string>(new[] { "one", "two", "three"});
            kitchenLocationKeywords = new List<string>(new[] { "kitchen" });
            tableDirectionKeywords = new List<string>(new[] { "right", "left"});

            headFollowPosition.pan = 0.0;
            headFollowPosition.tilt = -0.1;
            headHomePosition.pan = 0.0;
            headHomePosition.tilt = 0.0;
        }

        #region Keywords getters and setters
        /// <summary>
        /// List of keywords for a table location command
        /// </summary>
        public List<string> TableLocationKeywords
        {
            get { return tableLocationKeywords; }
        }
        /// <summary>
        /// List of keywords for a kitchen location command
        /// </summary>
        public List<string> KitchenLocationKeywords
        {
            get { return kitchenLocationKeywords; }
        }
        /// <summary>
        /// List of keywords for a table number on a table location command
        /// </summary>
        public List<string> TableNumberKeywords
        {
            get { return tableNumberKeywords; }
        }
        public List<string> TableDirectionKeywords
        {
            get { return tableDirectionKeywords; }
        }
        #endregion

        #region SP-GEN messages getters and setters
        /// <summary>
        /// Message to play when the robot starts to navigate to the kitchen location
        /// </summary>
        public string GoingToKitchenMessage
        {
            get { return goingToKitchenMessage; }
        }
        /// <summary>
        /// Message to play when the robot is going to take an order
        /// </summary>
        public String GoinToTakeOrderMessage
        {
            get { return goinToTakeOrderMessage; }
        }
        /// <summary>
        /// Ok Message
        /// </summary>
        public String OkMessage
        {
            get { return okMessage; }
        }
        /// <summary>
        /// Message to play when the robot ask for an order in the ordering phase)
        /// </summary>
        public string OrderAskingMessage
        {
            get { return orderAskingMessage; }
        }
        /// <summary>
        /// Message to play when the robot starts to follow a human.
        /// </summary>
        public string StartFollowMessage
        {
            get { return startFollowMessage; }
        }
        /// <summary>
        /// Message to play when the robot asks for a confirmation of the kitchen location.
        /// </summary>
        public string KitchenConfirmationMessage
        {
            get { return kitchenConfirmationMessage; }
        }
        /// <summary>
        /// Message to play when the robot asks for the direction of a table (left or right)
        /// </summary>
        public string TableDirectionAskingMessage
        {
            get { return tableDirectionAskingMessage; }
        }
        #endregion

        #region HEAD parameters getters and setters
        /// <summary>
        /// Pan parameter value for the HEAD HOME position
        /// </summary>
        public double HeadHomePan
        {
            get { return headHomePosition.pan; }
        }
        /// <summary>
        /// Tilt parameter value for the HEAD HOME position
        /// </summary>
        public double HeadHomeTilt
        {
            get { return headHomePosition.tilt; }
        }
        /// <summary>
        /// Pan parameter value for the HEAD FOLLOW position
        /// </summary>
        public double HeadFollowPan
        {
            get { return headFollowPosition.pan; }
        }
        /// <summary>
        /// Tilt parameter value for the HEAD FOLLOW position
        /// </summary>
        public double HeadFollowTilt
        {
            get { return headFollowPosition.tilt; }
        }
        #endregion

        #region ARMS parameters getters and setters
        /// <summary>
        /// Parameter value for the ARMS HOME position.
        /// </summary>
        public string ArmsHomePosition
        {
            get { return armsHomePosition; }
        }
        /// <summary>
        /// Parameter value for the ARMS FOLLOW position.
        /// </summary>
        public string ArmsFollowPosition
        {
            get { return armsFollowPosition; }
        }
        #endregion

        #region MVN_PLN getters and setters
        public MapLocation MvnPlnKitchenLocation
        {
            get{return mvnplnKitchenLocation;}
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Builds the message to play when the robot starts to navigate to a given table
        /// </summary>
        /// <param name="tableNumber">The number of the table (string)</param>
        /// <returns>The message to play</returns>
        public string builTableVisitMessage(string tableNumber)
        {
            return "I will going to vsit the table " + tableNumber;
        }
        /// <summary>
        /// Build the message to play when the robot asks for a confirmation of a table direction.
        /// </summary>
        /// <param name="tableNumber">the direction of the table</param>
        /// <returns>the message to play</returns>
        public string buildTableDirectionConfirmationMessage(string tableDirection)
        {
            return "I heard the table is at my " + tableDirection + " side, is that correct?";
        }
        /// <summary>
        /// Build the message to play when the robot asks for a confirmation of a table location.
        /// </summary>
        /// <param name="tableNumber">the number of the table</param>
        /// <returns>the message to play</returns>
        public string buildTableConfirmationMessage(string tableNumber)
        {
            return "I heard: table " + tableNumber + ", is that correct?";
        }
        /// <summary>
        /// Verify if any keyword, from a list of keywords, exists in a sentence.
        /// </summary>
        /// <param name="sentence">The sentence to check.</param>
        /// <param name="keywords">The list of keywords.</param>
        /// <returns>true: if the sentence contains any of the keywords.
        /// false: otherwise.</returns>
        public bool findKeywordsInSentence(string sentence, List<string> keywords)
        {         
            return keywords.Any(sentence.Contains);
        }
        
        /// <summary>
        /// Verify if any keyword, from a list of keywords, exists in a sentence.
        /// </summary>
        /// <param name="sentence">The sentence to check.</param>
        /// <param name="keywordsToVerify">The list of keywords to verify.</param>
        /// <param name="keywordsFound">The list of keywords found in the sentence.</param>
        /// <returns>true: if the sentence contains any of the keywords.
        /// false: otherwise.</returns>
        public bool findKeywordsInSentence(string sentence, List<string> keywordsToVerify, out List<string> keywordsFound)
        {
            keywordsFound = new List<string>();
            if (keywordsToVerify.Any(sentence.Contains))
            {
                foreach (string keyword in keywordsToVerify)
                {
                    if (sentence.Contains(keyword))
                        keywordsFound.Add(keyword);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Adds the MVN-PLN location (name, x,y and theta) to the locations dictionary.
        /// </summary>
        /// <param name="locationName"></param>
        /// <returns>the name of the MVN-PLN location</returns>
        public string addTableLocation(string tableNumber)
        {
            MapLocation tableLocation = new MapLocation("table"+tableNumber, tableNumber);
            tableNumber_mapPosition.Add(tableNumber, tableLocation);

            return tableLocation.Name;
        }
        /// <summary>
        /// Get the MVN_PLN location for a given table number (string)
        /// </summary>
        /// <param name="tableNumber">The table number one/two/three/etc</param>
        /// <returns>The MVN_PLN location</returns>
        public MapLocation getTableLocation(string tableNumber)
        {
            MapLocation tableLocation;
            tableNumber_mapPosition.TryGetValue(tableNumber, out tableLocation);

            return tableLocation;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner.Tests.ConfigurationFiles
{
    class DefaultStateMachine_WORLD
    {
        /// <summary>
        /// Stores the MVN_PLN-Location for the entrance location.
        /// </summary>
        private MapLocation _EntranceLocation;
        /// <summary>
        /// Stores the MVN_PLN-Location for the object-table location.
        /// </summary>
        private MapLocation _ObjectTableLocation;
        /// <summary>
        /// Stores the MVN_PLN-Location for the drop-table location.
        /// </summary>
        private MapLocation _DropTableLocation;
        /// <summary>
        /// The message the robot will play when it arrives to the object's table
        /// </summary>
        private string _TableArrivedMessage;
        /// <summary>
        /// Stores the ARMS-Position for navigation
        /// </summary>
        private string _ArmsNavigationPosition;
        /// <summary>
        /// Stores the default ARMS-position (home)
        /// </summary>
        private string _ArmsDefaultPosition;
        /// <summary>
        /// Stores the ARMS-position when the robots has an object in his hand
        /// </summary>
        private string _ArmsObjectTakenPosition;
        /// <summary>
        /// Stores a MVN-PLN-Location outside of the arena
        /// </summary>
        private MapLocation _LeaveLocation;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultStateMachine_WORLD()
		{
            //MVN_PLN must have loc0 as a location in the map
            _EntranceLocation = new MapLocation("loc0");
            _ObjectTableLocation = new MapLocation("loc1");
            _DropTableLocation = new MapLocation("loc2");
            _LeaveLocation = new MapLocation("loc3");

            _TableArrivedMessage = "I have arrived to the object location.";
            _ArmsNavigationPosition = "standby";
            _ArmsDefaultPosition = "home";
            _ArmsObjectTakenPosition = "navigation";
		}

        public string EntranceLocation
        {
            get{return _EntranceLocation.Name;}
        }
        public string ObjectTableLocation
        {
            get{return _ObjectTableLocation.Name;}
        }
        public string DropTableLocation
        {
            get{return _DropTableLocation.Name;}
        }
        public string TableArrivedMessage
        {
            get{return _TableArrivedMessage;}
        }
        public string ArmsNavigationPosition
        {
            get { return _ArmsNavigationPosition; }
        }
        public string ArmsDefaultPosition
        {
            get { return _ArmsDefaultPosition; }
        }
        public string ArmsObjectTakenPosition
        {
            get { return _ArmsObjectTakenPosition; }
        }
        public string LeaveLocation
        {
            get { return _LeaveLocation.Name; }
        }
	}
}

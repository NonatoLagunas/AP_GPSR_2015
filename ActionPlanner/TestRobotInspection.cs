using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner
{
	public	enum ripSta {InitialConditions, TryOpenTheDoor, WaitForDoorIsOpen, GoToEntrance, GoToRegistrationDesk, 
				 IntroduceYourself, DeliverForm, WaitingForLeaveCommand, LeavingTheArena, TryingToLeaveAfterReset}
	enum RIPSobjectives { }


	public class TestRobotInspection
	{
		List<string> objectives;

		MapLocation entranceDoor;
		MapLocation exitDoor;
		MapLocation registrationDesk;

		bool tryOpenDoor;

		string introduceYourselfText;

		bool armToDeliverIsRightArm;
		string armDeliverPosition;
		
		public TestRobotInspection()
		{
			this.entranceDoor = new MapLocation("entrancedoor", " entrance ");
			this.exitDoor = new MapLocation("exitdoor",  " exit door ");
			this.registrationDesk = new MapLocation("registrationdesk", "registration Desk");
		
			this.tryOpenDoor = false;

			this.objectives = new List<string>();
			this.objectives.Add("Enter the door");
			this.objectives.Add("Go to the Registration Desk");
			this.objectives.Add("Introduce myself");
			this.objectives.Add("Deliver the registration form");
			this.objectives.Add("Wait for [leave the arena] command");
			this.objectives.Add("Move after relaese emergency button");
			this.objectives.Add("Leave the arena");

			this.introduceYourselfText = "Hello. My name is Justina. I'm a Pumas at home team member";

			this.armToDeliverIsRightArm = true;
			this.armDeliverPosition = ArmsPP.heilHitler.ToString();
		}

		public MapLocation EntranceDoor
		{
			get{ return this.entranceDoor;}
			set{ this.entranceDoor = value;}
		}

		public MapLocation ExitDoor
		{
			get { return this.exitDoor; }
			set { this.exitDoor = value; }
		}

		public MapLocation RegistrationDesk
		{
			get { return this.registrationDesk; }
			set { this.registrationDesk = value; }
		}

		public bool TryOpenDoor
		{
			get { return this.tryOpenDoor; }
			set { this.tryOpenDoor = value; }
		}

		public bool ArmToDeliverIsRightArm
		{
			get { return this.armToDeliverIsRightArm; }
			set { this.armToDeliverIsRightArm = value; }
		}

		public string IntroduceYourselfText
		{
			get { return this.introduceYourselfText; }
			set { this.introduceYourselfText = value; }
		}

		public string ArmDeliverPosition
		{
			get { return this.armDeliverPosition; }
			set { this.armDeliverPosition = value; }
		}
	}
}

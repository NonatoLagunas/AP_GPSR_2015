using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace ActionPlanner
{
	public class CmdStartTest : SyncCommandExecuter
	{
		HAL9000Brain hal9000Brain;

		public CmdStartTest(HAL9000Brain brain)
			: base("start_robocup_test")
		{
			this.hal9000Brain = brain;
		}

		protected override Response SyncTask(Command command)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("CmdStart: Received: " + command.StringToSend);
			bool success = true;

			if (command.HasParams)
			{
                /*if (command.Parameters.Contains("followme"))
					this.hal9000Brain.StartFollowHumanTest();
				else if (command.Parameters.Contains("robozoo"))
					this.hal9000Brain.StartRoboZooTest();
				else if (command.Parameters.Contains("openchallenge"))
					this.hal9000Brain.StartOpenChallengeTest();
				else if (command.Parameters.Contains("restaurant"))
					this.hal9000Brain.StartRestaurant();
				else success = false;*/
			}
			else success = false;

			return Response.CreateFromCommand(command, success);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.API;
using Robotics.Controls;

namespace ActionPlanner
{
	public class CmdRecognized:SyncCommandExecuter
	{
		HAL9000Brain hal9000Brain;

		public CmdRecognized(HAL9000Brain brain)
			: base("recognized")
		{
			this.hal9000Brain = brain;
		}

		protected override Response SyncTask(Command command)
		{
            TextBoxStreamWriter.DefaultLog.WriteLine("CmdRecognized: Received but no enqueued: " + command.StringToSend);
			//this.hal9000Brain.RecognizedSentences.Enqueue(command.Parameters);
			return Response.CreateFromCommand(command, true);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;
namespace ActionPlanner
{
	public static class TextBox
	{
		public static void WriteHAL( string textToWrite)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine(@"HAL9000\>_ "+textToWrite);
		}
	}
}

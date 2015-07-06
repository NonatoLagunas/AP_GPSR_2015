using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;
using Robotics.Mathematics;

namespace ActionPlanner
{
	public class PhysicalObject
	{
		string name;
		string room;	//Room in which the object is
		string region; //Region in which the object is
		string location; //Location in which the object is
		Vector3 position;
		Queue<string> lastOwners;
		double weight;
		double volume;
		bool isManipulable;
		string purpose;
		string material;

		public PhysicalObject(string name, string room, string region, string location, Vector3 position, double weight,
			double volume, bool isManipulable, string purpose, string material)
		{
			this.name = name;
			this.room = room;
			this.region = region;
			this.location = location;
			this.position = position;
			this.weight = weight;
			this.volume = volume;
			this.isManipulable = isManipulable;
			this.purpose = purpose;
			this.material = material;

			
			this.lastOwners = new Queue<string>();
			this.lastOwners.Enqueue("unknown");
		}

		public PhysicalObject(string name)
			: this(name, "unknown", "unknown", "unknown", Vector3.Zero, -1, -1, false, "unknown", "unknown")
		{
		}

		public string Name
		{
			get { return this.name; }
			set
			{
				if (String.IsNullOrEmpty(value))
					TextBoxStreamWriter.DefaultLog.WriteLine("Physical Object: Invalid name");
				else this.name = value;
			}
		}
	}
}

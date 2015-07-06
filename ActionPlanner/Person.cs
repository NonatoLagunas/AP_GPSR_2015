using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;

namespace ActionPlanner
{
	public class Person
	{
		private string name;
		private string room;
		private string region;
		private string location;
		private List<PhysicalObject> ownedPhysicalObjects;
		private List<SentenceImperative> askedActions;
		private string occupation;
		private int age;
		private string genre;

		public Person(string name, string room, string region, string location, string occupation, int age, string genre)
		{
			this.name = name;
			this.room = room;
			this.region = region;
			this.location = location;
			this.occupation = occupation;
			this.age = age;
			this.genre = genre;
			this.ownedPhysicalObjects = new List<PhysicalObject>();
			this.askedActions = new List<SentenceImperative>();
		}

		public Person(string name)
			: this(name, "unknown", "unknown", "unknown", "unknown", -1, "unknown")
		{
		}

		public string Name
		{
			get { return this.name; }
			set
			{
				if (String.IsNullOrEmpty(value))
					TextBoxStreamWriter.DefaultLog.WriteLine("PersonClass: Invalid person name");
				else this.name = value;
			}
		}
	}
}

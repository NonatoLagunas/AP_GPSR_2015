using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;
using Robotics.Mathematics;

namespace ActionPlanner
{

    public class MapLocation
    {

        private string name;
		private string alias; 
		private double predefTorsoHeight;
        private Vector2 position;


		public MapLocation() : this("loc0", 0.8) { }

        public MapLocation(string Name,double predefHeight)
        {
			this.name = Name;
			this.alias = Name; 
			this.predefTorsoHeight = predefHeight;
        }

		public MapLocation(string Name, string Alias)
		{
			this.name = Name;
			this.alias = Alias;
			this.predefTorsoHeight = 0.8;
		}

		public MapLocation(string Name)
		{
			this.name = Name;
			this.alias = Name;
			this.predefTorsoHeight = 0.8;
		}

        public MapLocation(string Name, Vector2 position)
        {
            this.name = Name;
            this.alias = Name;
            this.predefTorsoHeight = 0.8;
            this.position = position;
        }


		public string Name
		{
			get { return this.name; }
			set
			{
				if (string.IsNullOrEmpty(value))
					TextBoxStreamWriter.DefaultLog.WriteLine("MapRoom: Invalid name");
				else
					this.name = value;
			}
		}

		public string Alias
		{
			get { return this.alias; }
			set { this.alias = value; }
		}

		public double PredefHeight
		{
			get { return this.predefTorsoHeight; }
			set
			{
				if (value >= 0 && value<=0.82 )
					this.predefTorsoHeight = value;
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("MapLocation: Invalid predefTorsoHeight.");
			} 
		}

        public Vector2 Position
        {
            get { return this.position; }
        }
    }
}

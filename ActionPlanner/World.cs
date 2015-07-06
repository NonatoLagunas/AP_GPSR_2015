using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;

namespace ActionPlanner
{
	public class World
	{
		public string CurrentRoom { get; set; }
		public string RoomWhereHumanIs { get; set; }
		public string RoomWhereMaster { get; set; }
		public string RoomWhereFatherIs { get; set; }
		public string RoomWhereIsraIs { get; set; }
        public string RoomWhereJesusIs { get; set; }
		private SortedList<string, MapRoom> rooms;


		public World()
		{
			this.rooms = new SortedList<string,MapRoom>();		
		}

		public SortedList<string,MapRoom> Rooms
		{
			get 
			{
				return this.rooms;
			}
		}

		public bool LoadRooms()
		{
			MapRoom[] temp = MapRoom.DeserializeFromXML("MapRooms.xml");
			if (temp == null)
				return false;
			foreach (MapRoom MR in temp)
			{
				if (!this.rooms.ContainsKey(MR.Name))
					this.rooms.Add(MR.Name, MR);
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("World: Repeated room");
			}
			return true; 
		}

		public bool SaveRooms()
		{
			return MapRoom.SerializeToXML(this.rooms.Values.ToArray(), "MapRooms.xml");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;
using System.Xml.Serialization;
using System.IO;

namespace ActionPlanner
{
	public class MapRoom
    {
		private string name;
		private List<MapLocation> locations;

		public MapRoom(string name)
		{
			this.name = name;
			this.locations = new List<MapLocation>();
		}

		public static bool SerializeToXML(MapRoom[] mapRooms, string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(MapRoom[]));
			if (File.Exists(path))
			{
				string[] emptyStrings = { "" };
				File.WriteAllLines(path, emptyStrings);
			}
			else
				File.Create(path);


			try
			{
				Stream stream = File.OpenWrite(path);
				serializer.Serialize(stream, mapRooms);
				stream.Close();
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static MapRoom[] DeserializeFromXML(string path)
		{
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(MapRoom[]));
				MapRoom[] regions;
				Stream stream = File.OpenRead(path);
				regions = (MapRoom[])serializer.Deserialize(stream);
				stream.Close();
				return regions;
			}
			catch
			{
				return null;
			}
		}

		public MapRoom() : this("room0") { }

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

		public List<MapLocation> Locations
		{
			get
			{
				return this.locations;
			}
			set
			{
				if (value != null)
					this.locations = value;
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("MapRoom: Invalid list of locations");
			}
		}
    }
}

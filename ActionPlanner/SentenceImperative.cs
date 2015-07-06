using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner
{
	public enum VerbType { Go, Leave, Get, Find, Introduce, Follow, Ask, Bring, Answer, 
		Say, Move, Make, StopFollow, SayHello, Confirm, Clean, Learn, Pour, Order, Turn };
	public class SentenceImperative
	{
		private VerbType actionType;
		private string directObject;
		private bool isAPerson;
		private string preposition;
		private string indirectObject;
		private string complements;

		public SentenceImperative()
		{
			this.actionType = VerbType.Say;
			this.directObject = "mmm";
			this.isAPerson = false;
			this.preposition = "";
			this.indirectObject = "";
			this.complements = "";
		}

		public VerbType ActionClass
		{
			get { return this.actionType; }
			set { this.actionType = value; }
		}

		public string DirectObject
		{
			get { return this.directObject; }
			set { this.directObject = value; }
		}

		public bool IsAPerson
		{
			get { return this.isAPerson; }
			set { this.isAPerson = value; }
		}

		public string Preposition
		{
			get { return this.preposition; }
			set { this.preposition = value; }
		}

		public string IndirectObject
		{
			get { return this.indirectObject; }
			set { this.indirectObject = value; }
		}

		public string Complements
		{
			get { return this.complements; }
			set { this.complements = value; }
		}

		public string SynthetizableCommand
		{
			get
			{
				string command = actionType.ToString();
				if (actionType == VerbType.Go)
				{
					command += " to the " + indirectObject;
				}
				else
				{
					if (!isAPerson) command += " the ";
					else if (this.directObject == "person" || this.directObject == "human")
						command += " the ";
					else command += " ";
					command += this.directObject;
				}
				return command;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner
{
	public enum SentenceType { Declarative, Imperative, Interrogative, Subordinate };
	public enum Prepositions { To, From, In };
	public class SentenceBuilder
	{
		public SentenceType SentenceType { get; set; }
		public string Subject { get; set; }
		public bool IsSubjectAProperNoun { get; set; }
		public bool IsSubjectAPerson { get; set; }
		public VerbType VerbType { get; set; }
		public string DirectObject { get; set; }
		public bool IsDOaProperNoun { get; set; }
		public bool IsDOaPerson { get; set; }
		public Prepositions Preposition { get; set; }
		public string IndirectObject { get; set; }
		public bool IsIDOaProperNoun { get; set; }
		public bool IsIDOaPerson { get; set; }

		public SentenceBuilder()
		{
			this.SentenceType = SentenceType.Imperative;
			this.Subject = "";
			this.IsSubjectAPerson = false;
			this.IsSubjectAProperNoun = false;
			this.VerbType = VerbType.Say;
			this.DirectObject = "";
			this.IsDOaPerson = false;
			this.IsDOaProperNoun = false;
			this.Preposition = Prepositions.To;
			this.IndirectObject = "";
			this.IsIDOaPerson = false;
			this.IsIDOaProperNoun = false;
		}
	}
}

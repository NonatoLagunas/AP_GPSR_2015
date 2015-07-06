using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Robotics.Controls;

namespace ActionPlanner
{
	public class NaturalLanguageProcessor
	{
		HAL9000Brain hal9000Brain;
		SortedList<int, string> commonQuestions;
		SortedList<int, string> answersToCommonQuestions;
		List<string> whatAmIThinkingAnswers;
		SortedList<int, string> knownVerbs;
		SortedList<int, VerbType> knownVerbsTypes;
		Random random;

		public NaturalLanguageProcessor(HAL9000Brain hal9000Brain)
		{
			this.hal9000Brain = hal9000Brain;
			this.commonQuestions = new SortedList<int, string>();
			this.answersToCommonQuestions = new SortedList<int, string>();
			this.whatAmIThinkingAnswers = new List<string>();
			this.knownVerbs = new SortedList<int, string>();
			this.knownVerbsTypes = new SortedList<int, VerbType>();
			this.random = new Random(DateTime.Now.Second);
		}

		public bool Initialize()
		{
			if (!loadKnownVerbs("KnownVerbs.txt"))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("Language Processor: Can't load known verbs");
				return false;
			}
			if (!loadCommonQuestions("CommonQuestions.txt"))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("Language Processor: Can't load common questions");
			}
			if (!loadWhatAreYouThinking("WhatAreYouThinking.txt"))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("Language Processor: Can't load what are you thinking answers");
				return false;
			}
			return true;
		}

		private bool loadKnownVerbs(string fileName)
		{
			if (!File.Exists(fileName)) return false;

			string[] lines = File.ReadAllLines(fileName);
			string[] parts;
			char[] delimiters = { ' ', '\t' };

			this.knownVerbs.Clear();
			this.knownVerbsTypes.Clear();
			foreach (string s in lines)
			{
				if (s.StartsWith("//")) continue;
				if (s.Length < 5) continue;

				parts = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2) continue;

				if (!this.knownVerbs.ContainsKey(parts[0].GetHashCode()))
				{
					this.knownVerbs.Add(parts[0].GetHashCode(), parts[0]);
					switch (parts[1])
					{
						case "go":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Go);
							break;
						case "leave":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Leave);
							break;
						case "get":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Get);
							break;
						case "find":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Find);
							break;
						case "introduce":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Introduce);
							break;
						case "follow":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Follow);
							break;
						case "ask":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Ask);
							break;
						case "bring":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Bring);
							break;
						case "answer":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Answer);
							break;
						case "say":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Say);
							break;
						case "make":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Make);
							break;
						case "stop":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.StopFollow);
							break;
						case "clean":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Clean);
							break;
						case "learn":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Learn);
							break;
						case "pour":
							this.knownVerbsTypes.Add(parts[0].GetHashCode(), VerbType.Pour);
							break;
					}
				}
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("Language Processor: HashCode repeated in known verbs");
			}
			if (this.knownVerbs.Count < 1) return false;

			return true;
		}

		private bool loadCommonQuestions(string fileName)
		{
			if (!File.Exists(fileName)) return false;

			string[] lines = File.ReadAllLines(fileName);

			this.commonQuestions.Clear();
			this.answersToCommonQuestions.Clear();
			for (int i = 0; i < lines.Length - 1; i++)
			{
				if (lines[i].StartsWith("//")) continue;
				if (lines[i].Length < 5) continue;

				if (!this.commonQuestions.ContainsKey(lines[i].GetHashCode()))
				{
					this.commonQuestions.Add(lines[i].GetHashCode(), lines[i]);
					if (lines[++i].Length < 5) return false;
					this.answersToCommonQuestions.Add(lines[i - 1].GetHashCode(), lines[i]);
				}
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("Language Processor: HashCode repeated in common questions");
			}
			if (this.commonQuestions.Count < 1) return false;
			if (this.answersToCommonQuestions.Count < 1) return false;

			return true;
		}

		private bool loadWhatAreYouThinking(string fileName)
		{
			if (!File.Exists(fileName)) return false;

			string[] lines = File.ReadAllLines(fileName);
			foreach (string s in lines)
			{
				if (s.StartsWith("//")) continue;
				if (s.Length < 5) continue;

				this.whatAmIThinkingAnswers.Add(s);
			}
			if (this.whatAmIThinkingAnswers.Count < 2) return false;

			return true;
		}

		private string whatAreYouThinking()
		{
			return this.whatAmIThinkingAnswers[random.Next(answersToCommonQuestions.Count)];
		}

		private string getCurrentTime()
		{
			string answer = "the time is ";

			answer += DateTime.Now.Hour.ToString() + " hours ";
			answer += DateTime.Now.Minute.ToString() + " minutes ";

			return answer;
		}

		private string getCurrentDate()
		{
			string answer = "today is ";

			answer += DateTime.Now.DayOfWeek.ToString() + " ";

			switch (DateTime.Now.Month)
			{
				case 1:
					answer += "january ";
					break;
				case 2:
					answer += "february ";
					break;
				case 3:
					answer += "march ";
					break;
				case 4:
					answer += "april ";
					break;
				case 5:
					answer += "may ";
					break;
				case 6:
					answer += "june ";
					break;
				case 7:
					answer += "july ";
					break;
				case 8:
					answer += "august ";
					break;
				case 9:
					answer += "september ";
					break;
				case 10:
					answer += "october ";
					break;
				case 11:
					answer += "november ";
					break;
				case 12:
					answer += "december ";
					break;
			}

			answer += DateTime.Now.Day.ToString() + " ";

			return answer;
		}

		public SentenceImperative Understand(string command)
		{
			if (String.IsNullOrEmpty(command)) return null;
			command = command.ToLower();
			if (command.StartsWith("robot "))
				command = command.Replace("robot ", "");

			SentenceImperative missunderstoodAction = new SentenceImperative();
			SentenceImperative understoodAction = new SentenceImperative();

			#region Answer yes or not
			if (command == "yes" || command == "yes robot")
			{
				understoodAction.ActionClass = VerbType.Confirm;
				understoodAction.DirectObject = "yes";
				return understoodAction;
			}
			if (command == "no" || command == "robot no")
			{
				understoodAction.ActionClass = VerbType.Confirm;
				understoodAction.DirectObject = "no";
				return understoodAction;
			}
			if (command == "start" || command == "start test")
			{
				understoodAction.ActionClass = VerbType.Confirm;
				understoodAction.DirectObject = "yes";
				return understoodAction;
			}
			if (command == "grasp" || command == "grasp object")
			{
				understoodAction.ActionClass = VerbType.Order;
				understoodAction.DirectObject = "grasp";
				return understoodAction;
			}
			if (command == "copy" || command == "copy movement" || command == "mirror")
			{
				understoodAction.ActionClass = VerbType.Order;
				understoodAction.DirectObject = "mirror";
				return understoodAction;
			}
			if (command == "handshake")
			{
				understoodAction.ActionClass = VerbType.Order;
				understoodAction.DirectObject = "handshake";
				return understoodAction;
			}
			if (command == "dance")
			{
				understoodAction.ActionClass = VerbType.Order;
				understoodAction.DirectObject = "dance";
				return understoodAction;
			}
			if (command == "introduce" || command == "introduce yourself")
			{
				understoodAction.ActionClass = VerbType.Order;
				understoodAction.DirectObject = "presentation";
				return understoodAction;
			}
			if (command == "hypno")
			{
				understoodAction.ActionClass = VerbType.Order;
				understoodAction.DirectObject = "hypno";
				return understoodAction;
			}
			#endregion

			#region Answer a common question
			int tempHashCode = command.GetHashCode();
			if (this.commonQuestions.ContainsKey(tempHashCode))
			{
				understoodAction.ActionClass = VerbType.Say;
				understoodAction.DirectObject = answersToCommonQuestions[tempHashCode];
				return understoodAction;
			}
			#endregion

			#region Common questions with non-predefined answers
			understoodAction.ActionClass = VerbType.Say;
			if (command == "say hello to fractal")
			{
				understoodAction.ActionClass = VerbType.SayHello;
				return understoodAction;
			}
			if (command == "what are you thinking")
			{
				understoodAction.DirectObject = whatAreYouThinking();
				return understoodAction;
			}
			if (command == "what date is today")
			{
				understoodAction.DirectObject = getCurrentDate();
				return understoodAction;
			}
			if (command == "what time is it")
			{
				understoodAction.DirectObject = getCurrentTime();
				return understoodAction;
			}

			if (command == "where are you")
			{
				if (this.hal9000Brain.CurrentRoom != "unknown")
					understoodAction.DirectObject = "I am in the " + this.hal9000Brain.CurrentRoom;
				else understoodAction.DirectObject = "I don't know where am I";
				return understoodAction;
			}
			#endregion

			#region Simple Actions
			command = command.Replace(" this ", " ");
			command = command.Replace(" the ", " ");
			command = command.Replace(" and ", " ");
			command = command.Replace(" a ", " ");
			command = command.Replace(" an ", " ");
			//command = command.Replace(" up ", " ");
			command = command.Replace(" into ", " ");
			command = command.Replace(" from ", " ");
            command = command.Replace(" to ", " ");
            command = command.Replace(" some ", " ");
			command = command.Replace(" seven up", " sevenup");
			command = command.Replace(" orange juice", " orangejuice");
			command = command.Replace(" peach juice", " peachjuice");
			command = command.Replace(" beer bottle", " beerbottle");
			command = command.Replace(" beer can", " beercan");
			command = command.Replace("clean up ", "clean ");
			command = command.Replace(" delivery location one", " deliverone");
			command = command.Replace(" delivery location two", " delivertwo");
			command = command.Replace(" delivery location three", " deliverthree");
			command = command.Replace(" tomato sauce", " tomatosouce");
			command = command.Replace(" hot sauce", " hotsouce");
			command = command.Replace(" lays intense", " laysintense");
			command = command.Replace(" orange juice", " orangejuice");
			command = command.Replace(" strawberry milk", " strawberrymilk");
			command = command.Replace(" pineapple juice", " pineapplejuice");
			command = command.Replace(" mango juice", " mangojuice");
			command = command.Replace(" chocolate milk", " chocolatemilk");
			command = command.Replace(" cleaning stuff", " cleaningstuff");
            command = command.Replace(" medical aid", " medicalaid");
            command = command.Replace(" bathroom light", " bathlight");

			char[] delimiters = { ' ', '\t' };
			string[] words = command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			List<int> wordsHashCodes = new List<int>();
			foreach (string s in words) wordsHashCodes.Add(s.GetHashCode());
			if (wordsHashCodes.Count < 2) return missunderstoodAction;

			if (!knownVerbs.ContainsKey(wordsHashCodes[0])) return missunderstoodAction;
			understoodAction.ActionClass = knownVerbsTypes[wordsHashCodes[0]];

			switch (understoodAction.ActionClass)
			{
				case VerbType.Answer:
					break;
				case VerbType.Ask:
					break;
				case VerbType.Bring:
					try
					{
						/*
						if (this.hal9000Brain.KnownObjects.ContainsKey(words[1]))
							understoodAction.DirectObject = words[1];
						else return missunderstoodAction;
						if (words[2] == "from" || words[2] == "to")
							understoodAction.Preposition = words[2];
						else return missunderstoodAction;
						if (this.hal9000Brain.KnownRooms.ContainsKey(words[3]) || this.hal9000Brain.KnownPersons.ContainsKey(words[3]))
							understoodAction.IndirectObject = words[3];
						else return missunderstoodAction;*/
						if (this.hal9000Brain.KnownObjects.ContainsKey(words[1]))
							understoodAction.DirectObject = words[1];
						else return missunderstoodAction;
						if (this.hal9000Brain.KnownRooms.ContainsKey(words[2]) || this.hal9000Brain.KnownPersons.ContainsKey(words[2]))
							understoodAction.IndirectObject = words[2];
						else return missunderstoodAction;
					}
					catch { }
					break;
				case VerbType.Find:
					try
					{
						if (this.hal9000Brain.KnownObjects.ContainsKey(words[1]))
						{
							understoodAction.DirectObject = words[1];
							understoodAction.IsAPerson = false;
						}
						else if (this.hal9000Brain.KnownPersons.ContainsKey(words[1]))
						{
							understoodAction.DirectObject = words[1];
							understoodAction.IsAPerson = true;
						}
						else if (words[1] == "person" || words[1] == "human")
						{
							understoodAction.DirectObject = "human";
							understoodAction.IsAPerson = true;
						}
						else return missunderstoodAction;
					}
					catch
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb find");
					}
					break;
				case VerbType.Follow:
					understoodAction.DirectObject = "nurse instructions";
					understoodAction.IsAPerson = false;
					break;
				case VerbType.Get:
					try
					{
						if (this.hal9000Brain.KnownObjects.ContainsKey(words[1]))
							understoodAction.DirectObject = words[1];
						else return missunderstoodAction;
						if (words.Length > 2)
						{
							if (this.hal9000Brain.KnownPersons.ContainsKey(words[2]))
								understoodAction.IndirectObject = words[2];
							else return missunderstoodAction;
						}
					}
					catch
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb get");
					}
					break;
				case VerbType.Go:
					try
					{
						if (this.hal9000Brain.KnownRooms.ContainsKey(words[1]))
							understoodAction.IndirectObject = words[1];
						else if (words[1] == "to")
						{
							if (this.hal9000Brain.KnownRooms.ContainsKey(words[2]))
								understoodAction.IndirectObject = words[2];
							else return missunderstoodAction;
						}
						else return missunderstoodAction;
					}
					catch
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb go");
					}
					break;
				case VerbType.Introduce:
					understoodAction.DirectObject = "my self";
					understoodAction.IsAPerson = true;
					break;
				case VerbType.Leave:
					understoodAction.DirectObject = "appartment";
					understoodAction.IsAPerson = false;
					break;
				case VerbType.Make:
					try
					{
						understoodAction.DirectObject = words[1];
					}
					catch
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb make");
					}
                    break;
                case VerbType.Turn:
                    //robot, turn bathroom lights on/off
                    try
                    {
                        understoodAction.DirectObject = words[1];
                        understoodAction.Complements = words[2];
                        
                    }
                    catch
                    {
                        TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb make");
                    }
                    break;
				case VerbType.StopFollow:
					understoodAction.IsAPerson = true;
					understoodAction.DirectObject = "follow you";
					break;
				case VerbType.Clean:
					//La word[1] es "up"
					try
					{
						if (this.hal9000Brain.KnownRooms.ContainsKey(words[1]))
							understoodAction.DirectObject = words[1];
						else return missunderstoodAction;
					}
					catch
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb clean");
					}
					understoodAction.IsAPerson = false;
					break;
				case VerbType.Learn:
					understoodAction.DirectObject = "";
					understoodAction.IndirectObject = "";
					break;

				case VerbType.Pour:
					try
					{
						if (this.hal9000Brain.KnownObjects.ContainsKey(words[1]))
							understoodAction.DirectObject = words[1];
						else return missunderstoodAction;
						if (this.hal9000Brain.KnownObjects.ContainsKey(words[2]))
							understoodAction.IndirectObject = words[2];
						else return missunderstoodAction;
					}
					catch
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000Language.-> Cannot understand verb pour");
					}
					break;
			}
			#endregion

			return understoodAction;
		}
	}
}

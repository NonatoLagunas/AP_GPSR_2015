using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Robotics.API;
using Robotics.API.PrimitiveSharedVariables;
using Robotics.API.MiscSharedVariables;
using Robotics.Controls;
using Robotics.HAL.Sensors;
using Robotics.Mathematics;
using ActionPlanner.Tests.StateMachines;

namespace ActionPlanner
{
	public delegate void HAL9000StatusChangedEventHandler(HAL9000StatusArgs args);
	public enum BrainWaveType { Theta = 200, Alpha = 100, Beta = 50 };
	public enum ArmsPP{ navigation, home, heilHitler};
    public enum TestToPerform
    {
        GPSR, Manipulation, Navigation, PersonRecognition, RoboZoo, AudioTest,
        OpenChallenge, RoboNurse, Restaurant, WakeMeUp, DefaultTest
    }


	//public enum TestNames { None, RobotInspection };
	
	public class HAL9000Brain
	{
        /**
         * Variables que guardan el estado actual del robot
         */
		private bool isAware;
		private string currentRoom;
		private string currentRegion;
		private string currentLocation;

        /**
         * Variables que guardan los nombres de los ARCHIVOS DE CONFIGURACIÓN.
         */
		private string validRoomsFile;
		private string validNamesFile;
		private string knownObjectsFile;

        /**
         * Variables donce se almacenará la información contenida en los ARCHIVOS DE COFIGURACIÓN.
         */
		private SortedList<string, Person> knownPersons;
		private SortedList<string, string> knownRooms;
		private SortedList<string, string> knownRegions;
		private SortedList<string, string> knownLocations;
		private SortedList<string, PhysicalObject> knownObjects;

        /**
         * Instancia del procesador de lenguaje natural (encargado Marco Negrete).
         */
		public NaturalLanguageProcessor languageProcessor;

        /**
         * Listas donde se almacenan la información de las variables compartidas en BB.
         */
		public Queue<string> recognizedSentences;
        public Queue<Gesture> lastRecoGestures;
        public Queue<string> lastRecoCellPhoneOrders;
        public Queue<string> lastPersonFallDetected;
        public Queue<string> sensorLectures;
        private List<SentenceImperative> actionsToPerform;

        /**
         * Variables compartidas de BB.
         */
		private StringSharedVariable gestureSharedVariable;
		private StringSharedVariable currentRoomSharedVar;
		private StringSharedVariable currentRegionSharedVar;
		private StringSharedVariable currentLocationSharedVar;
		private RecognizedSpeechSharedVariable recogSpeechsSharedVar;
		private StringSharedVariable skeletonsSharedVariable;
		private DoubleSharedVariable nearestObjForLocDirection;
        private IntSharedVariable svInGoorRegionForLoc;
        private StringSharedVariable cell_phoneSharedVariable;
        private StringSharedVariable fallSharedVariable;
        private StringSharedVariable sensorsSharedVariable;

        /**
         * FLAGS para determinar a que variables compartidas está suscrito el action planner.
         */
		private bool isSuscribedToGestureVar;
		private bool isSuscribedToRoomVar;
		private bool isSuscribedToRegionVar;
		private bool isSuscribedToLocationVar;
		private bool isSuscribedToRecoSpeechs;
		private bool isSuscribedToSkeletons;
		private bool isSuscribedToNearesObjForLoc;
        private bool isSuscribedToInGoodRegion;
        private bool isSuscribedToCellphone;
        private bool isSuscribedToFall;
        private bool isSuscribedToHomeSensors;

		private HAL9000CmdMan cmdMan;
		private ConnectionManager cnnMan;
		private CmdRecognized cmdRecognized;
		private CmdStartTest cmdStartTest;

		private Thread reasoningThread;
		private Thread predefinedTaskThread;
		private HAL9000Status status;

		//Variables for homogeneous transformations
		private double headPan;
		private double headTilt;
		

		//Variables for Robocup Tests
		List<string> foundHumans;
		private World world;

		// Variable para las pruebas
		public TestRobotInspection ripsTest;
		

		public HAL9000Brain()
		{
			this.isAware = false;
			this.currentRoom = "unknown";
			this.currentRegion = "unknown";
			this.currentLocation = "unknown";
			this.knownPersons = new SortedList<string, Person>();
			this.knownRooms = new SortedList<string, string>();
			this.knownRegions = new SortedList<string, string>();
			this.knownLocations = new SortedList<string, string>();
			this.knownObjects = new SortedList<string, PhysicalObject>();
			this.languageProcessor = new NaturalLanguageProcessor(this);
			this.actionsToPerform = new List<SentenceImperative>();
			this.recognizedSentences = new Queue<string>();
			this.lastRecoGestures = new Queue<Gesture>();
            this.lastPersonFallDetected = new Queue<string>();
            this.sensorLectures = new Queue<string>();

			this.validRoomsFile = "ValidRooms.txt";
			this.validNamesFile = "ValidNames.txt";
			this.knownObjectsFile = "KnownObjects.txt";

			this.world = new World();
			this.foundHumans = new List<string>();

			this.currentRoomSharedVar = new StringSharedVariable("robotRoom");
			this.currentRegionSharedVar = new StringSharedVariable("robotRegion");
			this.currentLocationSharedVar = new StringSharedVariable("robotLocation");
			this.gestureSharedVariable = new StringSharedVariable("gesture");
			this.recogSpeechsSharedVar = new RecognizedSpeechSharedVariable("recognizedSpeech");
			this.skeletonsSharedVariable = new StringSharedVariable("hf_skeletons");
			this.nearestObjForLocDirection = new DoubleSharedVariable("mp_objforloc");
            this.svInGoorRegionForLoc = new IntSharedVariable("mp_ingoodregion");
            this.cell_phoneSharedVariable = new StringSharedVariable("cell_phone");
            this.fallSharedVariable = new StringSharedVariable("positionhumanfall");
            this.sensorsSharedVariable = new StringSharedVariable("home_sensors");

			this.isSuscribedToGestureVar = false;
			this.isSuscribedToLocationVar = false;
			this.isSuscribedToRegionVar = false;
			this.isSuscribedToRoomVar = false;
			this.isSuscribedToRecoSpeechs = false;
			this.isSuscribedToSkeletons = false;
			this.isSuscribedToNearesObjForLoc = false;
            this.isSuscribedToInGoodRegion = false;
            this.isSuscribedToCellphone = false;
			this.isSuscribedToFall = false;
            this.isSuscribedToHomeSensors = false;

			this.status = new HAL9000Status();
			this.status.BrainWaveType = BrainWaveType.Alpha;

			this.cmdMan = new HAL9000CmdMan(this.status);
			this.cnnMan = new ConnectionManager("ACT-PLN", 2025, this.cmdMan);
			this.cnnMan.ClientConnected += new System.Net.Sockets.TcpClientConnectedEventHandler(cnnMan_ClientConnected);
			this.cnnMan.ClientDisconnected += new System.Net.Sockets.TcpClientDisconnectedEventHandler(cnnMan_ClientDisconnected);
			this.cnnMan.DataReceived += new ConnectionManagerDataReceivedEH(cnnMan_DataReceived);
            this.cmdMan.SharedVariablesLoaded += new SharedVariablesLoadedEventHandler(cmdMan_SharedVariablesLoaded);
			this.SetupCommandExecuters();
			this.cnnMan.Start();
			this.cmdMan.Start();
		}

		public double getHeadTilt()
		{
			return this.headTilt;
		}

        public HAL9000Status Status
        {
            get { return this.status; }
        }

		#region Connection To BB
		private void cnnMan_DataReceived(ConnectionManager connectionManager, System.Net.Sockets.TcpPacket packet)
		{
			//TextBoxStreamWriter.DefaultLog.WriteLine("RECEIVED: " + packet.DataString);
		}

		private void cnnMan_ClientDisconnected(System.Net.EndPoint ep)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Client Disconnected");
			this.status.ConnectionStatus = "Disconnected";
			this.status.IsConnectedToBB = false;
            this.status.IsSuscribedToGestureVar = false;
            this.status.IsSuscribedToLocationVar = false;
            this.status.IsSuscribedToRecoSpeechVar = false;
            this.status.IsSuscribedToRegionVar = false;
            this.status.IsSuscribedToRoomVar = false;
			this.OnStatusChanged(new HAL9000StatusArgs(this.status));
		}

		private void cnnMan_ClientConnected(System.Net.Sockets.Socket s)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Client Connected");         
			this.OnStatusChanged(new HAL9000StatusArgs(this.status));
		}

        private void cmdMan_SharedVariablesLoaded(CommandManager cmdMan)
        {
            int temp = 0;
            string temps;
            temp = this.cmdMan.SharedVariables.LoadFromBlackboard(5000, out temps);
            if (!String.IsNullOrEmpty(temps))
                TextBoxStreamWriter.DefaultLog.WriteLine("WorldMap: Shared Vars Loading Error: " + temps);

            this.status.IsConnectedToBB = true;
            this.status.ConnectionStatus = "Connected";
            this.SuscribeToSharedVars();
            this.OnStatusChanged(new HAL9000StatusArgs(this.status));
        }

		private void SetupCommandExecuters()
		{
			this.cmdRecognized = new CmdRecognized(this);
			this.cmdStartTest = new CmdStartTest(this);

			this.cmdMan.CommandExecuters.Add(this.cmdRecognized);
			this.cmdMan.CommandExecuters.Add(this.cmdStartTest);
		}
		#endregion

		#region Knowledge
		private bool loadValidRooms(string fileName)
		{
			if (!File.Exists(fileName)) return false;

			string[] lines = File.ReadAllLines(fileName);
			string[] parts;
			char[] delimiters = { ' ', '\t' };

			this.knownRooms.Clear();
			foreach (string s in lines)
			{
				if (s.StartsWith("//")) continue;
				if (s.Length < 3) continue;

				parts = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 1) continue;

				if (!this.knownRooms.ContainsKey(parts[0]))
					this.knownRooms.Add(parts[0], parts[0]);
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Repeated HashCode in valid rooms");
			}
			if (this.knownRooms.Count < 2) return false;

			return true;
		}

		private bool loadValidNames(string fileName)
		{
			if (!File.Exists(fileName)) return false;

			string[] lines = File.ReadAllLines(fileName);
			string[] parts;
			char[] delimiters = { ' ', '\t' };

			this.knownPersons.Clear();
			foreach (string s in lines)
			{
				if (s.StartsWith("//")) continue;
				if (s.Length < 3) continue;

				parts = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 1) continue;

				if (!this.knownPersons.ContainsKey(parts[0]))
				{
					Person tempPerson = new Person(parts[0]);
					this.knownPersons.Add(parts[0], tempPerson);
				}
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Repeated HashCode in valid names");
			}
			if (this.knownPersons.Count < 2) return false;

			return true;
		}

		private bool loadKnownObjects(string fileName)
		{
			if (!File.Exists(fileName)) return false;

			string[] lines = File.ReadAllLines(fileName);
			string[] parts;
			char[] delimiters = { ' ', '\t' };

			this.knownObjects.Clear();
			foreach (string s in lines)
			{
				if (s.StartsWith("//")) continue;
				if (s.Length < 3) continue;

				parts = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 1) continue;

				if (!this.knownObjects.ContainsKey(parts[0]))
				{
					PhysicalObject tempObject = new PhysicalObject(parts[0]);
					this.knownObjects.Add(parts[0], tempObject);
				}
				else
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Repeated HashCode in known objects: \"" + parts[0] + "\"");
			}
			if (this.knownObjects.Count < 2) return false;

			return true;
		}
		#endregion

		#region Tasks Threads
		private void ReasoningThreadTask()
		{
			int count = 50;
			while (--count > 0 && !this.status.IsSystemReady) Thread.Sleep(200);
			if (this.status.IsSystemReady)
			{
				this.cmdMan.Ready = true;
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SYSTEM READY");
			}
			else
			{
				if (!this.status.IsConnectedToBB) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Not connected to BB");
				if (!this.status.IsSuscribedToGestureVar) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> No suscribed to gesture variable");
				if (!this.status.IsSuscribedToLocationVar) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> No suscribed to location variable");
				if (!this.status.IsSuscribedToRegionVar) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> No suscribed to region variable");
				if (!this.status.IsSuscribedToRoomVar) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> No suscribed to room variable");
				if (!this.status.IsSuscribedToRecoSpeechVar) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> No suscribed to reco speech variable");
				if (!this.status.KnownLocationLoaded) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Known locations not loaded");
				if (!this.status.KnownObjectsLoaded) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Known objects not loaded");
				if (!this.status.KnownPersonsLoaded) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Known person not loaded");
				if (!this.status.KnownRegionsLoaded) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Known regions not loaded");
				if (!this.status.KnownRoomsLoaded) TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Known rooms not loaded");
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> SYSTEM NOT READY");
			}
			this.OnStatusChanged(new HAL9000StatusArgs(this.status));

            /////**CHECAR EL FUNCIONAMIENTO DEL ACTION PLANNER AL QUITAR ESTA LÍNEA***/////
			while (this.status.IsRunning)
			{
                Thread.Sleep((int)this.status.BrainWaveType);
			}
		}
		#endregion

		#region Shared Variables Methods
		private void SuscribeToSharedVars()
		{
			
			if (!this.isSuscribedToGestureVar)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.gestureSharedVariable.Name))
						this.cmdMan.SharedVariables.Add(this.gestureSharedVariable);
					else this.gestureSharedVariable = (StringSharedVariable)this.cmdMan.SharedVariables[this.gestureSharedVariable.Name];
					this.gestureSharedVariable.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.gestureSharedVariable.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(gestureSharedVariable_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to gesture variable");
					this.status.IsSuscribedToGestureVar = true;
					this.isSuscribedToGestureVar = true;
				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to gesture variable"); }
			}

			if (!this.isSuscribedToLocationVar)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.currentLocationSharedVar.Name))
						this.cmdMan.SharedVariables.Add(this.currentLocationSharedVar);
					else this.currentLocationSharedVar = (StringSharedVariable)this.cmdMan.SharedVariables[this.currentLocationSharedVar.Name];
					this.currentLocationSharedVar.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.currentLocationSharedVar.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(currentLocationSharedVar_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to location variable");
					this.status.IsSuscribedToLocationVar = true;
					this.isSuscribedToLocationVar = true;
				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to location variable"); }
			}
			
			if (!this.isSuscribedToRegionVar)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.currentRegionSharedVar.Name))
						this.cmdMan.SharedVariables.Add(this.currentRegionSharedVar);
					else this.currentRegionSharedVar = (StringSharedVariable)this.cmdMan.SharedVariables[this.currentRegionSharedVar.Name];
					this.currentRegionSharedVar.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.currentRegionSharedVar.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(currentRegionSharedVar_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to region variable");
					this.status.IsSuscribedToRegionVar = true;
					this.isSuscribedToRegionVar = true;
				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to region variable"); }
			}
			
			if (!this.isSuscribedToRoomVar)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.currentRoomSharedVar.Name))
						this.cmdMan.SharedVariables.Add(this.currentRoomSharedVar);
					else this.currentRoomSharedVar = (StringSharedVariable)this.cmdMan.SharedVariables[this.currentRoomSharedVar.Name];
					this.currentRoomSharedVar.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.currentRoomSharedVar.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(currentRoomSharedVar_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to room variable");
					this.status.IsSuscribedToRoomVar = true;
					this.isSuscribedToRoomVar = true;
				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to room variable"); }
			}
			
			if (!this.isSuscribedToRecoSpeechs)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.recogSpeechsSharedVar.Name))
						this.cmdMan.SharedVariables.Add(this.recogSpeechsSharedVar);
					else 
						this.recogSpeechsSharedVar = (RecognizedSpeechSharedVariable)this.cmdMan.SharedVariables[this.recogSpeechsSharedVar.Name];
					
					this.recogSpeechsSharedVar.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.recogSpeechsSharedVar.WriteNotification += new SharedVariableSubscriptionReportEventHadler<Robotics.HAL.Sensors.RecognizedSpeech>(recogSpeechsSharedVar_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to reco speech variable");
					this.status.IsSuscribedToRecoSpeechVar = true;
					this.isSuscribedToRecoSpeechs = true;

				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to reco speech variable"); }
			}
			if (!this.isSuscribedToSkeletons)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.skeletonsSharedVariable.Name))
						this.cmdMan.SharedVariables.Add(this.skeletonsSharedVariable);
					else this.skeletonsSharedVariable = (StringSharedVariable)this.cmdMan.SharedVariables[this.skeletonsSharedVariable.Name];
					this.skeletonsSharedVariable.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.skeletonsSharedVariable.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(skeletonsSharedVar_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to skeletons.");
					this.status.IsSuscribedToRoomVar = true;
					this.isSuscribedToSkeletons = true;
				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to skeletons variable"); }
			}
			if (!this.isSuscribedToNearesObjForLoc)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.nearestObjForLocDirection.Name))
						this.cmdMan.SharedVariables.Add(this.nearestObjForLocDirection);
					else this.nearestObjForLocDirection = (DoubleSharedVariable)this.cmdMan.SharedVariables[this.nearestObjForLocDirection.Name];
					this.nearestObjForLocDirection.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.nearestObjForLocDirection.WriteNotification += new SharedVariableSubscriptionReportEventHadler<double>(nearestObjForLocDirection_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to " + this.nearestObjForLocDirection.Name + " shared var");
					this.status.IsSuscribedToNearObjForLoc = true;
					this.isSuscribedToNearesObjForLoc = true;
				}
				catch
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to " + this.nearestObjForLocDirection.Name + " shared var");
				}
			}
			if (!this.isSuscribedToInGoodRegion)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.svInGoorRegionForLoc.Name))
						this.cmdMan.SharedVariables.Add(this.svInGoorRegionForLoc);
					else this.svInGoorRegionForLoc = (IntSharedVariable)this.cmdMan.SharedVariables[this.svInGoorRegionForLoc.Name];
					this.svInGoorRegionForLoc.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.svInGoorRegionForLoc.WriteNotification += new SharedVariableSubscriptionReportEventHadler<int>(svInGoorRegionForLoc_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to " + this.svInGoorRegionForLoc.Name + " shared var");
					this.status.IsSuscribedToInGoodRegion = true;
					this.isSuscribedToInGoodRegion = true;
				}
				catch
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HA9000.-> Can't suscribe to " + this.svInGoorRegionForLoc.Name + " shared var");
				}
            }
            if (!this.isSuscribedToCellphone)
            {
                try
                {
                    if (!this.cmdMan.SharedVariables.Contains(this.cell_phoneSharedVariable.Name))
                        this.cmdMan.SharedVariables.Add(this.cell_phoneSharedVariable);
                    else this.cell_phoneSharedVariable = (StringSharedVariable)this.cmdMan.SharedVariables[this.cell_phoneSharedVariable.Name];
                    this.cell_phoneSharedVariable.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
                    this.cell_phoneSharedVariable.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(cellphoneSharedVar_WriteNotification);
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to Cellphone.");
                    this.status.IsSuscribedToRoomVar = true;
                    this.isSuscribedToCellphone = true;
                }
                catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to cellphone variable"); }
            }

			if (!this.isSuscribedToFall)
			{
				try
				{
					if (!this.cmdMan.SharedVariables.Contains(this.fallSharedVariable.Name))
						this.cmdMan.SharedVariables.Add(this.fallSharedVariable);
					else this.fallSharedVariable = (StringSharedVariable)this.cmdMan.SharedVariables[this.fallSharedVariable.Name];
					this.fallSharedVariable.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
					this.fallSharedVariable.WriteNotification += new SharedVariableSubscriptionReportEventHadler<string>(fallSharedVariable_WriteNotification);
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Suscribed to Fall variable.");
					this.isSuscribedToFall = true;
				}
				catch { TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Can't suscribe to fall variable"); }
			}

            if (!this.isSuscribedToHomeSensors)
            {
                try
                {
                    if (!this.cmdMan.SharedVariables.Contains(this.sensorsSharedVariable.Name))
                        this.cmdMan.SharedVariables.Add(this.sensorsSharedVariable);
                    else this.sensorsSharedVariable = (StringSharedVariable)this.cmdMan.SharedVariables[this.sensorsSharedVariable.Name];
                    this.sensorsSharedVariable.Subscribe(SharedVariableReportType.SendContent, SharedVariableSubscriptionType.WriteOthers);
                    this.sensorsSharedVariable.WriteNotification+=new SharedVariableSubscriptionReportEventHadler<string>(sensorsSharedVariable_WriteNotification);
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000 -> Subscibed to Home Sensors variable.");
                    this.isSuscribedToHomeSensors = true;
                }
                catch
                {
                    TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000 -> Can't subscribe to Home Sensors variable");
                }
            }
		}

        void sensorsSharedVariable_WriteNotification(SharedVariableSubscriptionReport<string> report)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Home Sensors shared var updated.");
            if (!report.Equals(string.Empty))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Home Sensors shared var enqueued.");
                this.sensorLectures.Enqueue(report.Value);
            }
        }

		void fallSharedVariable_WriteNotification(SharedVariableSubscriptionReport<string> report)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Fall shared var updated.");
			if (!report.Equals(string.Empty))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Fall shared var enqueued.");
				this.lastPersonFallDetected.Enqueue(report.Value);
			}
		}

		private void svInGoorRegionForLoc_WriteNotification(SharedVariableSubscriptionReport<int> report)
		{
			this.cmdMan.InGoodRegionForLoc = report.Value;
		}

		private void nearestObjForLocDirection_WriteNotification(SharedVariableSubscriptionReport<double> report)
		{
			this.cmdMan.NearObjForLocDirection = report.Value;
		}

		private void recogSpeechsSharedVar_WriteNotification(SharedVariableSubscriptionReport<Robotics.HAL.Sensors.RecognizedSpeech> report)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Reco speech shared var updated");
			for (int i = 0; i < report.Value.Count; i++)
				this.recognizedSentences.Enqueue(report.Value[i].Text);

		}

		private void skeletonsSharedVar_WriteNotification(SharedVariableSubscriptionReport<string> report)
		{
			//TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Skeletons shared var updated: " + report.Value);
			//TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Skeletons shared var updated.");
		}

		private void currentRoomSharedVar_WriteNotification(SharedVariableSubscriptionReport<string> report)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Current room updated to: " + report.Value);
			this.currentRoom = report.Value;
		}

		private void currentRegionSharedVar_WriteNotification(SharedVariableSubscriptionReport<string> report)
		{
			this.currentRegion = report.Value;
		}

		private void currentLocationSharedVar_WriteNotification(SharedVariableSubscriptionReport<string> report)
		{
			this.currentLocation = report.Value;
		}

		private void gestureSharedVariable_WriteNotification(SharedVariableSubscriptionReport<string> report)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Received reconized gesture \"" + report.Value + "\"");
			char[] delimiters = { ' ' };
			string[] parts = report.Value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			this.lastRecoGestures.Enqueue(new Gesture(parts[0], parts[1]));
		}

        private void cellphoneSharedVar_WriteNotification(SharedVariableSubscriptionReport<string> report)
        {
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cellphone shared var updated.");
            if (!report.Equals(string.Empty))
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cellphone shared var enqueued.");
                this.lastRecoCellPhoneOrders.Enqueue(report.Value);
            }
        }
		#endregion

		#region Robocup Test Configuration

		private void StartRobocupTest(ThreadStart goalFunction)
		{
			if (this.status.IsExecutingPredefinedTask) return;
			if (this.predefinedTaskThread == null)
			{
				this.predefinedTaskThread = new Thread(goalFunction);
				this.predefinedTaskThread.IsBackground = true;
				this.status.IsExecutingPredefinedTask = true;
				this.predefinedTaskThread.Start();
				return;
			}

			if (this.predefinedTaskThread.IsAlive) return;

			this.predefinedTaskThread = new Thread(goalFunction);
			this.predefinedTaskThread.IsBackground = true;
			this.status.IsExecutingPredefinedTask = true;
			this.predefinedTaskThread.Start();
		}

		public void StopRobocupTest()
		{
			this.status.IsExecutingPredefinedTask = false;
			if (this.predefinedTaskThread == null) return;
			if (this.predefinedTaskThread.IsAlive) this.predefinedTaskThread.Join(200);
			if (this.predefinedTaskThread.IsAlive) this.predefinedTaskThread.Abort();
			this.status.TestBeingExecuted = "None";
			this.status.IsPaused = false;
			this.OnStatusChanged(new HAL9000StatusArgs(this.status));
		}

		public void PauseRobocupTest()
		{
			if (this.status.IsExecutingPredefinedTask)
				this.status.IsPaused = !this.status.IsPaused;
			string paused = this.status.IsPaused ? "paused" : "resumed";
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Robocup test " + paused);
			this.OnStatusChanged(new HAL9000StatusArgs(this.status));
		}

        public void StartSM(TestToPerform test)
        {
            switch (test)
            {
                case TestToPerform.AudioTest:
                    this.StartRobocupTest(new ThreadStart(this.AudioTest));
                    break;
                
                case TestToPerform.GPSR:
                    this.StartRobocupTest(new ThreadStart(this.GPSR));
                    break;

                case TestToPerform.Manipulation:
                    this.StartRobocupTest(new ThreadStart(this.Manipulation));
                    break;

                case TestToPerform.Navigation:
                    this.StartRobocupTest(new ThreadStart(this.Navigation));
                    break;

                case TestToPerform.OpenChallenge:
                    this.StartRobocupTest(new ThreadStart(this.OpenChallenge));
                    break;

                case TestToPerform.PersonRecognition:
                    this.StartRobocupTest(new ThreadStart(this.PersonRecognition));
                    break;

                case TestToPerform.Restaurant:
                    this.StartRobocupTest(new ThreadStart(this.Restaurant));
                    break;

                case TestToPerform.RoboNurse:
                    this.StartRobocupTest(new ThreadStart(this.RoboNurse));
                    break;

                case TestToPerform.RoboZoo:
                    this.StartRobocupTest(new ThreadStart(this.RoboZoo));
                    break;

                case TestToPerform.WakeMeUp:
                    this.StartRobocupTest(new ThreadStart(this.WakeMeUp));
                    break;

                case TestToPerform.DefaultTest:
                    this.StartRobocupTest(new ThreadStart(this.DefaultTest));
                    break;
            }
        }

		#endregion

		#region Robocup Test Execution
		private void AudioTest()
        {
            this.status.TestBeingExecuted = "Speech Recognition and Audio Detection";

            this.recognizedSentences.Clear();
            this.lastRecoGestures.Clear();

            AudioTest audioTest = new AudioTest(this, this.cmdMan);
            audioTest.Execute();

            StopRobocupTest();
		}

		private void RoboZoo()
        {
            this.status.TestBeingExecuted = "Robo Zoo";

			this.recognizedSentences.Clear();
			this.lastRecoGestures.Clear();

			RoboZoo robozooTest = new RoboZoo(this, this.cmdMan);
			robozooTest.Execute();

            StopRobocupTest();
		}
		private void GPSR()
        {
            this.status.TestBeingExecuted = "GPSR"; 

			this.recognizedSentences.Clear();
			this.lastRecoGestures.Clear();

            GPSR gpsrTest = new GPSR(this, this.cmdMan);
            gpsrTest.Execute();

            StopRobocupTest();
		}

		private void Manipulation()
        {
            this.status.TestBeingExecuted = "Manipulation and Object Recognition";

			this.recognizedSentences.Clear();
			this.lastRecoGestures.Clear();

            ManipulationTest manipulationTest = new ManipulationTest(this, this.cmdMan);
            manipulationTest.Execute();

            StopRobocupTest();
		}
		private void Navigation()
        {
            this.status.TestBeingExecuted = "Navigation Test";

			this.recognizedSentences.Clear();
			this.lastRecoGestures.Clear();

			NavigationTest navigationTest = new NavigationTest(this, this.cmdMan);
			navigationTest.Execute();

            StopRobocupTest();
		}

		private void PersonRecognition()
        {
            this.status.TestBeingExecuted = "Person Recognition";

			this.recognizedSentences.Clear();
			this.lastRecoGestures.Clear();

            PersonRecognition test = new PersonRecognition(this, this.cmdMan);
            test.Execute();

            StopRobocupTest();
		}

		private void OpenChallenge()
        {
            this.status.TestBeingExecuted = "Open Challenge";

            this.recognizedSentences.Clear();
            this.lastRecoGestures.Clear();

			OpenChallenge openChallengeTest = new OpenChallenge(this, cmdMan);
			openChallengeTest.Execute();

            StopRobocupTest();
		}

		private void Restaurant()
        {
            this.status.TestBeingExecuted = "Restaurant";

            this.recognizedSentences.Clear();
            this.lastRecoGestures.Clear();

			Restaurant restaurant = new Restaurant(this, this.cmdMan);
			restaurant.Execute();

            StopRobocupTest();
		}

        private void RoboNurse()
        {
            this.status.TestBeingExecuted = "Robo Nurse";

            this.recognizedSentences.Clear();
            this.lastRecoGestures.Clear();

            RoboNurse test = new RoboNurse(this, this.cmdMan);
            test.Execute();

            StopRobocupTest();
        }

        private void WakeMeUp()
        {
            this.status.TestBeingExecuted = "Wake Me Up";

            this.recognizedSentences.Clear();
            this.lastRecoGestures.Clear();

            WakeMeUp test = new WakeMeUp(this, this.cmdMan);
            test.Execute();

            StopRobocupTest();
        }

        public void DefaultTest()
        {
            this.status.TestBeingExecuted = "Default Test";

            this.recognizedSentences.Clear();
            this.lastRecoGestures.Clear();
            DefaultStateMachineSM defaultTest = new DefaultStateMachineSM(this, this.cmdMan);
            defaultTest.Execute();

            StopRobocupTest();
        }
		#endregion		
		
		#region NewSubTask

		public void SayAsync(string textToSay)
		{
			int timeForCharacter_ms = 250;
			int timeOut = textToSay.Length * timeForCharacter_ms + 1000; 

			if( ! this.cmdMan.SPG_GEN_asay( textToSay , timeOut)) 
				TextBoxStreamWriter.DefaultLog.WriteLine("!! SpGen TimeOut !!"+ timeForCharacter_ms.ToString() + "[ms] , textToSay:[ " + textToSay+" ]");

			Thread.Sleep(1000);
		}
		
		bool ArmsGoto(ArmsPP leftArm, ArmsPP rightArm)
		{
			bool success;

			this.cmdMan.ARMS_la_goto(leftArm.ToString());
			success= this.cmdMan.ARMS_ra_goto(rightArm.ToString(), 10000);

			success &= this.cmdMan.WaitForResponse(JustinaCommands.ARMS_la_goto, 2000);

			return success;
		}

		#endregion 

		#region  Subtasks execution

		/// <summary>
		/// Waits for a user's confirmation. If there is a new recognized sentence, the language processor
		/// understand it and returns yes or not. If there is no new reco senteces, returns false
		/// </summary>
		/// <param name="confirmation"> out: user's confirmation, yes or not</param>
		/// <returns>Wheter a confirmation was found or not</returns>
		private bool IsThereAUsersVoiceConfirmation(out bool confirmation)
		{
			confirmation = false;
			if (this.recognizedSentences.Count > 0)
			{
				SentenceImperative si = this.languageProcessor.Understand(this.recognizedSentences.Dequeue());
				if (si != null && si.ActionClass == VerbType.Confirm && si.DirectObject == "yes")
					confirmation = true;
				else confirmation = false;
				return true;
			}
			else return false;
		}

        public string WaitForHumanOrders(string question, int msTimeout, bool returnExpectedOnly, params string[] expectedStrings)
        {
            string recognized = "nothing";
            string expectedMatch = String.Empty;
            bool found = false;
			long milisecs = -11000;
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            do
            {
				if (sw.ElapsedMilliseconds - milisecs > 6000)
				{
					milisecs = sw.ElapsedMilliseconds;
					SayAsync(question);
				}

                this.recognizedSentences.Clear();
                while ((this.recognizedSentences.Count < 1) && (sw.ElapsedMilliseconds < msTimeout))
                {
                    Thread.Sleep(500);
                }
                if ((this.recognizedSentences.Count < 1) || (sw.ElapsedMilliseconds >= msTimeout))
                    return null;
                while (recognizedSentences.Count > 0)
                {
                    found = false;
                    recognized = recognizedSentences.Dequeue();
                    foreach (string expected in expectedStrings)
                    {
                        if (!recognized.ToLower().Contains(expected.ToLower()))
                            continue;

                        expectedMatch = expected;
                        found = true;
                        break;
                    }
					if (expectedMatch == "no")
						return expectedMatch;
                    if (found)
                    {
                        SayAsync("I heard " + recognized + ". Is that correct?");
						if (this.WaitForUserConfirmation())
						{
							return returnExpectedOnly ? expectedMatch : recognized;
						}
						else
						{
							SayAsync(question);
							milisecs = sw.ElapsedMilliseconds;
						}
                    }
                }

            } while (sw.ElapsedMilliseconds < msTimeout);
            return String.Empty;
        }

		public bool WaitForUserConfirmation()
		{
			bool userConfirmation;
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\UserConfirmRoutine.-> Waiting for user confirmation");
			Thread.Sleep(1000); //Sleep to avoid eco
			this.recognizedSentences.Clear();
			//int count = 0;	//QUTAR
			while (!this.IsThereAUsersVoiceConfirmation(out userConfirmation))
			//{//QUITAR
				//count++;//QUITAR
				//if(count%10==0 && count>0)
				//	SayAsync("please speak louder in the form: robot, my name is...");

				Thread.Sleep(500);
			//}//QUITAR
			if (userConfirmation)
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\UserConfirmRoutine.-> Received user confirmation \"yes\"");
			else
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\UserConfirmRoutine.-> Received user confirmation \"no\"");
			return userConfirmation;
		}

		public bool FindSpecificStringInRecoSentences(string strToFind)
		{

			if (this.recognizedSentences.Count > 0)
			{
				if (this.recognizedSentences.Dequeue().ToLower().Contains(strToFind))
					return true;
				else
					return false;
			}
			else return false;
		}

		public bool FindSpecificGesture(string gestureToFind, out string requestingID)
		{
			requestingID = "";
			Gesture g;
			if (this.lastRecoGestures.Count > 0)
			{
				g = this.lastRecoGestures.Dequeue();
				if (g.GestureName.ToLower() == gestureToFind)
				{
					requestingID = g.ID;
					return true;
				}
				else return false;
			}
			else return false;
		}

		public bool FindUserComplexCommand(out SentenceImperative foundAction)
		{
			foundAction = null;
			if (this.recognizedSentences.Count > 0)
			{
				foundAction = this.languageProcessor.Understand(this.recognizedSentences.Dequeue());
				if (foundAction != null && foundAction.ActionClass != VerbType.Say && foundAction.ActionClass != VerbType.Confirm) return true;
				else return false;
			}
			else return false;
		}

		private void AnswerCommonQuestion()
		{
			if (this.recognizedSentences.Count > 0)
			{
				SentenceImperative foundAction = this.languageProcessor.Understand(this.recognizedSentences.Dequeue());
				if (foundAction != null && foundAction.ActionClass == VerbType.Say)
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Answering \"" + foundAction.DirectObject + "\"");
					this.cmdMan.SPG_GEN_say(foundAction.DirectObject, 10000);
				}
			}
		}

		/// <summary>
		/// Checks if there is a stopfollow command. Returns true only when a stopfollow command is received.
		/// Otherwise, returns false and answer a common question if received.
		/// </summary>
		/// <returns></returns>
		private bool AnswerCommonQuestionOrCatchStop()
		{
			if (this.recognizedSentences.Count > 0)
			{
				string strToAnalyse = this.recognizedSentences.Dequeue();
				SentenceImperative foundAction = this.languageProcessor.Understand(strToAnalyse);
				if (foundAction != null && foundAction.ActionClass == VerbType.StopFollow)
					return true;
				else if (foundAction != null && foundAction.ActionClass == VerbType.Say)
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Answering \"" + foundAction.DirectObject + "\"");
					this.cmdMan.SPG_GEN_say(foundAction.DirectObject);
					return false;
				}
				else return false;
			}
			else return false;
		}

		private bool FindAndRememberAnUnknownHuman(string defaultName, out string foundHuman)
		{
			bool fail = false;
			bool taskExecuted = false;
			bool succes = false;
			int nextState = 0;
			string foundHumanName = "";
			int watingTimeCounter = 0;
			int waitingConfirmationCounter = 0;
			bool userConfirmation = false;
			List<string> rejectedNames = new List<string>();
			int rejectionsCounting = 0;

			this.recognizedSentences.Clear();
			while (this.status.IsRunning && this.status.IsExecutingPredefinedTask && !fail && !taskExecuted)
			{
				switch (nextState)
				{
					case 0:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Looking for human");
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Activating reco human");
						this.cmdMan.PRS_FND_sleep(false);
						this.cmdMan.SPG_GEN_asay("I am looking for human", 2000);
						if (this.cmdMan.ST_PLN_findhuman("human", "hdtilt tor", 90000, out foundHumanName))
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Found human: " + foundHumanName +
								". Sending <alignhuman> command");
							this.cmdMan.ST_PLN_alignhuman("", 20000);
							this.cmdMan.SPG_GEN_shutup(2000);
							Thread.Sleep(500);
							this.cmdMan.SPG_GEN_say("Hello human, I am the robot justina, please tell me your name.", 15000);
						}
						else
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot find human");
							this.cmdMan.SPG_GEN_shutup(2000);
							Thread.Sleep(500);
							this.cmdMan.SPG_GEN_say("I cannot find a human. I will continue with the test.", 10000);
							this.cmdMan.HEAD_lookat(0, 0, 4000);
							this.cmdMan.SPG_GEN_say("Please, tell me your name", 10000);
						}
						this.recognizedSentences.Clear();
						nextState = 10;
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Waiting for human name");
						break;
					case 10:
						if (this.recognizedSentences.Count > 0)
						{
							string strToAnalyse = this.recognizedSentences.Dequeue();
							nextState = 10;
							foreach (Person p in this.knownPersons.Values)
								if (strToAnalyse.Contains(p.Name) && !rejectedNames.Contains(p.Name))
								{
									watingTimeCounter = 0;
									nextState = 20;
									foundHumanName = p.Name;
									TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Received \"my name is " + p.Name + "\"");
								}
						}
						else if ((++watingTimeCounter % 30) == 0)
						{
							//watingTimeCounter = 0;
							if (watingTimeCounter > 150)
							{
								watingTimeCounter = 0;
								TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> No name given by user. Using default name");
								this.cmdMan.SPG_GEN_shutup(2000);
								this.cmdMan.SPG_GEN_say("I cannot understand your name. You will be " + defaultName +
									" for the remainder of the test");
								foundHumanName = defaultName;
								nextState = 35;
							}
							else
							{
								this.cmdMan.SPG_GEN_shutup(2000);
								Thread.Sleep(500);
								this.cmdMan.SPG_GEN_say("Please, tell me your name again", 12000);
								this.recognizedSentences.Clear();
								nextState = 10;
							}
						}
						else nextState = 10;
						break;
					case 20:
						this.cmdMan.SPG_GEN_say("Is your name " + foundHumanName + "? Please say robot yes or robot no to confirm", 13000);
						this.recognizedSentences.Clear();
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Waiting for user's confirmation");
						nextState = 30;
						watingTimeCounter = 0;
						break;
					case 30:
						if (!this.IsThereAUsersVoiceConfirmation(out userConfirmation))
						{
							if (++waitingConfirmationCounter > 30)
							{
								this.cmdMan.SPG_GEN_shutup(2000);
								this.cmdMan.SPG_GEN_say("Please confirm again", 5000);
								this.recognizedSentences.Clear();
								waitingConfirmationCounter = 0;
							}
							nextState = 30;

						}
						else if (userConfirmation)
						{
							nextState = 35;
						}
						else
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Confirm \"no\" received");
							this.cmdMan.SPG_GEN_say("O.K. Please tell me your name again", 10000);
							this.recognizedSentences.Clear();
							rejectedNames.Add(foundHumanName);
							rejectionsCounting++;
							nextState = 40;
						}
						break;
					case 35:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Confirm \"yes\" received");
						this.cmdMan.SPG_GEN_say("O.K. I am going to remember your face. Please look straight forward to my eyes", 12000);
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Sending <remember_human> command");
						if (this.cmdMan.ST_PLN_rememberhuman(foundHumanName, "", 60000))
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Face of " + foundHumanName + " remembered succesfully");
							this.cmdMan.SPG_GEN_say("I have remembered your face.", 10000);
							this.cmdMan.HEAD_lookat(0, 0, 4000);
							succes = true;
						}
						else
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot remember the face of " + foundHumanName);
							this.cmdMan.SPG_GEN_say("I cannot remember your face. I will continue with the test", 12000);
							this.cmdMan.HEAD_lookat(0, 0, 4000);
						}
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumandRoutine.-> Desactivating reco human");
						this.cmdMan.PRS_FND_sleep(true);
						taskExecuted = true;
						break;
					case 40:
						if (this.recognizedSentences.Count > 0)
						{
							string strToAnalyse = this.recognizedSentences.Dequeue();
							nextState = 40;
							foreach (Person p in this.knownPersons.Values)
								if (strToAnalyse.Contains(p.Name) && !rejectedNames.Contains(p.Name))
								{
									watingTimeCounter = 0;
									nextState = 50;
									foundHumanName = p.Name;
									TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Received \"my name is " + p.Name + "\"");
								}
						}
						else if ((++watingTimeCounter % 30) == 0)
						{
							//watingTimeCounter = 0;
							if (watingTimeCounter > 150)
							{
								watingTimeCounter = 0;
								TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> No name given by user. Using default name");
								this.cmdMan.SPG_GEN_shutup(2000);
								this.cmdMan.SPG_GEN_say("I cannot understand your name. You will be " + defaultName +
									" for the remainder of the test");
								foundHumanName = defaultName;
								nextState = 65;
							}
							else
							{
								this.cmdMan.SPG_GEN_shutup(2000);
								Thread.Sleep(500);
								this.cmdMan.SPG_GEN_say("Please, tell me your name again", 12000);
								this.recognizedSentences.Clear();
								nextState = 40;
							}
						}
						else nextState = 40;
						break;
					case 50:
						this.cmdMan.SPG_GEN_say("Is your name " + foundHumanName + "?", 13000);
						this.recognizedSentences.Clear();
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Waiting for user's confirmation");
						nextState = 60;
						break;
					case 60:
						if (!this.IsThereAUsersVoiceConfirmation(out userConfirmation))
						{
							if (++waitingConfirmationCounter > 30)
							{
								this.cmdMan.SPG_GEN_shutup(2000);
								this.cmdMan.SPG_GEN_say("Please confirm again", 5000);
								this.recognizedSentences.Clear();
								waitingConfirmationCounter = 0;
							}
							nextState = 60;
						}
						else if (userConfirmation)
						{
							nextState = 65;
						}
						else
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Confirm \"no\" received");
							this.cmdMan.SPG_GEN_say("O.K. Please tell me your name again", 10000);
							this.recognizedSentences.Clear();
							rejectedNames.Add(foundHumanName);
							if (++rejectionsCounting > 3)
							{
								TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Too much rejected names. Use of default names");
								this.cmdMan.SPG_GEN_shutup(2000);
								this.cmdMan.SPG_GEN_say("I cannot understand your name. You will be " + defaultName +
									" for the remainder of the test");
								foundHumanName = defaultName;
								nextState = 65;
							}
							else nextState = 40;
						}
						break;
					case 65:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Confirm \"yes\" received");
						this.cmdMan.SPG_GEN_say("O.K. I am going to remember your face. Please look straight forward to my eyes", 12000);
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Sending <remember_human> command");
						if (this.cmdMan.ST_PLN_rememberhuman(foundHumanName, "", 60000))
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Face of " + foundHumanName + " remembered succesfully");
							this.cmdMan.SPG_GEN_say("I have remembered your face.", 10000);
							this.cmdMan.HEAD_lookat(0, 0, 4000);
							succes = true;
						}
						else
						{
							TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Cannot remember the face of " + foundHumanName);
							this.cmdMan.SPG_GEN_say("I cannot remember your face. I will continue with the test", 12000);
							this.cmdMan.HEAD_lookat(0, 0, 4000);
						}
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumandRoutine.-> Desactivating reco human");
						this.cmdMan.PRS_FND_sleep(true);
						taskExecuted = true;
						break;
					default:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Invalid State");
						fail = true;
						break;
				}
				Thread.Sleep((int)this.status.BrainWaveType);
			}

			if (fail)
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Can't execute \"findHumanAndRemember\" routine");
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Current subroutine state: " + nextState.ToString());
			}
			else TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\FindHumanRoutine.-> Subroutine \"Find and Remember Human\" executed succesfully");

			foundHuman = foundHumanName;
			return !fail && succes;
		}

		private bool FindObjectAndTakeIt(out string foundObjectName)
		{
			foundObjectName = "unknown";
			return true;
		}

		public bool DoPresentation()
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\DoPresSubroutine.-> Executing doPresentation");
			this.cmdMan.SPG_GEN_say("Hello. I am the Robot Justina and I was built in the National University of Mexico", 15000);
			this.cmdMan.SPG_GEN_say("I would like to show you my design", 10000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.SPG_GEN_say("I have a mecha-tronic head");
			this.cmdMan.ARMS_ra_goto("showHead", 10000);
			this.cmdMan.HEAD_lookat(0.7, 0.0, 10000);
			this.cmdMan.HEAD_lookat(0.0, 0.5, 10000);
			this.cmdMan.HEAD_lookat(-0.7, 0.0, 10000);
			this.cmdMan.HEAD_lookat(0.0, -0.5, 10000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.HEAD_lookat(0, 0, 10000);
			this.cmdMan.SPG_GEN_say("I have a stereo camera in my face");
			this.cmdMan.ARMS_ra_goto("showHead", 10000);
			this.cmdMan.ARMS_ra_opengrip(5000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			//this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.ARMS_goto("showArm", 12000);
			this.cmdMan.SPG_GEN_say("I have two arms", 10000);
			this.cmdMan.SPG_GEN_say("These are an anthropomophic seven degree of freedom manipulators");
			this.cmdMan.ARMS_ra_opengrip(5000);
			this.cmdMan.ARMS_la_opengrip(5000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			//this.cmdMan.ARMS_la_closegrip(5000);
			Thread.Sleep(2000);
			this.cmdMan.ARMS_la_goto("home", 10000);
			//this.cmdMan.ARMS_la_opengrip(5000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.SPG_GEN_say("I have a kinect system");
			Thread.Sleep(1500);
			this.cmdMan.ARMS_ra_goto("showHead", 10000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.SPG_GEN_say("I have a laser sensor");
			this.cmdMan.ARMS_ra_goto("showLaser", 10000);
			this.cmdMan.ARMS_ra_goto("home", 12000);
			this.cmdMan.SPG_GEN_say("Finally, I have a differential pair mobile base for navigating", 15000);
			cmdMan.MVN_PLN_move(0, Math.PI / 4, 4000);
			cmdMan.MVN_PLN_move(0, -Math.PI / 4, 4000);
			this.cmdMan.SPG_GEN_say("Thank you very much for your attention");
			//this.cmdMan.ARMS_ra_move("hello", 15000);
			//this.cmdMan.ARMS_ra_goto("home", 10000);
			return true;
		}

		public bool DoPresentationSpanish()
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\DoPresSubroutine.-> Executing doPresentation");
			this.cmdMan.HEAD_lookat(0, 0, 1000);
			this.cmdMan.SPG_GEN_say("Hola. Soy Justina y soy un Robot. Fui construida en la Universidad Nacional Autonoma de Mexico.", 15000);
			this.cmdMan.SPG_GEN_say("Me encantaria mostrarte mi disenio.", 10000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);			

			this.cmdMan.SPG_GEN_say("Tengo una cabeza mecatronica.");
			this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			this.cmdMan.ARMS_ra_goto("showHead", 10000);
			this.cmdMan.HEAD_lookat(0.7, 0.0, 10000);
			this.cmdMan.HEAD_lookat(0.0, 0.5, 10000);
			this.cmdMan.HEAD_lookat(-0.7, 0.0, 10000);
			this.cmdMan.HEAD_lookat(0.0, -0.5, 10000);
			this.cmdMan.HEAD_lookat(0, 0, 10000);

			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.ARMS_la_closegrip(5000);
			//this.cmdMan.ARMS_ra_hand_move("fist", 1000);
			this.cmdMan.SPG_GEN_say("Tengo una camara estereo que uso como ojos.");
			this.cmdMan.ARMS_ra_goto("showHead", 10000);
			//this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			/*this.cmdMan.ARMS_ra_opengrip(5000);
			this.cmdMan.ARMS_ra_closegrip(5000);*/
			//this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.ARMS_goto("showArm", 12000);
			this.cmdMan.SPG_GEN_say("Tambien tengo dos brazos.", 10000);
			this.cmdMan.SPG_GEN_say("Ambos son manipuladores antropomorficos de siete grados de libertad.");
			//this.cmdMan.ARMS_ra_hand_move("point", 1000);
			this.cmdMan.ARMS_ra_opengrip(5000);
			this.cmdMan.ARMS_la_opengrip(5000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			this.cmdMan.ARMS_la_closegrip(5000);
			//this.cmdMan.ARMS_ra_hand_move("fist", 1000);
			Thread.Sleep(2000);
			this.cmdMan.ARMS_la_goto("home", 10000);
			//this.cmdMan.ARMS_la_opengrip(5000);
			//this.cmdMan.ARMS_ra_goto("heilHitler", 12000);			
			this.cmdMan.SPG_GEN_say("Tengo un sistema Kinect");
			Thread.Sleep(1500);
			this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			this.cmdMan.ARMS_ra_goto("showKinect", 10000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			
			this.cmdMan.ARMS_ra_goto("showLaser", 10000);
			this.cmdMan.SPG_GEN_say("Tengo ademas un sensor laser.");
			Thread.Sleep(1000);
			//this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			this.cmdMan.ARMS_ra_goto("home", 12000);
			this.cmdMan.SPG_GEN_say("Finalmente, Tengo una base movil de par diferencial.", 15000);
			cmdMan.MVN_PLN_move(0, Math.PI / 4, 4000);
			cmdMan.MVN_PLN_move(0, -Math.PI / 4, 4000);
			this.cmdMan.SPG_GEN_say("Muchas gracias por su atencion.");
			this.cmdMan.ARMS_ra_goto("navigation", 10000);
			this.cmdMan.ARMS_ra_hand_move("good", 1000);
			Thread.Sleep(1000);
			//this.cmdMan.ARMS_ra_move("hello", 15000);
			this.cmdMan.ARMS_ra_goto("home", 10000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			return true;
		}

		public bool DoPresentationMultipleLanguage(string lang)
		{
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\DoPresSubroutine.-> Executing doPresentation");
			this.cmdMan.HEAD_lookat(0, 0, 1000);

			string presentation = "Hola. Soy Justina y soy un Robot. Fu(i') construida en la Universidad Nacional Autonoma de M(e')xico. Me encantaria mostrarte mi dise(n-)o.";
			string cabeza = "tengo una cabeza mecatronica.";
			string brazos = "Tambi(e')n tengo dos brazos. Ambos son manipuladores antropomorficos de siete grados de libertad.";
			string kinect = "Tengo un sistema Kinect";
			string laser = "Tengo ademas un sensor laser.";
			string presBase = "Finalmente, Tengo una base movil de par diferencial.";
			string gracias = "Muchas gracias por su atenci(o')n";

			if (lang.Equals("ENG"))
			{
				presentation = "Hello. I am the Robot Justina and I was built in the National University of Mexico. I would like to show you my design";
				cabeza = "I have a mecha-tronic head";
				brazos = "I have two arms. These are an anthropomophic seven degree of freedom manipulators";
				kinect = "I have a kinect system";
				laser = "I have a laser sensor";
				presBase = "Finally, I have a differential pair mobile base for navigating";
				gracias = "Thank you very much for your attention";
			}
			if (lang.Equals("POR"))
			{
				presentation = "Hola. Soy Justina y soy un Robot. Fu(i') construida en la Universidad Nacional Autonoma de M(e')xico. Me encantaria mostrarte mi disenho.";
				cabeza = "tengo una cabeza mecatronica.";
				brazos = "Tambi(e')n tengo dos brazos. Ambos son manipuladores antropomorficos de siete grados de libertad.";
				kinect = "Tengo un sistema Kinect";
				laser = "Tengo ademas un sensor laser.";
				presBase = "Finalmente, Tengo una base movil de par diferencial.";
				gracias = "Muchas gracias por su atenci(o')n";
			}

			this.cmdMan.SPG_GEN_say(presentation, 25000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);

			this.cmdMan.SPG_GEN_say(cabeza);
			this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			this.cmdMan.ARMS_ra_goto("showHead", 10000);
			this.cmdMan.HEAD_lookat(0.7, 0.0, 10000);
			this.cmdMan.HEAD_lookat(0.0, 0.5, 10000);
			this.cmdMan.HEAD_lookat(-0.7, 0.0, 10000);
			this.cmdMan.HEAD_lookat(0.0, -0.5, 10000);
			this.cmdMan.HEAD_lookat(0, 0, 10000);

			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.ARMS_la_closegrip(5000);
			//this.cmdMan.ARMS_ra_hand_move("fist", 1000);
			//this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			/*this.cmdMan.ARMS_ra_opengrip(5000);
			this.cmdMan.ARMS_ra_closegrip(5000);*/
			//this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.ARMS_goto("showArm", 12000);
			this.cmdMan.SPG_GEN_say(brazos, 20000);
			//this.cmdMan.ARMS_ra_hand_move("point", 1000);
			this.cmdMan.ARMS_ra_opengrip(5000);
			this.cmdMan.ARMS_la_opengrip(5000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			this.cmdMan.ARMS_la_closegrip(5000);
			//this.cmdMan.ARMS_ra_hand_move("fist", 1000);
			Thread.Sleep(2000);
			this.cmdMan.ARMS_la_goto("home", 10000);
			//this.cmdMan.ARMS_la_opengrip(5000);
			//this.cmdMan.ARMS_ra_goto("heilHitler", 12000);
			this.cmdMan.SPG_GEN_say(kinect);
			Thread.Sleep(1500);
			this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			this.cmdMan.ARMS_ra_goto("showKinect", 10000);
			this.cmdMan.ARMS_ra_goto("heilHitler", 12000);

			this.cmdMan.ARMS_ra_goto("showLaser", 10000);
			this.cmdMan.SPG_GEN_say(laser);
			Thread.Sleep(2000);
			//this.cmdMan.ARMS_ra_hand_move("mark", 1000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			this.cmdMan.ARMS_ra_goto("home", 12000);
			this.cmdMan.SPG_GEN_say(presBase, 15000);
			cmdMan.MVN_PLN_move(0, Math.PI / 4, 4000);
			cmdMan.MVN_PLN_move(0, -Math.PI / 4, 4000);
			this.cmdMan.SPG_GEN_say(gracias);
			this.cmdMan.ARMS_ra_goto("navigation", 10000);
			this.cmdMan.ARMS_ra_hand_move("good", 1000);
			Thread.Sleep(1000);
			//this.cmdMan.ARMS_ra_move("hello", 15000);
			this.cmdMan.ARMS_ra_goto("home", 10000);
			this.cmdMan.ARMS_ra_closegrip(5000);
			return true;
		}

		public void WaitForTheDoorToBeOpened()
		{
			bool fail = false;
			bool taskExecuted = false;
			int nextState = 0;

			while (this.status.IsRunning && this.status.IsExecutingPredefinedTask && !fail && !taskExecuted)
			{
				switch (nextState)
				{
					case 0:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> Saying 'I am waiting ...'");
						this.cmdMan.SPG_GEN_say("I am waiting for the door is opened", 10000);
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> Waiting for the door is opened");
						
						while (this.cmdMan.MVN_PLN_obstacle("door", 2000)) 
							Thread.Sleep(500);
						
						if (!this.cmdMan.MVN_PLN_obstacle("door", 2000)) 
							nextState = 10;
						else 
							nextState = 0;
						
							break;
					case 10:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> The door is open");
						this.cmdMan.SPG_GEN_say("I can see now that the door is open", 10000);
						taskExecuted = true;
						break;
					default:
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> Invalid State");
						fail = true;
						break;
				}
				Thread.Sleep((int)this.status.BrainWaveType);
			}

			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\WaitForDoorRoutine.-> Subroutine \"wait for door\" executed succesfully");

			return;
		}

		private Vector3 CalculateKinectToArm(double x, double y, double z, double headPan, double headTilt)
		{
			double x4, y4, z4;
			x4 = -y;
			y4 = z;
			z4 = -x;

			HomogeneousM headTiltM = new HomogeneousM(-Math.PI / 2 + headTilt, 0, -0.065, 0);
			HomogeneousM headPanM = new HomogeneousM(headPan, 0.23, 0.0425, Math.PI / 2);
			HomogeneousM result = new HomogeneousM(headPanM.Matrix * headTiltM.Matrix);

			Vector3 pos = new Vector3(x4, y4, z4);
			Vector3 resultPos = result.Transform(pos);

			resultPos.X += 0.03;
			resultPos.Y += 0.145;

			Vector3 armPos = new Vector3(-resultPos.Z, resultPos.X, -resultPos.Y);

			return armPos;
		}

		public bool GetCloseToTable(string location, int timeout_ms)
		{
			//this.cmdMan.ARMS_ra_goto("home", 8000);
			//this.cmdMan.ARMS_la_goto("home", 8000);
			if (!cmdMan.MVN_PLN_getclose(location, timeout_ms))
				if (!cmdMan.MVN_PLN_getclose(location, timeout_ms))
					if (cmdMan.MVN_PLN_getclose(location, timeout_ms))
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location reached: " + location);
					}

			return AlignWithTableEdgeUsingVision();
		}

		public bool AlignWithTableEdgeUsingVision()
		{

			Vector3 p0 = Vector3.Zero;
			Vector3 p1 = Vector3.Zero;
			Vector3 p2 = Vector3.Zero;
			double x0, y0, z0, x1, y1, z1, x2, y2, z2;
			x0 = y0 = z0 = x1 = y1 = z1 = x2 = y2 = z2 = 0;
			uint tunedSuccessCounter = 0;
			uint reloadedSuccessCounter = 0;

            this.headPan = 0;
            this.cmdMan.HEAD_lookat(0, 0, 7000);
			this.headTilt = MathUtil.ToRadians(-46);
			if (!this.cmdMan.HEAD_lookat(this.headPan, this.headTilt, 7000))
				this.cmdMan.HEAD_lookat(this.headPan, this.headTilt, 7000);
			Thread.Sleep(2000);

			//Here we are assuming kinect has coordinate system of: x -> right, y -> up, z -> front
			//And MVN-PLN coordinates are: x -> front, y -> left, z -> up

			for (int i = 0; i < 3; i++)
			{
				if (this.cmdMan.OBJ_FNDT_findedgetuned(this.headTilt, out x0, out y0, out z0, out x1, out y1, out z1, 5000))
				{
					p0.X -= x0;
					p0.Y += y0;
					p0.Z += z0;
					p1.X -= x1;
					p1.Y += y1;
					p1.Z += z1;

					tunedSuccessCounter++;
				}
			}

			for (int i = 0; i < 3; i++)
			{
				if (this.cmdMan.OBJ_FNDT_findedgereloaded(this.headTilt, out x2, out y2, out z2, 5000))
				{
					p2.X -= x2;
					p2.Y += y2;
					p2.Z += z2;

					reloadedSuccessCounter++;
				}
			}

            if (tunedSuccessCounter > 0)
            {
                p0.X /= tunedSuccessCounter;
                p0.Y /= tunedSuccessCounter;
                p0.Z /= tunedSuccessCounter;
                p1.X /= tunedSuccessCounter;
                p1.Y /= tunedSuccessCounter;
                p1.Z /= tunedSuccessCounter;
            }
            if (reloadedSuccessCounter > 0)
            {
                p2.X /= reloadedSuccessCounter;
                p2.Y /= reloadedSuccessCounter;
                p2.Z /= reloadedSuccessCounter;
            }
			if (tunedSuccessCounter > 0 && reloadedSuccessCounter > 0)
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("tuned method worked " + tunedSuccessCounter + " times.");
				TextBoxStreamWriter.DefaultLog.WriteLine("reloaded method worked " + reloadedSuccessCounter + " times.");

				Vector3 unitLineVector = (p1 - p0).Unitary;
				Vector3 unitOtherPointVector = (p2 - p0).Unitary;

				if (Math.Abs(Vector3.Dot(unitLineVector, unitOtherPointVector)) >= 0.8)
				{
					p0 = new Vector3(p0.Z, p0.X, p0.Y);
					p1 = new Vector3(p1.Z, p1.X, p1.Y);

					//Las coordenadas de la línea de tendencia son enviadas con respecto a la base del robot
					if (this.cmdMan.MVN_PLN_getclose(p0.X - 0.22, p0.Y, p0.Z, p1.X - 0.22, p1.Y, p1.Z, 10000))
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Aliged with table succesfully usign tuned method.");
						return true;
					}
				}
				else
				{
					TextBoxStreamWriter.DefaultLog.WriteLine(string.Format("HAL9000.-> getting close with reloaded method... distance: {0}\t\t angle: {1}", Math.Sqrt(p2.X * p2.X + p2.Z * p2.Z) - 0.10, Math.Atan2(p2.X, p2.Z)));
					this.cmdMan.MVN_PLN_move(Math.Sqrt(p2.X * p2.X + p2.Z * p2.Z)-0.22, Math.Atan2(p2.X, p2.Z), 10000);
					return true;
				}

				return false;
			}

			if (tunedSuccessCounter > 0)
			{
				p0 = new Vector3(p0.Z, p0.X, p0.Y);
				p1 = new Vector3(p1.Z, p1.X, p1.Y);

				//Las coordenadas de la línea de tendencia son enviadas con respecto a la base del robot
				if (!this.cmdMan.MVN_PLN_getclose(p0.X - 0.22, p0.Y, p0.Z, p1.X - 0.22, p1.Y, p1.Z, 10000))
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot align with table edge");
					return false;
				}

				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Aliged with table succesfully usign tuned method.");

				return true;
			}

			if (reloadedSuccessCounter > 0)
			{
				TextBoxStreamWriter.DefaultLog.WriteLine(string.Format("HAL9000.-> getting close with reloaded method... distance: {0}\t\t angle: {1}", Math.Sqrt(p2.X * p2.X + p2.Z * p2.Z) - 0.10, Math.Atan2(p2.X, p2.Z)));
				this.cmdMan.MVN_PLN_move(Math.Sqrt(p2.X * p2.X + p2.Z * p2.Z)-0.22, Math.Atan2(p2.X, p2.Z), 10000);
				return true;
			}
			
			//ESTA LÍNEA ES PARA CORREGIR EL POSIBLE ERROR DEBIDO A UN MAL CÁLCULO DE POSICIÓN DE LA CABEZA
			//HAY QUE QUITAR ESTA LINEA Y CORREGIR LA CABEZA
			//HAY QUE DECIRLE A iMe QUE CORRIJA LA CABEZA
			//this.headTilt *= 1.1;
			//p0 = this.TransHeadKinect2Robot(p0);
			//p1 = this.TransHeadKinect2Robot(p1);

			TextBoxStreamWriter.DefaultLog.WriteLine("Align with vision failed.");

			return false;
		}

		public bool GetCloseToTable(string location, out double height, int timeout_ms)
		{
			height = 0;
			bool success;
			if (cmdMan.MVN_PLN_getclose(location, timeout_ms))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Location reached: " + location);
			}
			success=AlignWithTableEdgeUsingVision(out height);
			return success;
		}

		public bool AlignWithTableEdgeUsingVision(out double height)
		{
			Vector3 p0 = Vector3.Zero;
			Vector3 p1 = Vector3.Zero;
			double x0, y0, z0, x1, y1, z1;

			//////Comentar esta línea si esta función se usa dentro de una máquina de estados
			//////////Para probar esto ya o es necesario porque se agregó el botón de Test Generic Function
			//this.status.IsExecutingPredefinedTask = true;
			height = 0;
			this.headPan = 0;
			this.headTilt = -1.1;
			if (!this.cmdMan.HEAD_lookat(this.headPan, this.headTilt, 7000))
				this.cmdMan.HEAD_lookat(this.headPan, this.headTilt, 7000);
			Thread.Sleep(3000);
			for (int i = 0; i < 3; i++)
			{
				if (!this.cmdMan.OBJ_FNDT_findedgereturns(this.headTilt, out x0, out y0, out z0, out x1, out y1, out z1, 5000))
				{
					TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot get table edge using vision tuned");

					if (!this.cmdMan.OBJ_FNDT_findedgereloaded(this.headTilt, out x0, out y0, out z0, 5000))
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot get table edge using vision reloaded");
						
						return false;
					}


					TextBoxStreamWriter.DefaultLog.WriteLine(string.Format("HAL9000.-> getting close... distance: {0}\t\t angle: {1}", Math.Sqrt(x0 * x0 + z0 * z0) - 0.15, Math.Atan(-x0 / z0)));
					this.cmdMan.MVN_PLN_move(Math.Sqrt(x0 * x0 + z0 * z0), Math.Atan(-x0 / z0), 10000);
					height = y0 ;
					return true;


					//////Comentar esta línea si esta función se usa dentro de una máquina de estados
					//this.status.IsExecutingPredefinedTask = false;

				}
				height = (y0 + y1) / 2;
				p0.X -= x0;
				p0.Y += y0;
				p0.Z += z0;
				p1.X -= x1;
				p1.Y += y1;
				p1.Z += z1;
			}
			
			p0.X /= 3;
			p0.Y /= 3;
			p0.Z /= 3;
			p1.X /= 3;
			p1.Y /= 3;
			p1.Z /= 3;

			//ESTA LÍNEA ES PARA CORREGIR EL POSIBLE ERROR DEBIDO A UN MAL CÁLCULO DE POSICIÓN DE LA CABEZA
			//HAY QUE QUITAR ESTA LINEA Y CORREGIR LA CABEZA
			//HAY QUE DECIRLE A iMe QUE CORRIJA LA CABEZA
			//this.headTilt *= 1.1;
			//p0 = this.TransHeadKinect2Robot(p0);
			//p1 = this.TransHeadKinect2Robot(p1);

			p0 = new Vector3(p0.Z, p0.X, p0.Y);
			p1 = new Vector3(p1.Z, p1.X, p1.Y);

			//Las coordenadas de la línea de tendencia son enviadas con respecto a la base del robot
			if (!this.cmdMan.MVN_PLN_getclose(p0.X, p0.Y, p0.Z, p1.X, p1.Y, p1.Z, 10000))
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot align with table edge");
				
				return false;
			}

			//////Comentar esta línea si esta función se usa dentro de una máquina de estados
			//this.status.IsExecutingPredefinedTask = false;
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Aliged with table succesfully usign vision module");
			
			return true;
		}

        public bool GetCloseWithCorrectioByVision(string location, int timeOut)
        {
            bool success = false;
            this.cmdMan.MVN_PLN_getclose("locationtest", 120000);

            while (!this.cmdMan.IsResponseReceived(JustinaCommands.MVN_PLN_getclose))
            {
            }

            return success;
        }

		public bool GetCloseToHumanFall(out bool notAHumanFall, int timeOut)
		{
			notAHumanFall = false;
			string locationToParse = "";
			int flag;
			double z = 0.0, x = 0.0, angle = 0.0, distance =0.0;

			if (lastPersonFallDetected.Count > 0)
				locationToParse = this.lastPersonFallDetected.Dequeue();
			else
				return false;

			try
			{
				char[] delimiters = { ' ' };
				string[] parts = locationToParse.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				int.TryParse(parts[0], out flag);
				if (flag == 1)
				{
					double.TryParse(parts[1], out x);
					double.TryParse(parts[2], out z);
				}
				else
				{
					//point at the direction of the fall
					notAHumanFall = true;
					return false;
				}
				
			}
			catch
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("CmdMan: Cannot parse response from locationToParse");
				return false;
			}

			distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2));
			//angle = Math.Asin(z / distance) - Math.PI/2;
			angle = Math.Atan2(-x,z);

			if (!cmdMan.MVN_PLN_move(distance-.1, angle, timeOut))
				if (!cmdMan.MVN_PLN_move(distance-.1, angle, timeOut))
					if (!cmdMan.MVN_PLN_move(distance-.1, angle, timeOut))
					{
						TextBoxStreamWriter.DefaultLog.WriteLine("MVN_PLN: cannot reach fall location");
						return false;
					}

			return true;
		}

		#endregion

		#region Transformations

		public Vector3 TransHeadKinect2Robot(Vector3 vector)
		{
			HomogeneousM robot2HeadkinectMatrix = new HomogeneousM(robot2neckMatrix().Matrix * neck2headKinectMatrix().Matrix);
			return robot2HeadkinectMatrix.Transform(vector);
		}

		public Vector3 TransHeadKinect2RightArm(Vector3 vector)
		{
			HomogeneousM ra2hk = new HomogeneousM(this.neck2RightArmMatrix().Inverse * this.neck2headKinectMatrix().Matrix);
			return ra2hk.Transform(vector);
		}

		public Vector3 TransHeadKinect2LeftArm(Vector3 vector)
		{
			HomogeneousM la2hk = new HomogeneousM(this.neck2LeftArmMatrix().Inverse * this.neck2headKinectMatrix().Matrix);
			return la2hk.Transform(vector);
		}

		public Vector3 TransRightArm2Robot(Vector3 vector)
		{
			HomogeneousM robot2ra = new HomogeneousM(this.robot2neckMatrix().Matrix * this.neck2RightArmMatrix().Matrix);
			return robot2ra.Transform(vector);
		}

		public Vector3 TransLeftArm2Robot(Vector3 vector)
		{
			HomogeneousM robot2la = new HomogeneousM(this.robot2neckMatrix().Matrix * this.neck2LeftArmMatrix().Matrix);
			return robot2la.Transform(vector);
		}

		private HomogeneousM robot2neckMatrix()
		{
			HomogeneousM robot2neck;

			HomogeneousM robot2torso;
			HomogeneousM torso2neck;

			double torsoElevation = 0.0;
			double torsoPan = 0.0;

			// robot to torso transf ( traslation to star)
			robot2torso = new HomogeneousM(0, 0.4165, -0.11085, 0);
			// torso to neck ( torso pan , torso elevation, traslation ) 
			torso2neck = new HomogeneousM(torsoPan, torsoElevation + .34195 + .04, .14125, 0);

			return robot2neck = new HomogeneousM(robot2torso.Matrix * torso2neck.Matrix);
		}

		private HomogeneousM neck2headKinectMatrix()
		{
			HomogeneousM neck2HeadKinect;

			HomogeneousM neckPan2Tilt;
			HomogeneousM tilt2xTraslation;
			HomogeneousM xTraslation2zTraslation;

			double headPan = this.headPan;
			double headTilt = this.headTilt;

			// Neck to Head Pan 
			neckPan2Tilt = new HomogeneousM(this.headPan, .25975, .0417, MathUtil.PiOver2);

			// Head Tilt to Traslation in X ROBOT axis 
			tilt2xTraslation = new HomogeneousM(this.headTilt, 0, 0.0321, -MathUtil.PiOver2);

			// X Traslation to Traslation in -Z ROBOT axis
			xTraslation2zTraslation = new HomogeneousM(-MathUtil.PiOver2, 0.0688, 0, -MathUtil.PiOver2);

			return neck2HeadKinect = new HomogeneousM(neckPan2Tilt.Matrix * tilt2xTraslation.Matrix * xTraslation2zTraslation.Matrix);
		}

		private HomogeneousM neck2RightArmMatrix()
		{
			HomogeneousM neck2armMatrix;

			HomogeneousM translationY;
			HomogeneousM translationX;
			HomogeneousM translationZ;

			translationY = new HomogeneousM(MathUtil.PiOver2, 0, -0.175, MathUtil.PiOver2);
			translationX = new HomogeneousM(-MathUtil.PiOver2, -0.027, 0, MathUtil.PiOver2);
			translationZ = new HomogeneousM(0, 0, 0.0974, 0);

			neck2armMatrix = new HomogeneousM(translationY.Matrix * translationX.Matrix * translationZ.Matrix);

			return neck2armMatrix;
		}

		private HomogeneousM neck2LeftArmMatrix()
		{
			HomogeneousM neck2leftArmMatrix;

			HomogeneousM translationY;
			HomogeneousM translationX;
			HomogeneousM translationZ;

			translationY = new HomogeneousM(MathUtil.PiOver2, 0, 0.175, MathUtil.PiOver2);
			translationX = new HomogeneousM(-MathUtil.PiOver2, -0.027, 0, MathUtil.PiOver2);
			translationZ = new HomogeneousM(0, 0, 0.0974, 0);

			neck2leftArmMatrix = new HomogeneousM(translationY.Matrix * translationX.Matrix * translationZ.Matrix);

			return neck2leftArmMatrix;
		}

		#endregion


		public bool BecomeAware()
		{
			this.status.IsExecutingPredefinedTask = false;
			if (this.isAware) return true;
			if (this.languageProcessor.Initialize())
			{
				this.status.KnowledgeLoadingStatus = "Loaded";
				this.status.KnownLocationLoaded = true;
				this.status.KnownRegionsLoaded = true;
			}

			if (!this.world.LoadRooms())
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Cannot load rooms");

			if (this.loadKnownObjects(this.knownObjectsFile)) this.status.KnownObjectsLoaded = true;
			if (this.loadValidNames(this.validNamesFile)) this.status.KnownPersonsLoaded = true;
			if (this.loadValidRooms(this.validRoomsFile)) this.status.KnownRoomsLoaded = true;
			//foreach (MapRoom mr in this.world.Rooms.Values)
			//	this.knownRooms.Add(mr.Name, mr.Name);
			//this.status.KnownRoomsLoaded = true;

			this.reasoningThread = new Thread(new ThreadStart(this.ReasoningThreadTask));
			this.reasoningThread.IsBackground = true;
			this.status.IsRunning = true;
			this.reasoningThread.Start();
			this.isAware = true;
			this.OnStatusChanged(new HAL9000StatusArgs(this.status));
			return true;
		}

		public bool SaveKnowledged()
		{
			bool success = true;

			List<string> temp = new List<string>();

			foreach (string s in this.knownRooms.Values)
				temp.Add(s);
			File.WriteAllLines(this.validRoomsFile, temp.ToArray());

			temp.Clear();
			foreach (string s in this.knownObjects.Keys)
				temp.Add(s);
			File.WriteAllLines(this.knownObjectsFile, temp.ToArray());

			temp.Clear();
			foreach (string s in this.knownPersons.Keys)
				temp.Add(s);
			File.WriteAllLines(this.validNamesFile, temp.ToArray());

			return success;
		}

		public void StopAllSystems()
		{
			this.cmdMan.Stop();
			this.cnnMan.Stop();

			this.world.SaveRooms();

			this.status.IsRunning = false;
			this.status.IsExecutingPredefinedTask = false;
			if (this.reasoningThread.IsAlive) this.reasoningThread.Join(200);
			if (this.reasoningThread.IsAlive) this.reasoningThread.Abort();
			if (this.predefinedTaskThread != null)
			{
				if (this.predefinedTaskThread.IsAlive) this.predefinedTaskThread.Join(200);
				if (this.predefinedTaskThread.IsAlive) this.predefinedTaskThread.Abort();
			}
		}
		
		public void OnStatusChanged(HAL9000StatusArgs args)
		{
			if (this.HAL9000StatusChangeg != null) this.HAL9000StatusChangeg(args);
		}

		public event HAL9000StatusChangedEventHandler HAL9000StatusChangeg;

		#region Properties
		/// <summary>
		/// Gets the queue of recognized sentences. When a recognized command is received, a sentence is enqueued
		/// </summary>
		public Queue<string> RecognizedSentences
		{
			get { return this.recognizedSentences; }
		}

		public SortedList<string, string> KnownRooms
		{
			get { return this.knownRooms; }
		}

		public SortedList<string, string> KnownRegions
		{
			get { return this.knownRegions; }
		}

		public SortedList<string, string> KnownLocations
		{
			get { return this.knownLocations; }
		}

		public SortedList<string, Person> KnownPersons
		{
			get { return this.knownPersons; }
		}

		public SortedList<string, PhysicalObject> KnownObjects
		{
			get { return this.knownObjects; }
		}

		public string CurrentRoom
		{
			get { return this.currentRoom; }
		}

		public string CurrentRegion
		{
			get { return this.currentRegion; }
		}

		public string CurrentLocation
		{
			get { return this.currentLocation; }
		}

		public bool TryOpenDoor { get; set; }

		public bool UseFloorLaser {
			get
			{
				return this.cmdMan.UseFloorLaser;
			}
			set
			{
				this.cmdMan.UseFloorLaser = value;
			}
		}

		public string DrinksRoom { get; set; }

		private bool useLocalization;
		public bool UseLocalization
		{
			get { return this.useLocalization; }
			set
			{
				this.useLocalization = value;
				this.cmdMan.UseLocalization = value;
			}
		}
		#endregion

    }
}

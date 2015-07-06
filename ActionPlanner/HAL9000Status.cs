using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner
{
	public class HAL9000Status
	{
		public HAL9000Status()
		{
			this.SharedVarsStatus = "Not Suscribed";
			this.ConnectionStatus = "Not Connected";
			this.KnowledgeLoadingStatus = "Not Loaded";
			this.TestBeingExecuted = "None";
			this.IsConnectedToBB = false;
			this.IsSuscribedToLocationVar = false;
			this.IsSuscribedToRegionVar = false;
			this.IsSuscribedToRoomVar = false;
			this.IsSuscribedToGestureVar = false;
            this.IsSuscribedToRecoSpeechVar = false;
			this.IsSuscribedToNearObjForLoc = false;
			this.IsSuscribedToInGoodRegion = false;
			this.KnownLocationLoaded = false;
			this.KnownObjectsLoaded = false;
			this.KnownPersonsLoaded = false;
			this.KnownRegionsLoaded = false;
			this.KnownRoomsLoaded = false;
			this.IsRunning = false;
            this.IsPaused = false;
			this.IsExecutingPredefinedTask = false;
		}

		public string SharedVarsStatus { get; set; }
		public string ConnectionStatus { get; set; }
		public string KnowledgeLoadingStatus { get; set; }
		public string TestBeingExecuted { get; set; }
		public bool IsConnectedToBB { get; set; }
		public bool IsSuscribedToRoomVar { get; set; }
		public bool IsSuscribedToRegionVar { get; set; }
		public bool IsSuscribedToLocationVar { get; set; }
		public bool IsSuscribedToGestureVar { get; set; }
        public bool IsSuscribedToRecoSpeechVar { get; set; }
		public bool IsSuscribedToSkeletons { get; set; }
		public bool IsSuscribedToNearObjForLoc { get; set; }
		public bool IsSuscribedToInGoodRegion { get; set; }
		public bool KnownRoomsLoaded { get; set; }
		public bool KnownLocationLoaded { get; set; }
		public bool KnownRegionsLoaded { get; set; }
		public bool KnownPersonsLoaded { get; set; }
		public bool KnownObjectsLoaded { get; set; }
		public BrainWaveType BrainWaveType { get; set; }

        public bool IsPaused { get; set; }
		public bool IsRunning { get; set; }
		public bool IsExecutingPredefinedTask { get; set; }

		public bool IsSystemReady
		{
			get
			{
				return this.IsConnectedToBB && this.AreSharedVarsReady && this.KnownLocationLoaded && 
				this.KnownObjectsLoaded && this.KnownPersonsLoaded && this.KnownRegionsLoaded && this.KnownRoomsLoaded;
			}
		}
		public bool AreSharedVarsReady
		{
			get
			{
				return this.IsSuscribedToLocationVar && this.IsSuscribedToRegionVar &&
				this.IsSuscribedToRoomVar && this.IsSuscribedToGestureVar && this.IsSuscribedToRecoSpeechVar;
			}
		}
		public string GeneralStatus
		{
			get
			{
				if (this.IsSystemReady)
					return "SYSTEM READY";
				else return "SYSTEM NOT READY";
			}
		}

		public bool IsKnowledgeReady
		{
			get
			{
				return this.KnownLocationLoaded && this.KnownObjectsLoaded && this.KnownPersonsLoaded &&
					this.KnownRegionsLoaded && this.KnownRoomsLoaded;
			}
		}
	}
}

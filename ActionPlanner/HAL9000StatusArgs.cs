using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActionPlanner
{
	public class HAL9000StatusArgs
	{
		bool isSystemReady;
		string generalStatus;
		string testBeingExecuted;
        bool isPaused;

		public HAL9000StatusArgs(HAL9000Status status)
		{
			this.isSystemReady = status.IsSystemReady;
			this.generalStatus = status.GeneralStatus;
			this.testBeingExecuted = status.TestBeingExecuted;
            this.isPaused = status.IsPaused;
		}

		public bool IsSystemReady { get { return this.isSystemReady; } }
		public string GeneralStatus { get { return this.generalStatus; } }
		public string TestBeingExecuted { get { return this.testBeingExecuted; } }
        public bool IsPaused { get { return this.isPaused; } }
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Robotics.Controls;

namespace ActionPlanner
{
	public enum StageToPerform { StageI, StageII, Finals }

	public partial class frmActionPlanner : Form
	{
		private StageToPerform stageToPerform;
		private HAL9000Brain hal9000Brain;
		private HAL9000StatusChangedEventHandler dlgStatusChangedEH;

        public frmActionPlanner()
        {
            InitializeComponent();
            if (!Directory.Exists(".\\" + DateTime.Today.ToString("yyyy-MM-dd")))
                Directory.CreateDirectory(".\\" + DateTime.Today.ToString("yyyy-MM-dd"));
            TextBoxStreamWriter.DefaultLog = new TextBoxStreamWriter(txtConsole, ".\\" + DateTime.Today.ToString("yyyy-MM-dd") + "\\" +
                 DateTime.Now.ToString("HH.mm.ss") + ".txt", 300);

            TextBoxStreamWriter.DefaultLog.AppendDate = false;
            TextBoxStreamWriter.DefaultLog.DefaultPriority = 1;
            TextBoxStreamWriter.DefaultLog.TextBoxVerbosityThreshold = 1;
            this.hal9000Brain = new HAL9000Brain();
		}

		#region Form Event Handlers

		private void frmActionPlanner_Load(object sender, EventArgs e)
		{
			this.Text = "HAL9000    Non-Mechatronic Version";
			this.dlgStatusChangedEH = new HAL9000StatusChangedEventHandler(this.hal9000Brain_HAL9000StatusChangeg);
			this.hal9000Brain.HAL9000StatusChangeg += this.dlgStatusChangedEH;
			this.hal9000Brain.BecomeAware();
            this.propertyGrid1.SelectedObject = this.hal9000Brain;
		}

		private void hal9000Brain_HAL9000StatusChangeg(HAL9000StatusArgs args)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(this.dlgStatusChangedEH, args);
				return;
			}
			if (args.IsSystemReady) this.lblGeneralStatus.ForeColor = Color.Black;
			else this.lblGeneralStatus.ForeColor = Color.Red;
            if (!args.IsPaused) this.lblGeneralStatus.Text = args.GeneralStatus;
            else this.lblGeneralStatus.Text = "SYSTEM PAUSED";
			this.lblExecuting.Text = "Executing: " + args.TestBeingExecuted + "   |";
            if (args.TestBeingExecuted.ToLower().Contains("none")) this.btnPresentation.Enabled = true;
		}

		private void frmActionPlanner_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.hal9000Brain.StopAllSystems();
		}

		private void rbStage1_CheckedChanged(object sender, EventArgs e)
		{
			this.SetStageSettings();
		}

		private void rbStage2_CheckedChanged(object sender, EventArgs e)
		{
			this.SetStageSettings();
		}

		private void rbStageFinals_CheckedChanged(object sender, EventArgs e)
		{
			this.SetStageSettings();
		}

		private void SetStageSettings()
		{
			if (rbStage1.Checked)
            {
                btnTest_1.Enabled = true;
                btnTest_2.Enabled = true;
                btnTest_3.Enabled = true;
                btnTest_4.Enabled = true;
                btnTest_5.Enabled = true;
                btnTest_6.Enabled = true;

                btnTest_1.Text = "GPSR";
                btnTest_2.Text = "Manipulation and Object Recognition";
                btnTest_3.Text = "Navigation Test";
                btnTest_4.Text = "Person Recognition Test";
                btnTest_5.Text = "RoboZoo";
                btnTest_6.Text = "Speech Recognition && Audio Detection";

				stageToPerform = StageToPerform.StageI;
			}
			else if (rbStage2.Checked)
            {
                btnTest_1.Enabled = true;
                btnTest_2.Enabled = true;
                btnTest_3.Enabled = true;
                btnTest_4.Enabled = true;
                btnTest_5.Enabled = false;
                btnTest_6.Enabled = false;

                btnTest_1.Text = "Open Challenge";
                btnTest_2.Text = "Restaurant";
                btnTest_3.Text = "Robo-Nurse";
                btnTest_4.Text = "Wake Me Up Test";

				stageToPerform = StageToPerform.StageII;
			}
			else if (rbStageFinals.Checked)
            {
                btnTest_1.Enabled = false;
                btnTest_2.Enabled = false;
                btnTest_3.Enabled = false;
                btnTest_4.Enabled = false;
                btnTest_5.Enabled = false;
                btnTest_6.Enabled = true;

				btnTest_6.Text = "¡¡¡ FINAL TEST !!!!";

				stageToPerform = StageToPerform.Finals;
			}
		}

		private void btnPresentation_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;

            hal9000Brain.StartSM(test);
		}

		#endregion

        private void chkTryOpenDoor_CheckedChanged(object sender, EventArgs e)
        {
            this.hal9000Brain.TryOpenDoor = this.chkTryOpenDoor.Checked;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            this.hal9000Brain.PauseRobocupTest();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.hal9000Brain.StopRobocupTest();
        }
        
		private void chkUseLocalization_CheckedChanged(object sender, EventArgs e)
		{
			this.hal9000Brain.UseLocalization = this.chkUseLocalization.Checked;
		}

        private void btnTest_1_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;
            if (this.stageToPerform == StageToPerform.StageI)
                test = TestToPerform.GPSR;
            else if (this.stageToPerform == StageToPerform.StageII)
                test = TestToPerform.OpenChallenge;

            hal9000Brain.StartSM(test);
        }
        private void btnTest_2_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;
            if (this.stageToPerform == StageToPerform.StageI)
                test = TestToPerform.Manipulation;
            else if (this.stageToPerform == StageToPerform.StageII)
                test = TestToPerform.Restaurant;

            hal9000Brain.StartSM(test);
        }

        private void btnTest_3_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;
            if (this.stageToPerform == StageToPerform.StageI)
                test = TestToPerform.Navigation;
            else if (this.stageToPerform == StageToPerform.StageII)
                test = TestToPerform.RoboNurse;

            hal9000Brain.StartSM(test);
        }

        private void btnTest_4_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;
            if (this.stageToPerform == StageToPerform.StageI)
                test = TestToPerform.PersonRecognition;
            else if (this.stageToPerform == StageToPerform.StageII)
                test = TestToPerform.WakeMeUp;

            hal9000Brain.StartSM(test);
        }

        private void btnTest_5_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;
            if (this.stageToPerform == StageToPerform.StageI)
                test = TestToPerform.RoboZoo;
            else if (this.stageToPerform == StageToPerform.StageII)
                test = TestToPerform.WakeMeUp;

            hal9000Brain.StartSM(test);
        }

        private void btnTest_6_Click(object sender, EventArgs e)
        {
            TestToPerform test = TestToPerform.DefaultTest;
            if (this.stageToPerform == StageToPerform.StageI)
                test = TestToPerform.AudioTest;
            else if (this.stageToPerform == StageToPerform.StageII)
                test = TestToPerform.AudioTest;

            hal9000Brain.StartSM(test);
        }
	}
}

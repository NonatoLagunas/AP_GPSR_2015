namespace ActionPlanner
{
	partial class frmActionPlanner
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmActionPlanner));
            this.btnTest_1 = new System.Windows.Forms.Button();
            this.btnTest_2 = new System.Windows.Forms.Button();
            this.btnTest_3 = new System.Windows.Forms.Button();
            this.btnTest_4 = new System.Windows.Forms.Button();
            this.btnTest_5 = new System.Windows.Forms.Button();
            this.btnTest_6 = new System.Windows.Forms.Button();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.lblExecuting = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblGeneralStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gbStage = new System.Windows.Forms.GroupBox();
            this.rbStage2 = new System.Windows.Forms.RadioButton();
            this.rbStageFinals = new System.Windows.Forms.RadioButton();
            this.rbStage1 = new System.Windows.Forms.RadioButton();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPresentation = new System.Windows.Forms.Button();
            this.chkTryOpenDoor = new System.Windows.Forms.CheckBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.btnPause = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.General = new System.Windows.Forms.TabPage();
            this.testButtonsTab = new System.Windows.Forms.TabPage();
            this.chkUseLocalization = new System.Windows.Forms.CheckBox();
            this.mainStatusStrip.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.gbStage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.General.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnTest_1
            // 
            this.btnTest_1.Location = new System.Drawing.Point(11, 44);
            this.btnTest_1.Name = "btnTest_1";
            this.btnTest_1.Size = new System.Drawing.Size(113, 40);
            this.btnTest_1.TabIndex = 0;
            this.btnTest_1.Text = "GPSR";
            this.btnTest_1.UseVisualStyleBackColor = true;
            this.btnTest_1.Click += new System.EventHandler(this.btnTest_1_Click);
            // 
            // btnTest_2
            // 
            this.btnTest_2.Location = new System.Drawing.Point(130, 44);
            this.btnTest_2.Name = "btnTest_2";
            this.btnTest_2.Size = new System.Drawing.Size(113, 40);
            this.btnTest_2.TabIndex = 0;
            this.btnTest_2.Text = "Manipulation and Object Recognition";
            this.btnTest_2.UseVisualStyleBackColor = true;
            this.btnTest_2.Click += new System.EventHandler(this.btnTest_2_Click);
            // 
            // btnTest_3
            // 
            this.btnTest_3.Location = new System.Drawing.Point(249, 44);
            this.btnTest_3.Name = "btnTest_3";
            this.btnTest_3.Size = new System.Drawing.Size(113, 40);
            this.btnTest_3.TabIndex = 0;
            this.btnTest_3.Text = "Navigation Test";
            this.btnTest_3.UseVisualStyleBackColor = true;
            this.btnTest_3.Click += new System.EventHandler(this.btnTest_3_Click);
            // 
            // btnTest_4
            // 
            this.btnTest_4.Location = new System.Drawing.Point(11, 90);
            this.btnTest_4.Name = "btnTest_4";
            this.btnTest_4.Size = new System.Drawing.Size(113, 40);
            this.btnTest_4.TabIndex = 0;
            this.btnTest_4.Text = "Person Recognition Test";
            this.btnTest_4.UseVisualStyleBackColor = true;
            this.btnTest_4.Click += new System.EventHandler(this.btnTest_4_Click);
            // 
            // btnTest_5
            // 
            this.btnTest_5.Location = new System.Drawing.Point(130, 90);
            this.btnTest_5.Name = "btnTest_5";
            this.btnTest_5.Size = new System.Drawing.Size(113, 40);
            this.btnTest_5.TabIndex = 0;
            this.btnTest_5.Text = "RoboZoo";
            this.btnTest_5.UseVisualStyleBackColor = true;
            this.btnTest_5.Click += new System.EventHandler(this.btnTest_5_Click);
            // 
            // btnTest_6
            // 
            this.btnTest_6.Location = new System.Drawing.Point(249, 90);
            this.btnTest_6.Name = "btnTest_6";
            this.btnTest_6.Size = new System.Drawing.Size(113, 40);
            this.btnTest_6.TabIndex = 0;
            this.btnTest_6.Text = "Speech Recognition && Audio Detection";
            this.btnTest_6.UseVisualStyleBackColor = true;
            this.btnTest_6.Click += new System.EventHandler(this.btnTest_6_Click);
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtConsole.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsole.ForeColor = System.Drawing.Color.LimeGreen;
            this.txtConsole.Location = new System.Drawing.Point(3, 137);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtConsole.Size = new System.Drawing.Size(574, 252);
            this.txtConsole.TabIndex = 0;
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblExecuting,
            this.lblGeneralStatus});
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 480);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Size = new System.Drawing.Size(801, 22);
            this.mainStatusStrip.TabIndex = 2;
            this.mainStatusStrip.Text = "statusStrip1";
            // 
            // lblExecuting
            // 
            this.lblExecuting.Name = "lblExecuting";
            this.lblExecuting.Size = new System.Drawing.Size(697, 17);
            this.lblExecuting.Spring = true;
            this.lblExecuting.Text = "Executing: None   |";
            this.lblExecuting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblGeneralStatus
            // 
            this.lblGeneralStatus.Name = "lblGeneralStatus";
            this.lblGeneralStatus.Size = new System.Drawing.Size(89, 17);
            this.lblGeneralStatus.Text = "SYSTEM READY";
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(801, 24);
            this.mainMenuStrip.TabIndex = 3;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // gbStage
            // 
            this.gbStage.Controls.Add(this.rbStage2);
            this.gbStage.Controls.Add(this.rbStageFinals);
            this.gbStage.Controls.Add(this.rbStage1);
            this.gbStage.Location = new System.Drawing.Point(371, 37);
            this.gbStage.Name = "gbStage";
            this.gbStage.Size = new System.Drawing.Size(80, 93);
            this.gbStage.TabIndex = 5;
            this.gbStage.TabStop = false;
            this.gbStage.Text = "Stage";
            // 
            // rbStage2
            // 
            this.rbStage2.AutoSize = true;
            this.rbStage2.Location = new System.Drawing.Point(6, 45);
            this.rbStage2.Name = "rbStage2";
            this.rbStage2.Size = new System.Drawing.Size(62, 17);
            this.rbStage2.TabIndex = 0;
            this.rbStage2.TabStop = true;
            this.rbStage2.Text = "Stage 2";
            this.rbStage2.UseVisualStyleBackColor = true;
            this.rbStage2.CheckedChanged += new System.EventHandler(this.rbStage2_CheckedChanged);
            // 
            // rbStageFinals
            // 
            this.rbStageFinals.AutoSize = true;
            this.rbStageFinals.Location = new System.Drawing.Point(6, 70);
            this.rbStageFinals.Name = "rbStageFinals";
            this.rbStageFinals.Size = new System.Drawing.Size(52, 17);
            this.rbStageFinals.TabIndex = 0;
            this.rbStageFinals.TabStop = true;
            this.rbStageFinals.Text = "Finals";
            this.rbStageFinals.UseVisualStyleBackColor = true;
            this.rbStageFinals.CheckedChanged += new System.EventHandler(this.rbStageFinals_CheckedChanged);
            // 
            // rbStage1
            // 
            this.rbStage1.AutoSize = true;
            this.rbStage1.Location = new System.Drawing.Point(6, 19);
            this.rbStage1.Name = "rbStage1";
            this.rbStage1.Size = new System.Drawing.Size(62, 17);
            this.rbStage1.TabIndex = 0;
            this.rbStage1.TabStop = true;
            this.rbStage1.Text = "Stage 1";
            this.rbStage1.UseVisualStyleBackColor = true;
            this.rbStage1.CheckedChanged += new System.EventHandler(this.rbStage1_CheckedChanged);
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.Red;
            this.btnStop.Location = new System.Drawing.Point(457, 44);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 46);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "S T O P";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPresentation
            // 
            this.btnPresentation.Location = new System.Drawing.Point(457, 96);
            this.btnPresentation.Name = "btnPresentation";
            this.btnPresentation.Size = new System.Drawing.Size(120, 29);
            this.btnPresentation.TabIndex = 7;
            this.btnPresentation.Text = "Presentation";
            this.btnPresentation.UseVisualStyleBackColor = true;
            this.btnPresentation.Click += new System.EventHandler(this.btnPresentation_Click);
            // 
            // chkTryOpenDoor
            // 
            this.chkTryOpenDoor.AutoSize = true;
            this.chkTryOpenDoor.Location = new System.Drawing.Point(582, 11);
            this.chkTryOpenDoor.Name = "chkTryOpenDoor";
            this.chkTryOpenDoor.Size = new System.Drawing.Size(96, 17);
            this.chkTryOpenDoor.TabIndex = 8;
            this.chkTryOpenDoor.Text = "Try Open Door";
            this.chkTryOpenDoor.UseVisualStyleBackColor = true;
            this.chkTryOpenDoor.CheckedChanged += new System.EventHandler(this.chkTryOpenDoor_CheckedChanged);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Location = new System.Drawing.Point(583, 44);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(157, 345);
            this.propertyGrid1.TabIndex = 9;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // btnPause
            // 
            this.btnPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPause.Location = new System.Drawing.Point(457, 3);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(119, 35);
            this.btnPause.TabIndex = 10;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnPause);
            this.panel1.Controls.Add(this.btnTest_1);
            this.panel1.Controls.Add(this.btnTest_4);
            this.panel1.Controls.Add(this.propertyGrid1);
            this.panel1.Controls.Add(this.btnTest_2);
            this.panel1.Controls.Add(this.chkTryOpenDoor);
            this.panel1.Controls.Add(this.btnTest_5);
            this.panel1.Controls.Add(this.btnPresentation);
            this.panel1.Controls.Add(this.btnTest_3);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnTest_6);
            this.panel1.Controls.Add(this.gbStage);
            this.panel1.Controls.Add(this.txtConsole);
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(749, 399);
            this.panel1.TabIndex = 12;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.General);
            this.tabControl1.Controls.Add(this.testButtonsTab);
            this.tabControl1.Location = new System.Drawing.Point(12, 29);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(775, 441);
            this.tabControl1.TabIndex = 13;
            // 
            // General
            // 
            this.General.Controls.Add(this.panel1);
            this.General.Location = new System.Drawing.Point(4, 22);
            this.General.Name = "General";
            this.General.Padding = new System.Windows.Forms.Padding(3);
            this.General.Size = new System.Drawing.Size(767, 415);
            this.General.TabIndex = 0;
            this.General.Text = "General";
            this.General.UseVisualStyleBackColor = true;
            // 
            // testButtonsTab
            // 
            this.testButtonsTab.Location = new System.Drawing.Point(4, 22);
            this.testButtonsTab.Name = "testButtonsTab";
            this.testButtonsTab.Size = new System.Drawing.Size(767, 415);
            this.testButtonsTab.TabIndex = 2;
            this.testButtonsTab.Text = "Test Buttons";
            this.testButtonsTab.UseVisualStyleBackColor = true;
            // 
            // chkUseLocalization
            // 
            this.chkUseLocalization.AutoSize = true;
            this.chkUseLocalization.Location = new System.Drawing.Point(620, 7);
            this.chkUseLocalization.Name = "chkUseLocalization";
            this.chkUseLocalization.Size = new System.Drawing.Size(104, 17);
            this.chkUseLocalization.TabIndex = 14;
            this.chkUseLocalization.Text = "Use Localization";
            this.chkUseLocalization.UseVisualStyleBackColor = true;
            this.chkUseLocalization.CheckedChanged += new System.EventHandler(this.chkUseLocalization_CheckedChanged);
            // 
            // frmActionPlanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 502);
            this.Controls.Add(this.chkUseLocalization);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "frmActionPlanner";
            this.Text = "HAL9000 - Marcosoft";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmActionPlanner_FormClosing);
            this.Load += new System.EventHandler(this.frmActionPlanner_Load);
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.gbStage.ResumeLayout(false);
            this.gbStage.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.General.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnTest_1;
		private System.Windows.Forms.Button btnTest_2;
		private System.Windows.Forms.Button btnTest_3;
		private System.Windows.Forms.Button btnTest_4;
		private System.Windows.Forms.Button btnTest_5;
		private System.Windows.Forms.Button btnTest_6;
		private System.Windows.Forms.TextBox txtConsole;
		private System.Windows.Forms.StatusStrip mainStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel lblGeneralStatus;
		private System.Windows.Forms.ToolStripStatusLabel lblExecuting;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.GroupBox gbStage;
		private System.Windows.Forms.RadioButton rbStage1;
		private System.Windows.Forms.RadioButton rbStage2;
		private System.Windows.Forms.RadioButton rbStageFinals;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnPresentation;
        private System.Windows.Forms.CheckBox chkTryOpenDoor;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Button btnPause;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage General;
        private System.Windows.Forms.TabPage testButtonsTab;
        private System.Windows.Forms.CheckBox chkUseLocalization;
	}
}


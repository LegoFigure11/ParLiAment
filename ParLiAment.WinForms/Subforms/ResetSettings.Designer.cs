namespace ParLiAment.WinForms.Subforms
{
    partial class ResetSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetSettings));
            L_ExtraTimeReturnHome = new Label();
            TB_ExtraTimeReturnHome = new TextBox();
            TB_ExtraTimeCloseGame = new TextBox();
            L_ExtraTimeCloseGame = new Label();
            TB_ExtraTimeLoadProfile = new TextBox();
            L_ExtraTimeLoadProfile = new Label();
            TB_ExtraTimeLoadGame = new TextBox();
            L_ExtraTimeLoadGame = new Label();
            CB_AvoidUpdate = new ComboBox();
            CB_ScreenOff = new ComboBox();
            L_AvoidUpdate = new Label();
            label1 = new Label();
            SuspendLayout();
            // 
            // L_ExtraTimeReturnHome
            // 
            L_ExtraTimeReturnHome.AutoSize = true;
            L_ExtraTimeReturnHome.Location = new Point(12, 15);
            L_ExtraTimeReturnHome.Name = "L_ExtraTimeReturnHome";
            L_ExtraTimeReturnHome.Size = new Size(156, 15);
            L_ExtraTimeReturnHome.TabIndex = 0;
            L_ExtraTimeReturnHome.Text = "Extra Time to  Return Home:";
            // 
            // TB_ExtraTimeReturnHome
            // 
            TB_ExtraTimeReturnHome.Location = new Point(201, 12);
            TB_ExtraTimeReturnHome.Name = "TB_ExtraTimeReturnHome";
            TB_ExtraTimeReturnHome.Size = new Size(100, 23);
            TB_ExtraTimeReturnHome.TabIndex = 1;
            TB_ExtraTimeReturnHome.TextChanged += TB_ExtraTimeReturnHome_TextChanged;
            // 
            // TB_ExtraTimeCloseGame
            // 
            TB_ExtraTimeCloseGame.Location = new Point(201, 37);
            TB_ExtraTimeCloseGame.Name = "TB_ExtraTimeCloseGame";
            TB_ExtraTimeCloseGame.Size = new Size(100, 23);
            TB_ExtraTimeCloseGame.TabIndex = 3;
            TB_ExtraTimeCloseGame.TextChanged += TB_ExtraTimeCloseGame_TextChanged;
            // 
            // L_ExtraTimeCloseGame
            // 
            L_ExtraTimeCloseGame.AutoSize = true;
            L_ExtraTimeCloseGame.Location = new Point(12, 40);
            L_ExtraTimeCloseGame.Name = "L_ExtraTimeCloseGame";
            L_ExtraTimeCloseGame.Size = new Size(145, 15);
            L_ExtraTimeCloseGame.TabIndex = 2;
            L_ExtraTimeCloseGame.Text = "Extra Time to Close Game:";
            // 
            // TB_ExtraTimeLoadProfile
            // 
            TB_ExtraTimeLoadProfile.Location = new Point(201, 62);
            TB_ExtraTimeLoadProfile.Name = "TB_ExtraTimeLoadProfile";
            TB_ExtraTimeLoadProfile.Size = new Size(100, 23);
            TB_ExtraTimeLoadProfile.TabIndex = 5;
            TB_ExtraTimeLoadProfile.TextChanged += TB_ExtraTimeLoadProfile_TextChanged;
            // 
            // L_ExtraTimeLoadProfile
            // 
            L_ExtraTimeLoadProfile.AutoSize = true;
            L_ExtraTimeLoadProfile.Location = new Point(12, 65);
            L_ExtraTimeLoadProfile.Name = "L_ExtraTimeLoadProfile";
            L_ExtraTimeLoadProfile.Size = new Size(145, 15);
            L_ExtraTimeLoadProfile.TabIndex = 4;
            L_ExtraTimeLoadProfile.Text = "Extra Time to Load Profile:";
            // 
            // TB_ExtraTimeLoadGame
            // 
            TB_ExtraTimeLoadGame.Location = new Point(201, 87);
            TB_ExtraTimeLoadGame.Name = "TB_ExtraTimeLoadGame";
            TB_ExtraTimeLoadGame.Size = new Size(100, 23);
            TB_ExtraTimeLoadGame.TabIndex = 7;
            TB_ExtraTimeLoadGame.TextChanged += TB_ExtraTimeLoadGame_TextChanged;
            // 
            // L_ExtraTimeLoadGame
            // 
            L_ExtraTimeLoadGame.AutoSize = true;
            L_ExtraTimeLoadGame.Location = new Point(12, 90);
            L_ExtraTimeLoadGame.Name = "L_ExtraTimeLoadGame";
            L_ExtraTimeLoadGame.Size = new Size(142, 15);
            L_ExtraTimeLoadGame.TabIndex = 6;
            L_ExtraTimeLoadGame.Text = "Extra Time to Load Game:";
            // 
            // CB_AvoidUpdate
            // 
            CB_AvoidUpdate.FormattingEnabled = true;
            CB_AvoidUpdate.Items.AddRange(new object[] { "True", "False" });
            CB_AvoidUpdate.Location = new Point(201, 112);
            CB_AvoidUpdate.Name = "CB_AvoidUpdate";
            CB_AvoidUpdate.Size = new Size(100, 23);
            CB_AvoidUpdate.TabIndex = 169;
            CB_AvoidUpdate.SelectedIndexChanged += CB_AvoidUpdate_SelectedIndexChanged;
            // 
            // CB_ScreenOff
            // 
            CB_ScreenOff.FormattingEnabled = true;
            CB_ScreenOff.Items.AddRange(new object[] { "True", "False" });
            CB_ScreenOff.Location = new Point(201, 137);
            CB_ScreenOff.Name = "CB_ScreenOff";
            CB_ScreenOff.Size = new Size(100, 23);
            CB_ScreenOff.TabIndex = 170;
            CB_ScreenOff.SelectedIndexChanged += CB_ScreenOff_SelectedIndexChanged;
            // 
            // L_AvoidUpdate
            // 
            L_AvoidUpdate.AutoSize = true;
            L_AvoidUpdate.Location = new Point(12, 115);
            L_AvoidUpdate.Name = "L_AvoidUpdate";
            L_AvoidUpdate.Size = new Size(123, 15);
            L_AvoidUpdate.TabIndex = 171;
            L_AvoidUpdate.Text = "Avoid System Update:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 140);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 172;
            label1.Text = "Turn off Screen:";
            // 
            // ResetSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(313, 170);
            Controls.Add(label1);
            Controls.Add(L_AvoidUpdate);
            Controls.Add(CB_ScreenOff);
            Controls.Add(CB_AvoidUpdate);
            Controls.Add(TB_ExtraTimeLoadGame);
            Controls.Add(L_ExtraTimeLoadGame);
            Controls.Add(TB_ExtraTimeLoadProfile);
            Controls.Add(L_ExtraTimeLoadProfile);
            Controls.Add(TB_ExtraTimeCloseGame);
            Controls.Add(L_ExtraTimeCloseGame);
            Controls.Add(TB_ExtraTimeReturnHome);
            Controls.Add(L_ExtraTimeReturnHome);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ResetSettings";
            Text = "ResetSettings";
            FormClosing += ResetSettings_FormClosing;
            Load += ResetSettings_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label L_ExtraTimeReturnHome;
        private TextBox TB_ExtraTimeReturnHome;
        private TextBox TB_ExtraTimeCloseGame;
        private Label L_ExtraTimeCloseGame;
        private TextBox TB_ExtraTimeLoadProfile;
        private Label L_ExtraTimeLoadProfile;
        private TextBox TB_ExtraTimeLoadGame;
        private Label L_ExtraTimeLoadGame;
        private ComboBox CB_AvoidUpdate;
        private ComboBox CB_ScreenOff;
        private Label L_AvoidUpdate;
        private Label label1;
    }
}

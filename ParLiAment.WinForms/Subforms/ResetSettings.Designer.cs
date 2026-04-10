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
            L_OT = new Label();
            TB_OT = new TextBox();
            TB_ButtonPressDelay = new TextBox();
            L_ButtonPressDelay = new Label();
            TB_PageChangeDelay = new TextBox();
            L_PageChangeDelay = new Label();
            TB_NameRejectDelay = new TextBox();
            L_NameRejectDelay = new Label();
            TB_ReloadNameDelay = new TextBox();
            L_LoadNameDelay = new Label();
            SuspendLayout();
            // 
            // L_OT
            // 
            L_OT.AutoSize = true;
            L_OT.Location = new Point(12, 15);
            L_OT.Name = "L_OT";
            L_OT.Size = new Size(59, 15);
            L_OT.TabIndex = 0;
            L_OT.Text = "OT Name:";
            // 
            // TB_OT
            // 
            TB_OT.Location = new Point(201, 12);
            TB_OT.Name = "TB_OT";
            TB_OT.Size = new Size(100, 23);
            TB_OT.TabIndex = 1;
            TB_OT.TextChanged += TB_OT_TextChanged;
            // 
            // TB_ButtonPressDelay
            // 
            TB_ButtonPressDelay.Location = new Point(201, 37);
            TB_ButtonPressDelay.Name = "TB_ButtonPressDelay";
            TB_ButtonPressDelay.Size = new Size(100, 23);
            TB_ButtonPressDelay.TabIndex = 3;
            TB_ButtonPressDelay.TextChanged += TB_ButtonPressDelay_TextChanged;
            // 
            // L_ButtonPressDelay
            // 
            L_ButtonPressDelay.AutoSize = true;
            L_ButtonPressDelay.Location = new Point(12, 40);
            L_ButtonPressDelay.Name = "L_ButtonPressDelay";
            L_ButtonPressDelay.Size = new Size(143, 15);
            L_ButtonPressDelay.TabIndex = 2;
            L_ButtonPressDelay.Text = "Name Screen Input Delay:";
            // 
            // TB_PageChangeDelay
            // 
            TB_PageChangeDelay.Location = new Point(201, 62);
            TB_PageChangeDelay.Name = "TB_PageChangeDelay";
            TB_PageChangeDelay.Size = new Size(100, 23);
            TB_PageChangeDelay.TabIndex = 5;
            TB_PageChangeDelay.TextChanged += TB_PageChangeDelay_TextChanged;
            // 
            // L_PageChangeDelay
            // 
            L_PageChangeDelay.AutoSize = true;
            L_PageChangeDelay.Location = new Point(12, 65);
            L_PageChangeDelay.Name = "L_PageChangeDelay";
            L_PageChangeDelay.Size = new Size(165, 15);
            L_PageChangeDelay.TabIndex = 4;
            L_PageChangeDelay.Text = "Keyboard Page Change Delay:";
            // 
            // TB_NameRejectDelay
            // 
            TB_NameRejectDelay.Location = new Point(201, 87);
            TB_NameRejectDelay.Name = "TB_NameRejectDelay";
            TB_NameRejectDelay.Size = new Size(100, 23);
            TB_NameRejectDelay.TabIndex = 7;
            TB_NameRejectDelay.TextChanged += TB_NameRejectDelay_TextChanged;
            // 
            // L_NameRejectDelay
            // 
            L_NameRejectDelay.AutoSize = true;
            L_NameRejectDelay.Location = new Point(12, 90);
            L_NameRejectDelay.Name = "L_NameRejectDelay";
            L_NameRejectDelay.Size = new Size(126, 15);
            L_NameRejectDelay.TabIndex = 6;
            L_NameRejectDelay.Text = "Name Rejection Delay:";
            // 
            // TB_ReloadNameDelay
            // 
            TB_ReloadNameDelay.Location = new Point(201, 112);
            TB_ReloadNameDelay.Name = "TB_ReloadNameDelay";
            TB_ReloadNameDelay.Size = new Size(100, 23);
            TB_ReloadNameDelay.TabIndex = 9;
            TB_ReloadNameDelay.TextChanged += TB_LoadNameDelay_TextChanged;
            // 
            // L_LoadNameDelay
            // 
            L_LoadNameDelay.AutoSize = true;
            L_LoadNameDelay.Location = new Point(12, 115);
            L_LoadNameDelay.Name = "L_LoadNameDelay";
            L_LoadNameDelay.Size = new Size(141, 15);
            L_LoadNameDelay.TabIndex = 8;
            L_LoadNameDelay.Text = "Load Name Screen Delay:";
            // 
            // ResetSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(313, 142);
            Controls.Add(TB_ReloadNameDelay);
            Controls.Add(L_LoadNameDelay);
            Controls.Add(TB_NameRejectDelay);
            Controls.Add(L_NameRejectDelay);
            Controls.Add(TB_PageChangeDelay);
            Controls.Add(L_PageChangeDelay);
            Controls.Add(TB_ButtonPressDelay);
            Controls.Add(L_ButtonPressDelay);
            Controls.Add(TB_OT);
            Controls.Add(L_OT);
            Name = "ResetSettings";
            Text = "ResetSettings";
            FormClosing += ResetSettings_FormClosing;
            Load += ResetSettings_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label L_OT;
        private TextBox TB_OT;
        private TextBox TB_ButtonPressDelay;
        private Label L_ButtonPressDelay;
        private TextBox TB_PageChangeDelay;
        private Label L_PageChangeDelay;
        private TextBox TB_NameRejectDelay;
        private Label L_NameRejectDelay;
        private TextBox TB_ReloadNameDelay;
        private Label L_LoadNameDelay;
    }
}
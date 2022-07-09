namespace Elemica
{
    partial class ElemicaPrintOptionsForm
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
            this.buttonPrint = new System.Windows.Forms.Button();
            this.buttonPrintSend = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelTimesSent = new System.Windows.Forms.Label();
            this.labelLastSent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonPrint
            // 
            this.buttonPrint.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonPrint.Location = new System.Drawing.Point(259, 85);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(87, 23);
            this.buttonPrint.TabIndex = 1;
            this.buttonPrint.Text = "Print";
            this.buttonPrint.UseVisualStyleBackColor = false;
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);
            // 
            // buttonPrintSend
            // 
            this.buttonPrintSend.AutoSize = true;
            this.buttonPrintSend.BackColor = System.Drawing.SystemColors.Control;
            this.buttonPrintSend.Location = new System.Drawing.Point(166, 85);
            this.buttonPrintSend.Name = "buttonPrintSend";
            this.buttonPrintSend.Size = new System.Drawing.Size(87, 23);
            this.buttonPrintSend.TabIndex = 2;
            this.buttonPrintSend.Text = "Send and Print";
            this.buttonPrintSend.UseVisualStyleBackColor = false;
            this.buttonPrintSend.Click += new System.EventHandler(this.buttonPrintSend_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.Location = new System.Drawing.Point(10, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(336, 23);
            this.labelInfo.TabIndex = 4;
            this.labelInfo.Text = "Do you wish to send this purchase order to elemica?";
            // 
            // labelTimesSent
            // 
            this.labelTimesSent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimesSent.Location = new System.Drawing.Point(10, 32);
            this.labelTimesSent.Name = "labelTimesSent";
            this.labelTimesSent.Size = new System.Drawing.Size(173, 13);
            this.labelTimesSent.TabIndex = 5;
            this.labelTimesSent.Text = "Times Sent : ";
            // 
            // labelLastSent
            // 
            this.labelLastSent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLastSent.Location = new System.Drawing.Point(9, 50);
            this.labelLastSent.Name = "labelLastSent";
            this.labelLastSent.Size = new System.Drawing.Size(174, 13);
            this.labelLastSent.TabIndex = 6;
            this.labelLastSent.Text = "Last Sent : ";
            // 
            // ElemicaPrintOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.dexDefaultColorsProvider.SetAutoSetDexColors(this, false);
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(358, 120);
            this.ControlArea = false;
            this.ControlBox = false;
            this.Controls.Add(this.labelLastSent);
            this.Controls.Add(this.labelTimesSent);
            this.Controls.Add(this.buttonPrintSend);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.labelInfo);
            this.HResizeable = false;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElemicaPrintOptionsForm";
            this.ShowInTaskbar = false;
            this.StatusArea = false;
            this.Text = "Microsoft Dynamics GP";
            this.VResizeable = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonPrint;
        private System.Windows.Forms.Button buttonPrintSend;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelLastSent;
        private System.Windows.Forms.Label labelTimesSent;
    }
}


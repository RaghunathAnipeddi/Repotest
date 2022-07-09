namespace Elemica
{
    partial class ElemicaStausForm
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
            this.progressBarPOElemicaStaus = new System.Windows.Forms.ProgressBar();
            this.labelProgressStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBarPOElemicaStaus
            // 
            this.progressBarPOElemicaStaus.Location = new System.Drawing.Point(12, 52);
            this.progressBarPOElemicaStaus.MarqueeAnimationSpeed = 10;
            this.progressBarPOElemicaStaus.Maximum = 10;
            this.progressBarPOElemicaStaus.Name = "progressBarPOElemicaStaus";
            this.progressBarPOElemicaStaus.Size = new System.Drawing.Size(268, 38);
            this.progressBarPOElemicaStaus.TabIndex = 0;
            // 
            // labelProgressStatus
            // 
            this.labelProgressStatus.Location = new System.Drawing.Point(13, 13);
            this.labelProgressStatus.Name = "labelProgressStatus";
            this.labelProgressStatus.Size = new System.Drawing.Size(266, 36);
            this.labelProgressStatus.TabIndex = 1;
            this.labelProgressStatus.Text = "Validating PO";
            // 
            // ElemicaStausForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.dexDefaultColorsProvider.SetAutoSetDexColors(this, false);
            this.ClientSize = new System.Drawing.Size(291, 101);
            this.ControlArea = false;
            this.ControlBox = false;
            this.Controls.Add(this.labelProgressStatus);
            this.Controls.Add(this.progressBarPOElemicaStaus);
            this.HResizeable = false;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElemicaStausForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StatusArea = false;
            this.Text = "PO Elemica Status ";
            this.VResizeable = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarPOElemicaStaus;
        private System.Windows.Forms.Label labelProgressStatus;
    }
}


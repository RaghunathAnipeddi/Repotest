namespace Chempoint.GP.SO
{
    partial class GP_Lot_Info
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBoxSopNumber = new System.Windows.Forms.TextBox();
            this.TextBoxLineSeq = new System.Windows.Forms.TextBox();
            this.TextBoxItemNmbr = new System.Windows.Forms.TextBox();
            this.TextBoxSopType = new System.Windows.Forms.TextBox();
            this.lblLotNmbr = new System.Windows.Forms.Label();
            this.lblqty = new System.Windows.Forms.Label();
            this.lblcoasent = new System.Windows.Forms.Label();
            this.LotsDetailGrid = new Microsoft.Dynamics.Framework.UI.WinForms.Controls.BusinessGridView();
            ((System.ComponentModel.ISupportInitialize)(this.LotsDetailGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonSave
            // 
            this.ButtonSave.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.ButtonSave.BackColor = System.Drawing.Color.Transparent;
            this.dexButtonProvider.SetButtonType(this.ButtonSave, Microsoft.Dexterity.Shell.DexButtonType.ToolbarWithSeparator);
            this.ButtonSave.FlatAppearance.BorderSize = 0;
            this.ButtonSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ButtonSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ButtonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.ButtonSave.Image = global::GP_Lots_Form.Properties.Resources.Toolbar_Save;
            this.ButtonSave.Image = global::Chempoint.GP.SO.Properties.Resources.Toolbar_Save;            
            this.ButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonSave.Location = new System.Drawing.Point(6, -1);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(70, 25);
            this.ButtonSave.TabIndex = 1;
            this.ButtonSave.Text = "Save";
            this.ButtonSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonSave.UseVisualStyleBackColor = false;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.ButtonCancel.BackColor = System.Drawing.Color.Transparent;
            this.dexButtonProvider.SetButtonType(this.ButtonCancel, Microsoft.Dexterity.Shell.DexButtonType.ToolbarWithSeparator);
            this.ButtonCancel.FlatAppearance.BorderSize = 0;
            this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.ButtonCancel.Image = global::GP_Lots_Form.Properties.Resources.Toolbar_Cancel;
            this.ButtonCancel.Image = global::Chempoint.GP.SO.Properties.Resources.Toolbar_Cancel;
            this.ButtonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ButtonCancel.Location = new System.Drawing.Point(81, -1);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(77, 25);
            this.ButtonCancel.TabIndex = 2;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ButtonCancel.UseVisualStyleBackColor = false;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // label1
            // 
            this.dexLabelProvider.SetLinkField(this.label1, "textBoxSopNumbe");
            this.label1.Location = new System.Drawing.Point(13, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Sop Numbe";
            // 
            // label2
            // 
            this.dexLabelProvider.SetLinkField(this.label2, "textBoxSopType");
            this.label2.Location = new System.Drawing.Point(12, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Sop Type";
            // 
            // label3
            // 
            this.dexLabelProvider.SetLinkField(this.label3, "textBoxItemNmbr");
            this.label3.Location = new System.Drawing.Point(12, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Item Number";
            // 
            // label4
            // 
            this.dexLabelProvider.SetLinkField(this.label4, "textBoxLineSwq");
            this.label4.Location = new System.Drawing.Point(12, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Line Seq";
            // 
            // TextBoxSopNumber
            // 
            this.TextBoxSopNumber.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TextBoxSopNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBoxSopNumber.Location = new System.Drawing.Point(96, 34);
            this.TextBoxSopNumber.Name = "TextBoxSopNumber";
            this.TextBoxSopNumber.ReadOnly = true;
            this.TextBoxSopNumber.Size = new System.Drawing.Size(161, 13);
            this.TextBoxSopNumber.TabIndex = 7;
            this.TextBoxSopNumber.TabStop = false;
            // 
            // TextBoxLineSeq
            // 
            this.TextBoxLineSeq.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TextBoxLineSeq.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBoxLineSeq.Location = new System.Drawing.Point(96, 90);
            this.TextBoxLineSeq.Name = "TextBoxLineSeq";
            this.TextBoxLineSeq.ReadOnly = true;
            this.TextBoxLineSeq.Size = new System.Drawing.Size(161, 13);
            this.TextBoxLineSeq.TabIndex = 8;
            this.TextBoxLineSeq.TabStop = false;
            // 
            // TextBoxItemNmbr
            // 
            this.TextBoxItemNmbr.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TextBoxItemNmbr.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBoxItemNmbr.Location = new System.Drawing.Point(96, 72);
            this.TextBoxItemNmbr.Name = "TextBoxItemNmbr";
            this.TextBoxItemNmbr.ReadOnly = true;
            this.TextBoxItemNmbr.Size = new System.Drawing.Size(161, 13);
            this.TextBoxItemNmbr.TabIndex = 9;
            this.TextBoxItemNmbr.TabStop = false;
            // 
            // TextBoxSopType
            // 
            this.TextBoxSopType.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TextBoxSopType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBoxSopType.Location = new System.Drawing.Point(96, 53);
            this.TextBoxSopType.Name = "TextBoxSopType";
            this.TextBoxSopType.ReadOnly = true;
            this.TextBoxSopType.Size = new System.Drawing.Size(161, 13);
            this.TextBoxSopType.TabIndex = 10;
            this.TextBoxSopType.TabStop = false;
            // 
            // lblLotNmbr
            // 
            this.lblLotNmbr.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblLotNmbr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLotNmbr.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLotNmbr.Location = new System.Drawing.Point(13, 108);
            this.lblLotNmbr.Name = "lblLotNmbr";
            this.lblLotNmbr.Size = new System.Drawing.Size(168, 21);
            this.lblLotNmbr.TabIndex = 12;
            this.lblLotNmbr.Text = "Lot #";
            this.lblLotNmbr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblqty
            // 
            this.lblqty.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblqty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblqty.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblqty.Location = new System.Drawing.Point(180, 108);
            this.lblqty.Name = "lblqty";
            this.lblqty.Size = new System.Drawing.Size(85, 21);
            this.lblqty.TabIndex = 13;
            this.lblqty.Text = "Qty";
            this.lblqty.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblcoasent
            // 
            this.lblcoasent.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblcoasent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblcoasent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblcoasent.Location = new System.Drawing.Point(264, 108);
            this.lblcoasent.Name = "lblcoasent";
            this.lblcoasent.Size = new System.Drawing.Size(90, 21);
            this.lblcoasent.TabIndex = 14;
            this.lblcoasent.Text = "COA_Sent";
            this.lblcoasent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LotsDetailGrid
            // 
            this.LotsDetailGrid.AllowUserToResizeColumns = false;
            this.LotsDetailGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.LotsDetailGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.LotsDetailGrid.AutoFitColumnWidth = false;
            this.LotsDetailGrid.CanUseInactiveSelectionColor = false;
            this.LotsDetailGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.LotsDetailGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.LotsDetailGrid.ColumnHeadersVisible = false;
            this.LotsDetailGrid.CurrentRowFollowsMouse = false;
            this.LotsDetailGrid.DisplayColumns = 3;
            this.LotsDetailGrid.DisplayRows = 8;
            this.LotsDetailGrid.DrawHorizontalLine = false;
            this.LotsDetailGrid.EnableSelectAll = true;
            this.LotsDetailGrid.EnableSingleSelect = false;
            this.LotsDetailGrid.EnterKeyRaisesPerformCellAction = false;
            this.LotsDetailGrid.FillWithDummyColumn = true;
            this.LotsDetailGrid.FillWithDummyRows = true;
            this.LotsDetailGrid.FocusMode = Microsoft.Dynamics.Framework.UI.WinForms.Controls.BusinessGridViewFocusMode.Cell;
            this.LotsDetailGrid.KeyboardMode = Microsoft.Dynamics.Framework.UI.WinForms.Controls.BusinessGridViewKeyboardMode.Standard;
            this.LotsDetailGrid.ListMode = true;
            this.LotsDetailGrid.Location = new System.Drawing.Point(13, 128);
            this.LotsDetailGrid.MultiSelect = false;
            this.LotsDetailGrid.Name = "LotsDetailGrid";
            this.LotsDetailGrid.Renderer = null;
            this.LotsDetailGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.LotsDetailGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.LotsDetailGrid.RowHeadersVisible = false;
            this.LotsDetailGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.LotsDetailGrid.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.Empty;
            this.LotsDetailGrid.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Empty;
            this.LotsDetailGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LotsDetailGrid.SelectCurrentRowOnFocus = false;
            this.LotsDetailGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.LotsDetailGrid.ShowPinSelectors = false;
            this.LotsDetailGrid.Size = new System.Drawing.Size(341, 134);
            this.LotsDetailGrid.SuperToolTipIcon = null;
            this.LotsDetailGrid.SuperToolTipText = "";
            this.LotsDetailGrid.TabIndex = 3;
            this.LotsDetailGrid.UseAlternatingRowColor = false;
            this.LotsDetailGrid.UseInactiveSelectionColor = false;
            // 
            // GP_Lot_Info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 302);
            this.Controls.Add(this.LotsDetailGrid);
            this.Controls.Add(this.lblcoasent);
            this.Controls.Add(this.lblqty);
            this.Controls.Add(this.lblLotNmbr);
            this.Controls.Add(this.TextBoxSopType);
            this.Controls.Add(this.TextBoxItemNmbr);
            this.Controls.Add(this.TextBoxLineSeq);
            this.Controls.Add(this.TextBoxSopNumber);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonSave);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "GP_Lot_Info";
            this.Text = "GP Lot# Information";
            this.Load += new System.EventHandler(this.GP_Lot_Info_Load);
            ((System.ComponentModel.ISupportInitialize)(this.LotsDetailGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TextBoxSopNumber;
        private System.Windows.Forms.TextBox TextBoxLineSeq;
        private System.Windows.Forms.TextBox TextBoxItemNmbr;
        private System.Windows.Forms.TextBox TextBoxSopType;
        private System.Windows.Forms.Label lblLotNmbr;
        private System.Windows.Forms.Label lblqty;
        private System.Windows.Forms.Label lblcoasent;
        private Microsoft.Dynamics.Framework.UI.WinForms.Controls.BusinessGridView LotsDetailGrid;
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Dexterity.Bridge;
using Microsoft.Dexterity.Applications;
using Microsoft.Dexterity.Shell;

namespace Elemica
{
    /// <summary>
    /// This class repesents a dialog window for the print options of PO print button.
    /// </summary>
    public partial class ElemicaPrintOptionsForm : DexUIForm
    {
        //Variables
        string button_Id = "0";

        /// <summary>
        /// Initializes the window
        /// </summary>
        public ElemicaPrintOptionsForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method creates an instance of POEntryPrintDialogForm and opens that window in an dialog.
        /// </summary>
        /// <returns></returns>
        public string ShowMessageBox()
        {
            try
            {
                // creates an instance
                //newPrintDialogForm = new POEntryPrintDialogForm();
                // opens the window in dialog window.
                this.ShowDialog();

                return button_Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Print Dialog window", ex);
            }
        }

        /// <summary>
        /// This method sets the label contents.
        /// Initially labelTimesSent and  labelLastSent are invisible. Only if this po is already
        /// send to elemica, then only this will make visible.
        /// </summary>
        /// <param name="timesSent"></param>
        /// <param name="lastDateSent"></param>
        public void SetLabelContents(int timesSent, DateTime lastDateSent)
        {
            labelTimesSent.Text = labelTimesSent.Text + timesSent.ToString();
            if (timesSent > 0)
                labelLastSent.Text = labelLastSent.Text + lastDateSent.ToShortDateString();
            else
                labelLastSent.Text = labelLastSent.Text +  "  NA";
        }

        /// <summary>
        /// This event is called when Print button is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // assings button_Id and disposes the current window
                button_Id = "1";
                this.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This event is called when Print And Send button is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPrintSend_Click(object sender, EventArgs e)
        {
            try
            {
                // assings button_Id and disposes the current window
                button_Id = "2";
                this.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
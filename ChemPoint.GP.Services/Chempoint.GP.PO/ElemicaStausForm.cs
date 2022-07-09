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
    /// This class is as a progress bar for the PO print button.
    /// </summary>
    public partial class ElemicaStausForm : DexUIForm
    {
        /// <summary>
        /// Initializes the form
        /// </summary>
        public ElemicaStausForm()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// This method sets the label text value
        /// </summary>
        /// <param name="message"></param>
        public void SetProgressWindowLabelText(string message)
        {
            try
            {
                labelProgressStatus.Text = message;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while setting the progress window label value", ex);
            }
        }

        /// <summary>
        /// This method sets the progress bar status value.
        /// </summary>
        /// <param name="value"></param>
        public void SetProgessBarStatus(int value)
        {
            try
            {
                progressBarPOElemicaStaus.Value = value;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while setting the progress bar status value", ex);
            }
        }
    }
}
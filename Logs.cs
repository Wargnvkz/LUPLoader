using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LUPLoader
{
    public partial class Logs : Form
    {
        public DateTime LastOutput = DateTime.MinValue;
        public Logs()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Visible && textBox1.Visible)
            {
                if (Log.LastLog > LastOutput)
                {
                    LastOutput = DateTime.Now;
                    var txt = String.Join("\r\n", Log.CurrentMessages) + "\r\n";
                    textBox1.Text = txt;
                    textBox1.SelectionStart = textBox1.TextLength;
                    textBox1.ScrollToCaret();
                }
            }
        }
    }
}

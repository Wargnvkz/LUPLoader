using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace LUPLoader
{
    public partial class Warning : Form
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        DateTime StartWarning;
        double Timeout = 60;

        public Warning(string text,string helper)
        {
            InitializeComponent();
            Text = text;
            label2.Text = Text;
            StartWarning = DateTime.Now;
            var tm = Settings.GetOptionValue<int>(Constants.DelayedTimeout);
            if (tm > 0) Timeout = tm;
            textBox1.Text = helper;
            //SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        private void Warning_SizeChanged(object sender, EventArgs e)
        {
            /*panel1.Location = new Point(
    this.ClientSize.Width / 2 - panel1.Size.Width / 2,
    this.ClientSize.Height / 2 - panel1.Size.Height / 2);
            panel1.Anchor = AnchorStyles.None;*/
        }


        private void Warning_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.None || DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                switch (e.CloseReason)
                {
                    case CloseReason.ApplicationExitCall:
                    case CloseReason.MdiFormClosing:
                    case CloseReason.None:
                    case CloseReason.UserClosing:
                        DialogResult = System.Windows.Forms.DialogResult.OK;
                        break;

                    case CloseReason.FormOwnerClosing:
                    case CloseReason.TaskManagerClosing:
                    case CloseReason.WindowsShutDown:
                        DialogResult = System.Windows.Forms.DialogResult.Retry;
                        break;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - StartWarning).TotalSeconds > Timeout)
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }
    }
}

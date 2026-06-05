using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Napominator
{
    public partial class Message_To_Polina : Form
    {
        Random rnd = new Random();
        int counter = 10;
        Rectangle resolution = Screen.PrimaryScreen!.Bounds;
        Boolean allowFormClose = false;
        Boolean showDesktop = false;
        Boolean AllowCloseWindow = false;
        int timerPeriodSeconds = 0;

        public Message_To_Polina()
        {
            InitializeComponent();
            this.Hide();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
            LockWorkStation();
        }

        [DllImport("user32.dll")]
        public static extern void LockWorkStation();
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;

        string USERNAME = Functions.username2USERNAME(System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]);

        public void Set_counter(int _i)
        {
            counter = _i;
            RefreshCounter();
        }
        public void Set_NotifyText(string _s)
        {
            richTextBox_NotifyText.Text = _s;
        }

        public void Set_FormCaption(string _s)
        {
            this.Text = _s;
        }
        public void Set_ShowDesktop(Boolean _showDesktop)
        {
            showDesktop = _showDesktop;
        }
        public void Set_AllowCloseWindow(Boolean allowCloseWindow)
        {
            AllowCloseWindow = allowCloseWindow;
        }

        public void Set_TimerPeriod(int seconds)
        {
            timerPeriodSeconds = seconds;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (AllowCloseWindow == false)
            {
                richTextBox_NotifyText.Text = "Не верю! Не верю!";
            }
            else
            {
                timer_Polina_Form.Stop();
                this.Close();
            }
        }

        private void timer_Polina_Form_Tick(object sender, EventArgs e)
        {
            richTextBox_NotifyText.SelectAll();
            richTextBox_NotifyText.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox_NotifyText.DeselectAll();

            if (counter <= 0)
            {
                allowFormClose = true;
                this.Close();
            }
            else
            {
                if (showDesktop)
                {
                    IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
                    SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
                    //System.Threading.Thread.Sleep(1000);
                    //SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL_UNDO, IntPtr.Zero);
                }
                this.Hide();
                this.Show();

                if (AllowCloseWindow)
                {
                    this.TopMost = false;
                }
                else
                {
                    this.TopMost = true;
                }
                CenterAndMAximize();

                counter--;

                RefreshCounter();

                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                richTextBox_NotifyText.ForeColor = randomColor;
                randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                richTextBox_NotifyText.ForeColor = randomColor;
            }
        }

        void RefreshCounter()
        {
            int minutes = counter / 60;
            int seconds = counter % 60;
            label_Count.Text = $"{minutes:00}:{seconds:00}";
        }

        private void CenterAndMAximize()
        {
            int sizeKoeff = 95;
            //if (USERNAME == "d")
            //    sizeKoeff = 30;

            this.CenterToScreen();

            Size size = this.Size;
            size.Width = resolution.Width / 100 * sizeKoeff;
            size.Height = resolution.Height / 100 * sizeKoeff;
            Point point = ((Point)size);
            
            point.X = (resolution.Width - (resolution.Width - resolution.Width / 100 * 1) );
            point.Y = (resolution.Height - (resolution.Height - Height / 100 * 1) );

            this.Location = point;
            this.Size = size;

            size.Width -= 40;
            size.Height -= 90;
            richTextBox_NotifyText.Size = size;
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            if (AllowCloseWindow == false)
            {
                Point point = button1.Location;

                point.X = rnd.Next(resolution.Width);
                point.Y = rnd.Next(resolution.Height);

                button1.Location = point;
                button1.Text = "Попробуй поймай! :)";
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.X))
            {
                allowFormClose = true;
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Message_To_Polina_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowFormClose)
            {
                e.Cancel = true;
            }
        }

        private void Message_To_Polina_Shown(object sender, EventArgs e)
        {           

            if (AllowCloseWindow == false)
            {
                timer_Polina_Form.Interval = 1000 * 1;
                timer_Polina_Form.Start();
            }
            else
            {
                allowFormClose = true;
                this.ControlBox = true;
                CenterAndMAximize();
            }

            if (timerPeriodSeconds > 0)
            {
                timer_Polina_Form.Interval = 1000 * timerPeriodSeconds;
                timer_Polina_Form.Start();
            }
        }
    }
}

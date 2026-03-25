namespace Napominator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            button_StartStop = new Button();
            textBox_Log = new TextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            notifyIcon1 = new NotifyIcon(components);
            groupBox1 = new GroupBox();
            label6 = new Label();
            label5 = new Label();
            dateTime_to = new DateTimePicker();
            label4 = new Label();
            dateTime_from = new DateTimePicker();
            checkBox_Papa = new CheckBox();
            checkBox_Mama = new CheckBox();
            label3 = new Label();
            textBox_NotifyText = new TextBox();
            checkBox_Polina = new CheckBox();
            groupBox2 = new GroupBox();
            btn28min = new Button();
            btn25min = new Button();
            textBox_PersonalPeriod = new TextBox();
            textBox_PersonalPeriodText = new TextBox();
            label8 = new Label();
            btnPersonalTimerStartStop = new Button();
            label7 = new Label();
            tb_noiselevel = new TextBox();
            timer_Personal = new System.Windows.Forms.Timer(components);
            btn_IPInfo = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // button_StartStop
            // 
            button_StartStop.Location = new Point(1, 1);
            button_StartStop.Name = "button_StartStop";
            button_StartStop.Size = new Size(86, 23);
            button_StartStop.TabIndex = 0;
            button_StartStop.Text = "Start";
            button_StartStop.UseVisualStyleBackColor = true;
            button_StartStop.Click += Button_StartStop_Click;
            // 
            // textBox_Log
            // 
            textBox_Log.Location = new Point(1, 214);
            textBox_Log.Multiline = true;
            textBox_Log.Name = "textBox_Log";
            textBox_Log.ScrollBars = ScrollBars.Both;
            textBox_Log.Size = new Size(705, 205);
            textBox_Log.TabIndex = 1;
            // 
            // timer1
            // 
            timer1.Tick += Timer1_Tick;
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "Napominator";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += NotifyIcon1_DoubleClick;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(dateTime_to);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(dateTime_from);
            groupBox1.Controls.Add(checkBox_Papa);
            groupBox1.Controls.Add(checkBox_Mama);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBox_NotifyText);
            groupBox1.Controls.Add(checkBox_Polina);
            groupBox1.Controls.Add(groupBox2);
            groupBox1.Location = new Point(93, 1);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(613, 207);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Параметры";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(11, 100);
            label6.Name = "label6";
            label6.Size = new Size(86, 15);
            label6.TabIndex = 14;
            label6.Text = "Время работы";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(238, 102);
            label5.Name = "label5";
            label5.Size = new Size(21, 15);
            label5.TabIndex = 13;
            label5.Text = "по";
            // 
            // dateTime_to
            // 
            dateTime_to.Format = DateTimePickerFormat.Time;
            dateTime_to.Location = new Point(265, 97);
            dateTime_to.Name = "dateTime_to";
            dateTime_to.Size = new Size(82, 23);
            dateTime_to.TabIndex = 12;
            dateTime_to.Value = new DateTime(2023, 1, 13, 21, 0, 0, 0);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(131, 100);
            label4.Name = "label4";
            label4.Size = new Size(13, 15);
            label4.TabIndex = 11;
            label4.Text = "с";
            // 
            // dateTime_from
            // 
            dateTime_from.Format = DateTimePickerFormat.Time;
            dateTime_from.Location = new Point(150, 97);
            dateTime_from.Name = "dateTime_from";
            dateTime_from.Size = new Size(82, 23);
            dateTime_from.TabIndex = 10;
            dateTime_from.Value = new DateTime(2023, 1, 13, 9, 0, 0, 0);
            // 
            // checkBox_Papa
            // 
            checkBox_Papa.AutoSize = true;
            checkBox_Papa.Location = new Point(197, 18);
            checkBox_Papa.Name = "checkBox_Papa";
            checkBox_Papa.Size = new Size(79, 19);
            checkBox_Papa.TabIndex = 9;
            checkBox_Papa.Text = "Для папы";
            checkBox_Papa.UseVisualStyleBackColor = true;
            checkBox_Papa.CheckedChanged += CheckBox_Papa_CheckedChanged;
            // 
            // checkBox_Mama
            // 
            checkBox_Mama.AutoSize = true;
            checkBox_Mama.Location = new Point(108, 18);
            checkBox_Mama.Name = "checkBox_Mama";
            checkBox_Mama.Size = new Size(83, 19);
            checkBox_Mama.TabIndex = 8;
            checkBox_Mama.Text = "Для мамы";
            checkBox_Mama.UseVisualStyleBackColor = true;
            checkBox_Mama.CheckedChanged += CheckBox_Mama_CheckedChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 40);
            label3.Name = "label3";
            label3.Size = new Size(115, 15);
            label3.TabIndex = 7;
            label3.Text = "Текст напоминания";
            // 
            // textBox_NotifyText
            // 
            textBox_NotifyText.Font = new Font("Segoe UI", 8.25F);
            textBox_NotifyText.Location = new Point(127, 37);
            textBox_NotifyText.Multiline = true;
            textBox_NotifyText.Name = "textBox_NotifyText";
            textBox_NotifyText.ScrollBars = ScrollBars.Vertical;
            textBox_NotifyText.Size = new Size(478, 54);
            textBox_NotifyText.TabIndex = 2;
            textBox_NotifyText.Text = "Что делать?";
            // 
            // checkBox_Polina
            // 
            checkBox_Polina.AutoSize = true;
            checkBox_Polina.Location = new Point(6, 18);
            checkBox_Polina.Name = "checkBox_Polina";
            checkBox_Polina.Size = new Size(96, 19);
            checkBox_Polina.TabIndex = 0;
            checkBox_Polina.Text = "Для Полины";
            checkBox_Polina.UseVisualStyleBackColor = true;
            checkBox_Polina.CheckedChanged += CheckBox_Polina_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btn28min);
            groupBox2.Controls.Add(btn25min);
            groupBox2.Controls.Add(textBox_PersonalPeriod);
            groupBox2.Controls.Add(textBox_PersonalPeriodText);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(btnPersonalTimerStartStop);
            groupBox2.Location = new Point(6, 118);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(594, 89);
            groupBox2.TabIndex = 20;
            groupBox2.TabStop = false;
            groupBox2.Text = "Дополнительный таймер";
            // 
            // btn28min
            // 
            btn28min.Location = new Point(383, 16);
            btn28min.Name = "btn28min";
            btn28min.Size = new Size(50, 24);
            btn28min.TabIndex = 23;
            btn28min.Text = "28min";
            btn28min.UseVisualStyleBackColor = true;
            // 
            // btn25min
            // 
            btn25min.Location = new Point(327, 16);
            btn25min.Name = "btn25min";
            btn25min.Size = new Size(50, 24);
            btn25min.TabIndex = 22;
            btn25min.Text = "25min";
            btn25min.UseVisualStyleBackColor = true;
            // 
            // textBox_PersonalPeriod
            // 
            textBox_PersonalPeriod.Location = new Point(137, 16);
            textBox_PersonalPeriod.Name = "textBox_PersonalPeriod";
            textBox_PersonalPeriod.Size = new Size(73, 23);
            textBox_PersonalPeriod.TabIndex = 17;
            textBox_PersonalPeriod.Text = "5";
            textBox_PersonalPeriod.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_PersonalPeriodText
            // 
            textBox_PersonalPeriodText.Font = new Font("Segoe UI", 8.25F);
            textBox_PersonalPeriodText.Location = new Point(6, 40);
            textBox_PersonalPeriodText.Multiline = true;
            textBox_PersonalPeriodText.Name = "textBox_PersonalPeriodText";
            textBox_PersonalPeriodText.ScrollBars = ScrollBars.Vertical;
            textBox_PersonalPeriodText.Size = new Size(582, 43);
            textBox_PersonalPeriodText.TabIndex = 21;
            textBox_PersonalPeriodText.Text = "Что делать?";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 19);
            label8.Name = "label8";
            label8.Size = new Size(125, 15);
            label8.TabIndex = 16;
            label8.Text = "Личный таймер в сек";
            // 
            // btnPersonalTimerStartStop
            // 
            btnPersonalTimerStartStop.Location = new Point(216, 15);
            btnPersonalTimerStartStop.Name = "btnPersonalTimerStartStop";
            btnPersonalTimerStartStop.Size = new Size(86, 24);
            btnPersonalTimerStartStop.TabIndex = 19;
            btnPersonalTimerStartStop.Text = "START";
            btnPersonalTimerStartStop.UseVisualStyleBackColor = true;
            btnPersonalTimerStartStop.Click += btnPersonalTimerStartStop_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 6.75F);
            label7.Location = new Point(1, 187);
            label7.Name = "label7";
            label7.Size = new Size(77, 12);
            label7.TabIndex = 17;
            label7.Text = "2023-11-21 00:02";
            // 
            // tb_noiselevel
            // 
            tb_noiselevel.Location = new Point(1, 425);
            tb_noiselevel.Multiline = true;
            tb_noiselevel.Name = "tb_noiselevel";
            tb_noiselevel.ScrollBars = ScrollBars.Both;
            tb_noiselevel.Size = new Size(705, 26);
            tb_noiselevel.TabIndex = 18;
            // 
            // timer_Personal
            // 
            timer_Personal.Tick += timer_Personal_Tick;
            // 
            // btn_IPInfo
            // 
            btn_IPInfo.Location = new Point(1, 68);
            btn_IPInfo.Name = "btn_IPInfo";
            btn_IPInfo.Size = new Size(86, 23);
            btn_IPInfo.TabIndex = 19;
            btn_IPInfo.Text = "IP Info";
            btn_IPInfo.UseVisualStyleBackColor = true;
            btn_IPInfo.Click += btn_IPInfo_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(705, 456);
            Controls.Add(btn_IPInfo);
            Controls.Add(tb_noiselevel);
            Controls.Add(label7);
            Controls.Add(groupBox1);
            Controls.Add(textBox_Log);
            Controls.Add(button_StartStop);
            Name = "MainForm";
            Text = "Напоминатор!";
            FormClosing += Form1_FormClosing;
            FormClosed += MainForm_FormClosed;
            Load += Form1_Load;
            Shown += Form1_Shown;
            Resize += Form1_Resize;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_StartStop;
        private TextBox textBox_Log;
        private GroupBox groupBox1;
        private CheckBox checkBox_Polina;
        private Label label3;
        private TextBox textBox_NotifyText;
        private CheckBox checkBox_Papa;
        private CheckBox checkBox_Mama;
        private Label label5;
        private DateTimePicker dateTime_to;
        private Label label4;
        private DateTimePicker dateTime_from;
        private Label label6;
        private Label label7;
        private TextBox tb_noiselevel;
        public System.Windows.Forms.Timer timer1;
        public NotifyIcon notifyIcon1;
        private Button btnPersonalTimerStartStop;
        private TextBox textBox_PersonalPeriod;
        private Label label8;
        private GroupBox groupBox2;
        public System.Windows.Forms.Timer timer_Personal;
        private TextBox textBox_PersonalPeriodText;
        private Button btn_IPInfo;
        private Button btn28min;
        private Button btn25min;
    }
}
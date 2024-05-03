namespace WinFormsApp1
{
    partial class Form1
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
            textBox_Period = new TextBox();
            label6 = new Label();
            label5 = new Label();
            dateTime_to = new DateTimePicker();
            label4 = new Label();
            dateTime_from = new DateTimePicker();
            checkBox_Papa = new CheckBox();
            checkBox_Mama = new CheckBox();
            label3 = new Label();
            textBox_NotifyLenght = new TextBox();
            label2 = new Label();
            label1 = new Label();
            textBox_NotifyText = new TextBox();
            checkBox_Polina = new CheckBox();
            label7 = new Label();
            tb_noiselevel = new TextBox();
            groupBox1.SuspendLayout();
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
            textBox_Log.Location = new Point(1, 202);
            textBox_Log.Multiline = true;
            textBox_Log.Name = "textBox_Log";
            textBox_Log.ScrollBars = ScrollBars.Both;
            textBox_Log.Size = new Size(705, 217);
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
            groupBox1.Controls.Add(textBox_Period);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(dateTime_to);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(dateTime_from);
            groupBox1.Controls.Add(checkBox_Papa);
            groupBox1.Controls.Add(checkBox_Mama);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBox_NotifyLenght);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox_NotifyText);
            groupBox1.Controls.Add(checkBox_Polina);
            groupBox1.Location = new Point(93, 1);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(613, 195);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Параметры";
            // 
            // textBox_Period
            // 
            textBox_Period.Location = new Point(140, 141);
            textBox_Period.Name = "textBox_Period";
            textBox_Period.Size = new Size(82, 23);
            textBox_Period.TabIndex = 15;
            textBox_Period.Text = "5";
            textBox_Period.TextAlign = HorizontalAlignment.Center;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(4, 173);
            label6.Name = "label6";
            label6.Size = new Size(86, 15);
            label6.TabIndex = 14;
            label6.Text = "Время работы";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(228, 175);
            label5.Name = "label5";
            label5.Size = new Size(21, 15);
            label5.TabIndex = 13;
            label5.Text = "по";
            // 
            // dateTime_to
            // 
            dateTime_to.Format = DateTimePickerFormat.Time;
            dateTime_to.Location = new Point(255, 170);
            dateTime_to.Name = "dateTime_to";
            dateTime_to.Size = new Size(82, 23);
            dateTime_to.TabIndex = 12;
            dateTime_to.Value = new DateTime(2023, 1, 13, 21, 0, 0, 0);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(121, 173);
            label4.Name = "label4";
            label4.Size = new Size(13, 15);
            label4.TabIndex = 11;
            label4.Text = "с";
            // 
            // dateTime_from
            // 
            dateTime_from.Format = DateTimePickerFormat.Time;
            dateTime_from.Location = new Point(140, 170);
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
            label3.Location = new Point(6, 46);
            label3.Name = "label3";
            label3.Size = new Size(115, 15);
            label3.TabIndex = 7;
            label3.Text = "Текст напоминания";
            // 
            // textBox_NotifyLenght
            // 
            textBox_NotifyLenght.Location = new Point(429, 141);
            textBox_NotifyLenght.Name = "textBox_NotifyLenght";
            textBox_NotifyLenght.Size = new Size(29, 23);
            textBox_NotifyLenght.TabIndex = 6;
            textBox_NotifyLenght.Text = "5";
            textBox_NotifyLenght.TextAlign = HorizontalAlignment.Center;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(239, 144);
            label2.Name = "label2";
            label2.Size = new Size(184, 15);
            label2.TabIndex = 5;
            label2.Text = "Длительность напоминания сек";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 144);
            label1.Name = "label1";
            label1.Size = new Size(115, 15);
            label1.TabIndex = 4;
            label1.Text = "Периодичность cek";
            // 
            // textBox_NotifyText
            // 
            textBox_NotifyText.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            textBox_NotifyText.Location = new Point(195, 42);
            textBox_NotifyText.Multiline = true;
            textBox_NotifyText.Name = "textBox_NotifyText";
            textBox_NotifyText.ScrollBars = ScrollBars.Vertical;
            textBox_NotifyText.Size = new Size(410, 93);
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
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 6.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(705, 456);
            Controls.Add(tb_noiselevel);
            Controls.Add(label7);
            Controls.Add(groupBox1);
            Controls.Add(textBox_Log);
            Controls.Add(button_StartStop);
            Name = "Form1";
            Text = "Напоминатор! v 270324";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Shown += Form1_Shown;
            Resize += Form1_Resize;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_StartStop;
        private TextBox textBox_Log;
        private GroupBox groupBox1;
        private CheckBox checkBox_Polina;
        private Label label3;
        private TextBox textBox_NotifyLenght;
        private Label label2;
        private Label label1;
        private TextBox textBox_NotifyText;
        private CheckBox checkBox_Papa;
        private CheckBox checkBox_Mama;
        private Label label5;
        private DateTimePicker dateTime_to;
        private Label label4;
        private DateTimePicker dateTime_from;
        private Label label6;
        private TextBox textBox_Period;
        private Label label7;
        private TextBox tb_noiselevel;
        public System.Windows.Forms.Timer timer1;
        public NotifyIcon notifyIcon1;
    }
}
namespace Napominator
{
    partial class Message_To_Polina
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
            components = new System.ComponentModel.Container();
            button1 = new Button();
            button2 = new Button();
            timer_Polina_Form = new System.Windows.Forms.Timer(components);
            label_Count = new Label();
            richTextBox_NotifyText = new RichTextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(121, 23);
            button1.TabIndex = 2;
            button1.Text = "Закрыть это окно.";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            button1.MouseEnter += button1_MouseEnter;
            // 
            // button2
            // 
            button2.Enabled = false;
            button2.Location = new Point(139, 12);
            button2.Name = "button2";
            button2.Size = new Size(165, 23);
            button2.TabIndex = 3;
            button2.Text = "lock session";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // timer_Polina_Form
            // 
            timer_Polina_Form.Tick += timer_Polina_Form_Tick;
            // 
            // label_Count
            // 
            label_Count.AutoSize = true;
            label_Count.Font = new Font("Segoe UI", 18F);
            label_Count.Location = new Point(310, 6);
            label_Count.Name = "label_Count";
            label_Count.Size = new Size(28, 32);
            label_Count.TabIndex = 4;
            label_Count.Text = "[]";
            // 
            // richTextBox_NotifyText
            // 
            richTextBox_NotifyText.BorderStyle = BorderStyle.FixedSingle;
            richTextBox_NotifyText.Font = new Font("Segoe UI", 36F);
            richTextBox_NotifyText.Location = new Point(12, 41);
            richTextBox_NotifyText.Name = "richTextBox_NotifyText";
            richTextBox_NotifyText.ReadOnly = true;
            richTextBox_NotifyText.Size = new Size(767, 375);
            richTextBox_NotifyText.TabIndex = 5;
            richTextBox_NotifyText.Text = "TEXT";
            // 
            // Message_To_Polina
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(952, 428);
            ControlBox = false;
            Controls.Add(richTextBox_NotifyText);
            Controls.Add(label_Count);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Message_To_Polina";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "НАПОМИНАТОР!!!";
            TopMost = true;
            FormClosing += Message_To_Polina_FormClosing;
            Shown += Message_To_Polina_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private Button button2;
        private System.Windows.Forms.Timer timer_Polina_Form;
        private Label label_Count;
        private RichTextBox richTextBox_NotifyText;
    }
}
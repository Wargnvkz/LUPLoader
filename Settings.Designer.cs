namespace LUPLoader
{
    partial class SettingsForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txbTCPPort = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txbLogPath = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.txbSAPHost = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txbSAPInstance = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txbSAPInstanceName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txbSAPClient = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txbSAPLogin = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txbSAPPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.txbDelayedTimeout = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.button7 = new System.Windows.Forms.Button();
            this.GranulateLoadButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(12, 419);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(321, 419);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Отмена";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Порт TCP сервера:";
            // 
            // txbTCPPort
            // 
            this.txbTCPPort.Location = new System.Drawing.Point(119, 12);
            this.txbTCPPort.Name = "txbTCPPort";
            this.txbTCPPort.Size = new System.Drawing.Size(47, 20);
            this.txbTCPPort.TabIndex = 7;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 327);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(219, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "Сброс времени  поступления мешков";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 304);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Путь к файлам логов:";
            // 
            // txbLogPath
            // 
            this.txbLogPath.Location = new System.Drawing.Point(127, 301);
            this.txbLogPath.Name = "txbLogPath";
            this.txbLogPath.Size = new System.Drawing.Size(269, 20);
            this.txbLogPath.TabIndex = 10;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoCheck = false;
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(9, 396);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(105, 17);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Режим отладки";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.checkBox1_Click);
            // 
            // txbSAPHost
            // 
            this.txbSAPHost.Location = new System.Drawing.Point(124, 38);
            this.txbSAPHost.Name = "txbSAPHost";
            this.txbSAPHost.Size = new System.Drawing.Size(272, 20);
            this.txbSAPHost.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 41);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Сервер SAP:";
            // 
            // txbSAPInstance
            // 
            this.txbSAPInstance.Location = new System.Drawing.Point(124, 64);
            this.txbSAPInstance.Name = "txbSAPInstance";
            this.txbSAPInstance.Size = new System.Drawing.Size(272, 20);
            this.txbSAPInstance.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Инстанция:";
            // 
            // txbSAPInstanceName
            // 
            this.txbSAPInstanceName.Location = new System.Drawing.Point(124, 90);
            this.txbSAPInstanceName.Name = "txbSAPInstanceName";
            this.txbSAPInstanceName.Size = new System.Drawing.Size(272, 20);
            this.txbSAPInstanceName.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 93);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Имя инстанции:";
            // 
            // txbSAPClient
            // 
            this.txbSAPClient.Location = new System.Drawing.Point(124, 116);
            this.txbSAPClient.Name = "txbSAPClient";
            this.txbSAPClient.Size = new System.Drawing.Size(272, 20);
            this.txbSAPClient.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Мандант:";
            // 
            // txbSAPLogin
            // 
            this.txbSAPLogin.Location = new System.Drawing.Point(124, 142);
            this.txbSAPLogin.Name = "txbSAPLogin";
            this.txbSAPLogin.Size = new System.Drawing.Size(272, 20);
            this.txbSAPLogin.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 145);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Логин:";
            // 
            // txbSAPPassword
            // 
            this.txbSAPPassword.Location = new System.Drawing.Point(124, 168);
            this.txbSAPPassword.Name = "txbSAPPassword";
            this.txbSAPPassword.PasswordChar = '*';
            this.txbSAPPassword.Size = new System.Drawing.Size(272, 20);
            this.txbSAPPassword.TabIndex = 23;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 171);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Пароль:";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(254, 194);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(142, 23);
            this.button4.TabIndex = 24;
            this.button4.Text = "Проверка связи с SAP";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(9, 269);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(200, 23);
            this.button5.TabIndex = 25;
            this.button5.Text = "Изменить пароль администратора";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // txbDelayedTimeout
            // 
            this.txbDelayedTimeout.Location = new System.Drawing.Point(216, 232);
            this.txbDelayedTimeout.Name = "txbDelayedTimeout";
            this.txbDelayedTimeout.Size = new System.Drawing.Size(50, 20);
            this.txbDelayedTimeout.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 235);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(204, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Повторить отложенную команду через";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(272, 235);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(42, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "секунд";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(237, 327);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(159, 23);
            this.button6.TabIndex = 29;
            this.button6.Text = "Обнулить гранулят";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(234, 274);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(86, 13);
            this.label11.TabIndex = 30;
            this.label11.Text = "Уровень логов:";
            // 
            // cmbLogLevel
            // 
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] {
            "Действия пользователя и ошибки",
            "+Команды",
            "Полный"});
            this.cmbLogLevel.Location = new System.Drawing.Point(321, 269);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(75, 21);
            this.cmbLogLevel.TabIndex = 31;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(12, 356);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(219, 23);
            this.button7.TabIndex = 32;
            this.button7.Text = "Сдвиг мешков";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // GranulateLoadButton
            // 
            this.GranulateLoadButton.Location = new System.Drawing.Point(237, 356);
            this.GranulateLoadButton.Name = "GranulateLoadButton";
            this.GranulateLoadButton.Size = new System.Drawing.Size(159, 23);
            this.GranulateLoadButton.TabIndex = 33;
            this.GranulateLoadButton.Text = "Загрузка гранулята";
            this.GranulateLoadButton.UseVisualStyleBackColor = true;
            this.GranulateLoadButton.Click += new System.EventHandler(this.GranulateLoadButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 454);
            this.Controls.Add(this.GranulateLoadButton);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.cmbLogLevel);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txbDelayedTimeout);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.txbSAPPassword);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txbSAPLogin);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txbSAPClient);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txbSAPInstanceName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txbSAPInstance);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txbSAPHost);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.txbLogPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.txbTCPPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "SettingsForm";
            this.Text = "Настройки";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txbTCPPort;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txbLogPath;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox txbSAPHost;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txbSAPInstance;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txbSAPInstanceName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txbSAPClient;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txbSAPLogin;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txbSAPPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox txbDelayedTimeout;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button GranulateLoadButton;
    }
}
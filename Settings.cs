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
    public partial class SettingsForm : Form
    {
        public UPMControl UPM;
        public override string ToString()
        {
            return "Порт: " + TCPServerPort+"; Путь логов: "+ LogPath+"; SAP: "+SAPHost+"/"+SAPInstance+"/"+SAPInstanceName+"/"+SAPClient+"/"+SAPLogin;
        }
        public int TCPServerPort
        {
            get
            {
                int v = -1;
                int.TryParse(txbTCPPort.Text, out v);
                return v;
            }
            set
            {
                txbTCPPort.Text = value.ToString();
            }
        }

        public string LogPath
        {
            get
            {
                return txbLogPath.Text;
            }
            set
            {
                txbLogPath.Text = value;
            }
        }

        public bool IsDebug
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public string SAPHost
        {
            get
            {
                return txbSAPHost.Text;
            }
            set
            {
                txbSAPHost.Text = value;
            }
        }
        public string SAPInstance
        {
            get
            {
                return txbSAPInstance.Text;
            }
            set
            {
                txbSAPInstance.Text = value;
            }
        }
        public string SAPInstanceName
        {
            get
            {
                return txbSAPInstanceName.Text;
            }
            set
            {
                txbSAPInstanceName.Text = value;
            }
        }

        public string SAPClient
        {
            get
            {
                return txbSAPClient.Text;
            }
            set
            {
                txbSAPClient.Text = value;
            }
        }
        public string SAPLogin
        {
            get
            {
                return txbSAPLogin.Text;
            }
            set
            {
                txbSAPLogin.Text = value;
            }
        }
        public string SAPPassword
        {
            get
            {
                return txbSAPPassword.Text;
            }
            set
            {
                txbSAPPassword.Text = value;
            }
        }

        public int DelayedTimeout
        {
            get
            {
                int v = 60;
                int.TryParse(txbDelayedTimeout.Text, out v);
                return v;

            }
            set
            {
                txbDelayedTimeout.Text = value.ToString();
            }
        }
        public int LogLevel
        {
            get
            {
                return cmbLogLevel.SelectedIndex;
            }
            set
            {
                cmbLogLevel.SelectedIndex = value;
            }
        }
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void txbInt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar < '0' || e.KeyChar > '9') e.KeyChar = '\0';
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var pwd=Prompt.ShowDialog("Введите пароль:", "Требуется пароль",true);
            if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
            {
                if (MessageBox.Show("Склад действительно пустой?", "Предупреждение", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    UPMAction.ResetMaterialsLastBag();
                }
            }
        }

        

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                var pwd = Prompt.ShowDialog("Введите пароль:", "Требуется пароль", true);
                if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
                {
                    checkBox1.Checked = true;
                }
            }else{
                checkBox1.Checked = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //new SAPConnect.Logon(SAPHost, SAPInstance, SAPInstanceName, SAPLogin, SAPPassword, SAPClient, "RU", "5");
            try
            {
                Settings.SetOptionValue(Constants.SAPHost, SAPHost);
                Settings.SetOptionValue(Constants.SAPInstance, SAPInstance);
                Settings.SetOptionValue(Constants.SAPInstanceName, SAPInstanceName);
                Settings.SetOptionValue(Constants.SAPClient, SAPClient);
                Settings.SetOptionValue(Constants.SAPLogin, SAPLogin);
                Settings.SetOptionValue(Constants.SAPPassword, SAPPassword);
                Settings.SetOptionValue(Constants.DelayedTimeout, DelayedTimeout.ToString());

                SAPConnect.AppData.ResetInstance();
                var tst = SAPConnect.AppData.Instance.GetTable("MARA", new List<string>() { "MATNR" }, new List<string>() { "MATNR = '1'" });
                MessageBox.Show("Проверка связи прошла успешно");
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                Exception ex1=ex;
                do
                {
                    sb.AppendLine(ex1.Message);
                    sb.AppendLine(ex1.StackTrace);
                    sb.AppendLine("------------------------------");
                    ex1 = ex1.InnerException;
                } while (ex1 != null);
                MessageBox.Show("Ошибка: "+sb.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var psw=Prompt.ShowDialog("Введите старый пароль", "Введите пароль", true);
            var adm_pwd=Settings.GetOptionValue<string>(Constants.AdminPassword);
            if (psw == (adm_pwd ?? ""))
            {
                string pswn = "", pswr = "";
                do
                {
                    pswn = Prompt.ShowDialog("Введите новый пароль", "Введите пароль", true);
                    pswr = Prompt.ShowDialog("Повторите пароль", "Введите пароль", true);
                    if (pswn != pswr)
                    {
                        MessageBox.Show("Пароли не совпадают");
                    }
                } while (pswn != pswr);
                Settings.SetOptionValue(Constants.AdminPassword, pswn);
                MessageBox.Show("Пароль успешно изменен");
                Log.Add("Смена пароля администратора");

            }
            else
            {
                MessageBox.Show("Неверный пароль");
                Log.Add("Введен неверный пароль администратора");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var pwd = Prompt.ShowDialog("Введите пароль:", "Требуется пароль", true);
            if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
            {
                var Material = Prompt.ShowDialog("Введите номер гранулята:", "Гранулят", false);
                if (MessageBox.Show("Склад действительно пустой?", "Предупреждение", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    UPMAction.ResetMaterialsLastBag(Material);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var pwd = Prompt.ShowDialog("Введите пароль", "Введите пароль", true);
            if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
            {
                var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
                long n;
                bool b=false;
                string material;
                do{
                    material = Prompt.ShowDialog("Введите номер материала", "Ввод данных", false);
                    if (String.IsNullOrWhiteSpace(material)) return;
                    material = material.Trim().TrimStart('0');
                    b=long.TryParse(material,out n);
                    if (!b)
                    {
                        MessageBox.Show("Материал должен быть целым числом");
                    }
                    
                    var mn = material.Trim().TrimStart('0').PadLeft(18, '0');
                    var fg = gran.Find(g => g == mn);
                    if (fg == null)
                    {
                        b = false;
                        MessageBox.Show("Загружаемый материал не принадлежит группе материалов \"Гранулят\"");
                    }

                }
                while(!b);

                int nbags = 0;
                do
                {
                    var bags = Prompt.ShowDialog("Введите количество мешков", "Ввод данных", false);
                    if (String.IsNullOrWhiteSpace(bags)) return;
                    b = int.TryParse(bags, out nbags);
                    if (!b)
                    {
                        MessageBox.Show("Количество мешков должно быть целым числом");
                    }
                } while (!b);

                Log.Add("Пользователь выполняет сдвиг мешков для материала "+material+" на "+nbags);
                var LastBag = UPMAction.GetLastBag(material);
                DateTime newlastbag;
                UPMAction.MoveBags(material, nbags, LastBag,out newlastbag);
                UPMAction.SetLastBag(material, newlastbag);

            }
            else
            {
                MessageBox.Show("Неверный пароль");
                Log.Add("Введен неверный пароль администратора");
            }
        }

        private void GranulateLoadButton_Click(object sender, EventArgs e)
        {
            if (UPM == null) return;
            var pwd = Prompt.ShowDialog("Введите пароль", "Введите пароль", true);
            if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
            {
                var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
                long n;
                bool b = false;
                string material;
                do
                {
                    material = Prompt.ShowDialog("Введите номер загружаемого материала", "Ввод данных", false);
                    if (String.IsNullOrWhiteSpace(material)) return;
                    material = material.Trim().TrimStart('0');
                    b = long.TryParse(material, out n);
                    if (!b || n<=0)
                    {
                        b = false;
                        MessageBox.Show("Материал должен быть целым числом");
                    }

                    var mn = material.Trim().TrimStart('0').PadLeft(18, '0');
                    var fg = gran.Find(g => g == mn);
                    if (fg == null)
                    {
                        b = false;
                        MessageBox.Show("Загружаемый материал не принадлежит группе материалов \"Гранулят\"");
                    }

                }
                while (!b);

                int lup = 0;
                do
                {
                    var lups = Prompt.ShowDialog("Введите номер ЛУП", "Ввод данных", false);
                    if (String.IsNullOrWhiteSpace(lups)) return;
                    b = int.TryParse(lups, out lup);
                    if (!b || lup<0)
                    {
                        b = false;
                        MessageBox.Show("Номер ЛУП должен быть целым числом");
                    }
                } while (!b);

                int nbags = 0;
                do
                {
                    var bags = Prompt.ShowDialog("Введите количество загружаемых мешков", "Ввод данных", false);
                    if (String.IsNullOrWhiteSpace(bags)) return;
                    b = int.TryParse(bags, out nbags);
                    if (!b || n<0)
                    {
                        MessageBox.Show("Количество мешков должно быть целым числом");
                    }
                } while (!b);

                var command = new UPMCommand();
                command.NetworkClient = null;
                command.CommandGotAt = DateTime.Now;
                var shift = new Shift(DateTime.Now);
                command.ShiftDate = shift.Date;
                command.IsNightShift = shift.IsNightShift;
                command.Command = UPMCommandType.GranulateLoad;
                command.LUP = lup;
                command.BagQuant = nbags;
                command.Material = material;
                command.MessageID = 0;
                UPM.DelayedActions.Enqueue(new UPMControl.DelayedCommand(command, ""));
                Log.Add("Команда \"" + command.ToString() + "\" поставлена в очередь", true, 1);

            }
            else
            {
                MessageBox.Show("Неверный пароль");
                Log.Add("Введен неверный пароль администратора");
            }
            

        }
    }
}

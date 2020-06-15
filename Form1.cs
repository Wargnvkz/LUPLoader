using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Configuration;

namespace LUPLoader
{
    public partial class Form1 : Form
    {


        bool ShowWarning = true;
        
        public bool UPMStarted = false;
        //protected ControllerDeviceServer TCPServer;
        protected ControllerDeviceClient TCPClient;

        //List<MachineStatus> CurrentStatuses;
        //List<MachineStatus> PreviousStatuses;

        UPMControl UPM;
        //int TCPPortNumber = 5000;

        Shift shift;

        Logs logs;

        bool IsDebugMode=false;

        public Form1()
        {
            InitializeComponent();
            Log.Add("Программа запущена");
            StartStopServer();
            /*PreviousStatuses=new List<MachineStatus>(){ new MachineStatus(){MachineNumber=1,Line=1, LineWork=true},
                                                        new MachineStatus(){MachineNumber=2,Line=1, LineWork=true},
                                                        new MachineStatus(){MachineNumber=3,Line=1, LineWork=true},
                                                        new MachineStatus(){MachineNumber=4,Line=1, LineWork=true},
                                                        new MachineStatus(){MachineNumber=5,Line=1, LineWork=true}
            };
            CurrentStatuses= new List<MachineStatus>(){ new MachineStatus(){MachineNumber=1,Line=1, LineWork=true},
                                                        new MachineStatus(){MachineNumber=2,Line=1, LineWork=true},
                                                        new MachineStatus(){MachineNumber=3,Line=2, LineWork=true},
                                                        new MachineStatus(){MachineNumber=4,Line=1, LineWork=false},
                                                        new MachineStatus(){MachineNumber=5,Line=1, LineWork=true}
            };

            var lst=GetChangedStatuses(CurrentStatuses, PreviousStatuses);*/

            //RunServer();
            DebugMode(true);
            shift = new Shift(DateTime.Now);
        }

        

        /*public List<MachineStatus> GetChangedStatuses(List<MachineStatus> CurrentStatuses, List<MachineStatus> PreviousStatuses)
        {
            return CurrentStatuses.Where(c=>
            {
                var ps=PreviousStatuses.Find(p=>c.MachineNumber==p.MachineNumber);
                if (ps!=null)
                {
                    return c!=ps;
                }else return true;
            }).ToList();
        }*/

        /*public void ConnectToSAP()
        {
            var SAPHost = Settings.GetOptionValue<string>(Constants.SAPHost);
            var SAPInstance = Settings.GetOptionValue<string>(Constants.SAPInstance);
            var SAPInstanceName = Settings.GetOptionValue<string>(Constants.SAPInstanceName);
            var SAPClient = Settings.GetOptionValue<string>(Constants.SAPClient);
            var SAPLogin = Settings.GetOptionValue<string>(Constants.SAPLogin);
            var SAPPassword = Settings.GetOptionValue<string>(Constants.SAPPassword);
            //new SAPConnect.Logon(SAPHost,SAPInstance,SAPInstanceName,SAPLogin,SAPPassword,SAPClient,"RU","5");
            //Log.Add("Установлены параметры соединения с SAP. " + SAPHost + " / " + SAPInstance + " / " + SAPInstanceName + " / " + SAPClient + " / " + SAPLogin);
        }*/

        public bool StartUPM()
        {
            Log.Add("Запуск UPM");
            var port = Settings.GetOptionValue<int>(Constants.TCPServerPortNumber);
            if (port > 0)
            {
                TCPConnectionSettings tcs = new TCPConnectionSettings() { Address = IPAddress.Parse("127.0.0.1"), Port = port, Timeout = 1000 };
                Log.Add("Параметры соединения: "+tcs.Address+":"+tcs.Port);
                UPM = new UPMControl(tcs);
                UPM.Start();
                UPMStarted = true;
                Log.Add("Класс UPM запущен");
                return true;
            }
            else
            {
                UPMStarted = false;
                Log.Add("Неверные настройки сервера");
                return false;
            }
        }

        public bool StopUPM()
        {
            if (UPM != null)
            {
                UPM.Stop();
                UPMStarted = false;
                Log.Add("Класс UPM остановлен");
                return true;
            }
            return false;
        }

        /*public void RunServer()
        {
            var port = GetOptionValue<int>(TCPServerPortNumber);
            if (port > 0)
            {
                TCPConnectionSettings tcs = new TCPConnectionSettings() { Address = IPAddress.Parse("127.0.0.1"), Port = port, Timeout = 1000 };
                TCPServer = new ControllerDeviceServer(tcs);
                TCPServer.Start();
                ServerStarted = true;


                TCPConnectionSettings tcs = new TCPConnectionSettings() { Address = IPAddress.Parse("127.0.0.1"), Port = port, Timeout = 1000 };
                TCPClient = new ControllerDeviceClient(tcs);
                TCPClient.Write(new List<byte>() { 0xff, 0x01, 0x01, 0xff, 0x10 });
            }
            else
            {
                ServerStarted = false;
            }

        }
        public void StopServer()
        {
            //TCPServer.Close();
            //TCPClient.Close();
            //s.Close();
            //c.Close();
        }*/

        protected DateTime LastListOutput = DateTime.MinValue;
        protected DateTime ShowWarningDisable=DateTime.MinValue;
        public Warning FormWarning = null;
        //private bool ShiftChanged = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (UPM == null) return;
            if (UPM.DelayedActionsLastChange > LastListOutput || (LastListOutput==DateTime.MinValue && UPM.DelayedActions.Count!=0))
            {
                var cmdlst = UPM.GetDelayedCommandsList();
                FillListView(cmdlst);
                LastListOutput = DateTime.Now;
            }
            if (ShowWarning)
            {
                ProcessCommandMessage.Visible = false;
                if (FormWarning != null) return;
                if (UPM.HasDelayedCommands())
                {
                    var cmd = UPM.PeekNextDelayedCommands();
                    Log.Add("Пользователю выдано предупреждение. Команда: " + cmd.ToString());
                    FormWarning = new Warning(cmd.ErrorMessage,UPMException.Helper(cmd.MessageType));
                    timer1.Enabled = false;
                    var dialogresult = FormWarning.ShowDialog();
                    timer1.Enabled = true;
                    switch (dialogresult)
                    {
                        case System.Windows.Forms.DialogResult.OK:
                            Log.Add("Пользователь выбрал обработать эту команду");
                            UPM.ProcessNextDelayedAction();
                            break;
                        case System.Windows.Forms.DialogResult.Retry:
                            Log.Add("Пользователь выбрал отложить обработку");
                            ShowWarning = false;
                            Log.Add("Режим отображения предупреждений отключен");
                            ProcessCommandMessage.Visible = true;
                            ShowWarningDisable = DateTime.Now;
                            break;
                        case System.Windows.Forms.DialogResult.Cancel:
                            Log.Add("Пользователь выбрал отменить эту команду");
                            cmd = UPM.TakeNextDelayedCommands();
                            break;
                        default:
                            break;

                    }
                    FormWarning.Close();
                    FormWarning = null;
                    //textBox1.Text = String.Join("\r\n", UPM.Commands.Select(c => c.ToString()));
                }
            }
            else
            {
                if (ShowWarningDisable.AddMinutes(5) < DateTime.Now)
                {
                    Log.Add("Программа автоматически перешла в режим отображения предупреждений");
                    ShowWarning = true;
                }
            }

            Shift sh = new Shift(DateTime.Now);
            if (sh.ShiftStart != shift.ShiftStart)
            {
                shift = sh;
                UPMAction.ChangeShift(shift.Date,shift.IsNightShift,UPMAction.LUPWeight);
            }


            /*s.Write(s.Buffer);
            s.Buffer.Clear();
            textBox1.Text += Encoding.ASCII.GetString(c.Buffer.ToArray());
            c.Buffer.Clear();*/
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.Add("Главное окно закрыто");
            StopUPM();
            if (TCPClient!=null&&TCPClient.IsStarted) TCPClient.Close();
            Log.Add("Програма остановлена");
            Log.Add("-------------------------------------------------------------------");
            Log.Add("",false);
        }

        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            Log.Add("Открыто окно настроек");
            var sf = new SettingsForm();
            sf.UPM = UPM;
            sf.TCPServerPort = Settings.GetOptionValue<int>(Constants.TCPServerPortNumber);
            sf.LogPath = Settings.GetOptionValue<string>(Constants.LogPath);

            sf.SAPHost = Settings.GetOptionValue<string>(Constants.SAPHost);
            sf.SAPInstance = Settings.GetOptionValue<string>(Constants.SAPInstance);
            sf.SAPInstanceName = Settings.GetOptionValue<string>(Constants.SAPInstanceName);
            sf.SAPClient = Settings.GetOptionValue<string>(Constants.SAPClient);
            sf.SAPLogin = Settings.GetOptionValue<string>(Constants.SAPLogin);
            sf.SAPPassword = Settings.GetOptionValue<string>(Constants.SAPPassword);
            sf.DelayedTimeout = Settings.GetOptionValue<int>(Constants.DelayedTimeout);
            sf.LogLevel= Settings.GetOptionValue<int>(Constants.LogLevel);


            Log.Add("Значения настроек: "+sf.ToString());

            var res=sf.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                Settings.SetOptionValue(Constants.TCPServerPortNumber, sf.TCPServerPort.ToString());
                Settings.SetOptionValue(Constants.LogPath, sf.LogPath);

                Settings.SetOptionValue(Constants.SAPHost, sf.SAPHost);
                Settings.SetOptionValue(Constants.SAPInstance, sf.SAPInstance);
                Settings.SetOptionValue(Constants.SAPInstanceName, sf.SAPInstanceName);
                Settings.SetOptionValue(Constants.SAPClient, sf.SAPClient);
                Settings.SetOptionValue(Constants.SAPLogin, sf.SAPLogin);
                Settings.SetOptionValue(Constants.SAPPassword, sf.SAPPassword);
                Settings.SetOptionValue(Constants.DelayedTimeout, sf.DelayedTimeout.ToString());
                Settings.SetOptionValue(Constants.LogLevel, sf.LogLevel.ToString());

                SAPConnect.AppData.ResetInstance();
                //ConnectToSAP();
                Log.Add("Новое значение настроек: " + sf.ToString());
                IsDebugMode = sf.IsDebug;
            }
            sf.Close();
            DebugMode();
        }

        private void StartMenuItem_Click(object sender, EventArgs e)
        {
            Log.Add("Нажата кнопка остановки/запуска сервера");
            StartStopServer();
            Log.Add("Сервер " + (UPMStarted ? "Работает" : "Остановлен"));
        }

        protected void StartStopServer()
        {
            Log.Add("Запуск/остановка сервера");
            if (!UPMStarted)
            {
                Log.Add("Запуск сервера");
                if (StartUPM())
                {
                    StartMenuItem.Text = "Остановить сервер";
                    Log.Add("Сервер запущен");
                }
                else
                {
                    Log.Add("Сервер не запущен");
                }

            }
            else
            {
                if (StopUPM())
                {
                    StartMenuItem.Text = "Запустить сервер";
                    Log.Add("Сервер остановлен");
                }
                else
                {
                    Log.Add("Не получилось остановить сервер");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var port = Settings.GetOptionValue<int>(Constants.TCPServerPortNumber);
            if (port > 0)
            {
                TCPConnectionSettings tcs = new TCPConnectionSettings() { Address = IPAddress.Parse("127.0.0.1"), Port = port, Timeout = 1000 };
                TCPClient = new ControllerDeviceClient(tcs);
                TCPClient.Start();

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (TCPClient!=null)
            if (TCPClient.IsStarted)
            {
                TCPClient.Close();
            }

        }

        public byte ID = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            if (TCPClient == null) return;
            Random rnd = new Random();
            var index = domainUpDown1.SelectedIndex;
            switch (index)//rnd.Next(3))
            {
                case 0:
                    {
                        var a = rnd.Next(60000) + 256;
                        var b = rnd.Next(60000) + 256;
                        byte c1 = (byte)(a);
                        byte d1 = (byte)(a >> 8);
                        byte c2 = (byte)(b);
                        byte d2 = (byte)(b >> 8);
                        TCPClient.Write(new List<byte>() { 0xff,ID, 0x01, c1, d1, c2, d2 });
                        break;
                    }
                case 1:
                    {
                        byte a = (byte)(rnd.Next(2) + 1);
                        byte b = (byte)(rnd.Next(12) + 1);
                        byte c = (byte)(rnd.Next(3));
                        TCPClient.Write(new List<byte>() { 0xff,ID, 0x02, a, b, c });
                        break;
                    }
                case 2:
                    {
                        byte a = (byte)(rnd.Next(2) + 1);

                        byte b = 1;// (byte)(rnd.Next(2) + 1);
                        byte c = 12;// (byte)(rnd.Next(500));
                        var dt = new List<byte>() { 0xff, ID, 0x03, a };
                        dt.AddRange(Encoding.ASCII.GetBytes("1000000"+c.ToString("D3")));
                        dt.Add(b);
                        TCPClient.Write(dt);
                        break;
                    }
                case 3:
                    {
                        //byte a = (byte)(rnd.Next(2));
                        /*byte a=0;
                        var _8=new TimeSpan(8,0,0);
                        var _20=new TimeSpan(20,0,0);
                        var now=DateTime.Now.AddDays(-1);
                        if (now.TimeOfDay < _8) { a = 1; now = now.AddDays(-1); }
                        if (now.TimeOfDay >= _20) { a = 1; }
                        
                        var dt = new List<byte>() { 0xff, ID, 0x04, (byte)now.Day,(byte)now.Month,(byte)(now.Year-2000),a };*/
                        var dt = new List<byte>() { 0xFF, 0x20, 0x04, 0x09, 0x04, 0x14, 0x00 };
                        TCPClient.Write(dt);
                        break;

                    }
                case 4:
                    {
                        Shift s = new Shift(DateTime.Now);
                        var d = (byte)s.ShiftStart.Day;
                        var m = (byte)s.ShiftStart.Month;
                        var y = (byte)(s.ShiftStart.Year%100);
                        var n = s.IsNightShift?(byte)0x01:(byte)0x00;
                        var dt = new List<byte>{0xFF,0x16,0x05,d,m,y,n,
                                                0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x31,0x34,  0x4C,0x04, 0x14,0x00, 0x0D,0x00, 0x07,0x00, 0x02,0x00,  0x05,  0xD2,0xE5,0xF1,0xF2,0x31,
                                                0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x31,0x34,  0x1A,0x04, 0x0A,0x00, 0x06,0x00, 0x04,0x00, 0xFE,0xFF,  0x05,  0xD2,0xE5,0xF1,0xF2,0x32,
                                                0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x32,0x30,  0x4C,0x04, 0x0C,0x00, 0x04,0x00, 0x08,0x00, 0x00,0x00,  0x00,
                                                0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x39,0x33,  0x4C,0x04, 0x08,0x00, 0x03,0x00, 0x05,0x00, 0x02,0x00,  0x00};
                        TCPClient.Write(dt);
                        break;
                    }
                case 5:
                    {
                        Shift s = new Shift(DateTime.Now);
                        var d = (byte)s.ShiftStart.Day;
                        var m = (byte)s.ShiftStart.Month;
                        var y = (byte)(s.ShiftStart.Year % 100);
                        var n = s.IsNightShift ? (byte)0x01 : (byte)0x00;
                        var dt = new List<byte>{0xFF,0x16,0x06,d,m,y,n,
                                                0x01,  0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x31,0x34,  0x20,0x4E,0x00,0x00,  0x10,0x27,0x00,0x00,  0x98,0x3A,0x00,0x00,  0x98,0x3A,0x00,0x00,  0x02,0x00,  0x05,  0xD2,0xE5,0xF1,0xF2,0x31,
                                                0x01,  0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x31,0x32,  0x00,0x00,0x00,0x00,  0x10,0x27,0x00,0x00,  0x00,0x00,0x00,0x00,  0x10,0x27,0x00,0x00,  0xFE,0xFF,  0x05,  0xD2,0xE5,0xF1,0xF2,0x32,
                                                0x02,  0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x32,0x30,  0x10,0x27,0x00,0x00,  0x50,0xC3,0x00,0x00,  0x40,0x9c,0x00,0x00,  0x20,0x4e,0x00,0x00,  0x00,0x00,  0x00,
                                                0x03,  0x31,0x30,0x30,0x30,0x30,0x30,0x30,0x30,0x39,0x33,  0x00,0x00,0x00,0x00,  0x7c,0x15,0x00,0x00,  0x00,0x00,0x00,0x00,  0x7c,0x15,0x00,0x00,  0x05,0x00,  0x00};
                        TCPClient.Write(dt);
                        break;
                    }
            }
            ID++;

        }


        protected void FillListView(List<UPMControl.DelayedCommand> lst)
        {
            listView1.Items.Clear();
            for (int i = 0; i < lst.Count; i++)
            {
                listView1.Items.Add(new ListViewItem(new string[] { (i + 1).ToString(), lst[i].CommandDateTime.ToString("dd-MM-yyyy HH\\:mm\\:ss"), lst[i].Command.ToString(), lst[i].ErrorMessage }));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Log.Add("Пользователь выбрал выполнить следующую команду");
            ShowWarning = true;
            ProcessCommandMessage.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var pwd = Prompt.ShowDialog("Введите пароль:", "Требуется пароль", true);
            if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
            {
                if (UPM != null)
                {
                    var cmd = UPM.TakeNextDelayedCommands();
                    if (cmd != null)
                        Log.Add("Пользователь выбрал отменить команду \"" + cmd.ToString() + "\"");
                }
            }
            
        }

        private void OpenLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (logs == null)
                {
                    logs = new Logs();
                }
                if (logs.Visible)
                {
                    logs.Hide();
                    OpenLogToolStripMenuItem.Text = "Показать лог";
                }
                else
                {
                    logs.Show();
                    OpenLogToolStripMenuItem.Text = "Скрыть лог";
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
                if (logs != null)
                {
                    try
                    {
                        logs.Close();
                    }
                    catch (Exception)
                    {
                    }
                    logs = null;
                }
            }
        }

        public void DebugMode(bool start=false)
        {
            if (start)
            {
                panel1.Visible = false;
                this.Size = new Size(this.Size.Width, this.Size.Height - panel1.Size.Height);
                listView1.Size = new Size(listView1.Size.Width, listView1.Size.Height + panel1.Size.Height);
            }
            else
            {
                if (IsDebugMode)
                {
                    if (!panel1.Visible)
                    {
                        panel1.Visible = true;
                        this.Size = new Size(this.Size.Width, this.Size.Height + panel1.Size.Height);
                        listView1.Size = new Size(listView1.Size.Width, listView1.Size.Height - panel1.Size.Height);
                    }
                }
                else
                {
                    if (panel1.Visible)
                    {
                        panel1.Visible = false;
                        this.Size = new Size(this.Size.Width, this.Size.Height - panel1.Size.Height);
                        listView1.Size = new Size(listView1.Size.Width, listView1.Size.Height + panel1.Size.Height);
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var sdt = Prompt.ShowDialog("Введите время последнего мешка", "Введите дату и время", false);
            DateTime dt;
            if (!DateTime.TryParse(sdt,out dt))
            {
                MessageBox.Show("Неверный формат даты/времени");
            }else{
                UPMAction.ResetMaterialsLastBag(dt);
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                var pwd = Prompt.ShowDialog("Введите пароль:", "Требуется пароль", true);
                if (pwd == Settings.GetOptionValue<string>(Constants.AdminPassword))
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }


    }

    public class Shift
    {
        public DateTime Date;
        public bool IsNightShift;
        public DateTime ShiftStart;
        public Shift()
        {
        }
        public Shift(DateTime DT)
        {
            var _8 = new TimeSpan(8, 0, 0);
            var _20 = new TimeSpan(20, 0, 0);
            var tm = DT.TimeOfDay;
            if (tm >= _8 && tm < _20)
            {
                this.Date = DT.Date;
                this.IsNightShift = false;

            }
            else
            {
                if (tm < _8)
                {
                    this.Date = DT.Date.AddDays(-1);
                    this.IsNightShift = true;

                }
                else
                {
                    this.Date = DT.Date;
                    this.IsNightShift = true;
                }
            }
            this.ShiftStart = this.Date.AddHours(this.IsNightShift ? 20 : 8);
        }
    }

}

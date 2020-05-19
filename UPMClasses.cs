using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UPM.Classes;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using System.Globalization;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace LUPLoader
{
    public static class Constants
    {
        public static string MachineNumberSetttings = "MachineNumber";
        public static string TCPServerPortNumber = "TCPServerPort";
        public static string LogPath = "LogPath";
        public static string AdminPassword = "AdminPassword";
        public static string SAPHost = "Server";
        public static string SAPInstance = "Instance";
        public static string SAPInstanceName = "InstanceName";
        public static string SAPClient = "Client";
        public static string SAPLogin = "Login";
        public static string SAPPassword = "Password";
        public static string DelayedTimeout = "DelayedTimeout";
        public static string LogLevel = "LogLevel";
    }

    public class ControllerDeviceClient : IODevice<TCPConnectionSettings>
    {
        TcpClient Client;
        TCPConnectionSettings Settings;
        List<byte> Buffer = new List<byte>();
        string error;
        public ControllerDeviceClient(TCPConnectionSettings settings)
            : base(settings)
        {
            ReadEvent += REvent;
            WriteEvent += WEvent;
        }
        protected override void WriteToDevice(List<byte> data)
        {
            if (Client.Connected)
            {
                Client.GetStream().Write(data.ToArray(), 0, data.Count);
            }
        }
        protected override void ReadFromDevice()
        {
            if (Client.Connected)
            {
                List<byte> dt = new List<byte>();
                byte[] buf = new byte[2048];
                var ns = Client.GetStream();
                while (ns.DataAvailable)
                {
                    var sz = ns.Read(buf, 0, 2048);
                    dt.AddRange(buf.Take(sz));
                }
                Buffer.AddRange(dt);
            }
        }


        protected override void DeviceSetup(TCPConnectionSettings settings)
        {
            Settings = settings;
            Client = new TcpClient();
            Client.ReceiveTimeout = settings.Timeout;
            Client.SendTimeout = settings.Timeout;
        }

        protected override void OpenDevice()
        {
            Client.Connect(Settings.Address, Settings.Port);
        }

        protected override void CloseDevice()
        {
            if (Client.Connected)
                Client.Close();
        }

        public void REvent(List<byte> data, Exception ex)
        {
            if (ex != null)
            {
                error = ex.Message;
            }
        }
        public void WEvent(List<byte> data, Exception ex)
        {
            if (ex != null)
            {
                error = ex.Message;
            }
        }
    }
    
    public class ControllerDeviceServer : IODevice<TCPConnectionSettings>,IDisposable
    {
        TCPServer Server;
        TCPConnectionSettings Settings;
        private List<TCPConnectedClient> _Clients = new List<TCPConnectedClient>();

        public List<TCPConnectedClient> Clients
        {
            get
            {
                lock (_lockClients)
                {
                    RemoveDisconnectedClients();
                    var cl = new List<TCPConnectedClient>();
                    cl.AddRange(_Clients);
                    return cl;
                }
            }
        }

        public ControllerDeviceServer(TCPConnectionSettings settings)
            : base(settings)
        {
            Settings = settings;
        }
        protected override void WriteToDevice(List<byte> data=null)
        {
            
        }

        
        protected override void ReadFromDevice()
        {
            lock (_lockClients)
            {
                RemoveDisconnectedClients();
                foreach (var client in Clients)
                {
                    if (client != null && client.Connection.Connected)
                    {
                        List<byte> dt = new List<byte>();
                        byte[] buf = new byte[2048];
                        var ns = client.Connection.GetStream();
                        while (ns.DataAvailable)
                        {
                            var sz = ns.Read(buf, 0, 2048);
                            dt.AddRange(buf.Take(sz));
                        }
                        client.AddData(dt);
                        if (dt.Count!=0)
                            Log.Add("Получено от клиента " + client.Connection.Client.RemoteEndPoint.ToString() + ": <"+Log.ByteArrayToHexString(dt.ToArray())+">",true,2);

                    }
                }
            }
        }


        protected override void DeviceSetup(TCPConnectionSettings settings)
        {
            Server = new TCPServer(settings);
            Server.ConnectionEstablishedEvent += TCPClientConnected;
        }

        protected override void OpenDevice()
        {
            Server.Start();
        }

        protected override void CloseDevice()
        {
            Server.Stop();
        }

        protected void TCPClientConnected(TcpClient client)
        {
            var tc = new TCPConnectedClient(client);
            client.ReceiveTimeout = Settings.Timeout;
            client.SendTimeout = Settings.Timeout;
            AddClient(tc);
            Log.Add("Подключился клиент "+client.Client.RemoteEndPoint.ToString(),true,0);
        }

        public override void Dispose()
        {
            Server.Dispose();
            base.Dispose();
        }

        protected object _lock = new object();
        protected object _lockClients = new object();
        public void RemoveDisconnectedClients()
        {
            lock (_lock)
            {
                _Clients.RemoveAll(c => !c.Connection.Connected);
            }
        }

        public void AddClient(TCPConnectedClient client)
        {
            lock (_lockClients)
            {
                _Clients.Add(client);
            }
        }

    }

    public class TCPServer:IDisposable
    {
        TcpListener Server;
        Thread TCPServerThread;
        public List<TcpClient> Clients = new List<TcpClient>();
        public delegate void ConnectionEstablished(TcpClient client);
        public event ConnectionEstablished ConnectionEstablishedEvent;
        private bool ThreadWorking = true;

        public TCPServer(TCPConnectionSettings settings)
        {
            Server = new TcpListener(settings.Address, settings.Port);
            TCPServerThread = new Thread(TCPServerProc);
        }

        protected void TCPServerProc(object p)
        {
            while (ThreadWorking)
            {
                if (Server.Pending())
                {
                    var TcpClient = Server.AcceptTcpClient();
                    Clients.Add(TcpClient);
                    var cee = ConnectionEstablishedEvent;
                    if (cee!= null)
                    {
                        cee(TcpClient);
                    }
                }
                Clients.RemoveAll(c => !c.Connected);
                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            if (TCPServerThread != null)
            {
                if (TCPServerThread.ThreadState == ThreadState.WaitSleepJoin)
                    TCPServerThread.Interrupt();
                else if (TCPServerThread.ThreadState == ThreadState.Running)
                    TCPServerThread.Abort();
            }
            Server.Stop();
        }

        public void Start()
        {
            ThreadWorking = true;
            Server.Start();
            TCPServerThread.Start();
        }
        public void Stop()
        {
            ThreadWorking = false;
            Server.Stop();
        }
    }

    public class TCPConnectedClient
    {
        public TcpClient Connection;
        public List<byte> Buffer=new List<byte>();
        public List<UPMCommand> Commands = new List<UPMCommand>();
        protected object _lock = new object();
        protected object _lockCmd = new object();
        public TCPConnectedClient(TcpClient client)
        {
            Connection = client;
        }
        public void AddData(List<byte> data)
        {
            lock (_lock)
            {
                Buffer.AddRange(data);
            }
        }
        public List<byte> TakeData(int count)
        {
            List<byte> data;
            lock (_lock)
            {
                if (Buffer.Count >= count)
                {
                    data = Buffer.GetRange(0, count);
                    Buffer.RemoveRange(0, count);
                    return data;
                }
                else
                {
                    return new List<byte>();
                }
            }
        }
        public void AddCommand(UPMCommand cmd)
        {
            lock (_lockCmd)
            {
                Commands.Add(cmd);
            }
        }
        public UPMCommand GetCommand()
        {
            lock (_lockCmd)
            {
                if (Commands.Count > 0)
                {
                    var cmd = Commands[0];
                    Commands.RemoveAt(0);
                    return cmd;
                }
                else return null;
            }
        }
    }

    public class TCPConnectionSettings
    {
        public IPAddress Address;
        public int Port;
        public int Timeout;
    }

    public class UPMData
    {
        
        /*public MachineStatus[] MachineStatuses;
        public double[] LUPWeights;
        public SAPConnectionStatus SAPStatus;
        public UPMConnectionStatus UPMStatus;*/
    }

    /*public class SAPConnectionStatus
    {
        public MachineStatus LastAction;
        public bool IsLastActionSuccessful;
    }*/

    /*public class UPMConnectionStatus
    {
        public UPMCommand LastCommand;
        public bool ResponseSent;
    }*/

    [DataContract]
    public class UPMCommand
    {
        [DataMember]
        public UPMCommandType Command;
        [DataMember]
        public int[] LUPWeight=new int[2];
        [DataMember]
        public int LUP;
        [DataMember]
        public int PL;
        [DataMember]
        public string Material;
        [DataMember]
        public int BagQuant;
        [DataMember]
        public bool PLLineWork;
        [DataMember]
        public byte MessageID;
        [DataMember]
        public byte Status;
        [DataMember]
        public DateTime CommandGotAt;
        [DataMember]
        public DateTime ShiftDate=new DateTime(2000,1,1);
        [DataMember]
        public bool IsNightShift=false;
        [DataMember]
        public List<Correction> Corrections;
        public override string ToString()
        {
            switch(Command){
                case UPMCommandType.LUPWeight:
                    return String.Format("Вес в LUP: ID:{2} LUP1:{0}, LUP2:{1}", LUPWeight[0], LUPWeight[1], MessageID);
                case UPMCommandType.MachineStatus:
                    return String.Format("Статус линии: ID:{3}, PL:{0}, LUP:{1}, Линия:{2}", PL, LUP, (Status==2?"LUP3":(PLLineWork?"Работает":"Остановлена")),MessageID);
                case UPMCommandType.GranulateLoad:
                    return String.Format("Загрузка гранулята: ID:{3}, LUP:{0}, Материал:{1}, Мешков:{2}", LUP, Material,BagQuant,MessageID);
                case UPMCommandType.UPMIncome:
                    return String.Format("Запрос прихода гранулята на УПМ: ID:{0}, Смена:{1}",MessageID,ShiftDate.ToShortDateString()+(IsNightShift?"Н":"Д"));
                case UPMCommandType.EndShiftCorrection:
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Данные о поправках:");
                        foreach (var corr in Corrections)
                        {
                            sb.Append(String.Format("(Материал: {0} Упаковка: {1} Мешков: {2} Поправка: {3}{4})",corr.Material,corr.BagWeight,corr.BagQuantity,corr.CorrectionValue,String.IsNullOrWhiteSpace(corr.CorrectionText)?"":"("+corr.CorrectionText+")"));
                        }
                        return sb.ToString();
                    }
            }
            return "";
        }
        public TCPConnectedClient NetworkClient;

        public byte[] ResponseOK()
        {
            var resp = new byte[3]{0xff,0,0};
            resp[1]=MessageID;
            return resp;
        }
        public byte[] ResponseErr()
        {
            var resp = new byte[3] { 0xff, 0, 1 };
            resp[1] = MessageID;
            return resp;
        }
        public byte[] Response(byte[] data)
        {
            var resp = new byte[2] { 0xff, 0 };
            resp[1] = MessageID;
            resp=resp.Concat(data).ToArray();
            return resp;
        }
    }

    [Serializable]
    [DataContract]
    public class Correction
    {
        [DataMember]
        public string Material;
        [DataMember]
        public int BagWeight;
        [DataMember]
        public int BagQuantity;
        [DataMember]
        public short CorrectionValue;
        [DataMember]
        public string CorrectionText;
    }

    public enum UPMCommandType
    {
        LUPWeight,
        MachineStatus,
        GranulateLoad,
        UPMIncome,
        EndShiftCorrection
    }

    /*public class MachineStatus
    {
        public int MachineNumber;
        public int Line;
        public bool LineWork;

        public static bool operator ==(MachineStatus s1, MachineStatus s2)
        {
            if (object.ReferenceEquals(s1, null))
            {
                return object.ReferenceEquals(s2, null);
            }
            else
            {
                if (object.ReferenceEquals(s2, null)) return false;
            }

            if (s1.MachineNumber == s2.MachineNumber)
            {
                return s1.Line == s2.Line && s1.LineWork == s2.LineWork;
            }
            else return false; // как сравнить то, что нельзя сравнивать?
        }
        public static bool operator !=(MachineStatus s1, MachineStatus s2)
        {
            return !(s1 == s2);
        }
    }*/


    public class UPMControl : MainControl<UPMData>
    {
        ControllerDeviceServer tcpserver;
        //ControllerDeviceClient tcpclient;
        TCPConnectionSettings ConnectionSettings;
        //public List<UPMCommand> Commands=new List<UPMCommand>();

        public DiskQueue<DelayedCommand> DelayedActions = new DiskQueue<DelayedCommand>("DelayedActions");
        public DateTime DelayedActionsLastChange = DateTime.MinValue;

        public UPMControl(TCPConnectionSettings connectionSettings)
        {
            ConnectionSettings = connectionSettings;
            tcpserver = new ControllerDeviceServer(connectionSettings);
            //tcpclient = new ControllerDeviceClient(connectionSettings);
        }

        protected override void ReadData(ref UPMData data)
        {
            UPMCommand cmd;

            foreach (var client in tcpserver.Clients)
            {
                var ns = client.Connection.GetStream();
                while (UPMParseCommand(client, out cmd))
                {
                    Log.Add("Получена команда \"" + cmd.ToString() + "\" от клиента " + client.Connection.Client.RemoteEndPoint.ToString(), true, 1);
                    client.AddCommand(cmd);
                    //var resp = cmd.ResponseOK();
                    //ns.Write(resp, 0, resp.Length);
                    //Log.Add("Ответ клиенту " + client.Connection.Client.RemoteEndPoint.ToString() + ": <" + Log.ByteArrayToHexString(resp) + "> - \"OK\"", true, 2);
                    //Log.Add("Respond OK sent to client at " + client.Connection.Client.RemoteEndPoint.ToString());
                    Report.AddCommand(cmd);
                    try
                    {
                        if (DelayedActions.Count == 0 || cmd.Command != UPMCommandType.GranulateLoad)
                        {
                            if (!UPMAction.ProcessCommand(ref cmd))
                            {
                                DelayedActions.Enqueue(new DelayedCommand(cmd, ""));
                                DelayedActionsLastChange = DateTime.Now;
                                Log.Add("Команда \"" + cmd.ToString() + "\" не может быть выполнена и была отложена", true, 0);
                            }
                        }
                        else
                        {
                            DelayedActions.Enqueue(new DelayedCommand(cmd, ""));
                            DelayedActionsLastChange = DateTime.Now;
                            Log.Add("Команда \"" + cmd.ToString() + "\" была поставлена в очередь до выполнения предыдущей команды", true, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        DelayedActions.Enqueue(new DelayedCommand(cmd, ex));
                        DelayedActionsLastChange = DateTime.Now;
                        if (ex is UPMException)
                        {
                            Log.Add("Команда \"" + cmd.ToString() + "\" была отложена с сообщением \"" + ex.Message + "\"", true, 0);
                        }
                        else
                        {
                            Log.Add("При выполнении команды \"" + cmd.ToString() + "\" возникло исключение " + ex.Message + " " + ex.StackTrace, true, 0);
                        }
                    }
                }

            }
        }

        public bool HasDelayedCommands()
        {
            return !DelayedActions.IsEmpty;
        }
        public List<DelayedCommand> GetDelayedCommandsList()
        {
            return DelayedActions.ToList();
        }

        public DelayedCommand PeekNextDelayedCommands()
        {
            DelayedCommand cmd;
            if (DelayedActions.TryPeek(out cmd))
            {
                return cmd;
            }
            return null;

        }
        public DelayedCommand TakeNextDelayedCommands()
        {
            DelayedCommand cmd;
            if (DelayedActions.TryDequeue(out cmd))
            {
                DelayedActionsLastChange = DateTime.Now;
                return cmd;
            }
            return null;

        }

        public void ProcessNextDelayedAction()
        {
            DelayedCommand cmd_str;
            if (DelayedActions.TryPeek(out cmd_str))
            {
                try
                {
                    if (UPMAction.ProcessCommand(ref cmd_str.Command))
                    {
                        DelayedActionsLastChange = DateTime.Now;
                        DelayedActions.TryDequeue(out cmd_str);
                        Log.Add("Команда \"" + cmd_str.Command.ToString() + "\" выполнена успешно", true, 0);

                    }
                }
                catch (Exception ex)
                {
                    cmd_str.ErrorMessage = ex.Message;
                    cmd_str.StackTrace = ex.StackTrace;
                    if (ex is UPMException)
                    {
                        cmd_str.IsUPMException = true;
                        cmd_str.MessageType = (ex as UPMException).UPMMessage;
                    }
                    // Либо добиваем первую команду и тормозим остальные, пока не выполнится
                    //DelayedActions.ReplaceFirst(cmd_str);
                    //либо убираем не получившуюся из начала и ставим в конец
                    DelayedCommand cmd_str1;
                    DelayedActions.TryDequeue(out cmd_str1);
                    cmd_str.LastTry = DateTime.Now;
                    DelayedActions.Enqueue(cmd_str);
                    if (ex is UPMException)
                    {
                        Log.Add("Команда \"" + cmd_str.Command.ToString() + "\" была отложена с сообщением \"" + ex.Message + "\"", true, 0);
                    }
                    else
                    {
                        Log.Add("При выполнении команды \"" + cmd_str.Command.ToString() + "\" возникло исключение " + ex.Message + " " + ex.StackTrace, true, 0);
                    }
                }

            }
        }

        [DataContract]
        public class DelayedCommand
        {
            [DataMember]
            public UPMCommand Command;
            [DataMember]
            public string ErrorMessage;
            [DataMember]
            public string StackTrace;
            [DataMember]
            public readonly DateTime CommandDateTime = DateTime.Now;
            [DataMember]
            public DateTime LastTry = DateTime.Now;
            [DataMember]
            public bool IsUPMException = false;
            [DataMember]
            public UPMExceptionMessage MessageType;

            public DelayedCommand(UPMCommand cmd, string message, UPMExceptionMessage MsgType = UPMExceptionMessage.NotUPMError)
            {
                Command = cmd;
                ErrorMessage = message;
                IsUPMException = true;
                MessageType = MsgType;
            }
            public DelayedCommand(UPMCommand cmd, Exception ex)
            {
                Command = cmd;
                ErrorMessage = ex.Message;
                StackTrace = ex.StackTrace;
                if (ex is UPMException) { IsUPMException = true; MessageType = (ex as UPMException).UPMMessage; }
            }
            public override string ToString()
            {
                return Command.ToString() + ". Время: " + CommandDateTime.ToString("dd-MM-yyyy HH\\:mm\\:ss") + ". Сообщение: " + ErrorMessage;
            }
        }

        protected override void WriteData(ref UPMData data)
        {
        }
        protected override void CalculationProcedure(ref UPMData data)
        {
            //data.LUPWeights
        }
        protected override void Start(ref UPMData data)
        {
            tcpserver.Start();
            //tcpclient.Start();
        }
        protected override void Stop(ref UPMData data)
        {
            tcpserver.Close();
            //tcpclient.Close();
        }
        protected bool UPMParseCommand(TCPConnectedClient client, out UPMCommand command)
        {
            var data = client.Buffer;
            var StartI = data.IndexOf(0xff);
            if (StartI < 0) { command = null; return false; }
            if (data.Count < StartI + 3) { command = null; return false; }
            var MsgID = data[StartI + 1];
            try
            {
                bool HasResult = false;
                int MsgLength = 0;
                command = new UPMCommand();
                command.NetworkClient = client;
                command.CommandGotAt = DateTime.Now;
                switch (data[StartI + 2])
                {
                    case 1:
                        {
                            MsgLength = 7;
                            if (data.Count >= StartI + MsgLength)
                            {
                                byte lWeight1 = data[StartI + 3];
                                byte hWeight1 = data[StartI + 4];
                                byte lWeight2 = data[StartI + 5];
                                byte hWeight2 = data[StartI + 6];
                                int Weight1 = hWeight1 * 256 + lWeight1;
                                int Weight2 = hWeight2 * 256 + lWeight2;
                                HasResult = true;
                                command.Command = UPMCommandType.LUPWeight;
                                command.LUPWeight[0] = Weight1;
                                command.LUPWeight[1] = Weight2;
                                command.MessageID = MsgID;
                            }
                            else
                            {
                                MsgLength = 0;
                            }
                        }
                        break;
                    case 2:
                        {
                            MsgLength = 6;
                            if (data.Count >= StartI + MsgLength)
                            {
                                byte lup = data[StartI + 3];
                                byte pl = data[StartI + 4];
                                byte Status = data[StartI + 5];
                                HasResult = true;
                                command.Command = UPMCommandType.MachineStatus;
                                command.LUP = lup;
                                command.PL = pl;
                                command.MessageID = MsgID;
                                command.Status = Status;
                                switch (Status)
                                {
                                    case 0:
                                        command.PLLineWork = false;
                                        break;
                                    case 1:
                                        command.PLLineWork = true;
                                        break;
                                    case 2:
                                        command.PLLineWork = true;
                                        break;
                                }
                            }
                            else
                            {
                                MsgLength = 0;
                            }

                        }
                        break;
                    case 3:
                        {
                            MsgLength = 15;
                            if (data.Count >= StartI + MsgLength)
                            {
                                byte lup = data[StartI + 3];
                                byte bagquant = data[StartI + 14];
                                var material = data.GetRange(StartI + 4, 10).ToArray();
                                var materialNumber = Encoding.ASCII.GetString(material);
                                HasResult = true;
                                command.Command = UPMCommandType.GranulateLoad;
                                command.LUP = lup;
                                command.BagQuant = bagquant;
                                command.Material = materialNumber;
                                command.MessageID = MsgID;
                                var shift = new Shift(DateTime.Now);
                                command.ShiftDate = shift.Date;
                                command.IsNightShift = shift.IsNightShift;

                            }
                            else
                            {
                                MsgLength = 0;
                            }

                        }
                        break;
                    case 4:
                        {
                            MsgLength = 7;
                            if (data.Count >= StartI + MsgLength)
                            {
                                byte day = data[StartI + 3];
                                byte month = data[StartI + 4];
                                byte year = data[StartI + 5];
                                byte shift = data[StartI + 6];

                                /*var material = data.GetRange(StartI + 4, 10).ToArray();
                                var materialNumber = Encoding.ASCII.GetString(material);*/
                                HasResult = true;
                                command.Command = UPMCommandType.UPMIncome;
                                command.ShiftDate = new DateTime(year+2000, month, day);
                                command.IsNightShift = shift == 1;
                                command.MessageID = MsgID;

                            }
                            else
                            {
                                MsgLength = 0;
                            }

                        }
                        break;
                    case 5:
                        {
                            if (data.Count >= StartI + MsgLength)
                            {
                                byte day = data[StartI + 3];
                                byte month = data[StartI + 4];
                                byte year = data[StartI + 5];
                                byte shift = data[StartI + 6];

                                /*var material = data.GetRange(StartI + 4, 10).ToArray();
                                var materialNumber = Encoding.ASCII.GetString(material);*/
                                HasResult = true;
                                command.Command = UPMCommandType.EndShiftCorrection;
                                command.ShiftDate = new DateTime(year+2000, month, day);
                                command.IsNightShift = shift == 1;
                                command.MessageID = MsgID;

                                command.Corrections = new List<Correction>();
                                int DataOffset = 7;
                                do
                                {
                                    var corr = new Correction();

                                    //10 byte StartI+DataOffset+0
                                    var arr = data.ToArray();
                                    var material = data.GetRange(StartI + DataOffset + 0, 10).ToArray();
                                    var materialNumber = Encoding.ASCII.GetString(material);
                                    corr.Material = materialNumber;

                                    corr.BagWeight = BitConverter.ToInt16(arr, StartI + DataOffset + 10);
                                    //corr.BagWeight = data[StartI + DataOffset + 11] + 256 * data[StartI + DataOffset + 12];

                                    corr.BagQuantity = BitConverter.ToInt16(arr, StartI + DataOffset + 12);
                                    //corr.BagQuantity = data[StartI + DataOffset + 13] + 256 * data[StartI + DataOffset + 14];
                                    
                                    corr.CorrectionValue = BitConverter.ToInt16(arr, StartI + DataOffset + 14);
                                    //corr.CorrectionData = (short)((UInt16)data[StartI + DataOffset + 15] | ((UInt16)data[StartI + DataOffset + 16]) << 8);
                                    var textlength = data[StartI + DataOffset + 16];
                                    var btext = data.GetRange(StartI + DataOffset + 17, textlength).ToArray();
                                    corr.CorrectionText = Encoding.GetEncoding(1251).GetString(btext);
                                    command.Corrections.Add(corr);
                                    DataOffset += 17 + textlength;
                                } while (data.Count>StartI+DataOffset&&data[StartI+DataOffset]!=0xff);
                                MsgLength = DataOffset;
                            }
                            else
                            {
                                MsgLength = 0;
                            }

                        }
                        break;
                }

                client.TakeData(StartI + MsgLength);

                return HasResult;
            }
            catch (Exception ex)
            {
                var StartINext = data.IndexOf(0xff, StartI+1);
                if (StartINext > 0)
                {
                    client.TakeData(StartINext - 1);
                }
                command = null; 
                return false;
            }
        }
    }

    public static class UPMAction
    {
        public static int[] LUPWeight = new int[2];

        public static bool LoadBags(int LUP, int BagQuant, LUPLastBag LastBag, out LUPLastBag newLastBag)
        {
            ///проверить наличие нужного количества материала на складе, даже если мешки уже привезли
            var Material = LastBag.Material;

            var Storage = GetStorageMaterial();
            //int LUP = 1;
            //var BagQuant = 10;
            //Material = "1000000012"
            var lastbag = LastBag;// new DateTime(2018, 12, 06);
            newLastBag = LastBag;
            var a = HU_At_UPM(Material, lastbag.LastBag, lastbag.LastTransferOrder );
            if (a.Count >= BagQuant)
            {
                Dictionary<string, double> BatchQauntity = new Dictionary<string, double>();
                var for_load = a.GetRange(0, BagQuant);
                foreach (var l in for_load)
                {
                    if (BatchQauntity.ContainsKey(l.Batch))
                    {
                        BatchQauntity[l.Batch] += l.Quantity;
                    }
                    else
                    {
                        BatchQauntity.Add(l.Batch, l.Quantity);
                    }
                }

                foreach (var bq in BatchQauntity)
                {
                    var batchMaterial = Storage.Find(s => s.Batch == bq.Key && s.Material.Trim().TrimStart('0')==Material);
                    if (batchMaterial == null) throw new UPMException("Партии "+bq.Key+" материала "+Material+" нет на складе УПМ",UPMExceptionMessage.NoGranulate);
                    if (batchMaterial.Available < bq.Value) throw new UPMException("Партии " + bq.Key + " материала " + Material + " недостаточно на складе УПМ",UPMExceptionMessage.NoEnoughGranulate);
                }

                var lastForLoad = for_load.Last();
                lastbag.LastBag = lastForLoad.DT;
                lastbag.LastTransferOrder = lastForLoad.TransferOrderNumber;

                int loadedBags = 0;
                foreach (var l in for_load)
                {
                    double BagWeight=0;
                    var hu=l.SU.Trim().TrimStart('0');
                    var innum=SAPConnect.AppData.Instance.GetTable<HUNUM>("VEKP","EXIDV = '"+hu.PadLeft(20,'0')+"'");
                    if (innum.Count>0){
                        var num = innum[0].InnerNumber;
                        var mat = SAPConnect.AppData.Instance.GetTable<HUMAT>("VEPO", "VENUM = '" + num+"'" );
                        if (mat.Count > 0)
                        {
                            BagWeight = mat[0].Quantity;
                        }
                    }
                    var res=SAPConnect.AppData.Instance.ZMOVE(l.SU, "LUP" + LUP.ToString());
                    if (res[0].Error)
                    {
                        throw new LoadBagException("SAP: " + res[0].Message, loadedBags, newLastBag.LastBag, newLastBag.LastTransferOrder);
                    }
                    Report.AddBagLoaded(DateTime.Now, Material, LUP, BagWeight, hu);
                    newLastBag = new LUPLastBag() { Material = lastbag.Material, LastBag = l.DT, LastTransferOrder = l.TransferOrderNumber };
                    loadedBags++;
                    Log.Add("Мешок " + l.SU + " был загружен в LUP" + LUP.ToString(), true, 0);
                }
                return true;
            }
            else
            {
                throw new UPMException("На УПМ недостаточно гранулята " + Material.ToString(), UPMExceptionMessage.NoBags);
            }
        }

        /*public static bool LoadBags(int LUP, string Material, int BagQuant, LastBag curLastBag, out LastBag newLastBag)
        {
            ///проверить наличие нужного количества материала на складе, даже если мешки уже привезли

            var Storage = GetStorageMaterial();
            //int LUP = 1;
            //var BagQuant = 10;
            //Material = "1000000012"
            var lastbag = curLastBag;// new DateTime(2018, 12, 06);
            newLastBag = curLastBag;
            var a = HU_At_UPM(Material, lastbag);
            if (a.Count >= BagQuant)
            {
                Dictionary<string, double> BatchQauntity = new Dictionary<string, double>();
                var for_load = a.GetRange(0, BagQuant);
                foreach (var l in for_load)
                {
                    if (BatchQauntity.ContainsKey(l.Batch))
                    {
                        BatchQauntity[l.Batch] += l.Quantity;
                    }
                    else
                    {
                        BatchQauntity.Add(l.Batch, l.Quantity);
                    }
                }

                foreach (var bq in BatchQauntity)
                {
                    var batchMaterial = Storage.Find(s => s.Batch == bq.Key && s.Material.Trim().TrimStart('0') == Material);
                    if (batchMaterial == null) throw new UPMException("Партии " + bq.Key + " материала " + Material + " нет на складе УПМ", UPMExceptionMessage.NoGranulate);
                    if (batchMaterial.Available < bq.Value) throw new UPMException("Партии " + bq.Key + " материала " + Material + " недостаточно на складе УПМ", UPMExceptionMessage.NoEnoughGranulate);
                }

                var lastloaded=for_load.Last();

                lastbag = new LastBag() { LastBagDateTime = lastloaded.DT, TransferOrder = lastloaded.TransferOrderNumber };

                int loadedBags = 0;
                foreach (var l in for_load)
                {
                    double BagWeight = 0;
                    var hu = l.SU.Trim().TrimStart('0');
                    var innum = SAPConnect.AppData.Instance.GetTable<HUNUM>("VEKP", "EXIDV = '" + hu.PadLeft(20, '0') + "'");
                    if (innum.Count > 0)
                    {
                        var num = innum[0].InnerNumber;
                        var mat = SAPConnect.AppData.Instance.GetTable<HUMAT>("VEPO", "VENUM = '" + num + "'");
                        if (mat.Count > 0)
                        {
                            BagWeight = mat[0].Quantity;
                        }
                    }
                    var res = SAPConnect.AppData.Instance.ZMOVE(l.SU, "LUP" + LUP.ToString());
                    if (res[0].Error)
                    {
                        throw new LoadBagException("SAP: " + res[0].Message, loadedBags, newLastBag.LastBagDateTime);
                    }
                    Report.AddBagLoaded(DateTime.Now, Material, LUP, BagWeight, hu);
                    newLastBag = new LastBag() { LastBagDateTime = l.DT, TransferOrder = l.TransferOrderNumber };
                    loadedBags++;
                    Log.Add("Мешок " + l.SU + " был загружен в LUP" + LUP.ToString(), true, 0);
                }
                return true;
            }
            else
            {
                throw new UPMException("На УПМ недостаточно гранулята " + Material.ToString(), UPMExceptionMessage.NoBags);
            }
        }*/

        public static List<HU> HU_At_UPM(string MaterialNumber, DateTime fromdate,long LastTransferOrder)
        {
            if (String.IsNullOrWhiteSpace(MaterialNumber)) throw new UPMException("Не указан материал", UPMExceptionMessage.MaterialNotSpecified);
            Int64 imn = 0;
            if (!Int64.TryParse(MaterialNumber, out imn)) throw new UPMException("Номер материала должен быть числом", UPMExceptionMessage.NotAGranulate);
            var mn = MaterialNumber.Trim().TrimStart('0').PadLeft(18, '0');
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            var fg = gran.Find(g => g == mn);
            if (fg == null) throw new UPMException("Загружаемый материал не принадлежит группе материалов \"Гранулят\"", UPMExceptionMessage.NotAGranulate);

            var to_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(NLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG') AND MATNR = '" + mn + "'");
            to_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            to_upm = to_upm.FindAll(h => h.DT > fromdate);

            var from_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(VLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG') AND MATNR = '" + mn + "'");
            from_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            from_upm = from_upm.FindAll(h => h.DT > fromdate);

            var fu_hu_num = from_upm.Select(n => n.SU).ToList();
            var Got_HU_to_UPM = to_upm.FindAll(n => !fu_hu_num.Contains(n.SU)).OrderBy(g => g.DT).ToList();

            var index=Got_HU_to_UPM.FindIndex(hu => hu.TransferOrderNumber == LastTransferOrder);
            Got_HU_to_UPM = Got_HU_to_UPM.Skip(index).ToList();

            return Got_HU_to_UPM;
        }

        public static List<HU> HU_At_UPM(string MaterialNumber, LastBag lastbag, long LastTransferOrder)
        {
            if (String.IsNullOrWhiteSpace(MaterialNumber)) throw new UPMException("Не указан материал", UPMExceptionMessage.MaterialNotSpecified);
            Int64 imn = 0;
            if (!Int64.TryParse(MaterialNumber, out imn)) throw new UPMException("Номер материала должен быть числом", UPMExceptionMessage.NotAGranulate);
            var mn = MaterialNumber.Trim().TrimStart('0').PadLeft(18, '0');
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            var fg = gran.Find(g => g == mn);
            if (fg == null) throw new UPMException("Загружаемый материал не принадлежит группе материалов \"Гранулят\"", UPMExceptionMessage.NotAGranulate);

            var fromdate = lastbag.LastBagDateTime;
            var to_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(NLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG') AND MATNR = '" + mn + "'");
            to_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            to_upm = to_upm.OrderBy(b => b.DT).ThenBy(b1 => b1.TransferOrderNumber).ToList();
            to_upm = to_upm.FindAll(h => (h.DT > fromdate) || (h.DT == fromdate && h.TransferOrderNumber > lastbag.TransferOrder));

            var from_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(VLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG') AND MATNR = '" + mn + "'");
            from_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            from_upm = from_upm.OrderBy(b => b.DT).ThenBy(b1 => b1.TransferOrderNumber).ToList();
            from_upm = from_upm.FindAll(h => (h.DT > fromdate) || (h.DT== fromdate && h.TransferOrderNumber>lastbag.TransferOrder));

            var fu_hu_num = from_upm.Select(n => n.SU).ToList();
            var Got_HU_to_UPM = to_upm.FindAll(n => !fu_hu_num.Contains(n.SU)).OrderBy(g => g.DT).ToList();

            var index = Got_HU_to_UPM.FindIndex(hu => hu.TransferOrderNumber == LastTransferOrder);
            Got_HU_to_UPM = Got_HU_to_UPM.Skip(index).ToList();

            return Got_HU_to_UPM;
        }

        public static List<StorageState> GetStorageMaterial(string MaterialNumber = null)
        {
            string matopt = "";
            if (!String.IsNullOrWhiteSpace(MaterialNumber))
            {
                matopt = " AND MATNR = '" + MaterialNumber.Trim().TrimStart('0').PadLeft(18, '0') + "'";
            }
            var lst = SAPConnect.AppData.Instance.GetTable<StorageState>("MCHB", "LGORT IN ('УПМ') AND CLABS <> 0" + matopt);
            return lst;
        }

        public static bool MoveBags(LUPLastBag LastBag, int BagQuant, out LUPLastBag newLastBag)
        //public static bool MoveBags(string Material, int BagQuant, DateTime LastBag, out DateTime newLastBag)
        {
            var lastbag = LastBag.LastBag;// new DateTime(2018, 12, 06);
            var lastbagq = lastbag;
            newLastBag = LastBag;
            if (BagQuant <= 0)
                lastbagq=lastbagq.AddDays(-5);
            var a = HU_At_UPM(LastBag.Material, lastbagq,LastBag.LastTransferOrder);
            var index = 0;
            for (index = 0; index < a.Count; index++)
            {
                if (a[index].DT >= lastbag) break;
            }
            var newind = index + BagQuant + (BagQuant>0?-1:0);
            if (newind < 0 || newind > a.Count - 1)
            {
                //throw new OverflowException();
                return false;
            }
            else
            {
                newLastBag = new LUPLastBag();
                newLastBag.Material = LastBag.Material;
                newLastBag.LastBag=a[newind].DT;
                newLastBag.LastTransferOrder = a[newind].TransferOrderNumber;
                return true;
            }
            
        }

        // PL: PL01 - PL12 (БМ01-БМ12 для EEQ)
        // LUP: LUP1 - LUP3
        public static void Z_SET_MACHINE_LINE(string PL, string LUP)
        {
            SAPConnect.AppData.Instance.Z_SET_MACHINE_LINE(PL, LUP);
        }

        // machine: 1 - 12... Номер машины
        // MachineStop: Останавливается ли машина 
        public static void Z_SET_MACHINE_STOP_FLAG(int machine, bool MachineStop)
        {
            SAPConnect.AppData.Instance.Z_SET_MACHINE_STOP_FLAG(machine, MachineStop);
        }

        public class HUNUM
        {
            [SAPConnect.SAPGetTable("VENUM")]
            public string InnerNumber;
            [SAPConnect.SAPGetTable("EXIDV")]
            public string HU;
        }

        public class HUMAT
        {
            [SAPConnect.SAPGetTable("VENUM")]
            public string InnerNumber;
            [SAPConnect.SAPGetTable("VEMNG")]
            public double Quantity;
        }

        public class HU
        {

            [SAPConnect.SAPGetTable("TANUM")]
            public long TransferOrderNumber;
            [SAPConnect.SAPGetTable("VLENR")]
            public string SU;
            [SAPConnect.SAPGetTable("QDATU", SAPConnect.SAPType.DATE)]
            public DateTime Date;
            [SAPConnect.SAPGetTable("QZEIT", SAPConnect.SAPType.TIME)]
            public TimeSpan Time;
            [SAPConnect.SAPGetTable("VBELN")]
            public string Delivery;

            [SAPConnect.SAPGetTable("MATNR")]
            public string MaterialNumber;

            [SAPConnect.SAPGetTable("CHARG")]
            public string Batch;
            [SAPConnect.SAPGetTable("VISTM")]
            public double Quantity;

            [SAPConnect.SAPExcludeFromReading]
            public DateTime DT
            {
                get
                {
                    return Date.Add(Time);
                }
            }
        }
        public class StorageState
        {
            [SAPConnect.ColumnName("Материал", 0, true)]
            [SAPConnect.SAPGetTable("MATNR", SAPConnect.SAPType.Corresponded)]
            public string Material;

            [SAPConnect.ColumnName("Название материала", 1)]
            [SAPConnect.SAPExcludeFromReading]
            public string MaterialName;

            [SAPConnect.SAPGetTable("WERKS", SAPConnect.SAPType.Corresponded)]
            public string Plant;

            [SAPConnect.SAPGetTable("LGORT", SAPConnect.SAPType.Corresponded)]
            public string Storage;

            [SAPConnect.SAPGetTable("CHARG", SAPConnect.SAPType.Corresponded)]
            public string Batch;


            [SAPConnect.SAPGetTable("LFGJA", SAPConnect.SAPType.Corresponded)]
            public string Year;
            [SAPConnect.SAPGetTable("LFMON", SAPConnect.SAPType.Corresponded)]
            public string Period;


            [SAPConnect.ColumnName("В свободном использовании", 6)]
            [SAPConnect.SAPGetTable("CLABS", SAPConnect.SAPType.Corresponded)]
            public double Available;

            [SAPConnect.SAPGetTable("CUMLM", SAPConnect.SAPType.Corresponded)]
            public double InMovement;

            [SAPConnect.SAPGetTable("CINSM", SAPConnect.SAPType.Corresponded)]
            public double AtQualityControl;

            [SAPConnect.SAPGetTable("CEINM", SAPConnect.SAPType.Corresponded)]
            public double LimitedUsage;
            public StorageState()
            {
            }
            public StorageState(StorageState ss)
            {
                Material = ss.Material;
                Plant = ss.Plant;
                Storage = ss.Storage;
                Batch = ss.Batch;

                Year = ss.Year;
                Period = ss.Period;

                Available = ss.Available;
                InMovement = ss.InMovement;
                AtQualityControl = ss.AtQualityControl;
                LimitedUsage = ss.LimitedUsage;


            }
        }

        public static LUPLastBag GetLastBag(string Material)
        {
            var selectedLanguage = "ru-RU";
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(selectedLanguage);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(selectedLanguage);

            DateTime LastBag=DateTime.MinValue;
            LUPLastBag result=new LUPLastBag();
            result.Material=Material;
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("GetLastBag", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaterialNumber", Material);
                    //cmd.Parameters.AddWithValue("@invID", invID);
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            
                            result.LastBag=!dr.IsDBNull(0)?dr.GetDateTime(0):new DateTime(2000,1,1,0,0,0);
                            result.LastTransferOrder=!dr.IsDBNull(1)?dr.GetInt64(1):9999999999L;

                           /* SQLInventory smi = new SQLInventory();
                            smi.InvID = dr.GetInt32(0);
                            smi.Date = dr.GetDateTime(1);
                            smi.Night = dr.GetBoolean(2);
                            lsd.Add(smi);*/
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
                //return DateTime.MinValue;
            }
        }
        public static void SetLastBag(string Material,DateTime LastBag, long LastTransferOrder)
        {
            var selectedLanguage = "ru-RU";
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(selectedLanguage);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(selectedLanguage);

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SetLastBag", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaterialNumber", Material);
                    cmd.Parameters.AddWithValue("@LastBagTime", LastBag);
                    cmd.Parameters.AddWithValue("@LastTransferOrder", LastTransferOrder);
                    cmd.ExecuteNonQuery();
                    Log.Add("Время следующего мешка для материала " + Material + " установлено в " + LastBag.ToString("dd-MM-yyyy hh\\:mm\\:ss"), true,0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SetLUPWeight(int LUPWeight1, int LUPWeight2)
        {
            var selectedLanguage = "ru-RU";
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(selectedLanguage);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(selectedLanguage);

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SetLUPWeight", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LUP1", LUPWeight1);
                    cmd.Parameters.AddWithValue("@LUP2", LUPWeight2);
                    cmd.ExecuteNonQuery();
                }
                LUPWeight[0] = LUPWeight1;
                LUPWeight[1]=LUPWeight2;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SetPLLUP(int PL, int LUP)
        {
            var selectedLanguage = "ru-RU";
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(selectedLanguage);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(selectedLanguage);

            try
            {
                var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SetPLLUP", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PL", PL);
                    cmd.Parameters.AddWithValue("@LUP", LUP);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static bool ProcessCommand(ref UPMCommand cmd)
        {
            Log.Add("Выполняется команда \"" + cmd.ToString() + "\"", true,0);
            switch (cmd.Command)
            {
                case UPMCommandType.LUPWeight:
                    {
                        UPMAction.SetLUPWeight(cmd.LUPWeight[0], cmd.LUPWeight[1]);
                        try
                        {
                            var resp = cmd.ResponseOK();
                            if (cmd.NetworkClient != null)
                            {
                                cmd.NetworkClient.Connection.GetStream().Write(resp, 0, resp.Length);
                                Log.Add("Ответ клиенту " + cmd.NetworkClient.Connection.Client.RemoteEndPoint.ToString() + ": <" + Log.ByteArrayToHexString(resp) + "> - \"OK\"", true, 2);
                            }
                            else
                            {
                                Log.Add("Нет подключения клиента", true, 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Add("Не удалось передать ответ клиенту");
                        }
                    }
                    break;
                case UPMCommandType.MachineStatus:
                    {
                        switch (cmd.Status)
                        {
                            case 0:
                                {
                                    UPMAction.SetPLLUP(cmd.PL, cmd.LUP);
                                    UPMAction.Z_SET_MACHINE_LINE("PL" + cmd.PL.ToString("D2"), "LUP" + cmd.LUP.ToString());
                                    UPMAction.Z_SET_MACHINE_STOP_FLAG(cmd.PL, true);
                                }
                                break;
                            case 1:
                                {
                                    UPMAction.SetPLLUP(cmd.PL, cmd.LUP);
                                    UPMAction.Z_SET_MACHINE_LINE("PL" + cmd.PL.ToString("D2"), "LUP" + cmd.LUP.ToString());
                                    UPMAction.Z_SET_MACHINE_STOP_FLAG(cmd.PL, false);
                                }
                                break;
                            case 2:
                                {
                                    UPMAction.SetPLLUP(cmd.PL, 3);
                                    UPMAction.Z_SET_MACHINE_LINE("PL" + cmd.PL.ToString("D2"), "LUP3");
                                    UPMAction.Z_SET_MACHINE_STOP_FLAG(cmd.PL, false);
                                }
                                break;
                        }
                        try
                        {
                            if (cmd.NetworkClient != null)
                            {

                                var resp = cmd.ResponseOK();
                                cmd.NetworkClient.Connection.GetStream().Write(resp, 0, resp.Length);
                                Log.Add("Ответ клиенту " + cmd.NetworkClient.Connection.Client.RemoteEndPoint.ToString() + ": <" + Log.ByteArrayToHexString(resp) + "> - \"OK\"", true, 2);
                            }
                            else
                            {
                                Log.Add("Нет подключения клиента", true, 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Add("Не удалось передать ответ клиенту");
                        }
                    }
                    break;
                case UPMCommandType.GranulateLoad:
                    {
                        try
                        {
                            if (cmd.NetworkClient != null)
                            {
                                var resp = cmd.ResponseOK();
                                cmd.NetworkClient.Connection.GetStream().Write(resp, 0, resp.Length);
                                Log.Add("Ответ клиенту " + cmd.NetworkClient.Connection.Client.RemoteEndPoint.ToString() + ": <" + Log.ByteArrayToHexString(resp) + "> - \"OK\"", true, 2);
                            }
                            else
                            {
                                Log.Add("Нет подключения клиента", true, 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Add("Не удалось передать ответ клиенту");
                        }
                        var LastBag = UPMAction.GetLastBag(cmd.Material);
                        try
                        {
                            if (cmd.BagQuant > 0)
                            {
                                if (UPMAction.LoadBags(cmd.LUP, cmd.BagQuant, LastBag, out LastBag))
                                {
                                    UPMAction.SetLastBag(cmd.Material, LastBag.LastBag,LastBag.LastTransferOrder);
                                }
                                else
                                {

                                    return false;
                                }
                            }else 
                                return true;
                        }
                        catch (Exception ex)
                        {
                            string addMsg = "";
                            UPMExceptionMessage uem=UPMExceptionMessage.SAPSQLError;
                            if (ex is LoadBagException)
                            {
                                var lbex=(LoadBagException)ex;
                                cmd.BagQuant -= lbex.Loaded;
                                addMsg = "Загружено мешков: " + lbex.Loaded.ToString();
                                try{
                                UPMAction.SetLastBag(cmd.Material, lbex.LastBag,lbex.LastTransferOrder);
                                }catch(Exception ex1){
                                    addMsg = "\nSQLError: " + ex1.Message;
                                }

                            }
                            if (ex is UPMException)
                            {
                                uem=(ex as UPMException).UPMMessage;
                            }
                            throw new UPMException(ex.Message+"\n "+addMsg,uem);
                        }
                    }
                    break;
                case UPMCommandType.UPMIncome:
                    {
                        // список мешков привезенных на UPM в течение смены
                        var HU_lst=HU_At_UPM(cmd.ShiftDate, cmd.IsNightShift);
                        // группировка мешков по материалу и весу мешка
                        var HU_lst_group=HU_lst.OrderBy(l=>l.MaterialNumber).ThenBy(l1=>l1.Quantity).GroupBy(l => new { l.MaterialNumber, l.Quantity });

                        try
                        {
                            if (cmd.NetworkClient != null)
                            {
                                if (cmd.NetworkClient.Connection.GetStream() != null)
                                {
                                    var response_lst = new List<byte>();
                                    response_lst.Add(0xff);
                                    response_lst.Add(cmd.MessageID);
                                    response_lst.Add(0x04);

                                    foreach (var HU_BLock in HU_lst_group)
                                    {

                                        var material_bytes = Encoding.ASCII.GetBytes(HU_BLock.Key.MaterialNumber.Trim().TrimStart('0').PadRight(10));
                                        if (material_bytes.Length > 10)
                                        {
                                            Log.Add("Материал \"" + HU_BLock.Key.MaterialNumber.Trim() + "\" имеет неверную длину. Должно быть 10 символов.");
                                            throw new Exception();
                                        }
                                        response_lst.AddRange(material_bytes);
                                        var BagWeight = (UInt16)HU_BLock.Key.Quantity;
                                        var bBagWeight = BitConverter.GetBytes(BagWeight);
                                        response_lst.AddRange(bBagWeight);
                                        var BagQuantity = (UInt16)HU_BLock.Count();
                                        var bBagQuantity = BitConverter.GetBytes(BagQuantity);
                                        response_lst.AddRange(bBagQuantity);
                                    }
                                    var resp = response_lst.ToArray();
                                    cmd.NetworkClient.Connection.GetStream().Write(resp, 0, resp.Length);
                                    Log.Add("Ответ клиенту " + cmd.NetworkClient.Connection.Client.RemoteEndPoint.ToString() + ": <" + Log.ByteArrayToHexString(resp) + "> - \"OK\"", true, 2);
                                }
                            }
                            else
                            {
                                Log.Add("Нет подключения клиента", true, 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Add("Не удалось передать ответ клиенту");
                        }
                        break;
                    }
                case UPMCommandType.EndShiftCorrection:
                    {
                        try
                        {
                            if (cmd.NetworkClient != null)
                            {
                                var resp = cmd.ResponseOK();
                                cmd.NetworkClient.Connection.GetStream().Write(resp, 0, resp.Length);
                                Log.Add("Ответ клиенту " + cmd.NetworkClient.Connection.Client.RemoteEndPoint.ToString() + ": <" + Log.ByteArrayToHexString(resp) + "> - \"OK\"", true, 2);
                            }else
                            {
                                Log.Add("Нет подключения клиента", true, 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Add("Не удалось передать ответ клиенту");
                        }
                        // список мешков привезенных на UPM в течение смены
                        var HU_lst = HU_At_UPM(cmd.ShiftDate, cmd.IsNightShift);
                        Report.MaprDuoCorrections(cmd.ShiftDate, cmd.IsNightShift, cmd.Corrections, HU_lst);
                    }
                    break;
            }
            return true;
        }

        public static void ResetMaterialsLastBag(DateTime? NewLast=null)
        {
            DateTime now;
            if (!NewLast.HasValue)
                now = DateTime.Now;
            else now = NewLast.Value;
            //now = new DateTime(2017, 08, 12, 9, 40, 57);
            Log.Add("Установка времени последних загруженных мешков на " + now, true,0);
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            gran.ForEach(g => UPMAction.SetLastBag(g.Trim().TrimStart('0'), now, 9999999999));
        }
        public static void ResetMaterialsLastBag(string Material, DateTime? NewLast = null)
        {
            DateTime now;
            if (!NewLast.HasValue)
                now = DateTime.Now;
            else now = NewLast.Value;
            //now = new DateTime(2017, 08, 12, 9, 40, 57);
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            var mat=gran.Find(g => g.Trim().TrimStart('0') == Material);
            if (mat != null)
            {
                mat = mat.Trim().TrimStart('0');
                Log.Add("Установка времени последних загруженных мешков для материала " + mat + " на " + now, true,0);
                UPMAction.SetLastBag(mat, now, 9999999999);
            }
            else
            {
                Log.Add("Материал "+Material+"не является гранулятом", true,0);
            }
        }

        public static List<LUPLastBag> GetLUPLastBag()
        {
            var selectedLanguage = "ru-RU";
            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(selectedLanguage);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(selectedLanguage);


            var cs = ConfigurationManager.ConnectionStrings["UPMConnectionString"].ConnectionString;
            SqlConnection connection = new SqlConnection(cs);
            connection.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM LUPLastBag ", connection);
            cmd.CommandType = CommandType.Text;

            SqlDataReader dr = cmd.ExecuteReader();
            List<LUPLastBag> lsd = new List<LUPLastBag>();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    LUPLastBag llb = new LUPLastBag();
                    llb.Material = dr.IsDBNull(0) ? "" : dr.GetString(0);
                    llb.LastBag = dr.IsDBNull(1) ? DateTime.MinValue : dr.GetDateTime(1); ;
                    llb.LastTransferOrder= dr.IsDBNull(2) ? 9999999999L : dr.GetInt64(2);
                    lsd.Add(llb);

                }
            }
            connection.Close();
            return lsd;
        }


        public static List<HU> HU_At_UPM(DateTime fromdate)
        {
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            //var fg = gran.Find(g => g == mn);

            var to_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(NLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG')");
            to_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            to_upm = to_upm.FindAll(h => h.DT > fromdate);

            var from_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(VLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG')");
            from_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            from_upm = from_upm.FindAll(h => h.DT > fromdate);

            var fu_hu_num = from_upm.Select(n => n.SU).ToList();
            var Got_HU_to_UPM = to_upm.FindAll(n => !fu_hu_num.Contains(n.SU)).OrderBy(g => g.DT).ToList();

            return Got_HU_to_UPM;
        }
        public static List<HU> HU_At_UPM(DateTime ShiftDate,bool isNight)
        {
            var fromdate = ShiftDate;
            var gran = SAPConnect.AppData.Instance.GetTable("MARA", (new string[] { "MATNR" }).ToList(), (new string[] { "MATKL = '100000000'" }).ToList());
            //var fg = gran.Find(g => g == mn);

            var to_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(NLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG')");
            to_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            to_upm = to_upm.FindAll(h => h.DT > fromdate);

            /*var from_upm = SAPConnect.AppData.Instance.GetTable<HU>("LTAP", -1, "(VLTYP = '921' AND VBELN <> '' AND (QDATU >= '" + fromdate.ToString("yyyyMMdd") + "') AND LETYP = 'BAG')");
            from_upm.RemoveAll(t => !gran.Contains(t.MaterialNumber));
            from_upm = from_upm.FindAll(h => h.DT > fromdate);*/

            //var fu_hu_num = from_upm.Select(n => n.SU).ToList();
            //var Got_HU_to_UPM = to_upm.FindAll(n => !fu_hu_num.Contains(n.SU)).OrderBy(g => g.DT).ToList();
            var Got_HU_to_UPM = to_upm.OrderBy(g => g.DT).ToList();
            DateTime shiftStarts = ShiftDate.Date;
            DateTime shiftEnds = ShiftDate.Date;
            if (isNight)
            {
                shiftStarts=shiftStarts.AddHours(20);
                shiftEnds=shiftEnds.AddHours(32);
            }
            else
            {
                shiftStarts=shiftStarts.AddHours(8);
                shiftEnds=shiftEnds.AddHours(20);
            }
            Got_HU_to_UPM = Got_HU_to_UPM.FindAll(h => h.DT >= shiftStarts && h.DT < shiftEnds);
            return Got_HU_to_UPM;
        }
        public static List<MaterialLeft> GetMaterials()
        {
            var timelst = GetLUPLastBag();

            var mb = SAPConnect.AppData.Instance.GetTable<MaterialBunker>("MCHB", "CLABS <> 0 AND LGORT = 'УПМ '");
            var materials = mb.Select(m => m.MaterialNumber.Trim().TrimStart('0')).Distinct().ToList();

            var mattimes = timelst.FindAll(tl => materials.Contains(tl.Material));

            var mintime = mattimes.Select(t => t.LastBag).Min();
            var hus = HU_At_UPM(mintime);
            List<MaterialLeft> ml = new List<MaterialLeft>();
            var hg = hus.GroupBy(h => new { h.MaterialNumber, h.Quantity });
            foreach (var h in hg)
            {
                var material = h.Key.MaterialNumber;
                var mtnr = material.Trim().TrimStart('0');
                var qnt = h.Key.Quantity;
                var lb = timelst.Find(t => t.Material == mtnr);
                if (lb != null)
                {
                    var hl = h.ToList();
                    hl = hl.OrderBy(he => he.DT).ToList();
                    var lastbag = timelst.Find(te => te.Material == mtnr);

                    var index = hl.FindIndex(hu => hu.TransferOrderNumber == lastbag.LastTransferOrder);
                    hl = hl.Skip(index).ToList();

                    var hll = hl.FindAll(hh => hh.DT > lb.LastBag);
                    if (hll.Count > 0)
                    {
                        ml.Add(new MaterialLeft() { Material = mtnr, Batch = "", Quant = hll.Sum(hle => hle.Quantity), BagCount=hll.Count,BaseWeight=qnt });
                    }
                }
            }
            return ml;
        }

        public class MaterialLeft
        {
            [SAPConnect.ColumnName("Материал", 0)]
            public string Material;
            [SAPConnect.ColumnName("Партия", 1)]
            public string Batch;
            [SAPConnect.ColumnName("Количество", 2)]
            public double Quant;
            [SAPConnect.ColumnName("Количество мешков", 3)]
            public double BagCount;
            [SAPConnect.ColumnName("Вес мешка", 3)]
            public double BaseWeight;
        }

        public class MaterialBunker
        {
            [SAPConnect.SAPGetTable("MATNR")]
            public string MaterialNumber;
            [SAPConnect.SAPExcludeFromReading]
            public string MaterialName;
            [SAPConnect.SAPGetTable("LGORT")]
            public string Bunker;
            [SAPConnect.SAPGetTable("CLABS")]
            public double Volume;
            [SAPConnect.SAPExcludeFromReading]
            public System.Drawing.Color Color;
        }

        public class LUPLastBag
        {
            public string Material;
            public DateTime LastBag;
            public long LastTransferOrder;
        }

        public static void ChangeShift(DateTime ShiftDate,bool isNight, int[] LUPWeight)
        {
            
            var bagsAtUPM = GetMaterials();
            var lupweight = LUPWeight;
            
            Report.AddLUPAtShiftStart(ShiftDate,isNight,LUPWeight[0],LUPWeight[1],0);
            Report.AddMaterialAtShiftStart(ShiftDate, isNight, bagsAtUPM);
        }

    }

    public static class Log
    {
        static object lockobject = new object();
        public static List<string> CurrentMessages=new List<string>();
        private static StreamWriter File=null;
        private static DateTime CurrentDate = DateTime.MinValue;
        public static DateTime LastLog=DateTime.MinValue;
        public static string LogPath;
        public static int LogLevel;
        static Log()
        {
            ChangeDate();
            AppDomain.CurrentDomain.ProcessExit +=StopLog;
        }

        private static void ChangeDate()
        {
            var date = DateTime.Now.Date;
            if (DateTime.Now.TimeOfDay < new TimeSpan(8, 0, 0))
            {
                date = date.AddDays(-1);
            }
            if (date != CurrentDate)
            {
                if (File!=null)
                    File.Close();
                CurrentDate = date;

                var path = Settings.GetOptionValue<string>(Constants.LogPath);
                if(String.IsNullOrWhiteSpace(path))
                {

                    path = System.Reflection.Assembly.GetEntryAssembly().Location;
                    path = Path.GetDirectoryName(path);
                }
                if (!path.EndsWith("\\")) path = path + "\\";
                LogPath = path;
                File = new StreamWriter(path + "log_" + CurrentDate.ToString("yyyyMMdd")+".log",true);
                File.AutoFlush = true;
                CurrentMessages.Clear();
            }
            LogLevel = Settings.GetOptionValue<int>(Constants.LogLevel);
        }

        public static void Add(string Message, bool ShowDateTime = true,int loglevel=0)
        {
            lock (lockobject)
            {
                ChangeDate();
                if (loglevel > LogLevel) return;
                StringBuilder sb = new StringBuilder();
                LastLog = DateTime.Now;
                if (ShowDateTime)
                {
                    sb.Append(LastLog.ToString("dd-MM-yyyy HH\\:mm\\:ss    "));
                }
                sb.Append(Message);
                var msg=sb.ToString();
                File.WriteLine(msg);
                File.Flush();
                CurrentMessages.Add(msg);
            }
        }

        public static void Add(Exception ex, int loglevel = 0)
        {
            lock (lockobject)
            {
                ChangeDate();
                if (loglevel > LogLevel) return;
                StringBuilder sb = new StringBuilder();
                LastLog = DateTime.Now;
                sb.Append(LastLog.ToString("dd-MM-yyyy HH\\:mm\\:ss    "));
                sb.Append(ex.Message);
                sb.Append(ex.StackTrace);
                var msg = sb.ToString();
                /*File.Write(DateTime.Now.ToString("dd-MM-yyyy HH\\:mm\\:ss    "));
                File.WriteLine(ex.Message);
                File.WriteLine(ex.StackTrace);*/
                File.Write(msg);
                File.Flush();
                CurrentMessages.Add(msg);
            }
        }
        static void StopLog(object sender, EventArgs e)
        {
            try
            {
                if (File != null) File.Close();
            }
            finally
            {
                File = null;
            }
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            return String.Join(", ", data.Select(d => "0x" + d.ToString("X2")));
        }
        /*~Log()
        {
            try
            {
                if (File != null) File.Close();
            }
            finally { }
        }*/
        /*public static void Dispose()
        {
            try
            {
                if (File != null) File.Close();
            }
            finally
            {
                File = null;
            }
        }*/
    }

    public class UPMException : Exception
    {
        public UPMExceptionMessage UPMMessage;
        public UPMException(string message, UPMExceptionMessage msg)
            : base(message)
        {
            UPMMessage = msg;
        }

        public static string Helper(UPMExceptionMessage msg)
        {
            switch (msg)
            {
                case UPMExceptionMessage.SAPSQLError:
                    return
@"Действия персонала:
• В случае блокировки материала пользователем подождите 30 минут. Если ошибка не ушла сообщите
  пользователю о блокировке материала. В случае, когда это сделать не представляется возможным
  сообщите об ошибке системному администратору или в IT-отдел.
";
                case UPMExceptionMessage.MaterialNotSpecified:
                case UPMExceptionMessage.NotAGranulate:
                    return @"Причина ошибки и действия персонала:
• Ошибка в программе MaprDuo. Проверьте тип используемого гранулята и данные настроек гранулятов. ";
                case UPMExceptionMessage.NoBags:
                    return 
@"Причина ошибки и действия персонала:
•  Гранулят не поступал на УПМ со склада в течении текущей смены. Проверьте в SAP наличие поставки гранулята со склада на УПМ.
   Проверьте фактическое количество гранулята на УПМ. Возможно в SAP не была сформирована из цеха заявка, а гранулят был завезен
   (такое возможно в ночные смены). После оформления поставки гранулята ошибка исчезнет и все действия по перемещению гранулята
   в LUP будут завершены.
•  При переходе на другой гранулят в программе Mapr Duo не был своевременно изменен тип используемого гранулята,а его загрузка
   в LUP уже осуществлена. В этом случае, выберите в прогорамме реально используемы гранулят и в течении своей смены обязательно
   сообщите об ошибке системному администратору. В этом случае, ошибку может устранить только системный администратор.";
                case UPMExceptionMessage.NoGranulate:
                case UPMExceptionMessage.NoEnoughGranulate:
                    return 
@"Причина ошибки и действия персонала:
•  Фактически, мешки с гранулятом уже находятся на УПМ, а проводка со склада еще не сделана. Проверьте в SAP наличие поставки на
   гранулят со склада на УПМ. Проверьте в SAP наличие проводки гранулята со склада на УПМ.  В случае их отсутствия сообщите работнику
   склада. После проведения в SAP гранулята со склада на УПМ ошибка исчезнет и все действия по перемещению гранулята в LUP будут завершены. 
";

            }
            return @"";
        }
    }

    public enum UPMExceptionMessage
    {
        NotUPMError,
        NoBags,
        NoGranulate,
        NoEnoughGranulate,
        MaterialNotSpecified,
        NotAGranulate,
        SAPSQLError
    }

    
    public class LoadBagException : Exception
    {
        public int Loaded;
        public DateTime LastBag;
        public long LastTransferOrder;
        public LoadBagException(string message,int loaded,DateTime lastBag, long lastTransferOrder)
            : base(message)
        {
            Loaded = loaded;
            LastBag = lastBag;
            LastTransferOrder = lastTransferOrder;
        }
    }

    public static class Settings
    {
        public static void SetOptionValue(string option, string value)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings.Remove(option);
            config.AppSettings.Settings.Add(option, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public static T GetOptionValue<T>(string option)
        {
            var vl = ConfigurationManager.AppSettings[option];
            if (vl != null)
            {
                return (T)Convert.ChangeType(vl, typeof(T));
            }
            else { return default(T); }

        }
    }

    public class Prompt
    {
        public static string ShowDialog(string text, string caption, bool password)
        {
            Form prompt = new Form()
            {
                Width = 420,
                Height = 120,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 10, Top = 10, Text = text,Width=400 };
            TextBox textBox = new TextBox() { Left = 10, Top = 25, Width = 400 };
            if (password) textBox.PasswordChar = '*';
            Button confirmation = new Button() { Text = "ОК", Left = 170, Width = 100, Top = 55, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }


    public class LastBag
    {
        public long TransferOrder;
        public DateTime LastBagDateTime;
    }
}

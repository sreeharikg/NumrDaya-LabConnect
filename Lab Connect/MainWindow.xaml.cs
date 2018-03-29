using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using Npgsql;
using NpgsqlTypes;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace Lab_Connect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //DBdataEntryPentra(null);
            //DBdataEntryTba40fr(null);
            //DBdataEntry(null);
            InitializeComponent();
            getPorts();
        }

        public System.IO.Ports.SerialPort tba25frPort;
        public System.IO.Ports.SerialPort tba40frPort;
        public System.IO.Ports.SerialPort cobase44Port;
        public System.IO.Ports.SerialPort pentraEs60Port;
        public string allowedChars = "01234567890.";
        public string tempResult = "";

        #region other functions
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            getPorts();
        }
        private string hexToString(string hexStr)
        {
            string data = "";
            for (int i = 0; i < hexStr.Length; i += 2)
            {
                // Convert the number expressed in base-16 to an integer.
                int value = Convert.ToInt32(hexStr.Substring(i, 2), 16);
                // Get the character corresponding to the integral value.
                string stringValue = Char.ConvertFromUtf32(value);
                data += ((char)value).ToString();
            }
            return data;
        }

        private void getPorts()
        {
            port_Tba25fr.Items.Clear();
            port_Tba40fr.Items.Clear();
            port_cobas244.Items.Clear();
            port_pentraEs60.Items.Clear();
            if (System.IO.Ports.SerialPort.GetPortNames().Length <= 0)
            {
                port_Tba25fr.Items.Add("no ports");
                port_Tba40fr.Items.Add("no ports");
                port_cobas244.Items.Add("no ports");
                port_pentraEs60.Items.Add("no ports");
            }
            foreach (String s in System.IO.Ports.SerialPort.GetPortNames())
            {
                port_Tba25fr.Items.Add(s);
                port_Tba40fr.Items.Add(s);
                port_cobas244.Items.Add(s);
                port_pentraEs60.Items.Add(s);
            }
        }



        #endregion

        #region TB25FR
        private void connect_tba25fr_Click(object sender, RoutedEventArgs e)
        {
        //    String port = port_Tba25fr.Text;
        //    int baudrate = Convert.ToInt32(buadRate_Tba25fr.Text);
        //    Parity parity = (Parity)Enum.Parse(typeof(Parity), parity_Tba25fr.Text);
        //    int databits = Convert.ToInt32(dataBit_Tba25fr.Text);
        //    StopBits stopbits = (StopBits)Enum.Parse(typeof(StopBits), stopbit_Tba25fr.Text);
            //tba25fr_connect(port, baudrate, parity, databits, stopbits);
            bool error = false;
            tba25frPort = new System.IO.Ports.SerialPort();
            tba25frPort.DataReceived += new SerialDataReceivedEventHandler(tba25fr_DataReceived);
            // If the port is open, close it.
            if (tba25frPort.IsOpen) tba25frPort.Close();
            else
            {
                // Set the port's settings
                tba25frPort.BaudRate = int.Parse(buadRate_Tba25fr.Text);
                tba25frPort.DataBits = int.Parse(dataBit_Tba25fr.Text);
                tba25frPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbit_Tba25fr.Text);
                tba25frPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity_Tba25fr.Text);
                tba25frPort.PortName = port_Tba25fr.Text;
                tba25frPort.DtrEnable = true;
                tba25frPort.RtsEnable = true;

                try
                {
                    // Open the port
                    tba25frPort.Open();
                }
                catch (UnauthorizedAccessException) { error = true; }
                catch (IOException) { error = true; }
                catch (ArgumentException) { error = true; }

                if (error) MessageBox.Show(this, "Could not open the COM port.  Most likely it is already in use, has been removed, or is unavailable.", "COM Port Unavalible");
                else
                {
                    // Show the initial pin states
                    connect_tba25fr.Content = "Connected";
                    connect_tba25fr.IsEnabled = false;
                }
            }

        }


        private void  tba25fr_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();
            //MessageBox.Show(tba25frPort.ReadExisting());
            //this.Dispatcher.Invoke(() =>
            //{
            //    tba25frView.dataView.Text = dtn + "\n";
            //    tba25frView.dataView.Text = tba25frPort.ReadExisting() + "\n";
            //});
            // Obtain the number of bytes waiting in the port's buffer
            int bytes = tba25frPort.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];

            // Read the data from the port and store it in our buffer
            tba25frPort.Read(buffer, 0, bytes);
            tba25frPort.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);
            //MessageBox.Show(buffer.ToString());
            string hex = BitConverter.ToString(buffer);
            if (!hex.Contains("03"))
            {
                tempResult += hex;
            }
            else
            {
                tempResult += hex;
                string thrdData = tempResult;
               // dbentry25fr(thrdData);
                Task.Run(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    DBdataEntry(thrdData);
                });
                
                tempResult = "";
            }

        }
        //private async Task dbentry25fr(string thrdData)
        //{
        //    Task.Run(() =>
        //    {
        //        DBdataEntry(thrdData);
        //    });
        //}
        private void DBdataEntry(String strData)
        {
            try
            {
                strData = strData.Replace("-", "");
                string resultStr = hexToString(strData.Substring(2, strData.IndexOf("17")));
                string results = hexToString(strData.Substring(98, strData.IndexOf("17") - 98));
                string Pdata = hexToString(strData.Substring(strData.IndexOf("17") + 2, strData.Length - strData.IndexOf("17") - 6));
                var cmpltDt = DateTime.ParseExact(Pdata.Substring(0, 12), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                var ordDt = DateTime.ParseExact(resultStr.Substring(33, 12), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
               
                string assayId,rslt, resultss = "";
                try
                {
                    for (int i = 0; i < results.Length / 15; i++)
                    {
                        assayId = results.Substring(i * 15, 4).Trim();
                        rslt = results.Substring((i * 15) + 4, 6).Trim();
                        rslt = new string(rslt.Where(c => allowedChars.Contains(c)).ToArray());
                        resultss += assayId + "@" + rslt + "_";
                    }
                    resultss = resultss.Substring(0, resultss.Length - 1);
                }
                catch (Exception eg)
                {
                    resultss = eg.ToString();
                }

                //DateTime cpltedDate = DateTime.ParseExact(Pdata.Substring(0, 11), "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture);
                //string ordrdDate=
                //string str = "";
                //for (int i = 0; i < strData.Length; i += 4)
                //    str += (char)Int16.Parse(strData.Substring(i, 4), NumberStyles.AllowHexSpecifier);
                DBHelper DB = new DBHelper();
                using (DBCommand cmd = new DBCommand("select nextval('lab_machine_data_id_seq');"))
                {
                    string nxtId = Convert.ToString(DB.ExecuteScalar(cmd));


                    List<DBCommand> commands = new List<DBCommand>();
                    using (DBCommand cmdInsert = new DBCommand("insert into lab_machine_data(id,test_id,sample_id,result_string,completed_date,ordered_date,tba25fr)values(@id,@test_id,@sample_id,@result_string,@completed_date,@ordered_date,@tba25fr)"))
                    {
                        cmdInsert.Parameters.AddWithValue("@id", nxtId);
                        cmdInsert.Parameters.AddWithValue("@test_id", Pdata.Substring(12, 15));
                        cmdInsert.Parameters.AddWithValue("@sample_id", resultStr.Substring(6, 20));
                        cmdInsert.Parameters.AddWithValue("@result_string", resultss);
                       // cmdInsert.Parameters.AddWithValue("@msg_str", hexToString(strData));
                        cmdInsert.Parameters.AddWithValue("@completed_date", cmpltDt);
                        cmdInsert.Parameters.AddWithValue("@ordered_date", ordDt);
                        cmdInsert.Parameters.AddWithValue("@tba25fr", 1);
                        commands.Add(cmdInsert);

                        var result = DB.ExecuteNonQueriesInTransaction(commands);


                    }
                }
            }
            catch (Exception eg)
            {
                //string path = @"C:\numrErrors\log.txt";
                //if (File.Exists(path))
                //{
                //    File.AppendAllText(path, eg.TargetSite+"\n"+eg.StackTrace+"\n"+eg.Message + " -" + System.DateTime.Now+"\n\n");
                //}
                //else
                //{
                //    Directory.CreateDirectory(@"C:\numrErrors\");
                //    File.WriteAllText(path, eg.TargetSite + "\n" + eg.StackTrace + "\n" + eg.Message + " -" + System.DateTime.Now + "\n\n");
                //}
                //Common.Util.Logger.Error("TBA25fr msg splitting", eg);
                System.Windows.Forms.MessageBox.Show(eg.TargetSite + "\n" + eg.StackTrace + "\n" + eg.Message + "");
                //tba40frPort.Close();
                //tba40frPort.Open();
                return;
            }
        }

        private void disconnect_tba25fr_Copy_Click(object sender, RoutedEventArgs e)
        {
            //tba25frPort = new System.IO.Ports.SerialPort();
            if (tba25frPort!=null && tba25frPort.IsOpen)
            {
                connect_tba25fr.Content = "Connect";
                connect_tba25fr.IsEnabled = true;
                tba25frPort.Close();
                System.Windows.Forms.MessageBox.Show("Disconnected");
            }
        }
    #endregion

        #region tba40fr
        private void connect_Tba40fr_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;
            tba40frPort = new System.IO.Ports.SerialPort();
            tba40frPort.DataReceived += new SerialDataReceivedEventHandler(tba40fr_DataReceived);
            // If the port is open, close it.
            if (tba40frPort.IsOpen) tba40frPort.Close();
            else
            {
                // Set the port's settings
                tba40frPort.BaudRate = int.Parse(buadRate_Tba40fr.Text);
                tba40frPort.DataBits = int.Parse(dataBit_Tba40fr.Text);
                tba40frPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbit_Tba40fr.Text);
                tba40frPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity_Tba40fr.Text);
                tba40frPort.PortName = port_Tba40fr.Text;
                tba40frPort.DtrEnable = true;
                tba40frPort.RtsEnable = true;

                try
                {
                    // Open the port
                    tba40frPort.Open();
                }
                catch (UnauthorizedAccessException) { error = true; }
                catch (IOException) { error = true; }
                catch (ArgumentException) { error = true; }

                if (error) MessageBox.Show(this, "Could not open the COM port.  Most likely it is already in use, has been removed, or is unavailable.", "COM Port Unavalible");
                else
                {
                    // Show the initial pin states
                    connect_Tba40fr.Content = "Connected";
                    connect_Tba40fr.IsEnabled = false;
                }
            }
        }
        public string tba40frTempResult = "";
        private void tba40fr_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();
            //MessageBox.Show(tba25frPort.ReadExisting());
            //this.Dispatcher.Invoke(() =>
            //{
            //    tba25frView.dataView.Text = dtn + "\n";
            //    tba25frView.dataView.Text = tba25frPort.ReadExisting() + "\n";
            //});
            // Obtain the number of bytes waiting in the port's buffer
            int bytes = tba40frPort.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];

            // Read the data from the port and store it in our buffer
            tba40frPort.Read(buffer, 0, bytes);
            tba40frPort.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);

            //MessageBox.Show(buffer.ToString());
            //string data1 = tba40frPort.ReadExisting();
            //tba40frPort.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);
            //DBdataEntryTba40fr(data1);
            //System.Windows.Forms.MessageBox.Show(data1);

            //DBdataEntryTba40fr(data1);

            string hex = BitConverter.ToString(buffer);


            if (!hex.Contains("03"))
            {
                tba40frTempResult += hex;
            }
            else
            {
                tba40frTempResult += hex;
                string thrdData = tba40frTempResult;
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    DBdataEntryTba40fr(thrdData);
                }).Start();
                tba40frTempResult = "";
            }

        }

        private void DBdataEntryTba40fr(string strData)
        {
            try
            {
                strData = strData.Replace("-", "");
                string resultStr = hexToString(strData.Substring(strData.IndexOf("02")));

                if (!resultStr.Substring(0, 4).Contains("R0"))
                    return;
                //string Pdata = hexToString(strData.Substring(strData.IndexOf("17") + 2, strData.Length - strData.IndexOf("17") - 6));
                //var cmpltDt = DateTime.ParseExact(Pdata.Substring(0, 12), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                //DateTime cpltedDate = DateTime.ParseExact(Pdata.Substring(0, 11), "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture);
                //string ordrdDate=
                //string str = "";
                //for (int i = 0; i < strData.Length; i += 4)
                //    str += (char)Int16.Parse(strData.Substring(i, 4), NumberStyles.AllowHexSpecifier);
                string dt = resultStr.Substring(37, 5);
                string mm = dt.Substring(0, 2);
                string dd = dt.Substring(3, 2);
                if (dd.Substring(0,2).Contains(" "))
                    dd = "0" + dd.Substring(1, 1);
                if (mm.Contains(" "))
                    mm = "0" + mm.Substring(1, 1);
                string cpltedDate = DateTime.ParseExact(mm + "/" + dd, "MM/dd", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                Int32 testID = Convert.ToInt32(resultStr.Substring(3, 15));
                string sample = resultStr.Substring(18, 4);
                string results = resultStr.Substring(49, resultStr.Length - 50);
                string assayId,rslt, resultss = "";
                try
                {
                    for (int i = 0; i < results.Length / 10; i++)
                    {
                        assayId = results.Substring(i*10, 2).Trim();
                        rslt = results.Substring((i * 10) + 2, 6).Trim();
                        rslt = new string(rslt.Where(c => allowedChars.Contains(c)).ToArray());
                        resultss += assayId + "@" + rslt + "_";
                    }
                    resultss = resultss.Substring(0, resultss.Length - 1);
                }
                catch (Exception eg)
                {
                    resultss = eg.ToString();
                }

                DBHelper DB = new DBHelper();
                DBCommand dtCmd = new DBCommand("select now()::timestamp");
                string srvr = Convert.ToDateTime(DB.ExecuteScalar(dtCmd)).ToString("HH:mm:ss");
                cpltedDate += " " + srvr;
                using (DBCommand cmd = new DBCommand("select nextval('lab_machine_data_id_seq');"))
                {
                    string nxtId = Convert.ToString(DB.ExecuteScalar(cmd));


                    List<DBCommand> commands = new List<DBCommand>();
                    using (DBCommand cmdInsert = new DBCommand("insert into lab_machine_data(id,test_id,sample_id,result_string,completed_date,tba40fr)values(@id,@test_id,@sample_id,@result_string,@completed_date,@tba40fr)"))
                    {
                        cmdInsert.Parameters.AddWithValue("@id", nxtId);
                        cmdInsert.Parameters.AddWithValue("@test_id", testID);
                        cmdInsert.Parameters.AddWithValue("@sample_id", sample);
                        cmdInsert.Parameters.AddWithValue("@result_string", resultss);
                       // cmdInsert.Parameters.AddWithValue("@msg_Str", resultStr);
                        cmdInsert.Parameters.AddWithValue("@completed_date", cpltedDate);
                        cmdInsert.Parameters.AddWithValue("@tba40fr", 1);
                        commands.Add(cmdInsert);

                        var result = DB.ExecuteNonQueriesInTransaction(commands);


                    }
                }
            }
            catch (Exception e)
            {
                tba40frPort.Close();
                tba40frPort.Open();
                return;
            }
        }

        private void disconnect_Tba40fr_Click(object sender, RoutedEventArgs e)
        {
            //tba40frPort = new System.IO.Ports.SerialPort();
            if (tba40frPort!=null && tba40frPort.IsOpen)
            {
                connect_Tba40fr.Content = "Connect";
                connect_Tba40fr.IsEnabled = true;
                tba40frPort.Close();
                System.Windows.Forms.MessageBox.Show("Disconnected");
            }
        }
        #endregion

        #region cobas
        private void connect_cobas244_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;
            cobase44Port = new System.IO.Ports.SerialPort();
            cobase44Port.DataReceived += new SerialDataReceivedEventHandler(cobase44Port_DataReceived);
            // If the port is open, close it.
            if (cobase44Port.IsOpen) cobase44Port.Close();
            else
            {
                // Set the port's settings
                cobase44Port.BaudRate = int.Parse(buadRate_cobas244.Text);
                cobase44Port.DataBits = int.Parse(dataBit_cobas244.Text);
                cobase44Port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbit_cobas244.Text);
                cobase44Port.Parity = (Parity)Enum.Parse(typeof(Parity), parity_cobas244.Text);
                cobase44Port.PortName = port_cobas244.Text;
                cobase44Port.DtrEnable = true;
                cobase44Port.RtsEnable = true;

                try
                {
                    // Open the port
                    cobase44Port.Open();
                }
                catch (UnauthorizedAccessException) { error = true; }
                catch (IOException) { error = true; }
                catch (ArgumentException) { error = true; }

                if (error) MessageBox.Show(this, "Could not open the COM port.  Most likely it is already in use, has been removed, or is unavailable.", "COM Port Unavalible");
                else
                {
                    // Show the initial pin states
                    connect_cobas244.Content = "Connected";
                    connect_cobas244.IsEnabled = false;
                }
            }
        }
        public string cobasTempResult = "";
        private void cobase44Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();
            //MessageBox.Show(tba25frPort.ReadExisting());


            int bytes = cobase44Port.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];

            // Read the data from the port and store it in our buffer
            cobase44Port.Read(buffer, 0, bytes);
            //cobase44Port.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);

            //MessageBox.Show(buffer.ToString());
            //string data1 = tba40frPort.ReadExisting();
            //tba40frPort.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);
            //DBdataEntryTba40fr(data1);
            //System.Windows.Forms.MessageBox.Show(data1);

            //DBdataEntryTba40fr(data1);

            string hex = BitConverter.ToString(buffer);


            if (!hex.Contains("03"))
            {
                cobasTempResult += hex;
            }
            else
            {
                cobasTempResult += hex;
                string thrdData = cobasTempResult;
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    DBdataEntryCobas(thrdData);
                }).Start();
                cobasTempResult = "";
            }


                cobase44Port.Write(new byte[] { 0x06 }, 0, 1);
                //cobase44Port.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);
                //tempResult = "";
            

        }
        private void DBdataEntryCobas(string strData)
        {
            try
            {
                strData = strData.Replace("-", "");
                //var msgSplitCount = strData.
                string resultStr = hexToString(strData);
                //const char LF = '\u000a';
                const char LF = (char)13;
                string[] words = resultStr.Split(LF);
                string testID = "", sample = "",  results = "";
                //int count = strData.Count(LF);
                DateTime cmpltDt = DateTime.Now;
                DateTime ordDt = DateTime.Now;
                int i = 0;
                foreach (string s in words)
                {
                   if(s.StartsWith("o|")||s.StartsWith("O|"))
                   {
                       string[] pData = s.Split('|');
                       testID = pData[2];
                       sample = pData[3].Substring(0,pData[3].IndexOf("^"));
                       cmpltDt = DateTime.ParseExact(pData[22], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                       ordDt = DateTime.ParseExact(pData[7], "yyyyMMddHHmmss", CultureInfo.InvariantCulture); 
                   }
                   else if (s.StartsWith("r|") || s.StartsWith("R|"))
                   {
                       const char etb = '\u0017';
                       const char stx = '\u0002';
                       string temp = s;
                       if (s.Contains(etb))
                       {
                           temp += words[i + 1];
                           temp = temp.Replace(stx.ToString(), "");
                           temp = temp.Replace(etb.ToString(), "");
                       }
                       string[] reslt = temp.Split('|');
                       string[] assy=reslt[2].Split('^');
                       string assayId = assy[3].Substring(0, assy[3].IndexOf('/'));
                       string result =new string(reslt[3].Where(c => allowedChars.Contains(c)).ToArray());
                       //string result = reslt[3];
                       results += assayId + "@" + result + "_";
                   }
                   i++;
                }
                results = results.Substring(0, results.Length - 1);

                DBHelper DB = new DBHelper();

                using (DBCommand cmd = new DBCommand("select nextval('lab_machine_data_id_seq');"))
                {
                    string nxtId = Convert.ToString(DB.ExecuteScalar(cmd));


                    List<DBCommand> commands = new List<DBCommand>();
                    using (DBCommand cmdInsert = new DBCommand("insert into lab_machine_data(id,test_id,sample_id,result_string,completed_date,ordered_date,cobas)values(@id,@test_id,@sample_id,@result_string,@completed_date,@ordered_date,@cobas)"))
                    {
                        cmdInsert.Parameters.AddWithValue("@id", nxtId);
                        cmdInsert.Parameters.AddWithValue("@test_id", testID);
                        cmdInsert.Parameters.AddWithValue("@sample_id", sample);
                        cmdInsert.Parameters.AddWithValue("@result_string", results);
                        //cmdInsert.Parameters.AddWithValue("@msg_Str", resultStr);
                        cmdInsert.Parameters.AddWithValue("@completed_date", cmpltDt);
                        cmdInsert.Parameters.AddWithValue("@ordered_date", ordDt);
                        cmdInsert.Parameters.AddWithValue("@cobas", 1);
                        commands.Add(cmdInsert);

                        var result = DB.ExecuteNonQueriesInTransaction(commands);


                    }
                }

            }
            catch (Exception e)
            {
                return;
            }
        }
        private void disconnect_cobas244_Click(object sender, RoutedEventArgs e)
        {
            //cobase44Port = new System.IO.Ports.SerialPort();
            if (cobase44Port!=null && cobase44Port.IsOpen)
            {
                connect_cobas244.Content = "Connect";
                connect_cobas244.IsEnabled = true;
                cobase44Port.Close();
                System.Windows.Forms.MessageBox.Show("Disconnected");
            }
        }
        #endregion

        #region pentra
        private void connect_pentraEs60_Click(object sender, RoutedEventArgs e)
        {

            bool error = false;
            pentraEs60Port = new System.IO.Ports.SerialPort();
            pentraEs60Port.DataReceived += new SerialDataReceivedEventHandler(pentraEs60_DataReceived);
            // If the port is open, close it.
            if (pentraEs60Port.IsOpen) pentraEs60Port.Close();
            else
            {
                // Set the port's settings
                pentraEs60Port.BaudRate = int.Parse(buadRate_pentraEs60.Text);
                pentraEs60Port.DataBits = int.Parse(dataBit_pentraEs60.Text);
                pentraEs60Port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbit_pentraEs60.Text);
                pentraEs60Port.Parity = (Parity)Enum.Parse(typeof(Parity), parity_pentraEs60.Text);
                pentraEs60Port.PortName = port_pentraEs60.Text;
                pentraEs60Port.DtrEnable = true;
                pentraEs60Port.RtsEnable = true;

                try
                {
                    // Open the port
                    pentraEs60Port.Open();
                }
                catch (UnauthorizedAccessException) { error = true; }
                catch (IOException) { error = true; }
                catch (ArgumentException) { error = true; }

                if (error) MessageBox.Show(this, "Could not open the COM port.  Most likely it is already in use, has been removed, or is unavailable.", "COM Port Unavalible");
                else
                {
                    // Show the initial pin states
                    connect_pentraEs60.Content = "Connected";
                    connect_pentraEs60.IsEnabled = false;
                }
            }
        }

        public string pentraTempResult = "";
        private void pentraEs60_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();
            //MessageBox.Show(tba25frPort.ReadExisting());
            pentraEs60Port.Write(new byte[] { 0x06 }, 0, 1);

            int bytes = pentraEs60Port.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer = new byte[bytes];

            // Read the data from the port and store it in our buffer
            pentraEs60Port.Read(buffer, 0, bytes);
            //cobase44Port.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);

            //MessageBox.Show(buffer.ToString());
            //string data1 = tba40frPort.ReadExisting();
            //tba40frPort.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);
            //DBdataEntryTba40fr(data1);
            //System.Windows.Forms.MessageBox.Show(data1);

            //DBdataEntryTba40fr(data1);

            string hex = BitConverter.ToString(buffer);


            if (!hex.Contains("04"))
            {
                pentraTempResult += hex;
            }
            else
            {
                pentraTempResult += hex;
                string thrdData = pentraTempResult;
                //new Thread(() =>
                //{
                //    Thread.CurrentThread.IsBackground = true;
                //    DBdataEntryPentra(thrdData);
                //}).Start();
                Task.Run(() => DBdataEntryPentra(thrdData));
                pentraTempResult = "";
            }


            
                //cobase44Port.Write(new byte[] { 0x02, 0x06, 0x03 }, 0, 3);
                //tempResult = "";
            

        }

        private async Task DBdataEntryPentra(String strData)
        {
            #region old
            //try
            //{
            //    strData = strData.Replace("-", "");
            //    string resultStr = hexToString(strData);
            //    //string Pdata = hexToString(strData.Substring(strData.IndexOf("17") + 2, strData.Length - strData.IndexOf("17") - 6));
            //    //var cmpltDt = DateTime.ParseExact(Pdata.Substring(0, 12), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
            //    //var ordDt = DateTime.ParseExact(resultStr.Substring(33, 12), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
            //    //DateTime cpltedDate = DateTime.ParseExact(Pdata.Substring(0, 11), "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture);
            //    //string ordrdDate=
            //    //string str = "";
            //    //for (int i = 0; i < strData.Length; i += 4)
            //    //    str += (char)Int16.Parse(strData.Substring(i, 4), NumberStyles.AllowHexSpecifier);
            //    //strData = hexToString(strData);

            //    DBHelper DB = new DBHelper();

            //    List<DBCommand> commands = new List<DBCommand>();
            //    using (DBCommand cmdInsert = new DBCommand("insert into lab_results_pentra (data) values (@data)"))
            //    {
            //        cmdInsert.Parameters.AddWithValue("@data", resultStr);

            //        commands.Add(cmdInsert);

            //        var result = DB.ExecuteNonQueriesInTransaction(commands);


            //    }

            //}
            //catch (Exception e)
            //{
            //    return;
            //}
            #endregion
            try
            {
                strData = strData.Replace("-", "");
                string resultStr = hexToString(strData);
                const char LF = '\u000D';
                const char soh = (char)01;
                const char stx = (char)02;
                const char enq = (char)05;
                resultStr = resultStr.Split(enq)[1];
                string[] words = resultStr.Split(LF);

                string testID = "", sample = "", rsltStrTmp = "";
                DateTime cmpltDt = DateTime.Now;
                DateTime ordDt = DateTime.Now;
                foreach (string s in words)
                {
                    if (s.Contains(stx + "1H"))
                    {
                        string[] temp = s.Split('|');
                        cmpltDt = DateTime.ParseExact(temp[13].Trim(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    }
                    else if (s.Contains(stx+"2P"))
                    {
                        string[] temp = s.Split('|');
                        testID = temp[5].Trim();
                    }
                    else if (s.Contains(stx+"3O"))
                    {
                        string[] temp = s.Split('|');
                        sample = temp[2].Replace(soh.ToString(), "").Trim();
                    }
                    else if (s.Contains("^WBC^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "WBC@" + temp[3] + "_";
                    }
                    else if (s.Contains("^LYM%^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp +="LYM@"+temp[3]+ "_";
                    }
                    else if (s.Contains("^MON%^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "MON@" + temp[3] + "_";
                    }
                    else if (s.Contains("^NEU%^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "NEU@" + temp[3] + "_";
                    }
                    else if (s.Contains("^EOS%^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "EOS@" + temp[3] + "_";
                    }
                    else if (s.Contains("^BAS%^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "BAS@" + temp[3] + "_";
                    }
                    else if (s.Contains("^RBC^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "RBC@" + temp[3] + "_";
                    }
                    else if (s.Contains("^HGB^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "HGB@" + temp[3] + "_";
                    }
                    else if (s.Contains("^HCT^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "HCT@" + temp[3] + "_";
                    }
                    else if (s.Contains("^MCV^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "MCV@" + temp[3] + "_";
                    }
                    else if (s.Contains("^MCH^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "MCH@" + temp[3] + "_";
                    }
                    else if (s.Contains("^MCHC^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "MCHC@" + temp[3] + "_";
                    }
                    else if (s.Contains("^RDW^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "RDW@" + temp[3] + "_";
                    }
                    else if (s.Contains("^PLT^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "PLT@" + temp[3] + "_";
                    }
                    else if (s.Contains("^MPV^"))
                    {
                        string[] temp = s.Split('|');
                        rsltStrTmp += "MPV@" + temp[3] + "_";
                    }

                }
                rsltStrTmp = rsltStrTmp.Substring(0, rsltStrTmp.Length - 1);

                DBHelper DB = new DBHelper();

                using (DBCommand cmd = new DBCommand("select nextval('lab_machine_data_id_seq');"))
                {
                    string nxtId = Convert.ToString(DB.ExecuteScalar(cmd));


                    List<DBCommand> commands = new List<DBCommand>();
                    using (DBCommand cmdInsert = new DBCommand("insert into lab_machine_data(id,test_id,sample_id,result_string,completed_date,pentra)values(@id,@test_id,@sample_id,@result_string,@completed_date,@pentra)"))
                    {
                        cmdInsert.Parameters.AddWithValue("@id", nxtId);
                        cmdInsert.Parameters.AddWithValue("@test_id", sample);
                        cmdInsert.Parameters.AddWithValue("@sample_id", sample);
                        cmdInsert.Parameters.AddWithValue("@result_string", rsltStrTmp);
                      //  cmdInsert.Parameters.AddWithValue("@msg_Str", testID);
                        cmdInsert.Parameters.AddWithValue("@completed_date", cmpltDt);
                        cmdInsert.Parameters.AddWithValue("@pentra", 1);
                        commands.Add(cmdInsert);

                        var result = DB.ExecuteNonQueriesInTransaction(commands);


                    }
                }

            }
            catch (Exception e)
            {
                return;
            }
        }



        private void disconnect_pentraEs60_Click(object sender, RoutedEventArgs e)
        {
            //pentraEs60Port = new System.IO.Ports.SerialPort();
            if (pentraEs60Port!= null && pentraEs60Port.IsOpen)
            {
                connect_pentraEs60.Content = "Connect";
                connect_pentraEs60.IsEnabled = true;
                pentraEs60Port.Close();
                System.Windows.Forms.MessageBox.Show("Disconnected");
            }
        }
        #endregion


    }
}

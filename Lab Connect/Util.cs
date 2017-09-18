using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
namespace Common
{
    public static class Util
    {
        #region Date related
        public static DateTime? GetDateTimeFromString(string datetime)
        {
            if (datetime == "")
                return new DateTime();

            DateTime dt;
            if (DateTime.TryParse(datetime, out dt))
            {
                return dt;
            }
            return null;
        }
        public static DateTime GetDate(int year, int month, int day)
        {
            return new DateTime(year, month, day);
        }
        public static DateTime GetDateAndTime(int year, int month, int day, int hour, int mint, int second)
        {
            return new DateTime(year, month, day, hour, mint, second);
        }
        public static string GetDateString(int year, int month, int day)
        {
            return new DateTime(year, month, day).ToString("yyyy-MM-dd");
        }
        public static string GetDateAndTimeString(int year, int month, int day, int hour, int mint, int second)
        {
            return new DateTime(year, month, day, hour, mint, second).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ConvertToShortDateString(string date)
        {
            DateTime dt;
            if (DateTime.TryParse(date, out dt))
            {
                return dt.ToString("dd/MM/yyyy");
            }
            return "";
        }
        public static string ConvertToDateTimeString(string date)
        {
            DateTime dt;
            if (DateTime.TryParse(date, out dt))
            {
                return dt.ToString("dd/MM/yyyy HH:mm:ss");
            }
            return null;
        }

        public static string ConvertToDateTime12HrString(string date)
        {
            DateTime dt;
            if (DateTime.TryParse(date, out dt))
            {
                return dt.ToString("dd/MM/yyyy hh:mm:ss tt");
            }
            return null;
        }

        public static string GetDateTimeStringFromStringDBFormat(string dateTime)
        {
            DateTime dt;
            if (DateTime.TryParse(dateTime, out dt))
            {
                DateTime newdt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Month, dt.Day);
                return newdt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return null;
        }
        #endregion

        public static void SetAllTextboxSingleLine(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is TextBox)
                {
                    TextBox txt = (TextBox)control;
                    txt.Multiline = false;
                    txt.WordWrap = false;
                }
                if (control is GroupBox || control is Panel)
                {
                    SetAllTextboxSingleLine(control);
                }
            }
        }

        public static double RoundToNearest50(double amount)
        {
            var value = (Math.Round(amount * 2, MidpointRounding.AwayFromZero)) / 2;
            return value;
        }
        public static decimal RoundToNearest50(decimal amount)
        {
            double d = Convert.ToDouble(amount);
            decimal dec = Convert.ToDecimal((Math.Round(d * 2, MidpointRounding.AwayFromZero)) / 2);
            return dec;
        }
        public static decimal RoundTo2Decimal(decimal amount)
        {
            try
            {
                decimal step = (decimal)Math.Pow(10, 2);
                decimal tmp = Math.Truncate(Convert.ToDecimal(step) * amount);
                decimal rslt = tmp / step;
                return Convert.ToDecimal(String.Format("{0:0.00}", rslt));
            }
            catch (Exception eg)
            {
                Logger.Error("Error in rounding. fun : RoundTo2Decimal", eg);
                return amount;
            }
        }
        #region Logger
        private static readonly ILog _Logger = LogManager.GetLogger(typeof(Util));
        public static ILog Logger
        {
            get
            {
                log4net.Config.XmlConfigurator.Configure();
                log4net.GlobalContext.Properties["AppName"] = ConfigurationManager.AppSettings["ApplicationName"];
                return _Logger;

            }
        }
        #endregion


        public enum PrefixTypes
        {
            OPNumber = 1,
            ShareHolder = 2,
            OutPatient = 3,
            BillNumber = 4,
            Employee = 5,
            General = 6,
            Privilege = 7,

        }

        //public static void ShowInfoMessage(string message, bool autoClose = true, int seconds = 3)
        //{
        //    InformationMessageBox infoBox = new InformationMessageBox();
        //    infoBox.ShowMessage(message, autoClose, seconds);
        //    infoBox.ShowDialog();
        //}
        public static List<string> RoomRentItemCodeList
        {
            get
            {
                List<string> list = new List<string>();
                list.Add("05229");
                list.Add("05228");
                list.Add("10167");
                list.Add("00013");
                return list;
            }
        }
        public static void SetGridColumnsForCurrency(DataGridView dg)
        {
            DataGridViewColumnCollection columns = dg.Columns;
            foreach (DataGridViewColumn col in columns)
            {
                string colName = col.HeaderText.ToString().ToLower();
                if (colName.Contains("amount") || colName.Contains("price") || colName.Contains("rate") || colName.Contains("total") || colName.Contains("qty") || colName.Contains("quantity"))
                {
                    col.DefaultCellStyle.Format = "N2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        public static void SetFormControlFormattingForCurrency(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is DataGridView)
                {
                    SetGridColumnsForCurrency((DataGridView)control);
                }
                if (control is GroupBox || control is Panel)
                {
                    SetFormControlFormattingForCurrency(control);
                }
            }
        }

        public static string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        public static string NumberToWordsWithDecimal(string amount)
        {
            if (amount.Contains('.'))
            {
                int rupees = Convert.ToInt32(amount.Split(new char[] { '.' })[0]);
                int paisa = Convert.ToInt32(amount.Split(new char[] { '.' })[1]);
                string rupeesStr = NumberToWords(rupees);
                string amt;
                if (paisa > 0)
                {
                    amt = rupeesStr + " And " + NumberToWords(paisa) + " Paisa ";
                }
                else
                {
                    amt = rupeesStr;
                }
                return amt;
            }
            else
            {
                int rupees = Convert.ToInt32(amount);
                string rupeesStr = NumberToWords(rupees);
                return rupeesStr;
            }
        }
        private static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";
            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;

            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }


            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            words = textInfo.ToTitleCase(words);
            return words;
        }
        public static void showError(string errMessage, string applicationName = "")
        {

            MessageBox.Show(errMessage, applicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void showSucess(string sucessMessage, string applicationName = "")
        {

            MessageBox.Show(sucessMessage, applicationName, MessageBoxButtons.OK);
        }
        public static string GetSetting()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string settings = System.Configuration.ConfigurationManager.AppSettings["LabBill"];

            return settings;
        }
        public static string PharmacyBillTypeCodes = "'11','14','AG','BT','IJ','IK','CP','CPC'";
        //public static string PharmacyBillTypeCodes = "'11','14','AG','BT','IJ','IK'";
        public static List<string> PharmacyBillTypeCodeList
        {
            get
            {
                List<string> list = new List<string>();
                list.Add("11");
                list.Add("14");
                list.Add("AG");
                list.Add("BT");
                list.Add("IJ");
                list.Add("IK");
                list.Add("CP");
                list.Add("CPC");
                return list;
            }
        }
        public static List<string> LabBillTypes
        {
            get
            {
                List<string> labBillTypes = new List<string>();
                //'13','DIA','44','45','AH','CJ','2'
                labBillTypes.Add("13");
                labBillTypes.Add("DIA");
                labBillTypes.Add("44");
                labBillTypes.Add("45");
                labBillTypes.Add("AH");
                labBillTypes.Add("CJ");
                labBillTypes.Add("2");
                labBillTypes.Add("42"); // Health checkup
                return labBillTypes;
            }
        }
        public static string LabBillTypesCSV
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                bool isFirst = true;
                foreach (var lbt in Util.LabBillTypes)
                {
                    if (!isFirst)
                    {
                        sb.Append(",");
                    }
                    sb.AppendFormat("'{0}'", lbt);
                    isFirst = false;
                }
                return sb.ToString();
            }
        }
        public static bool Checkvalidmobilenumber(string mobile)
        {
            if (mobile != "")
            {
                Regex mobilePattern = new Regex(@"^([0]|\+91)?[789]\d{9}$");

                if (mobilePattern.IsMatch(mobile))
                    return true;
                else
                    return false;
            }
            return true;
        }
    }
}

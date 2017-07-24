using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Configuration;
using System.Windows.Forms;

namespace Lab_Connect
{
    class DbConnection
    {
        private NpgsqlConnection con;
        public NpgsqlConnection Connection
        {
            get
            {
                if (con == null)
                {
                    if (ConfigurationManager.ConnectionStrings["TiaHMSConnection"] == null)
                    {
                        MessageBox.Show("error:not establish connection");
                        throw new Exception("can't read connection");
                    }


                    con = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["TiaHMSConnection"].ConnectionString);
                }
                return con;
            }
        }
        public void Open()
        {
            Connection.Open();
        }
        public void Close()
        {
            if (con != null)
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                    con.ClearPool();
                }
            }
        }
    }
}

using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Lab_Connect
{
    public class DBHelper
    {

        DbConnection DbCon = new DbConnection();
        public NpgsqlConnection GetConnection()
        {
            return DbCon.Connection;
        }
        public void CloseConnection(NpgsqlConnection con)
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

        public TransactionResult ExecuteNonQueriesInTransaction(List<DBCommand> commands, NpgsqlConnection Con, NpgsqlTransaction transaction)
        {

            TransactionResult transResult = new TransactionResult();

            bool result = true;
            string sqlComm = string.Empty;
            List<bool> listResults = new List<bool>();
            try
            {

                foreach (var dbCommand in commands)
                {
                    NpgsqlCommand cmd = dbCommand.GetInternalCommand();
                    cmd.Connection = Con;
                    cmd.Transaction = transaction;
                    var r = cmd.ExecuteNonQuery() > 0;
                    if (!r)
                    {
                        sqlComm = cmd.CommandText;
                    }
                    listResults.Add(r);

                }
                foreach (var r in listResults)
                {
                    //result = result && r;
                }
                if (result)
                {

                    transResult.IsSucceess = true;
                }
                else
                {


                    transResult.IsSucceess = false;
                    transResult.ErrorMessage = "One of the transaction didn't insert/update/delete any record.";
                    transResult.ErrorCommandQuery = sqlComm;
                }


                return transResult;
            }
            catch (NpgsqlException ex)
            {
                //Log here

                transResult.IsSucceess = false;
                transResult.ErrorMessage = ex.Message;
                transResult.Description = ex.ToString();
                transResult.ErrorCommandQuery = ex.ErrorSql;
                //Util.Logger.Error(string.Format("Error from ExecuteNonQueriesInTransaction part of transact:{0}:{1}", ex.Message, ex.ErrorSql), ex);
                throw ex;
            }
            finally
            {

            }
            return transResult;
        }

        public TransactionResult ExecuteNonQueriesInTransaction(List<DBCommand> commands)
        {
            DbCon.Open();
            TransactionResult transResult = new TransactionResult();
            NpgsqlTransaction transaction = DbCon.Connection.BeginTransaction();
            bool result = true;
            string sqlComm = string.Empty;
            List<bool> listResults = new List<bool>();
            try
            {

                foreach (var dbCommand in commands)
                {
                    NpgsqlCommand cmd = dbCommand.GetInternalCommand();
                    cmd.Connection = DbCon.Connection;
                    cmd.Transaction = transaction;
                    var r = cmd.ExecuteNonQuery() > 0;
                    if (!r)
                    {
                        sqlComm = cmd.CommandText;
                    }

                    if (!sqlComm.Trim().ToLower().StartsWith("delete"))
                    {
                        listResults.Add(r);
                    }

                }
                foreach (var r in listResults)
                {
                    //result = result && r;
                }
                if (result)
                {
                    transaction.Commit();
                    transResult.IsSucceess = true;
                }
                else
                {

                    transaction.Rollback();
                    transResult.IsSucceess = false;
                    transResult.ErrorMessage = "One of the transaction didn't insert/update/delete any record.";
                    transResult.ErrorCommandQuery = sqlComm;
                    //Util.Logger.Error(string.Format("ExecuteNoqueries:No error, but return was false:{0}", sqlComm));
                }

                DbCon.Close();

                return transResult;
            }
            catch (NpgsqlException ex)
            {
                //Log here
                transaction.Rollback();
                transResult.IsSucceess = false;
                transResult.ErrorMessage = ex.Message;
                transResult.Description = ex.ToString();
                transResult.ErrorCommandQuery = ex.ErrorSql;
                //Util.Logger.Error(string.Format("Error from ExecuteNonQueriesInTransaction:{0}:{1}", ex.Message, ex.ErrorSql), ex);
                //DbCon.Close();
                throw ex;
            }
            finally
            {
                DbCon.Close();
            }
            return transResult;
        }
        public bool ExecuteNonQuery(DBCommand dbCommand)
        {
            bool result = false;
            NpgsqlCommand cmd = dbCommand.GetInternalCommand();
            cmd.Connection = DbCon.Connection;
            try
            {
                DbCon.Open();
                cmd.ExecuteNonQuery();
                DbCon.Close();
                result = true;
            }
            catch (NpgsqlException ex)
            {
                //Util.Logger.Error(string.Format("Error from ExecuteNonQuery:{0}:{1}", ex.Message, ex.ErrorSql), ex);
                //Log error
                throw ex;
            }
            finally
            {
                DbCon.Close();
            }
            return result;
        }
        public bool ExecuteNonQuery(DBCommand dbCommand, NpgsqlConnection Con, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand cmd = dbCommand.GetInternalCommand();
            cmd.Connection = Con;
            cmd.Transaction = transaction;
            try
            {

                cmd.ExecuteNonQuery();
                result = true;
            }
            catch (NpgsqlException ex)
            {
                //Log error
                //Util.Logger.Error(string.Format("Error from ExecuteNonQuery part of trans:{0}:{1}", ex.Message, ex.ErrorSql), ex);
                throw ex;
            }
            finally
            {

            }
            return result;
        }
        public object ExecuteScalar(DBCommand dbCommand)
        {
            object result = null;
            NpgsqlCommand cmd = dbCommand.GetInternalCommand();
            cmd.Connection = DbCon.Connection;
            try
            {
                DbCon.Open();
                result = cmd.ExecuteScalar();
                DbCon.Close();

            }
            catch (NpgsqlException ex)
            {
                //Log error

                //Util.Logger.Error(string.Format("Error from ExecuteScalar part of trans:{0}:{1}", ex.Message, ex.ErrorSql), ex);
                throw ex;
            }
            finally
            {
                DbCon.Close();
            }
            return result;
        }
        public DataTable FillDataTable(DBCommand dbCommand)
        {

            DataTable result = new DataTable();

            NpgsqlCommand cmd = dbCommand.GetInternalCommand();

            cmd.Connection = DbCon.Connection;

            try
            {
                DbCon.Open();
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                da.Fill(result);
                DbCon.Close();

            }

            catch (NpgsqlException ex)
            {
                //Log error
                string errorCommand = ex.ErrorSql;
                string e = ex.ToString();
                //Util.Logger.Error(string.Format("Error from ExecuteScalar part of trans:{0}:{1}", ex.Message, ex.ErrorSql), ex);
                //throw ex;
            }

            finally
            {
                DbCon.Close();
            }

            return result;

        }

    }

    public class DBCommand : IDisposable
    {
        private NpgsqlCommand _InternalCommand;
        public NpgsqlCommand GetInternalCommand()
        {
            _InternalCommand.CommandType = CommandType.Text;
            _InternalCommand.CommandText = SQLQuery;
            return _InternalCommand;
        }

        public DBCommand()
        {
            _InternalCommand = new NpgsqlCommand();
            Parameters.Clear();
        }
        public DBCommand(string sqlQuery)
        {
            _InternalCommand = new NpgsqlCommand();
            this.SQLQuery = sqlQuery;
            Parameters.Clear();

        }
        public string SQLQuery { get; set; }
        public NpgsqlParameterCollection Parameters
        {
            get
            {
                return _InternalCommand.Parameters;
            }
        }

        public void Dispose()
        {
            SQLQuery = "";
            _InternalCommand.Dispose();
        }
    }

    public class TransactionResult
    {
        public bool IsSucceess { get; set; }
        public string ErrorMessage { get; set; }
        public string Description { get; set; }
        public string ErrorCommandQuery { get; set; }

        public TransactionResult()
        {
            IsSucceess = false;
            ErrorMessage = "";
            Description = "";
            ErrorCommandQuery = "";
        }
    }

}

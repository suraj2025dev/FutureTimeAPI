using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using static Dapper.SqlMapper;
using System.Transactions;
using Library.Data;

namespace Library
{
    public class Dao1
    {

        private static int GetConnectionTimeOut()
        {
           return 100;
        }

        /// <summary>
        /// Create transaction scope. It must be created before creating connection, so it get automatically enlisted in the transaction.
        /// </summary>
        /// <param name="isolationLevel">Default is ReadCommitted. Even if you provide any other value, it wont accept it.</param>
        /// <param name="tso">Keep is as it is. No need to provide value to this param.</param>
        /// <returns></returns>
        public static TransactionScope CreateTransactionScope(System.Transactions.IsolationLevel isolationLevel = System.Transactions.IsolationLevel.ReadCommitted, TransactionScopeOption tso = TransactionScopeOption.Required)
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            options.Timeout = TimeSpan.FromSeconds(GetConnectionTimeOut());

            var scope = new TransactionScope(tso, options);

            return scope;
        }

        public static NpgsqlConnection CreateConnection()
        {
            var conn = new NpgsqlConnection(AppStatic.DB_CONN);
            conn.Open();
            return conn;
        }

        //TODO: SQLBULKCOPY

        //For insert/update operation
        public static int Execute(string sql, object param, NpgsqlConnection connection = null)
        {
            int affectedRows = 0;
            if (connection == null)
            {
                using (connection = CreateConnection())
                {
                    affectedRows = connection.Execute(sql, param, commandTimeout: GetConnectionTimeOut());
                }
            }
            else
            {
                affectedRows = connection.Execute(sql, param, commandTimeout: GetConnectionTimeOut());
            }

            return affectedRows;
        }

        public static DataSet ExecuteDataSet(string sql, object param = null, NpgsqlConnection connection = null)
        {
            IDataReader reader;
            var ds = new DataSet();
            if (connection == null)
            {
                using (connection = CreateConnection())
                {
                    reader = connection.ExecuteReader(sql, param, commandTimeout: GetConnectionTimeOut());

                    while (!reader.IsClosed)
                        ds.Tables.Add().Load(reader);
                }
            }
            else
            {
                reader = connection.ExecuteReader(sql, param, commandTimeout: GetConnectionTimeOut());

                while (!reader.IsClosed)
                    ds.Tables.Add().Load(reader);
            }
            return ds;
        }

        public static T ExecuteScalar<T>(string sql, object param = null, NpgsqlConnection connection = null)
        {
            if (connection == null)
            {
                using (connection = CreateConnection())
                {
                    return connection.ExecuteScalar<T>(sql, param, commandTimeout: GetConnectionTimeOut());
                }
            }
            else
            {
                return connection.ExecuteScalar<T>(sql, param, commandTimeout: GetConnectionTimeOut());
            }
        }

        /// <summary>
        /// Returns list of object T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(string sql, object param = null, NpgsqlConnection connection = null)
        {
            if (connection == null)
            {
                using (connection = CreateConnection())
                {
                    return connection.Query<T>(sql, param, commandTimeout: GetConnectionTimeOut());
                }
            }
            else
            {
                return connection.Query<T>(sql, param, commandTimeout: GetConnectionTimeOut());
            }
        }

        /// <summary>
        /// Here connection parameter is not set to default null. Reader can't read in isolated connection. It must be in same connection.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static GridReader QueryMultipleAsync(string sql, object param, NpgsqlConnection connection)
        {

            if (connection == null)
            {
                using (connection = CreateConnection())
                {
                    return connection.QueryMultipleAsync(sql, param, commandTimeout: GetConnectionTimeOut()).Result;
                }
            }
            else
            {
                return connection.QueryMultipleAsync(sql, param, commandTimeout: GetConnectionTimeOut()).Result;
            }
        }
    }


}

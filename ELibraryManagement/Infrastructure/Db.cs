using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ELibraryManagement.Infrastructure
{
    /// <summary>Single place for database access. Every query is parameterized and every connection is disposed.</summary>
    public static class Db
    {
        static string ConnStr { get { return ConfigurationManager.ConnectionStrings["con"].ConnectionString; } }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable Query(string sql, params SqlParameter[] args) { return Fill(sql, CommandType.Text, args); }
        public static DataTable Proc(string name, params SqlParameter[] args) { return Fill(name, CommandType.StoredProcedure, args); }

        public static int Exec(string sql, params SqlParameter[] args)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(args);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] args)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(args);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        static DataTable Fill(string text, CommandType type, SqlParameter[] args)
        {
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(text, con) { CommandType = type })
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddRange(args);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Dazzler.Test
{
   /// <summary>
   /// used to create an instace of the DataProvider class.
   /// </summary>
   /// <typeparam name="TProvider"></typeparam>
   public static class DatabaseProvider<TProvider> where TProvider : DatabaseProvider
   {
      public static TProvider Instance { get; } = Activator.CreateInstance<TProvider>();
   }


   public abstract class DatabaseProvider
   {
      public abstract DbProviderFactory Factory { get; }
      public abstract string ConnectionString { get; }

      public virtual void Dispose() { }


      public DbConnection GetOpenConnection()
      {
         var conn = Factory.CreateConnection();
         conn.ConnectionString = this.ConnectionString;
         conn.Open();
         if (conn.State != ConnectionState.Open) throw new InvalidOperationException("not opened!");
         return conn;
      }
      public DbConnection GetClosedConnection()
      {
         var conn = Factory.CreateConnection();
         conn.ConnectionString = this.ConnectionString;
         return conn;
      }
      public DbParameter CreateParameter(string name, object value)
      {
         var p = Factory.CreateParameter();
         p.ParameterName = name;
         p.Value = value ?? DBNull.Value;
         return p;
      }
   }



   public sealed class SqlServerClientProvider : DatabaseProvider
   {
      public override DbProviderFactory Factory => System.Data.SqlClient.SqlClientFactory.Instance;
      public override string ConnectionString => @"Server=(local);Database=tempdb;User ID=sa;Password=Password12!";
   }



   public sealed class MySqlProvider : DatabaseProvider
   {
      public override DbProviderFactory Factory => null; // MySql.Data.MySqlClient.MySqlClientFactory.Instance;
      public override string ConnectionString => @"Server=localhost;Database=test;Uid=root;Pwd=Password12!;";



      public DbConnection GetMySqlConnection(bool open = true, bool convertZeroDatetime = false, bool allowZeroDatetime = false)
      {
         string cs = this.ConnectionString;
         var csb = Factory.CreateConnectionStringBuilder();
         csb.ConnectionString = cs;
         ((dynamic)csb).AllowZeroDateTime = allowZeroDatetime;
         ((dynamic)csb).ConvertZeroDateTime = convertZeroDatetime;
         var conn = Factory.CreateConnection();
         conn.ConnectionString = csb.ConnectionString;
         if (open) conn.Open();
         return conn;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Dazzler.Test
{
   public abstract class TestBase<TProvider> : IDisposable where TProvider : DatabaseProvider
   {
      public TProvider Provider { get; } = DatabaseProvider<TProvider>.Instance;

      protected DbConnection GetOpenConnection() => Provider.GetOpenConnection();
      protected DbConnection GetClosedConnection() => Provider.GetClosedConnection();
      
      protected DbConnection _connection;
      protected DbConnection connection => _connection ?? (_connection = Provider.GetOpenConnection());


      static TestBase()
      {
         Console.WriteLine("Dazzler  : " + typeof(Dazzler.Mapper).AssemblyQualifiedName);
         var provider = DatabaseProvider<TProvider>.Instance;
         Console.WriteLine("Connection String: {0}", provider.ConnectionString);
         var factory = provider.Factory;
         Console.WriteLine("Provider: {0}", factory.GetType().FullName);
         Console.WriteLine(".NET: " + Environment.Version);
      }

      public virtual void Dispose()
      {
         _connection?.Dispose();
         _connection = null;
         Provider?.Dispose();
      }
   }
}

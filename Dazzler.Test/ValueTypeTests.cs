using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using Dazzler.Models;


namespace Dazzler.Test
{
   [TestClass]
   public sealed class ValueTypeTests : ValueTypeTests<SqlServerClientProvider> { }

   public class ValueTypeTests<TProvider> : TestBase<TProvider> where TProvider : DatabaseProvider
   {
      #region query value type tests

      [TestMethod]
      public void QueryStringValue()
      {
         ResultInfo ri = new ResultInfo();

         string value = "hello Dazzler!";

         var result = connection.Query<ValueTestResult>(CommandType.Text, $"select '{value}' String", ri: ri);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreEqual(value, result.FirstOrDefault()?.String, "Invalid value.");
      }

      [TestMethod]
      public void QueryDateTimeValue()
      {
         DateTime value = new DateTime(2021, 12, 31, 23, 59, 59);

         var result = connection.Query<ValueTestResult>(CommandType.Text, $"select '{value}' DateTime", null);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreEqual(value, result.FirstOrDefault()?.DateTime, "Invalid value.");
      }

      [TestMethod]
      public void QueryIntValue()
      {
         int value = int.MaxValue;

         var result = connection.Query<ValueTestResult>(CommandType.Text, $"select {value} Integer", null);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreEqual(value, result.FirstOrDefault()?.Integer, "Invalid value.");
      }

      [TestMethod]
      public void QueryDecimalValue()
      {
         decimal value = 987654321.12345M;

         var result = connection.Query<ValueTestResult>(CommandType.Text, $"select {value} Decimal", null);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreEqual(value, result.FirstOrDefault()?.Decimal, "Invalid value.");
      }

      [TestMethod]
      public void QueryDoubleValue()
      {
         double value = 123456789.987654D;

         var result = connection.Query<ValueTestResult>(CommandType.Text, $"select {value} [Double]", null);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreEqual(value, result.FirstOrDefault()?.Double, "Invalid value.");
      }

      [TestMethod]
      public void QueryGuidValue()
      {
         var result = connection.Query<ValueTestResult>(CommandType.Text, $"select NEWID() Guid", null);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreNotEqual(Guid.Empty, result.FirstOrDefault()?.Guid, "Invalid value.");
      }

      #endregion

      #region non-query strongly-typed input test
      [TestMethod]
      public void OutValueString_strong_typed()
      {
         var args = new QueryParameterModel()
         {
            value1 = "hello Dazzler",
            value2 = ""
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2, "Invalid output value.");
      }
      #endregion

      #region non-query output parameter tests

      [TestMethod]
      public void OutValueString()
      {
         var args = new
         {
            value1 = "hello Dazzler",
            value2__out = ""
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
      }

      [TestMethod]
      public void OutValueDateTime()
      {
         var args = new
         {
            value1 = new DateTime(2021, 12, 31, 23, 59, 59),
            value2__out = (DateTime?)null
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
      }

      [TestMethod]
      public void OutValueInt()
      {
         var args = new
         {
            value1 = int.MaxValue,
            value2__out = 0
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
      }

      [TestMethod]
      public void OutValueDecimal()
      {
         var args = new
         {
            value1 = decimal.MaxValue,
            value2__out = 0M
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
      }

      [TestMethod]
      public void OutValueDoule()
      {
         var args = new
         {
            value1 = 123456789.321D,
            value2__out = 0D
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
      }

      [TestMethod]
      public void OutValueGuid()
      {
         var args = new
         {
            value1 = Guid.NewGuid(),
            value2__out = (Guid?)null
         };

         var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
      }

      #endregion

      #region query with output parameter test

      [TestMethod]
      public void QueryWithOutputStringValue()
      {
         var args = new
         {
            value1 = "hello Dazzler",
            value2__out = ""
         };

         var result = connection.Query<ValueTestResult>(CommandType.Text, $"set @value2=@value1  select @value1 String ", args);
         Assert.AreEqual(1, result.Count, "Invalid row.");
         Assert.AreEqual(args.value1, result.FirstOrDefault().String, "Invalid output resultset.");
         Assert.AreEqual(args.value1, args.value2__out, "Invalid output parameter value.");
      }

      #endregion
   }
}

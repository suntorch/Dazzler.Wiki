using System;
using System.Data;

namespace Dazzler.Test
{
   public class ValueTestResult
   {
      public string String { get; set; }
      public int Integer { get; set; }
      public DateTime DateTime { get; set; }
      public decimal Decimal { get; set; }
      public double Double { get; set; }
      public Guid Guid { get; set; }
   }

   public class QueryTestResult
   {
      public string Name { get; set; }
      public int Age { get; set; }
      public DateTime Dob { get; set; }
      public decimal Money { get; set; }
   }

   public class QueryParameterModel
   {
      /// <summary>
      ///  input parameter.
      /// </summary>
      public string value1 { get; set; }

      /// <summary>
      /// output parameter.
      /// </summary>
      [Bind(ParameterDirection.Output, 200)]
      public string value2 { get; set; }
   }
}

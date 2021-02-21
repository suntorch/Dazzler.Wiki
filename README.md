# Dazzler - a simple object mapper for .Net

## Features
Dazzler is a NuGet data access library that extends IDbConnection interface.

- lightweight and high performance. :rocket: 
- mapping a query result :scroll: to **`strongly-typed`** object.
- 2-way binding :link: a class property to **`input`** and **`output`** parameters.



## Parameterized Query
A **Strongly-Typed**, **Anonymous** and **ExpandoObject** object can be passed as query parameters
and a property name should match with query parameter name in order to bind it. 
After a query is executed, **Output** and **Function Return** parameter will automatically take 
the value that is returned by a query. You don't have to do any extra work. :+1:

There are 2 methods to specify a direction of the query parameter.

- **`BindAttribute`** attribute class.
- special **`suffixes`** for the property name.

For the **strongly-typed** class type, both methods can be used.
But, Anonymous class type does not allow any attribute implementation, 
therefore, you will have to use **suffixes** in order to specify a direction.

Implementing both methods together in the **strongly-typed** class type is not recommended.
If both methods are specified together then only the BindAttribute will be used and 
Property Name Suffixes will be ignored.


### BindAttribute
This attribute is used to specify a query parameter information.

```C#
   public class QueryParameterModel
   {
      /// <summary>
      ///  Without BindAttribute the property will be bound as input parameter.
      /// </summary>
      public string value1 { get; set; }

      /// <summary>
      /// The BindAttribute specifies that the property will be bound as output parameter.
      /// </summary>
      [Bind(ParameterDirection.Output, 200)]
      public string value2 { get; set; }
   }
```


### Property Name Suffixes
A suffix is a special notation that is specified at the end of the property name
and it must use the pattern **PropertyName`[__in|out|ret[size]]`**.

A suffix consists of the following components:
| Component | Description |
| --- | --- |
|`__`| an identifiers of the suffix. (double Low Line '0x5F')
|`in`| specifies **input** parameter.
|`out`| specifies **output** parameter.
|`inout`| specifies **input** and **output** parameter.
|`ret`| specifies **return** parameter. Used to call database function.
|`size`| specifies a value size of the parameter. For example: `__out50`, `__ret200`, etc.



### Input/Output Parameters
Default direction is always **input** and no need to specify, but you could.

Using Anonymous class type:
```C#
var args = new
{
   value1 = 999, // same as value1__in = 999
   value2__out = 0
};

var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
```

Using Strongly-Typed class type with suffixes:
```C#
public class QueryParameterModel
{
   public int value1 { get; set; }
   public int value2__out { get; set; }
};
```

```C#
QueryParameterModel args = new QueryParameterModel()
{
   value1 = 999,
   value2__out = 0
};

var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
Assert.AreEqual(args.value1, args.value2__out, "Invalid output value.");
```

Using Strongly-Typed class type with attribute:
```C#
public class QueryParameterModel
{
   public int value1 { get; set; }

   [Bind(ParameterDirection.Output)]
   public int value2 { get; set; }
};
```
```C#
QueryParameterModel args = new QueryParameterModel()
{
   value1 = 999,
   value2 = 0
};

var result = connection.NonQuery(CommandType.Text, $"set @value2=@value1", args);
Assert.AreEqual(args.value1, args.value2, "Invalid output value.");
```

### Supported Value Types
It supports all Value-Type types, Enum, Guid, Array, and its nullable form.

```C#
var args = new
{
   stringValue = "Hello Dazzler",
   intValue = 1,
   decimalValue = 99.99,
   dateValue = DateTime.Now,
   guidValue = Guid.NewGuid(),
   enumValue = Level.High,       // it will get underlying value type of the Enum.
   imageData = new byte[1000]    // used for VarBinary
};
```
```C#
var args = new
{
   stringValue = null,           // string is naturally nullable.
   intValue = (int?)null,
   decimalValue = (decimal?)null,
   dateValue = (DateTime?)null,
   guidValue = (Guid?)null,
   enumValue = (Level?)null,
   imageData = (byte[]?)null
};
```


## Execute Commands
There is no big difference to execute a SQL Statement, Stored Procedure and Function, 
unless specifying a command type by **`CommandType`**. 


### Execute SQL Statement

```C#

// assigns the ouput value in SELECT
string sql = "select @Name Name, @Age Age, @Value=99";

string sql = @"
BEGIN
   -- do some business logic.
   select @Name Name, @Age Age

   -- assigns output values.
   set @Value = 99
END";

var args = new
{
   Name = "John",
   Age = 25,
   Value__out = 0
};

var result = connection.Query<ResultModel>(CommandType.Text, sql, args);
Assert.AreEqual(1, result.Count, "Invalid record count.");
Assert.AreEqual(25, result[0].Age, "Fetched wrong record.");
Assert.AreEqual(99, args.Value__out, "Invalid output value.");
```


### Execute Database Stored Procedure

```TSQL
CREATE OR ALTER PROCEDURE MyStoredProcedure
   @Name varchar(50),
   @Age int,
   @Value int OUTPUT
AS
BEGIN
   -- any output parameters.
   set @Value = 99

   -- any returning records.
   select @Name Name, @Age Age
END
```

```C#
var args = new
{
   Name = "John",
   Age = 25,
   Value__out = 0
};

var result = connection.Query<ResultModel>(CommandType.StoredProcedure, "MyStoredProcedure", args);
Assert.AreEqual(1, result.Count, "Invalid record count.");
Assert.AreEqual(25, result[0].Age, "Fetched wrong record.");
Assert.AreEqual(99, args.Value__out, "Invalid output value.");
```

### Execute Database Function

```TSQL
CREATE OR ALTER FUNCTION MyFunction(
   @Name varchar(50),
   @Age int
)
RETURN int
AS
BEGIN
   -- any returning records.
   select @Name Name, @Age Age

   -- function return.
   return 99
END
```

```C#
var args = new
{
   Name = "John",
   Age = 25,
   ReturnValue__ret = 0
};

var result = connection.Query<ResultModel>(CommandType.StoredProcedure, "MyFunction", args);
Assert.AreEqual(1, result.Count, "Invalid record count.");
Assert.AreEqual(25, result[0].Age, "Fetched wrong record.");
Assert.AreEqual(99, args.ReturnValue__out, "Invalid return value.");
```


## Paging
It allows to implement a pagination to fetch a some records from the given offset position.

```C#
string sql = "select Value from ( values (1),(2),(3),(4),(5),(6),(7) ) as tmp (Value)";

var result = connection.Query<ResultModel>(CommandType.Text, sql, offset: 2, limit: 2);

Assert.AreEqual(2, result.Count, "Invalid output record count.");
Assert.AreEqual(3, result[0].Value, "Fetched wrong record.");
Assert.AreEqual(4, result[1].Value, "Fetched wrong record.");
```


## Execution Events
Some application needs to monitor, log and control database actions globally without writing an extra code.
Using the following **pre** and **post** events, it allows to implement such needs.

-  :zap: ExecutingEvent(CommandEventArgs args)
-  :zap: ExecutedEvent(CommandEventArgs args, ResultInfo result)


The use cases can be as follows:

-  :speech_balloon: to monitor/report all database operations.
-  :speech_balloon: to monitor/report top Nth long running queries.
-  :speech_balloon: to accept/reject a query execution in centralized code base.


### Execution Event Implementation

Let's implement a storing all database operation into the DBLog table.
Please be aware of when we execute any database operation from the event 
method, the execution must not trigger an events. Otherwise, it will 
cause deadly recursive call for the event method and it will never end.
Set **`noevent=true`**

```C#
// in program starts
Mapper.ExecutingEvent += Mapper_ExecutingEvent;
Mapper.ExecutedEvent += Mapper_ExecutedEvent;

// in program exits
Mapper.ExecutingEvent -= Mapper_ExecutingEvent;
Mapper.ExecutedEvent -= Mapper_ExecutedEvent;

// event pre-execution method
private void Mapper_ExecutingEvent(CommandEventArgs args)
{
   // the event function will be invoked when a command is coming to execute.
   Console.WriteLine("Executing {0}: {1}", args.Kind, args.Sql);
}

// event post-execution method
private void Mapper_ExecutedEvent(CommandEventArgs args, ResultInfo result)
{
   var param = new
   {
      Started = DateTime.Now,
      Kind = args.Kind,  // no problem with Enum type, it will take a corresponding Int value.
      args.Sql,
      result.Duration,
      Rows = result.AffectedRows
   };

   // ATTENTION: Any database operation in this event function should not trigger events!
   // Otherwise, it will cause deadly recursive call for the event function and it will never end.

   var insertedRows = connection.NonQuery(CommandType.Text
      , "insert into DBLog (Started,Kind,Sql,Duration,Rows) values (@Started,@Kind,@Sql,@Duration,@Rows)"
      , param
      , noevent: true);

   Assert.AreEqual(1, insertedRows, "Invalid inserted log record.");
}
```


## DB providers can be used
It works across all .NET ADO providers including SQL Server, MySQL, Firebird, PostgreSQL and Oracle.



## Examples
You can see :eyes: and learn :green_book: from the test project [Dazzler.Test](https://github.com/suntorch/Dazzler.Wiki/tree/main/Dazzler.Test)



## Installation
Please use the following command in the NuGet Package Manager Console to install the library.
```
Install-Package SunTorch.Dazzler -Version 1.2.3
```

Happy coding! 


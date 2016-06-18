using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Program
{
    public static IList<string> TestBigNeedles = "ECMA-334,ISO/IEC 23270,component-oriented,Garbage collection,exception handling,type-safe,unified type system,versioning,programs,namespaces,types,members,assemblies,applications,libraries,Intermediate Language,metadata,value types,reference types,simple types,enum types,struct types,nullable types,class types,interface types,array types,delegate types,type declarations,boxing,unboxing,variables,Expressions,operands,operators,precedence,overloaded,statements,block,Declaration statements,Expression statements,Selection statements,Iteration statements,Jump statements,Classes,instances,objects,inheritance,polymorphism,derived classes,base classes,static members,instance members,static field,instance field,read-only fields,method,Static methods,Instance methods,parameters,return type,signature,arguments,value parameter,reference parameter,output parameter,parameter array,local variables,definitely assigned,static method,instance method,virtual method,non-virtual method,run-time type,compile-time type,overridden,abstract,overloading,overload resolution,function members,instance constructor,static constructor,Properties,accessors,read-write property,read-only property,write-only property,indexer,event,event handlers,operator,destructor,structs,array,elements,element type,length,single-dimensional array,multi-dimensional arrays,rank,jagged array,array initializer,interface,multiple inheritance,explicit interface member implementations,enum type,underlying type,delegate type,attributes,program,source files,compilation units,lexical grammar,syntactic grammar,Single-line comments,Delimited comments,verbatim identifier,keyword,literal,regular string literals,verbatim string literals,defined,undefined,entry point,application,application domain,Application startup,Application termination,termination status code,declaration space,global declaration space,hide,local variable declaration space,label declaration space,inherited,global namespace,accessible,inaccessible,declared accessibility,accessibility domain,top-level,nested,program text,at least as accessible as,signatures,scope,hidden,visible,fully qualified name,garbage collector,side effect,generic types,type parameters,references,default constructor,default value,null value,Not-a-Number,non-nullable value type,unwrapping,wrapping,instance,object,dynamic expression,boxing class,unbound generic type,type arguments,constructed type,open types,closed types,unbound type,bound type,Expression trees,expression tree types,initially assigned,initially unassigned,static variable,instance variable,local variable,definite assignment state,conversion,implicit,explicit,Explicit nullable conversions,user-defined conversions,lifted conversion operator,source type,target type,most specific,encompassed by,encompass,most encompassing type,most encompassed type,value,binding,static binding,dynamic binding,binding-time,dynamic objects,associativity,left-associative,right-associative,overloadable unary operators,overloadable binary operators,unary operator overload resolution,binary operator overload resolution,numeric promotion,Lifted operators,named argument,positional argument,type inference,inferred result type,inferred return type,applicable function member,better function member,normal form,expanded form,better conversion,eligible,object initializer,nested object initializer,implicitly typed array creation expression,projection initializer,overflow checking context,task,awaitable,awaiter,awaiter type,comparison operators,anonymous function,outer variable,captured,instantiated,Query expressions,range variable,sequence,transparent identifiers,Query expression pattern,simple assignment operator,compound assignment operators,end point,reachable,unreachable,statement list,implicitly typed local variable declaration,governing type,iteration variable,implicitly typed iteration variable,collection type,enumerator type,target,exit,exception propagation,throw point,exception variable,resource,Using directives,namespace alias qualifier,generic class declaration,static class,directly depends on,reference type constraint,value type constraint,depends on,effective base class,effective interface set,known to be a reference type,partial type declarations,defining partial method declaration,implementing partial method declaration,instance type,inherits,nested type,non-nested type,constant,field,static fields,instance fields,static variables,instance variables,readonly fields,volatile fields,volatile read,volatile write,generic method,statement body,expression body,optional parameter,required parameter,overriding,most derived implementation,override method,overridden base method,sealed method,abstract method,external method,partial method,extension method,result type,property,accessor body,external property,static property,instance property,automatically implemented property,read-write,read-only,write-only,inlining,auto-property,overriding property declaration,external event,static event,instance event,overriding event declaration,external indexer,user-defined conversion,external constructor,external static constructor,external destructor,iterator,enumerator interfaces,enumerable interfaces,yield type,enumerator object,before,running,suspended,after,enumerable object,async function,async,task type,resumption delegate,current caller,return task,synchronization context,multi-dimensional array,array covariance,covariant,contravariant,invariant,output-unsafe,input-unsafe,output-safe,input-safe,explicit base interfaces,base interfaces,interface mapping,re-implementing,re-implement,compatible,attribute class,attribute,multi-use attribute class,single-use attribute class,positional parameters,named parameters,attribute parameter types,Attribute specification,attribute sections,attribute instance,conditional methods,conditional attribute classes,conditional attribute class,unsafe code,referent type,Fixed variables,moveable variables,fixed size buffer,documentation comments,documentation generator,documentation file,documentation viewer".Split(',');

    public static string TestBigHaystack = @"
Introduction
C# (pronounced ""See Sharp"") is a simple, modern, object-oriented, and type-safe programming language. C# has its roots in the C family of languages and will be immediately familiar to C, C++, and Java programmers. C# is standardized by ECMA International as the 
 standard and by ISO/IEC as the 
 standard. Microsoft's C# compiler for the .NET Framework is a conforming implementation of both of these standards.
C# is an object-oriented language, but C# further includes support for 
 programming. Contemporary software design increasingly relies on software components in the form of self-contained and self-describing packages of functionality. Key to such components is that they present a programming model with properties, methods, and events; they have attributes that provide declarative information about the component; and they incorporate their own documentation. C# provides language constructs to directly support these concepts, making C# a very natural language in which to create and use software components.
Several C# features aid in the construction of robust and durable applications: 
 automatically reclaims memory occupied by unused objects; 
 provides a structured and extensible approach to error detection and recovery; and the 
 design of the language makes it impossible to read from uninitialized variables, to index arrays beyond their bounds, or to perform unchecked type casts.
C# has a 
. All C# types, including primitive types such as 
 and 
, inherit from a single root 
 type. Thus, all types share a set of common operations, and values of any type can be stored, transported, and operated upon in a consistent manner. Furthermore, C# supports both user-defined reference types and value types, allowing dynamic allocation of objects as well as in-line storage of lightweight structures.
To ensure that C# programs and libraries can evolve over time in a compatible manner, much emphasis has been placed on 
 in C#'s design. Many programming languages pay little attention to this issue, and, as a result, programs written in those languages break more often than necessary when newer versions of dependent libraries are introduced. Aspects of C#'s design that were directly influenced by versioning considerations include the separate 
 and 
 modifiers, the rules for method overload resolution, and support for explicit interface member declarations.
The rest of this chapter describes the essential features of the C# language. Although later chapters describe rules and exceptions in a detail-oriented and sometimes mathematical manner, this chapter strives for clarity and brevity at the expense of completeness. The intent is to provide the reader with an introduction to the language that will facilitate the writing of early programs and the reading of later chapters.
Hello world
The ""Hello, World"" program is traditionally used to introduce a programming language. Here it is in C#:
C# source files typically have the file extension 
. Assuming that the ""Hello, World"" program is stored in the file 
, the program can be compiled with the Microsoft C# compiler using the command line
which produces an executable assembly named 
. The output produced by this application when it is run is
The ""Hello, World"" program starts with a 
 directive that references the 
 namespace. Namespaces provide a hierarchical means of organizing C# programs and libraries. Namespaces contain types and other namespaces—for example, the 
 namespace contains a number of types, such as the 
 class referenced in the program, and a number of other namespaces, such as 
 and 
. A 
 directive that references a given namespace enables unqualified use of the types that are members of that namespace. Because of the 
 directive, the program can use 
 as shorthand for 
.
The 
 class declared by the ""Hello, World"" program has a single member, the method named 
. The 
 method is declared with the 
 modifier. While instance methods can reference a particular enclosing object instance using the keyword 
, static methods operate without reference to a particular object. By convention, a static method named 
 serves as the entry point of a program.
The output of the program is produced by the 
 method of the 
 class in the 
 namespace. This class is provided by the .NET Framework class libraries, which, by default, are automatically referenced by the Microsoft C# compiler. Note that C# itself does not have a separate runtime library. Instead, the .NET Framework is the runtime library of C#.
Program structure
The key organizational concepts in C# are 
, 
, 
, 
, and 
. C# programs consist of one or more source files. Programs declare types, which contain members and can be organized into namespaces. Classes and interfaces are examples of types. Fields, methods, properties, and events are examples of members. When C# programs are compiled, they are physically packaged into assemblies. Assemblies typically have the file extension 
 or 
, depending on whether they implement 
 or 
.
The example
declares a class named 
 in a namespace called 
. The fully qualified name of this class is 
. The class contains several members: a field named 
, two methods named 
 and 
, and a nested class named 
. The 
 class further contains three members: a field named 
, a field named 
, and a constructor. Assuming that the source code of the example is stored in the file 
, the command line
compiles the example as a library (code without a 
 entry point) and produces an assembly named 
.
Assemblies contain executable code in the form of 
 (IL) instructions, and symbolic information in the form of 
. Before it is executed, the IL code in an assembly is automatically converted to processor-specific code by the Just-In-Time (JIT) compiler of .NET Common Language Runtime.
Because an assembly is a self-describing unit of functionality containing both code and metadata, there is no need for 
 directives and header files in C#. The public types and members contained in a particular assembly are made available in a C# program simply by referencing that assembly when compiling the program. For example, this program uses the 
 class from the 
 assembly:
If the program is stored in the file 
, when 
 is compiled, the 
 assembly can be referenced using the compiler's 
 option:
This creates an executable assembly named 
, which, when run, produces the output:
C# permits the source text of a program to be stored in several source files. When a multi-file C# program is compiled, all of the source files are processed together, and the source files can freely reference each other—conceptually, it is as if all the source files were concatenated into one large file before being processed. Forward declarations are never needed in C# because, with very few exceptions, declaration order is insignificant. C# does not limit a source file to declaring only one public type nor does it require the name of the source file to match a type declared in the source file.
Types and variables
There are two kinds of types in C#: 
 and 
. Variables of value types directly contain their data whereas variables of reference types store references to their data, the latter being known as objects. With reference types, it is possible for two variables to reference the same object and thus possible for operations on one variable to affect the object referenced by the other variable. With value types, the variables each have their own copy of the data, and it is not possible for operations on one to affect the other (except in the case of 
 and 
 parameter variables).
C#'s value types are further divided into 
, 
, 
, and 
, and C#'s reference types are further divided into 
, 
, 
, and 
.
The following table provides an overview of C#'s type system.
Category
Description
Value types
Simple types
Signed integral: 
, 
, 
, 
Unsigned integral: 
, 
, 
, 
Unicode characters: 
IEEE floating point: 
, 
High-precision decimal: 
Boolean: 
Enum types
User-defined types of the form 
Struct types
User-defined types of the form 
Nullable types
Extensions of all other value types with a 
 value
Reference types
Class types
Ultimate base class of all other types: 
Unicode strings: 
User-defined types of the form 
Interface types
User-defined types of the form 
Array types
Single- and multi-dimensional, for example, 
 and 
Delegate types
User-defined types of the form e.g. 
The eight integral types provide support for 8-bit, 16-bit, 32-bit, and 64-bit values in signed or unsigned form.
The two floating point types, 
 and 
, are represented using the 32-bit single-precision and 64-bit double-precision IEEE 754 formats.
The 
 type is a 128-bit data type suitable for financial and monetary calculations.
C#'s 
 type is used to represent boolean values—values that are either 
 or 
.
Character and string processing in C# uses Unicode encoding. The 
 type represents a UTF-16 code unit, and the 
 type represents a sequence of UTF-16 code units.
The following table summarizes C#'s numeric types.
Category
Bits
Type
Range/Precision
Signed integral
8
-128...127
16
-32,768...32,767
32
-2,147,483,648...2,147,483,647
64
-9,223,372,036,854,775,808...9,223,372,036,854,775,807
Unsigned integral
8
0...255
16
0...65,535
32
0...4,294,967,295
64
0...18,446,744,073,709,551,615
Floating point
32
1.5 × 10^−45 to 3.4 × 10^38, 7-digit precision
64
5.0 × 10^−324 to 1.7 × 10^308, 15-digit precision
Decimal
128
1.0 × 10^−28 to 7.9 × 10^28, 28-digit precision
C# programs use 
 to create new types. A type declaration specifies the name and the members of the new type. Five of C#'s categories of types are user-definable: class types, struct types, interface types, enum types, and delegate types.
A class type defines a data structure that contains data members (fields) and function members (methods, properties, and others). Class types support single inheritance and polymorphism, mechanisms whereby derived classes can extend and specialize base classes.
A struct type is similar to a class type in that it represents a structure with data members and function members. However, unlike classes, structs are value types and do not require heap allocation. Struct types do not support user-specified inheritance, and all struct types implicitly inherit from type 
.
An interface type defines a contract as a named set of public function members. A class or struct that implements an interface must provide implementations of the interface's function members. An interface may inherit from multiple base interfaces, and a class or struct may implement multiple interfaces.
A delegate type represents references to methods with a particular parameter list and return type. Delegates make it possible to treat methods as entities that can be assigned to variables and passed as parameters. Delegates are similar to the concept of function pointers found in some other languages, but unlike function pointers, delegates are object-oriented and type-safe.
Class, struct, interface and delegate types all support generics, whereby they can be parameterized with other types.
An enum type is a distinct type with named constants. Every enum type has an underlying type, which must be one of the eight integral types. The set of values of an enum type is the same as the set of values of the underlying type.
C# supports single- and multi-dimensional arrays of any type. Unlike the types listed above, array types do not have to be declared before they can be used. Instead, array types are constructed by following a type name with square brackets. For example, 
 is a single-dimensional array of 
, 
 is a two-dimensional array of 
, and 
 is a single-dimensional array of single-dimensional arrays of 
.
Nullable types also do not have to be declared before they can be used. For each non-nullable value type 
 there is a corresponding nullable type 
, which can hold an additional value 
. For instance, 
 is a type that can hold any 32 bit integer or the value 
.
C#'s type system is unified such that a value of any type can be treated as an object. Every type in C# directly or indirectly derives from the 
 class type, and 
 is the ultimate base class of all types. Values of reference types are treated as objects simply by viewing the values as type 
. Values of value types are treated as objects by performing 
 and 
 operations. In the following example, an 
 value is converted to 
 and back again to 
.
When a value of a value type is converted to type 
, an object instance, also called a ""box,"" is allocated to hold the value, and the value is copied into that box. Conversely, when an 
 reference is cast to a value type, a check is made that the referenced object is a box of the correct value type, and, if the check succeeds, the value in the box is copied out.
C#'s unified type system effectively means that value types can become objects ""on demand."" Because of the unification, general-purpose libraries that use type 
 can be used with both reference types and value types.
There are several kinds of 
 in C#, including fields, array elements, local variables, and parameters. Variables represent storage locations, and every variable has a type that determines what values can be stored in the variable, as shown by the following table.
Type of Variable
Possible Contents
Non-nullable value type
A value of that exact type
Nullable value type
A null value or a value of that exact type
A null reference, a reference to an object of any reference type, or a reference to a boxed value of any value type
Class type
A null reference, a reference to an instance of that class type, or a reference to an instance of a class derived from that class type
Interface type
A null reference, a reference to an instance of a class type that implements that interface type, or a reference to a boxed value of a value type that implements that interface type
Array type
A null reference, a reference to an instance of that array type, or a reference to an instance of a compatible array type
Delegate type
A null reference or a reference to an instance of that delegate type
Expressions
 are constructed from 
 and 
. The operators of an expression indicate which operations to apply to the operands. Examples of operators include 
, 
, 
, 
, and 
. Examples of operands include literals, fields, local variables, and expressions.
When an expression contains multiple operators, the 
 of the operators controls the order in which the individual operators are evaluated. For example, the expression 
 is evaluated as 
 because the 
 operator has higher precedence than the 
 operator.
Most operators can be 
. Operator overloading permits user-defined operator implementations to be specified for operations where one or both of the operands are of a user-defined class or struct type.
The following table summarizes C#'s operators, listing the operator categories in order of precedence from highest to lowest. Operators in the same category have equal precedence.
Category
Expression
Description
Primary
Member access
Method and delegate invocation
Array and indexer access
Post-increment
Post-decrement
Object and delegate creation
Object creation with initializer
Anonymous object initializer
Array creation
Obtain 
 object for 
Evaluate expression in checked context
Evaluate expression in unchecked context
Obtain default value of type 
Anonymous function (anonymous method)
Unary
Identity
Negation
Logical negation
Bitwise negation
Pre-increment
Pre-decrement
Explicitly convert 
 to type 
Asynchronously wait for 
 to complete
Multiplicative
Multiplication
Division
Remainder
Additive
Addition, string concatenation, delegate combination
Subtraction, delegate removal
Shift
Shift left
Shift right
Relational and type testing
Less than
Greater than
Less than or equal
Greater than or equal
Return 
 if 
 is a 
, 
 otherwise
Return 
 typed as 
, or 
 if 
 is not a 
Equality
Equal
Not equal
Logical AND
Integer bitwise AND, boolean logical AND
Logical XOR
Integer bitwise XOR, boolean logical XOR
Logical OR
Integer bitwise OR, boolean logical OR
Conditional AND
Evaluates 
 only if 
 is 
Conditional OR
Evaluates 
 only if 
 is 
Null coalescing
Evaluates to 
 if 
 is 
, to 
 otherwise
Conditional
Evaluates 
 if 
 is 
, 
 if 
 is 
Assignment or anonymous function
Assignment
Compound assignment; supported operators are 
 
 
 
 
 
 
 
 
 
Anonymous function (lambda expression)
Statements
The actions of a program are expressed using 
. C# supports several different kinds of statements, a number of which are defined in terms of embedded statements.
A 
 permits multiple statements to be written in contexts where a single statement is allowed. A block consists of a list of statements written between the delimiters 
 and 
.
 are used to declare local variables and constants.
 are used to evaluate expressions. Expressions that can be used as statements include method invocations, object allocations using the 
 operator, assignments using 
 and the compound assignment operators, increment and decrement operations using the 
 and 
 operators and await expressions.
 are used to select one of a number of possible statements for execution based on the value of some expression. In this group are the 
 and 
 statements.
 are used to repeatedly execute an embedded statement. In this group are the 
, 
, 
, and 
 statements.
 are used to transfer control. In this group are the 
, 
, 
, 
, 
, and 
 statements.
The 
...
 statement is used to catch exceptions that occur during execution of a block, and the 
...
 statement is used to specify finalization code that is always executed, whether an exception occurred or not.
The 
 and 
 statements are used to control the overflow checking context for integral-type arithmetic operations and conversions.
The 
 statement is used to obtain the mutual-exclusion lock for a given object, execute a statement, and then release the lock.
The 
 statement is used to obtain a resource, execute a statement, and then dispose of that resource.
Below are examples of each kind of statement
Local variable declarations
Local constant declaration
Expression statement
 statement
 statement
 statement
 statement
 statement
 statement
 statement
 statement
 statement
 statement
 statement
 and 
 statements
 and 
 statements
 statement
 statement
Classes and objects
 are the most fundamental of C#'s types. A class is a data structure that combines state (fields) and actions (methods and other function members) in a single unit. A class provides a definition for dynamically created 
 of the class, also known as 
. Classes support 
 and 
, mechanisms whereby 
 can extend and specialize 
.
New classes are created using class declarations. A class declaration starts with a header that specifies the attributes and modifiers of the class, the name of the class, the base class (if given), and the interfaces implemented by the class. The header is followed by the class body, which consists of a list of member declarations written between the delimiters 
 and 
.
The following is a declaration of a simple class named 
:
Instances of classes are created using the 
 operator, which allocates memory for a new instance, invokes a constructor to initialize the instance, and returns a reference to the instance. The following statements create two 
 objects and store references to those objects in two variables:
The memory occupied by an object is automatically reclaimed when the object is no longer in use. It is neither necessary nor possible to explicitly deallocate objects in C#.
Members
The members of a class are either 
 or 
. Static members belong to classes, and instance members belong to objects (instances of classes).
The following table provides an overview of the kinds of members a class can contain.
Member
Description
Constants
Constant values associated with the class
Fields
Variables of the class
Methods
Computations and actions that can be performed by the class
Properties
Actions associated with reading and writing named properties of the class
Indexers
Actions associated with indexing instances of the class like an array
Events
Notifications that can be generated by the class
Operators
Conversions and expression operators supported by the class
Constructors
Actions required to initialize instances of the class or the class itself
Destructors
Actions to perform before instances of the class are permanently discarded
Types
Nested types declared by the class
Accessibility
Each member of a class has an associated accessibility, which controls the regions of program text that are able to access the member. There are five possible forms of accessibility. These are summarized in the following table.
Accessibility
Meaning
Access not limited
Access limited to this class or classes derived from this class
Access limited to this program
Access limited to this program or classes derived from this class
Access limited to this class
Type parameters
A class definition may specify a set of type parameters by following the class name with angle brackets enclosing a list of type parameter names. The type parameters can the be used in the body of the class declarations to define the members of the class. In the following example, the type parameters of 
 are 
 and 
:
A class type that is declared to take type parameters is called a generic class type. Struct, interface and delegate types can also be generic.
When the generic class is used, type arguments must be provided for each of the type parameters:
A generic type with type arguments provided, like 
 above, is called a constructed type.
Base classes
A class declaration may specify a base class by following the class name and type parameters with a colon and the name of the base class. Omitting a base class specification is the same as deriving from type 
. In the following example, the base class of 
 is 
, and the base class of 
 is 
:
A class inherits the members of its base class. Inheritance means that a class implicitly contains all members of its base class, except for the instance and static constructors, and the destructors of the base class. A derived class can add new members to those it inherits, but it cannot remove the definition of an inherited member. In the previous example, 
 inherits the 
 and 
 fields from 
, and every 
 instance contains three fields, 
, 
, and 
.
An implicit conversion exists from a class type to any of its base class types. Therefore, a variable of a class type can reference an instance of that class or an instance of any derived class. For example, given the previous class declarations, a variable of type 
 can reference either a 
 or a 
:
Fields
A field is a variable that is associated with a class or with an instance of a class.
A field declared with the 
 modifier defines a 
. A static field identifies exactly one storage location. No matter how many instances of a class are created, there is only ever one copy of a static field.
A field declared without the 
 modifier defines an 
. Every instance of a class contains a separate copy of all the instance fields of that class.
In the following example, each instance of the 
 class has a separate copy of the 
, 
, and 
 instance fields, but there is only one copy of the 
, 
, 
, 
, and 
 static fields:
As shown in the previous example, 
 may be declared with a 
 modifier. Assignment to a 
 field can only occur as part of the field's declaration or in a constructor in the same class.
Methods
A 
 is a member that implements a computation or action that can be performed by an object or class. 
 are accessed through the class. 
 are accessed through instances of the class.
Methods have a (possibly empty) list of 
, which represent values or variable references passed to the method, and a 
, which specifies the type of the value computed and returned by the method. A method's return type is 
 if it does not return a value.
Like types, methods may also have a set of type parameters, for which type arguments must be specified when the method is called. Unlike types, the type arguments can often be inferred from the arguments of a method call and need not be explicitly given.
The 
 of a method must be unique in the class in which the method is declared. The signature of a method consists of the name of the method, the number of type parameters and the number, modifiers, and types of its parameters. The signature of a method does not include the return type.
Parameters
Parameters are used to pass values or variable references to methods. The parameters of a method get their actual values from the 
 that are specified when the method is invoked. There are four kinds of parameters: value parameters, reference parameters, output parameters, and parameter arrays.
A 
 is used for input parameter passing. A value parameter corresponds to a local variable that gets its initial value from the argument that was passed for the parameter. Modifications to a value parameter do not affect the argument that was passed for the parameter.
Value parameters can be optional, by specifying a default value so that corresponding arguments can be omitted.
A 
 is used for both input and output parameter passing. The argument passed for a reference parameter must be a variable, and during execution of the method, the reference parameter represents the same storage location as the argument variable. A reference parameter is declared with the 
 modifier. The following example shows the use of 
 parameters.
An 
 is used for output parameter passing. An output parameter is similar to a reference parameter except that the initial value of the caller-provided argument is unimportant. An output parameter is declared with the 
 modifier. The following example shows the use of 
 parameters.
A 
 permits a variable number of arguments to be passed to a method. A parameter array is declared with the 
 modifier. Only the last parameter of a method can be a parameter array, and the type of a parameter array must be a single-dimensional array type. The 
 and 
 methods of the 
 class are good examples of parameter array usage. They are declared as follows.
Within a method that uses a parameter array, the parameter array behaves exactly like a regular parameter of an array type. However, in an invocation of a method with a parameter array, it is possible to pass either a single argument of the parameter array type or any number of arguments of the element type of the parameter array. In the latter case, an array instance is automatically created and initialized with the given arguments. This example
is equivalent to writing the following.
Method body and local variables
A method's body specifies the statements to execute when the method is invoked.
A method body can declare variables that are specific to the invocation of the method. Such variables are called 
. A local variable declaration specifies a type name, a variable name, and possibly an initial value. The following example declares a local variable 
 with an initial value of zero and a local variable 
 with no initial value.
C# requires a local variable to be 
 before its value can be obtained. For example, if the declaration of the previous 
 did not include an initial value, the compiler would report an error for the subsequent usages of 
 because 
 would not be definitely assigned at those points in the program.
A method can use 
 statements to return control to its caller. In a method returning 
, 
 statements cannot specify an expression. In a method returning non-
, 
 statements must include an expression that computes the return value.
Static and instance methods
A method declared with a 
 modifier is a 
. A static method does not operate on a specific instance and can only directly access static members.
A method declared without a 
 modifier is an 
. An instance method operates on a specific instance and can access both static and instance members. The instance on which an instance method was invoked can be explicitly accessed as 
. It is an error to refer to 
 in a static method.
The following 
 class has both static and instance members.
Each 
 instance contains a serial number (and presumably some other information that is not shown here). The 
 constructor (which is like an instance method) initializes the new instance with the next available serial number. Because the constructor is an instance member, it is permitted to access both the 
 instance field and the 
 static field.
The 
 and 
 static methods can access the 
 static field, but it would be an error for them to directly access the 
 instance field.
The following example shows the use of the 
 class.
Note that the 
 and 
 static methods are invoked on the class whereas the 
 instance method is invoked on instances of the class.
Virtual, override, and abstract methods
When an instance method declaration includes a 
 modifier, the method is said to be a 
. When no 
 modifier is present, the method is said to be a 
.
When a virtual method is invoked, the 
 of the instance for which that invocation takes place determines the actual method implementation to invoke. In a nonvirtual method invocation, the 
 of the instance is the determining factor.
A virtual method can be 
 in a derived class. When an instance method declaration includes an 
 modifier, the method overrides an inherited virtual method with the same signature. Whereas a virtual method declaration introduces a new method, an override method declaration specializes an existing inherited virtual method by providing a new implementation of that method.
An 
 method is a virtual method with no implementation. An abstract method is declared with the 
 modifier and is permitted only in a class that is also declared 
. An abstract method must be overridden in every non-abstract derived class.
The following example declares an abstract class, 
, which represents an expression tree node, and three derived classes, 
, 
, and 
, which implement expression tree nodes for constants, variable references, and arithmetic operations. (This is similar to, but not to be confused with the expression tree types introduced in 
).
The previous four classes can be used to model arithmetic expressions. For example, using instances of these classes, the expression 
 can be represented as follows.
The 
 method of an 
 instance is invoked to evaluate the given expression and produce a 
 value. The method takes as an argument a 
 that contains variable names (as keys of the entries) and values (as values of the entries). The 
 method is a virtual abstract method, meaning that non-abstract derived classes must override it to provide an actual implementation.
A 
's implementation of 
 simply returns the stored constant. A 
's implementation looks up the variable name in the hashtable and returns the resulting value. An 
's implementation first evaluates the left and right operands (by recursively invoking their 
 methods) and then performs the given arithmetic operation.
The following program uses the 
 classes to evaluate the expression 
 for different values of 
 and 
.
Method overloading
Method 
 permits multiple methods in the same class to have the same name as long as they have unique signatures. When compiling an invocation of an overloaded method, the compiler uses 
 to determine the specific method to invoke. Overload resolution finds the one method that best matches the arguments or reports an error if no single best match can be found. The following example shows overload resolution in effect. The comment for each invocation in the 
 method shows which method is actually invoked.
As shown by the example, a particular method can always be selected by explicitly casting the arguments to the exact parameter types and/or explicitly supplying type arguments.
Other function members
Members that contain executable code are collectively known as the 
 of a class. The preceding section describes methods, which are the primary kind of function members. This section describes the other kinds of function members supported by C#: constructors, properties, indexers, events, operators, and destructors.
The following code shows a generic class called 
, which implements a growable list of objects. The class contains several examples of the most common kinds of function members.
Constructors
C# supports both instance and static constructors. An 
 is a member that implements the actions required to initialize an instance of a class. A 
 is a member that implements the actions required to initialize a class itself when it is first loaded.
A constructor is declared like a method with no return type and the same name as the containing class. If a constructor declaration includes a 
 modifier, it declares a static constructor. Otherwise, it declares an instance constructor.
Instance constructors can be overloaded. For example, the 
 class declares two instance constructors, one with no parameters and one that takes an 
 parameter. Instance constructors are invoked using the 
 operator. The following statements allocate two 
 instances using each of the constructors of the 
 class.
Unlike other members, instance constructors are not inherited, and a class has no instance constructors other than those actually declared in the class. If no instance constructor is supplied for a class, then an empty one with no parameters is automatically provided.
Properties
 are a natural extension of fields. Both are named members with associated types, and the syntax for accessing fields and properties is the same. However, unlike fields, properties do not denote storage locations. Instead, properties have 
 that specify the statements to be executed when their values are read or written.
A property is declared like a field, except that the declaration ends with a 
 accessor and/or a 
 accessor written between the delimiters 
 and 
 instead of ending in a semicolon. A property that has both a 
 accessor and a 
 accessor is a 
, a property that has only a 
 accessor is a 
, and a property that has only a 
 accessor is a 
.
A 
 accessor corresponds to a parameterless method with a return value of the property type. Except as the target of an assignment, when a property is referenced in an expression, the 
 accessor of the property is invoked to compute the value of the property.
A 
 accessor corresponds to a method with a single parameter named 
 and no return type. When a property is referenced as the target of an assignment or as the operand of 
 or 
, the 
 accessor is invoked with an argument that provides the new value.
The 
 class declares two properties, 
 and 
, which are read-only and read-write, respectively. The following is an example of use of these properties.
Similar to fields and methods, C# supports both instance properties and static properties. Static properties are declared with the 
 modifier, and instance properties are declared without it.
The accessor(s) of a property can be virtual. When a property declaration includes a 
, 
, or 
 modifier, it applies to the accessor(s) of the property.
Indexers
An 
 is a member that enables objects to be indexed in the same way as an array. An indexer is declared like a property except that the name of the member is 
 followed by a parameter list written between the delimiters 
 and 
. The parameters are available in the accessor(s) of the indexer. Similar to properties, indexers can be read-write, read-only, and write-only, and the accessor(s) of an indexer can be virtual.
The 
 class declares a single read-write indexer that takes an 
 parameter. The indexer makes it possible to index 
 instances with 
 values. For example
Indexers can be overloaded, meaning that a class can declare multiple indexers as long as the number or types of their parameters differ.
Events
An 
 is a member that enables a class or object to provide notifications. An event is declared like a field except that the declaration includes an 
 keyword and the type must be a delegate type.
Within a class that declares an event member, the event behaves just like a field of a delegate type (provided the event is not abstract and does not declare accessors). The field stores a reference to a delegate that represents the event handlers that have been added to the event. If no event handles are present, the field is 
.
The 
 class declares a single event member called 
, which indicates that a new item has been added to the list. The 
 event is raised by the 
 virtual method, which first checks whether the event is 
 (meaning that no handlers are present). The notion of raising an event is precisely equivalent to invoking the delegate represented by the event—thus, there are no special language constructs for raising events.
Clients react to events through 
. Event handlers are attached using the 
 operator and removed using the 
 operator. The following example attaches an event handler to the 
 event of a 
.
For advanced scenarios where control of the underlying storage of an event is desired, an event declaration can explicitly provide 
 and 
 accessors, which are somewhat similar to the 
 accessor of a property.
Operators
An 
 is a member that defines the meaning of applying a particular expression operator to instances of a class. Three kinds of operators can be defined: unary operators, binary operators, and conversion operators. All operators must be declared as 
 and 
.
The 
 class declares two operators, 
 and 
, and thus gives new meaning to expressions that apply those operators to 
 instances. Specifically, the operators define equality of two 
 instances as comparing each of the contained objects using their 
 methods. The following example uses the 
 operator to compare two 
 instances.
The first 
 outputs 
 because the two lists contain the same number of objects with the same values in the same order. Had 
 not defined 
, the first 
 would have output 
 because 
 and 
 reference different 
 instances.
Destructors
A 
 is a member that implements the actions required to destruct an instance of a class. Destructors cannot have parameters, they cannot have accessibility modifiers, and they cannot be invoked explicitly. The destructor for an instance is invoked automatically during garbage collection.
The garbage collector is allowed wide latitude in deciding when to collect objects and run destructors. Specifically, the timing of destructor invocations is not deterministic, and destructors may be executed on any thread. For these and other reasons, classes should implement destructors only when no other solutions are feasible.
The 
 statement provides a better approach to object destruction.
Structs
Like classes, 
 are data structures that can contain data members and function members, but unlike classes, structs are value types and do not require heap allocation. A variable of a struct type directly stores the data of the struct, whereas a variable of a class type stores a reference to a dynamically allocated object. Struct types do not support user-specified inheritance, and all struct types implicitly inherit from type 
.
Structs are particularly useful for small data structures that have value semantics. Complex numbers, points in a coordinate system, or key-value pairs in a dictionary are all good examples of structs. The use of structs rather than classes for small data structures can make a large difference in the number of memory allocations an application performs. For example, the following program creates and initializes an array of 100 points. With 
 implemented as a class, 101 separate objects are instantiated—one for the array and one each for the 100 elements.
An alternative is to make 
 a struct.
Now, only one object is instantiated—the one for the array—and the 
 instances are stored in-line in the array.
Struct constructors are invoked with the 
 operator, but that does not imply that memory is being allocated. Instead of dynamically allocating an object and returning a reference to it, a struct constructor simply returns the struct value itself (typically in a temporary location on the stack), and this value is then copied as necessary.
With classes, it is possible for two variables to reference the same object and thus possible for operations on one variable to affect the object referenced by the other variable. With structs, the variables each have their own copy of the data, and it is not possible for operations on one to affect the other. For example, the output produced by the following code fragment depends on whether 
 is a class or a struct.
If 
 is a class, the output is 
 because 
 and 
 reference the same object. If 
 is a struct, the output is 
 because the assignment of 
 to 
 creates a copy of the value, and this copy is unaffected by the subsequent assignment to 
.
The previous example highlights two of the limitations of structs. First, copying an entire struct is typically less efficient than copying an object reference, so assignment and value parameter passing can be more expensive with structs than with reference types. Second, except for 
 and 
 parameters, it is not possible to create references to structs, which rules out their usage in a number of situations.
Arrays
An 
 is a data structure that contains a number of variables that are accessed through computed indices. The variables contained in an array, also called the 
 of the array, are all of the same type, and this type is called the 
 of the array.
Array types are reference types, and the declaration of an array variable simply sets aside space for a reference to an array instance. Actual array instances are created dynamically at run-time using the 
 operator. The 
 operation specifies the 
 of the new array instance, which is then fixed for the lifetime of the instance. The indices of the elements of an array range from 
 to 
. The 
 operator automatically initializes the elements of an array to their default value, which, for example, is zero for all numeric types and 
 for all reference types.
The following example creates an array of 
 elements, initializes the array, and prints out the contents of the array.
This example creates and operates on a 
. C# also supports 
. The number of dimensions of an array type, also known as the 
 of the array type, is one plus the number of commas written between the square brackets of the array type. The following example allocates a one-dimensional, a two-dimensional, and a three-dimensional array.
The 
 array contains 10 elements, the 
 array contains 50 (10 × 5) elements, and the 
 array contains 100 (10 × 5 × 2) elements.
The element type of an array can be any type, including an array type. An array with elements of an array type is sometimes called a 
 because the lengths of the element arrays do not all have to be the same. The following example allocates an array of arrays of 
:
The first line creates an array with three elements, each of type 
 and each with an initial value of 
. The subsequent lines then initialize the three elements with references to individual array instances of varying lengths.
The 
 operator permits the initial values of the array elements to be specified using an 
, which is a list of expressions written between the delimiters 
 and 
. The following example allocates and initializes an 
 with three elements.
Note that the length of the array is inferred from the number of expressions between 
 and 
. Local variable and field declarations can be shortened further such that the array type does not have to be restated.
Both of the previous examples are equivalent to the following:
Interfaces
An 
 defines a contract that can be implemented by classes and structs. An interface can contain methods, properties, events, and indexers. An interface does not provide implementations of the members it defines—it merely specifies the members that must be supplied by classes or structs that implement the interface.
Interfaces may employ 
. In the following example, the interface 
 inherits from both 
 and 
.
Classes and structs can implement multiple interfaces. In the following example, the class 
 implements both 
 and 
.
When a class or struct implements a particular interface, instances of that class or struct can be implicitly converted to that interface type. For example
In cases where an instance is not statically known to implement a particular interface, dynamic type casts can be used. For example, the following statements use dynamic type casts to obtain an object's 
 and 
 interface implementations. Because the actual type of the object is 
, the casts succeed.
In the previous 
 class, the 
 method from the 
 interface and the 
 method from the 
 interface are implemented using 
 members. C# also supports 
, using which the class or struct can avoid making the members 
. An explicit interface member implementation is written using the fully qualified interface member name. For example, the 
 class could implement the 
 and 
 methods using explicit interface member implementations as follows.
Explicit interface members can only be accessed via the interface type. For example, the implementation of 
 provided by the previous 
 class can only be invoked by first converting the 
 reference to the 
 interface type.
Enums
An 
 is a distinct value type with a set of named constants. The following example declares and uses an enum type named 
 with three constant values, 
, 
, and 
.
Each enum type has a corresponding integral type called the 
 of the enum type. An enum type that does not explicitly declare an underlying type has an underlying type of 
. An enum type's storage format and range of possible values are determined by its underlying type. The set of values that an enum type can take on is not limited by its enum members. In particular, any value of the underlying type of an enum can be cast to the enum type and is a distinct valid value of that enum type.
The following example declares an enum type named 
 with an underlying type of 
.
As shown by the previous example, an enum member declaration can include a constant expression that specifies the value of the member. The constant value for each enum member must be in the range of the underlying type of the enum. When an enum member declaration does not explicitly specify a value, the member is given the value zero (if it is the first member in the enum type) or the value of the textually preceding enum member plus one.
Enum values can be converted to integral values and vice versa using type casts. For example
The default value of any enum type is the integral value zero converted to the enum type. In cases where variables are automatically initialized to a default value, this is the value given to variables of enum types. In order for the default value of an enum type to be easily available, the literal 
 implicitly converts to any enum type. Thus, the following is permitted.
Delegates
A 
 represents references to methods with a particular parameter list and return type. Delegates make it possible to treat methods as entities that can be assigned to variables and passed as parameters. Delegates are similar to the concept of function pointers found in some other languages, but unlike function pointers, delegates are object-oriented and type-safe.
The following example declares and uses a delegate type named 
.
An instance of the 
 delegate type can reference any method that takes a 
 argument and returns a 
 value. The 
 method applies a given 
 to the elements of a 
, returning a 
 with the results. In the 
 method, 
 is used to apply three different functions to a 
.
A delegate can reference either a static method (such as 
 or 
 in the previous example) or an instance method (such as 
 in the previous example). A delegate that references an instance method also references a particular object, and when the instance method is invoked through the delegate, that object becomes 
 in the invocation.
Delegates can also be created using anonymous functions, which are ""inline methods"" that are created on the fly. Anonymous functions can see the local variables of the sourrounding methods. Thus, the multiplier example above can be written more easily without using a 
 class:
An interesting and useful property of a delegate is that it does not know or care about the class of the method it references; all that matters is that the referenced method has the same parameters and return type as the delegate.
Attributes
Types, members, and other entities in a C# program support modifiers that control certain aspects of their behavior. For example, the accessibility of a method is controlled using the 
, 
, 
, and 
 modifiers. C# generalizes this capability such that user-defined types of declarative information can be attached to program entities and retrieved at run-time. Programs specify this additional declarative information by defining and using 
.
The following example declares a 
 attribute that can be placed on program entities to provide links to their associated documentation.
All attribute classes derive from the 
 base class provided by the .NET Framework. Attributes can be applied by giving their name, along with any arguments, inside square brackets just before the associated declaration. If an attribute's name ends in 
, that part of the name can be omitted when the attribute is referenced. For example, the 
 attribute can be used as follows.
This example attaches a 
 to the 
 class and another 
 to the 
 method in the class. The public constructors of an attribute class control the information that must be provided when the attribute is attached to a program entity. Additional information can be provided by referencing public read-write properties of the attribute class (such as the reference to the 
 property previously).
The following example shows how attribute information for a given program entity can be retrieved at run-time using reflection.
When a particular attribute is requested through reflection, the constructor for the attribute class is invoked with the information provided in the program source, and the resulting attribute instance is returned. If additional information was provided through properties, those properties are set to the given values before the attribute instance is returned.
Lexical structure
Programs
A C# 
 consists of one or more 
, known formally as 
 (
). A source file is an ordered sequence of Unicode characters. Source files typically have a one-to-one correspondence with files in a file system, but this correspondence is not required. For maximal portability, it is recommended that files in a file system be encoded with the UTF-8 encoding.
Conceptually speaking, a program is compiled using three steps:
Transformation, which converts a file from a particular character repertoire and encoding scheme into a sequence of Unicode characters.
Lexical analysis, which translates a stream of Unicode input characters into a stream of tokens.
Syntactic analysis, which translates the stream of tokens into executable code.
Grammars
This specification presents the syntax of the C# programming language using two grammars. The 
 (
) defines how Unicode characters are combined to form line terminators, white space, comments, tokens, and pre-processing directives. The 
 (
) defines how the tokens resulting from the lexical grammar are combined to form C# programs.
Grammar notation
The lexical and syntactic grammars are presented in Backus-Naur form using the notation of the ANTLR grammar tool.
Lexical grammar
The lexical grammar of C# is presented in 
, 
, and 
. The terminal symbols of the lexical grammar are the characters of the Unicode character set, and the lexical grammar specifies how characters are combined to form tokens (
), white space (
), comments (
), and pre-processing directives (
).
Every source file in a C# program must conform to the 
 production of the lexical grammar (
).
Syntactic grammar
The syntactic grammar of C# is presented in the chapters and appendices that follow this chapter. The terminal symbols of the syntactic grammar are the tokens defined by the lexical grammar, and the syntactic grammar specifies how tokens are combined to form C# programs.
Every source file in a C# program must conform to the 
 production of the syntactic grammar (
).
Lexical analysis
The 
 production defines the lexical structure of a C# source file. Each source file in a C# program must conform to this lexical grammar production.
Five basic elements make up the lexical structure of a C# source file: Line terminators (
), white space (
), comments (
), tokens (
), and pre-processing directives (
). Of these basic elements, only tokens are significant in the syntactic grammar of a C# program (
).
The lexical processing of a C# source file consists of reducing the file into a sequence of tokens which becomes the input to the syntactic analysis. Line terminators, white space, and comments can serve to separate tokens, and pre-processing directives can cause sections of the source file to be skipped, but otherwise these lexical elements have no impact on the syntactic structure of a C# program.
In the case of interpolated string literals (
) a single token is initially produced by lexical analysis, but is broken up into several input elements which are repeatedly subjected to lexical analysis until all interpolated string literals have been resolved. The resulting tokens then serve as input to the syntactic analysis.
When several lexical grammar productions match a sequence of characters in a source file, the lexical processing always forms the longest possible lexical element. For example, the character sequence 
 is processed as the beginning of a single-line comment because that lexical element is longer than a single 
 token.
Line terminators
Line terminators divide the characters of a C# source file into lines.
For compatibility with source code editing tools that add end-of-file markers, and to enable a source file to be viewed as a sequence of properly terminated lines, the following transformations are applied, in order, to every source file in a C# program:
If the last character of the source file is a Control-Z character (
), this character is deleted.
A carriage-return character (
) is added to the end of the source file if that source file is non-empty and if the last character of the source file is not a carriage return (
), a line feed (
), a line separator (
), or a paragraph separator (
).
Comments
Two forms of comments are supported: single-line comments and delimited comments. 
 start with the characters 
 and extend to the end of the source line. 
 start with the characters 
 and end with the characters 
. Delimited comments may span multiple lines.
Comments do not nest. The character sequences 
 and 
 have no special meaning within a 
 comment, and the character sequences 
 and 
 have no special meaning within a delimited comment.
Comments are not processed within character and string literals.
The example
includes a delimited comment.
The example
shows several single-line comments.
White space
White space is defined as any character with Unicode class Zs (which includes the space character) as well as the horizontal tab character, the vertical tab character, and the form feed character.
Tokens
There are several kinds of tokens: identifiers, keywords, literals, operators, and punctuators. White space and comments are not tokens, though they act as separators for tokens.
Unicode character escape sequences
A Unicode character escape sequence represents a Unicode character. Unicode character escape sequences are processed in identifiers (
), character literals (
), and regular string literals (
). A Unicode character escape is not processed in any other location (for example, to form an operator, punctuator, or keyword).
A Unicode escape sequence represents the single Unicode character formed by the hexadecimal number following the ""
"" or ""
"" characters. Since C# uses a 16-bit encoding of Unicode code points in characters and string values, a Unicode character in the range U+10000 to U+10FFFF is not permitted in a character literal and is represented using a Unicode surrogate pair in a string literal. Unicode characters with code points above 0x10FFFF are not supported.
Multiple translations are not performed. For instance, the string literal ""
"" is equivalent to ""
"" rather than ""
"". The Unicode value 
 is the character ""
"".
The example
shows several uses of 
, which is the escape sequence for the letter ""
"". The program is equivalent to
Identifiers
The rules for identifiers given in this section correspond exactly to those recommended by the Unicode Standard Annex 31, except that underscore is allowed as an initial character (as is traditional in the C programming language), Unicode escape sequences are permitted in identifiers, and the ""
"" character is allowed as a prefix to enable keywords to be used as identifiers.
For information on the Unicode character classes mentioned above, see The Unicode Standard, Version 3.0, section 4.5.
Examples of valid identifiers include ""
"", ""
"", and ""
"".
An identifier in a conforming program must be in the canonical format defined by Unicode Normalization Form C, as defined by Unicode Standard Annex 15. The behavior when encountering an identifier not in Normalization Form C is implementation-defined; however, a diagnostic is not required.
The prefix ""
"" enables the use of keywords as identifiers, which is useful when interfacing with other programming languages. The character 
 is not actually part of the identifier, so the identifier might be seen in other languages as a normal identifier, without the prefix. An identifier with an 
 prefix is called a 
. Use of the 
 prefix for identifiers that are not keywords is permitted, but strongly discouraged as a matter of style.
The example:
defines a class named ""
"" with a static method named ""
"" that takes a parameter named ""
"". Note that since Unicode escapes are not permitted in keywords, the token ""
"" is an identifier, and is the same identifier as ""
"".
Two identifiers are considered the same if they are identical after the following transformations are applied, in order:
The prefix ""
"", if used, is removed.
Each 
 is transformed into its corresponding Unicode character.
Any 
s are removed.
Identifiers containing two consecutive underscore characters (
) are reserved for use by the implementation. For example, an implementation might provide extended keywords that begin with two underscores.
Keywords
A 
 is an identifier-like sequence of characters that is reserved, and cannot be used as an identifier except when prefaced by the 
 character.
In some places in the grammar, specific identifiers have special meaning, but are not keywords. Such identifiers are sometimes referred to as ""contextual keywords"". For example, within a property declaration, the ""
"" and ""
"" identifiers have special meaning (
). An identifier other than 
 or 
 is never permitted in these locations, so this use does not conflict with a use of these words as identifiers. In other cases, such as with the identifier ""
"" in implicitly typed local variable declarations (
), a contectual keyword can conflict with declared names. In such cases, the declared name takes precedence over the use of the identifier as a contextual keyword.
Literals
A 
 is a source code representation of a value.
Boolean literals
There are two boolean literal values: 
 and 
.
The type of a 
 is 
.
Integer literals
Integer literals are used to write values of types 
, 
, 
, and 
. Integer literals have two possible forms: decimal and hexadecimal.
The type of an integer literal is determined as follows:
If the literal has no suffix, it has the first of these types in which its value can be represented: 
, 
, 
, 
.
If the literal is suffixed by 
 or 
, it has the first of these types in which its value can be represented: 
, 
.
If the literal is suffixed by 
 or 
, it has the first of these types in which its value can be represented: 
, 
.
If the literal is suffixed by 
, 
, 
, 
, 
, 
, 
, or 
, it is of type 
.
If the value represented by an integer literal is outside the range of the 
 type, a compile-time error occurs.
As a matter of style, it is suggested that ""
"" be used instead of ""
"" when writing literals of type 
, since it is easy to confuse the letter ""
"" with the digit ""
"".
To permit the smallest possible 
 and 
 values to be written as decimal integer literals, the following two rules exist:
When a 
 with the value 2147483648 (2^31) and no 
 appears as the token immediately following a unary minus operator token (
), the result is a constant of type 
 with the value -2147483648 (-2^31). In all other situations, such a 
 is of type 
.
When a 
 with the value 9223372036854775808 (2^63) and no 
 or the 
 
 or 
 appears as the token immediately following a unary minus operator token (
), the result is a constant of type 
 with the value -9223372036854775808 (-2^63). In all other situations, such a 
 is of type 
.
Real literals
Real literals are used to write values of types 
, 
, and 
.
If no 
 is specified, the type of the real literal is 
. Otherwise, the real type suffix determines the type of the real literal, as follows:
A real literal suffixed by 
 or 
 is of type 
. For example, the literals 
, 
, 
, and 
 are all of type 
.
A real literal suffixed by 
 or 
 is of type 
. For example, the literals 
, 
, 
, and 
 are all of type 
.
A real literal suffixed by 
 or 
 is of type 
. For example, the literals 
, 
, 
, and 
 are all of type 
. This literal is converted to a 
 value by taking the exact value, and, if necessary, rounding to the nearest representable value using banker's rounding (
). Any scale apparent in the literal is preserved unless the value is rounded or the value is zero (in which latter case the sign and scale will be 0). Hence, the literal 
 will be parsed to form the decimal with sign 
, coefficient 
, and scale 
.
If the specified literal cannot be represented in the indicated type, a compile-time error occurs.
The value of a real literal of type 
 or 
 is determined by using the IEEE ""round to nearest"" mode.
Note that in a real literal, decimal digits are always required after the decimal point. For example, 
 is a real literal but 
 is not.
Character literals
A character literal represents a single character, and usually consists of a character in quotes, as in 
.
Note: The ANTLR grammar notation makes the following confusing! In ANTLR, when you write 
 it stands for a single quote 
. And when you write 
 it stands for a single backslash 
. Therefore the first rule for a character literal means it starts with a single quote, then a character, then a single quote. And the eleven possible simple escape sequences are 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
.
A character that follows a backslash character (
) in a 
 must be one of the following characters: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
. Otherwise, a compile-time error occurs.
A hexadecimal escape sequence represents a single Unicode character, with the value formed by the hexadecimal number following ""
"".
If the value represented by a character literal is greater than 
, a compile-time error occurs.
A Unicode character escape sequence (
) in a character literal must be in the range 
 to 
.
A simple escape sequence represents a Unicode character encoding, as described in the table below.
Escape sequence
Character name
Unicode encoding
Single quote
Double quote
Backslash
Null
Alert
Backspace
Form feed
New line
Carriage return
Horizontal tab
Vertical tab
The type of a 
 is 
.
String literals
C# supports two forms of string literals: 
 and 
.
A regular string literal consists of zero or more characters enclosed in double quotes, as in 
, and may include both simple escape sequences (such as 
 for the tab character), and hexadecimal and Unicode escape sequences.
A verbatim string literal consists of an 
 character followed by a double-quote character, zero or more characters, and a closing double-quote character. A simple example is 
. In a verbatim string literal, the characters between the delimiters are interpreted verbatim, the only exception being a 
. In particular, simple escape sequences, and hexadecimal and Unicode escape sequences are not processed in verbatim string literals. A verbatim string literal may span multiple lines.
A character that follows a backslash character (
) in a 
 must be one of the following characters: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
. Otherwise, a compile-time error occurs.
The example
shows a variety of string literals. The last string literal, 
, is a verbatim string literal that spans multiple lines. The characters between the quotation marks, including white space such as new line characters, are preserved verbatim.
Since a hexadecimal escape sequence can have a variable number of hex digits, the string literal 
 contains a single character with hex value 123. To create a string containing the character with hex value 12 followed by the character 3, one could write 
 or 
 instead.
The type of a 
 is 
.
Each string literal does not necessarily result in a new string instance. When two or more string literals that are equivalent according to the string equality operator (
) appear in the same program, these string literals refer to the same string instance. For instance, the output produced by
is 
 because the two literals refer to the same string instance.
Interpolated string literals
Interpolated string literals are similar to string literals, but contain holes delimited by 
 and 
, wherein expressions can occur. At runtime, the expressions are evaluated with the purpose of having their textual forms substituted into the string at the place where the hole occurs. The syntax and semantics of string interpolation are described in section (
).
Like string literals, interpolated string literals can be either regular or verbatim. Interpolated regular string literals are delimited by 
 and 
, and interpolated verbatim string literals are delimited by 
 and 
.
Like other literals, lexical analysis of an interpolated string literal initially results in a single token, as per the grammar below. However, before syntactic analysis, the single token of an interpolated string literal is broken into several tokens for the parts of the string enclosing the holes, and the input elements occurring in the holes are lexically analysed again. This may in turn produce more interpolated string literals to be processed, but, if lexically correct, will eventually lead to a sequence of tokens for syntactic analysis to process.
An 
 token is reinterpreted as multiple tokens and other input elements as follows, in order of occurrence in the 
:
Occurences of the following are reinterpreted as separate individual tokens: the leading 
 sign, 
, 
, 
, 
, 
, 
, 
 and 
.
Occurences of 
 and 
 between these are reprocessed as an 
 (
) and are reinterpreted as the resulting sequence of input elements. These may in turn include interpolated string literal tokens to be reinterpreted.
Syntactic analysis will recombine the tokens into an 
 (
).
Examples TODO
The null literal
The  
 can be implicitly converted to a reference type or nullable type.
Operators and punctuators
There are several kinds of operators and punctuators. Operators are used in expressions to describe operations involving one or more operands. For example, the expression 
 uses the 
 operator to add the two operands 
 and 
. Punctuators are for grouping and separating.
The vertical bar in the 
 and 
 productions are used to indicate that, unlike other productions in the syntactic grammar, no characters of any kind (not even whitespace) are allowed between the tokens. These productions are treated specially in order to enable the correct  handling of 
s (
).
Pre-processing directives
The pre-processing directives provide the ability to conditionally skip sections of source files, to report error and warning conditions, and to delineate distinct regions of source code. The term ""pre-processing directives"" is used only for consistency with the C and C++ programming languages. In C#, there is no separate pre-processing step; pre-processing directives are processed as part of the lexical analysis phase.
The following pre-processing directives are available:
 and 
, which are used to define and undefine, respectively, conditional compilation symbols (
).
, 
, 
, and 
, which are used to conditionally skip sections of source code (
).
, which is used to control line numbers emitted for errors and warnings (
).
 and 
, which are used to issue errors and warnings, respectively (
).
 and 
, which are used to explicitly mark sections of source code (
).
, which is used to specify optional contextual information to the compiler (
).
A pre-processing directive always occupies a separate line of source code and always begins with a 
 character and a pre-processing directive name. White space may occur before the 
 character and between the 
 character and the directive name.
A source line containing a 
, 
, 
, 
, 
, 
, 
, or 
 directive may end with a single-line comment. Delimited comments (the 
 style of comments) are not permitted on source lines containing pre-processing directives.
Pre-processing directives are not tokens and are not part of the syntactic grammar of C#. However, pre-processing directives can be used to include or exclude sequences of tokens and can in that way affect the meaning of a C# program. For example, when compiled, the program:
results in the exact same sequence of tokens as the program:
Thus, whereas lexically, the two programs are quite different, syntactically, they are identical.
Conditional compilation symbols
The conditional compilation functionality provided by the 
, 
, 
, and 
 directives is controlled through pre-processing expressions (
) and conditional compilation symbols.
A conditional compilation symbol has two possible states: 
 or 
. At the beginning of the lexical processing of a source file, a conditional compilation symbol is undefined unless it has been explicitly defined by an external mechanism (such as a command-line compiler option). When a 
 directive is processed, the conditional compilation symbol named in that directive becomes defined in that source file. The symbol remains defined until an 
 directive for that same symbol is processed, or until the end of the source file is reached. An implication of this is that 
 and 
 directives in one source file have no effect on other source files in the same program.
When referenced in a pre-processing expression, a defined conditional compilation symbol has the boolean value 
, and an undefined conditional compilation symbol has the boolean value 
. There is no requirement that conditional compilation symbols be explicitly declared before they are referenced in pre-processing expressions. Instead, undeclared symbols are simply undefined and thus have the value 
.
The name space for conditional compilation symbols is distinct and separate from all other named entities in a C# program. Conditional compilation symbols can only be referenced in 
 and 
 directives and in pre-processing expressions.
Pre-processing expressions
Pre-processing expressions can occur in 
 and 
 directives. The operators 
, 
, 
, 
 and 
 are permitted in pre-processing expressions, and parentheses may be used for grouping.
When referenced in a pre-processing expression, a defined conditional compilation symbol has the boolean value 
, and an undefined conditional compilation symbol has the boolean value 
.
Evaluation of a pre-processing expression always yields a boolean value. The rules of evaluation for a pre-processing expression are the same as those for a constant expression (
), except that the only user-defined entities that can be referenced are conditional compilation symbols.
Declaration directives
The declaration directives are used to define or undefine conditional compilation symbols.
The processing of a 
 directive causes the given conditional compilation symbol to become defined, starting with the source line that follows the directive. Likewise, the processing of an 
 directive causes the given conditional compilation symbol to become undefined, starting with the source line that follows the directive.
Any 
 and 
 directives in a source file must occur before the first 
 (
) in the source file; otherwise a compile-time error occurs. In intuitive terms, 
 and 
 directives must precede any ""real code"" in the source file.
The example:
is valid because the 
 directives precede the first token (the 
 keyword) in the source file.
The following example results in a compile-time error because a 
 follows real code:
A 
 may define a conditional compilation symbol that is already defined, without there being any intervening 
 for that symbol. The example below defines a conditional compilation symbol 
 and then defines it again.
A 
 may ""undefine"" a conditional compilation symbol that is not defined. The example below defines a conditional compilation symbol 
 and then undefines it twice; although the second 
 has no effect, it is still valid.
Conditional compilation directives
The conditional compilation directives are used to conditionally include or exclude portions of a source file.
As indicated by the syntax, conditional compilation directives must be written as sets consisting of, in order, an 
 directive, zero or more 
 directives, zero or one 
 directive, and an 
 directive. Between the directives are conditional sections of source code. Each section is controlled by the immediately preceding directive. A conditional section may itself contain nested conditional compilation directives provided these directives form complete sets.
A 
 selects at most one of the contained 
s for normal lexical processing:
The 
s of the 
 and 
 directives are evaluated in order until one yields 
. If an expression yields 
, the 
 of the corresponding directive is selected.
If all 
s yield 
, and if an 
 directive is present, the 
 of the 
 directive is selected.
Otherwise, no 
 is selected.
The selected 
, if any, is processed as a normal 
: the source code contained in the section must adhere to the lexical grammar; tokens are generated from the source code in the section; and pre-processing directives in the section have the prescribed effects.
The remaining 
s, if any, are processed as 
s: except for pre-processing directives, the source code in the section need not adhere to the lexical grammar; no tokens are generated from the source code in the section; and pre-processing directives in the section must be lexically correct but are not otherwise processed. Within a 
 that is being processed as a 
, any nested 
s (contained in nested 
...
 and 
...
 constructs) are also processed as 
s.
The following example illustrates how conditional compilation directives can nest:
Except for pre-processing directives, skipped source code is not subject to lexical analysis. For example, the following is valid despite the unterminated comment in the 
 section:
Note, however, that pre-processing directives are required to be lexically correct even in skipped sections of source code.
Pre-processing directives are not processed when they appear inside multi-line input elements. For example, the program:
results in the output:
In peculiar cases, the set of pre-processing directives that is processed might depend on the evaluation of the 
. The example:
always produces the same token stream (
 
 
 
), regardless of whether or not 
 is defined. If 
 is defined, the only processed directives are 
 and 
, due to the multi-line comment. If 
 is undefined, then three directives (
, 
, 
) are part of the directive set.
Diagnostic directives
The diagnostic directives are used to explicitly generate error and warning messages that are reported in the same way as other compile-time errors and warnings.
The example:
always produces a warning (""Code review needed before check-in""), and produces a compile-time error (""A build can't be both debug and retail"") if the conditional symbols 
 and 
 are both defined. Note that a 
 can contain arbitrary text; specifically, it need not contain well-formed tokens, as shown by the single quote in the word 
.
Region directives
The region directives are used to explicitly mark regions of source code.
No semantic meaning is attached to a region; regions are intended for use by the programmer or by automated tools to mark a section of source code. The message specified in a 
 or 
 directive likewise has no semantic meaning; it merely serves to identify the region. Matching 
 and 
 directives may have different 
s.
The lexical processing of a region:
corresponds exactly to the lexical processing of a conditional compilation directive of the form:
Line directives
Line directives may be used to alter the line numbers and source file names that are reported by the compiler in output such as warnings and errors, and that are used by caller info attributes (
).
Line directives are most commonly used in meta-programming tools that generate C# source code from some other text input.
When no 
 directives are present, the compiler reports true line numbers and source file names in its output. When processing a 
 directive that includes a 
 that is not 
, the compiler treats the line after the directive as having the given line number (and file name, if specified).
A 
 directive reverses the effect of all preceding #line directives. The compiler reports true line information for subsequent lines, precisely as if no 
 directives had been processed.
A 
 directive has no effect on the file and line numbers reported in error messages, but does affect source level debugging. When debugging, all lines between a 
 directive and the subsequent 
 directive (that is not 
) have no line number information. When stepping through code in the debugger, these lines will be skipped entirely.
Note that a 
 differs from a regular string literal in that escape characters are not processed; the ""
"" character simply designates an ordinary backslash character within a 
.
Pragma directives
The 
 preprocessing directive is used to specify optional contextual information to the compiler. The information supplied in a 
 directive will never change program semantics.
C# provides 
 directives to control compiler warnings. Future versions of the language may include additional 
 directives. To ensure interoperability with other C# compilers, the Microsoft C# compiler does not issue compilation errors for unknown 
 directives; such directives do however generate warnings.
Pragma warning
The 
 directive is used to disable or restore all or a particular set of warning messages during compilation of the subsequent program text.
A 
 directive that omits the warning list affects all warnings. A 
 directive the includes a warning list affects only those warnings that are specified in the list.
A 
 directive disables all or the given set of warnings.
A 
 directive restores all or the given set of warnings to the state that was in effect at the beginning of the compilation unit. Note that if a particular warning was disabled externally, a 
 (whether for all or the specific warning) will not re-enable that warning.
The following example shows use of 
 to temporarily disable the warning reported when obsoleted members are referenced, using the warning number from the Microsoft C# compiler.
Basic concepts
Application Startup
An assembly that has an 
 is called an 
. When an application is run, a new 
 is created. Several different instantiations of an application may exist on the same machine at the same time, and each has its own application domain.
An application domain enables application isolation by acting as a container for application state. An application domain acts as a container and boundary for the types defined in the application and the class libraries it uses. Types loaded into one application domain are distinct from the same type loaded into another application domain, and instances of objects are not directly shared between application domains. For instance, each application domain has its own copy of static variables for these types, and a static constructor for a type is run at most once per application domain. Implementations are free to provide implementation-specific policy or mechanisms for the creation and destruction of application domains.
 occurs when the execution environment calls a designated method, which is referred to as the application's entry point. This entry point method is always named 
, and can have one of the following signatures:
As shown, the entry point may optionally return an 
 value. This return value is used in application termination (
).
The entry point may optionally have one formal parameter. The parameter may have any name, but the type of the parameter must be 
. If the formal parameter is present, the execution environment creates and passes a 
 argument containing the command-line arguments that were specified when the application was started. The 
 argument is never null, but it may have a length of zero if no command-line arguments were specified.
Since C# supports method overloading, a class or struct may contain multiple definitions of some method, provided each has a different signature. However, within a single program, no class or struct may contain more than one method called 
 whose definition qualifies it to be used as an application entry point. Other overloaded versions of 
 are permitted, however, provided they have more than one parameter, or their only parameter is other than type 
.
An application can be made up of multiple classes or structs. It is possible for more than one of these classes or structs to contain a method called 
 whose definition qualifies it to be used as an application entry point. In such cases, an external mechanism (such as a command-line compiler option) must be used to select one of these 
 methods as the entry point.
In C#, every method must be defined as a member of a class or struct. Ordinarily, the declared accessibility (
) of a method is determined by the access modifiers (
) specified in its declaration, and similarly the declared accessibility of a type is determined by the access modifiers specified in its declaration. In order for a given method of a given type to be callable, both the type and the member must be accessible. However, the application entry point is a special case. Specifically, the execution environment can access the application's entry point regardless of its declared accessibility and regardless of the declared accessibility of its enclosing type declarations.
The application entry point method may not be in a generic class declaration.
In all other respects, entry point methods behave like those that are not entry points.
Application termination
 returns control to the execution environment.
If the return type of the application's 
 method is 
, the value returned serves as the application's 
. The purpose of this code is to allow communication of success or failure to the execution environment.
If the return type of the entry point method is 
, reaching the right brace (
) which terminates that method, or executing a 
 statement that has no expression, results in a termination status code of 
.
Prior to an application's termination, destructors for all of its objects that have not yet been garbage collected are called, unless such cleanup has been suppressed (by a call to the library method 
, for example).
Declarations
Declarations in a C# program define the constituent elements of the program. C# programs are organized using namespaces (
), which can contain type declarations and nested namespace declarations. Type declarations (
) are used to define classes (
), structs (
), interfaces (
), enums (
), and delegates (
). The kinds of members permitted in a type declaration depend on the form of the type declaration. For instance, class declarations can contain declarations for constants (
), fields (
), methods (
), properties (
), events (
), indexers (
), operators (
), instance constructors (
), static constructors (
), destructors (
), and nested types(
).
A declaration defines a name in the 
 to which the declaration belongs. Except for overloaded members (
), it is a compile-time error to have two or more declarations that introduce members with the same name in a declaration space. It is never possible for a declaration space to contain different kinds of members with the same name. For example, a declaration space can never contain a field and a method by the same name.
There are several different types of declaration spaces, as described in the following.
Within all source files of a program, 
s with no enclosing 
 are members of a single combined declaration space called the 
.
Within all source files of a program, 
s within 
s that have the same fully qualified namespace name are members of a single combined declaration space.
Each class, struct, or interface declaration creates a new declaration space. Names are introduced into this declaration space through 
s, 
s, 
s, or 
s. Except for overloaded instance constructor declarations and static constructor declarations, a class or struct cannot contain a member declaration with the same name as the class or struct. A class, struct, or interface permits the declaration of overloaded methods and indexers. Furthermore, a class or struct permits the declaration of overloaded instance constructors and operators. For example, a class, struct, or interface may contain multiple method declarations with the same name, provided these method declarations differ in their signature (
). Note that base classes do not contribute to the declaration space of a class, and base interfaces do not contribute to the declaration space of an interface. Thus, a derived class or interface is allowed to declare a member with the same name as an inherited member. Such a member is said to 
 the inherited member.
Each delegate declaration creates a new declaration space. Names are introduced into this declaration space through formal parameters (
s and 
s) and 
s.
Each enumeration declaration creates a new declaration space. Names are introduced into this declaration space through 
.
Each method declaration, indexer declaration, operator declaration, instance constructor declaration and anonymous function creates a new declaration space called a 
. Names are introduced into this declaration space through formal parameters (
s and 
s) and 
s. The body of the function member or anonymous function, if any, is considered to be nested within the local variable declaration space. It is an error for a local variable declaration space and a nested local variable declaration space to contain elements with the same name. Thus, within a nested declaration space it is not possible to declare a local variable or constant with the same name as a local variable or constant in an enclosing declaration space. It is possible for two declaration spaces to contain elements with the same name as long as neither declaration space contains the other.
Each 
 or 
 , as well as a 
for
, 
foreach
 and 
using
 statement, creates a local variable declaration space for local variables and local constants . Names are introduced into this declaration space through 
s and 
s. Note that blocks that occur as or within the body of a function member or anonymous function are nested within the local variable declaration space declared by those functions for their parameters. Thus it is an error to have e.g. a method with a local variable and a parameter of the same name.
Each 
 or 
 creates a separate declaration space for labels. Names are introduced into this declaration space through 
s, and the names are referenced through 
s. The 
 of a block includes any nested blocks. Thus, within a nested block it is not possible to declare a label with the same name as a label in an enclosing block.
The textual order in which names are declared is generally of no significance. In particular, textual order is not significant for the declaration and use of namespaces, constants, methods, properties, events, indexers, operators, instance constructors, destructors, static constructors, and types. Declaration order is significant in the following ways:
Declaration order for field declarations and local variable declarations determines the order in which their initializers (if any) are executed.
Local variables must be defined before they are used (
).
Declaration order for enum member declarations (
) is significant when 
 values are omitted.
The declaration space of a namespace is ""open ended"", and two namespace declarations with the same fully qualified name contribute to the same declaration space. For example
The two namespace declarations above contribute to the same declaration space, in this case declaring two classes with the fully qualified names 
 and 
. Because the two declarations contribute to the same declaration space, it would have caused a compile-time error if each contained a declaration of a class with the same name.
As specified above, the declaration space of a block includes any nested blocks. Thus, in the following example, the 
 and 
 methods result in a compile-time error because the name 
 is declared in the outer block and cannot be redeclared in the inner block. However, the 
 and 
 methods are valid since the two 
's are declared in separate non-nested blocks.
Members
Namespaces and types have 
. The members of an entity are generally available through the use of a qualified name that starts with a reference to the entity, followed by a ""
"" token, followed by the name of the member.
Members of a type are either declared in the type declaration or 
 from the base class of the type. When a type inherits from a base class, all members of the base class, except instance constructors, destructors and static constructors, become members of the derived type. The declared accessibility of a base class member does not control whether the member is inherited—inheritance extends to any member that isn't an instance constructor, static constructor, or destructor. However, an inherited member may not be accessible in a derived type, either because of its declared accessibility (
) or because it is hidden by a declaration in the type itself (
).
Namespace members
Namespaces and types that have no enclosing namespace are members of the 
. This corresponds directly to the names declared in the global declaration space.
Namespaces and types declared within a namespace are members of that namespace. This corresponds directly to the names declared in the declaration space of the namespace.
Namespaces have no access restrictions. It is not possible to declare private, protected, or internal namespaces, and namespace names are always publicly accessible.
Struct members
The members of a struct are the members declared in the struct and the members inherited from the struct's direct base class 
 and the indirect base class 
.
The members of a simple type correspond directly to the members of the struct type aliased by the simple type:
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
The members of 
 are the members of the 
 struct.
Enumeration members
The members of an enumeration are the constants declared in the enumeration and the members inherited from the enumeration's direct base class 
 and the indirect base classes 
 and 
.
Class members
The members of a class are the members declared in the class and the members inherited from the base class (except for class 
 which has no base class). The members inherited from the base class include the constants, fields, methods, properties, events, indexers, operators, and types of the base class, but not the instance constructors, destructors and static constructors of the base class. Base class members are inherited without regard to their accessibility.
A class declaration may contain declarations of constants, fields, methods, properties, events, indexers, operators, instance constructors, destructors, static constructors and types.
The members of 
 and 
 correspond directly to the members of the class types they alias:
The members of 
 are the members of the 
 class.
The members of 
 are the members of the 
 class.
Interface members
The members of an interface are the members declared in the interface and in all base interfaces of the interface. The members in class 
 are not, strictly speaking, members of any interface (
). However, the members in class 
 are available via member lookup in any interface type (
).
Array members
The members of an array are the members inherited from class 
.
Delegate members
The members of a delegate are the members inherited from class 
.
Member access
Declarations of members allow control over member access. The accessibility of a member is established by the declared accessibility (
) of the member combined with the accessibility of the immediately containing type, if any.
When access to a particular member is allowed, the member is said to be 
. Conversely, when access to a particular member is disallowed, the member is said to be 
. Access to a member is permitted when the textual location in which the access takes place is included in the accessibility domain (
) of the member.
Declared accessibility
The 
 of a member can be one of the following:
Public, which is selected by including a 
 modifier in the member declaration. The intuitive meaning of 
 is ""access not limited"".
Protected, which is selected by including a 
 modifier in the member declaration. The intuitive meaning of 
 is ""access limited to the containing class or types derived from the containing class"".
Internal, which is selected by including an 
 modifier in the member declaration. The intuitive meaning of 
 is ""access limited to this program"".
Protected internal (meaning protected or internal), which is selected by including both a 
 and an 
 modifier in the member declaration. The intuitive meaning of 
 is ""access limited to this program or types derived from the containing class"".
Private, which is selected by including a 
 modifier in the member declaration. The intuitive meaning of 
 is ""access limited to the containing type"".
Depending on the context in which a member declaration takes place, only certain types of declared accessibility are permitted. Furthermore, when a member declaration does not include any access modifiers, the context in which the declaration takes place determines the default declared accessibility.
Namespaces implicitly have 
 declared accessibility. No access modifiers are allowed on namespace declarations.
Types declared in compilation units or namespaces can have 
 or 
 declared accessibility and default to 
 declared accessibility.
Class members can have any of the five kinds of declared accessibility and default to 
 declared accessibility. (Note that a type declared as a member of a class can have any of the five kinds of declared accessibility, whereas a type declared as a member of a namespace can have only 
 or 
 declared accessibility.)
Struct members can have 
, 
, or 
 declared accessibility and default to 
 declared accessibility because structs are implicitly sealed. Struct members introduced in a struct (that is, not inherited by that struct) cannot have 
 or 
 declared accessibility. (Note that a type declared as a member of a struct can have 
, 
, or 
 declared accessibility, whereas a type declared as a member of a namespace can have only 
 or 
 declared accessibility.)
Interface members implicitly have 
 declared accessibility. No access modifiers are allowed on interface member declarations.
Enumeration members implicitly have 
 declared accessibility. No access modifiers are allowed on enumeration member declarations.
Accessibility domains
The 
 of a member consists of the (possibly disjoint) sections of program text in which access to the member is permitted. For purposes of defining the accessibility domain of a member, a member is said to be 
 if it is not declared within a type, and a member is said to be 
 if it is declared within another type. Furthermore, the 
 of a program is defined as all program text contained in all source files of the program, and the program text of a type is defined as all program text contained in the 
s of that type (including, possibly, types that are nested within the type).
The accessibility domain of a predefined type (such as 
, 
, or 
) is unlimited.
The accessibility domain of a top-level unbound type 
 (
) that is declared in a program 
 is defined as follows:
If the declared accessibility of 
 is 
, the accessibility domain of 
 is the program text of 
 and any program that references 
.
If the declared accessibility of 
 is 
, the accessibility domain of 
 is the program text of 
.
From these definitions it follows that the accessibility domain of a top-level unbound type is always at least the program text of the program in which that type is declared.
The accessibility domain for a constructed type 
 is the intersection of the accessibility domain of the unbound generic type 
 and the accessibility domains of the type arguments 
.
The accessibility domain of a nested member 
 declared in a type 
 within a program 
 is defined as follows (noting that 
 itself may possibly be a type):
If the declared accessibility of 
 is 
, the accessibility domain of 
 is the accessibility domain of 
.
If the declared accessibility of 
 is 
, let 
 be the union of the program text of 
 and the program text of any type derived from 
, which is declared outside 
. The accessibility domain of 
 is the intersection of the accessibility domain of 
 with 
.
If the declared accessibility of 
 is 
, let 
 be the union of the program text of 
 and the program text of any type derived from 
. The accessibility domain of 
 is the intersection of the accessibility domain of 
 with 
.
If the declared accessibility of 
 is 
, the accessibility domain of 
 is the intersection of the accessibility domain of 
 with the program text of 
.
If the declared accessibility of 
 is 
, the accessibility domain of 
 is the program text of 
.
From these definitions it follows that the accessibility domain of a nested member is always at least the program text of the type in which the member is declared. Furthermore, it follows that the accessibility domain of a member is never more inclusive than the accessibility domain of the type in which the member is declared.
In intuitive terms, when a type or member 
 is accessed, the following steps are evaluated to ensure that the access is permitted:
First, if 
 is declared within a type (as opposed to a compilation unit or a namespace), a compile-time error occurs if that type is not accessible.
Then, if 
 is 
, the access is permitted.
Otherwise, if 
 is 
, the access is permitted if it occurs within the program in which 
 is declared, or if it occurs within a class derived from the class in which 
 is declared and takes place through the derived class type (
).
Otherwise, if 
 is 
, the access is permitted if it occurs within the class in which 
 is declared, or if it occurs within a class derived from the class in which 
 is declared and takes place through the derived class type (
).
Otherwise, if 
 is 
, the access is permitted if it occurs within the program in which 
 is declared.
Otherwise, if 
 is 
, the access is permitted if it occurs within the type in which 
 is declared.
Otherwise, the type or member is inaccessible, and a compile-time error occurs.
In the example
the classes and members have the following accessibility domains:
The accessibility domain of 
 and 
 is unlimited.
The accessibility domain of 
, 
, 
, 
, 
, 
, and 
 is the program text of the containing program.
The accessibility domain of 
 is the program text of 
.
The accessibility domain of 
 and 
 is the program text of 
, including the program text of 
 and 
.
The accessibility domain of 
 is the program text of 
.
The accessibility domain of 
 and 
 is the program text of 
, including the program text of 
 and 
.
The accessibility domain of 
 is the program text of 
.
As the example illustrates, the accessibility domain of a member is never larger than that of a containing type. For example, even though all 
 members have public declared accessibility, all but 
 have accessibility domains that are constrained by a containing type.
As described in 
, all members of a base class, except for instance constructors, destructors and static constructors, are inherited by derived types. This includes even private members of a base class. However, the accessibility domain of a private member includes only the program text of the type in which the member is declared. In the example
the 
 class inherits the private member 
 from the 
 class. Because the member is private, it is only accessible within the 
 of 
. Thus, the access to 
 succeeds in the 
 method, but fails in the 
 method.
Protected access for instance members
When a 
 instance member is accessed outside the program text of the class in which it is declared, and when a 
 instance member is accessed outside the program text of the program in which it is declared, the access must take place within a class declaration that derives from the class in which it is declared. Furthermore, the access is required to take place through an instance of that derived class type or a class type constructed from it. This restriction prevents one derived class from accessing protected members of other derived classes, even when the members are inherited from the same base class.
Let 
 be a base class that declares a protected instance member 
, and let 
 be a class that derives from 
. Within the 
 of 
, access to 
 can take one of the following forms:
An unqualified 
 or 
 of the form 
.
A 
 of the form 
, provided the type of 
 is 
 or a class derived from 
, where 
 is the class type 
, or a class type constructed from 
A 
 of the form 
.
In addition to these forms of access, a derived class can access a protected instance constructor of a base class in a 
 (
).
In the example
within 
, it is possible to access 
 through instances of both 
 and 
, since in either case the access takes place through an instance of 
 or a class derived from 
. However, within 
, it is not possible to access 
 through an instance of 
, since 
 does not derive from 
.
In the example
the three assignments to 
 are permitted because they all take place through instances of class types constructed from the generic type.
Accessibility constraints
Several constructs in the C# language require a type to be 
 a member or another type. A type 
 is said to be at least as accessible as a member or type 
 if the accessibility domain of 
 is a superset of the accessibility domain of 
. In other words, 
 is at least as accessible as 
 if 
 is accessible in all contexts in which 
 is accessible.
The following accessibility constraints exist:
The direct base class of a class type must be at least as accessible as the class type itself.
The explicit base interfaces of an interface type must be at least as accessible as the interface type itself.
The return type and parameter types of a delegate type must be at least as accessible as the delegate type itself.
The type of a constant must be at least as accessible as the constant itself.
The type of a field must be at least as accessible as the field itself.
The return type and parameter types of a method must be at least as accessible as the method itself.
The type of a property must be at least as accessible as the property itself.
The type of an event must be at least as accessible as the event itself.
The type and parameter types of an indexer must be at least as accessible as the indexer itself.
The return type and parameter types of an operator must be at least as accessible as the operator itself.
The parameter types of an instance constructor must be at least as accessible as the instance constructor itself.
In the example
the 
 class results in a compile-time error because 
 is not at least as accessible as 
.
Likewise, in the example
the 
 method in 
 results in a compile-time error because the return type 
 is not at least as accessible as the method.
Signatures and overloading
Methods, instance constructors, indexers, and operators are characterized by their 
:
The signature of a method consists of the name of the method, the number of type parameters and the type and kind (value, reference, or output) of each of its formal parameters, considered in the order left to right. For these purposes, any type parameter of the method that occurs in the type of a formal parameter is identified not by its name, but by its ordinal position in the type argument list of the method. The signature of a method specifically does not include the return type, the 
 modifier that may be specified for the right-most parameter, nor the optional type parameter constraints.
The signature of an instance constructor consists of the type and kind (value, reference, or output) of each of its formal parameters, considered in the order left to right. The signature of an instance constructor specifically does not include the 
 modifier that may be specified for the right-most parameter.
The signature of an indexer consists of the type of each of its formal parameters, considered in the order left to right. The signature of an indexer specifically does not include the element type, nor does it include the 
 modifier that may be specified for the right-most parameter.
The signature of an operator consists of the name of the operator and the type of each of its formal parameters, considered in the order left to right. The signature of an operator specifically does not include the result type.
Signatures are the enabling mechanism for 
 of members in classes, structs, and interfaces:
Overloading of methods permits a class, struct, or interface to declare multiple methods with the same name, provided their signatures are unique within that class, struct, or interface.
Overloading of instance constructors permits a class or struct to declare multiple instance constructors, provided their signatures are unique within that class or struct.
Overloading of indexers permits a class, struct, or interface to declare multiple indexers, provided their signatures are unique within that class, struct, or interface.
Overloading of operators permits a class or struct to declare multiple operators with the same name, provided their signatures are unique within that class or struct.
Although 
 and 
 parameter modifiers are considered part of a signature, members declared in a single type cannot differ in signature solely by 
 and 
. A compile-time error occurs if two members are declared in the same type with signatures that would be the same if all parameters in both methods with 
 modifiers were changed to 
 modifiers. For other purposes of signature matching (e.g., hiding or overriding), 
 and 
 are considered part of the signature and do not match each other. (This restriction is to allow C#  programs to be easily translated to run on the Common Language Infrastructure (CLI), which does not provide a way to define methods that differ solely in 
 and 
.)
For the purposes of singatures, the types 
 and 
 are considered the same. Members declared in a single type can therefore not differ in signature solely by 
 and 
.
The following example shows a set of overloaded method declarations along with their signatures.
Note that any 
 and 
 parameter modifiers (
) are part of a signature. Thus, 
 and 
 are unique signatures. However, 
 and 
 cannot be declared within the same interface because their signatures differ solely by 
 and 
. Also, note that the return type and the 
 modifier are not part of a signature, so it is not possible to overload solely based on return type or on the inclusion or exclusion of the 
 modifier. As such, the declarations of the methods 
 and 
 identified above result in a compile-time error.
Scopes
The 
 of a name is the region of program text within which it is possible to refer to the entity declared by the name without qualification of the name. Scopes can be 
, and an inner scope may redeclare the meaning of a name from an outer scope (this does not, however, remove the restriction imposed by 
 that within a nested block it is not possible to declare a local variable with the same name as a local variable in an enclosing block). The name from the outer scope is then said to be 
 in the region of program text covered by the inner scope, and access to the outer name is only possible by qualifying the name.
The scope of a namespace member declared by a 
 (
) with no enclosing 
 is the entire program text.
The scope of a namespace member declared by a 
 within a 
 whose fully qualified name is 
 is the 
 of every 
 whose fully qualified name is 
 or starts with 
, followed by a period.
The scope of name defined by an 
 extends over the 
s, 
 and 
s of its immediately containing compilation unit or namespace body. An 
 does not contribute any new members to the underlying declaration space. In other words, an 
 is not transitive, but, rather, affects only the compilation unit or namespace body in which it occurs.
The scope of a name defined or imported by a 
 (
) extends over the 
s of the 
 or 
 in which the 
 occurs. A 
 may make zero or more namespace, type or member names available within a particular 
 or 
, but does not contribute any new members to the underlying declaration space. In other words, a 
 is not transitive but rather affects only the 
 or 
 in which it occurs.
The scope of a type parameter declared by a 
 on a 
 (
) is the 
, 
s, and 
 of that 
.
The scope of a type parameter declared by a 
 on a 
 (
) is the 
, 
s, and 
 of that 
.
The scope of a type parameter declared by a 
 on an 
 (
) is the 
, 
s, and 
 of that 
.
The scope of a type parameter declared by a 
 on a 
 (
) is the 
, 
, and 
s of that 
.
The scope of a member declared by a 
 (
) is the 
 in which the declaration occurs. In addition, the scope of a class member extends to the 
 of those derived classes that are included in the accessibility domain (
) of the member.
The scope of a member declared by a 
 (
) is the 
 in which the declaration occurs.
The scope of a member declared by an 
  (
) is the 
 in which the declaration occurs.
The scope of a parameter declared in a 
 (
) is the 
 of that 
.
The scope of a parameter declared in an 
 (
) is the 
 of that 
.
The scope of a parameter declared in an 
 (
) is the 
 of that 
.
The scope of a parameter declared in a 
 (
) is the 
 and 
 of that 
.
The scope of a parameter declared in a 
 (
) is the 
 of that 
The scope of a parameter declared in an 
 (
) is the 
 of that 
.
The scope of a label declared in a 
 (
) is the 
 in which the declaration occurs.
The scope of a local variable declared in a 
 (
) is the block in which the declaration occurs.
The scope of a local variable declared in a 
 of a 
 statement (
) is the 
.
The scope of a local variable declared in a 
 of a 
 statement (
) is the 
, the 
, the 
, and the contained 
 of the 
 statement.
The scope of a local constant declared in a 
 (
) is the block in which the declaration occurs. It is a compile-time error to refer to a local constant in a textual position that precedes its 
.
The scope of a variable declared as part of a 
, 
, 
 or 
 is determined by the expansion of the given construct.
Within the scope of a namespace, class, struct, or enumeration member it is possible to refer to the member in a textual position that precedes the declaration of the member. For example
Here, it is valid for 
 to refer to 
 before it is declared.
Within the scope of a local variable, it is a compile-time error to refer to the local variable in a textual position that precedes the 
 of the local variable. For example
In the 
 method above, the first assignment to 
 specifically does not refer to the field declared in the outer scope. Rather, it refers to the local variable and it results in a compile-time error because it textually precedes the declaration of the variable. In the 
 method, the use of 
 in the initializer for the declaration of 
 is valid because the use does not precede the 
. In the 
 method, a subsequent 
 correctly refers to a local variable declared in an earlier 
 within the same 
.
The scoping rules for local variables are designed to guarantee that the meaning of a name used in an expression context is always the same within a block. If the scope of a local variable were to extend only from its declaration to the end of the block, then in the example above, the first assignment would assign to the instance variable and the second assignment would assign to the local variable, possibly leading to compile-time errors if the statements of the block were later to be rearranged.
The meaning of a name within a block may differ based on the context in which the name is used. In the example
the name 
 is used in an expression context to refer to the local variable 
 and in a type context to refer to the class 
.
Name hiding
The scope of an entity typically encompasses more program text than the declaration space of the entity. In particular, the scope of an entity may include declarations that introduce new declaration spaces containing entities of the same name. Such declarations cause the original entity to become 
. Conversely, an entity is said to be 
 when it is not hidden.
Name hiding occurs when scopes overlap through nesting and when scopes overlap through inheritance. The characteristics of the two types of hiding are described in the following sections.
Hiding through nesting
Name hiding through nesting can occur as a result of nesting namespaces or types within namespaces, as a result of nesting types within classes or structs, and as a result of parameter and local variable declarations.
In the example
within the 
 method, the instance variable 
 is hidden by the local variable 
, but within the 
 method, 
 still refers to the instance variable.
When a name in an inner scope hides a name in an outer scope, it hides all overloaded occurrences of that name. In the example
the call 
 invokes the 
 declared in 
 because all outer occurrences of 
 are hidden by the inner declaration. For the same reason, the call 
 results in a compile-time error.
Hiding through inheritance
Name hiding through inheritance occurs when classes or structs redeclare names that were inherited from base classes. This type of name hiding takes one of the following forms:
A constant, field, property, event, or type introduced in a class or struct hides all base class members with the same name.
A method introduced in a class or struct hides all non-method base class members with the same name, and all base class methods with the same signature (method name and parameter count, modifiers, and types).
An indexer introduced in a class or struct hides all base class indexers with the same signature (parameter count and types).
The rules governing operator declarations (
) make it impossible for a derived class to declare an operator with the same signature as an operator in a base class. Thus, operators never hide one another.
Contrary to hiding a name from an outer scope, hiding an accessible name from an inherited scope causes a warning to be reported. In the example
the declaration of 
 in 
 causes a warning to be reported. Hiding an inherited name is specifically not an error, since that would preclude separate evolution of base classes. For example, the above situation might have come about because a later version of 
 introduced an 
 method that wasn't present in an earlier version of the class. Had the above situation been an error, then any change made to a base class in a separately versioned class library could potentially cause derived classes to become invalid.
The warning caused by hiding an inherited name can be eliminated through use of the 
 modifier:
The 
 modifier indicates that the 
 in 
 is ""new"", and that it is indeed intended to hide the inherited member.
A declaration of a new member hides an inherited member only within the scope of the new member.
In the example above, the declaration of 
 in 
 hides the 
 that was inherited from 
, but since the new 
 in 
 has private access, its scope does not extend to 
. Thus, the call 
 in 
 is valid and will invoke 
.
Namespace and type names
Several contexts in a C# program require a 
 or a 
 to be specified.
A 
 is a 
 that refers to a namespace. Following resolution as described below, the 
 of a 
 must refer to a namespace, or otherwise a compile-time error occurs. No type arguments (
) can be present in a 
 (only types can have type arguments).
A 
 is a 
 that refers to a type. Following resolution as described below, the 
 of a 
 must refer to a type, or otherwise a compile-time error occurs.
If the 
 is a qualified-alias-member its meaning is as described in 
. Otherwise, a 
 has one of four forms:
where 
 is a single identifier, 
 is a 
 and 
 is an optional 
. When no 
 is specified, consider 
 to be zero.
The meaning of a 
 is determined as follows:
If the 
 is of the form 
 or of the form 
:
If 
 is zero and the 
 appears within a generic method declaration (
) and if that declaration includes a type parameter (
) with name 
, then the 
 refers to that type parameter.
Otherwise, if the 
 appears within a type declaration, then for each instance type 
 (
), starting with the instance type of that type declaration and continuing with the instance type of each enclosing class or struct declaration (if any):
If 
 is zero and the declaration of 
 includes a type parameter with name 
, then the 
 refers to that type parameter.
Otherwise, if the 
 appears within the body of the type declaration, and 
 or any of its base types contain a nested accessible type having name 
 and 
 type parameters, then the 
 refers to that type constructed with the given type arguments. If there is more than one such type, the type declared within the more derived type is selected. Note that non-type members (constants, fields, methods, properties, indexers, operators, instance constructors, destructors, and static constructors) and type members with a different number of type parameters are ignored when determining the meaning of the 
.
If the previous steps were unsuccessful then, for each namespace 
, starting with the namespace in which the 
 occurs, continuing with each enclosing namespace (if any), and ending with the global namespace, the following steps are evaluated until an entity is located:
If 
 is zero and 
 is the name of a namespace in 
, then:
If the location where the 
 occurs is enclosed by a namespace declaration for 
 and the namespace declaration contains an 
 or 
 that associates the name 
 with a namespace or type, then the 
 is ambiguous and a compile-time error occurs.
Otherwise, the 
 refers to the namespace named 
 in 
.
Otherwise, if 
 contains an accessible type having name 
 and 
 type parameters, then:
If 
 is zero and the location where the 
 occurs is enclosed by a namespace declaration for 
 and the namespace declaration contains an 
 or 
 that associates the name 
 with a namespace or type, then the 
 is ambiguous and a compile-time error occurs.
Otherwise, the 
 refers to the type constructed with the given type arguments.
Otherwise, if the location where the 
 occurs is enclosed by a namespace declaration for 
:
If 
 is zero and the namespace declaration contains an 
 or 
 that associates the name 
 with an imported namespace or type, then the 
 refers to that namespace or type.
Otherwise, if the namespaces and type declarations imported by the 
s and 
s of the namespace declaration contain exactly one accessible type having name 
 and 
 type parameters, then the 
 refers to that type constructed with the given type arguments.
Otherwise, if the namespaces and type declarations imported by the 
s and 
s of the namespace declaration contain more than one accessible type having name 
 and 
 type parameters, then the 
 is ambiguous and an error occurs.
Otherwise, the 
 is undefined and a compile-time error occurs.
Otherwise, the 
 is of the form 
 or of the form 
. 
 is first resolved as a 
. If the resolution of 
 is not successful, a compile-time error occurs. Otherwise, 
 or 
 is resolved as follows:
If 
 is zero and 
 refers to a namespace and 
 contains a nested namespace with name 
, then the 
 refers to that nested namespace.
Otherwise, if 
 refers to a namespace and 
 contains an accessible type having name 
 and 
 type parameters, then the 
 refers to that type constructed with the given type arguments.
Otherwise, if 
 refers to a (possibly constructed) class or struct type and 
 or any of its base classes contain a nested accessible type having name 
 and 
 type parameters, then the 
 refers to that type constructed with the given type arguments. If there is more than one such type, the type declared within the more derived type is selected. Note that if the meaning of 
 is being determined as part of resolving the base class specification of 
 then the direct base class of 
 is considered to be object (
).
Otherwise, 
 is an invalid 
, and a compile-time error occurs.
A 
 is permitted to reference a static class (
) only if
The 
 is the 
 in a 
 of the form 
, or
The 
 is the 
 in a 
 (
1) of the form 
.
Fully qualified names
Every namespace and type has a 
, which uniquely identifies the namespace or type amongst all others. The fully qualified name of a namespace or type 
 is determined as follows:
If 
 is a member of the global namespace, its fully qualified name is 
.
Otherwise, its fully qualified name is 
, where 
 is the fully qualified name of the namespace or type in which 
 is declared.
In other words, the fully qualified name of 
 is the complete hierarchical path of identifiers that lead to 
, starting from the global namespace. Because every member of a namespace or type must have a unique name, it follows that the fully qualified name of a namespace or type is always unique.
The example below shows several namespace and type declarations along with their associated fully qualified names.
Automatic memory management
C# employs automatic memory management, which frees developers from manually allocating and freeing the memory occupied by objects. Automatic memory management policies are implemented by a 
. The memory management life cycle of an object is as follows:
When the object is created, memory is allocated for it, the constructor is run, and the object is considered live.
If the object, or any part of it, cannot be accessed by any possible continuation of execution, other than the running of destructors, the object is considered no longer in use, and it becomes eligible for destruction. The C# compiler and the garbage collector may choose to analyze code to determine which references to an object may be used in the future. For instance, if a local variable that is in scope is the only existing reference to an object, but that local variable is never referred to in any possible continuation of execution from the current execution point in the procedure, the garbage collector may (but is not required to) treat the object as no longer in use.
Once the object is eligible for destruction, at some unspecified later time the destructor (
) (if any) for the object is run. Under normal circumstances the destructor for the object is run once only, though implementation-specific APIs may allow this behavior to be overridden.
Once the destructor for an object is run, if that object, or any part of it, cannot be accessed by any possible continuation of execution, including the running of destructors, the object is considered inaccessible and the object becomes eligible for collection.
Finally, at some time after the object becomes eligible for collection, the garbage collector frees the memory associated with that object.
The garbage collector maintains information about object usage, and uses this information to make memory management decisions, such as where in memory to locate a newly created object, when to relocate an object, and when an object is no longer in use or inaccessible.
Like other languages that assume the existence of a garbage collector, C# is designed so that the garbage collector may implement a wide range of memory management policies. For instance, C# does not require that destructors be run or that objects be collected as soon as they are eligible, or that destructors be run in any particular order, or on any particular thread.
The behavior of the garbage collector can be controlled, to some degree, via static methods on the class 
. This class can be used to request a collection to occur, destructors to be run (or not run), and so forth.
Since the garbage collector is allowed wide latitude in deciding when to collect objects and run destructors, a conforming implementation may produce output that differs from that shown by the following code. The program
creates an instance of class 
 and an instance of class 
. These objects become eligible for garbage collection when the variable 
 is assigned the value 
, since after this time it is impossible for any user-written code to access them. The output could be either
or
because the language imposes no constraints on the order in which objects are garbage collected.
In subtle cases, the distinction between ""eligible for destruction"" and ""eligible for collection"" can be important. For example,
In the above program, if the garbage collector chooses to run the destructor of 
 before the destructor of 
, then the output of this program might be:
Note that although the instance of 
 was not in use and 
's destructor was run, it is still possible for methods of 
 (in this case, 
) to be called from another destructor. Also, note that running of a destructor may cause an object to become usable from the mainline program again. In this case, the running of 
's destructor caused an instance of 
 that was previously not in use to become accessible from the live reference 
. After the call to 
, the instance of 
 is eligible for collection, but the instance of 
 is not, because of the reference 
.
To avoid confusion and unexpected behavior, it is generally a good idea for destructors to only perform cleanup on data stored in their object's own fields, and not to perform any actions on referenced objects or static fields.
An alternative to using destructors is to let a class implement the 
 interface. This allows the client of the object to determine when to release the resources of the object, typically by accessing the object as a resource in a 
 statement (
).
Execution order
Execution of a C# program proceeds such that the side effects of each executing thread are preserved at critical execution points. A 
 is defined as a read or write of a volatile field, a write to a non-volatile variable, a write to an external resource, and the throwing of an exception. The critical execution points at which the order of these side effects must be preserved are references to volatile fields (
), 
 statements (
), and thread creation and termination. The execution environment is free to change the order of execution of a C# program, subject to the following constraints:
Data dependence is preserved within a thread of execution. That is, the value of each variable is computed as if all statements in the thread were executed in original program order.
Initialization ordering rules are preserved (
 and 
).
The ordering of side effects is preserved with respect to volatile reads and writes (
). Additionally, the execution environment need not evaluate part of an expression if it can deduce that that expression's value is not used and that no needed side effects are produced (including any caused by calling a method or accessing a volatile field). When program execution is interrupted by an asynchronous event (such as an exception thrown by another thread), it is not guaranteed that the observable side effects are visible in the original program order.
Types
The types of the C# language are divided into two main categories: 
 and 
. Both value types and reference types may be 
, which take one or more 
. Type parameters can designate both value types and reference types.
The final category of types, pointers, is available only in unsafe code. This is discussed further in 
.
Value types differ from reference types in that variables of the value types directly contain their data, whereas variables of the reference types store 
 to their data, the latter being known as 
. With reference types, it is possible for two variables to reference the same object, and thus possible for operations on one variable to affect the object referenced by the other variable. With value types, the variables each have their own copy of the data, and it is not possible for operations on one to affect the other.
C#'s type system is unified such that a value of any type can be treated as an object. Every type in C# directly or indirectly derives from the 
 class type, and 
 is the ultimate base class of all types. Values of reference types are treated as objects simply by viewing the values as type 
. Values of value types are treated as objects by performing boxing and unboxing operations (
).
Value types
A value type is either a struct type or an enumeration type. C# provides a set of predefined struct types called the 
. The simple types are identified through reserved words.
Unlike a variable of a reference type, a variable of a value type can contain the value 
 only if the value type is a nullable type.  For every non-nullable value type there is a corresponding nullable value type denoting the same set of values plus the value 
.
Assignment to a variable of a value type creates a copy of the value being assigned. This differs from assignment to a variable of a reference type, which copies the reference but not the object identified by the reference.
The System.ValueType type
All value types implicitly inherit from the class 
, which, in turn, inherits from class 
. It is not possible for any type to derive from a value type, and value types are thus implicitly sealed (
).
Note that 
 is not itself a 
. Rather, it is a 
 from which all 
s are automatically derived.
Default constructors
All value types implicitly declare a public parameterless instance constructor called the 
. The default constructor returns a zero-initialized instance known as the 
 for the value type:
For all 
s, the default value is the value produced by a bit pattern of all zeros:
For 
, 
, 
, 
, 
, 
, 
, and 
, the default value is 
.
For 
, the default value is 
.
For 
, the default value is 
.
For 
, the default value is 
.
For 
, the default value is 
.
For 
, the default value is 
.
For an 
 
, the default value is 
, converted to the type 
.
For a 
, the default value is the value produced by setting all value type fields to their default value and all reference type fields to 
.
For a 
 the default value is an instance for which the 
 property is false and the 
 property is undefined. The default value is also known as the 
 of the nullable type.
Like any other instance constructor, the default constructor of a value type is invoked using the 
 operator. For efficiency reasons, this requirement is not intended to actually have the implementation generate a constructor call. In the example below, variables 
 and 
 are both initialized to zero.
Because every value type implicitly has a public parameterless instance constructor, it is not possible for a struct type to contain an explicit declaration of a parameterless constructor. A struct type is however permitted to declare parameterized instance constructors (
).
Struct types
A struct type is a value type that can declare constants, fields, methods, properties, indexers, operators, instance constructors, static constructors, and nested types. The declaration of struct types is described in 
.
Simple types
C# provides a set of predefined struct types called the 
. The simple types are identified through reserved words, but these reserved words are simply aliases for predefined struct types in the 
 namespace, as described in the table below.
Reserved word
Aliased type
Because a simple type aliases a struct type, every simple type has members. For example, 
 has the members declared in 
 and the members inherited from 
, and the following statements are permitted:
The simple types differ from other struct types in that they permit certain additional operations:
Most simple types permit values to be created by writing 
literals
 (
). For example, 
 is a literal of type 
 and 
 is a literal of type 
. C# makes no provision for literals of struct types in general, and non-default values of other struct types are ultimately always created through instance constructors of those struct types.
When the operands of an expression are all simple type constants, it is possible for the compiler to evaluate the expression at compile-time. Such an expression is known as a 
 (
). Expressions involving operators defined by other struct types are not considered to be constant expressions.
Through 
 declarations it is possible to declare constants of the simple types (
). It is not possible to have constants of other struct types, but a similar effect is provided by 
 fields.
Conversions involving simple types can participate in evaluation of conversion operators defined by other struct types, but a user-defined conversion operator can never participate in evaluation of another user-defined operator (
).
Integral types
C# supports nine integral types: 
, 
, 
, 
, 
, 
, 
, 
, and 
. The integral types have the following sizes and ranges of values:
The 
 type represents signed 8-bit integers with values between -128 and 127.
The 
 type represents unsigned 8-bit integers with values between 0 and 255.
The 
 type represents signed 16-bit integers with values between -32768 and 32767.
The 
 type represents unsigned 16-bit integers with values between 0 and 65535.
The 
 type represents signed 32-bit integers with values between -2147483648 and 2147483647.
The 
 type represents unsigned 32-bit integers with values between 0 and 4294967295.
The 
 type represents signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.
The 
 type represents unsigned 64-bit integers with values between 0 and 18446744073709551615.
The 
 type represents unsigned 16-bit integers with values between 0 and 65535. The set of possible values for the 
 type corresponds to the Unicode character set. Although 
 has the same representation as 
, not all operations permitted on one type are permitted on the other.
The integral-type unary and binary operators always operate with signed 32-bit precision, unsigned 32-bit precision, signed 64-bit precision, or unsigned 64-bit precision:
For the unary 
 and 
 operators, the operand is converted to type 
, where 
 is the first of 
, 
, 
, and 
 that can fully represent all possible values of the operand. The operation is then performed using the precision of type 
, and the type of the result is 
.
For the unary 
 operator, the operand is converted to type 
, where 
 is the first of 
 and 
 that can fully represent all possible values of the operand. The operation is then performed using the precision of type 
, and the type of the result is 
. The unary 
 operator cannot be applied to operands of type 
.
For the binary 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and 
 operators, the operands are converted to type 
, where 
 is the first of 
, 
, 
, and 
 that can fully represent all possible values of both operands. The operation is then performed using the precision of type 
, and the type of the result is 
 (or 
 for the relational operators). It is not permitted for one operand to be of type 
 and the other to be of type 
 with the binary operators.
For the binary 
 and 
 operators, the left operand is converted to type 
, where 
 is the first of 
, 
, 
, and 
 that can fully represent all possible values of the operand. The operation is then performed using the precision of type 
, and the type of the result is 
.
The 
 type is classified as an integral type, but it differs from the other integral types in two ways:
There are no implicit conversions from other types to the 
 type. In particular, even though the 
, 
, and 
 types have ranges of values that are fully representable using the 
 type, implicit conversions from 
, 
, or 
 to 
 do not exist.
Constants of the 
 type must be written as 
s or as 
s in combination with a cast to type 
. For example, 
 is the same as 
.
The 
 and 
 operators and statements are used to control overflow checking for integral-type arithmetic operations and conversions (
). In a 
 context, an overflow produces a compile-time error or causes a 
 to be thrown. In an 
 context, overflows are ignored and any high-order bits that do not fit in the destination type are discarded.
Floating point types
C# supports two floating point types: 
 and 
. The 
 and 
 types are represented using the 32-bit single-precision and 64-bit double-precision IEEE 754 formats, which provide the following sets of values:
Positive zero and negative zero. In most situations, positive zero and negative zero behave identically as the simple value zero, but certain operations distinguish between the two (
).
Positive infinity and negative infinity. Infinities are produced by such operations as dividing a non-zero number by zero. For example, 
 yields positive infinity, and 
 yields negative infinity.
The 
 value, often abbreviated NaN. NaNs are produced by invalid floating-point operations, such as dividing zero by zero.
The finite set of non-zero values of the form 
, where 
 is 1 or -1, and 
 and 
 are determined by the particular floating-point type: For 
, 
 and 
, and for 
, 
 and 
. Denormalized floating-point numbers are considered valid non-zero values.
The 
 type can represent values ranging from approximately 
 to 
 with a precision of 7 digits.
The 
 type can represent values ranging from approximately 
 to 
 with a precision of 15-16 digits.
If one of the operands of a binary operator is of a floating-point type, then the other operand must be of an integral type or a floating-point type, and the operation is evaluated as follows:
If one of the operands is of an integral type, then that operand is converted to the floating-point type of the other operand.
Then, if either of the operands is of type 
, the other operand is converted to 
, the operation is performed using at least 
 range and precision, and the type of the result is 
 (or 
 for the relational operators).
Otherwise, the operation is performed using at least 
 range and precision, and the type of the result is 
 (or 
 for the relational operators).
The floating-point operators, including the assignment operators, never produce exceptions. Instead, in exceptional situations, floating-point operations produce zero, infinity, or NaN, as described below:
If the result of a floating-point operation is too small for the destination format, the result of the operation becomes positive zero or negative zero.
If the result of a floating-point operation is too large for the destination format, the result of the operation becomes positive infinity or negative infinity.
If a floating-point operation is invalid, the result of the operation becomes NaN.
If one or both operands of a floating-point operation is NaN, the result of the operation becomes NaN.
Floating-point operations may be performed with higher precision than the result type of the operation. For example, some hardware architectures support an ""extended"" or ""long double"" floating-point type with greater range and precision than the 
 type, and implicitly perform all floating-point operations using this higher precision type. Only at excessive cost in performance can such hardware architectures be made to perform floating-point operations with less precision, and rather than require an implementation to forfeit both performance and precision, C# allows a higher precision type to be used for all floating-point operations. Other than delivering more precise results, this rarely has any measurable effects. However, in expressions of the form 
, where the multiplication produces a result that is outside the 
 range, but the subsequent division brings the temporary result back into the 
 range, the fact that the expression is evaluated in a higher range format may cause a finite result to be produced instead of an infinity.
The decimal type
The 
 type is a 128-bit data type suitable for financial and monetary calculations. The 
 type can represent values ranging from 
 to approximately 
 with 28-29 significant digits.
The finite set of values of type 
 are of the form 
, where the sign 
 is 0 or 1, the coefficient 
 is given by 
, and the scale 
 is such that 
.The 
 type does not support signed zeros, infinities, or NaN's. A 
 is represented as a 96-bit integer scaled by a power of ten. For 
s with an absolute value less than 
, the value is exact to the 28th decimal place, but no further. For 
s with an absolute value greater than or equal to 
, the value is exact to 28 or 29 digits. Contrary to the 
 and 
 data types, decimal fractional numbers such as 0.1 can be represented exactly in the 
 representation. In the 
 and 
 representations, such numbers are often infinite fractions, making those representations more prone to round-off errors.
If one of the operands of a binary operator is of type 
, then the other operand must be of an integral type or of type 
. If an integral type operand is present, it is converted to 
 before the operation is performed.
The result of an operation on values of type 
 is that which would result from calculating an exact result (preserving scale, as defined for each operator) and then rounding to fit the representation. Results are rounded to the nearest representable value, and, when a result is equally close to two representable values, to the value that has an even number in the least significant digit position (this is known as ""banker's rounding""). A zero result always has a sign of 0 and a scale of 0.
If a decimal arithmetic operation produces a value less than or equal to 
 in absolute value, the result of the operation becomes zero. If a 
 arithmetic operation produces a result that is too large for the 
 format, a 
 is thrown.
The 
 type has greater precision but smaller range than the floating-point types. Thus, conversions from the floating-point types to 
 might produce overflow exceptions, and conversions from 
 to the floating-point types might cause loss of precision. For these reasons, no implicit conversions exist between the floating-point types and 
, and without explicit casts, it is not possible to mix floating-point and 
 operands in the same expression.
The bool type
The 
 type represents boolean logical quantities. The possible values of type 
 are 
 and 
.
No standard conversions exist between 
 and other types. In particular, the 
 type is distinct and separate from the integral types, and a 
 value cannot be used in place of an integral value, and vice versa.
In the C and C++ languages, a zero integral or floating-point value, or a null pointer can be converted to the boolean value 
, and a non-zero integral or floating-point value, or a non-null pointer can be converted to the boolean value 
. In C#, such conversions are accomplished by explicitly comparing an integral or floating-point value to zero, or by explicitly comparing an object reference to 
.
Enumeration types
An enumeration type is a distinct type with named constants. Every enumeration type has an underlying type, which must be 
, 
, 
, 
, 
, 
, 
 or 
. The set of values of the enumeration type is the same as the set of values of the underlying type. Values of the enumeration type are not restricted to the values of the named constants. Enumeration types are defined through enumeration declarations (
).
Nullable types
A nullable type can represent all values of its 
 plus an additional null value. A nullable type is written 
, where 
 is the underlying type. This syntax is shorthand for 
, and the two forms can be used interchangeably.
A 
 conversely is any value type other than 
 and its shorthand 
 (for any 
), plus any type parameter that is constrained to be a non-nullable value type (that is, any type parameter with a 
 constraint). The 
 type specifies the value type constraint for 
 (
), which means that the underlying type of a nullable type can be any non-nullable value type. The underlying type of a nullable type cannot be a nullable type or a reference type. For example, 
 and 
 are invalid types.
An instance of a nullable type 
 has two public read-only properties:
A 
 property of type 
A 
 property of type 
An instance for which 
 is true is said to be non-null. A non-null instance contains a known value and 
 returns that value.
An instance for which 
 is false is said to be null. A null instance has an undefined value. Attempting to read the 
 of a null instance causes a 
 to be thrown. The process of accessing the 
 property of a nullable instance is referred to as 
.
In addition to the default constructor, every nullable type 
 has a public constructor that takes a single argument of type 
. Given a value 
 of type 
, a constructor invocation of the form
creates a non-null instance of 
 for which the 
 property is 
. The process of creating a non-null instance of a nullable type for a given value is referred to as 
.
Implicit conversions are available from the 
 literal to 
 (
) and from 
 to 
 (
).
Reference types
A reference type is a class type, an interface type, an array type, or a delegate type.
A reference type value is a reference to an 
 of the type, the latter known as an 
. The special value 
 is compatible with all reference types and indicates the absence of an instance.
Class types
A class type defines a data structure that contains data members (constants and fields), function members (methods, properties, events, indexers, operators, instance constructors, destructors and static constructors), and nested types. Class types support inheritance, a mechanism whereby derived classes can extend and specialize base classes. Instances of class types are created using 
s (
).
Class types are described in 
.
Certain predefined class types have special meaning in the C# language, as described in the table below.
Class type
Description
The ultimate base class of all other types. See 
.
The string type of the C# language. See 
.
The base class of all value types. See 
.
The base class of all enum types. See 
.
The base class of all array types. See 
.
The base class of all delegate types. See 
.
The base class of all exception types. See 
.
The object type
The 
 class type is the ultimate base class of all other types. Every type in C# directly or indirectly derives from the 
 class type.
The keyword 
 is simply an alias for the predefined class 
.
The dynamic type
The 
 type, like 
, can reference any object. When operators are applied to expressions of type 
, their resolution is deferred until the program is run. Thus, if the operator cannot legally be applied to the referenced object, no error is given during compilation. Instead an exception will be thrown when resolution of the operator fails at run-time.
Its purpose is to allow dynamic binding, which is described in detail in 
.
 is considered identical to 
 except in the following respects:
Operations on expressions of type 
 can be dynamically bound (
).
Type inference (
) will prefer 
 over 
 if both are candidates.
Because of this equivalence, the following holds:
There is an implicit identity conversion between 
 and 
, and between constructed types that are the same when replacing 
 with 
Implicit and explicit conversions to and from 
 also apply to and from 
.
Method signatures that are the same when replacing 
 with 
 are considered the same signature
The type 
 is indistinguishable from 
 at run-time.
An expression of the type 
 is referred to as a 
.
The string type
The 
 type is a sealed class type that inherits directly from 
. Instances of the 
 class represent Unicode character strings.
Values of the 
 type can be written as string literals (
).
The keyword 
 is simply an alias for the predefined class 
.
Interface types
An interface defines a contract. A class or struct that implements an interface must adhere to its contract. An interface may inherit from multiple base interfaces, and a class or struct may implement multiple interfaces.
Interface types are described in 
.
Array types
An array is a data structure that contains zero or more variables which are accessed through computed indices. The variables contained in an array, also called the elements of the array, are all of the same type, and this type is called the element type of the array.
Array types are described in 
.
Delegate types
A delegate is a data structure that refers to one or more methods. For instance methods, it also refers to their corresponding object instances.
The closest equivalent of a delegate in C or C++ is a function pointer, but whereas a function pointer can only reference static functions, a delegate can reference both static and instance methods. In the latter case, the delegate stores not only a reference to the method's entry point, but also a reference to the object instance on which to invoke the method.
Delegate types are described in 
.
Boxing and unboxing
The concept of boxing and unboxing is central to C#'s type system. It provides a bridge between 
s and 
s by permitting any value of a 
 to be converted to and from type 
. Boxing and unboxing enables a unified view of the type system wherein a value of any type can ultimately be treated as an object.
Boxing conversions
A boxing conversion permits a 
 to be implicitly converted to a 
. The following boxing conversions exist:
From any 
 to the type 
.
From any 
 to the type 
.
From any 
 to any 
 implemented by the 
.
From any 
 to any 
 implemented by the underlying type of the 
.
From any 
 to the type 
.
From any 
 with an underlying 
 to the type 
.
Note that an implicit conversion from a type parameter will be executed as a boxing conversion if at run-time it ends up converting from a value type to a reference type (
).
Boxing a value of a 
 consists of allocating an object instance and copying the 
 value into that instance.
Boxing a value of a 
 produces a null reference if it is the 
 value (
 is 
), or the result of unwrapping and boxing the underlying value otherwise.
The actual process of boxing a value of a 
 is best explained by imagining the existence of a generic 
, which behaves as if it were declared as follows:
Boxing of a value 
 of type 
 now consists of executing the expression 
, and returning the resulting instance as a value of type 
. Thus, the statements
conceptually correspond to
A boxing class like 
 above doesn't actually exist and the dynamic type of a boxed value isn't actually a class type. Instead, a boxed value of type 
 has the dynamic type 
, and a dynamic type check using the 
 operator can simply reference type 
. For example,
will output the string ""
"" on the console.
A boxing conversion implies making a copy of the value being boxed. This is different from a conversion of a 
 to type 
, in which the value continues to reference the same instance and simply is regarded as the less derived type 
. For example, given the declaration
the following statements
will output the value 10 on the console because the implicit boxing operation that occurs in the assignment of 
 to 
 causes the value of 
 to be copied. Had 
 been declared a 
 instead, the value 20 would be output because 
 and 
 would reference the same instance.
Unboxing conversions
An unboxing conversion permits a 
 to be explicitly converted to a 
. The following unboxing conversions exist:
From the type 
 to any 
.
From the type 
 to any 
.
From any 
 to any 
 that implements the 
.
From any 
 to any 
 whose underlying type implements the 
.
From the type 
 to any 
.
From the type 
 to any 
 with an underlying 
.
Note that an explicit conversion to a type parameter will be executed as an unboxing conversion if at run-time it ends up converting from a reference type to a value type (
).
An unboxing operation to a 
 consists of first checking that the object instance is a boxed value of the given 
, and then copying the value out of the instance.
Unboxing to a 
 produces the null value of the 
 if the source operand is 
, or the wrapped result of unboxing the object instance to the underlying type of the 
 otherwise.
Referring to the imaginary boxing class described in the previous section, an unboxing conversion of an object 
 to a 
 
 consists of executing the expression 
. Thus, the statements
conceptually correspond to
For an unboxing conversion to a given 
 to succeed at run-time, the value of the source operand must be a reference to a boxed value of that 
. If the source operand is 
, a 
 is thrown. If the source operand is a reference to an incompatible object, a 
 is thrown.
For an unboxing conversion to a given 
 to succeed at run-time, the value of the source operand must be either 
 or a reference to a boxed value of the underlying 
 of the 
. If the source operand is a reference to an incompatible object, a 
 is thrown.
Constructed types
A generic type declaration, by itself, denotes an 
 that is used as a ""blueprint"" to form many different types, by way of applying 
. The type arguments are written within angle brackets (
 and 
) immediately following the name of the generic type. A type that includes at least one type argument is called a 
. A constructed type can be used in most places in the language in which a type name can appear. An unbound generic type can only be used within a 
 (
).
Constructed types can also be used in expressions as simple names (
) or when accessing a member (
).
When a 
 is evaluated, only generic types with the correct number of type parameters are considered. Thus, it is possible to use the same identifier to identify different types, as long as the types have different numbers of type parameters. This is useful when mixing generic and non-generic classes in the same program:
A 
 might identify a constructed type even though it doesn't specify type parameters directly. This can occur where a type is nested within a generic class declaration, and the instance type of the containing declaration is implicitly used for name lookup (
):
In unsafe code, a constructed type cannot be used as an 
 (
).
Type arguments
Each argument in a type argument list is simply a 
.
In unsafe code (
), a 
 may not be a pointer type. Each type argument must satisfy any constraints on the corresponding type parameter (
).
Open and closed types
All types can be classified as either 
 or 
. An open type is a type that involves type parameters. More specifically:
A type parameter defines an open type.
An array type is an open type if and only if its element type is an open type.
A constructed type is an open type if and only if one or more of its type arguments is an open type. A constructed nested type is an open type if and only if one or more of its type arguments or the type arguments of its containing type(s) is an open type.
A closed type is a type that is not an open type.
At run-time, all of the code within a generic type declaration is executed in the context of a closed constructed type that was created by applying type arguments to the generic declaration. Each type parameter within the generic type is bound to a particular run-time type. The run-time processing of all statements and expressions always occurs with closed types, and open types occur only during compile-time processing.
Each closed constructed type has its own set of static variables, which are not shared with any other closed constructed types. Since an open type does not exist at run-time, there are no static variables associated with an open type. Two closed constructed types are the same type if they are constructed from the same unbound generic type, and their corresponding type arguments are the same type.
Bound and unbound types
The term 
 refers to a non-generic type or an unbound generic type. The term 
 refers to a non-generic type or a constructed type.
An unbound type refers to the entity declared by a type declaration. An unbound generic type is not itself a type, and cannot be used as the type of a variable, argument or return value, or as a base type. The only construct in which an unbound generic type can be referenced is the 
 expression (
).
Satisfying constraints
Whenever a constructed type or generic method is referenced, the supplied type arguments are checked against the type parameter constraints declared on the generic type or method (
). For each 
 clause, the type argument 
 that corresponds to the named type parameter is checked against each constraint as follows:
If the constraint is a class type, an interface type, or a type parameter, let 
 represent that constraint with the supplied type arguments substituted for any type parameters that appear in the constraint. To satisfy the constraint, it must be the case that type 
 is convertible to type 
 by one of the following:
An identity conversion (
)
An implicit reference conversion (
)
A boxing conversion (
), provided that type A is a non-nullable value type.
An implicit reference, boxing or type parameter conversion from a type parameter 
 to 
.
If the constraint is the reference type constraint (
), the type 
 must satisfy one of the following:
 is an interface type, class type, delegate type or array type. Note that 
 and 
 are reference types that satisfy this constraint.
 is a type parameter that is known to be a reference type (
).
If the constraint is the value type constraint (
), the type 
 must satisfy one of the following:
 is a struct type or enum type, but not a nullable type. Note that 
 and 
 are reference types that do not satisfy this constraint.
 is a type parameter having the value type constraint (
).
If the constraint is the constructor constraint 
, the type 
 must not be 
 and must have a public parameterless constructor. This is satisfied if one of the following is true:
 is a value type, since all value types have a public default constructor (
).
 is a type parameter having the constructor constraint (
).
 is a type parameter having the value type constraint (
).
 is a class that is not 
 and contains an explicitly declared 
 constructor with no parameters.
 is not 
 and has a default constructor (
).
A compile-time error occurs if one or more of a type parameter's constraints are not satisfied by the given type arguments.
Since type parameters are not inherited, constraints are never inherited either. In the example below, 
 needs to specify the constraint on its type parameter 
 so that 
 satisfies the constraint imposed by the base class 
. In contrast, class 
 need not specify a constraint, because 
 implements 
 for any 
.
Type parameters
A type parameter is an identifier designating a value type or reference type that the parameter is bound to at run-time.
Since a type parameter can be instantiated with many different actual type arguments, type parameters have slightly different operations and restrictions than other types. These include:
A type parameter cannot be used directly to declare a base class (
) or interface (
).
The rules for member lookup on type parameters depend on the constraints, if any, applied to the type parameter. They are detailed in 
.
The available conversions for a type parameter depend on the constraints, if any, applied to the type parameter. They are detailed in 
 and 
.
The literal 
 cannot be converted to a type given by a type parameter, except if the type parameter is known to be a reference type (
). However, a 
 expression (
) can be used instead. In addition, a value with a type given by a type parameter can be compared with 
 using 
 and 
 (
) unless the type parameter has the value type constraint.
A 
 expression (
) can only be used with a type parameter if the type parameter is constrained by a 
 or the value type constraint (
).
A type parameter cannot be used anywhere within an attribute.
A type parameter cannot be used in a member access (
) or type name (
) to identify a static member or a nested type.
In unsafe code, a type parameter cannot be used as an 
 (
).
As a type, type parameters are purely a compile-time construct. At run-time, each type parameter is bound to a run-time type that was specified by supplying a type argument to the generic type declaration. Thus, the type of a variable declared with a type parameter will, at run-time, be a closed constructed type (
). The run-time execution of all statements and expressions involving type parameters uses the actual type that was supplied as the type argument for that parameter.
Expression tree types
 permit lambda expressions to be represented as data structures instead of executable code. Expression trees are values of 
 of the form 
, where 
 is any delegate type. For the remainder of this specification we will refer to these types using the shorthand 
.
If a conversion exists from a lambda expression to a delegate type 
, a conversion also exists to the expression tree type 
. Whereas the conversion of a lambda expression to a delegate type generates a delegate that references executable code for the lambda expression, conversion to an expression tree type creates an expression tree representation of the lambda expression.
Expression trees are efficient in-memory data representations of lambda expressionsand make the structure of the lambda expressiontransparent and explicit.
Just like a delegate type 
, 
 is said to have parameter and return types, which are the same as those of 
.
The following example represents a lambda expressionboth as executable code and as an expression tree. Because a conversion exists to 
, a conversion also exists to 
:
Following these assignments, the delegate 
 references a method that returns 
, and the expression tree 
 references a data structure that describes the expression 
.
The exact definition of the generic type 
 as well as the precise rules for constructing an expression tree when a lambda expressionis converted to an expression tree type, are both outside the scope of this specification.
Two things are important to make explicit:
Not all lambda expressions can be converted to expression trees. For instance, lambda expressions with statement bodies, and lambda expressions containing assignment expressions cannot be represented. In these cases, a conversion still exists, but will fail at compile-time. These exceptions are detailed in 
.
 offers an instance method 
 which produces a delegate of type 
:
Invoking this delegate causes the code represented by the expression tree to be executed. Thus, given the definitions above, del and del2 are equivalent, and the following two statements will have the same effect:
After executing this code,  
 and 
 will both have the value 
.
Variables
Variables represent storage locations. Every variable has a type that determines what values can be stored in the variable. C# is a type-safe language, and the C# compiler guarantees that values stored in variables are always of the appropriate type. The value of a variable can be changed through assignment or through use of the 
 and 
 operators.
A variable must be 
 (
) before its value can be obtained.
As described in the following sections, variables are either 
 or 
. An initially assigned variable has a well-defined initial value and is always considered definitely assigned. An initially unassigned variable has no initial value. For an initially unassigned variable to be considered definitely assigned at a certain location, an assignment to the variable must occur in every possible execution path leading to that location.
Variable categories
C# defines seven categories of variables: static variables, instance variables, array elements, value parameters, reference parameters, output parameters, and local variables. The sections that follow describe each of these categories.
In the example
 is a static variable, 
 is an instance variable, 
 is an array element, 
 is a value parameter, 
 is a reference parameter, 
 is an output parameter, and 
 is a local variable.
Static variables
A field declared with the 
 modifier is called a 
. A static variable comes into existence before execution of the static constructor (
) for its containing type, and ceases to exist when the associated application domain ceases to exist.
The initial value of a static variable is the default value (
) of the variable's type.
For purposes of definite assignment checking, a static variable is considered initially assigned.
Instance variables
A field declared without the 
 modifier is called an 
.
Instance variables in classes
An instance variable of a class comes into existence when a new instance of that class is created, and ceases to exist when there are no references to that instance and the instance's destructor (if any) has executed.
The initial value of an instance variable of a class is the default value (
) of the variable's type.
For the purpose of definite assignment checking, an instance variable of a class is considered initially assigned.
Instance variables in structs
An instance variable of a struct has exactly the same lifetime as the struct variable to which it belongs. In other words, when a variable of a struct type comes into existence or ceases to exist, so too do the instance variables of the struct.
The initial assignment state of an instance variable of a struct is the same as that of the containing struct variable. In other words, when a struct variable is considered initially assigned, so too are its instance variables, and when a struct variable is considered initially unassigned, its instance variables are likewise unassigned.
Array elements
The elements of an array come into existence when an array instance is created, and cease to exist when there are no references to that array instance.
The initial value of each of the elements of an array is the default value (
) of the type of the array elements.
For the purpose of definite assignment checking, an array element is considered initially assigned.
Value parameters
A parameter declared without a 
 or 
 modifier is a 
.
A value parameter comes into existence upon invocation of the function member (method, instance constructor, accessor, or operator) or anonymous function to which the parameter belongs, and is initialized with the value of the argument given in the invocation. A value parameter normally ceases to exist upon return of the function member or anonymous function. However, if the value parameter is captured by an anonymous function (
), its life time extends at least until the delegate or expression tree created from that anonymous function is eligible for garbage collection.
For the purpose of definite assignment checking, a value parameter is considered initially assigned.
Reference parameters
A parameter declared with a 
 modifier is a 
.
A reference parameter does not create a new storage location. Instead, a reference parameter represents the same storage location as the variable given as the argument in the function member or anonymous function invocation. Thus, the value of a reference parameter is always the same as the underlying variable.
The following definite assignment rules apply to reference parameters. Note the different rules for output parameters described in 
.
A variable must be definitely assigned (
) before it can be passed as a reference parameter in a function member or delegate invocation.
Within a function member or anonymous function, a reference parameter is considered initially assigned.
Within an instance method or instance accessor of a struct type, the 
 keyword behaves exactly as a reference parameter of the struct type (
).
Output parameters
A parameter declared with an 
 modifier is an 
.
An output parameter does not create a new storage location. Instead, an output parameter represents the same storage location as the variable given as the argument in the function member or delegate invocation. Thus, the value of an output parameter is always the same as the underlying variable.
The following definite assignment rules apply to output parameters. Note the different rules for reference parameters described in 
.
A variable need not be definitely assigned before it can be passed as an output parameter in a function member or delegate invocation.
Following the normal completion of a function member or delegate invocation, each variable that was passed as an output parameter is considered assigned in that execution path.
Within a function member or anonymous function, an output parameter is considered initially unassigned.
Every output parameter of a function member or anonymous function must be definitely assigned (
) before the function member or anonymous function returns normally.
Within an instance constructor of a struct type, the 
 keyword behaves exactly as an output parameter of the struct type (
).
Local variables
A 
 is declared by a 
, which may occur in a 
, a 
, a 
 or a 
; or by a 
 or a 
specific_catch_clause
 for a 
.
The lifetime of a local variable is the portion of program execution during which storage is guaranteed to be reserved for it. This lifetime extends at least from entry into the 
, 
, 
, 
, 
, or 
specific_catch_clause
 with which it is associated, until execution of that 
, 
, 
, 
, 
, or 
specific_catch_clause
 ends in any way. (Entering an enclosed 
 or calling a method suspends, but does not end, execution of the current 
, 
, 
, 
, 
, or 
specific_catch_clause
.) If the local variable is captured by an anonymous function (
), its lifetime extends at least until the delegate or expression tree created from the anonymous function, along with any other objects that come to reference the captured variable, are eligible for garbage collection.
If the parent 
, 
, 
, 
, 
, or 
specific_catch_clause
 is entered recursively, a new instance of the local variable is created each time, and its 
, if any, is evaluated each time.
A local variable introduced by a 
 is not automatically initialized and thus has no default value. For the purpose of definite assignment checking, a local variable introduced by a 
 is considered initially unassigned. A 
 may include a 
, in which case the variable is considered definitely assigned only after the initializing expression (
).
Within the scope of a local variableintroduced by a 
, it is a compile-time error to refer to that local variable in a textual position that precedes its 
. If the local variable declaration is implicit (
), it is also an error to refer to the variable within its 
.
A local variable introduced by a 
 or a 
specific_catch_clause
 is considered definitely assigned in its entire scope.
The actual lifetime of a local variable is implementation-dependent. For example, a compiler might statically determine that a local variable in a block is only used for a small portion of that block. Using this analysis, the compiler could generate code that results in the variable's storage having a shorter lifetime than its containing block.
The storage referred to by a local reference variable is reclaimed independently of the lifetime of that local reference variable (
).
Default values
The following categories of variables are automatically initialized to their default values:
Static variables.
Instance variables of class instances.
Array elements.
The default value of a variable depends on the type of the variable and is determined as follows:
For a variable of a 
, the default value is the same as the value computed by the 
's default constructor (
).
For a variable of a 
, the default value is 
.
Initialization to default values is typically done by having the memory manager or garbage collector initialize memory to all-bits-zero before it is allocated for use. For this reason, it is convenient to use all-bits-zero to represent the null reference.
Definite assignment
At a given location in the executable code of a function member, a variable is said to be 
 if the compiler can prove, by a particular static flow analysis (
), that the variable has been automatically initialized or has been the target of at least one assignment. Informally stated, the rules of definite assignment are:
An initially assigned variable (
) is always considered definitely assigned.
An initially unassigned variable (
) is considered definitely assigned at a given location if all possible execution paths leading to that location contain at least one of the following:
A simple assignment (
) in which the variable is the left operand.
An invocation expression (
) or object creation expression (
) that passes the variable as an output parameter.
For a local variable, a local variable declaration (
) that includes a variable initializer.
The formal specification underlying the above informal rules is described in 
, 
, and 
.
The definite assignment states of instance variables of a 
 variable are tracked individually as well as collectively. In additional to the rules above, the following rules apply to 
 variables and their instance variables:
An instance variable is considered definitely assigned if its containing 
 variable is considered definitely assigned.
A 
 variable is considered definitely assigned if each of its instance variables is considered definitely assigned.
Definite assignment is a requirement in the following contexts:
A variable must be definitely assigned at each location where its value is obtained. This ensures that undefined values never occur. The occurrence of a variable in an expression is considered to obtain the value of the variable, except when
the variable is the left operand of a simple assignment,
the variable is passed as an output parameter, or
the variable is a 
 variable and occurs as the left operand of a member access.
A variable must be definitely assigned at each location where it is passed as a reference parameter. This ensures that the function member being invoked can consider the reference parameter initially assigned.
All output parameters of a function member must be definitely assigned at each location where the function member returns (through a 
 statement or through execution reaching the end of the function member body). This ensures that function members do not return undefined values in output parameters, thus enabling the compiler to consider a function member invocation that takes a variable as an output parameter equivalent to an assignment to the variable.
The 
 variable of a 
 instance constructor must be definitely assigned at each location where that instance constructor returns.
Initially assigned variables
The following categories of variables are classified as initially assigned:
Static variables.
Instance variables of class instances.
Instance variables of initially assigned struct variables.
Array elements.
Value parameters.
Reference parameters.
Variables declared in a 
 clause or a 
 statement.
Initially unassigned variables
The following categories of variables are classified as initially unassigned:
Instance variables of initially unassigned struct variables.
Output parameters, including the 
 variable of struct instance constructors.
Local variables, except those declared in a 
 clause or a 
 statement.
Precise rules for determining definite assignment
In order to determine that each used variable is definitely assigned, the compiler must use a process that is equivalent to the one described in this section.
The compiler processes the body of each function member that has one or more initially unassigned variables. For each initially unassigned variable 
v
, the compiler determines a 
 for 
v
 at each of the following points in the function member:
At the beginning of each statement
At the end point (
) of each statement
On each arc which transfers control to another statement or to the end point of a statement
At the beginning of each expression
At the end of each expression
The definite assignment state of 
v
 can be either:
Definitely assigned. This indicates that on all possible control flows to this point, 
v
 has been assigned a value.
Not definitely assigned. For the state of a variable at the end of an expression of type 
, the state of a variable that isn't definitely assigned may (but doesn't necessarily) fall into one of the following sub-states:
Definitely assigned after true expression. This state indicates that 
v
 is definitely assigned if the boolean expression evaluated as true, but is not necessarily assigned if the boolean expression evaluated as false.
Definitely assigned after false expression. This state indicates that 
v
 is definitely assigned if the boolean expression evaluated as false, but is not necessarily assigned if the boolean expression evaluated as true.
The following rules govern how the state of a variable 
v
 is determined at each location.
General rules for statements
v
 is not definitely assigned at the beginning of a function member body.
v
 is definitely assigned at the beginning of any unreachable statement.
The definite assignment state of 
v
 at the beginning of any other statement is determined by checking the definite assignment state of 
v
 on all control flow transfers that target the beginning of that statement. If (and only if) 
v
 is definitely assigned on all such control flow transfers, then 
v
 is definitely assigned at the beginning of the statement. The set of possible control flow transfers is determined in the same way as for checking statement reachability (
).
The definite assignment state of 
v
 at the end point of a block, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
 statement is determined by checking the definite assignment state of 
v
 on all control flow transfers that target the end point of that statement. If 
v
 is definitely assigned on all such control flow transfers, then 
v
 is definitely assigned at the end point of the statement. Otherwise; 
v
 is not definitely assigned at the end point of the statement. The set of possible control flow transfers is determined in the same way as for checking statement reachability (
).
Block statements, checked, and unchecked statements
The definite assignment state of 
v
 on the control transfer to the first statement of the statement list in the block (or to the end point of the block, if the statement list is empty) is the same as the definite assignment statement of 
v
 before the block, 
, or 
 statement.
Expression statements
For an expression statement 
stmt
 that consists of the expression 
expr
:
v
 has the same definite assignment state at the beginning of 
expr
 as at the beginning of 
stmt
.
If 
v
 if definitely assigned at the end of 
expr
, it is definitely assigned at the end point of 
stmt
; otherwise; it is not definitely assigned at the end point of 
stmt
.
Declaration statements
If 
stmt
 is a declaration statement without initializers, then 
v
 has the same definite assignment state at the end point of 
stmt
 as at the beginning of 
stmt
.
If 
stmt
 is a declaration statement with initializers, then the definite assignment state for 
v
 is determined as if 
stmt
 were a statement list, with one assignment statement for each declaration with an initializer (in the order of declaration).
If statements
For an 
 statement 
stmt
 of the form:
v
 has the same definite assignment state at the beginning of 
expr
 as at the beginning of 
stmt
.
If 
v
 is definitely assigned at the end of 
expr
, then it is definitely assigned on the control flow transfer to 
then_stmt
 and to either 
else_stmt
 or to the end-point of 
stmt
 if there is no else clause.
If 
v
 has the state ""definitely assigned after true expression"" at the end of 
expr
, then it is definitely assigned on the control flow transfer to 
then_stmt
, and not definitely assigned on the control flow transfer to either 
else_stmt
 or to the end-point of 
stmt
 if there is no else clause.
If 
v
 has the state ""definitely assigned after false expression"" at the end of 
expr
, then it is definitely assigned on the control flow transfer to 
else_stmt
, and not definitely assigned on the control flow transfer to 
then_stmt
. It is definitely assigned at the end-point of 
stmt
 if and only if it is definitely assigned at the end-point of 
then_stmt
.
Otherwise, 
v
 is considered not definitely assigned on the control flow transfer to either the 
then_stmt
 or 
else_stmt
, or to the end-point of 
stmt
 if there is no else clause.
Switch statements
In a 
 statement 
stmt
 with a controlling expression 
expr
:
The definite assignment state of 
v
 at the beginning of 
expr
 is the same as the state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 on the control flow transfer to a reachable switch block statement list is the same as the definite assignment state of 
v
 at the end of 
expr
.
While statements
For a 
 statement 
stmt
 of the form:
v
 has the same definite assignment state at the beginning of 
expr
 as at the beginning of 
stmt
.
If 
v
 is definitely assigned at the end of 
expr
, then it is definitely assigned on the control flow transfer to 
while_body
 and to the end point of 
stmt
.
If 
v
 has the state ""definitely assigned after true expression"" at the end of 
expr
, then it is definitely assigned on the control flow transfer to 
while_body
, but not definitely assigned at the end-point of 
stmt
.
If 
v
 has the state ""definitely assigned after false expression"" at the end of 
expr
, then it is definitely assigned on the control flow transfer to the end point of 
stmt
, but not definitely assigned on the control flow transfer to 
while_body
.
Do statements
For a 
 statement 
stmt
 of the form:
v
 has the same definite assignment state on the control flow transfer from the beginning of 
stmt
 to 
do_body
 as at the beginning of 
stmt
.
v
 has the same definite assignment state at the beginning of 
expr
 as at the end point of 
do_body
.
If 
v
 is definitely assigned at the end of 
expr
, then it is definitely assigned on the control flow transfer to the end point of 
stmt
.
If 
v
 has the state ""definitely assigned after false expression"" at the end of 
expr
, then it is definitely assigned on the control flow transfer to the end point of 
stmt
.
For statements
Definite assignment checking for a 
 statement of the form:
is done as if the statement were written:
If the 
 is omitted from the 
 statement, then evaluation of definite assignment proceeds as if 
 were replaced with 
 in the above expansion.
Break, continue, and goto statements
The definite assignment state of 
v
 on the control flow transfer caused by a 
, 
, or 
 statement is the same as the definite assignment state of 
v
 at the beginning of the statement.
Throw statements
For a statement 
stmt
 of the form
The definite assignment state of 
v
 at the beginning of 
expr
 is the same as the definite assignment state of 
v
 at the beginning of 
stmt
.
Return statements
For a statement 
stmt
 of the form
The definite assignment state of 
v
 at the beginning of 
expr
 is the same as the definite assignment state of 
v
 at the beginning of 
stmt
.
If 
v
 is an output parameter, then it must be definitely assigned either:
after 
expr
or at the end of the 
 block of a 
-
 or 
-
-
 that encloses the 
 statement.
For a statement stmt of the form:
If 
v
 is an output parameter, then it must be definitely assigned either:
before 
stmt
or at the end of the 
 block of a 
-
 or 
-
-
 that encloses the 
 statement.
Try-catch statements
For a statement 
stmt
 of the form:
The definite assignment state of 
v
 at the beginning of 
try_block
 is the same as the definite assignment state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 at the beginning of 
catch_block_i
 (for any 
i
) is the same as the definite assignment state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 at the end-point of 
stmt
 is definitely assigned if (and only if) 
v
 is definitely assigned at the end-point of 
try_block
 and every 
catch_block_i
 (for every 
i
 from 1 to 
n
).
Try-finally statements
For a 
 statement 
stmt
 of the form:
The definite assignment state of 
v
 at the beginning of 
try_block
 is the same as the definite assignment state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 at the beginning of 
finally_block
 is the same as the definite assignment state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 at the end-point of 
stmt
 is definitely assigned if (and only if) at least one of the following is true:
v
 is definitely assigned at the end-point of 
try_block
v
 is definitely assigned at the end-point of 
finally_block
If a control flow transfer (for example, a 
 statement) is made that begins within 
try_block
, and ends outside of 
try_block
, then 
v
 is also considered definitely assigned on that control flow transfer if 
v
 is definitely assigned at the end-point of 
finally_block
. (This is not an only if—if 
v
 is definitely assigned for another reason on this control flow transfer, then it is still considered definitely assigned.)
Try-catch-finally statements
Definite assignment analysis for a 
-
-
 statement of the form:
is done as if the statement were a 
-
 statement enclosing a 
-
 statement:
The following example demonstrates how the different blocks of a 
 statement (
) affect definite assignment.
Foreach statements
For a 
 statement 
stmt
 of the form:
The definite assignment state of 
v
 at the beginning of 
expr
 is the same as the state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 on the control flow transfer to 
 or to the end point of 
stmt
 is the same as the state of 
v
 at the end of 
expr
.
Using statements
For a 
 statement 
stmt
 of the form:
The definite assignment state of 
v
 at the beginning of 
 is the same as the state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 on the control flow transfer to 
 is the same as the state of 
v
 at the end of 
.
Lock statements
For a 
 statement 
stmt
 of the form:
The definite assignment state of 
v
 at the beginning of 
expr
 is the same as the state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 on the control flow transfer to 
 is the same as the state of 
v
 at the end of 
expr
.
Yield statements
For a 
 statement 
stmt
 of the form:
The definite assignment state of 
v
 at the beginning of 
expr
 is the same as the state of 
v
 at the beginning of 
stmt
.
The definite assignment state of 
v
 at the end of 
stmt
 is the same as the state of 
v
 at the end of 
expr
.
A 
 statement has no effect on the definite assignment state.
General rules for simple expressions
The following rule applies to these kinds of expressions: literals (
), simple names (
), member access expressions (
), non-indexed base access expressions (
), 
 expressions (
), default value expressions (
) and 
 expressions (
).
The definite assignment state of 
v
 at the end of such an expression is the same as the definite assignment state of 
v
 at the beginning of the expression.
General rules for expressions with embedded expressions
The following rules apply to these kinds of expressions: parenthesized expressions (
), element access expressions (
), base access expressions with indexing (
), increment and decrement expressions (
, 
), cast expressions (
), unary 
, 
, 
, 
 expressions, binary 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
 expressions (
, 
, 
, 
), compound assignment expressions (
), 
 and 
 expressions (
), plus array and delegate creation expressions (
).
Each of these expressions has one or more sub-expressions that are unconditionally evaluated in a fixed order. For example, the binary 
 operator evaluates the left hand side of the operator, then the right hand side. An indexing operation evaluates the indexed expression, and then evaluates each of the index expressions, in order from left to right. For an expression 
expr
, which has sub-expressions 
e1, e2, ..., eN
, evaluated in that order:
The definite assignment state of 
v
 at the beginning of 
e1
 is the same as the definite assignment state at the beginning of 
expr
.
The definite assignment state of 
v
 at the beginning of 
ei
 (
i
 greater than one) is the same as the definite assignment state at the end of the previous sub-expression.
The definite assignment state of 
v
 at the end of 
expr
 is the same as the definite assignment state at the end of 
eN
Invocation expressions and object creation expressions
For an invocation expression 
expr
 of the form:
or an object creation expression of the form:
For an invocation expression, the definite assignment state of 
v
 before 
 is the same as the state of 
v
 before 
expr
.
For an invocation expression, the definite assignment state of 
v
 before 
arg1
 is the same as the state of 
v
 after 
.
For an object creation expression, the definite assignment state of 
v
 before 
arg1
 is the same as the state of 
v
 before 
expr
.
For each argument 
argi
, the definite assignment state of 
v
 after 
argi
 is determined by the normal expression rules, ignoring any 
 or 
 modifiers.
For each argument 
argi
 for any 
i
 greater than one, the definite assignment state of 
v
 before 
argi
 is the same as the state of 
v
 after the previous 
arg
.
If the variable 
v
 is passed as an 
 argument (i.e., an argument of the form 
) in any of the arguments, then the state of 
v
 after 
expr
 is definitely assigned. Otherwise; the state of 
v
 after 
expr
 is the same as the state of 
v
 after 
argN
.
For array initializers (
), object initializers (
), collection initializers (
) and anonymous object initializers (
), the definite assignment state is determined by the expansion that these constructs are defined in terms of.
Simple assignment expressions
For an expression 
expr
 of the form 
:
The definite assignment state of 
v
 before 
expr_rhs
 is the same as the definite assignment state of 
v
 before 
expr
.
The definite assignment state of 
v
 after 
expr
 is determined by:
If 
w
 is the same variable as 
v
, then the definite assignment state of 
v
 after 
expr
 is definitely assigned.
Otherwise, if the assignment occurs within the instance constructor of a struct type, if 
w
 is a property access designating an automatically implemented property 
P
 on the instance being constructed and 
v
 is the hidden backing field of 
P
, then the definite assignment state of 
v
 after 
expr
 is definitely assigned.
Otherwise, the definite assignment state of 
v
 after 
expr
 is the same as the definite assignment state of 
v
 after 
expr_rhs
.
&& (conditional AND) expressions
For an expression 
expr
 of the form 
:
The definite assignment state of 
v
 before 
expr_first
 is the same as the definite assignment state of 
v
 before 
expr
.
The definite assignment state of 
v
 before 
expr_second
 is definitely assigned if the state of 
v
 after 
expr_first
 is either definitely assigned or ""definitely assigned after true expression"". Otherwise, it is not definitely assigned.
The definite assignment state of 
v
 after 
expr
 is determined by:
If 
expr_first
 is a constant expression with the value 
, then the definite assignment state of 
v
 after 
expr
 is the same as the definite assignment state of 
v
 after 
expr_first
.
Otherwise, if the state of 
v
 after 
expr_first
 is definitely assigned, then the state of 
v
 after 
expr
 is definitely assigned.
Otherwise, if the state of 
v
 after 
expr_second
 is definitely assigned, and the state of 
v
 after 
expr_first
 is ""definitely assigned after false expression"", then the state of 
v
 after 
expr
 is definitely assigned.
Otherwise, if the state of 
v
 after 
expr_second
 is definitely assigned or ""definitely assigned after true expression"", then the state of 
v
 after 
expr
 is ""definitely assigned after true expression"".
Otherwise, if the state of 
v
 after 
expr_first
 is ""definitely assigned after false expression"", and the state of 
v
 after 
expr_second
 is ""definitely assigned after false expression"", then the state of 
v
 after 
expr
 is ""definitely assigned after false expression"".
Otherwise, the state of 
v
 after 
expr
 is not definitely assigned.
In the example
the variable 
 is considered definitely assigned in one of the embedded statements of an 
 statement but not in the other. In the 
 statement in method 
, the variable 
 is definitely assigned in the first embedded statement because execution of the expression 
 always precedes execution of this embedded statement. In contrast, the variable 
 is not definitely assigned in the second embedded statement, since 
 might have tested false, resulting in the variable 
 being unassigned.
|| (conditional OR) expressions
For an expression 
expr
 of the form 
:
The definite assignment state of 
v
 before 
expr_first
 is the same as the definite assignment state of 
v
 before 
expr
.
The definite assignment state of 
v
 before 
expr_second
 is definitely assigned if the state of 
v
 after 
expr_first
 is either definitely assigned or ""definitely assigned after false expression"". Otherwise, it is not definitely assigned.
The definite assignment statement of 
v
 after 
expr
 is determined by:
If 
expr_first
 is a constant expression with the value 
, then the definite assignment state of 
v
 after 
expr
 is the same as the definite assignment state of 
v
 after 
expr_first
.
Otherwise, if the state of 
v
 after 
expr_first
 is definitely assigned, then the state of 
v
 after 
expr
 is definitely assigned.
Otherwise, if the state of 
v
 after 
expr_second
 is definitely assigned, and the state of 
v
 after 
expr_first
 is ""definitely assigned after true expression"", then the state of 
v
 after 
expr
 is definitely assigned.
Otherwise, if the state of 
v
 after 
expr_second
 is definitely assigned or ""definitely assigned after false expression"", then the state of 
v
 after 
expr
 is ""definitely assigned after false expression"".
Otherwise, if the state of 
v
 after 
expr_first
 is ""definitely assigned after true expression"", and the state of 
v
 after 
expr_second
 is ""definitely assigned after true expression"", then the state of 
v
 after 
expr
 is ""definitely assigned after true expression"".
Otherwise, the state of 
v
 after 
expr
 is not definitely assigned.
In the example
the variable 
 is considered definitely assigned in one of the embedded statements of an 
 statement but not in the other. In the 
 statement in method 
, the variable 
 is definitely assigned in the second embedded statement because execution of the expression 
 always precedes execution of this embedded statement. In contrast, the variable 
 is not definitely assigned in the first embedded statement, since 
 might have tested true, resulting in the variable 
 being unassigned.
! (logical negation) expressions
For an expression 
expr
 of the form 
:
The definite assignment state of 
v
 before 
expr_operand
 is the same as the definite assignment state of 
v
 before 
expr
.
The definite assignment state of 
v
 after 
expr
 is determined by:
If the state of 
v
 after 
expr_operand 
is definitely assigned, then the state of 
v
 after 
expr
 is definitely assigned.
If the state of 
v
 after 
expr_operand 
is not definitely assigned, then the state of 
v
 after 
expr
 is not definitely assigned.
If the state of 
v
 after 
expr_operand 
is ""definitely assigned after false expression"", then the state of 
v
 after 
expr
 is ""definitely assigned after true expression"".
If the state of 
v
 after 
expr_operand 
is ""definitely assigned after true expression"", then the state of 
v
 after 
expr
 is ""definitely assigned after false expression"".
?? (null coalescing) expressions
For an expression 
expr
 of the form 
:
The definite assignment state of 
v
 before 
expr_first
 is the same as the definite assignment state of 
v
 before 
expr
.
The definite assignment state of 
v
 before 
expr_second
 is the same as the definite assignment state of 
v
 after 
expr_first
.
The definite assignment statement of 
v
 after 
expr
 is determined by:
If 
expr_first
 is a constant expression (
) with value null, then the the state of 
v
 after 
expr
 is the same as the state of 
v
 after 
expr_second
.
Otherwise, the state of 
v
 after 
expr
 is the same as the definite assignment state of 
v
 after 
expr_first
.
?: (conditional) expressions
For an expression 
expr
 of the form 
:
The definite assignment state of 
v
 before 
expr_cond
 is the same as the state of 
v
 before 
expr
.
The definite assignment state of 
v
 before 
expr_true
 is definitely assigned if and only if one of the following holds:
expr_cond
 is a constant expression with the value 
the state of 
v
 after 
expr_cond
 is definitely assigned or ""definitely assigned after true expression"".
The definite assignment state of 
v
 before 
expr_false
 is definitely assigned if and only if one of the following holds:
expr_cond
 is a constant expression with the value 
the state of 
v
 after 
expr_cond
 is definitely assigned or ""definitely assigned after false expression"".
The definite assignment state of 
v
 after 
expr
 is determined by:
If 
expr_cond
 is a constant expression (
) with value 
 then the state of 
v
 after 
expr
 is the same as the state of 
v
 after 
expr_true
.
Otherwise, if 
expr_cond
 is a constant expression (
) with value 
 then the state of 
v
 after 
expr
 is the same as the state of 
v
 after 
expr_false
.
Otherwise, if the state of 
v
 after 
expr_true
 is definitely assigned and the state of 
v
 after 
expr_false
 is definitely assigned, then the state of 
v
 after 
expr
 is definitely assigned.
Otherwise, the state of 
v
 after 
expr
 is not definitely assigned.
Anonymous functions
For a 
 or 
 
expr
 with a body (either 
 or 
) 
body
:
The definite assignment state of an outer variable 
v
 before 
body
 is the same as the state of 
v
 before 
expr
. That is, definite assignment state of outer variables is inherited from the context of the anonymous function.
The definite assignment state of an outer variable 
v
 after 
expr
 is the same as the state of 
v
 before 
expr
.
The example
generates a compile-time error since 
 is not definitely assigned where the anonymous function is declared. The example
also generates a compile-time error since the assignment to 
 in the anonymous function has no affect on the definite assignment state of 
 outside the anonymous function.
Variable references
A 
 is an 
 that is classified as a variable. A 
 denotes a storage location that can be accessed both to fetch the current value and to store a new value.
In C and C++, a 
 is known as an 
lvalue
.
Atomicity of variable references
Reads and writes of the following data types are atomic: 
, 
, 
, 
, 
, 
, 
, 
, 
, and reference types. In addition, reads and writes of enum types with an underlying type in the previous list are also atomic. Reads and writes of other types, including 
, 
, 
, and 
, as well as user-defined types, are not guaranteed to be atomic. Aside from the library functions designed for that purpose, there is no guarantee of atomic read-modify-write, such as in the case of increment or decrement.
Conversions
A 
 enables an expression to be treated as being of a particular type. A conversion may cause an expression of a given type to be treated as having a different type, or it may cause an expression without a type to get a type. Conversions can be 
 or 
, and this determines whether an explicit cast is required. For instance, the conversion from type 
 to type 
 is implicit, so expressions of type 
 can implicitly be treated as type 
. The opposite conversion, from type 
 to type 
, is explicit and so an explicit cast is required.
Some conversions are defined by the language. Programs may also define their own conversions (
).
Implicit conversions
The following conversions are classified as implicit conversions:
Identity conversions
Implicit numeric conversions
Implicit enumeration conversions.
Implicit nullable conversions
Null literal conversions
Implicit reference conversions
Boxing conversions
Implicit dynamic conversions
Implicit constant expression conversions
User-defined implicit conversions
Anonymous function conversions
Method group conversions
Implicit conversions can occur in a variety of situations, including function member invocations (
), cast expressions (
), and assignments (
).
The pre-defined implicit conversions always succeed and never cause exceptions to be thrown. Properly designed user-defined implicit conversions should exhibit these characteristics as well.
For the purposes of conversion, the types 
 and 
 are considered equivalent.
However, dynamic conversions (
 and 
) apply only to expressions of type 
 (
).
Identity conversion
An identity conversion converts from any type to the same type. This conversion exists such that an entity that already has a required type can be said to be convertible to that type.
Because object and dynamic are considered equivalent there is an identity conversion between 
 and 
, and between constructed types that are the same when replacing all occurences of 
 with 
.
Implicit numeric conversions
The implicit numeric conversions are:
From 
 to 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, or 
.
From 
 to 
, 
, or 
.
From 
 to 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
.
Conversions from 
, 
, 
, or 
 to 
 and from 
 or 
 to 
 may cause a loss of precision, but will never cause a loss of magnitude. The other implicit numeric conversions never lose any information.
There are no implicit conversions to the 
 type, so values of the other integral types do not automatically convert to the 
 type.
Implicit enumeration conversions
An implicit enumeration conversion permits the 
 
 to be converted to any 
 and to any 
 whose underlying type is an 
. In the latter case the conversion is evaluated by converting to the underlying 
 and wrapping the result (
).
Implicit interpolated string conversions
An implicit interpolated string conversion permits an 
 (
) to be converted to 
 or 
 (which implements 
).
When this conversion is applied a string value is not composed from the interpolated string. Instead an instance of 
 is created, as further described in 
.
Implicit nullable conversions
Predefined implicit conversions that operate on non-nullable value types can also be used with nullable forms of those types. For each of the predefined implicit identity and numeric conversions that convert from a non-nullable value type 
 to a non-nullable value type 
, the following implicit nullable conversions exist:
An implicit conversion from 
 to 
.
An implicit conversion from 
 to 
.
Evaluation of an implicit nullable conversion based on an underlying conversion from 
 to 
 proceeds as follows:
If the nullable conversion is from 
 to 
:
If the source value is null (
 property is false), the result is the null value of type 
.
Otherwise, the conversion is evaluated as an unwrapping from 
 to 
, followed by the underlying conversion from 
 to 
, followed by a wrapping (
) from 
 to 
.
If the nullable conversion is from 
 to 
, the conversion is evaluated as the underlying conversion from 
 to 
 followed by a wrapping from 
 to 
.
Null literal conversions
An implicit conversion exists from the 
 literal to any nullable type. This conversion produces the null value (
) of the given nullable type.
Implicit reference conversions
The implicit reference conversions are:
From any 
 to 
 and 
.
From any 
 
 to any 
 
, provided 
 is derived from 
.
From any 
 
 to any 
 
, provided 
 implements 
.
From any 
 
 to any 
 
, provided 
 is derived from 
.
From an 
 
 with an element type 
 to an 
 
 with an element type 
, provided all of the following are true:
 and 
 differ only in element type. In other words, 
 and 
 have the same number of dimensions.
Both 
 and 
 are 
s.
An implicit reference conversion exists from 
 to 
.
From any 
 to 
 and the interfaces it implements.
From a single-dimensional array type 
 to 
 and its base interfaces, provided that there is an implicit identity or reference conversion from 
 to 
.
From any 
 to 
 and the interfaces it implements.
From the null literal to any 
.
From any 
 to a 
 
 if it has an implicit identity or reference conversion to a 
 
 and 
 has an identity conversion to 
.
From any 
 to an interface or delegate type 
 if it has an implicit identity or reference conversion to an interface or delegate type 
 and 
 is variance-convertible (
) to 
.
Implicit conversions involving type parameters that are known to be reference types. See 
 for more details on implicit conversions involving type parameters.
The implicit reference conversions are those conversions between 
s that can be proven to always succeed, and therefore require no checks at run-time.
Reference conversions, implicit or explicit, never change the referential identity of the object being converted. In other words, while a reference conversion may change the type of the reference, it never changes the type or value of the object being referred to.
Boxing conversions
A boxing conversion permits a 
 to be implicitly converted to a reference type. A boxing conversion exists from any 
 to 
 and 
, to 
 and to any 
 implemented by the 
. Furthermore an 
 can be converted to the type 
.
A boxing conversion exists from a 
 to a reference type, if and only if a boxing conversion exists from the underlying 
 to the reference type.
A value type has a boxing conversion to an interface type 
 if it has a boxing conversion to an interface type 
 and 
 has an identity conversion to 
.
A value type has a boxing conversion to an interface type 
 if it has a boxing conversion to an interface or delegate type 
 and 
 is variance-convertible (
) to 
.
Boxing a value of a 
 consists of allocating an object instance and copying the 
 value into that instance. A struct can be boxed to the type 
, since that is a base class for all structs (
).
Boxing a value of a 
 proceeds as follows:
If the source value is null (
 property is false), the result is a null reference of the target type.
Otherwise, the result is a reference to a boxed 
 produced by unwrapping and boxing the source value.
Boxing conversions are described further in 
.
Implicit dynamic conversions
An implicit dynamic conversion exists from an expression of type 
 to any type 
. The conversion is dynamically bound (
), which means that an implicit conversion will be sought at run-time from the run-time type of the expression to 
. If no conversion is found, a run-time exception is thrown.
Note that this implicit conversion seemingly violates the advice in the beginning of 
 that an implicit conversion should never cause an exception. However it is not the conversion itself, but the 
finding
 of the conversion that causes the exception. The risk of run-time exceptions is inherent in the use of dynamic binding. If dynamic binding of the conversion is not desired, the expression can be first converted to 
, and then to the desired type.
The following example illustrates implicit dynamic conversions:
The assignments to 
 and 
 both employ implicit dynamic conversions, where the binding of the operations is suspended until run-time. At run-time, implicit conversions are sought from the run-time type of 
 -- 
 -- to the target type. A conversion is found to 
 but not to 
.
Implicit constant expression conversions
An implicit constant expression conversion permits the following conversions:
A 
 (
) of type 
 can be converted to type 
, 
, 
, 
, 
, or 
, provided the value of the 
 is within the range of the destination type.
A 
 of type 
 can be converted to type 
, provided the value of the 
 is not negative.
Implicit conversions involving type parameters
The following implicit conversions exist for a given type parameter 
:
From 
 to its effective base class 
, from 
 to any base class of 
, and from 
 to any interface implemented by 
. At run-time, if 
 is a value type, the conversion is executed as a boxing conversion. Otherwise, the conversion is executed as an implicit reference conversion or identity conversion.
From 
 to an interface type 
 in 
's effective interface set and from 
 to any base interface of 
. At run-time, if 
 is a value type, the conversion is executed as a boxing conversion. Otherwise, the conversion is executed as an implicit reference conversion or identity conversion.
From 
 to a type parameter 
, provided 
 depends on 
 (
). At run-time, if 
 is a value type, then 
 and 
 are necessarily the same type and no conversion is performed. Otherwise, if 
 is a value type, the conversion is executed as a boxing conversion. Otherwise, the conversion is executed as an implicit reference conversion or identity conversion.
From the null literal to 
, provided 
 is known to be a reference type.
From 
 to a reference type 
 if it has an implicit conversion to a reference type 
 and 
 has an identity conversion to 
. At run-time the conversion is executed the same way as the conversion to 
.
From 
 to an interface type 
 if it has an implicit conversion to an interface or delegate type 
 and 
 is variance-convertible to 
 (
). At run-time, if 
 is a value type, the conversion is executed as a boxing conversion. Otherwise, the conversion is executed as an implicit reference conversion or identity conversion.
If 
 is known to be a reference type (
), the conversions above are all classified as implicit reference conversions (
). If 
 is not known to be a reference type, the conversions above are classified as boxing conversions (
).
User-defined implicit conversions
A user-defined implicit conversion consists of an optional standard implicit conversion, followed by execution of a user-defined implicit conversion operator, followed by another optional standard implicit conversion. The exact rules for evaluating user-defined implicit conversions are described in 
.
Anonymous function conversions and method group conversions
Anonymous functions and method groups do not have types in and of themselves, but may be implicitly converted to delegate types or expression tree types. Anonymous function conversions are described in more detail in 
 and method group conversions in 
.
Explicit conversions
The following conversions are classified as explicit conversions:
All implicit conversions.
Explicit numeric conversions.
Explicit enumeration conversions.
Explicit nullable conversions.
Explicit reference conversions.
Explicit interface conversions.
Unboxing conversions.
Explicit dynamic conversions
User-defined explicit conversions.
Explicit conversions can occur in cast expressions (
).
The set of explicit conversions includes all implicit conversions. This means that redundant cast expressions are allowed.
The explicit conversions that are not implicit conversions are conversions that cannot be proven to always succeed, conversions that are known to possibly lose information, and conversions across domains of types sufficiently different to merit explicit notation.
Explicit numeric conversions
The explicit numeric conversions are the conversions from a 
 to another 
 for which an implicit numeric conversion (
) does not already exist:
From 
 to 
, 
, 
, 
, or 
.
From 
 to 
 and 
.
From 
 to 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
From 
 to 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
Because the explicit conversions include all implicit and explicit numeric conversions, it is always possible to convert from any 
 to any other 
 using a cast expression (
).
The explicit numeric conversions possibly lose information or possibly cause exceptions to be thrown. An explicit numeric conversion is processed as follows:
For a conversion from an integral type to another integral type, the processing depends on the overflow checking context (
) in which the conversion takes place:
In a 
 context, the conversion succeeds if the value of the source operand is within the range of the destination type, but throws a 
 if the value of the source operand is outside the range of the destination type.
In an 
 context, the conversion always succeeds, and proceeds as follows.
If the source type is larger than the destination type, then the source value is truncated by discarding its ""extra"" most significant bits. The result is then treated as a value of the destination type.
If the source type is smaller than the destination type, then the source value is either sign-extended or zero-extended so that it is the same size as the destination type. Sign-extension is used if the source type is signed; zero-extension is used if the source type is unsigned. The result is then treated as a value of the destination type.
If the source type is the same size as the destination type, then the source value is treated as a value of the destination type.
For a conversion from 
 to an integral type, the source value is rounded towards zero to the nearest integral value, and this integral value becomes the result of the conversion. If the resulting integral value is outside the range of the destination type, a 
 is thrown.
For a conversion from 
 or 
 to an integral type, the processing depends on the overflow checking context (
) in which the conversion takes place:
In a 
 context, the conversion proceeds as follows:
If the value of the operand is NaN or infinite, a 
 is thrown.
Otherwise, the source operand is rounded towards zero to the nearest integral value. If this integral value is within the range of the destination type then this value is the result of the conversion.
Otherwise, a 
 is thrown.
In an 
 context, the conversion always succeeds, and proceeds as follows.
If the value of the operand is NaN or infinite, the result of the conversion is an unspecified value of the destination type.
Otherwise, the source operand is rounded towards zero to the nearest integral value. If this integral value is within the range of the destination type then this value is the result of the conversion.
Otherwise, the result of the conversion is an unspecified value of the destination type.
For a conversion from 
 to 
, the 
 value is rounded to the nearest 
 value. If the 
 value is too small to represent as a 
, the result becomes positive zero or negative zero. If the 
 value is too large to represent as a 
, the result becomes positive infinity or negative infinity. If the 
 value is NaN, the result is also NaN.
For a conversion from 
 or 
 to 
, the source value is converted to 
 representation and rounded to the nearest number after the 28th decimal place if required (
). If the source value is too small to represent as a 
, the result becomes zero. If the source value is NaN, infinity, or too large to represent as a 
, a 
 is thrown.
For a conversion from 
 to 
 or 
, the 
 value is rounded to the nearest 
 or 
 value. While this conversion may lose precision, it never causes an exception to be thrown.
Explicit enumeration conversions
The explicit enumeration conversions are:
From 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
 to any 
.
From any 
 to 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
From any 
 to any other 
.
An explicit enumeration conversion between two types is processed by treating any participating 
 as the underlying type of that 
, and then performing an implicit or explicit numeric conversion between the resulting types. For example, given an 
 
 with and underlying type of 
, a conversion from 
 to 
 is processed as an explicit numeric conversion (
) from 
 to 
, and a conversion from 
 to 
 is processed as an implicit numeric conversion (
) from 
 to 
.
Explicit nullable conversions
 permit predefined explicit conversions that operate on non-nullable value types to also be used with nullable forms of those types. For each of the predefined explicit conversions that convert from a non-nullable value type 
 to a non-nullable value type 
 (
, 
, 
, 
, and 
), the following nullable conversions exist:
An explicit conversion from 
 to 
.
An explicit conversion from 
 to 
.
An explicit conversion from 
 to 
.
Evaluation of a nullable conversion based on an underlying conversion from 
 to 
 proceeds as follows:
If the nullable conversion is from 
 to 
:
If the source value is null (
 property is false), the result is the null value of type 
.
Otherwise, the conversion is evaluated as an unwrapping from 
 to 
, followed by the underlying conversion from 
 to 
, followed by a wrapping from 
 to 
.
If the nullable conversion is from 
 to 
, the conversion is evaluated as the underlying conversion from 
 to 
 followed by a wrapping from 
 to 
.
If the nullable conversion is from 
 to 
, the conversion is evaluated as an unwrapping from 
 to 
 followed by the underlying conversion from 
 to 
.
Note that an attempt to unwrap a nullable value will throw an exception if the value is 
.
Explicit reference conversions
The explicit reference conversions are:
From 
 and 
 to any other 
.
From any 
 
 to any 
 
, provided 
 is a base class of 
.
From any 
 
 to any 
 
, provided 
 is not sealed and provided 
 does not implement 
.
From any 
 
 to any 
 
, provided 
 is not sealed or provided 
 implements 
.
From any 
 
 to any 
 
, provided 
 is not derived from 
.
From an 
 
 with an element type 
 to an 
 
 with an element type 
, provided all of the following are true:
 and 
 differ only in element type. In other words, 
 and 
 have the same number of dimensions.
Both 
 and 
 are 
s.
An explicit reference conversion exists from 
 to 
.
From 
 and the interfaces it implements to any 
.
From a single-dimensional array type 
 to 
 and its base interfaces, provided that there is an explicit reference conversion from 
 to 
.
From 
 and its base interfaces to a single-dimensional array type 
, provided that there is an explicit identity or reference conversion from 
 to 
.
From 
 and the interfaces it implements to any 
.
From a reference type to a reference type 
 if it has an explicit reference conversion to a reference type 
 and 
 has an identity conversion 
.
From a reference type to an interface or delegate type 
 if it has an explicit reference conversion to an interface or delegate type 
 and either 
 is variance-convertible to 
 or 
 is variance-convertible to 
 (
).
From 
 to 
 where 
 is a generic delegate type, 
 is not compatible with or identical to 
, and for each type parameter 
 of 
 the following holds:
If 
 is invariant, then 
 is identical to 
.
If 
 is covariant, then there is an implicit or explicit identity or reference conversion from 
 to 
.
If 
 is contravariant, then 
 and 
 are either identical or both reference types.
Explicit conversions involving type parameters that are known to be reference types. For more details on explicit conversions involving type parameters, see 
.
The explicit reference conversions are those conversions between reference-types that require run-time checks to ensure they are correct.
For an explicit reference conversion to succeed at run-time, the value of the source operand must be 
, or the actual type of the object referenced by the source operand must be a type that can be converted to the destination type by an implicit reference conversion (
) or boxing conversion (
). If an explicit reference conversion fails, a 
 is thrown.
Reference conversions, implicit or explicit, never change the referential identity of the object being converted. In other words, while a reference conversion may change the type of the reference, it never changes the type or value of the object being referred to.
Unboxing conversions
An unboxing conversion permits a reference type to be explicitly converted to a 
. An unboxing conversion exists from the types 
, 
 and 
 to any 
, and from any 
 to any 
 that implements the 
. Furthermore type 
 can be unboxed to any 
.
An unboxing conversion exists from a reference type to a 
 if an unboxing conversion exists from the reference type to the underlying 
 of the 
.
A value type 
 has an unboxing conversion from an interface type 
 if it has an unboxing conversion from an interface type 
 and 
 has an identity conversion to 
.
A value type 
 has an unboxing conversion from an interface type 
 if it has an unboxing conversion from an interface or delegate type 
 and either 
 is variance-convertible to 
 or 
 is variance-convertible to 
 (
).
An unboxing operation consists of first checking that the object instance is a boxed value of the given 
, and then copying the value out of the instance. Unboxing a null reference to a 
 produces the null value of the 
. A struct can be unboxed from the type 
, since that is a base class for all structs (
).
Unboxing conversions are described further in 
.
Explicit dynamic conversions
An explicit dynamic conversion exists from an expression of type 
 to any type 
. The conversion is dynamically bound (
), which means that an explicit conversion will be sought at run-time from the run-time type of the expression to 
. If no conversion is found, a run-time exception is thrown.
If dynamic binding of the conversion is not desired, the expression can be first converted to 
, and then to the desired type.
Assume the following class is defined:
The following example illustrates explicit dynamic conversions:
The best conversion of 
 to 
 is found at compile-time to be an explicit reference conversion. This fails at run-time, because 
 is not in fact a 
. The conversion of 
 to 
 however, as an explicit dynamic conversion, is suspended to run-time, where a user defined conversion from the run-time type of 
 -- 
 -- to 
 is found, and succeeds.
Explicit conversions involving type parameters
The following explicit conversions exist for a given type parameter 
:
From the effective base class 
 of 
 to 
 and from any base class of 
 to 
. At run-time, if 
 is a value type, the conversion is executed as an unboxing conversion. Otherwise, the conversion is executed as an explicit reference conversion or identity conversion.
From any interface type to 
. At run-time, if 
 is a value type, the conversion is executed as an unboxing conversion. Otherwise, the conversion is executed as an explicit reference conversion or identity conversion.
From 
 to any 
 
 provided there is not already an implicit conversion from 
 to 
. At run-time, if 
 is a value type, the conversion is executed as a boxing conversion followed by an explicit reference conversion. Otherwise, the conversion is executed as an explicit reference conversion or identity conversion.
From a type parameter 
 to 
, provided 
 depends on 
 (
). At run-time, if 
 is a value type, then 
 and 
 are necessarily the same type and no conversion is performed. Otherwise, if 
 is a value type, the conversion is executed as an unboxing conversion. Otherwise, the conversion is executed as an explicit reference conversion or identity conversion.
If 
 is known to be a reference type, the conversions above are all classified as explicit reference conversions (
). If 
 is not known to be a reference type, the conversions above are classified as unboxing conversions (
).
The above rules do not permit a direct explicit conversion from an unconstrained type parameter to a non-interface type, which might be surprising. The reason for this rule is to prevent confusion and make the semantics of such conversions clear. For example, consider the following declaration:
If the direct explicit conversion of 
 to 
 were permitted, one might easily expect that 
 would return 
. However, it would not, because the standard numeric conversions are only considered when the types are known to be numeric at binding-time. In order to make the semantics clear, the above example must instead be written:
This code will now compile but executing 
 would then throw an exception at run-time, since a boxed 
 cannot be converted directly to a 
.
User-defined explicit conversions
A user-defined explicit conversion consists of an optional standard explicit conversion, followed by execution of a user-defined implicit or explicit conversion operator, followed by another optional standard explicit conversion. The exact rules for evaluating user-defined explicit conversions are described in 
.
Standard conversions
The standard conversions are those pre-defined conversions that can occur as part of a user-defined conversion.
Standard implicit conversions
The following implicit conversions are classified as standard implicit conversions:
Identity conversions (
)
Implicit numeric conversions (
)
Implicit nullable conversions (
)
Implicit reference conversions (
)
Boxing conversions (
)
Implicit constant expression conversions (
)
Implicit conversions involving type parameters (
)
The standard implicit conversions specifically exclude user-defined implicit conversions.
Standard explicit conversions
The standard explicit conversions are all standard implicit conversions plus the subset of the explicit conversions for which an opposite standard implicit conversion exists. In other words, if a standard implicit conversion exists from a type 
 to a type 
, then a standard explicit conversion exists from type 
 to type 
 and from type 
 to type 
.
User-defined conversions
C# allows the pre-defined implicit and explicit conversions to be augmented by 
. User-defined conversions are introduced by declaring conversion operators (
) in class and struct types.
Permitted user-defined conversions
C# permits only certain user-defined conversions to be declared. In particular, it is not possible to redefine an already existing implicit or explicit conversion.
For a given source type 
 and target type 
, if 
 or 
 are nullable types, let 
 and 
 refer to their underlying types, otherwise 
 and 
 are equal to 
 and 
 respectively. A class or struct is permitted to declare a conversion from a source type 
 to a target type 
 only if all of the following are true:
 and 
 are different types.
Either 
 or 
 is the class or struct type in which the operator declaration takes place.
Neither 
 nor 
 is an 
.
Excluding user-defined conversions, a conversion does not exist from 
 to 
 or from 
 to 
.
The restrictions that apply to user-defined conversions are discussed further in 
.
Lifted conversion operators
Given a user-defined conversion operator that converts from a non-nullable value type 
 to a non-nullable value type 
, a 
 exists that converts from 
 to 
. This lifted conversion operator performs an unwrapping from 
 to 
 followed by the user-defined conversion from 
 to 
 followed by a wrapping from 
 to 
, except that a null valued 
 converts directly to a null valued 
.
A lifted conversion operator has the same implicit or explicit classification as its underlying user-defined conversion operator. The term ""user-defined conversion"" applies to the use of both user-defined and lifted conversion operators.
Evaluation of user-defined conversions
A user-defined conversion converts a value from its type, called the 
, to another type, called the 
. Evaluation of a user-defined conversion centers on finding the 
 user-defined conversion operator for the particular source and target types. This determination is broken into several steps:
Finding the set of classes and structs from which user-defined conversion operators will be considered. This set consists of the source type and its base classes and the target type and its base classes (with the implicit assumptions that only classes and structs can declare user-defined operators, and that non-class types have no base classes). For the purposes of this step, if either the source or target type is a 
, their underlying type is used instead.
From that set of types, determining which user-defined and lifted conversion operators are applicable. For a conversion operator to be applicable, it must be possible to perform a standard conversion (
) from the source type to the operand type of the operator, and it must be possible to perform a standard conversion from the result type of the operator to the target type.
From the set of applicable user-defined operators, determining which operator is unambiguously the most specific. In general terms, the most specific operator is the operator whose operand type is ""closest"" to the source type and whose result type is ""closest"" to the target type. User-defined conversion operators are preferred over lifted conversion operators. The exact rules for establishing the most specific user-defined conversion operator are defined in the following sections.
Once a most specific user-defined conversion operator has been identified, the actual execution of the user-defined conversion involves up to three steps:
First, if required, performing a standard conversion from the source type to the operand type of the user-defined or lifted conversion operator.
Next, invoking the user-defined or lifted conversion operator to perform the conversion.
Finally, if required, performing a standard conversion from the result type of the user-defined or lifted conversion operator to the target type.
Evaluation of a user-defined conversion never involves more than one user-defined or lifted conversion operator. In other words, a conversion from type 
 to type 
 will never first execute a user-defined conversion from 
 to 
 and then execute a user-defined conversion from 
 to 
.
Exact definitions of evaluation of user-defined implicit or explicit conversions are given in the following sections. The definitions make use of the following terms:
If a standard implicit conversion (
) exists from a type 
 to a type 
, and if neither 
 nor 
 are 
s, then 
 is said to be 
 
, and 
 is said to 
 
.
The 
 in a set of types is the one type that encompasses all other types in the set. If no single type encompasses all other types, then the set has no most encompassing type. In more intuitive terms, the most encompassing type is the ""largest"" type in the set—the one type to which each of the other types can be implicitly converted.
The 
 in a set of types is the one type that is encompassed by all other types in the set. If no single type is encompassed by all other types, then the set has no most encompassed type. In more intuitive terms, the most encompassed type is the ""smallest"" type in the set—the one type that can be implicitly converted to each of the other types.
Processing of user-defined implicit conversions
A user-defined implicit conversion from type 
 to type 
 is processed as follows:
Determine the types 
 and 
. If 
 or 
 are nullable types, 
 and 
 are their underlying types, otherwise 
 and 
 are equal to 
 and 
 respectively.
Find the set of types, 
, from which user-defined conversion operators will be considered. This set consists of 
 (if 
 is a class or struct), the base classes of 
 (if 
 is a class), and 
 (if 
 is a class or struct).
Find the set of applicable user-defined and lifted conversion operators, 
. This set consists of the user-defined and lifted implicit conversion operators declared by the classes or structs in 
 that convert from a type encompassing 
 to a type encompassed by 
. If 
 is empty, the conversion is undefined and a compile-time error occurs.
Find the most specific source type, 
, of the operators in 
:
If any of the operators in 
 convert from 
, then 
 is 
.
Otherwise, 
 is the most encompassed type in the combined set of source types of the operators in 
. If exactly one most encompassed type cannot be found, then the conversion is ambiguous and a compile-time error occurs.
Find the most specific target type, 
, of the operators in 
:
If any of the operators in 
 convert to 
, then 
 is 
.
Otherwise, 
 is the most encompassing type in the combined set of target types of the operators in 
. If exactly one most encompassing type cannot be found, then the conversion is ambiguous and a compile-time error occurs.
Find the most specific conversion operator:
If 
 contains exactly one user-defined conversion operator that converts from 
 to 
, then this is the most specific conversion operator.
Otherwise, if 
 contains exactly one lifted conversion operator that converts from 
 to 
, then this is the most specific conversion operator.
Otherwise, the conversion is ambiguous and a compile-time error occurs.
Finally, apply the conversion:
If 
 is not 
, then a standard implicit conversion from 
 to 
 is performed.
The most specific conversion operator is invoked to convert from 
 to 
.
If 
 is not 
, then a standard implicit conversion from 
 to 
 is performed.
Processing of user-defined explicit conversions
A user-defined explicit conversion from type 
 to type 
 is processed as follows:
Determine the types 
 and 
. If 
 or 
 are nullable types, 
 and 
 are their underlying types, otherwise 
 and 
 are equal to 
 and 
 respectively.
Find the set of types, 
, from which user-defined conversion operators will be considered. This set consists of 
 (if 
 is a class or struct), the base classes of 
 (if 
 is a class), 
 (if 
 is a class or struct), and the base classes of 
 (if 
 is a class).
Find the set of applicable user-defined and lifted conversion operators, 
. This set consists of the user-defined and lifted implicit or explicit conversion operators declared by the classes or structs in 
 that convert from a type encompassing or encompassed by 
 to a type encompassing or encompassed by 
. If 
 is empty, the conversion is undefined and a compile-time error occurs.
Find the most specific source type, 
, of the operators in 
:
If any of the operators in 
 convert from 
, then 
 is 
.
Otherwise, if any of the operators in 
 convert from types that encompass 
, then 
 is the most encompassed type in the combined set of source types of those operators. If no most encompassed type can be found, then the conversion is ambiguous and a compile-time error occurs.
Otherwise, 
 is the most encompassing type in the combined set of source types of the operators in 
. If exactly one most encompassing type cannot be found, then the conversion is ambiguous and a compile-time error occurs.
Find the most specific target type, 
, of the operators in 
:
If any of the operators in 
 convert to 
, then 
 is 
.
Otherwise, if any of the operators in 
 convert to types that are encompassed by 
, then 
 is the most encompassing type in the combined set of target types of those operators. If exactly one most encompassing type cannot be found, then the conversion is ambiguous and a compile-time error occurs.
Otherwise, 
 is the most encompassed type in the combined set of target types of the operators in 
. If no most encompassed type can be found, then the conversion is ambiguous and a compile-time error occurs.
Find the most specific conversion operator:
If 
 contains exactly one user-defined conversion operator that converts from 
 to 
, then this is the most specific conversion operator.
Otherwise, if 
 contains exactly one lifted conversion operator that converts from 
 to 
, then this is the most specific conversion operator.
Otherwise, the conversion is ambiguous and a compile-time error occurs.
Finally, apply the conversion:
If 
 is not 
, then a standard explicit conversion from 
 to 
 is performed.
The most specific user-defined conversion operator is invoked to convert from 
 to 
.
If 
 is not 
, then a standard explicit conversion from 
 to 
 is performed.
Anonymous function conversions
An 
 or 
 is classified as an anonymous function (
). The expression does not have a type but can be implicitly converted to a compatible delegate type or expression tree type. Specifically, an anonymous function 
 is compatible with a delegate type 
 provided:
If 
 contains an 
, then 
 and 
 have the same number of parameters.
If 
 does not contain an 
, then 
 may have zero or more parameters of any type, as long as no parameter of 
 has the 
 parameter modifier.
If 
 has an explicitly typed parameter list, each parameter in 
 has the same type and modifiers as the corresponding parameter in 
.
If 
 has an implicitly typed parameter list, 
 has no 
 or 
 parameters.
If the body of 
 is an expression, and either 
 has a 
 return type or 
 is async and 
 has the return type 
, then when each parameter of 
 is given the type of the corresponding parameter in 
, the body of 
 is a valid expression (wrt 
) that would be permitted as a 
 (
).
If the body of 
 is a statement block, and either 
 has a 
 return type or 
 is async and 
 has the return type 
, then when each parameter of 
 is given the type of the corresponding parameter in 
, the body of 
 is a valid statement block (wrt 
) in which no 
 statement specifies an expression.
If the body of 
 is an expression, and 
either
 
 is non-async and 
 has a non-void return type 
, 
or
 
 is async and 
 has a return type 
, then when each parameter of 
 is given the type of the corresponding parameter in 
, the body of 
 is a valid expression (wrt 
) that is implicitly convertible to 
.
If the body of 
 is a statement block, and 
either
 
 is non-async and 
 has a non-void return type 
, 
or
 
 is async and 
 has a return type 
, then when each parameter of 
 is given the type of the corresponding parameter in 
, the body of 
 is a valid statement block (wrt 
) with a non-reachable end point in which each 
 statement specifies an expression that is implicitly convertible to 
.
For the purpose of brevity, this section uses the short form for the task types 
 and 
 (
).
A lambda expression 
 is compatible with an expression tree type 
 if 
 is compatible with the delegate type 
. Note that this does not apply to anonymous methods, only lambda expressions.
Certain lambda expressions cannot be converted to expression tree types: Even though the conversion 
exists
, it fails at compile-time. This is the case if the lambda expression:
Has a 
 body
Contains simple or compound assignment operators
Contains a dynamically bound expression
Is async
The examples that follow use a generic delegate type 
 which represents a function that takes an argument of type 
 and returns a value of type 
:
In the assignments
the parameter and return types of each anonymous function are determined from the type of the variable to which the anonymous function is assigned.
The first assignment successfully converts the anonymous function to the delegate type 
 because, when 
 is given type 
, 
 is a valid expression that is implicitly convertible to type 
.
Likewise, the second assignment successfully converts the anonymous function to the delegate type 
 because the result of 
 (of type 
) is implicitly convertible to type 
.
However, the third assignment is a compile-time error because, when 
 is given type 
, the result of 
 (of type 
) is not implicitly convertible to type 
.
The fourth assignment successfully converts the anonymous async function to the delegate type 
 because the result of 
 (of type 
) is implicitly convertible to the result type 
 of the task type 
.
Anonymous functions may influence overload resolution, and participate in type inference. See 
 for further details.
Evaluation of anonymous function conversions to delegate types
Conversion of an anonymous function to a delegate type produces a delegate instance which references the anonymous function and the (possibly empty) set of captured outer variables that are active at the time of the evaluation. When the delegate is invoked, the body of the anonymous function is executed. The code in the body is executed using the set of captured outer variables referenced by the delegate.
The invocation list of a delegate produced from an anonymous function contains a single entry. The exact target object and target method of the delegate are unspecified. In particular, it is unspecified whether the target object of the delegate is 
, the 
 value of the enclosing function member, or some other object.
Conversions of semantically identical anonymous functions with the same (possibly empty) set of captured outer variable instances to the same delegate types are permitted (but not required) to return the same delegate instance. The term semantically identical is used here to mean that execution of the anonymous functions will, in all cases, produce the same effects given the same arguments. This rule permits code such as the following to be optimized.
Since the two anonymous function delegates have the same (empty) set of captured outer variables, and since the anonymous functions are semantically identical, the compiler is permitted to have the delegates refer to the same target method. Indeed, the compiler is permitted to return the very same delegate instance from both anonymous function expressions.
Evaluation of anonymous function conversions to expression tree types
Conversion of an anonymous function to an expression tree type produces an expression tree (
). More precisely, evaluation of the anonymous function conversion leads to the construction of an object structure that represents the structure of the anonymous function itself. The precise structure of the expression tree, as well as the exact process for creating it, are implementation defined.
Implementation example
This section describes a possible implementation of anonymous function conversions in terms of other C# constructs. The implementation described here is based on the same principles used by the Microsoft C# compiler, but it is by no means a mandated implementation, nor is it the only one possible. It only briefly mentions conversions to expression trees, as their exact semantics are outside the scope of this specification.
The remainder of this section gives several examples of code that contains anonymous functions with different characteristics. For each example, a corresponding translation to code that uses only other C# constructs is provided. In the examples, the identifier 
 is assumed by represent the following delegate type:
The simplest form of an anonymous function is one that captures no outer variables:
This can be translated to a delegate instantiation that references a compiler generated static method in which the code of the anonymous function is placed:
In the following example, the anonymous function references instance members of 
:
This can be translated to a compiler generated instance method containing the code of the anonymous function:
In this example, the anonymous function captures a local variable:
The lifetime of the local variable must now be extended to at least the lifetime of the anonymous function delegate. This can be achieved by ""hoisting"" the local variable into a field of a compiler generated class. Instantiation of the local variable (
) then corresponds to creating an instance of the compiler generated class, and accessing the local variable corresponds to accessing a field in the instance of the compiler generated class. Furthermore, the anonymous function becomes an instance method of the compiler generated class:
Finally, the following anonymous function captures 
 as well as two local variables with different lifetimes:
Here, a compiler generated class is created for each statement block in which locals are captured such that the locals in the different blocks can have independent lifetimes. An instance of 
, the compiler generated class for the inner statement block, contains the local variable 
 and a field that references an instance of 
.  An instance of 
, the compiler generated class for the outer statement block, contains the local variable 
 and a field that references 
 of the enclosing function member. With these data structures it is possible to reach all captured outer variables through an instance of 
, and the code of the anonymous function can thus be implemented as an instance method of that class.
The same technique applied here to capture local variables can also be used when converting anonymous functions to expression trees: References to the compiler generated objects can be stored in the expression tree, and access to the local variables can be represented as field accesses on these objects. The advantage of this approach is that it allows the ""lifted"" local variables to be shared between delegates and expression trees.
Method group conversions
An implicit conversion (
) exists from a method group (
) to a compatible delegate type. Given a delegate type 
 and an expression 
 that is classified as a method group, an implicit conversion exists from 
 to 
 if 
 contains at least one method that is applicable in its normal form (
) to an argument list constructed by use of the parameter types and modifiers of 
, as described in the following.
The compile-time application of a conversion from a method group 
 to a delegate type 
 is described in the following. Note that the existence of an implicit conversion from 
 to 
 does not guarantee that the compile-time application of the conversion will succeed without error.
A single method 
 is selected corresponding to a method invocation (
) of the form 
, with the following modifications:
The argument list 
 is a list of expressions, each classified as a variable and with the type and modifier (
 or 
) of the corresponding parameter in the 
 of 
.
The candidate methods considered are only those methods that are applicable in their normal form (
), not those applicable only in their expanded form.
If the algorithm of 
 produces an error, then a compile-time error occurs. Otherwise the algorithm produces a single best method 
 having the same number of parameters as 
 and the conversion is considered to exist.
The selected method 
 must be compatible (
) with the delegate type 
, or otherwise, a compile-time error occurs.
If the selected method 
 is an instance method, the instance expression associated with 
 determines the target object of the delegate.
If the selected method M is an extension method which is denoted by means of a member access on an instance expression, that instance expression determines the target object of the delegate.
The result of the conversion is a value of type 
, namely a newly created delegate that refers to the selected method and target object.
Note that this process can lead to the creation of a delegate to an extension method, if the algorithm of 
 fails to find an instance method but succeeds in processing the invocation of 
 as an extension method invocation (
). A delegate thus created captures the extension method as well as its first argument.
The following example demonstrates method group conversions:
The assignment to 
 implicitly converts the method group 
 to a value of type 
.
The assignment to 
 shows how it is possible to create a delegate to a method that has less derived (contra-variant) parameter types and a more derived (covariant) return type.
The assignment to 
 shows how no conversion exists if the method is not applicable.
The assignment to 
 shows how the method must be applicable in its normal form.
The assignment to 
 shows how parameter and return types of the delegate and method are allowed to differ only for reference types.
As with all other implicit and explicit conversions, the cast operator can be used to explicitly perform a method group conversion. Thus, the example
could instead be written
Method groups may influence overload resolution, and participate in type inference. See 
 for further details.
The run-time evaluation of a method group conversion proceeds as follows:
If the method selected at compile-time is an instance method, or it is an extension method which is accessed as an instance method, the target object of the delegate is determined from the instance expression associated with 
:
The instance expression is evaluated. If this evaluation causes an exception, no further steps are executed.
If the instance expression is of a 
, the value computed by the instance expression becomes the target object. If the selected method is an instance method and the target object is 
, a 
 is thrown and no further steps are executed.
If the instance expression is of a 
, a boxing operation (
) is performed to convert the value to an object, and this object becomes the target object.
Otherwise the selected method is part of a static method call, and the target object of the delegate is 
.
A new instance of the delegate type 
 is allocated. If there is not enough memory available to allocate the new instance, a 
 is thrown and no further steps are executed.
The new delegate instance is initialized with a reference to the method that was determined at compile-time and a reference to the target object computed above.
Expressions
An expression is a sequence of operators and operands. This chapter defines the syntax, order of evaluation of operands and operators, and meaning of expressions.
Expression classifications
An expression is classified as one of the following:
A value. Every value has an associated type.
A variable. Every variable has an associated type, namely the declared type of the variable.
A namespace. An expression with this classification can only appear as the left hand side of a 
 (
). In any other context, an expression classified as a namespace causes a compile-time error.
A type. An expression with this classification can only appear as the left hand side of a 
 (
), or as an operand for the 
 operator (
), the 
 operator (
), or the 
 operator (
). In any other context, an expression classified as a type causes a compile-time error.
A method group, which is a set of overloaded methods resulting from a member lookup (
). A method group may have an associated instance expression and an associated type argument list. When an instance method is invoked, the result of evaluating the instance expression becomes the instance represented by 
 (
). A method group is permitted in an 
 (
) , a 
 (
) and as the left hand side of an is operator, and can be implicitly converted to a compatible delegate type (
). In any other context, an expression classified as a method group causes a compile-time error.
A null literal. An expression with this classification can be implicitly converted to a reference type or nullable type.
An anonymous function. An expression with this classification can be implicitly converted to a compatible delegate type or expression tree type.
A property access. Every property access has an associated type, namely the type of the property. Furthermore, a property access may have an associated instance expression. When an accessor (the 
 or 
 block) of an instance property access is invoked, the result of evaluating the instance expression becomes the instance represented by 
 (
).
An event access. Every event access has an associated type, namely the type of the event. Furthermore, an event access may have an associated instance expression. An event access may appear as the left hand operand of the 
 and 
 operators (
). In any other context, an expression classified as an event access causes a compile-time error.
An indexer access. Every indexer access has an associated type, namely the element type of the indexer. Furthermore, an indexer access has an associated instance expression and an associated argument list. When an accessor (the 
 or 
 block) of an indexer access is invoked, the result of evaluating the instance expression becomes the instance represented by 
 (
), and the result of evaluating the argument list becomes the parameter list of the invocation.
Nothing. This occurs when the expression is an invocation of a method with a return type of 
. An expression classified as nothing is only valid in the context of a 
 (
).
The final result of an expression is never a namespace, type, method group, or event access. Rather, as noted above, these categories of expressions are intermediate constructs that are only permitted in certain contexts.
A property access or indexer access is always reclassified as a value by performing an invocation of the 
get accessor
 or the 
set accessor
. The particular accessor is determined by the context of the property or indexer access: If the access is the target of an assignment, the 
set accessor
 is invoked to assign a new value (
). Otherwise, the 
get accessor
 is invoked to obtain the current value (
).
Values of expressions
Most of the constructs that involve an expression ultimately require the expression to denote a 
. In such cases, if the actual expression denotes a namespace, a type, a method group, or nothing, a compile-time error occurs. However, if the expression denotes a property access, an indexer access, or a variable, the value of the property, indexer, or variable is implicitly substituted:
The value of a variable is simply the value currently stored in the storage location identified by the variable. A variable must be considered definitely assigned (
) before its value can be obtained, or otherwise a compile-time error occurs.
The value of a property access expression is obtained by invoking the 
get accessor
 of the property. If the property has no 
get accessor
, a compile-time error occurs. Otherwise, a function member invocation (
) is performed, and the result of the invocation becomes the value of the property access expression.
The value of an indexer access expression is obtained by invoking the 
get accessor
 of the indexer. If the indexer has no 
get accessor
, a compile-time error occurs. Otherwise, a function member invocation (
) is performed with the argument list associated with the indexer access expression, and the result of the invocation becomes the value of the indexer access expression.
Static and Dynamic Binding
The process of determining the meaning of an operation based on the type or value of constituent expressions (arguments, operands, receivers) is often referred to as 
. For instance the meaning of a method call is determined based on the type of the receiver and arguments. The meaning of an operator is determined based on the type of its operands.
In C# the meaning of an operation is usually determined at compile-time, based on the compile-time type of its constituent expressions. Likewise, if an expression contains an error, the error is detected and reported by the compiler. This approach is known as 
.
However, if an expression is a dynamic expression (i.e. has the type 
) this indicates that any binding that it participates in should be based on its run-time type (i.e. the actual type of the object it denotes at run-time) rather than the type it has at compile-time. The binding of such an operation is therefore deferred until the time where the operation is to be executed during the running of the program. This is referred to as 
.
When an operation is dynamically bound, little or no checking is performed by the compiler. Instead if the run-time binding fails, errors are reported as exceptions at run-time.
The following operations in C# are subject to binding:
Member access: 
Method invocation: 
Delegate invocaton:
Element access: 
Object creation: 
Overloaded unary operators: 
, 
, 
, 
, 
, 
, 
, 
Overloaded binary operators: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
,
, 
, 
, 
, 
Assignment operators: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
Implicit and explicit conversions
When no dynamic expressions are involved, C# defaults to static binding, which means that the compile-time types of constituent expressions are used in the selection process. However, when one of the constituent expressions in the operations listed above is a dynamic expression, the operation is instead dynamically bound.
Binding-time
Static binding takes place at compile-time, whereas dynamic binding takes place at run-time. In the following sections, the term 
 refers to either compile-time or run-time, depending on when the binding takes place.
The following example illustrates the notions of static and dynamic binding and of binding-time:
The first two calls are statically bound: the overload of 
 is picked based on the compile-time type of their argument. Thus, the binding-time is compile-time.
The third call is dynamically bound: the overload of 
 is picked based on the run-time type of its argument. This happens because the argument is a dynamic expression -- its compile-time type is 
. Thus, the binding-time for the third call is run-time.
Dynamic binding
The purpose of dynamic binding is to allow C# programs to interact with 
, i.e. objects that do not follow the normal rules of the C# type system. Dynamic objects may be objects from other programming languages with different types systems, or they may be objects that are programmatically setup to implement their own binding semantics for different operations.
The mechanism by which a dynamic object implements its own semantics is implementation defined. A given interface -- again implementation defined -- is implemented by dynamic objects to signal to the C# run-time that they have special semantics. Thus, whenever operations on a dynamic object are dynamically bound, their own binding semantics, rather than those of C# as specified in this document, take over.
While the purpose of dynamic binding is to allow interoperation with dynamic objects, C# allows dynamic binding on all objects, whether they are dynamic or not. This allows for a smoother integration of dynamic objects, as the results of operations on them may not themselves be dynamic objects, but are still of a type unknown to the programmer at compile-time. Also dynamic binding can help eliminate error-prone reflection-based code even when no objects involved are dynamic objects.
The following sections describe for each construct in the language exactly when dynamic binding is applied, what compile time checking -- if any -- is applied, and what the compile-time result and expression classification is.
Types of constituent expressions
When an operation is statically bound, the type of a constituent expression (e.g. a receiver, and argument, an index or an operand) is always considered to be the compile-time type of that expression.
When an operation is dynamically bound, the type of a constituent expression is determined in different ways depending on the compile-time type of the constituent expression:
A constituent expression of compile-time type 
 is considered to have the type of the actual value that the expression evaluates to at runtime
A constituent expression whose compile-time type is a type parameter is considered to have the type which the type parameter is bound to at runtime
Otherwise the constituent expression is considered to have its compile-time type.
Operators
Expressions are constructed from 
 and 
. The operators of an expression indicate which operations to apply to the operands. Examples of operators include 
, 
, 
, 
, and 
. Examples of operands include literals, fields, local variables, and expressions.
There are three kinds of operators:
Unary operators. The unary operators take one operand and use either prefix notation (such as 
) or postfix notation (such as 
).
Binary operators. The binary operators take two operands and all use infix notation (such as 
).
Ternary operator. Only one ternary operator, 
, exists; it takes three operands and uses infix notation (
).
The order of evaluation of operators in an expression is determined by the 
 and 
 of the operators (
).
Operands in an expression are evaluated from left to right. For example, in 
, method 
 is called using the old value of 
, then method 
 is called with the old value of 
, and, finally, method 
 is called with the new value of 
. This is separate from and unrelated to operator precedence.
Certain operators can be 
. Operator overloading permits user-defined operator implementations to be specified for operations where one or both of the operands are of a user-defined class or struct type (
).
Operator precedence and associativity
When an expression contains multiple operators, the 
 of the operators controls the order in which the individual operators are evaluated. For example, the expression 
 is evaluated as 
 because the 
 operator has higher precedence than the binary 
 operator. The precedence of an operator is established by the definition of its associated grammar production. For example, an 
 consists of a sequence of 
s separated by 
 or 
 operators, thus giving the 
 and 
 operators lower precedence than the 
, 
, and 
 operators.
The following table summarizes all operators in order of precedence from highest to lowest:
Section
Category
Operators
Primary
  
  
  
  
  
  
  
  
  
  
Unary
  
  
  
  
  
  
Multiplicative
  
  
Additive
  
Shift
  
Relational and type testing
  
  
  
  
  
Equality
  
Logical AND
Logical XOR
Logical OR
Conditional AND
Conditional OR
Null coalescing
Conditional
, 
Assignment and lambda expression
  
  
  
  
  
  
  
  
  
  
  
When an operand occurs between two operators with the same precedence, the associativity of the operators controls the order in which the operations are performed:
Except for the assignment operators and the null coalescing operator, all binary operators are 
, meaning that operations are performed from left to right. For example, 
 is evaluated as 
.
The assignment operators, the null coalescing operator and the conditional operator (
) are 
, meaning that operations are performed from right to left. For example, 
 is evaluated as 
.
Precedence and associativity can be controlled using parentheses. For example, 
 first multiplies 
 by 
 and then adds the result to 
, but 
 first adds 
 and 
 and then multiplies the result by 
.
Operator overloading
All unary and binary operators have predefined implementations that are automatically available in any expression. In addition to the predefined implementations, user-defined implementations can be introduced by including 
 declarations in classes and structs (
). User-defined operator implementations always take precedence over predefined operator implementations: Only when no applicable user-defined operator implementations exist will the predefined operator implementations be considered, as described in 
 and 
.
The 
 are:
Although 
 and 
 are not used explicitly in expressions (and therefore are not included in the precedence table in 
), they are considered operators because they are invoked in several expression contexts: boolean expressions (
) and expressions involving the conditional (
), and conditional logical operators (
).
The 
 are:
Only the operators listed above can be overloaded. In particular, it is not possible to overload member access, method invocation, or the 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and 
 operators.
When a binary operator is overloaded, the corresponding assignment operator, if any, is also implicitly overloaded. For example, an overload of operator 
 is also an overload of operator 
. This is described further in 
. Note that the assignment operator itself (
) cannot be overloaded. An assignment always performs a simple bit-wise copy of a value into a variable.
Cast operations, such as 
, are overloaded by providing user-defined conversions (
).
Element access, such as 
, is not considered an overloadable operator. Instead, user-defined indexing is supported through indexers (
).
In expressions, operators are referenced using operator notation, and in declarations, operators are referenced using functional notation. The following table shows the relationship between operator and functional notations for unary and binary operators. In the first entry, 
op
 denotes any overloadable unary prefix operator. In the second entry, 
op
 denotes the unary postfix 
 and 
 operators. In the third entry, 
op
 denotes any overloadable binary operator.
Operator notation
Functional notation
User-defined operator declarations always require at least one of the parameters to be of the class or struct type that contains the operator declaration. Thus, it is not possible for a user-defined operator to have the same signature as a predefined operator.
User-defined operator declarations cannot modify the syntax, precedence, or associativity of an operator. For example, the 
 operator is always a binary operator, always has the precedence level specified in 
, and is always left-associative.
While it is possible for a user-defined operator to perform any computation it pleases, implementations that produce results other than those that are intuitively expected are strongly discouraged. For example, an implementation of 
 should compare the two operands for equality and return an appropriate 
 result.
The descriptions of individual operators in 
 through 
 specify the predefined implementations of the operators and any additional rules that apply to each operator. The descriptions make use of the terms 
, 
, and 
, definitions of which are found in the following sections.
Unary operator overload resolution
An operation of the form 
 or 
, where 
 is an overloadable unary operator, and 
 is an expression of type 
, is processed as follows:
The set of candidate user-defined operators provided by 
 for the operation 
 is determined using the rules of 
.
If the set of candidate user-defined operators is not empty, then this becomes the set of candidate operators for the operation. Otherwise, the predefined unary 
 implementations, including their lifted forms, become the set of candidate operators for the operation. The predefined implementations of a given operator are specified in the description of the operator (
 and 
).
The overload resolution rules of 
 are applied to the set of candidate operators to select the best operator with respect to the argument list 
, and this operator becomes the result of the overload resolution process. If overload resolution fails to select a single best operator, a binding-time error occurs.
Binary operator overload resolution
An operation of the form 
, where 
 is an overloadable binary operator, 
 is an expression of type 
, and 
 is an expression of type 
, is processed as follows:
The set of candidate user-defined operators provided by 
 and 
 for the operation 
 is determined. The set consists of the union of the candidate operators provided by 
 and the candidate operators provided by 
, each determined using the rules of 
. If 
 and 
 are the same type, or if 
 and 
 are derived from a common base type, then shared candidate operators only occur in the combined set once.
If the set of candidate user-defined operators is not empty, then this becomes the set of candidate operators for the operation. Otherwise, the predefined binary 
 implementations, including their lifted forms,  become the set of candidate operators for the operation. The predefined implementations of a given operator are specified in the description of the operator (
 through 
). For predefined enum and delegate operators, the only operators considered are those defined by an enum or delegate type that is the binding-time type of one of the operands.
The overload resolution rules of 
 are applied to the set of candidate operators to select the best operator with respect to the argument list 
, and this operator becomes the result of the overload resolution process. If overload resolution fails to select a single best operator, a binding-time error occurs.
Candidate user-defined operators
Given a type 
 and an operation 
, where 
 is an overloadable operator and 
 is an argument list, the set of candidate user-defined operators provided by 
 for 
 is determined as follows:
Determine the type 
. If 
 is a nullable type, 
 is its underlying type, otherwise 
 is equal to 
.
For all 
 declarations in 
 and all lifted forms of such operators, if at least one operator is applicable (
) with respect to the argument list 
, then the set of candidate operators consists of all such applicable operators in 
.
Otherwise, if 
 is 
, the set of candidate operators is empty.
Otherwise, the set of candidate operators provided by 
 is the set of candidate operators provided by the direct base class of 
, or the effective base class of 
 if 
 is a type parameter.
Numeric promotions
Numeric promotion consists of automatically performing certain implicit conversions of the operands of the predefined unary and binary numeric operators. Numeric promotion is not a distinct mechanism, but rather an effect of applying overload resolution to the predefined operators. Numeric promotion specifically does not affect evaluation of user-defined operators, although user-defined operators can be implemented to exhibit similar effects.
As an example of numeric promotion, consider the predefined implementations of the binary 
 operator:
When overload resolution rules (
) are applied to this set of operators, the effect is to select the first of the operators for which implicit conversions exist from the operand types. For example, for the operation 
, where 
 is a 
 and 
 is a 
, overload resolution selects 
 as the best operator. Thus, the effect is that 
 and 
 are converted to 
, and the type of the result is 
. Likewise, for the operation 
, where 
 is an 
 and 
 is a 
, overload resolution selects 
 as the best operator.
Unary numeric promotions
Unary numeric promotion occurs for the operands of the predefined 
, 
, and 
 unary operators. Unary numeric promotion simply consists of converting operands of type 
, 
, 
, 
, or 
 to type 
. Additionally, for the unary 
 operator, unary numeric promotion converts operands of type 
 to type 
.
Binary numeric promotions
Binary numeric promotion occurs for the operands of the predefined 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and 
 binary operators. Binary numeric promotion implicitly converts both operands to a common type which, in case of the non-relational operators, also becomes the result type of the operation. Binary numeric promotion consists of applying the following rules, in the order they appear here:
If either operand is of type 
, the other operand is converted to type 
, or a binding-time error occurs if the other operand is of type 
 or 
.
Otherwise, if either operand is of type 
, the other operand is converted to type 
.
Otherwise, if either operand is of type 
, the other operand is converted to type 
.
Otherwise, if either operand is of type 
, the other operand is converted to type 
, or a binding-time error occurs if the other operand is of type 
, 
, 
, or 
.
Otherwise, if either operand is of type 
, the other operand is converted to type 
.
Otherwise, if either operand is of type 
 and the other operand is of type 
, 
, or 
, both operands are converted to type 
.
Otherwise, if either operand is of type 
, the other operand is converted to type 
.
Otherwise, both operands are converted to type 
.
Note that the first rule disallows any operations that mix the 
 type with the 
 and 
 types. The rule follows from the fact that there are no implicit conversions between the 
 type and the 
 and 
 types.
Also note that it is not possible for an operand to be of type 
 when the other operand is of a signed integral type. The reason is that no integral type exists that can represent the full range of 
 as well as the signed integral types.
In both of the above cases, a cast expression can be used to explicitly convert one operand to a type that is compatible with the other operand.
In the example
a binding-time error occurs because a 
 cannot be multiplied by a 
. The error is resolved by explicitly converting the second operand to 
, as follows:
Lifted operators
 permit predefined and user-defined operators that operate on non-nullable value types to also be used with nullable forms of those types. Lifted operators are constructed from predefined and user-defined operators that meet certain requirements, as described in the following:
For the unary operators
a lifted form of an operator exists if the operand and result types are both non-nullable value types. The lifted form is constructed by adding a single 
 modifier to the operand and result types. The lifted operator produces a null value if the operand is null. Otherwise, the lifted operator unwraps the operand, applies the underlying operator, and wraps the result.
For the binary operators
a lifted form of an operator exists if the operand and result types are all non-nullable value types. The lifted form is constructed by adding a single 
 modifier to each operand and result type. The lifted operator produces a null value if one or both operands are null (an exception being the 
 and 
 operators of the 
 type, as described in 
). Otherwise, the lifted operator unwraps the operands, applies the underlying operator, and wraps the result.
For the equality operators
a lifted form of an operator exists if the operand types are both non-nullable value types and if the result type is 
. The lifted form is constructed by adding a single 
 modifier to each operand type. The lifted operator considers two null values equal, and a null value unequal to any non-null value. If both operands are non-null, the lifted operator unwraps the operands and applies the underlying operator to produce the 
 result.
For the relational operators
a lifted form of an operator exists if the operand types are both non-nullable value types and if the result type is 
. The lifted form is constructed by adding a single 
 modifier to each operand type. The lifted operator produces the value 
 if one or both operands are null. Otherwise, the lifted operator unwraps the operands and applies the underlying operator to produce the 
 result.
Member lookup
A member lookup is the process whereby the meaning of a name in the context of a type is determined. A member lookup can occur as part of evaluating a 
 (
) or a 
 (
) in an expression. If the 
 or 
 occurs as the 
 of an 
 (
), the member is said to be invoked.
If a member is a method or event, or if it is a constant, field or property of either a delegate type (
) or the type 
 (
), then the member is said to be 
invocable
.
Member lookup considers not only the name of a member but also the number of type parameters the member has and whether the member is accessible. For the purposes of member lookup, generic methods and nested generic types have the number of type parameters indicated in their respective declarations and all other members have zero type parameters.
A member lookup of a name 
 with 
 type parameters in a type 
 is processed as follows:
First, a set of accessible members named 
 is determined:
If 
 is a type parameter, then the set is the union of the sets of accessible members named 
 in each of the types specified as a primary constraint or secondary constraint (
) for 
, along with the set of accessible members named 
 in 
.
Otherwise, the set consists of all accessible (
) members named 
 in 
, including inherited members and the accessible members named 
 in 
. If 
 is a constructed type, the set of members is obtained by substituting type arguments as described in 
. Members that include an 
 modifier are excluded from the set.
Next, if 
 is zero, all nested types whose declarations include type parameters are removed. If 
 is not zero, all members with a different number of type parameters are removed. Note that when 
 is zero, methods having type parameters are not removed, since the type inference process (
) might be able to infer the type arguments.
Next, if the member is 
invoked
, all non-
invocable
 members are removed from the set.
Next, members that are hidden by other members are removed from the set. For every member 
 in the set, where 
 is the type in which the member 
 is declared, the following rules are applied:
If 
 is a constant, field, property, event, or enumeration member, then all members declared in a base type of 
 are removed from the set.
If 
 is a type declaration, then all non-types declared in a base type of 
 are removed from the set, and all type declarations with the same number of type parameters as 
 declared in a base type of 
 are removed from the set.
If 
 is a method, then all non-method members declared in a base type of 
 are removed from the set.
Next, interface members that are hidden by class members are removed from the set. This step only has an effect if 
 is a type parameter and 
 has both an effective base class other than 
 and a non-empty effective interface set (
). For every member 
 in the set, where 
 is the type in which the member 
 is declared, the following rules are applied if 
 is a class declaration other than 
:
If 
 is a constant, field, property, event, enumeration member, or type declaration, then all members declared in an interface declaration are removed from the set.
If 
 is a method, then all non-method members declared in an interface declaration are removed from the set, and all methods with the same signature as 
 declared in an interface declaration are removed from the set.
Finally, having removed hidden members, the result of the lookup is determined:
If the set consists of a single member that is not a method, then this member is the result of the lookup.
Otherwise, if the set contains only methods, then this group of methods is the result of the lookup.
Otherwise, the lookup is ambiguous, and a binding-time error occurs.
For member lookups in types other than type parameters and interfaces, and member lookups in interfaces that are strictly single-inheritance (each interface in the inheritance chain has exactly zero or one direct base interface), the effect of the lookup rules is simply that derived members hide base members with the same name or signature. Such single-inheritance lookups are never ambiguous. The ambiguities that can possibly arise from member lookups in multiple-inheritance interfaces are described in 
.
Base types
For purposes of member lookup, a type 
 is considered to have the following base types:
If 
 is 
, then 
 has no base type.
If 
 is an 
, the base types of 
 are the class types 
, 
, and 
.
If 
 is a 
, the base types of 
 are the class types 
 and 
.
If 
 is a 
, the base types of 
 are the base classes of 
, including the class type 
.
If 
 is an 
, the base types of 
 are the base interfaces of 
 and the class type 
.
If 
 is an 
, the base types of 
 are the class types 
 and 
.
If 
 is a 
, the base types of 
 are the class types 
 and 
.
Function members
Function members are members that contain executable statements. Function members are always members of types and cannot be members of namespaces. C# defines the following categories of function members:
Methods
Properties
Events
Indexers
User-defined operators
Instance constructors
Static constructors
Destructors
Except for destructors and static constructors (which cannot be invoked explicitly), the statements contained in function members are executed through function member invocations. The actual syntax for writing a function member invocation depends on the particular function member category.
The argument list (
) of a function member invocation provides actual values or variable references for the parameters of the function member.
Invocations of generic methods may employ type inference to determine the set of type arguments to pass to the method. This process is described in 
.
Invocations of methods, indexers, operators and instance constructors employ overload resolution to determine which of a candidate set of function members to invoke. This process is described in 
.
Once a particular function member has been identified at binding-time, possibly through overload resolution, the actual run-time process of invoking the function member is described in 
.
The following table summarizes the processing that takes place in constructs involving the six categories of function members that can be explicitly invoked. In the table, 
, 
, 
, and 
 indicate expressions classified as variables or values, 
 indicates an expression classified as a type, 
 is the simple name of a method, and 
 is the simple name of a property.
Construct
Example
Description
Method invocation
Overload resolution is applied to select the best method 
 in the containing class or struct. The method is invoked with the argument list 
. If the method is not 
, the instance expression is 
.
Overload resolution is applied to select the best method 
 in the class or struct 
. A binding-time error occurs if the method is not 
. The method is invoked with the argument list 
.
Overload resolution is applied to select the best method F in the class, struct, or interface given by the type of 
. A binding-time error occurs if the method is 
. The method is invoked with the instance expression 
 and the argument list 
.
Property access
The 
 accessor of the property 
 in the containing class or struct is invoked. A compile-time error occurs if 
 is write-only. If 
 is not 
, the instance expression is 
.
The 
 accessor of the property 
 in the containing class or struct is invoked with the argument list 
. A compile-time error occurs if 
 is read-only. If 
 is not 
, the instance expression is 
.
The 
 accessor of the property 
 in the class or struct 
 is invoked. A compile-time error occurs if 
 is not 
 or if 
 is write-only.
The 
 accessor of the property 
 in the class or struct 
 is invoked with the argument list 
. A compile-time error occurs if 
 is not 
 or if 
 is read-only.
The 
 accessor of the property 
 in the class, struct, or interface given by the type of 
 is invoked with the instance expression 
. A binding-time error occurs if 
 is 
 or if 
 is write-only.
The 
 accessor of the property 
 in the class, struct, or interface given by the type of 
 is invoked with the instance expression 
 and the argument list 
. A binding-time error occurs if 
 is 
 or if 
 is read-only.
Event access
The 
 accessor of the event 
 in the containing class or struct is invoked. If 
 is not static, the instance expression is 
.
The 
 accessor of the event 
 in the containing class or struct is invoked. If 
 is not static, the instance expression is 
.
The 
 accessor of the event 
 in the class or struct 
 is invoked. A binding-time error occurs if 
 is not static.
The 
 accessor of the event 
 in the class or struct 
 is invoked. A binding-time error occurs if 
 is not static.
The 
 accessor of the event 
 in the class, struct, or interface given by the type of 
 is invoked with the instance expression 
. A binding-time error occurs if 
 is static.
The 
 accessor of the event 
 in the class, struct, or interface given by the type of 
 is invoked with the instance expression 
. A binding-time error occurs if 
 is static.
Indexer access
Overload resolution is applied to select the best indexer in the class, struct, or interface given by the type of e. The 
 accessor of the indexer is invoked with the instance expression 
 and the argument list 
. A binding-time error occurs if the indexer is write-only.
Overload resolution is applied to select the best indexer in the class, struct, or interface given by the type of 
. The 
 accessor of the indexer is invoked with the instance expression 
 and the argument list 
. A binding-time error occurs if the indexer is read-only.
Operator invocation
Overload resolution is applied to select the best unary operator in the class or struct given by the type of 
. The selected operator is invoked with the argument list 
.
Overload resolution is applied to select the best binary operator in the classes or structs given by the types of 
 and 
. The selected operator is invoked with the argument list 
.
Instance constructor invocation
Overload resolution is applied to select the best instance constructor in the class or struct 
. The instance constructor is invoked with the argument list 
.
Argument lists
Every function member and delegate invocation includes an argument list which provides actual values or variable references for the parameters of the function member. The syntax for specifying the argument list of a function member invocation depends on the function member category:
For instance constructors, methods, indexers and delegates, the arguments are specified as an 
, as described below. For indexers, when invoking the 
 accessor, the argument list additionally includes the expression specified as the right operand of the assignment operator.
For properties, the argument list is empty when invoking the 
 accessor, and consists of the expression specified as the right operand of the assignment operator when invoking the 
 accessor.
For events, the argument list consists of the expression specified as the right operand of the 
 or 
 operator.
For user-defined operators, the argument list consists of the single operand of the unary operator or the two operands of the binary operator.
The arguments of properties (
), events (
), and user-defined operators (
) are always passed as value parameters (
). The arguments of indexers (
) are always passed as value parameters (
) or parameter arrays (
). Reference and output parameters are not supported for these categories of function members.
The arguments of an instance constructor, method, indexer or delegate invocation are specified as an 
:
An 
 consists of one or more 
s, separated by commas. Each argument consists of an optional  
 followed by an 
. An 
 with an 
 is referred to as a 
, whereas an 
 without an 
 is a 
. It is an error for a positional argument to appear after a named argument in an 
.
The 
 can take one of the following forms:
An 
, indicating that the argument is passed as a value parameter (
).
The keyword 
 followed by a 
 (
), indicating that the argument is passed as a reference parameter (
). A variable must be definitely assigned (
) before it can be passed as a reference parameter. The keyword 
 followed by a 
 (
), indicating that the argument is passed as an output parameter (
). A variable is considered definitely assigned (
) following a function member invocation in which the variable is passed as an output parameter.
Corresponding parameters
For each argument in an argument list there has to be a corresponding parameter in the function member or delegate being invoked.
The parameter list used in the following is determined as follows:
For virtual methods and indexers defined in classes, the parameter list is picked from the most specific declaration or override of the function member, starting with the static type of the receiver, and searching through its base classes.
For interface methods and indexers, the parameter list is picked form the most specific definition of the member, starting with the interface type and searching through the base interfaces. If no unique parameter list is found, a parameter list with inaccessible names and no optional parameters is constructed, so that invocations cannot use named parameters or omit optional arguments.
For partial methods, the parameter list of the defining partial method declaration is used.
For all other function members and delegates there is only a single parameter list, which is the one used.
The position of an argument or parameter is defined as the number of arguments or parameters preceding it in the argument list or parameter list.
The corresponding parameters for function member arguments are established as follows:
Arguments in the 
 of instance constructors, methods, indexers and delegates:
A positional argument where a fixed parameter occurs at the same position in the parameter list corresponds to that parameter.
A positional argument of a function member with a parameter array invoked in its normal form corresponds to the parameter  array, which must occur at the same position in the parameter list.
A positional argument of a function member with a parameter array invoked in its expanded form, where no fixed parameter occurs at the same position in the parameter list, corresponds to an element in the parameter array.
A named argument corresponds to the parameter of the same name in the parameter list.
For indexers, when invoking the 
 accessor, the expression specified as the right operand of the assignment operator corresponds to the implicit 
 parameter of the 
 accessor declaration.
For properties, when invoking the 
 accessor there are no arguments. When invoking the 
 accessor, the expression specified as the right operand of the assignment operator corresponds to the implicit 
 parameter of the 
 accessor declaration.
For user-defined unary operators (including conversions), the single operand corresponds to the single parameter of the operator declaration.
For user-defined binary operators, the left operand corresponds to the first parameter, and the right operand corresponds to the second parameter of the operator declaration.
Run-time evaluation of argument lists
During the run-time processing of a function member invocation (
), the expressions or variable references of an argument list are evaluated in order, from left to right, as follows:
For a value parameter, the argument expression is evaluated and an implicit conversion (
) to the corresponding parameter type is performed. The resulting value becomes the initial value of the value parameter in the function member invocation.
For a reference or output parameter, the variable reference is evaluated and the resulting storage location becomes the storage location represented by the parameter in the function member invocation. If the variable reference given as a reference or output parameter is an array element of a 
, a run-time check is performed to ensure that the element type of the array is identical to the type of the parameter. If this check fails, a 
 is thrown.
Methods, indexers, and instance constructors may declare their right-most parameter to be a parameter array (
). Such function members are invoked either in their normal form or in their expanded form depending on which is applicable (
):
When a function member with a parameter array is invoked in its normal form, the argument given for the parameter array must be a single expression that is implicitly convertible (
) to the parameter array type. In this case, the parameter array acts precisely like a value parameter.
When a function member with a parameter array is invoked in its expanded form, the invocation must specify zero or more positional arguments for the parameter array, where each argument is an expression that is implicitly convertible (
) to the element type of the parameter array. In this case, the invocation creates an instance of the parameter array type with a length corresponding to the number of arguments, initializes the elements of the array instance with the given argument values, and uses the newly created array instance as the actual argument.
The expressions of an argument list are always evaluated in the order they are written. Thus, the example
produces the output
The array co-variance rules (
) permit a value of an array type 
 to be a reference to an instance of an array type 
, provided an implicit reference conversion exists from 
 to 
. Because of these rules, when an array element of a 
 is passed as a reference or output parameter, a run-time check is required to ensure that the actual element type of the array is identical to that of the parameter. In the example
the second invocation of 
 causes a 
 to be thrown because the actual element type of 
 is 
 and not 
.
When a function member with a parameter array is invoked in its expanded form, the invocation is processed exactly as if an array creation expression with an array initializer (
) was inserted around the expanded parameters. For example, given the declaration
the following invocations of the expanded form of the method
correspond exactly to
In particular, note that an empty array is created when there are zero arguments given for the parameter array.
When arguments are omitted from a function member with corresponding optional parameters, the default arguments of the function member declaration are implicitly passed. Because these are always constant, their evaluation will not impact the evaluation order of the remaining arguments.
Type inference
When a generic method is called without specifying type arguments, a 
 process attempts to infer type arguments for the call. The presence of type inference allows a more convenient syntax to be used for calling a generic method, and allows the programmer to avoid specifying redundant type information. For example, given the method declaration:
it is possible to invoke the 
 method without explicitly specifying a type argument:
Through type inference, the type arguments 
 and 
 are determined from the arguments to the method.
Type inference occurs as part of the binding-time processing of a method invocation (
) and takes place before the overload resolution step of the invocation. When a particular method group is specified in a method invocation, and no type arguments are specified as part of the method invocation, type inference is applied to each generic method in the method group. If type inference succeeds, then the inferred type arguments are used to determine the types of arguments for subsequent overload resolution. If overload resolution chooses a generic method as the one to invoke, then the inferred type arguments are used as the actual type arguments for the invocation. If type inference for a particular method fails, that method does not participate in overload resolution. The failure of type inference, in and of itself, does not cause a binding-time error. However, it often leads to a binding-time error when overload resolution then fails to find any applicable methods.
If the supplied number of arguments is different than the number of parameters in the method, then inference immediately fails. Otherwise, assume that the generic method has the following signature:
With a method call of the form 
 the task of type inference is to find unique type arguments 
 for each of the type parameters 
 so that the call 
 becomes valid.
During the process of inference each type parameter 
 is either 
fixed
 to a particular type 
 or 
unfixed
 with an associated set of 
bounds
. Each of the bounds is some type 
. Initially each type variable 
 is unfixed with an empty set of bounds.
Type inference takes place in phases. Each phase will try to infer type arguments for more type variables based on the findings of the previous phase. The first phase makes some initial inferences of bounds, whereas the second phase fixes type variables to specific types and infers further bounds. The second phase may have to be repeated a number of times.
Note:
 Type inference takes place not only when a generic method is called. Type inference for conversion of method groups is described in 
 and finding the best common type of a set of expressions is described in 
.
The first phase
For each of the method arguments 
:
If 
 is an anonymous function, an 
explicit parameter type inference
 (
) is made from 
 to 
Otherwise, if 
 has a type 
 and 
 is a value parameter then a 
lower-bound inference
 is made 
from
 
 
to
 
.
Otherwise, if 
 has a type 
 and 
 is a 
 or 
 parameter then an 
exact inference
 is made 
from
 
 
to
 
.
Otherwise, no inference is made for this argument.
The second phase
The second phase proceeds as follows:
All 
unfixed
 type variables 
 which do not 
depend on
 (
) any 
 are fixed (
).
If no such type variables exist, all 
unfixed
 type variables 
 are 
fixed
 for which all of the following hold:
There is at least one type variable 
 that depends on 
 has a non-empty set of bounds
If no such type variables exist and there are still 
unfixed
 type variables, type inference fails.
Otherwise, if no further 
unfixed
 type variables exist, type inference succeeds.
Otherwise, for all arguments 
 with corresponding parameter type 
 where the 
output types
 (
) contain 
unfixed
 type variables 
 but the 
input types
 (
) do not, an 
output type inference
 (
) is made 
from
 
 
to
 
. Then the second phase is repeated.
Input types
If 
 is a method group or implicitly typed anonymous function and 
 is a delegate type or expression tree type then all the parameter types of 
 are 
input types
 of 
 
with type
 
.
Output types
If 
 is a method group or an anonymous function and 
 is a delegate type or expression tree type then the return type of 
 is an 
output type of
 
 
with type
 
.
Dependence
An 
unfixed
 type variable 
 
depends directly on
 an unfixed type variable 
 if for some argument 
 with type 
 
 occurs in an 
input type
 of 
 with type 
 and 
 occurs in an 
output type
 of 
 with type 
.
 
depends on
 
 if 
 
depends directly on
 
 or if 
 
depends directly on
 
 and 
 
depends on
 
. Thus ""depends on"" is the transitive but not reflexive closure of ""depends directly on"".
Output type inferences
An 
output type inference
 is made 
from
 an expression 
 
to
 a type 
 in the following way:
If 
 is an anonymous function with inferred return type  
 (
) and 
 is a delegate type or expression tree type with return type 
, then a 
lower-bound inference
 (
) is made 
from
 
 
to
 
.
Otherwise, if 
 is a method group and 
 is a delegate type or expression tree type with parameter types 
 and return type 
, and overload resolution of 
 with the types 
 yields a single method with return type 
, then a 
lower-bound inference
 is made 
from
 
 
to
 
.
Otherwise, if 
 is an expression with type 
, then a 
lower-bound inference
 is made 
from
 
 
to
 
.
Otherwise, no inferences are made.
Explicit parameter type inferences
An 
explicit parameter type inference
 is made 
from
 an expression 
 
to
 a type 
 in the following way:
If 
 is an explicitly typed anonymous function with parameter types 
 and 
 is a delegate type or expression tree type with parameter types 
 then for each 
 an 
exact inference
 (
) is made 
from
 
 
to
 the corresponding 
.
Exact inferences
An 
exact inference
 
from
 a type 
 
to
 a type 
 is made as follows:
If 
 is one of the 
unfixed
 
 then 
 is added to the set of exact bounds for 
.
Otherwise, sets 
 and 
 are determined by checking if any of the following cases apply:
 is an array type 
 and 
 is an array type 
  of the same rank
 is the type 
 and 
 is the type 
 is a constructed type 
and 
 is a constructed type 
If any of these cases apply then an 
exact inference
 is made 
from
 each 
 
to
 the corresponding 
.
Otherwise no inferences are made.
Lower-bound inferences
A 
lower-bound inference
 
from
 a type 
 
to
 a type 
 is made as follows:
If 
 is one of the 
unfixed
 
 then 
 is added to the set of lower bounds for 
.
Otherwise, if 
 is the type 
and 
 is the type 
 then a lower bound inference is made from 
 to 
.
Otherwise, sets 
 and 
 are determined by checking if any of the following cases apply:
 is an array type 
 and 
 is an array type 
 (or a type parameter whose effective base type is 
) of the same rank
 is one of 
, 
 or 
 and 
 is a one-dimensional array type 
(or a type parameter whose effective base type is 
)
 is a constructed class, struct, interface or delegate type 
 and there is a unique type 
 such that 
 (or, if 
 is a type parameter, its effective base class or any member of its effective interface set) is identical to, inherits from (directly or indirectly), or implements (directly or indirectly) 
.
(The ""uniqueness"" restriction means that in the case interface 
, then no inference is made when inferring from 
 to 
 because 
 could be 
 or 
.)
If any of these cases apply then an inference is made 
from
 each 
 
to
 the corresponding 
 as follows:
If 
 is not known to be a reference type then an 
exact inference
 is made
Otherwise, if 
 is an array type then a 
lower-bound inference
 is made
Otherwise, if 
 is 
 then inference depends on the i-th type parameter of 
:
If it is covariant then a 
lower-bound inference
 is made.
If it is contravariant then an 
upper-bound inference
 is made.
If it is invariant then an 
exact inference
 is made.
Otherwise, no inferences are made.
Upper-bound inferences
An 
upper-bound inference
 
from
 a type 
 
to
 a type 
 is made as follows:
If 
 is one of the 
unfixed
 
 then 
 is added to the set of upper bounds for 
.
Otherwise, sets 
 and 
 are determined by checking if any of the following cases apply:
 is an array type 
 and 
 is an array type 
 of the same rank
 is one of 
, 
 or 
 and 
 is a one-dimensional array type 
 is the type 
 and 
 is the type 
 is constructed class, struct, interface or delegate type 
 and 
 is a class, struct, interface or delegate type which is identical to, inherits from (directly or indirectly), or implements (directly or indirectly) a unique type 
(The ""uniqueness"" restriction means that if we have 
, then no inference is made when inferring from 
 to 
. Inferences are not made from 
 to either 
 or 
.)
If any of these cases apply then an inference is made 
from
 each 
 
to
 the corresponding 
 as follows:
If  
 is not known to be a reference type then an 
exact inference
 is made
Otherwise, if 
 is an array type then an 
upper-bound inference
 is made
Otherwise, if 
 is 
 then inference depends on the i-th type parameter of 
:
If it is covariant then an 
upper-bound inference
 is made.
If it is contravariant then a 
lower-bound inference
 is made.
If it is invariant then an 
exact inference
 is made.
Otherwise, no inferences are made.
Fixing
An 
unfixed
 type variable 
 with a set of bounds is 
fixed
 as follows:
The set of 
candidate types
 
 starts out as the set of all types in the set of bounds for 
.
We then examine each bound for 
 in turn: For each exact bound 
 of 
 all types 
 which are not identical to 
 are removed from the candidate set. For each lower bound 
 of 
 all types 
 to which there is 
not
 an implicit conversion from 
 are removed from the candidate set. For each upper bound 
 of 
 all types 
 from which there is 
not
 an implicit conversion to 
 are removed from the candidate set.
If among the remaining candidate types 
 there is a unique type 
 from which there is an implicit conversion to all the other candidate types, then 
 is fixed to 
.
Otherwise, type inference fails.
Inferred return type
The inferred return type of an anonymous function 
 is used during type inference and overload resolution. The inferred return type can only be determined for an anonymous function where all parameter types are known, either because they are explicitly given, provided through an anonymous function conversion or inferred during type inference on an enclosing generic method invocation.
The 
 is determined as follows:
If the body of 
 is an 
 that has a type, then the inferred result type of 
 is the type of that expression.
If the body of 
 is a 
 and the set of expressions in the block's 
 statements has a best common type 
 (
), then the inferred result type of 
 is 
.
Otherwise, a result type cannot be inferred for 
.
The 
 is determined as follows:
If 
 is async and the body of 
 is either an expression classified as nothing (
), or a statement block where no return statements have expressions, the inferred return type is 
If 
 is async and has an inferred result type 
, the inferred return type is 
.
If 
 is non-async and has an inferred result type 
, the inferred return type is 
.
Otherwise a return type cannot be inferred for 
.
As an example of type inference involving anonymous functions, consider the 
 extension method declared in the 
 class:
Assuming the 
 namespace was imported with a 
 clause, and given a class 
 with a 
 property of type 
, the 
 method can be used to select the names of a list of customers:
The extension method invocation (
) of 
 is processed by rewriting the invocation to a static method invocation:
Since type arguments were not explicitly specified, type inference is used to infer the type arguments. First, the 
 argument is related to the 
 parameter, inferring 
 to be 
. Then, using the anonymous function type inference process described above, 
 is given type 
, and the expression 
 is related to the return type of the 
 parameter, inferring 
 to be 
. Thus, the invocation is equivalent to
and the result is of type 
.
The following example demonstrates how anonymous function type inference allows type information to ""flow"" between arguments in a generic method invocation. Given the method:
Type inference for the invocation:
proceeds as follows: First, the argument 
 is related to the 
 parameter, inferring 
 to be 
. Then, the parameter of the first anonymous function, 
, is given the inferred type 
, and the expression 
 is related to the return type of 
, inferring 
 to be 
. Finally, the parameter of the second anonymous function, 
, is given the inferred type 
, and the expression 
 is related to the return type of 
, inferring 
 to be 
. Thus, the result of the invocation is of type 
.
Type inference for conversion of method groups
Similar to calls of generic methods, type inference must also be applied when a method group 
 containing a generic method is converted to a given delegate type 
 (
). Given a method
and the method group 
 being assigned to the delegate type 
 the task of type inference is to find type arguments 
 so that the expression:
becomes compatible (
) with 
.
Unlike the type inference algorithm for generic method calls, in this case there are only argument 
types
, no argument 
expressions
. In particular, there are no anonymous functions and hence no need for multiple phases of inference.
Instead, all 
 are considered 
unfixed
, and a 
lower-bound inference
 is made 
from
 each argument type 
 of 
 
to
 the corresponding parameter type 
 of 
. If for any of the 
 no bounds were found, type inference fails. Otherwise, all 
 are 
fixed
 to corresponding 
, which are the result of type inference.
Finding the best common type of a set of expressions
In some cases, a common type needs to be inferred for a set of expressions. In particular, the element types of implicitly typed arrays and the return types of anonymous functions with 
 bodies are found in this way.
Intuitively, given a set of expressions 
 this inference should be equivalent to calling a method
with the 
 as arguments.
More precisely, the inference starts out with an 
unfixed
 type variable 
. 
Output type inferences
 are then made 
from
 each 
 
to
 
. Finally, 
 is 
fixed
 and, if successful, the resulting type 
 is the resulting best common type for the expressions. If no such 
 exists, the expressions have no best common type.
Overload resolution
Overload resolution is a binding-time mechanism for selecting the best function member to invoke given an argument list and a set of candidate function members. Overload resolution selects the function member to invoke in the following distinct contexts within C#:
Invocation of a method named in an 
 (
).
Invocation of an instance constructor named in an 
 (
).
Invocation of an indexer accessor through an 
 (
).
Invocation of a predefined or user-defined operator referenced in an expression (
 and 
).
Each of these contexts defines the set of candidate function members and the list of arguments in its own unique way, as described in detail in the sections listed above. For example, the set of candidates for a method invocation does not include methods marked 
 (
), and methods in a base class are not candidates if any method in a derived class is applicable (
).
Once the candidate function members and the argument list have been identified, the selection of the best function member is the same in all cases:
Given the set of applicable candidate function members, the best function member in that set is located. If the set contains only one function member, then that function member is the best function member. Otherwise, the best function member is the one function member that is better than all other function members with respect to the given argument list, provided that each function member is compared to all other function members using the rules in 
. If there is not exactly one function member that is better than all other function members, then the function member invocation is ambiguous and a binding-time error occurs.
The following sections define the exact meanings of the terms 
 and 
.
Applicable function member
A function member is said to be an 
 with respect to an argument list 
 when all of the following are true:
Each argument in 
 corresponds to a parameter in the function member declaration as described in 
, and any parameter to which no argument corresponds is an optional parameter.
For each argument in 
, the parameter passing mode of the argument (i.e., value, 
, or 
) is identical to the parameter passing mode of the corresponding parameter, and
for a value parameter or a parameter array, an implicit conversion (
) exists from the argument to the type of the corresponding parameter, or
for a 
 or 
 parameter, the type of the argument is identical to the type of the corresponding parameter. After all, a 
 or 
 parameter is an alias for the argument passed.
For a function member that includes a parameter array, if the function member is applicable by the above rules, it is said to be applicable in its 
. If a function member that includes a parameter array is not applicable in its normal form, the function member may instead be applicable in its 
:
The expanded form is constructed by replacing the parameter array in the function member declaration with zero or more value parameters of the element type of the parameter array such that the number of arguments in the argument list 
 matches the total number of parameters. If 
 has fewer arguments than the number of fixed parameters in the function member declaration, the expanded form of the function member cannot be constructed and is thus not applicable.
Otherwise, the expanded form is applicable if for each argument in 
 the parameter passing mode of the argument is identical to the parameter passing mode of the corresponding parameter, and
for a fixed value parameter or a value parameter created by the expansion, an implicit conversion (
) exists from the type of the argument to the type of the corresponding parameter, or
for a 
 or 
 parameter, the type of the argument is identical to the type of the corresponding parameter.
Better function member
For the purposes of determining the better function member, a stripped-down argument list A is constructed containing just the argument expressions themselves in the order they appear in the original argument list.
Parameter lists for each of the candidate function members are constructed in the following way:
The expanded form is used if the function member was applicable only in the expanded form.
Optional parameters with no corresponding arguments are removed from the parameter list
The parameters are reordered so that they occur at the same position as the corresponding argument in the argument list.
Given an argument list 
 with a set of argument expressions 
 and two applicable function members 
 and 
 with parameter types 
 and 
, 
 is defined to be a 
 than 
 if
for each argument, the implicit conversion from 
 to 
 is not better than the implicit conversion from 
 to 
, and
for at least one argument, the conversion from 
 to 
 is better than the conversion from 
 to 
.
When performing this evaluation, if 
 or 
 is applicable in its expanded form, then 
 or 
 refers to a parameter in the expanded form of the parameter list.
In case the parameter type sequences 
 and 
 are equivalent (i.e. each 
 has an identity conversion to the corresponding 
), the following tie-breaking rules are applied, in order, to determine the better function member.
If 
 is a non-generic method and 
 is a generic method, then 
 is better than 
.
Otherwise, if 
 is applicable in its normal form and 
 has a 
 array and is applicable only in its expanded form, then 
 is better than 
.
Otherwise, if 
 has more declared parameters than 
, then 
 is better than 
. This can occur if both methods have 
 arrays and are applicable only in their expanded forms.
Otherwise if all parameters of 
 have a corresponding argument whereas default arguments need to be substituted for at least one optional parameter in 
 then 
 is better than 
.
Otherwise, if 
 has more specific parameter types than 
, then 
 is better than 
. Let 
 and 
 represent the uninstantiated and unexpanded parameter types of 
 and 
. 
's parameter types are more specific than 
's if, for each parameter, 
 is not less specific than 
, and, for at least one parameter, 
 is more specific than 
:
A type parameter is less specific than a non-type parameter.
Recursively, a constructed type is more specific than another constructed type (with the same number of type arguments) if at least one type argument is more specific and no type argument is less specific than the corresponding type argument in the other.
An array type is more specific than another array type (with the same number of dimensions) if the element type of the first is more specific than the element type of the second.
Otherwise if one member is a non-lifted operator and  the other is a lifted operator, the non-lifted one is better.
Otherwise, neither function member is better.
Better conversion from expression
Given an implicit conversion 
 that converts from an expression 
 to a type 
, and an implicit conversion 
 that converts from an expression 
 to a type 
, 
 is a 
 than 
 if 
 does not exactly match 
 and at least one of the following holds:
 exactly matches 
 (
)
 is a better conversion target than 
 (
)
Exactly matching Expression
Given an expression 
 and a type 
, 
 exactly matches 
 if one of the following holds:
 has a type 
, and an identity conversion exists from 
 to 
 is an anonymous function, 
 is either a delegate type 
 or an expression tree type 
 and one of the following holds:
An inferred return type 
 exists for 
 in the context of the parameter list of 
 (
), and an identity conversion exists from 
 to the return type of 
Either 
 is non-async and 
 has a return type 
 or 
 is async and 
 has a return type 
, and one of the following holds:
The body of 
 is an expression that exactly matches 
The body of 
 is a statement block where every return statement returns an expression that exactly matches 
Better conversion target
Given two different types 
 and 
, 
 is a better conversion target than 
 if no implicit conversion from 
 to 
 exists, and at least one of the following holds:
An implicit conversion from 
 to 
 exists
 is either a delegate type 
 or an expression tree type 
, 
 is either a delegate type 
 or an expression tree type 
, 
 has a return type 
 and one of the following holds:
 is void returning
 has a return type 
, and 
 is a better conversion target than 
 is 
, 
 is 
, and 
 is a better conversion target than 
 is 
 or 
 where 
 is a signed integral type, and 
 is 
 or 
 where 
 is an unsigned integral type. Specifically:
 is 
 and 
 is 
, 
, 
, or 
 is 
 and 
 is 
, 
, or 
 is 
 and 
 is 
, or 
 is 
 and 
 is 
Overloading in generic classes
While signatures as declared must be unique, it is possible that substitution of type arguments results in identical signatures. In such cases, the tie-breaking rules of overload resolution above will pick the most specific member.
The following examples show overloads that are valid and invalid according to this rule:
Compile-time checking of dynamic overload resolution
For most dynamically bound operations the set of possible candidates for resolution is unknown at compile-time. In certain cases, however the candidate set is known at compile-time:
Static method calls with dynamic arguments
Instance method calls where the receiver is not a dynamic expression
Indexer calls where the receiver is not a dynamic expression
Constructor calls with dynamic arguments
In these cases a limited compile-time check is performed for each candidate to see if any of them could possibly apply at run-time.This check consists of the following steps:
Partial type inference: Any type argument that does not depend directly or indirectly on an argument of type 
 is inferred using the rules of 
. The remaining type arguments are unknown.
Partial applicability check: Applicability is checked according to 
, but ignoring parameters whose types are unknown.
If no candidate passes this test, a compile-time error occurs.
Function member invocation
This section describes the process that takes place at run-time to invoke a particular function member. It is assumed that a binding-time process has already determined the particular member to invoke, possibly by applying overload resolution to a set of candidate function members.
For purposes of describing the invocation process, function members are divided into two categories:
Static function members. These are instance constructors, static methods, static property accessors, and user-defined operators. Static function members are always non-virtual.
Instance function members. These are instance methods, instance property accessors, and indexer accessors. Instance function members are either non-virtual or virtual, and are always invoked on a particular instance. The instance is computed by an instance expression, and it becomes accessible within the function member as 
 (
).
The run-time processing of a function member invocation consists of the following steps, where 
 is the function member and, if 
 is an instance member, 
 is the instance expression:
If 
 is a static function member:
The argument list is evaluated as described in 
.
 is invoked.
If 
 is an instance function member declared in a 
:
 is evaluated. If this evaluation causes an exception, then no further steps are executed.
If 
 is not classified as a variable, then a temporary local variable of 
's type is created and the value of 
 is assigned to that variable. 
 is then reclassified as a reference to that temporary local variable. The temporary variable is accessible as 
 within 
, but not in any other way. Thus, only when 
 is a true variable is it possible for the caller to observe the changes that 
 makes to 
.
The argument list is evaluated as described in 
.
 is invoked. The variable referenced by 
 becomes the variable referenced by 
.
If 
 is an instance function member declared in a 
:
 is evaluated. If this evaluation causes an exception, then no further steps are executed.
The argument list is evaluated as described in 
.
If the type of 
 is a 
, a boxing conversion (
) is performed to convert 
 to type 
, and 
 is considered to be of type 
 in the following steps. In this case, 
 could only be a member of 
.
The value of 
 is checked to be valid. If the value of 
 is 
, a 
 is thrown and no further steps are executed.
The function member implementation to invoke is determined:
If the binding-time type of 
 is an interface, the function member to invoke is the implementation of 
 provided by the run-time type of the instance referenced by 
. This function member is determined by applying the interface mapping rules (
) to determine the implementation of 
 provided by the run-time type of the instance referenced by 
.
Otherwise, if 
 is a virtual function member, the function member to invoke is the implementation of 
 provided by the run-time type of the instance referenced by 
. This function member is determined by applying the rules for determining the most derived implementation (
) of 
 with respect to the run-time type of the instance referenced by 
.
Otherwise, 
 is a non-virtual function member, and the function member to invoke is 
 itself.
The function member implementation determined in the step above is invoked. The object referenced by 
 becomes the object referenced by 
.
Invocations on boxed instances
A function member implemented in a 
 can be invoked through a boxed instance of that 
 in the following situations:
When the function member is an 
 of a method inherited from type 
 and is invoked through an instance expression of type 
.
When the function member is an implementation of an interface function member and is invoked through an instance expression of an 
.
When the function member is invoked through a delegate.
In these situations, the boxed instance is considered to contain a variable of the 
, and this variable becomes the variable referenced by 
 within the function member invocation. In particular, this means that when a function member is invoked on a boxed instance, it is possible for the function member to modify the value contained in the boxed instance.
Primary expressions
Primary expressions include the simplest forms of expressions.
Primary expressions are divided between 
s and 
s. Treating array-creation-expression in this way, rather than listing it along with the other simple expression forms, enables the grammar to disallow potentially confusing code such as
which would otherwise be interpreted as
Literals
A 
 that consists of a 
 (
) is classified as a value.
Interpolated strings
An 
 consists of a 
 sign followed by a regular or verbatim string literal, wherein holes, delimited by 
 and 
, enclose expressions and formatting specifications. An interpolated string expression is the result of an 
 that has been broken up into individual tokens, as described in 
.
The 
 in an interpolation must have an implicit conversion to 
.
An 
 is classified as a value. If it is immediately converted to 
 or 
 with an implicit interpolated string conversion (
), the interpolated string expression has that type. Otherwise, it has the type 
.
If the type of an interpolated string is 
 or 
, the meaning is a call to 
. If the type is 
, the meaning of the expression is a call to 
. In both cases, the argument list of the call consists of a format string literal with placeholders for each interpolation, and an argument for each expression corresponding to the place holders.
The format string literal is constructed as follows, where 
 is the number of interpolations in the 
:
If an 
 or an 
 follows the 
 sign, then the format string literal is that token.
Otherwise, the format string literal consists of:
First the 
 or 
Then for each number 
 from 
 to 
:
The decimal representation of 
Then, if the corresponding 
 has a 
, a 
 (comma) followed by the decimal representation of the value of the 
Then the 
, 
, 
 or 
 immediately following the corresponding interpolation.
The subsequent arguments are simply the 
expressions
 from the 
interpolations
 (if any), in order.
TODO: examples.
Simple names
A 
 consists of an identifier, optionally followed by a type argument list:
A 
 is either of the form 
 or of the form 
, where 
 is a single identifier and 
 is an optional 
. When no 
 is specified, consider 
 to be zero. The 
 is evaluated and classified as follows:
If 
 is zero and the 
 appears within a 
 and if the 
's (or an enclosing 
's) local variable declaration space (
) contains a local variable, parameter or constant with name 
, then the 
 refers to that local variable, parameter or constant and is classified as a variable or value.
If 
 is zero and the 
 appears within the body of a generic method declaration and if that declaration includes a type parameter with name 
, then the 
 refers to that type parameter.
Otherwise, for each instance type 
 (
), starting with the instance type of the immediately enclosing type declaration and continuing with the instance type of each enclosing class or struct declaration (if any):
If 
 is zero and the declaration of 
 includes a type parameter with name 
, then the 
 refers to that type parameter.
Otherwise, if a member lookup (
) of 
 in 
 with 
 type arguments produces a match:
If 
 is the instance type of the immediately enclosing class or struct type and the lookup identifies one or more methods, the result is a method group with an associated instance expression of 
. If a type argument list was specified, it is used in calling a generic method (
).
Otherwise, if 
 is the instance type of the immediately enclosing class or struct type, if the lookup identifies an instance member, and if the reference occurs within the body of an instance constructor, an instance method, or an instance accessor, the result is the same as a member access (
) of the form 
. This can only happen when 
 is zero.
Otherwise, the result is the same as a member access (
) of the form 
 or 
. In this case, it is a binding-time error for the 
 to refer to an instance member.
Otherwise, for each namespace 
, starting with the namespace in which the 
 occurs, continuing with each enclosing namespace (if any), and ending with the global namespace, the following steps are evaluated until an entity is located:
If 
 is zero and 
 is the name of a namespace in 
, then:
If the location where the 
 occurs is enclosed by a namespace declaration for 
 and the namespace declaration contains an 
 or 
 that associates the name 
 with a namespace or type, then the 
 is ambiguous and a compile-time error occurs.
Otherwise, the 
 refers to the namespace named 
 in 
.
Otherwise, if 
 contains an accessible type having name 
 and 
 type parameters, then:
If 
 is zero and the location where the 
 occurs is enclosed by a namespace declaration for 
 and the namespace declaration contains an 
 or 
 that associates the name 
 with a namespace or type, then the 
 is ambiguous and a compile-time error occurs.
Otherwise, the 
 refers to the type constructed with the given type arguments.
Otherwise, if the location where the 
 occurs is enclosed by a namespace declaration for 
:
If 
 is zero and the namespace declaration contains an 
 or 
 that associates the name 
 with an imported namespace or type, then the 
 refers to that namespace or type.
Otherwise, if the namespaces and type declarations imported by the 
s and 
s of the namespace declaration contain exactly one accessible type or non-extension static membre having name 
 and 
 type parameters, then the 
 refers to that type or member constructed with the given type arguments.
Otherwise, if the namespaces and types imported by the 
s of the namespace declaration contain more than one accessible type or non-extension-method static member having name 
 and 
 type parameters, then the 
 is ambiguous and an error occurs.
Note that this entire step is exactly parallel to the corresponding step in the processing of a 
 (
).
Otherwise, the 
 is undefined and a compile-time error occurs.
Parenthesized expressions
A 
 consists of an 
 enclosed in parentheses.
A 
 is evaluated by evaluating the 
 within the parentheses. If the 
 within the parentheses denotes a namespace or type, a compile-time error occurs. Otherwise, the result of the 
 is the result of the evaluation of the contained 
.
Member access
A 
 consists of a 
, a 
, or a 
, followed by a ""
"" token, followed by an 
, optionally followed by a 
.
The 
 production is defined in 
.
A 
 is either of the form 
 or of the form 
, where 
 is a primary-expression, 
 is a single identifier and 
 is an optional 
. When no 
 is specified, consider 
 to be zero.
A 
 with a 
 of type 
 is dynamically bound (
). In this case the compiler classifies the member access as a property access of type 
. The rules below to determine the meaning of the 
 are then applied at run-time, using the run-time type instead of the compile-time type of the 
. If this run-time classification leads to a method group, then the member access must be the 
 of an 
.
The 
 is evaluated and classified as follows:
If 
 is zero and 
 is a namespace and 
 contains a nested namespace with name 
, then the result is that namespace.
Otherwise, if 
 is a namespace and 
 contains an accessible type having name 
 and 
 type parameters, then the result is that type constructed with the given type arguments.
If 
 is a 
 or a 
 classified as a type, if 
 is not a type parameter, and if a member lookup (
) of 
 in 
 with 
 type parameters produces a match, then 
 is evaluated and classified as follows:
If 
 identifies a type, then the result is that type constructed with the given type arguments.
If 
 identifies one or more methods, then the result is a method group with no associated instance expression. If a type argument list was specified, it is used in calling a generic method (
).
If 
 identifies a 
 property, then the result is a property access with no associated instance expression.
If 
 identifies a 
 field:
If the field is 
 and the reference occurs outside the static constructor of the class or struct in which the field is declared, then the result is a value, namely the value of the static field 
 in 
.
Otherwise, the result is a variable, namely the static field 
 in 
.
If 
 identifies a 
 event:
If the reference occurs within the class or struct in which the event is declared, and the event was declared without 
 (
), then 
 is processed exactly as if 
 were a static field.
Otherwise, the result is an event access with no associated instance expression.
If 
 identifies a constant, then the result is a value, namely the value of that constant.
If 
 identifies an enumeration member, then the result is a value, namely the value of that enumeration member.
Otherwise, 
 is an invalid member reference, and a compile-time error occurs.
If 
 is a property access, indexer access, variable, or value, the type of which is 
, and a member lookup (
) of 
 in 
 with 
 type arguments produces a match, then 
 is evaluated and classified as follows:
First, if 
 is a property or indexer access, then the value of the property or indexer access is obtained (
) and 
 is reclassified as a value.
If 
 identifies one or more methods, then the result is a method group with an associated instance expression of 
. If a type argument list was specified, it is used in calling a generic method (
).
If 
 identifies an instance property,
If 
 is 
, 
 identifies an automatically implemented property (
) without a setter, and the reference occurs within an instance constructor for a class or struct type 
, then the result is a variable, namely the hidden backing field for the auto-property given by 
 in the instance of 
 given by 
.
Otherwise, the result is a property access with an associated instance expression of 
.
If 
 is a 
 and 
 identifies an instance field of that 
:
If the value of 
 is 
, then a 
 is thrown.
Otherwise, if the field is 
 and the reference occurs outside an instance constructor of the class in which the field is declared, then the result is a value, namely the value of the field 
 in the object referenced by 
.
Otherwise, the result is a variable, namely the field 
 in the object referenced by 
.
If 
 is a 
 and 
 identifies an instance field of that 
:
If 
 is a value, or if the field is 
 and the reference occurs outside an instance constructor of the struct in which the field is declared, then the result is a value, namely the value of the field 
 in the struct instance given by 
.
Otherwise, the result is a variable, namely the field 
 in the struct instance given by 
.
If 
 identifies an instance event:
If the reference occurs within the class or struct in which the event is declared, and the event was declared without 
 (
), and the reference does not occur as the left-hand side of a 
 or 
 operator, then 
 is processed exactly as if 
 was an instance field.
Otherwise, the result is an event access with an associated instance expression of 
.
Otherwise, an attempt is made to process 
 as an extension method invocation (
). If this fails, 
 is an invalid member reference, and a binding-time error occurs.
Identical simple names and type names
In a member access of the form 
, if 
 is a single identifier, and if the meaning of 
 as a 
 (
) is a constant, field, property, local variable, or parameter with the same type as the meaning of 
 as a 
 (
), then both possible meanings of 
 are permitted. The two possible meanings of 
 are never ambiguous, since 
 must necessarily be a member of the type 
 in both cases. In other words, the rule simply permits access to the static members and nested types of 
 where a compile-time error would otherwise have occurred. For example:
Grammar ambiguities
The productions for 
 (
) and 
 (
) can give rise to ambiguities in the grammar for expressions. For example, the statement:
could be interpreted as a call to 
 with two arguments, 
 and 
. Alternatively, it could be interpreted as a call to 
 with one argument, which is a call to a generic method 
 with two type arguments and one regular argument.
If a sequence of tokens can be parsed (in context) as a 
 (
), 
 (
), or 
 (
) ending with a 
 (
), the token immediately following the closing 
 token is examined. If it is one of
then the 
 is retained as part of the 
, 
 or 
 and any other possible parse of the sequence of tokens is discarded. Otherwise, the 
 is not considered to be part of the 
, 
 or 
, even if there is no other possible parse of the sequence of tokens. Note that these rules are not applied when parsing a 
 in a 
 (
). The statement
will, according to this rule, be interpreted as a call to 
 with one argument, which is a call to a generic method 
 with two type arguments and one regular argument. The statements
will each be interpreted as a call to 
 with two arguments. The statement
will be interpreted as a less than operator, greater than operator, and unary plus operator, as if the statement had been written 
, instead of as a 
 with a 
 followed by a binary plus operator. In the statement
the tokens 
 are interpreted as a 
 with a 
.
Invocation expressions
An 
 is used to invoke a method.
An 
 is dynamically bound (
) if at least one of the following holds:
The 
 has compile-time type 
.
At least one argument of the optional 
 has compile-time type 
 and the 
 does not have a delegate type.
In this case the compiler classifies the 
 as a value of type 
. The rules below to determine the meaning of the 
 are then applied at run-time, using the run-time type instead of the compile-time type of those of the 
 and arguments which have the compile-time type 
. If the 
 does not have compile-time type 
, then the method invocation undergoes a limited compile time check as described in 
.
The 
 of an 
 must be a method group or a value of a 
. If the 
 is a method group, the 
 is a method invocation (
). If the 
 is a value of a 
, the 
 is a delegate invocation (
). If the 
 is neither a method group nor a value of a 
, a binding-time error occurs.
The optional 
 (
) provides values or variable references for the parameters of the method.
The result of evaluating an 
 is classified as follows:
If the 
 invokes a method or delegate that returns 
, the result is nothing. An expression that is classified as nothing is permitted only in the context of a 
 (
) or as the body of a 
 (
). Otherwise a binding-time error occurs.
Otherwise, the result is a value of the type returned by the method or delegate.
Method invocations
For a method invocation, the 
 of the 
 must be a method group. The method group identifies the one method to invoke or the set of overloaded methods from which to choose a specific method to invoke. In the latter case, determination of the specific method to invoke is based on the context provided by the types of the arguments in the 
.
The binding-time processing of a method invocation of the form 
, where 
 is a method group (possibly including a 
), and 
 is an optional 
, consists of the following steps:
The set of candidate methods for the method invocation is constructed. For each method 
 associated with the method group 
:
If 
 is non-generic, 
 is a candidate when:
 has no type argument list, and
 is applicable with respect to 
 (
).
If 
 is generic and 
 has no type argument list, 
 is a candidate when:
Type inference (
) succeeds, inferring a list of type arguments for the call, and
Once the inferred type arguments are substituted for the corresponding method type parameters, all constructed types in the parameter list of F satisfy their constraints (
), and the parameter list of 
 is applicable with respect to 
 (
).
If 
 is generic and 
 includes a type argument list, 
 is a candidate when:
 has the same number of method type parameters as were supplied in the type argument list, and
Once the type arguments are substituted for the corresponding method type parameters, all constructed types in the parameter list of F satisfy their constraints (
), and the parameter list of 
 is applicable with respect to 
 (
).
The set of candidate methods is reduced to contain only methods from the most derived types: For each method 
 in the set, where 
 is the type in which the method 
 is declared, all methods declared in a base type of 
 are removed from the set. Furthermore, if 
 is a class type other than 
, all methods declared in an interface type are removed from the set. (This latter rule only has affect when the method group was the result of a member lookup on a type parameter having an effective base class other than object and a non-empty effective interface set.)
If the resulting set of candidate methods is empty, then further processing along the following steps are abandoned, and instead an attempt is made to process the invocation as an extension method invocation (
). If this fails, then no applicable methods exist, and a binding-time error occurs.
The best method of the set of candidate methods is identified using the overload resolution rules of 
. If a single best method cannot be identified, the method invocation is ambiguous, and a binding-time error occurs. When performing overload resolution, the parameters of a generic method are considered after substituting the type arguments (supplied or inferred) for the corresponding method type parameters.
Final validation of the chosen best method is performed:
The method is validated in the context of the method group: If the best method is a static method, the method group must have resulted from a 
 or a 
 through a type. If the best method is an instance method, the method group must have resulted from a 
, a 
 through a variable or value, or a 
. If neither of these requirements is true, a binding-time error occurs.
If the best method is a generic method, the type arguments (supplied or inferred) are checked against the constraints (
) declared on the generic method. If any type argument does not satisfy the corresponding constraint(s) on the type parameter, a binding-time error occurs.
Once a method has been selected and validated at binding-time by the above steps, the actual run-time invocation is processed according to the rules of function member invocation described in 
.
The intuitive effect of the resolution rules described above is as follows: To locate the particular method invoked by a method invocation, start with the type indicated by the method invocation and proceed up the inheritance chain until at least one applicable, accessible, non-override method declaration is found. Then perform type inference and overload resolution on the set of applicable, accessible, non-override methods declared in that type and invoke the method thus selected. If no method was found, try instead to process the invocation as an extension method invocation.
Extension method invocations
In a method invocation (
) of one of the forms
if the normal processing of the invocation finds no applicable methods, an attempt is made to process the construct as an extension method invocation. If 
expr
 or any of the 
args
 has compile-time type 
, extension methods will not apply.
The objective is to find the best 
 
, so that the corresponding static method invocation can take place:
An extension method 
 is 
 if:
 is a non-generic, non-nested class
The name of 
 is 
 is accessible and applicable when applied to the arguments as a static method as shown above
An implicit identity, reference or boxing conversion exists from 
expr
 to the type of the first parameter of 
.
The search for 
 proceeds as follows:
Starting with the closest enclosing namespace declaration, continuing with each enclosing namespace declaration, and ending with the containing compilation unit, successive attempts are made to find a candidate set of extension methods:
If the given namespace or compilation unit directly contains non-generic type declarations 
 with eligible extension methods 
, then the set of those extension methods is the candidate set.
If types 
 imported by 
using_static_declarations
 and directly declared in namespaces imported by 
s in the given namespace or compilation unit directly contain eligible extension methods 
, then the set of those extension methods is the candidate set.
If no candidate set is found in any enclosing namespace declaration or compilation unit, a compile-time error occurs.
Otherwise, overload resolution is applied to the candidate set as described in (
). If no single best method is found, a compile-time error occurs.
 is the type within which the best method is declared as an extension method.
Using 
 as a target, the method call is then processed as a static method invocation (
).
The preceding rules mean that instance methods take precedence over extension methods, that extension methods available in inner namespace declarations take precedence over extension methods available in outer namespace declarations, and that extension methods declared directly in a namespace take precedence over extension methods imported into that same namespace with a using namespace directive. For example:
In the example, 
's method takes precedence over the first extension method, and 
's method takes precedence over both extension methods.
The output of this example is:
 takes precendece over 
, and 
 takes precedence over both 
 and 
.
Delegate invocations
For a delegate invocation, the 
 of the 
 must be a value of a 
. Furthermore, considering the 
 to be a function member with the same parameter list as the 
, the 
 must be applicable (
) with respect to the 
 of the 
.
The run-time processing of a delegate invocation of the form 
, where 
 is a 
 of a 
 and 
 is an optional 
, consists of the following steps:
 is evaluated. If this evaluation causes an exception, no further steps are executed.
The value of 
 is checked to be valid. If the value of 
 is 
, a 
 is thrown and no further steps are executed.
Otherwise, 
 is a reference to a delegate instance. Function member invocations (
) are performed on each of the callable entities in the invocation list of the delegate. For callable entities consisting of an instance and instance method, the instance for the invocation is the instance contained in the callable entity.
Element access
An 
 consists of a 
, followed by a ""
"" token, followed by an 
, followed by a ""
"" token. The 
 consists of one or more 
s, separated by commas.
The 
 of an 
 is not allowed to contain 
 or 
 arguments.
An 
 is dynamically bound (
) if at least one of the following holds:
The 
 has compile-time type 
.
At least one expression of the 
 has compile-time type 
 and the 
 does not have an array type.
In this case the compiler classifies the 
 as a value of type 
. The rules below to determine the meaning of the 
 are then applied at run-time, using the run-time type instead of the compile-time type of those of the 
 and 
 expressions which have the compile-time type 
. If the 
 does not have compile-time type 
, then the element access undergoes a limited compile time check as described in 
.
If the 
 of an 
 is a value of an 
, the 
 is an array access (
). Otherwise, the 
 must be a variable or value of a class, struct, or interface type that has one or more indexer members, in which case the 
 is an indexer access (
).
Array access
For an array access, the 
 of the 
 must be a value of an 
. Furthermore, the 
 of an array access is not allowed to contain named arguments.The number of expressions in the 
 must be the same as the rank of the 
, and each expression must be of type 
, 
, 
, 
, or must be implicitly convertible to one or more of these types.
The result of evaluating an array access is a variable of the element type of the array, namely the array element selected by the value(s) of the expression(s) in the 
.
The run-time processing of an array access of the form 
, where 
 is a 
 of an 
 and 
 is an 
, consists of the following steps:
 is evaluated. If this evaluation causes an exception, no further steps are executed.
The index expressions of the 
 are evaluated in order, from left to right. Following evaluation of each index expression, an implicit conversion (
) to one of the following types is performed: 
, 
, 
, 
. The first type in this list for which an implicit conversion exists is chosen. For instance, if the index expression is of type 
 then an implicit conversion to 
 is performed, since implicit conversions from 
 to 
 and from 
 to 
 are possible. If evaluation of an index expression or the subsequent implicit conversion causes an exception, then no further index expressions are evaluated and no further steps are executed.
The value of 
 is checked to be valid. If the value of 
 is 
, a 
 is thrown and no further steps are executed.
The value of each expression in the 
 is checked against the actual bounds of each dimension of the array instance referenced by 
. If one or more values are out of range, a 
 is thrown and no further steps are executed.
The location of the array element given by the index expression(s) is computed, and this location becomes the result of the array access.
Indexer access
For an indexer access, the 
 of the 
 must be a variable or value of a class, struct, or interface type, and this type must implement one or more indexers that are applicable with respect to the 
 of the 
.
The binding-time processing of an indexer access of the form 
, where 
 is a 
 of a class, struct, or interface type 
, and 
 is an 
, consists of the following steps:
The set of indexers provided by 
 is constructed. The set consists of all indexers declared in 
 or a base type of 
 that are not 
 declarations and are accessible in the current context (
).
The set is reduced to those indexers that are applicable and not hidden by other indexers. The following rules are applied to each indexer 
 in the set, where 
 is the type in which the indexer 
 is declared:
If 
 is not applicable with respect to 
 (
), then 
 is removed from the set.
If 
 is applicable with respect to 
 (
), then all indexers declared in a base type of 
 are removed from the set.
If 
 is applicable with respect to 
 (
) and 
 is a class type other than 
, all indexers declared in an interface are removed from the set.
If the resulting set of candidate indexers is empty, then no applicable indexers exist, and a binding-time error occurs.
The best indexer of the set of candidate indexers is identified using the overload resolution rules of 
. If a single best indexer cannot be identified, the indexer access is ambiguous, and a binding-time error occurs.
The index expressions of the 
 are evaluated in order, from left to right. The result of processing the indexer access is an expression classified as an indexer access. The indexer access expression references the indexer determined in the step above, and has an associated instance expression of 
 and an associated argument list of 
.
Depending on the context in which it is used, an indexer access causes invocation of either the 
get accessor
 or the 
set accessor
 of the indexer. If the indexer access is the target of an assignment, the 
set accessor
 is invoked to assign a new value (
). In all other cases, the 
get accessor
 is invoked to obtain the current value (
).
This access
A 
 consists of the reserved word 
.
A 
 is permitted only in the 
 of an instance constructor, an instance method, or an instance accessor. It has one of the following meanings:
When 
 is used in a 
 within an instance constructor of a class, it is classified as a value. The type of the value is the instance type (
) of the class within which the usage occurs, and the value is a reference to the object being constructed.
When 
 is used in a 
 within an instance method or instance accessor of a class, it is classified as a value. The type of the value is the instance type (
) of the class within which the usage occurs, and the value is a reference to the object for which the method or accessor was invoked.
When 
 is used in a 
 within an instance constructor of a struct, it is classified as a variable. The type of the variable is the instance type (
) of the struct within which the usage occurs, and the variable represents the struct being constructed. The 
 variable of an instance constructor of a struct behaves exactly the same as an 
 parameter of the struct type—in particular, this means that the variable must be definitely assigned in every execution path of the instance constructor.
When 
 is used in a 
 within an instance method or instance accessor of a struct, it is classified as a variable. The type of the variable is the instance type (
) of the struct within which the usage occurs.
If the method or accessor is not an iterator (
), the 
 variable represents the struct for which the method or accessor was invoked, and behaves exactly the same as a 
 parameter of the struct type.
If the method or accessor is an iterator, the 
 variable represents a copy of the struct for which the method or accessor was invoked, and behaves exactly the same as a value parameter of the struct type.
Use of 
 in a 
 in a context other than the ones listed above is a compile-time error. In particular, it is not possible to refer to 
 in a static method, a static property accessor, or in a 
 of a field declaration.
Base access
A 
 consists of the reserved word 
 followed by either a ""
"" token and an identifier or an 
 enclosed in square brackets:
A 
 is used to access base class members that are hidden by similarly named members in the current class or struct. A 
 is permitted only in the 
 of an instance constructor, an instance method, or an instance accessor. When 
 occurs in a class or struct, 
 must denote a member of the base class of that class or struct. Likewise, when 
 occurs in a class, an applicable indexer must exist in the base class.
At binding-time, 
 expressions of the form 
 and 
 are evaluated exactly as if they were written 
 and 
, where 
 is the base class of the class or struct in which the construct occurs. Thus, 
 and 
 correspond to 
 and 
, except 
 is viewed as an instance of the base class.
When a 
 references a virtual function member (a method, property, or indexer), the determination of which function member to invoke at run-time (
) is changed. The function member that is invoked is determined by finding the most derived implementation (
) of the function member with respect to 
 (instead of with respect to the run-time type of 
, as would be usual in a non-base access). Thus, within an 
 of a 
 function member, a 
 can be used to invoke the inherited implementation of the function member. If the function member referenced by a 
 is abstract, a binding-time error occurs.
Postfix increment and decrement operators
The operand of a postfix increment or decrement operation must be an expression classified as a variable, a property access, or an indexer access. The result of the operation is a value of the same type as the operand.
If the 
 has the compile-time type 
 then the operator is dynamically bound (
), the 
 or 
 has the compile-time type 
 and the following rules are applied at run-time using the run-time type of the 
.
If the operand of a postfix increment or decrement operation is a property or indexer access, the property or indexer must have both a 
 and a 
 accessor. If this is not the case, a binding-time error occurs.
Unary operator overload resolution (
) is applied to select a specific operator implementation. Predefined 
 and 
 operators exist for the following types: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and any enum type. The predefined 
 operators return the value produced by adding 1 to the operand, and the predefined 
 operators return the value produced by subtracting 1 from the operand. In a 
 context, if the result of this addition or subtraction is outside the range of the result type and the result type is an integral type or enum type, a 
 is thrown.
The run-time processing of a postfix increment or decrement operation of the form 
 or 
 consists of the following steps:
If 
 is classified as a variable:
 is evaluated to produce the variable.
The value of 
 is saved.
The selected operator is invoked with the saved value of 
 as its argument.
The value returned by the operator is stored in the location given by the evaluation of 
.
The saved value of 
 becomes the result of the operation.
If 
 is classified as a property or indexer access:
The instance expression (if 
 is not 
) and the argument list (if 
 is an indexer access) associated with 
 are evaluated, and the results are used in the subsequent 
 and 
 accessor invocations.
The 
 accessor of 
 is invoked and the returned value is saved.
The selected operator is invoked with the saved value of 
 as its argument.
The 
 accessor of 
 is invoked with the value returned by the operator as its 
 argument.
The saved value of 
 becomes the result of the operation.
The 
 and 
 operators also support prefix notation (
). Typically, the result of 
 or 
 is the value of 
 before the operation, whereas the result of 
 or 
 is the value of 
 after the operation. In either case, 
 itself has the same value after the operation.
An 
 or 
 implementation can be invoked using either postfix or prefix notation. It is not possible to have separate operator implementations for the two notations.
The new operator
The 
 operator is used to create new instances of types.
There are three forms of 
 expressions:
Object creation expressions are used to create new instances of class types and value types.
Array creation expressions are used to create new instances of array types.
Delegate creation expressions are used to create new instances of delegate types.
The 
 operator implies creation of an instance of a type, but does not necessarily imply dynamic allocation of memory. In particular, instances of value types require no additional memory beyond the variables in which they reside, and no dynamic allocations occur when 
 is used to create instances of value types.
Object creation expressions
An 
 is used to create a new instance of a 
 or a 
.
The 
 of an 
 must be a 
, a 
 or a 
. The 
 cannot be an 
 
.
The optional 
 (
) is permitted only if the 
 is a 
 or a 
.
An object creation expression can omit the constructor argument list and enclosing parentheses provided it includes an object initializer or collection initializer. Omitting the constructor argument list and enclosing parentheses is equivalent to specifying an empty argument list.
Processing of an object creation expression that includes an object initializer or collection initializer consists of first processing the instance constructor and then processing the member or element initializations specified by the object initializer (
) or collection initializer (
).
If any of the arguments in the optional 
 has the compile-time type 
 then the 
 is dynamically bound (
) and the following rules are applied at run-time using the run-time type of those arguments of the 
 that have the compile time type 
. However, the object creation undergoes a limited compile time check as described in 
.
The binding-time processing of an 
 of the form 
, where 
 is a 
 or a 
 and 
 is an optional 
, consists of the following steps:
If 
 is a 
 and 
 is not present:
The 
 is a default constructor invocation. The result of the 
 is a value of type 
, namely the default value for 
 as defined in 
.
Otherwise, if 
 is a 
 and 
 is not present:
If no value type constraint or constructor constraint (
) has been specified for 
, a binding-time error occurs.
The result of the 
 is a value of the run-time type that the type parameter has been bound to, namely the result of invoking the default constructor of that type. The run-time type may be a reference type or a value type.
Otherwise, if 
 is a 
 or a 
:
If 
 is an 
 
, a compile-time error occurs.
The instance constructor to invoke is determined using the overload resolution rules of 
. The set of candidate instance constructors consists of all accessible instance constructors declared in 
 which are applicable with respect to 
 (
). If the set of candidate instance constructors is empty, or if a single best instance constructor cannot be identified, a binding-time error occurs.
The result of the 
 is a value of type 
, namely the value produced by invoking the instance constructor determined in the step above.
Otherwise, the 
 is invalid, and a binding-time error occurs.
Even if the 
 is dynamically bound, the compile-time type is still 
.
The run-time processing of an 
 of the form 
, where 
 is 
 or a 
 and 
 is an optional 
, consists of the following steps:
If 
 is a 
:
A new instance of class 
 is allocated. If there is not enough memory available to allocate the new instance, a 
 is thrown and no further steps are executed.
All fields of the new instance are initialized to their default values (
).
The instance constructor is invoked according to the rules of function member invocation (
). A reference to the newly allocated instance is automatically passed to the instance constructor and the instance can be accessed from within that constructor as 
.
If 
 is a 
:
An instance of type 
 is created by allocating a temporary local variable. Since an instance constructor of a 
 is required to definitely assign a value to each field of the instance being created, no initialization of the temporary variable is necessary.
The instance constructor is invoked according to the rules of function member invocation (
). A reference to the newly allocated instance is automatically passed to the instance constructor and the instance can be accessed from within that constructor as 
.
Object initializers
An 
 specifies values for zero or more fields, properties or indexed elements of an object.
An object initializer consists of a sequence of member initializers, enclosed by 
 and 
 tokens and separated by commas. Each 
 designates a target for the initialization. An 
 must name an accessible field or property of the object being initialized, whereas an 
 enclosed in square brackets must specify arguments for an accessible indexer on the object being initialized. It is an error for an object initializer to include more than one member initializer for the same field or property.
Each 
 is followed by an equals sign and either an expression, an object initializer or a collection initializer. It is not possible for expressions within the object initializer to refer to the newly created object it is initializing.
A member initializer that specifies an expression after the equals sign is processed in the same way as an assignment (
) to the target.
A member initializer that specifies an object initializer after the equals sign is a 
, i.e. an initialization of an embedded object. Instead of assigning a new value to the field or property, the assignments in the nested object initializer are treated as assignments to members of the field or property. Nested object initializers cannot be applied to properties with a value type, or to read-only fields with a value type.
A member initializer that specifies a collection initializer after the equals sign is an initialization of an embedded collection. Instead of assigning a new collection to the target field, property or indexer, the elements given in the initializer are added to the collection referenced by the target. The target must be of a collection type that satisfies the requirements specified in 
.
The arguments to an index initializer will always be evaluated exactly once. Thus, even if the arguments end up never getting used (e.g. because of an empty nested initializer), they will be evaluated for their side effects.
The following class represents a point with two coordinates:
An instance of 
 can be created and initialized as follows:
which has the same effect as
where 
 is an otherwise invisible and inaccessible temporary variable. The following class represents a rectangle created from two points:
An instance of 
 can be created and initialized as follows:
which has the same effect as
where 
, 
 and 
 are temporary variables that are otherwise invisible and inaccessible.
If 
's constructor allocates the two embedded 
 instances
the following construct can be used to initialize the embedded 
 instances instead of assigning new instances:
which has the same effect as
Given an appropriate definition of C, the following example:
is equivalent to this series of assignments:
where 
, etc., are generated variables that are invisible and inaccessible to the source code. Note that the arguments for 
 are evaluated only once, and the arguments for 
 are evaluated once even though they are never used.
Collection initializers
A collection initializer specifies the elements of a collection.
A collection initializer consists of a sequence of element initializers, enclosed by 
 and 
 tokens and separated by commas. Each element initializer specifies an element to be added to the collection object being initialized, and consists of a list of expressions enclosed by 
 and 
 tokens and separated by commas.  A single-expression element initializer can be written without braces, but cannot then be an assignment expression, to avoid ambiguity with member initializers. The 
 production is defined in 
.
The following is an example of an object creation expression that includes a collection initializer:
The collection object to which a collection initializer is applied must be of a type that implements 
 or a compile-time error occurs. For each specified element in order, the collection initializer invokes an 
 method on the target object with the expression list of the element initializer as argument list, applying normal member lookup and overload resolution for each invocation. Thus, the collection object must have an applicable instance or extension method with the name 
 for each element initializer.
The following class represents a contact with a name and a list of phone numbers:
A 
 can be created and initialized as follows:
which has the same effect as
where 
, 
 and 
 are temporary variables that are otherwise invisible and inaccessible.
Array creation expressions
An 
 is used to create a new instance of an 
.
An array creation expression of the first form allocates an array instance of the type that results from deleting each of the individual expressions from the expression list. For example, the array creation expression 
 produces an array instance of type 
, and the array creation expression 
 produces an array of type 
. Each expression in the expression list must be of type 
, 
, 
, or 
, or implicitly convertible to one or more of these types. The value of each expression determines the length of the corresponding dimension in the newly allocated array instance. Since the length of an array dimension must be nonnegative, it is a compile-time error to have a 
 with a negative value in the expression list.
Except in an unsafe context (
), the layout of arrays is unspecified.
If an array creation expression of the first form includes an array initializer, each expression in the expression list must be a constant and the rank and dimension lengths specified by the expression list must match those of the array initializer.
In an array creation expression of the second or third form, the rank of the specified array type or rank specifier must match that of the array initializer. The individual dimension lengths are inferred from the number of elements in each of the corresponding nesting levels of the array initializer. Thus, the expression
exactly corresponds to
An array creation expression of the third form is referred to as an 
. It is similar to the second form, except that the element type of the array is not explicitly given, but determined as the best common type (
) of the set of expressions in the array initializer. For a multidimensional array, i.e., one where the 
 contains at least one comma, this set comprises all 
s found in nested 
s.
Array initializers are described further in 
.
The result of evaluating an array creation expression is classified as a value, namely a reference to the newly allocated array instance. The run-time processing of an array creation expression consists of the following steps:
The dimension length expressions of the 
 are evaluated in order, from left to right. Following evaluation of each expression, an implicit conversion (
) to one of the following types is performed: 
, 
, 
, 
. The first type in this list for which an implicit conversion exists is chosen. If evaluation of an expression or the subsequent implicit conversion causes an exception, then no further expressions are evaluated and no further steps are executed.
The computed values for the dimension lengths are validated as follows. If one or more of the values are less than zero, a 
 is thrown and no further steps are executed.
An array instance with the given dimension lengths is allocated. If there is not enough memory available to allocate the new instance, a 
 is thrown and no further steps are executed.
All elements of the new array instance are initialized to their default values (
).
If the array creation expression contains an array initializer, then each expression in the array initializer is evaluated and assigned to its corresponding array element. The evaluations and assignments are performed in the order the expressions are written in the array initializer—in other words, elements are initialized in increasing index order, with the rightmost dimension increasing first. If evaluation of a given expression or the subsequent assignment to the corresponding array element causes an exception, then no further elements are initialized (and the remaining elements will thus have their default values).
An array creation expression permits instantiation of an array with elements of an array type, but the elements of such an array must be manually initialized. For example, the statement
creates a single-dimensional array with 100 elements of type 
. The initial value of each element is 
. It is not possible for the same array creation expression to also instantiate the sub-arrays, and the statement
results in a compile-time error. Instantiation of the sub-arrays must instead be performed manually, as in
When an array of arrays has a ""rectangular"" shape, that is when the sub-arrays are all of the same length, it is more efficient to use a multi-dimensional array. In the example above, instantiation of the array of arrays creates 101 objects—one outer array and 100 sub-arrays. In contrast,
creates only a single object, a two-dimensional array, and accomplishes the allocation in a single statement.
The following are examples of implicitly typed array creation expressions:
The last expression causes a compile-time error because neither 
 nor 
 is implicitly convertible to the other, and so there is no best common type. An explicitly typed array creation expression must be used in this case, for example specifying the type to be 
. Alternatively, one of the elements can be cast to a common base type, which would then become the inferred element type.
Implicitly typed array creation expressions can be combined with anonymous object initializers (
) to create anonymously typed data structures. For example:
Delegate creation expressions
A 
 is used to create a new instance of a 
.
The argument of a delegate creation expression must be a method group, an anonymous function or a value of either the compile time type 
 or a 
. If the argument is a method group, it identifies the method and, for an instance method, the object for which to create a delegate. If the argument is an anonymous function it directly defines the parameters and method body of the delegate target. If the argument is a value it identifies a delegate instance of which to create a copy.
If the 
 has the compile-time type 
, the 
 is dynamically bound (
), and the rules below are applied at run-time using the run-time type of the 
. Otherwise the rules are applied at compile-time.
The binding-time processing of a 
 of the form 
, where 
 is a 
 and 
 is an 
, consists of the following steps:
If 
 is a method group, the delegate creation expression is processed in the same way as a method group conversion (
) from 
 to 
.
If 
 is an anonymous function, the delegate creation expression is processed in the same way as an anonymous function conversion (
) from 
 to 
.
If 
 is a value, 
 must be compatible (
) with 
, and the result is a reference to a newly created delegate of type 
 that refers to the same invocation list as 
. If 
 is not compatible with 
, a compile-time error occurs.
The run-time processing of a 
 of the form 
, where 
 is a 
 and 
 is an 
, consists of the following steps:
If 
 is a method group, the delegate creation expression is evaluated as a method group conversion (
) from 
 to 
.
If 
 is an anonymous function, the delegate creation is evaluated as an anonymous function conversion from 
 to 
 (
).
If 
 is a value of a 
:
 is evaluated. If this evaluation causes an exception, no further steps are executed.
If the value of 
 is 
, a 
 is thrown and no further steps are executed.
A new instance of the delegate type 
 is allocated. If there is not enough memory available to allocate the new instance, a 
 is thrown and no further steps are executed.
The new delegate instance is initialized with the same invocation list as the delegate instance given by 
.
The invocation list of a delegate is determined when the delegate is instantiated and then remains constant for the entire lifetime of the delegate. In other words, it is not possible to change the target callable entities of a delegate once it has been created. When two delegates are combined or one is removed from another (
), a new delegate results; no existing delegate has its contents changed.
It is not possible to create a delegate that refers to a property, indexer, user-defined operator, instance constructor, destructor, or static constructor.
As described above, when a delegate is created from a method group, the formal parameter list and return type of the delegate determine which of the overloaded methods to select. In the example
the 
 field is initialized with a delegate that refers to the second 
 method because that method exactly matches the formal parameter list and return type of 
. Had the second 
 method not been present, a compile-time error would have occurred.
Anonymous object creation expressions
An 
 is used to create an object of an anonymous type.
An anonymous object initializer declares an anonymous type and returns an instance of that type. An anonymous type is a nameless class type that inherits directly from 
. The members of an anonymous type are a sequence of read-only properties inferred from the anonymous object initializer used to create an instance of the type. Specifically, an anonymous object initializer of the form
declares an anonymous type of the form
where each 
 is the type of the corresponding expression 
. The expression used in a 
 must have a type. Thus, it is a compile-time error for an expression in a 
 to be null or an anonymous function. It is also a compile-time error for the expression to have an unsafe type.
The names of an anonymous type and of the parameter to its 
 method are automatically generated by the compiler and cannot be referenced in program text.
Within the same program, two anonymous object initializers that specify a sequence of properties of the same names and compile-time types in the same order will produce instances of the same anonymous type.
In the example
the assignment on the last line is permitted because 
 and 
 are of the same anonymous type.
The 
 and 
 methods on anonymous types override the methods inherited from 
, and are defined in terms of the 
 and 
 of the properties, so that two instances of the same anonymous type are equal if and only if all their properties are equal.
A member declarator can be abbreviated to a simple name (
), a member access (
), a base access (
) or a null-conditional member access (
). This is called a 
 and is shorthand for a declaration of and assignment to a property with the same name. Specifically, member declarators of the forms
are precisely equivalent to the following, respectively:
Thus, in a projection initializer the 
 selects both the value and the field or property to which the value is assigned. Intuitively, a projection initializer projects not just a value, but also the name of the value.
The typeof operator
The 
 operator is used to obtain the 
 object for a type.
The first form of 
 consists of a 
 keyword followed by a parenthesized 
. The result of an expression of this form is the 
 object for the indicated type. There is only one 
 object for any given type. This means that for a type 
, 
 is always true. The 
 cannot be 
.
The second form of 
 consists of a 
 keyword followed by a parenthesized 
. An 
 is very similar to a 
 (
) except that an 
 contains 
s where a 
 contains 
s. When the operand of a 
 is a sequence of tokens that satisfies the grammars of both 
 and 
, namely when it contains neither a 
 nor a 
, the sequence of tokens is considered to be a 
. The meaning of an 
 is determined as follows:
Convert the sequence of tokens to a 
 by replacing each 
 with a 
 having the same number of commas and the keyword 
 as each 
.
Evaluate the resulting 
, while ignoring all type parameter constraints.
The 
 resolves to the unbound generic type associated with the resulting constructed type (
).
The result of the 
 is the 
 object for the resulting unbound generic type.
The third form of 
 consists of a 
 keyword followed by a parenthesized 
 keyword. The result of an expression of this form is the 
 object that represents the absence of a type. The type object returned by 
 is distinct from the type object returned for any type. This special type object is useful in class libraries that allow reflection onto methods in the language, where those methods wish to have a way to represent the return type of any method, including void methods, with an instance of 
.
The 
 operator can be used on a type parameter. The result is the 
 object for the run-time type that was bound to the type parameter. The 
 operator can also be used on a constructed type or an unbound generic type (
). The 
 object for an unbound generic type is not the same as the 
 object of the instance type. The instance type is always a closed constructed type at run-time so its 
 object depends on the run-time type arguments in use, while the unbound generic type has no type arguments.
The example
produces the following output:
Note that 
 and 
 are the same type.
Also note that the result of 
 does not depend on the type argument but the result of 
 does.
The checked and unchecked operators
The 
 and 
 operators are used to control the 
 for integral-type arithmetic operations and conversions.
The 
 operator evaluates the contained expression in a checked context, and the 
 operator evaluates the contained expression in an unchecked context. A 
 or 
 corresponds exactly to a 
 (
), except that the contained expression is evaluated in the given overflow checking context.
The overflow checking context can also be controlled through the 
 and 
 statements (
).
The following operations are affected by the overflow checking context established by the 
 and 
 operators and statements:
The predefined 
 and 
 unary operators (
 and 
), when the operand is of an integral type.
The predefined 
 unary operator (
), when the operand is of an integral type.
The predefined 
, 
, 
, and 
 binary operators (
), when both operands are of integral types.
Explicit numeric conversions (
) from one integral type to another integral type, or from 
 or 
 to an integral type.
When one of the above operations produce a result that is too large to represent in the destination type, the context in which the operation is performed controls the resulting behavior:
In a 
 context, if the operation is a constant expression (
), a compile-time error occurs. Otherwise, when the operation is performed at run-time, a 
 is thrown.
In an 
 context, the result is truncated by discarding any high-order bits that do not fit in the destination type.
For non-constant expressions (expressions that are evaluated at run-time) that are not enclosed by any 
 or 
 operators or statements, the default overflow checking context is 
 unless external factors (such as compiler switches and execution environment configuration) call for 
 evaluation.
For constant expressions (expressions that can be fully evaluated at compile-time), the default overflow checking context is always 
. Unless a constant expression is explicitly placed in an 
 context, overflows that occur during the compile-time evaluation of the expression always cause compile-time errors.
The body of an anonymous function is not affected by 
 or 
 contexts in which the anonymous function occurs.
In the example
no compile-time errors are reported since neither of the expressions can be evaluated at compile-time. At run-time, the 
 method throws a 
, and the 
 method returns -727379968 (the lower 32 bits of the out-of-range result). The behavior of the 
 method depends on the default overflow checking context for the compilation, but it is either the same as 
 or the same as 
.
In the example
the overflows that occur when evaluating the constant expressions in 
 and 
 cause compile-time errors to be reported because the expressions are evaluated in a 
 context. An overflow also occurs when evaluating the constant expression in 
, but since the evaluation takes place in an 
 context, the overflow is not reported.
The 
 and 
 operators only affect the overflow checking context for those operations that are textually contained within the ""
"" and ""
"" tokens. The operators have no effect on function members that are invoked as a result of evaluating the contained expression. In the example
the use of 
 in 
 does not affect the evaluation of 
 in 
, so 
 is evaluated in the default overflow checking context.
The 
 operator is convenient when writing constants of the signed integral types in hexadecimal notation. For example:
Both of the hexadecimal constants above are of type 
. Because the constants are outside the 
 range, without the 
 operator, the casts to 
 would produce compile-time errors.
The 
 and 
 operators and statements allow programmers to control certain aspects of some numeric calculations. However, the behavior of some numeric operators depends on their operands' data types. For example, multiplying two decimals always results in an exception on overflow even within an explicitly 
 construct. Similarly, multiplying two floats never results in an exception on overflow even within an explicitly 
 construct. In addition, other operators are never affected by the mode of checking, whether default or explicit.
Default value expressions
A default value expression is used to obtain the default value (
) of a type. Typically a default value expression is used for type parameters, since it may not be known if the type parameter is a value type or a reference type. (No conversion exists from the 
 literal to a type parameter unless the type parameter is known to be a reference type.)
If the 
 in a 
 evaluates at run-time to a reference type, the result is 
 converted to that type. If the 
 in a 
 evaluates at run-time to a value type, the result is the 
's default value (
).
A 
 is a constant expression (
) if the type is a reference type or a type parameter that is known to be a reference type (
). In addition, a 
 is a constant expression if the type is one of the following value types: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or any enumeration type.
Nameof expressions
A 
 is used to obtain the name of a program entity as a constant string.
Grammatically speaking, the 
 operand is always an expression. Because 
 is not a reserved keyword, a nameof expression is always syntactically ambiguous with an invocation of the simple name 
. For compatibility reasons, if a name lookup (
) of the name 
 succeeds, the expression is treated as an 
 -- regardless of whether the invocation is legal. Otherwise it is a 
.
The meaning of the 
 of a 
 is the meaning of it as an expression; that is, either as a 
, a 
 or a 
. However, where the lookup described in 
 and 
 results in an error because an instance member was found in a static context, a 
 produces no such error.
It is a compile-time error for a 
 designating a method group to have a 
. It is a compile time error for a 
 to have the type 
.
A 
 is a constant expression of type 
, and has no effect at runtime. Specifically, its 
 is not evaluated, and is ignored for the purposes of definite assignment analysis (
). Its value is the last identifier of the 
 before the optional final 
, transformed in the following way:
The prefix ""
"", if used, is removed.
Each 
 is transformed into its corresponding Unicode character.
Any 
formatting_characters
 are removed.
These are the same transformations applied in 
 when testing equality between identifiers.
TODO: examples
Anonymous method expressions
An 
 is one of two ways of defining an anonymous function. These are further described in 
.
Unary operators
The 
, 
, 
, 
, 
, 
, 
, cast, and 
 operators are called the unary operators.
If the operand of a 
 has the compile-time type 
, it is dynamically bound (
). In this case the compile-time type of the 
 is 
, and the resolution described below will take place at run-time using the run-time type of the operand.
Null-conditional operator
The null-conditional operator applies a list of operations to its operand only if that operand is non-null. Otherwise the result of applying the operator is 
.
The list of operations can include member access and element access operations (which may themselves be null-conditional), as well as invocation.
For example, the expression 
 is a 
 with a 
 
 and 
 
 (null-conditional element access), 
 (null-conditional member access) and 
 (invocation).
For a 
 
 with a 
 
, let 
 be the expression obtained by textually removing the leading 
 from each of the 
 of 
 that have one. Conceptually, 
 is the expression that will be evaluated if none of the null checks represented by the 
s do find a 
.
Also, let 
 be the expression obtained by textually removing the leading 
 from just the first of the 
 in 
. This may lead to a 
primary-expression
 (if there was just one 
) or to another 
.
For example, if 
 is the expression 
, then 
 is the expression 
 and 
 is the expression 
.
If 
 is classified as nothing, then 
 is classified as nothing. Otherwise E is classified as a value.
 and 
 are used to determine the meaning of 
:
If 
 occurs as a 
 the meaning of 
 is the same as the statement
except that P is evaluated only once.
Otherwise, if 
 is classified as nothing a compile-time error occurs.
Otherwise, let 
 be the type of 
.
If 
 is a type parameter that is not known to be a reference type or a non-nullable value type, a compile-time error occurs.
If 
 is a non-nullable value type, then the type of 
 is 
, and the meaning of 
 is the same as
except that 
 is evaluated only once.
Otherwise the type of E is T0, and the meaning of E is the same as
except that 
 is evaluated only once.
If 
 is itself a 
, then these rules are applied again, nesting the tests for 
 until there are no further 
's, and the expression has been reduced all the way down to the primary-expression 
.
For example, if the expression 
 occurs as a statement-expression, as in the statement:
its meaning is equivalent to:
which again is equivalent to:
Except that 
 and 
 are evaluated only once.
If it occurs in a context where its value is used, as in:
and assuming that the type of the final invocation is not a non-nullable value type, its meaning is equivalent to:
except that 
 and 
 are evaluated only once.
Null-conditional expressions as projection initializers
A null-conditional expression is only allowed as a 
 in an 
 (
) if it ends with an (optionally null-conditional) member access. Grammatically, this requirement can be expressed as:
This is a special case of the grammar for 
 above. The production for 
 in 
 then includes only 
.
Null-conditional expressions as statement expressions
A null-conditional expression is only allowed as a 
 (
) if it ends with an invocation. Grammatically, this requirement can be expressed as:
This is a special case of the grammar for 
 above. The production for 
 in 
 then includes only 
.
Unary plus operator
For an operation of the form 
, unary operator overload resolution (
) is applied to select a specific operator implementation. The operand is converted to the parameter type of the selected operator, and the type of the result is the return type of the operator. The predefined unary plus operators are:
For each of these operators, the result is simply the value of the operand.
Unary minus operator
For an operation of the form 
, unary operator overload resolution (
) is applied to select a specific operator implementation. The operand is converted to the parameter type of the selected operator, and the type of the result is the return type of the operator. The predefined negation operators are:
Integer negation:
The result is computed by subtracting 
 from zero. If the value of of 
 is the smallest representable value of the operand type (-2^31 for 
 or -2^63 for 
), then the mathematical negation of 
 is not representable within the operand type. If this occurs within a 
 context, a 
 is thrown; if it occurs within an 
 context, the result is the value of the operand and the overflow is not reported.
If the operand of the negation operator is of type 
, it is converted to type 
, and the type of the result is 
. An exception is the rule that permits the 
 value -2147483648 (-2^31) to be written as a decimal integer literal (
).
If the operand of the negation operator is of type 
, a compile-time error occurs. An exception is the rule that permits the 
 value -9223372036854775808 (-2^63) to be written as a decimal integer literal (
).
Floating-point negation:
The result is the value of 
 with its sign inverted. If 
 is NaN, the result is also NaN.
Decimal negation:
The result is computed by subtracting 
 from zero. Decimal negation is equivalent to using the unary minus operator of type 
.
Logical negation operator
For an operation of the form 
, unary operator overload resolution (
) is applied to select a specific operator implementation. The operand is converted to the parameter type of the selected operator, and the type of the result is the return type of the operator. Only one predefined logical negation operator exists:
This operator computes the logical negation of the operand: If the operand is 
, the result is 
. If the operand is 
, the result is 
.
Bitwise complement operator
For an operation of the form 
, unary operator overload resolution (
) is applied to select a specific operator implementation. The operand is converted to the parameter type of the selected operator, and the type of the result is the return type of the operator. The predefined bitwise complement operators are:
For each of these operators, the result of the operation is the bitwise complement of 
.
Every enumeration type 
 implicitly provides the following bitwise complement operator:
The result of evaluating 
, where 
 is an expression of an enumeration type 
 with an underlying type 
, is exactly the same as evaluating 
, except that the conversion to 
 is always performed as if in an 
 context (
).
Prefix increment and decrement operators
The operand of a prefix increment or decrement operation must be an expression classified as a variable, a property access, or an indexer access. The result of the operation is a value of the same type as the operand.
If the operand of a prefix increment or decrement operation is a property or indexer access, the property or indexer must have both a 
 and a 
 accessor. If this is not the case, a binding-time error occurs.
Unary operator overload resolution (
) is applied to select a specific operator implementation. Predefined 
 and 
 operators exist for the following types: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and any enum type. The predefined 
 operators return the value produced by adding 1 to the operand, and the predefined 
 operators return the value produced by subtracting 1 from the operand. In a 
 context, if the result of this addition or subtraction is outside the range of the result type and the result type is an integral type or enum type, a 
 is thrown.
The run-time processing of a prefix increment or decrement operation of the form 
 or 
 consists of the following steps:
If 
 is classified as a variable:
 is evaluated to produce the variable.
The selected operator is invoked with the value of 
 as its argument.
The value returned by the operator is stored in the location given by the evaluation of 
.
The value returned by the operator becomes the result of the operation.
If 
 is classified as a property or indexer access:
The instance expression (if 
 is not 
) and the argument list (if 
 is an indexer access) associated with 
 are evaluated, and the results are used in the subsequent 
 and 
 accessor invocations.
The 
 accessor of 
 is invoked.
The selected operator is invoked with the value returned by the 
 accessor as its argument.
The 
 accessor of 
 is invoked with the value returned by the operator as its 
 argument.
The value returned by the operator becomes the result of the operation.
The 
 and 
 operators also support postfix notation (
). Typically, the result of 
 or 
 is the value of 
 before the operation, whereas the result of 
 or 
 is the value of 
 after the operation. In either case, 
 itself has the same value after the operation.
An 
 or 
 implementation can be invoked using either postfix or prefix notation. It is not possible to have separate operator implementations for the two notations.
Cast expressions
A 
 is used to explicitly convert an expression to a given type.
A 
 of the form 
, where 
 is a 
 and 
 is a 
, performs an explicit conversion (
) of the value of 
 to type 
. If no explicit conversion exists from 
 to 
, a binding-time error occurs. Otherwise, the result is the value produced by the explicit conversion. The result is always classified as a value, even if 
 denotes a variable.
The grammar for a 
 leads to certain syntactic ambiguities. For example, the expression 
 could either be interpreted as a 
 (a cast of 
 to type 
) or as an 
 combined with a 
 (which computes the value 
.
To resolve 
 ambiguities, the following rule exists: A sequence of one or more 
s (
) enclosed in parentheses is considered the start of a 
 only if at least one of the following are true:
The sequence of tokens is correct grammar for a 
, but not for an 
.
The sequence of tokens is correct grammar for a 
, and the token immediately following the closing parentheses is the token ""
"", the token ""
"", the token ""
"", an 
 (
), a 
 (
), or any 
 (
) except 
 and 
.
The term ""correct grammar"" above means only that the sequence of tokens must conform to the particular grammatical production. It specifically does not consider the actual meaning of any constituent identifiers. For example, if 
 and 
 are identifiers, then 
 is correct grammar for a type, even if 
 doesn't actually denote a type.
From the disambiguation rule it follows that, if 
 and 
 are identifiers, 
, 
, and 
 are 
s, but 
 is not, even if 
 identifies a type. However, if 
 is a keyword that identifies a predefined type (such as 
), then all four forms are 
s (because such a keyword could not possibly be an expression by itself).
Await expressions
The await operator is used to suspend evaluation of the enclosing async function until the asynchronous operation represented by the operand has completed.
An 
 is only allowed in the body of an async function (
). Within the nearest enclosing async function, an 
 may not occur in these places:
Inside a nested (non-async) anonymous function
Inside the block of a 
In an unsafe context
Note that an 
 cannot occur in most places within a 
, because those are syntactically transformed to use non-async lambda expressions.
Inside of an async function, 
 cannot be used as an identifier. There is therefore no syntactic ambiguity between await-expressions and various expressions involving identifiers. Outside of async functions, 
 acts as a normal identifier.
The operand of an 
 is called the 
. It represents an asynchronous operation that may or may not be complete at the time the 
 is evaluated. The purpose of the await operator is to suspend execution of the enclosing async function until the awaited task is complete, and then obtain its outcome.
Awaitable expressions
The task of an await expression is required to be 
. An expression 
 is awaitable if one of the following holds:
 is of compile time type 
 has an accessible instance or extension method called 
 with no parameters and no type parameters, and a return type 
 for which all of the following hold:
 implements the interface 
 (hereafter known as 
 for brevity)
 has an accessible, readable instance property 
 of type 
 has an accessible instance method 
 with no parameters and no type parameters
The purpose of the 
 method is to obtain an 
 for the task. The type 
 is called the 
 for the await expression.
The purpose of the 
 property is to determine if the task is already complete. If so, there is no need to suspend evaluation.
The purpose of the 
 method is to sign up a ""continuation"" to the task; i.e. a delegate (of type 
) that will be invoked once the task is complete.
The purpose of the 
 method is to obtain the outcome of the task once it is complete. This outcome may be successful completion, possibly with a result value, or it may be an exception which is thrown by the 
 method.
Classification of await expressions
The expression 
 is classified the same way as the expression 
. Thus, if the return type of 
 is 
, the 
 is classified as nothing. If it has a non-void return type 
, the 
 is classified as a value of type 
.
Runtime evaluation of await expressions
At runtime, the expression 
 is evaluated as follows:
An awaiter 
 is obtained by evaluating the expression 
.
A 
 
 is obtained by evaluating the expression 
.
If 
 is 
 then evaluation depends on whether 
 implements the interface 
 (hereafter known as 
 for brevity). This check is done at binding time; i.e. at runtime if 
 has the compile time type 
, and at compile time otherwise. Let 
 denote the resumption delegate (
):
If 
 does not implement 
, then the expression 

 is evaluated.
If 
 does implement 
, then the expression 

 is evaluated.
Evaluation is then suspended, and control is returned to the current caller of the async function.
Either immediately after (if 
 was 
), or upon later invocation of the resumption delegate (if 
 was 
), the expression 
 is evaluated. If it returns a value, that value is the result of the 
. Otherwise the result is nothing.
An awaiter's implementation of the interface methods 
 and 
 should cause the delegate 
 to be invoked at most once. Otherwise, the behavior of the enclosing async function is undefined.
Arithmetic operators
The 
, 
, 
, 
, and 
 operators are called the arithmetic operators.
If an operand of an arithmetic operator has the compile-time type 
, then the expression is dynamically bound (
). In this case the compile-time type of the expression is 
, and the resolution described below will take place at run-time using the run-time type of those operands that have the compile-time type 
.
Multiplication operator
For an operation of the form 
, binary operator overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined multiplication operators are listed below. The operators all compute the product of 
 and 
.
Integer multiplication:
In a 
 context, if the product is outside the range of the result type, a 
 is thrown. In an 
 context, overflows are not reported and any significant high-order bits outside the range of the result type are discarded.
Floating-point multiplication:
The product is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, 
 and 
 are positive finite values. 
 is the result of 
. If the result is too large for the destination type, 
 is infinity. If the result is too small for the destination type, 
 is zero.
+y
-y
+0
-0
+inf
-inf
NaN
+x
+z
-z
+0
-0
+inf
-inf
NaN
-x
-z
+z
-0
+0
-inf
+inf
NaN
+0
+0
-0
+0
-0
NaN
NaN
NaN
-0
-0
+0
-0
+0
NaN
NaN
NaN
+inf
+inf
-inf
NaN
NaN
+inf
-inf
NaN
-inf
-inf
+inf
NaN
NaN
-inf
+inf
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
Decimal multiplication:
If the resulting value is too large to represent in the 
 format, a 
 is thrown. If the result value is too small to represent in the 
 format, the result is zero. The scale of the result, before any rounding, is the sum of the scales of the two operands.
Decimal multiplication is equivalent to using the multiplication operator of type 
.
Division operator
For an operation of the form 
, binary operator overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined division operators are listed below. The operators all compute the quotient of 
 and 
.
Integer division:
If the value of the right operand is zero, a 
 is thrown.
The division rounds the result towards zero. Thus the absolute value of the result is the largest possible integer that is less than or equal to the absolute value of the quotient of the two operands. The result is zero or positive when the two operands have the same sign and zero or negative when the two operands have opposite signs.
If the left operand is the smallest representable 
 or 
 value and the right operand is 
, an overflow occurs. In a 
 context, this causes a 
 (or a subclass thereof) to be thrown. In an 
 context, it is implementation-defined as to whether a 
 (or a subclass thereof) is thrown or the overflow goes unreported with the resulting value being that of the left operand.
Floating-point division:
The quotient is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, 
 and 
 are positive finite values. 
 is the result of 
. If the result is too large for the destination type, 
 is infinity. If the result is too small for the destination type, 
 is zero.
+y
-y
+0
-0
+inf
-inf
NaN
+x
+z
-z
+inf
-inf
+0
-0
NaN
-x
-z
+z
-inf
+inf
-0
+0
NaN
+0
+0
-0
NaN
NaN
+0
-0
NaN
-0
-0
+0
NaN
NaN
-0
+0
NaN
+inf
+inf
-inf
+inf
-inf
NaN
NaN
NaN
-inf
-inf
+inf
-inf
+inf
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
Decimal division:
If the value of the right operand is zero, a 
 is thrown. If the resulting value is too large to represent in the 
 format, a 
 is thrown. If the result value is too small to represent in the 
 format, the result is zero. The scale of the result is the smallest scale that will preserve a result equal to the nearest representantable decimal value to the true mathematical result.
Decimal division is equivalent to using the division operator of type 
.
Remainder operator
For an operation of the form 
, binary operator overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined remainder operators are listed below. The operators all compute the remainder of the division between 
 and 
.
Integer remainder:
The result of 
 is the value produced by 
. If 
 is zero, a 
 is thrown.
If the left operand is the smallest 
 or 
 value and the right operand is 
, a 
 is thrown. In no case does 
 throw an exception where 
 would not throw an exception.
Floating-point remainder:
The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, 
 and 
 are positive finite values. 
 is the result of 
 and is computed as 
, where 
 is the largest possible integer that is less than or equal to 
. This method of computing the remainder is analogous to that used for integer operands, but differs from the IEEE 754 definition (in which 
 is the integer closest to 
).
+y
-y
+0
-0
+inf
-inf
NaN
+x
+z
+z
NaN
NaN
x
x
NaN
-x
-z
-z
NaN
NaN
-x
-x
NaN
+0
+0
+0
NaN
NaN
+0
+0
NaN
-0
-0
-0
NaN
NaN
-0
-0
NaN
+inf
NaN
NaN
NaN
NaN
NaN
NaN
NaN
-inf
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
Decimal remainder:
If the value of the right operand is zero, a 
 is thrown. The scale of the result, before any rounding, is the larger of the scales of the two operands, and the sign of the result, if non-zero, is the same as that of 
.
Decimal remainder is equivalent to using the remainder operator of type 
.
Addition operator
For an operation of the form 
, binary operator overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined addition operators are listed below. For numeric and enumeration types, the predefined addition operators compute the sum of the two operands. When one or both operands are of type string, the predefined addition operators concatenate the string representation of the operands.
Integer addition:
In a 
 context, if the sum is outside the range of the result type, a 
 is thrown. In an 
 context, overflows are not reported and any significant high-order bits outside the range of the result type are discarded.
Floating-point addition:
The sum is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, 
 and 
 are nonzero finite values, and 
 is the result of 
. If 
 and 
 have the same magnitude but opposite signs, 
 is positive zero. If 
 is too large to represent in the destination type, 
 is an infinity with the same sign as 
.
y
+0
-0
+inf
-inf
NaN
x
z
x
x
+inf
-inf
NaN
+0
y
+0
+0
+inf
-inf
NaN
-0
y
+0
-0
+inf
-inf
NaN
+inf
+inf
+inf
+inf
+inf
NaN
NaN
-inf
-inf
-inf
-inf
NaN
-inf
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
Decimal addition:
If the resulting value is too large to represent in the 
 format, a 
 is thrown. The scale of the result, before any rounding, is the larger of the scales of the two operands.
Decimal addition is equivalent to using the addition operator of type 
.
Enumeration addition. Every enumeration type implicitly provides the following predefined operators, where 
 is the enum type, and 
 is the underlying type of 
:
At run-time these operators are evaluated exactly as 
.
String concatenation:
These overloads of the binary 
 operator perform string concatenation. If an operand of string concatenation is 
, an empty string is substituted. Otherwise, any non-string argument is converted to its string representation by invoking the virtual 
 method inherited from type 
. If 
 returns 
, an empty string is substituted.
The result of the string concatenation operator is a string that consists of the characters of the left operand followed by the characters of the right operand. The string concatenation operator never returns a 
 value. A 
 may be thrown if there is not enough memory available to allocate the resulting string.
Delegate combination. Every delegate type implicitly provides the following predefined operator, where 
 is the delegate type:
The binary 
 operator performs delegate combination when both operands are of some delegate type 
. (If the operands have different delegate types, a binding-time error occurs.) If the first operand is 
, the result of the operation is the value of the second operand (even if that is also 
). Otherwise, if the second operand is 
, then the result of the operation is the value of the first operand. Otherwise, the result of the operation is a new delegate instance that, when invoked, invokes the first operand and then invokes the second operand. For examples of delegate combination, see 
 and 
. Since 
 is not a delegate type, 
 
 is not defined for it.
Subtraction operator
For an operation of the form 
, binary operator overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined subtraction operators are listed below. The operators all subtract 
 from 
.
Integer subtraction:
In a 
 context, if the difference is outside the range of the result type, a 
 is thrown. In an 
 context, overflows are not reported and any significant high-order bits outside the range of the result type are discarded.
Floating-point subtraction:
The difference is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaNs. In the table, 
 and 
 are nonzero finite values, and 
 is the result of 
. If 
 and 
 are equal, 
 is positive zero. If 
 is too large to represent in the destination type, 
 is an infinity with the same sign as 
.
NaN
y
+0
-0
+inf
-inf
NaN
x
z
x
x
-inf
+inf
NaN
+0
-y
+0
+0
-inf
+inf
NaN
-0
-y
-0
+0
-inf
+inf
NaN
+inf
+inf
+inf
+inf
NaN
+inf
NaN
-inf
-inf
-inf
-inf
-inf
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
NaN
Decimal subtraction:
If the resulting value is too large to represent in the 
 format, a 
 is thrown. The scale of the result, before any rounding, is the larger of the scales of the two operands.
Decimal subtraction is equivalent to using the subtraction operator of type 
.
Enumeration subtraction. Every enumeration type implicitly provides the following predefined operator, where 
 is the enum type, and 
 is the underlying type of 
:
This operator is evaluated exactly as 
. In other words, the operator computes the difference between the ordinal values of 
 and 
, and the type of the result is the underlying type of the enumeration.
This operator is evaluated exactly as 
. In other words, the operator subtracts a value from the underlying type of the enumeration, yielding a value of the enumeration.
Delegate removal. Every delegate type implicitly provides the following predefined operator, where 
 is the delegate type:
The binary 
 operator performs delegate removal when both operands are of some delegate type 
. If the operands have different delegate types, a binding-time error occurs. If the first operand is 
, the result of the operation is 
. Otherwise, if the second operand is 
, then the result of the operation is the value of the first operand. Otherwise, both operands represent invocation lists (
) having one or more entries, and the result is a new invocation list consisting of the first operand's list with the second operand's entries removed from it, provided the second operand's list is a proper contiguous sublist of the first's.     (To determine sublist equality, corresponding entries are compared as for the delegate equality operator (
).) Otherwise, the result is the value of the left operand. Neither of the operands' lists is changed in the process. If the second operand's list matches multiple sublists of contiguous entries in the first operand's list, the right-most matching sublist of contiguous entries is removed. If removal results in an empty list, the result is 
. For example:
Shift operators
The 
 and 
 operators are used to perform bit shifting operations.
If an operand of a 
 has the compile-time type 
, then the expression is dynamically bound (
). In this case the compile-time type of the expression is 
, and the resolution described below will take place at run-time using the run-time type of those operands that have the compile-time type 
.
For an operation of the form 
 or 
, binary operator overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
When declaring an overloaded shift operator, the type of the first operand must always be the class or struct containing the operator declaration, and the type of the second operand must always be 
.
The predefined shift operators are listed below.
Shift left:
The 
 operator shifts 
 left by a number of bits computed as described below.
The high-order bits outside the range of the result type of 
 are discarded, the remaining bits are shifted left, and the low-order empty bit positions are set to zero.
Shift right:
The 
 operator shifts 
 right by a number of bits computed as described below.
When 
 is of type 
 or 
, the low-order bits of 
 are discarded, the remaining bits are shifted right, and the high-order empty bit positions are set to zero if 
 is non-negative and set to one if 
 is negative.
When 
 is of type 
 or 
, the low-order bits of 
 are discarded, the remaining bits are shifted right, and the high-order empty bit positions are set to zero.
For the predefined operators, the number of bits to shift is computed as follows:
When the type of 
 is 
 or 
, the shift count is given by the low-order five bits of 
. In other words, the shift count is computed from 
.
When the type of 
 is 
 or 
, the shift count is given by the low-order six bits of 
. In other words, the shift count is computed from 
.
If the resulting shift count is zero, the shift operators simply return the value of 
.
Shift operations never cause overflows and produce the same results in 
 and 
 contexts.
When the left operand of the 
 operator is of a signed integral type, the operator performs an arithmetic shift right wherein the value of the most significant bit (the sign bit) of the operand is propagated to the high-order empty bit positions. When the left operand of the 
 operator is of an unsigned integral type, the operator performs a logical shift right wherein high-order empty bit positions are always set to zero. To perform the opposite operation of that inferred from the operand type, explicit casts can be used. For example, if 
 is a variable of type 
, the operation 
 performs a logical shift right of 
.
Relational and type-testing operators
The 
, 
, 
, 
, 
, 
, 
 and 
 operators are called the relational and type-testing operators.
The 
 operator is described in 
 and the 
 operator is described in 
.
The 
, 
, 
, 
, 
 and 
 operators are 
.
If an operand of a comparison operator has the compile-time type 
, then the expression is dynamically bound (
). In this case the compile-time type of the expression is 
, and the resolution described below will take place at run-time using the run-time type of those operands that have the compile-time type 
.
For an operation of the form 
 
op
 
, where 
op
 is a comparison operator, overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined comparison operators are described in the following sections. All predefined comparison operators return a result of type 
, as described in the following table.
Operation
Result
 if 
 is equal to 
, 
 otherwise
 if 
 is not equal to 
, 
 otherwise
 if 
 is less than 
, 
 otherwise
 if 
 is greater than 
, 
 otherwise
 if 
 is less than or equal to 
, 
 otherwise
 if 
 is greater than or equal to 
, 
 otherwise
Integer comparison operators
The predefined integer comparison operators are:
Each of these operators compares the numeric values of the two integer operands and returns a 
 value that indicates whether the particular relation is 
 or 
.
Floating-point comparison operators
The predefined floating-point comparison operators are:
The operators compare the operands according to the rules of the IEEE 754 standard:
If either operand is NaN, the result is 
 for all operators except 
, for which the result is 
. For any two operands, 
 always produces the same result as 
. However, when one or both operands are NaN, the 
, 
, 
, and 
 operators do not produce the same results as the logical negation of the opposite operator. For example, if either of 
 and 
 is NaN, then 
 is 
, but 
 is 
.
When neither operand is NaN, the operators compare the values of the two floating-point operands with respect to the ordering
where 
 and 
 are the smallest and largest positive finite values that can be represented in the given floating-point format. Notable effects of this ordering are:
Negative and positive zeros are considered equal.
A negative infinity is considered less than all other values, but equal to another negative infinity.
A positive infinity is considered greater than all other values, but equal to another positive infinity.
Decimal comparison operators
The predefined decimal comparison operators are:
Each of these operators compares the numeric values of the two decimal operands and returns a 
 value that indicates whether the particular relation is 
 or 
. Each decimal comparison is equivalent to using the corresponding relational or equality operator of type 
.
Boolean equality operators
The predefined boolean equality operators are:
The result of 
 is 
 if both 
 and 
 are 
 or if both 
 and 
 are 
. Otherwise, the result is 
.
The result of 
 is 
 if both 
 and 
 are 
 or if both 
 and 
 are 
. Otherwise, the result is 
. When the operands are of type 
, the 
 operator produces the same result as the 
 operator.
Enumeration comparison operators
Every enumeration type implicitly provides the following predefined comparison operators:
The result of evaluating 
, where 
 and 
 are expressions of an enumeration type 
 with an underlying type 
, and 
 is one of the comparison operators, is exactly the same as evaluating 
. In other words, the enumeration type comparison operators simply compare the underlying integral values of the two operands.
Reference type equality operators
The predefined reference type equality operators are:
The operators return the result of comparing the two references for equality or non-equality.
Since the predefined reference type equality operators accept operands of type 
, they apply to all types that do not declare applicable 
 and 
 members. Conversely, any applicable user-defined equality operators effectively hide the predefined reference type equality operators.
The predefined reference type equality operators require one of the following:
Both operands are a value of a type known to be a 
 or the literal 
. Furthermore, an explicit reference conversion (
) exists from the type of either operand to the type of the other operand.
One operand is a value of type 
 where 
 is a 
 and the other operand is the literal 
. Furthermore 
 does not have the value type constraint.
Unless one of these conditions are true, a binding-time error occurs. Notable implications of these rules are:
It is a binding-time error to use the predefined reference type equality operators to compare two references that are known to be different at binding-time. For example, if the binding-time types of the operands are two class types 
 and 
, and if neither 
 nor 
 derives from the other, then it would be impossible for the two operands to reference the same object. Thus, the operation is considered a binding-time error.
The predefined reference type equality operators do not permit value type operands to be compared. Therefore, unless a struct type declares its own equality operators, it is not possible to compare values of that struct type.
The predefined reference type equality operators never cause boxing operations to occur for their operands. It would be meaningless to perform such boxing operations, since references to the newly allocated boxed instances would necessarily differ from all other references.
If an operand of a type parameter type 
 is compared to 
, and the run-time type of 
 is a value type, the result of the comparison is 
.
The following example checks whether an argument of an unconstrained type parameter type is 
.
The 
 construct is permitted even though 
 could represent a value type, and the result is simply defined to be 
 when 
 is a value type.
For an operation of the form 
 or 
, if any applicable 
 or 
 exists, the operator overload resolution (
) rules will select that operator instead of the predefined reference type equality operator. However, it is always possible to select the predefined reference type equality operator by explicitly casting one or both of the operands to type 
. The example
produces the output
The 
 and 
 variables refer to two distinct 
 instances containing the same characters. The first comparison outputs 
 because the predefined string equality operator (
) is selected when both operands are of type 
. The remaining comparisons all output 
 because the predefined reference type equality operator is selected when one or both of the operands are of type 
.
Note that the above technique is not meaningful for value types. The example
outputs 
 because the casts create references to two separate instances of boxed 
 values.
String equality operators
The predefined string equality operators are:
Two 
 values are considered equal when one of the following is true:
Both values are 
.
Both values are non-null references to string instances that have identical lengths and identical characters in each character position.
The string equality operators compare string values rather than string references. When two separate string instances contain the exact same sequence of characters, the values of the strings are equal, but the references are different. As described in 
, the reference type equality operators can be used to compare string references instead of string values.
Delegate equality operators
Every delegate type implicitly provides the following predefined comparison operators:
Two delegate instances are considered equal as follows:
If either of the delegate instances is 
, they are equal if and only if both are 
.
If the delegates have different run-time type they are never equal.
If both of the delegate instances have an invocation list (
), those instances are equal if and only if their invocation lists are the same length, and each entry in one's invocation list is equal (as defined below) to the corresponding entry, in order, in the other's invocation list.
The following rules govern the equality of invocation list entries:
If two invocation list entries both refer to the same static method then the entries are equal.
If two invocation list entries both refer to the same non-static method on the same target object (as defined by the reference equality operators) then the entries are equal.
Invocation list entries produced from evaluation of semantically identical 
s or 
s with the same (possibly empty) set of captured outer variable instances are permitted (but not required) to be equal.
Equality operators and null
The 
 and 
 operators permit one operand to be a value of a nullable type and the other to be the 
 literal, even if no predefined or user-defined operator (in unlifted or lifted form) exists for the operation.
For an operation of one of the forms
where 
 is an expression of a nullable type, if operator overload resolution (
) fails to find an applicable operator, the result is instead computed from the 
 property of 
. Specifically, the first two forms are translated into 
, and last two forms are translated into 
.
The is operator
The 
 operator is used to dynamically check if the run-time type of an object is compatible with a given type. The result of the operation 
, where 
 is an expression and 
 is a type, is a boolean value indicating whether 
 can successfully be converted to type 
 by a reference conversion, a boxing conversion, or an unboxing conversion. The operation is evaluated as follows, after type arguments have been substituted for all type parameters:
If 
 is an anonymous function, a compile-time error occurs
If 
 is a method group or the 
 literal, of if the type of 
 is a reference type or a nullable type and the value of 
 is null, the result is false.
Otherwise, let 
 represent the dynamic type of 
 as follows:
If the type of 
 is a reference type, 
 is the run-time type of the instance reference by 
.
If the type of 
 is a nullable type, 
 is the underlying type of that nullable type.
If the type of 
 is a non-nullable value type, 
 is the type of 
.
The result of the operation depends on 
 and 
 as follows:
If 
 is a reference type, the result is true if 
 and 
 are the same type, if 
 is a reference type and an implicit reference conversion from 
 to 
 exists, or if 
 is a value type and a boxing conversion from 
 to 
 exists.
If 
 is a nullable type, the result is true if 
 is the underlying type of 
.
If 
 is a non-nullable value type, the result is true if 
 and 
 are the same type.
Otherwise, the result is false.
Note that user defined conversions, are not considered by the 
 operator.
The as operator
The 
 operator is used to explicitly convert a value to a given reference type or nullable type. Unlike a cast expression (
), the 
 operator never throws an exception. Instead, if the indicated conversion is not possible, the resulting value is 
.
In an operation of the form 
, 
 must be an expression and 
 must be a reference type, a type parameter known to be a reference type, or a nullable type. Furthermore, at least one of the following must be true, or otherwise a compile-time error occurs:
An identity (
), implicit nullable (
), implicit reference (
), boxing (
), explicit nullable (
), explicit reference (
), or unboxing (
) conversion exists from 
 to 
.
The type of 
 or 
 is an open type.
 is the 
 literal.
If the compile-time type of 
 is not 
, the operation 
 produces the same result as
except that 
 is only evaluated once. The compiler can be expected to optimize 
 to perform at most one dynamic type check as opposed to the two dynamic type checks implied by the expansion above.
If the compile-time type of 
 is 
, unlike the cast operator the 
 operator is not dynamically bound (
). Therefore the expansion in this case is:
Note that some conversions, such as user defined conversions, are not possible with the 
 operator and should instead be performed using cast expressions.
In the example
the type parameter 
 of 
 is known to be a reference type, because it has the class constraint. The type parameter 
 of 
 is not however; hence the use of the 
 operator in 
 is disallowed.
Logical operators
The 
, 
, and 
 operators are called the logical operators.
If an operand of a logical operator has the compile-time type 
, then the expression is dynamically bound (
). In this case the compile-time type of the expression is 
, and the resolution described below will take place at run-time using the run-time type of those operands that have the compile-time type 
.
For an operation of the form 
, where 
 is one of the logical operators, overload resolution (
) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.
The predefined logical operators are described in the following sections.
Integer logical operators
The predefined integer logical operators are:
The 
 operator computes the bitwise logical 
 of the two operands, the 
 operator computes the bitwise logical 
 of the two operands, and the 
 operator computes the bitwise logical exclusive 
 of the two operands. No overflows are possible from these operations.
Enumeration logical operators
Every enumeration type 
 implicitly provides the following predefined logical operators:
The result of evaluating 
, where 
 and 
 are expressions of an enumeration type 
 with an underlying type 
, and 
 is one of the logical operators, is exactly the same as evaluating 
. In other words, the enumeration type logical operators simply perform the logical operation on the underlying type of the two operands.
Boolean logical operators
The predefined boolean logical operators are:
The result of 
 is 
 if both 
 and 
 are 
. Otherwise, the result is 
.
The result of 
 is 
 if either 
 or 
 is 
. Otherwise, the result is 
.
The result of 
 is 
 if 
 is 
 and 
 is 
, or 
 is 
 and 
 is 
. Otherwise, the result is 
. When the operands are of type 
, the 
 operator computes the same result as the 
 operator.
Nullable boolean logical operators
The nullable boolean type 
 can represent three values, 
, 
, and 
, and is conceptually similar to the three-valued type used for boolean expressions in SQL. To ensure that the results produced by the 
 and 
 operators for 
 operands are consistent with SQL's three-valued logic, the following predefined operators are provided:
The following table lists the results produced by these operators for all combinations of the values 
, 
, and 
.
Conditional logical operators
The 
 and 
 operators are called the conditional logical operators. They are also called the ""short-circuiting"" logical operators.
The 
 and 
 operators are conditional versions of the 
 and 
 operators:
The operation 
 corresponds to the operation 
, except that 
 is evaluated only if 
 is not 
.
The operation 
 corresponds to the operation 
, except that 
 is evaluated only if 
 is not 
.
If an operand of a conditional logical operator has the compile-time type 
, then the expression is dynamically bound (
). In this case the compile-time type of the expression is 
, and the resolution described below will take place at run-time using the run-time type of those operands that have the compile-time type 
.
An operation of the form 
 or 
 is processed by applying overload resolution (
) as if the operation was written 
 or 
. Then,
If overload resolution fails to find a single best operator, or if overload resolution selects one of the predefined integer logical operators, a binding-time error occurs.
Otherwise, if the selected operator is one of the predefined boolean logical operators (
) or nullable boolean logical operators (
), the operation is processed as described in 
.
Otherwise, the selected operator is a user-defined operator, and the operation is processed as described in 
.
It is not possible to directly overload the conditional logical operators. However, because the conditional logical operators are evaluated in terms of the regular logical operators, overloads of the regular logical operators are, with certain restrictions, also considered overloads of the conditional logical operators. This is described further in 
.
Boolean conditional logical operators
When the operands of 
 or 
 are of type 
, or when the operands are of types that do not define an applicable 
 or 
, but do define implicit conversions to 
, the operation is processed as follows:
The operation 
 is evaluated as 
. In other words, 
 is first evaluated and converted to type 
. Then, if 
 is 
, 
 is evaluated and converted to type 
, and this becomes the result of the operation. Otherwise, the result of the operation is 
.
The operation 
 is evaluated as 
. In other words, 
 is first evaluated and converted to type 
. Then, if 
 is 
, the result of the operation is 
. Otherwise, 
 is evaluated and converted to type 
, and this becomes the result of the operation.
User-defined conditional logical operators
When the operands of 
 or 
 are of types that declare an applicable user-defined 
 or 
, both of the following must be true, where 
 is the type in which the selected operator is declared:
The return type and the type of each parameter of the selected operator must be 
. In other words, the operator must compute the logical 
 or the logical 
 of two operands of type 
, and must return a result of type 
.
 must contain declarations of 
 and 
.
A binding-time error occurs if either of these requirements is not satisfied. Otherwise, the 
 or 
 operation is evaluated by combining the user-defined 
 or 
 with the selected user-defined operator:
The operation 
 is evaluated as 
, where 
 is an invocation of the 
 declared in 
, and 
 is an invocation of the selected 
. In other words, 
 is first evaluated and 
 is invoked on the result to determine if 
 is definitely false. Then, if 
 is definitely false, the result of the operation is the value previously computed for 
. Otherwise, 
 is evaluated, and the selected 
 is invoked on the value previously computed for 
 and the value computed for 
 to produce the result of the operation.
The operation 
 is evaluated as 
, where 
 is an invocation of the 
 declared in 
, and 
 is an invocation of the selected 
. In other words, 
 is first evaluated and 
 is invoked on the result to determine if 
 is definitely true. Then, if 
 is definitely true, the result of the operation is the value previously computed for 
. Otherwise, 
 is evaluated, and the selected 
 is invoked on the value previously computed for 
 and the value computed for 
 to produce the result of the operation.
In either of these operations, the expression given by 
 is only evaluated once, and the expression given by 
 is either not evaluated or evaluated exactly once.
For an example of a type that implements 
 and 
, see 
.
The null coalescing operator
The 
 operator is called the null coalescing operator.
A null coalescing expression of the form 
 requires 
 to be of a nullable type or reference type. If 
 is non-null, the result of 
 is 
; otherwise, the result is 
. The operation evaluates 
 only if 
 is null.
The null coalescing operator is right-associative, meaning that operations are grouped from right to left. For example, an expression of the form 
 is evaluated as 
. In general terms, an expression of the form 
 returns the first of the operands that is non-null, or null if all operands are null.
The type of the expression 
 depends on which implicit conversions are available on the operands. In order of preference, the type of 
 is 
, 
, or 
, where 
 is the type of 
 (provided that 
 has a type), 
 is the type of 
 (provided that 
 has a type), and 
 is the underlying type of 
 if 
 is a nullable type, or 
 otherwise. Specifically, 
 is processed as follows:
If 
 exists and is not a nullable type or a reference type, a compile-time error occurs.
If 
 is a dynamic expression, the result type is 
. At run-time, 
 is first evaluated. If 
 is not null, 
 is converted to dynamic, and this becomes the result. Otherwise, 
 is evaluated, and this becomes the result.
Otherwise, if 
 exists and is a nullable type and an implicit conversion exists from 
 to 
, the result type is 
. At run-time, 
 is first evaluated. If 
 is not null, 
 is unwrapped to type 
, and this becomes the result. Otherwise, 
 is evaluated and converted to type 
, and this becomes the result.
Otherwise, if 
 exists and an implicit conversion exists from 
 to 
, the result type is 
. At run-time, 
 is first evaluated. If 
 is not null, 
 becomes the result. Otherwise, 
 is evaluated and converted to type 
, and this becomes the result.
Otherwise, if 
 has a type 
 and an implicit conversion exists from 
 to 
, the result type is 
. At run-time, 
 is first evaluated. If 
 is not null, 
 is unwrapped to type 
 (if 
 exists and is nullable) and converted to type 
, and this becomes the result. Otherwise, 
 is evaluated and becomes the result.
Otherwise, 
 and 
 are incompatible, and a compile-time error occurs.
Conditional operator
The 
 operator is called the conditional operator. It is at times also called the ternary operator.
A conditional expression of the form 
 first evaluates the condition 
. Then, if 
 is 
, 
 is evaluated and becomes the result of the operation. Otherwise, 
 is evaluated and becomes the result of the operation. A conditional expression never evaluates both 
 and 
.
The conditional operator is right-associative, meaning that operations are grouped from right to left. For example, an expression of the form 
 is evaluated as 
.
The first operand of the 
 operator must be an expression that can be implicitly converted to 
, or an expression of a type that implements 
. If neither of these requirements is satisfied, a compile-time error occurs.
The second and third operands, 
 and 
, of the 
 operator control the type of the conditional expression.
If 
 has type 
 and 
 has type 
 then
If an implicit conversion (
) exists from 
 to 
, but not from 
 to 
, then 
 is the type of the conditional expression.
If an implicit conversion (
) exists from 
 to 
, but not from 
 to 
, then 
 is the type of the conditional expression.
Otherwise, no expression type can be determined, and a compile-time error occurs.
If only one of 
 and 
 has a type, and both 
 and 
, of are implicitly convertible to that type, then that is the type of the conditional expression.
Otherwise, no expression type can be determined, and a compile-time error occurs.
The run-time processing of a conditional expression of the form 
 consists of the following steps:
First, 
 is evaluated, and the 
 value of 
 is determined:
If an implicit conversion from the type of 
 to 
 exists, then this implicit conversion is performed to produce a 
 value.
Otherwise, the 
 defined by the type of 
 is invoked to produce a 
 value.
If the 
 value produced by the step above is 
, then 
 is evaluated and converted to the type of the conditional expression, and this becomes the result of the conditional expression.
Otherwise, 
 is evaluated and converted to the type of the conditional expression, and this becomes the result of the conditional expression.
Anonymous function expressions
An 
 is an expression that represents an ""in-line"" method definition. An anonymous function does not have a value or type in and of itself, but is convertible to a compatible delegate or expression tree type. The evaluation of an anonymous function conversion depends on the target type of the conversion: If it is a delegate type, the conversion evaluates to a delegate value referencing the method which the anonymous function defines. If it is an expression tree type, the conversion evaluates to an expression tree which represents the structure of the method as an object structure.
For historical reasons there are two syntactic flavors of anonymous functions, namely 
s and 
s. For almost all purposes, 
s are more concise and expressive than 
s, which remain in the language for backwards compatibility.
The 
 operator has the same precedence as assignment (
) and is right-associative.
An anonymous function with the 
 modifier is an async function and follows the rules described in 
.
The parameters of an anonymous function in the form of a 
 can be explicitly or implicitly typed. In an explicitly typed parameter list, the type of each parameter is explicitly stated. In an implicitly typed parameter list, the types of the parameters are inferred from the context in which the anonymous function occurs—specifically, when the anonymous function is converted to a compatible delegate type or expression tree type, that type provides the parameter types (
).
In an anonymous function with a single, implicitly typed parameter, the parentheses may be omitted from the parameter list. In other words, an anonymous function of the form
can be abbreviated to
The parameter list of an anonymous function in the form of an 
 is optional. If given, the parameters must be explicitly typed. If not, the anonymous function is convertible to a delegate with any parameter list not containing 
 parameters.
A 
 body of an anonymous function is reachable (
) unless the anonymous function occurs inside an unreachable statement.
Some examples of anonymous functions follow below:
The behavior of 
s and 
s is the same except for the following points:
s permit the parameter list to be omitted entirely, yielding convertibility to delegate types of any list of value parameters.
s permit parameter types to be omitted and inferred whereas 
s require parameter types to be explicitly stated.
The body of a 
 can be an expression or a statement block whereas the body of an 
 must be a statement block.
Only 
s have conversions to compatible expression tree types (
).
Anonymous function signatures
The optional 
 of an anonymous function defines the names and optionally the types of the formal parameters for the anonymous function. The scope of the parameters of the anonymous function is the 
. (
) Together with the parameter list (if given) the anonymous-method-body constitutes a declaration space (
). It is thus a compile-time error for the name of a parameter of the anonymous function to match the name of a local variable, local constant or parameter whose scope includes the 
 or 
.
If an anonymous function has an 
, then the set of compatible delegate types and expression tree types is restricted to those that have the same parameter types and modifiers in the same order. In contrast to method group conversions (
), contra-variance of anonymous function parameter types is not supported. If an anonymous function does not have an 
, then the set of compatible delegate types and expression tree types is restricted to those that have no 
 parameters.
Note that an 
 cannot include attributes or a parameter array. Nevertheless, an 
 may be compatible with a delegate type whose parameter list contains a parameter array.
Note also that conversion to an expression tree type, even if compatible, may still fail at compile-time (
).
Anonymous function bodies
The body (
 or 
) of an anonymous function is subject to the following rules:
If the anonymous function includes a signature, the parameters specified in the signature are available in the body. If the anonymous function has no signature it can be converted to a delegate type or expression type having parameters (
), but the parameters cannot be accessed in the body.
Except for 
 or 
 parameters specified in the signature (if any) of the nearest enclosing anonymous function, it is a compile-time error for the body to access a 
 or 
 parameter.
When the type of 
 is a struct type, it is a compile-time error for the body to access 
. This is true whether the access is explicit (as in 
) or implicit (as in 
 where 
 is an instance member of the struct). This rule simply prohibits such access and does not affect whether member lookup results in a member of the struct.
The body has access to the outer variables (
) of the anonymous function. Access of an outer variable will reference the instance of the variable that is active at the time the 
 or 
 is evaluated (
).
It is a compile-time error for the body to contain a 
 statement, 
 statement, or 
 statement whose target is outside the body or within the body of a contained anonymous function.
A 
 statement in the body returns control from an invocation of the nearest enclosing anonymous function, not from the enclosing function member. An expression specified in a 
 statement must be implicitly convertible to the return type of the delegate type or expression tree type to which the nearest enclosing 
 or 
 is converted (
).
It is explicitly unspecified whether there is any way to execute the block of an anonymous function other than through evaluation and invocation of the 
 or 
. In particular, the compiler may choose to implement an anonymous function by synthesizing one or more named methods or types. The names of any such synthesized elements must be of a form reserved for compiler use.
Overload resolution and anonymous functions
Anonymous functions in an argument list participate in type inference and overload resolution. Please refer to 
 and 
 for the exact rules.
The following example illustrates the effect of anonymous functions on overload resolution.
The 
 class has two 
 methods. Each takes a 
 argument, which extracts the value to sum over from a list item. The extracted value can be either an 
 or a 
 and the resulting sum is likewise either an 
 or a 
.
The 
 methods could for example be used to compute sums from a list of detail lines in an order.
In the first invocation of 
, both 
 methods are applicable because the anonymous function 
 is compatible with both 
 and 
. However, overload resolution picks the first 
 method because the conversion to 
 is better than the conversion to 
.
In the second invocation of 
, only the second 
 method is applicable because the anonymous function 
 produces a value of type 
. Thus, overload resolution picks the second 
 method for that invocation.
Anonymous functions and dynamic binding
An anonymous function cannot be a receiver, argument or operand of a dynamically bound operation.
Outer variables
Any local variable, value parameter, or parameter array whose scope includes the 
 or 
 is called an 
 of the anonymous function. In an instance function member of a class, the 
 value is considered a value parameter and is an outer variable of any anonymous function contained within the function member.
Captured outer variables
When an outer variable is referenced by an anonymous function, the outer variable is said to have been 
 by the anonymous function. Ordinarily, the lifetime of a local variable is limited to execution of the block or statement with which it is associated (
). However, the lifetime of a captured outer variable is extended at least until the delegate or expression tree created from the anonymous function becomes eligible for garbage collection.
In the example
the local variable 
 is captured by the anonymous function, and the lifetime of 
 is extended at least until the delegate returned from 
 becomes eligible for garbage collection (which doesn't happen until the very end of the program). Since each invocation of the anonymous function operates on the same instance of 
, the output of the example is:
When a local variable or a value parameter is captured by an anonymous function, the local variable or parameter is no longer considered to be a fixed variable (
), but is instead considered to be a moveable variable. Thus any 
 code that takes the address of a captured outer variable must first use the 
 statement to fix the variable.
Note that unlike an uncaptured variable, a captured local variable can be simultaneously exposed to multiple threads of execution.
Instantiation of local variables
A local variable is considered to be 
 when execution enters the scope of the variable. For example, when the following method is invoked, the local variable 
 is instantiated and initialized three times—once for each iteration of the loop.
However, moving the declaration of 
 outside the loop results in a single instantiation of 
:
When not captured, there is no way to observe exactly how often a local variable is instantiated—because the lifetimes of the instantiations are disjoint, it is possible for each instantation to simply use the same storage location. However, when an anonymous function captures a local variable, the effects of instantiation become apparent.
The example
produces the output:
However, when the declaration of 
 is moved outside the loop:
the output is:
If a for-loop declares an iteration variable, that variable itself is considered to be declared outside of the loop. Thus, if the example is changed to capture the iteration variable itself:
only one instance of the iteration variable is captured, which produces the output:
It is possible for anonymous function delegates to share some captured variables yet have separate instances of others. For example, if 
 is changed to
the three delegates capture the same instance of 
 but separate instances of 
, and the output is:
Separate anonymous functions can capture the same instance of an outer variable. In the example:
the two anonymous functions capture the same instance of the local variable 
, and they can thus ""communicate"" through that variable. The output of the example is:
Evaluation of anonymous function expressions
An anonymous function 
 must always be converted to a delegate type 
 or an expression tree type 
, either directly or through the execution of a delegate creation expression 
. This conversion determines the result of the anonymous function, as described in 
.
Query expressions
 provide a language integrated syntax for queries that is similar to relational and hierarchical query languages such as SQL and XQuery.
A query expression begins with a 
 clause and ends with either a 
 or 
 clause. The initial 
 clause can be followed by zero or more 
, 
, 
, 
 or 
 clauses. Each 
 clause is a generator introducing a 
 which ranges over the elements of a 
. Each 
 clause introduces a range variable representing a value computed by means of previous range variables. Each 
 clause is a filter that excludes items from the result. Each 
 clause compares specified keys of the source sequence with keys of another sequence, yielding matching pairs. Each 
 clause reorders items according to specified criteria.The final 
 or 
 clause specifies the shape of the result in terms of the range variables. Finally, an 
 clause can be used to ""splice"" queries by treating the results of one query as a generator in a subsequent query.
Ambiguities in query expressions
Query expressions contain a number of ""contextual keywords"", i.e., identifiers that have special meaning in a given context. Specifically these are 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
 and 
. In order to avoid ambiguities in query expressions caused by mixed use of these identifiers as keywords or simple names, these identifiers are considered keywords when occurring anywhere within a query expression.
For this purpose, a query expression is any expression that starts with ""
"" followed by any token except ""
"", ""
"" or ""
"".
In order to use these words as identifiers within a query expression, they can be prefixed with ""
"" (
).
Query expression translation
The C# language does not specify the execution semantics of query expressions. Rather, query expressions are translated into invocations of methods that adhere to the 
query expression pattern
 (
). Specifically, query expressions are translated into invocations of methods named 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and 
.These methods are expected to have particular signatures and result types, as described in 
. These methods can be instance methods of the object being queried or extension methods that are external to the object, and they implement the actual execution of the query.
The translation from query expressions to method invocations is a syntactic mapping that occurs before any type binding or overload resolution has been performed. The translation is guaranteed to be syntactically correct, but it is not guaranteed to produce semantically correct C# code. Following translation of query expressions, the resulting method invocations are processed as regular method invocations, and this may in turn uncover errors, for example if the methods do not exist, if arguments have wrong types, or if the methods are generic and type inference fails.
A query expression is processed by repeatedly applying the following translations until no further reductions are possible. The translations are listed in order of application: each section assumes that the translations in the preceding sections have been performed exhaustively, and once exhausted, a section will not later be revisited in the processing of the same query expression.
Assignment to range variables is not allowed in query expressions. However a C# implementation is permitted to not always enforce this restriction, since this may sometimes not be possible with the syntactic translation scheme presented here.
Certain translations inject range variables with transparent identifiers denoted by 
. The special properties of transparent identifiers are discussed further in 
.
Select and groupby clauses with continuations
A query expression with a continuation
is translated into
The translations in the following sections assume that queries have no 
 continuations.
The example
is translated into
the final translation of which is
Explicit range variable types
A 
 clause that explicitly specifies a range variable type
is translated into
A 
 clause that explicitly specifies a range variable type
is translated into
The translations in the following sections assume that queries have no explicit range variable types.
The example
is translated into
the final translation of which is
Explicit range variable types are useful for querying collections that implement the non-generic 
 interface, but not the generic 
 interface. In the example above, this would be the case if 
 were of type 
.
Degenerate query expressions
A query expression of the form
is translated into
The example
is translated into
A degenerate query expression is one that trivially selects the elements of the source. A later phase of the translation removes degenerate queries introduced by other translation steps by replacing them with their source. It is important however to ensure that the result of a query expression is never the source object itself, as that would reveal the type and identity of the source to the client of the query. Therefore this step protects degenerate queries written directly in source code by explicitly calling 
 on the source. It is then up to the implementers of 
 and other query operators to ensure that these methods never return the source object itself.
From, let, where, join and orderby clauses
A query expression with a second 
 clause followed by a 
 clause
is translated into
A query expression with a second 
 clause followed by something other than a 
 clause:
is translated into
A query expression with a 
 clause
is translated into
A query expression with a 
 clause
is translated into
A query expression with a 
 clause without an 
 followed by a 
 clause
is translated into
A query expression with a 
 clause without an 
 followed by something other than a 
 clause
is translated into
A query expression with a 
 clause with an 
 followed by a 
 clause
is translated into
A query expression with a 
 clause with an 
 followed by something other than a 
 clause
is translated into
A query expression with an 
 clause
is translated into
If an ordering clause specifies a 
 direction indicator, an invocation of 
 or 
 is produced instead.
The following translations assume that there are no 
, 
, 
 or 
 clauses, and no more than the one initial 
 clause in each query expression.
The example
is translated into
The example
is translated into
the final translation of which is
where 
 is a compiler generated identifier that is otherwise invisible and inaccessible.
The example
is translated into
the final translation of which is
where 
 is a compiler generated identifier that is otherwise invisible and inaccessible.
The example
is translated into
The example
is translated into
the final translation of which is
where 
 and 
 are compiler generated identifiers that are otherwise invisible and inaccessible.
The example
has the final translation
Select clauses
A query expression of the form
is translated into
except when v is the identifier x, the translation is simply
For example
is simply translated into
Groupby clauses
A query expression of the form
is translated into
except when v is the identifier x, the translation is
The example
is translated into
Transparent identifiers
Certain translations inject range variables with 
 denoted by 
. Transparent identifiers are not a proper language feature; they exist only as an intermediate step in the query expression translation process.
When a query translation injects a transparent identifier, further translation steps propagate the transparent identifier into anonymous functions and anonymous object initializers. In those contexts, transparent identifiers have the following behavior:
When a transparent identifier occurs as a parameter in an anonymous function, the members of the associated anonymous type are automatically in scope in the body of the anonymous function.
When a member with a transparent identifier is in scope, the members of that member are in scope as well.
When a transparent identifier occurs as a member declarator in an anonymous object initializer, it introduces a member with a transparent identifier.
In the translation steps described above, transparent identifiers are always introduced together with anonymous types, with the intent of capturing multiple range variables as members of a single object. An implementation of C# is permitted to use a different mechanism than anonymous types to group together multiple range variables. The following translation examples assume that anonymous types are used, and show how transparent identifiers can be translated away.
The example
is translated into
which is further translated into
which, when transparent identifiers are erased, is equivalent to
where 
 is a compiler generated identifier that is otherwise invisible and inaccessible.
The example
is translated into
which is further reduced to
the final translation of which is
where 
, 
, and 
 are compiler generated identifiers that are otherwise invisible and inaccessible.
The query expression pattern
The 
 establishes a pattern of methods that types can implement to support query expressions. Because query expressions are translated to method invocations by means of a syntactic mapping, types have considerable flexibility in how they implement the query expression pattern. For example, the methods of the pattern can be implemented as instance methods or as extension methods because the two have the same invocation syntax, and the methods can request delegates or expression trees because anonymous functions are convertible to both.
The recommended shape of a generic type 
 that supports the query expression pattern is shown below. A generic type is used in order to illustrate the proper relationships between parameter and result types, but it is possible to implement the pattern for non-generic types as well.
The methods above use the generic delegate types 
 and 
, but they could equally well have used other delegate or expression tree types with the same relationships in parameter and result types.
Notice the recommended relationship between 
 and 
 which ensures that the 
 and 
 methods are available only on the result of an 
 or 
. Also notice the recommended shape of the result of 
 -- a sequence of sequences, where each inner sequence has an additional 
 property.
The 
 namespace provides an implementation of the query operator pattern for any type that implements the 
 interface.
Assignment operators
The assignment operators assign a new value to a variable, a property, an event, or an indexer element.
The left operand of an assignment must be an expression classified as a variable, a property access, an indexer access, or an event access.
The 
 operator is called the 
. It assigns the value of the right operand to the variable, property, or indexer element given by the left operand. The left operand of the simple assignment operator may not be an event access (except as described in 
). The simple assignment operator is described in 
.
The assignment operators other than the 
 operator are called the 
. These operators perform the indicated operation on the two operands, and then assign the resulting value to the variable, property, or indexer element given by the left operand. The compound assignment operators are described in 
.
The 
 and 
 operators with an event access expression as the left operand are called the 
event assignment operators
. No other assignment operator is valid with an event access as the left operand. The event assignment operators are described in 
.
The assignment operators are right-associative, meaning that operations are grouped from right to left. For example, an expression of the form 
 is evaluated as 
.
Simple assignment
The 
 operator is called the simple assignment operator.
If the left operand of a simple assignment is of the form 
 or 
 where 
 has the compile-time type 
, then the assignment is dynamically bound (
). In this case the compile-time type of the assignment expression is 
, and the resolution described below will take place at run-time based on the run-time type of 
.
In a simple assignment, the right operand must be an expression that is implicitly convertible to the type of the left operand. The operation assigns the value of the right operand to the variable, property, or indexer element given by the left operand.
The result of a simple assignment expression is the value assigned to the left operand. The result has the same type as the left operand and is always classified as a value.
If the left operand is a property or indexer access, the property or indexer must have a 
 accessor. If this is not the case, a binding-time error occurs.
The run-time processing of a simple assignment of the form 
 consists of the following steps:
If 
 is classified as a variable:
 is evaluated to produce the variable.
 is evaluated and, if required, converted to the type of 
 through an implicit conversion (
).
If the variable given by 
 is an array element of a 
, a run-time check is performed to ensure that the value computed for 
 is compatible with the array instance of which 
 is an element. The check succeeds if 
 is 
, or if an implicit reference conversion (
) exists from the actual type of the instance referenced by 
 to the actual element type of the array instance containing 
. Otherwise, a 
 is thrown.
The value resulting from the evaluation and conversion of 
 is stored into the location given by the evaluation of 
.
If 
 is classified as a property or indexer access:
The instance expression (if 
 is not 
) and the argument list (if 
 is an indexer access) associated with 
 are evaluated, and the results are used in the subsequent 
 accessor invocation.
 is evaluated and, if required, converted to the type of 
 through an implicit conversion (
).
The 
 accessor of 
 is invoked with the value computed for 
 as its 
 argument.
The array co-variance rules (
) permit a value of an array type 
 to be a reference to an instance of an array type 
, provided an implicit reference conversion exists from 
 to 
. Because of these rules, assignment to an array element of a 
 requires a run-time check to ensure that the value being assigned is compatible with the array instance. In the example
the last assignment causes a 
 to be thrown because an instance of 
 cannot be stored in an element of a 
.
When a property or indexer declared in a 
 is the target of an assignment, the instance expression associated with the property or indexer access must be classified as a variable. If the instance expression is classified as a value, a binding-time error occurs. Because of 
, the same rule also applies to fields.
Given the declarations:
in the example
the assignments to 
, 
, 
, and 
 are permitted because 
 and 
 are variables. However, in the example
the assignments are all invalid, since 
 and 
 are not variables.
Compound assignment
If the left operand of a compound assignment is of the form 
 or 
 where 
 has the compile-time type 
, then the assignment is dynamically bound (
). In this case the compile-time type of the assignment expression is 
, and the resolution described below will take place at run-time based on the run-time type of 
.
An operation of the form 
 is processed by applying binary operator overload resolution (
) as if the operation was written 
. Then,
If the return type of the selected operator is implicitly convertible to the type of 
, the operation is evaluated as 
, except that 
 is evaluated only once.
Otherwise, if the selected operator is a predefined operator, if the return type of the selected operator is explicitly convertible to the type of 
, and if 
 is implicitly convertible to the type of 
 or the operator is a shift operator, then the operation is evaluated as 
, where 
 is the type of 
, except that 
 is evaluated only once.
Otherwise, the compound assignment is invalid, and a binding-time error occurs.
The term ""evaluated only once"" means that in the evaluation of 
, the results of any constituent expressions of 
 are temporarily saved and then reused when performing the assignment to 
. For example, in the assignment 
, where 
 is a method returning 
, and 
 and 
 are methods returning 
, the methods are invoked only once, in the order 
, 
, 
.
When the left operand of a compound assignment is a property access or indexer access, the property or indexer must have both a 
 accessor and a 
 accessor. If this is not the case, a binding-time error occurs.
The second rule above permits 
 to be evaluated as 
 in certain contexts. The rule exists such that the predefined operators can be used as compound operators when the left operand is of type 
, 
, 
, 
, or 
. Even when both arguments are of one of those types, the predefined operators produce a result of type 
, as described in 
. Thus, without a cast it would not be possible to assign the result to the left operand.
The intuitive effect of the rule for predefined operators is simply that 
 is permitted if both of 
 and 
 are permitted. In the example
the intuitive reason for each error is that a corresponding simple assignment would also have been an error.
This also means that compound assignment operations support lifted operations. In the example
the lifted operator 
 is used.
Event assignment
If the left operand of a 
 or 
 operator is classified as an event access, then the expression is evaluated as follows:
The instance expression, if any, of the event access is evaluated.
The right operand of the 
 or 
 operator is evaluated, and, if required, converted to the type of the left operand through an implicit conversion (
).
An event accessor of the event is invoked, with argument list consisting of the right operand, after evaluation and, if necessary, conversion. If the operator was 
, the 
 accessor is invoked; if the operator was 
, the 
 accessor is invoked.
An event assignment expression does not yield a value. Thus, an event assignment expression is valid only in the context of a 
 (
).
Expression
An 
 is either a 
 or an 
.
Constant expressions
A 
 is an expression that can be fully evaluated at compile-time.
A constant expression must be the 
 literal or a value with one of  the following types: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or any enumeration type. Only the following constructs are permitted in constant expressions:
Literals (including the 
 literal).
References to 
 members of class and struct types.
References to members of enumeration types.
References to 
 parameters or local variables
Parenthesized sub-expressions, which are themselves constant expressions.
Cast expressions, provided the target type is one of the types listed above.
 and 
 expressions
Default value expressions
Nameof expressions
The predefined 
, 
, 
, and 
 unary operators.
The predefined 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and 
 binary operators, provided each operand is of a type listed above.
The 
 conditional operator.
The following conversions are permitted in constant expressions:
Identity conversions
Numeric conversions
Enumeration conversions
Constant expression conversions
Implicit and explicit reference conversions, provided that the source of the conversions is a constant expression that evaluates to the null value.
Other conversions including boxing, unboxing and implicit reference conversions of non-null values are not permitted in constant expressions. For example:
the initialization of iis an error because a boxing conversion is required. The initialization of str is an error because an implicit reference conversion from a non-null value is required.
Whenever an expression fulfills the requirements listed above, the expression is evaluated at compile-time. This is true even if the expression is a sub-expression of a larger expression that contains non-constant constructs.
The compile-time evaluation of constant expressions uses the same rules as run-time evaluation of non-constant expressions, except that where run-time evaluation would have thrown an exception, compile-time evaluation causes a compile-time error to occur.
Unless a constant expression is explicitly placed in an 
 context, overflows that occur in integral-type arithmetic operations and conversions during the compile-time evaluation of the expression always cause compile-time errors (
).
Constant expressions occur in the contexts listed below. In these contexts, a compile-time error occurs if an expression cannot be fully evaluated at compile-time.
Constant declarations (
).
Enumeration member declarations (
).
Default arguments of formal parameter lists (
)
 labels of a 
 statement (
).
 statements (
).
Dimension lengths in an array creation expression (
) that includes an initializer.
Attributes (
).
An implicit constant expression conversion (
) permits a constant expression of type 
 to be converted to 
, 
, 
, 
, 
, or 
, provided the value of the constant expression is within the range of the destination type.
Boolean expressions
A 
 is an expression that yields a result of type 
; either directly or through application of 
 in certain contexts as specified in the following.
The controlling conditional expression of an 
 (
), 
 (
), 
 (
), or 
 (
) is a 
. The controlling conditional expression of the 
 operator (
) follows the same rules as a 
, but for reasons of operator precedence is classified as a 
.
A 
 
 is required to be able to produce a value of type 
, as follows:
If 
 is implicitly convertible to 
 then at runtime that implicit conversion is applied.
Otherwise, unary operator overload resolution (
) is used to find a unique best implementation of operator 
 on 
, and that implementation is applied at runtime.
If no such operator is found, a binding-time error occurs.
The 
 struct type in 
 provides an example of a type that implements 
 and 
.
Statements
C# provides a variety of statements. Most of these statements will be familiar to developers who have programmed in C and C++.
The 
 nonterminal is used for statements that appear within other statements. The use of 
 rather than 
 excludes the use of declaration statements and labeled statements in these contexts. The example
results in a compile-time error because an 
 statement requires an 
 rather than a 
 for its if branch. If this code were permitted, then the variable 
 would be declared, but it could never be used. Note, however, that by placing 
's declaration in a block, the example is valid.
End points and reachability
Every statement has an 
. In intuitive terms, the end point of a statement is the location that immediately follows the statement. The execution rules for composite statements (statements that contain embedded statements) specify the action that is taken when control reaches the end point of an embedded statement. For example, when control reaches the end point of a statement in a block, control is transferred to the next statement in the block.
If a statement can possibly be reached by execution, the statement is said to be 
. Conversely, if there is no possibility that a statement will be executed, the statement is said to be 
.
In the example
the second invocation of 
 is unreachable because there is no possibility that the statement will be executed.
A warning is reported if the compiler determines that a statement is unreachable. It is specifically not an error for a statement to be unreachable.
To determine whether a particular statement or end point is reachable, the compiler performs flow analysis according to the reachability rules defined for each statement. The flow analysis takes into account the values of constant expressions (
) that control the behavior of statements, but the possible values of non-constant expressions are not considered. In other words, for purposes of control flow analysis, a non-constant expression of a given type is considered to have any possible value of that type.
In the example
the boolean expression of the 
 statement is a constant expression because both operands of the 
 operator are constants. As the constant expression is evaluated at compile-time, producing the value 
, the 
 invocation is considered unreachable. However, if 
 is changed to be a local variable
the 
 invocation is considered reachable, even though, in reality, it will never be executed.
The 
 of a function member is always considered reachable. By successively evaluating the reachability rules of each statement in a block, the reachability of any given statement can be determined.
In the example
the reachability of the second 
 is determined as follows:
The first 
 expression statement is reachable because the block of the 
 method is reachable.
The end point of the first 
 expression statement is reachable because that statement is reachable.
The 
 statement is reachable because the end point of the first 
 expression statement is reachable.
The second 
 expression statement is reachable because the boolean expression of the 
 statement does not have the constant value 
.
There are two situations in which it is a compile-time error for the end point of a statement to be reachable:
Because the 
 statement does not permit a switch section to ""fall through"" to the next switch section, it is a compile-time error for the end point of the statement list of a switch section to be reachable. If this error occurs, it is typically an indication that a 
 statement is missing.
It is a compile-time error for the end point of the block of a function member that computes a value to be reachable. If this error occurs, it typically is an indication that a 
 statement is missing.
Blocks
A 
 permits multiple statements to be written in contexts where a single statement is allowed.
A 
 consists of an optional 
 (
), enclosed in braces. If the statement list is omitted, the block is said to be empty.
A block may contain declaration statements (
). The scope of a local variable or constant declared in a block is the block.
A block is executed as follows:
If the block is empty, control is transferred to the end point of the block.
If the block is not empty, control is transferred to the statement list. When and if control reaches the end point of the statement list, control is transferred to the end point of the block.
The statement list of a block is reachable if the block itself is reachable.
The end point of a block is reachable if the block is empty or if the end point of the statement list is reachable.
A 
 that contains one or more 
 statements (
) is called an iterator block. Iterator blocks are used to implement function members as iterators (
). Some additional restrictions apply to iterator blocks:
It is a compile-time error for a 
 statement to appear in an iterator block (but 
 statements are permitted).
It is a compile-time error for an iterator block to contain an unsafe context (
). An iterator block always defines a safe context, even when its declaration is nested in an unsafe context.
Statement lists
A 
 consists of one or more statements written in sequence. Statement lists occur in 
s (
) and in 
s (
).
A statement list is executed by transferring control to the first statement. When and if control reaches the end point of a statement, control is transferred to the next statement. When and if control reaches the end point of the last statement, control is transferred to the end point of the statement list.
A statement in a statement list is reachable if at least one of the following is true:
The statement is the first statement and the statement list itself is reachable.
The end point of the preceding statement is reachable.
The statement is a labeled statement and the label is referenced by a reachable 
 statement.
The end point of a statement list is reachable if the end point of the last statement in the list is reachable.
The empty statement
An 
 does nothing.
An empty statement is used when there are no operations to perform in a context where a statement is required.
Execution of an empty statement simply transfers control to the end point of the statement. Thus, the end point of an empty statement is reachable if the empty statement is reachable.
An empty statement can be used when writing a 
 statement with a null body:
Also, an empty statement can be used to declare a label just before the closing ""
"" of a block:
Labeled statements
A 
 permits a statement to be prefixed by a label. Labeled statements are permitted in blocks, but are not permitted as embedded statements.
A labeled statement declares a label with the name given by the 
. The scope of a label is the whole block in which the label is declared, including any nested blocks. It is a compile-time error for two labels with the same name to have overlapping scopes.
A label can be referenced from 
 statements (
) within the scope of the label. This means that 
 statements can transfer control within blocks and out of blocks, but never into blocks.
Labels have their own declaration space and do not interfere with other identifiers. The example
is valid and uses the name 
 as both a parameter and a label.
Execution of a labeled statement corresponds exactly to execution of the statement following the label.
In addition to the reachability provided by normal flow of control, a labeled statement is reachable if the label is referenced by a reachable 
 statement. (Exception: If a 
 statement is inside a 
 that includes a 
 block, and the labeled statement is outside the 
, and the end point of the 
 block is unreachable, then the labeled statement is not reachable from that 
 statement.)
Declaration statements
A 
 declares a local variable or constant. Declaration statements are permitted in blocks, but are not permitted as embedded statements.
Local variable declarations
A 
 declares one or more local variables.
The 
 of a 
 either directly specifies the type of the variables introduced by the declaration, or indicates with the identifier 
 that the type should be inferred based on an initializer. The type is followed by a list of 
s, each of which introduces a new variable. A 
 consists of an 
 that names the variable, optionally followed by an ""
"" token and a 
 that gives the initial value of the variable.
In the context of a local variable declaration, the identifier var acts as a contextual keyword (
).When the 
 is specified as 
 and no type named 
 is in scope, the declaration is an 
, whose type is inferred from the type of the associated initializer expression. Implicitly typed local variable declarations are subject to the following restrictions:
The 
 cannot include multiple 
s.
The 
 must include a 
.
The 
 must be an 
.
The initializer 
 must have a compile-time type.
The initializer 
 cannot refer to the declared variable itself
The following are examples of incorrect implicitly typed local variable declarations:
The value of a local variable is obtained in an expression using a 
 (
), and the value of a local variable is modified using an 
 (
). A local variable must be definitely assigned (
) at each location where its value is obtained.
The scope of a local variable declared in a 
 is the block in which the declaration occurs. It is an error to refer to a local variable in a textual position that precedes the 
 of the local variable. Within the scope of a local variable, it is a compile-time error to declare another local variable or constant with the same name.
A local variable declaration that declares multiple variables is equivalent to multiple declarations of single variables with the same type. Furthermore, a variable initializer in a local variable declaration corresponds exactly to an assignment statement that is inserted immediately after the declaration.
The example
corresponds exactly to
In an implicitly typed local variable declaration, the type of the local variable being declared is taken to be the same as the type of the expression used to initialize the variable. For example:
The implicitly typed local variable declarations above are precisely equivalent to the following explicitly typed declarations:
Local constant declarations
A 
 declares one or more local constants.
The 
 of a 
 specifies the type of the constants introduced by the declaration. The type is followed by a list of 
s, each of which introduces a new constant. A 
 consists of an 
 that names the constant, followed by an ""
"" token, followed by a 
 (
) that gives the value of the constant.
The 
 and 
 of a local constant declaration must follow the same rules as those of a constant member declaration (
).
The value of a local constant is obtained in an expression using a 
 (
).
The scope of a local constant is the block in which the declaration occurs. It is an error to refer to a local constant in a textual position that precedes its 
. Within the scope of a local constant, it is a compile-time error to declare another local variable or constant with the same name.
A local constant declaration that declares multiple constants is equivalent to multiple declarations of single constants with the same type.
Expression statements
An 
 evaluates a given expression. The value computed by the expression, if any, is discarded.
Not all expressions are permitted as statements. In particular, expressions such as 
 and 
 that merely compute a value (which will be discarded), are not permitted as statements.
Execution of an 
 evaluates the contained expression and then transfers control to the end point of the 
. The end point of an 
 is reachable if that 
 is reachable.
Selection statements
Selection statements select one of a number of possible statements for execution based on the value of some expression.
The if statement
The 
 statement selects a statement for execution based on the value of a boolean expression.
An 
 part is associated with the lexically nearest preceding 
 that is allowed by the syntax. Thus, an 
 statement of the form
is equivalent to
An 
 statement is executed as follows:
The 
 (
) is evaluated.
If the boolean expression yields 
, control is transferred to the first embedded statement. When and if control reaches the end point of that statement, control is transferred to the end point of the 
 statement.
If the boolean expression yields 
 and if an 
 part is present, control is transferred to the second embedded statement. When and if control reaches the end point of that statement, control is transferred to the end point of the 
 statement.
If the boolean expression yields 
 and if an 
 part is not present, control is transferred to the end point of the 
 statement.
The first embedded statement of an 
 statement is reachable if the 
 statement is reachable and the boolean expression does not have the constant value 
.
The second embedded statement of an 
 statement, if present, is reachable if the 
 statement is reachable and the boolean expression does not have the constant value 
.
The end point of an 
 statement is reachable if the end point of at least one of its embedded statements is reachable. In addition, the end point of an 
 statement with no 
 part is reachable if the 
 statement is reachable and the boolean expression does not have the constant value 
.
The switch statement
The switch statement selects for execution a statement list having an associated switch label that corresponds to the value of the switch expression.
A 
 consists of the keyword 
, followed by a parenthesized expression (called the switch expression), followed by a 
. The 
 consists of zero or more 
s, enclosed in braces. Each 
 consists of one or more 
s followed by a 
 (
).
The 
 of a 
 statement is established by the switch expression.
If the type of the switch expression is 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or an 
, or if it is the nullable type corresponding to one of these types, then that is the governing type of the 
 statement.
Otherwise, exactly one user-defined implicit conversion (
) must exist from the type of the switch expression to one of the following possible governing types: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or,  a nullable type corresponding to one of those types.
Otherwise, if no such implicit conversion exists, or if more than one such implicit conversion exists, a compile-time error occurs.
The constant expression of each 
 label must denote a value that is implicitly convertible (
) to the governing type of the 
 statement. A compile-time error occurs if two or more 
 labels in the same 
 statement specify the same constant value.
There can be at most one 
 label in a switch statement.
A 
 statement is executed as follows:
The switch expression is evaluated and converted to the governing type.
If one of the constants specified in a 
 label in the same 
 statement is equal to the value of the switch expression, control is transferred to the statement list following the matched 
 label.
If none of the constants specified in 
 labels in the same 
 statement is equal to the value of the switch expression, and if a 
 label is present, control is transferred to the statement list following the 
 label.
If none of the constants specified in 
 labels in the same 
 statement is equal to the value of the switch expression, and if no 
 label is present, control is transferred to the end point of the 
 statement.
If the end point of the statement list of a switch section is reachable, a compile-time error occurs. This is known as the ""no fall through"" rule. The example
is valid because no switch section has a reachable end point. Unlike C and C++, execution of a switch section is not permitted to ""fall through"" to the next switch section, and the example
results in a compile-time error. When execution of a switch section is to be followed by execution of another switch section, an explicit 
 or 
 statement must be used:
Multiple labels are permitted in a 
. The example
is valid. The example does not violate the ""no fall through"" rule because the labels 
 and 
 are part of the same 
.
The ""no fall through"" rule prevents a common class of bugs that occur in C and C++ when 
 statements are accidentally omitted. In addition, because of this rule, the switch sections of a 
 statement can be arbitrarily rearranged without affecting the behavior of the statement. For example, the sections of the 
 statement above can be reversed without affecting the behavior of the statement:
The statement list of a switch section typically ends in a 
, 
, or 
 statement, but any construct that renders the end point of the statement list unreachable is permitted. For example, a 
 statement controlled by the boolean expression 
 is known to never reach its end point. Likewise, a 
 or 
 statement always transfers control elsewhere and never reaches its end point. Thus, the following example is valid:
The governing type of a 
 statement may be the type 
. For example:
Like the string equality operators (
), the 
 statement is case sensitive and will execute a given switch section only if the switch expression string exactly matches a 
 label constant.
When the governing type of a 
 statement is 
, the value 
 is permitted as a case label constant.
The 
s of a 
 may contain declaration statements (
). The scope of a local variable or constant declared in a switch block is the switch block.
The statement list of a given switch section is reachable if the 
 statement is reachable and at least one of the following is true:
The switch expression is a non-constant value.
The switch expression is a constant value that matches a 
 label in the switch section.
The switch expression is a constant value that doesn't match any 
 label, and the switch section contains the 
 label.
A switch label of the switch section is referenced by a reachable 
 or 
 statement.
The end point of a 
 statement is reachable if at least one of the following is true:
The 
 statement contains a reachable 
 statement that exits the 
 statement.
The 
 statement is reachable, the switch expression is a non-constant value, and no 
 label is present.
The 
 statement is reachable, the switch expression is a constant value that doesn't match any 
 label, and no 
 label is present.
Iteration statements
Iteration statements repeatedly execute an embedded statement.
The while statement
The 
 statement conditionally executes an embedded statement zero or more times.
A 
 statement is executed as follows:
The 
 (
) is evaluated.
If the boolean expression yields 
, control is transferred to the embedded statement. When and if control reaches the end point of the embedded statement (possibly from execution of a 
 statement), control is transferred to the beginning of the 
 statement.
If the boolean expression yields 
, control is transferred to the end point of the 
 statement.
Within the embedded statement of a 
 statement, a 
 statement (
) may be used to transfer control to the end point of the 
 statement (thus ending iteration of the embedded statement), and a 
 statement (
) may be used to transfer control to the end point of the embedded statement (thus performing another iteration of the 
 statement).
The embedded statement of a 
 statement is reachable if the 
 statement is reachable and the boolean expression does not have the constant value 
.
The end point of a 
 statement is reachable if at least one of the following is true:
The 
 statement contains a reachable 
 statement that exits the 
 statement.
The 
 statement is reachable and the boolean expression does not have the constant value 
.
The do statement
The 
 statement conditionally executes an embedded statement one or more times.
A 
 statement is executed as follows:
Control is transferred to the embedded statement.
When and if control reaches the end point of the embedded statement (possibly from execution of a 
 statement), the 
 (
) is evaluated. If the boolean expression yields 
, control is transferred to the beginning of the 
 statement. Otherwise, control is transferred to the end point of the 
 statement.
Within the embedded statement of a 
 statement, a 
 statement (
) may be used to transfer control to the end point of the 
 statement (thus ending iteration of the embedded statement), and a 
 statement (
) may be used to transfer control to the end point of the embedded statement.
The embedded statement of a 
 statement is reachable if the 
 statement is reachable.
The end point of a 
 statement is reachable if at least one of the following is true:
The 
 statement contains a reachable 
 statement that exits the 
 statement.
The end point of the embedded statement is reachable and the boolean expression does not have the constant value 
.
The for statement
The 
 statement evaluates a sequence of initialization expressions and then, while a condition is true, repeatedly executes an embedded statement and evaluates a sequence of iteration expressions.
The 
, if present, consists of either a 
 (
) or a list of 
s (
) separated by commas. The scope of a local variable declared by a 
 starts at the 
 for the variable and extends to the end of the embedded statement. The scope includes the 
 and the 
.
The 
, if present, must be a 
 (
).
The 
, if present, consists of a list of 
s (
) separated by commas.
A for statement is executed as follows:
If a 
 is present, the variable initializers or statement expressions are executed in the order they are written. This step is only performed once.
If a 
 is present, it is evaluated.
If the 
 is not present or if the evaluation yields 
, control is transferred to the embedded statement. When and if control reaches the end point of the embedded statement (possibly from execution of a 
 statement), the expressions of the 
, if any, are evaluated in sequence, and then another iteration is performed, starting with evaluation of the 
 in the step above.
If the 
 is present and the evaluation yields 
, control is transferred to the end point of the 
 statement.
Within the embedded statement of a 
 statement, a 
 statement (
) may be used to transfer control to the end point of the 
 statement (thus ending iteration of the embedded statement), and a 
 statement (
) may be used to transfer control to the end point of the embedded statement (thus executing the 
 and performing another iteration of the 
 statement, starting with the 
).
The embedded statement of a 
 statement is reachable if one of the following is true:
The 
 statement is reachable and no 
 is present.
The 
 statement is reachable and a 
 is present and does not have the constant value 
.
The end point of a 
 statement is reachable if at least one of the following is true:
The 
 statement contains a reachable 
 statement that exits the 
 statement.
The 
 statement is reachable and a 
 is present and does not have the constant value 
.
The foreach statement
The 
 statement enumerates the elements of a collection, executing an embedded statement for each element of the collection.
The 
 and 
 of a 
 statement declare the 
 of the statement. If the 
 identifier is given as the 
, and no type named 
 is in scope, the iteration variable is said to be an 
, and its type is taken to be the element type of the 
 statement, as specified below. The iteration variable corresponds to a read-only local variable with a scope that extends over the embedded statement. During execution of a 
 statement, the iteration variable represents the collection element for which an iteration is currently being performed. A compile-time error occurs if the embedded statement attempts to modify the iteration variable (via assignment or the 
 and 
 operators) or pass the iteration variable as a 
 or 
 parameter.
In the following, for brevity, 
, 
, 
 and 
 refer to the corresponding types in the namespaces 
 and 
.
The compile-time processing of a foreach statement first determines the 
, 
 and 
 of the expression. This determination proceeds as follows:
If the type 
 of 
 is an array type then there is an implicit reference conversion from 
 to the 
 interface (since 
 implements this interface). The 
 is the 
 interface, the 
 is the 
 interface and the 
 is the element type of the array type 
.
If the type 
 of 
 is 
 then there is an implicit conversion from 
 to the 
 interface (
). The 
 is the 
 interface and the 
 is the 
 interface. If the 
 identifier is given as the 
 then the 
 is 
, otherwise it is 
.
Otherwise, determine whether the type 
 has an appropriate 
 method:
Perform member lookup on the type 
 with identifier 
 and no type arguments. If the member lookup does not produce a match, or it produces an ambiguity, or produces a match that is not a method group, check for an enumerable interface as described below. It is recommended that a warning be issued if member lookup produces anything except a method group or no match.
Perform overload resolution using the resulting method group and an empty argument list. If overload resolution results in no applicable methods, results in an ambiguity, or results in a single best method but that method is either static or not public, check for an enumerable interface as described below. It is recommended that a warning be issued if overload resolution produces anything except an unambiguous public instance method or no applicable methods.
If the return type 
 of the 
 method is not a class, struct or interface type, an error is produced and no further steps are taken.
Member lookup is performed on 
 with the identifier 
 and no type arguments. If the member lookup produces no match, the result is an error, or the result is anything except a public instance property that permits reading, an error is produced and no further steps are taken.
Member lookup is performed on 
 with the identifier 
 and no type arguments. If the member lookup produces no match, the result is an error, or the result is anything except a method group, an error is produced and no further steps are taken.
Overload resolution is performed on the method group with an empty argument list. If overload resolution results in no applicable methods, results in an ambiguity, or results in a single best method but that method is either static or not public, or its return type is not 
, an error is produced and no further steps are taken.
The 
 is 
, the 
 is 
, and the 
 is the type of the 
 property.
Otherwise, check for an enumerable interface:
If among all the types 
 for which there is an implicit conversion from 
 to 
, there is a unique type 
 such that 
 is not 
 and for all the other 
 there is an implicit conversion from 
 to 
, then the 
 is the interface 
, the 
 is the interface 
, and the 
 is 
.
Otherwise, if there is more than one such type 
, then an error is produced and no further steps are taken.
Otherwise, if there is an implicit conversion from 
 to the 
 interface, then the 
 is this interface, the 
 is the interface 
, and the 
 is 
.
Otherwise, an error is produced and no further steps are taken.
The above steps, if successful, unambiguously produce a collection type 
, enumerator type 
 and element type 
. A foreach statement of the form
is then expanded to:
The variable 
 is not visible to or accessible to the expression 
 or the embedded statement or any other source code of the program. The variable 
 is read-only in the embedded statement. If there is not an explicit conversion (
) from 
 (the element type) to 
 (the 
 in the foreach statement), an error is produced and no further steps are taken. If 
 has the value 
, a 
 is thrown at run-time.
An implementation is permitted to implement a given foreach-statement differently, e.g. for performance reasons, as long as the behavior is consistent with the above expansion.
The placement of 
 inside the while loop is important for how it is captured by any anonymous function occurring in the 
.
For example:
If 
 was declared outside of the while loop, it would be shared among all iterations, and its value after the for loop would be the final value, 
, which is what the invocation of 
 would print. Instead, because each iteration has its own variable 
, the one captured by 
 in the first iteration will continue to hold the value 
, which is what will be printed. (Note: earlier versions of C# declared 
 outside of the while loop.)
The body of the finally block is constructed according to the following steps:
If there is an implicit conversion from 
 to the 
 interface, then
If 
 is a non-nullable value type then the finally clause is expanded to the semantic equivalent  of:
Otherwise the finally clause is expanded to the semantic equivalent of:
except that if 
 is a value type, or a type parameter instantiated to a value type, then the cast of 
 to 
 will not cause boxing to occur.
Otherwise, if 
 is a sealed type, the finally clause is expanded to an empty block:
Otherwise, the finally clause is expanded to:
The local variable 
 is not visible to or accessible to any user code. In particular, it does not conflict with any other variable whose scope includes the finally block.
The order in which 
 traverses the elements of an array, is as follows: For single-dimensional arrays elements are traversed in increasing index order, starting with index 
 and ending with index 
. For multi-dimensional arrays, elements are traversed such that the indices of the rightmost dimension are increased first, then the next left dimension, and so on to the left.
The following example prints out each value in a two-dimensional array, in element order:
The output produced is as follows:
In the example
the type of 
 is inferred to be 
, the element type of 
.
Jump statements
Jump statements unconditionally transfer control.
The location to which a jump statement transfers control is called the 
 of the jump statement.
When a jump statement occurs within a block, and the target of that jump statement is outside that block, the jump statement is said to 
 the block. While a jump statement may transfer control out of a block, it can never transfer control into a block.
Execution of jump statements is complicated by the presence of intervening 
 statements. In the absence of such 
 statements, a jump statement unconditionally transfers control from the jump statement to its target. In the presence of such intervening 
 statements, execution is more complex. If the jump statement exits one or more 
 blocks with associated 
 blocks, control is initially transferred to the 
 block of the innermost 
 statement. When and if control reaches the end point of a 
 block, control is transferred to the 
 block of the next enclosing 
 statement. This process is repeated until the 
 blocks of all intervening 
 statements have been executed.
In the example
the 
 blocks associated with two 
 statements are executed before control is transferred to the target of the jump statement.
The output produced is as follows:
The break statement
The 
 statement exits the nearest enclosing 
, 
, 
, 
, or 
 statement.
The target of a 
 statement is the end point of the nearest enclosing 
, 
, 
, 
, or 
 statement. If a 
 statement is not enclosed by a 
, 
, 
, 
, or 
 statement, a compile-time error occurs.
When multiple 
, 
, 
, 
, or 
 statements are nested within each other, a 
 statement applies only to the innermost statement. To transfer control across multiple nesting levels, a 
 statement (
) must be used.
A 
 statement cannot exit a 
 block (
). When a 
 statement occurs within a 
 block, the target of the 
 statement must be within the same 
 block; otherwise, a compile-time error occurs.
A 
 statement is executed as follows:
If the 
 statement exits one or more 
 blocks with associated 
 blocks, control is initially transferred to the 
 block of the innermost 
 statement. When and if control reaches the end point of a 
 block, control is transferred to the 
 block of the next enclosing 
 statement. This process is repeated until the 
 blocks of all intervening 
 statements have been executed.
Control is transferred to the target of the 
 statement.
Because a 
 statement unconditionally transfers control elsewhere, the end point of a 
 statement is never reachable.
The continue statement
The 
 statement starts a new iteration of the nearest enclosing 
, 
, 
, or 
 statement.
The target of a 
 statement is the end point of the embedded statement of the nearest enclosing 
, 
, 
, or 
 statement. If a 
 statement is not enclosed by a 
, 
, 
, or 
 statement, a compile-time error occurs.
When multiple 
, 
, 
, or 
 statements are nested within each other, a 
 statement applies only to the innermost statement. To transfer control across multiple nesting levels, a 
 statement (
) must be used.
A 
 statement cannot exit a 
 block (
). When a 
 statement occurs within a 
 block, the target of the 
 statement must be within the same 
 block; otherwise a compile-time error occurs.
A 
 statement is executed as follows:
If the 
 statement exits one or more 
 blocks with associated 
 blocks, control is initially transferred to the 
 block of the innermost 
 statement. When and if control reaches the end point of a 
 block, control is transferred to the 
 block of the next enclosing 
 statement. This process is repeated until the 
 blocks of all intervening 
 statements have been executed.
Control is transferred to the target of the 
 statement.
Because a 
 statement unconditionally transfers control elsewhere, the end point of a 
 statement is never reachable.
The goto statement
The 
 statement transfers control to a statement that is marked by a label.
The target of a 
 
 statement is the labeled statement with the given label. If a label with the given name does not exist in the current function member, or if the 
 statement is not within the scope of the label, a compile-time error occurs. This rule permits the use of a 
 statement to transfer control out of a nested scope, but not into a nested scope. In the example
a 
 statement is used to transfer control out of a nested scope.
The target of a 
 statement is the statement list in the immediately enclosing 
 statement (
), which contains a 
 label with the given constant value. If the 
 statement is not enclosed by a 
 statement, if the 
 is not implicitly convertible (
) to the governing type of the nearest enclosing 
 statement, or if the nearest enclosing 
 statement does not contain a 
 label with the given constant value, a compile-time error occurs.
The target of a 
 statement is the statement list in the immediately enclosing 
 statement (
), which contains a 
 label. If the 
 statement is not enclosed by a 
 statement, or if the nearest enclosing 
 statement does not contain a 
 label, a compile-time error occurs.
A 
 statement cannot exit a 
 block (
). When a 
 statement occurs within a 
 block, the target of the 
 statement must be within the same 
 block, or otherwise a compile-time error occurs.
A 
 statement is executed as follows:
If the 
 statement exits one or more 
 blocks with associated 
 blocks, control is initially transferred to the 
 block of the innermost 
 statement. When and if control reaches the end point of a 
 block, control is transferred to the 
 block of the next enclosing 
 statement. This process is repeated until the 
 blocks of all intervening 
 statements have been executed.
Control is transferred to the target of the 
 statement.
Because a 
 statement unconditionally transfers control elsewhere, the end point of a 
 statement is never reachable.
The return statement
The 
 statement returns control to the current caller of the function in which the 
 statement appears.
A 
 statement with no expression can be used only in a function member that does not compute a value, that is, a method with the result type (
) 
, the 
 accessor of a property or indexer, the 
 and 
 accessors of an event, an instance constructor, a static constructor, or a destructor.
A 
 statement with an expression can only be used in a function member that computes a value, that is, a method with a non-void result type, the 
 accessor of a property or indexer, or a user-defined operator. An implicit conversion (
) must exist from the type of the expression to the return type of the containing function member.
Return statements can also be used in the body of anonymous function expressions (
), and participate in determining which conversions exist for those functions.
It is a compile-time error for a 
 statement to appear in a 
 block (
).
A 
 statement is executed as follows:
If the 
 statement specifies an expression, the expression is evaluated and the resulting value is converted to the return type of the containing function by an implicit conversion. The result of the conversion becomes the result value produced by the function.
If the 
 statement is enclosed by one or more 
 or 
 blocks with associated 
 blocks, control is initially transferred to the 
 block of the innermost 
 statement. When and if control reaches the end point of a 
 block, control is transferred to the 
 block of the next enclosing 
 statement. This process is repeated until the 
 blocks of all enclosing 
 statements have been executed.
If the containing function is not an async function, control is returned to the caller of the containing function along with the result value, if any.
If the containing function is an async function, control is returned to the current caller, and the result value, if any, is recorded in the return task as described in (
).
Because a 
 statement unconditionally transfers control elsewhere, the end point of a 
 statement is never reachable.
The throw statement
The 
 statement throws an exception.
A 
 statement with an expression throws the value produced by evaluating the expression. The expression must denote a value of the class type 
, of a class type that derives from 
 or of a type parameter type that has 
 (or a subclass thereof) as its effective base class. If evaluation of the expression produces 
, a 
 is thrown instead.
A 
 statement with no expression can be used only in a 
 block, in which case that statement re-throws the exception that is currently being handled by that 
 block.
Because a 
 statement unconditionally transfers control elsewhere, the end point of a 
 statement is never reachable.
When an exception is thrown, control is transferred to the first 
 clause in an enclosing 
 statement that can handle the exception. The process that takes place from the point of the exception being thrown to the point of transferring control to a suitable exception handler is known as 
. Propagation of an exception consists of repeatedly evaluating the following steps until a 
 clause that matches the exception is found. In this description, the 
 is initially the location at which the exception is thrown.
In the current function member, each 
 statement that encloses the throw point is examined. For each statement 
, starting with the innermost 
 statement and ending with the outermost 
 statement, the following steps are evaluated:
If the 
 block of 
 encloses the throw point and if S has one or more 
 clauses, the 
 clauses are examined in order of appearance to locate a suitable handler for the exception, according to the rules specified in Section 
. If a matching 
 clause is located, the exception propagation is completed by transferring control to the block of that 
 clause.
Otherwise, if the 
 block or a 
 block of 
 encloses the throw point and if 
 has a 
 block, control is transferred to the 
 block. If the 
 block throws another exception, processing of the current exception is terminated. Otherwise, when control reaches the end point of the 
 block, processing of the current exception is continued.
If an exception handler was not located in the current function invocation, the function invocation is terminated, and one of the following occurs:
If the current function is non-async, the steps above are repeated for the caller of the function with a throw point corresponding to the statement from which the function member was invoked.
If the current function is async and task-returning, the exception is recorded in the return task, which is put into a faulted or cancelled state as described in 
.
If the current function is async and void-returning, the synchronization context of the current thread is notified as described in 
.
If the exception processing terminates all function member invocations in the current thread, indicating that the thread has no handler for the exception, then the thread is itself terminated. The impact of such termination is implementation-defined.
The try statement
The 
 statement provides a mechanism for catching exceptions that occur during execution of a block. Furthermore, the 
 statement provides the ability to specify a block of code that is always executed when control leaves the 
 statement.
There are three possible forms of 
 statements:
A 
 block followed by one or more 
 blocks.
A 
 block followed by a 
 block.
A 
 block followed by one or more 
 blocks followed by a 
 block.
When a 
 clause specifies an 
, the type must be 
, a type that derives from 
 or a type parameter type that has 
 (or a subclass thereof) as its effective base class.
When a 
 clause specifies both an 
 with an 
, an 
 of the given name and type is declared. The exception variable corresponds to a local variable with a scope that extends over the 
 clause. During execution of the 
 and 
, the exception variable represents the exception currently being handled. For purposes of definite assignment checking, the exception variable is considered definitely assigned in its entire scope.
Unless a 
 clause includes an exception variable name, it is impossible to access the exception object in the filter and 
 block.
A 
 clause that does not specify an 
 is called a general 
 clause.
Some programming languages may support exceptions that are not representable as an object derived from 
, although such exceptions could never be generated by C# code. A general 
 clause may be used to catch such exceptions. Thus, a general 
 clause is semantically different from one that specifies the type 
, in that the former may also catch exceptions from other languages.
In order to locate a handler for an exception, 
 clauses are examined in lexical order. If a 
 clause specifies a type but no exception filter, it is a compile-time error for a later 
 clause in the same 
 statement to specify a type that is the same as, or is derived from, that type. If a 
 clause specifies no type and no filter, it must be the last 
 clause for that 
 statement.
Within a 
 block, a 
 statement (
) with no expression can be used to re-throw the exception that was caught by the 
 block. Assignments to an exception variable do not alter the exception that is re-thrown.
In the example
the method 
 catches an exception, writes some diagnostic information to the console, alters the exception variable, and re-throws the exception. The exception that is re-thrown is the original exception, so the output produced is:
If the first catch block had thrown 
 instead of rethrowing the current exception, the output produced is would be as follows:
It is a compile-time error for a 
, 
, or 
 statement to transfer control out of a 
 block. When a 
, 
, or 
 statement occurs in a 
 block, the target of the statement must be within the same 
 block, or otherwise a compile-time error occurs.
It is a compile-time error for a 
 statement to occur in a 
 block.
A 
 statement is executed as follows:
Control is transferred to the 
 block.
When and if control reaches the end point of the 
 block:
If the 
 statement has a 
 block, the 
 block is executed.
Control is transferred to the end point of the 
 statement.
If an exception is propagated to the 
 statement during execution of the 
 block:
The 
 clauses, if any, are examined in order of appearance to locate a suitable handler for the exception. If a 
 clause does not specify a type, or specifies the exception type or a base type of the exception type:
If the 
 clause declares an exception variable, the exception object is assigned to the exception variable.
If the 
 clause declares an exception filter, the filter is evaluated. If it evaluates to 
, the catch clause is not a match, and the search continues through any subsequent 
 clauses for a suitable handler.
Otherwise, the 
 clause is considered a match, and control is transferred to the matching 
 block.
When and if control reaches the end point of the 
 block:
If the 
 statement has a 
 block, the 
 block is executed.
Control is transferred to the end point of the 
 statement.
If an exception is propagated to the 
 statement during execution of the 
 block:
If the 
 statement has a 
 block, the 
 block is executed.
The exception is propagated to the next enclosing 
 statement.
If the 
 statement has no 
 clauses or if no 
 clause matches the exception:
If the 
 statement has a 
 block, the 
 block is executed.
The exception is propagated to the next enclosing 
 statement.
The statements of a 
 block are always executed when control leaves a 
 statement. This is true whether the control transfer occurs as a result of normal execution, as a result of executing a 
, 
, 
, or 
 statement, or as a result of propagating an exception out of the 
 statement.
If an exception is thrown during execution of a 
 block, and is not caught within the same finally block, the exception is propagated to the next enclosing 
 statement. If another exception was in the process of being propagated, that exception is lost. The process of propagating an exception is discussed further in the description of the 
 statement (
).
The 
 block of a 
 statement is reachable if the 
 statement is reachable.
A 
 block of a 
 statement is reachable if the 
 statement is reachable.
The 
 block of a 
 statement is reachable if the 
 statement is reachable.
The end point of a 
 statement is reachable if both of the following are true:
The end point of the 
 block is reachable or the end point of at least one 
 block is reachable.
If a 
 block is present, the end point of the 
 block is reachable.
The checked and unchecked statements
The 
 and 
 statements are used to control the 
 for integral-type arithmetic operations and conversions.
The 
 statement causes all expressions in the 
 to be evaluated in a checked context, and the 
 statement causes all expressions in the 
 to be evaluated in an unchecked context.
The 
 and 
 statements are precisely equivalent to the 
 and 
 operators (
), except that they operate on blocks instead of expressions.
The lock statement
The 
 statement obtains the mutual-exclusion lock for a given object, executes a statement, and then releases the lock.
The expression of a 
 statement must denote a value of a type known to be a 
. No implicit boxing conversion (
) is ever performed for the expression of a 
 statement, and thus it is a compile-time error for the expression to denote a value of a 
.
A 
 statement of the form
where 
 is an expression of a 
, is precisely equivalent to
except that 
 is only evaluated once.
While a mutual-exclusion lock is held, code executing in the same execution thread can also obtain and release the lock. However, code executing in other threads is blocked from obtaining the lock until the lock is released.
Locking 
 objects in order to synchronize access to static data is not recommended. Other code might lock on the same type, which can result in deadlock. A better approach is to synchronize access to static data by locking a private static object. For example:
The using statement
The 
 statement obtains one or more resources, executes a statement, and then disposes of the resource.
A 
 is a class or struct that implements 
, which includes a single parameterless method named 
. Code that is using a resource can call 
 to indicate that the resource is no longer needed. If 
 is not called, then automatic disposal eventually occurs as a consequence of garbage collection.
If the form of 
 is 
 then the type of the 
 must be either 
 or a type that can be implicitly converted to 
. If the form of 
 is 
 then this expression must be implicitly convertible to 
.
Local variables declared in a 
 are read-only, and must include an initializer. A compile-time error occurs if the embedded statement attempts to modify these local variables (via assignment or the 
 and 
 operators) , take the address of them, or pass them as 
 or 
 parameters.
A 
 statement is translated into three parts: acquisition, usage, and disposal. Usage of the resource is implicitly enclosed in a 
 statement that includes a 
 clause. This 
 clause disposes of the resource. If a 
 resource is acquired, then no call to 
 is made, and no exception is thrown. If the resource is of type 
 it is dynamically converted through an implicit dynamic conversion (
) to 
 during acquisition in order to ensure that the conversion is successful before the usage and disposal.
A 
 statement of the form
corresponds to one of three possible expansions. When 
 is a non-nullable value type, the expansion is
Otherwise, when 
 is a nullable value type or a reference type other than 
, the expansion is
Otherwise, when 
 is 
, the expansion is
In either expansion, the 
 variable is read-only in the embedded statement, and the 
 variable is inaccessible in, and invisible to, the embedded statement.
An implementation is permitted to implement a given using-statement differently, e.g. for performance reasons, as long as the behavior is consistent with the above expansion.
A 
 statement of the form
has the same three possible expansions. In this case 
 is implicitly the compile-time type of the 
, if it has one. Otherwise the interface 
 itself is used as the 
. The 
 variable is inaccessible in, and invisible to, the embedded statement.
When a 
 takes the form of a 
, it is possible to acquire multiple resources of a given type. A 
 statement of the form
is precisely equivalent to a sequence of nested 
 statements:
The example below creates a file named 
 and writes two lines of text to the file. The example then opens that same file for reading and copies the contained lines of text to the console.
Since the 
 and 
 classes implement the 
 interface, the example can use 
 statements to ensure that the underlying file is properly closed following the write or read operations.
The yield statement
The 
 statement is used in an iterator block (
) to yield a value to the enumerator object (
) or enumerable object (
) of an iterator or to signal the end of the iteration.
 is not a reserved word; it has special meaning only when used immediately before a 
 or 
 keyword. In other contexts, 
 can be used as an identifier.
There are several restrictions on where a 
 statement can appear, as described in the following.
It is a compile-time error for a 
 statement (of either form) to appear outside a 
, 
 or 
It is a compile-time error for a 
 statement (of either form) to appear inside an anonymous function.
It is a compile-time error for a 
 statement (of either form) to appear in the 
 clause of a 
 statement.
It is a compile-time error for a 
 statement to appear anywhere in a 
 statement that contains any 
 clauses.
The following example shows some valid and invalid uses of 
 statements.
An implicit conversion (
) must exist from the type of the expression in the 
 statement to the yield type (
) of the iterator.
A 
 statement is executed as follows:
The expression given in the statement is evaluated, implicitly converted to the yield type, and assigned to the 
 property of the enumerator object.
Execution of the iterator block is suspended. If the 
 statement is within one or more 
 blocks, the associated 
 blocks are not executed at this time.
The 
 method of the enumerator object returns 
 to its caller, indicating that the enumerator object successfully advanced to the next item.
The next call to the enumerator object's 
 method resumes execution of the iterator block from where it was last suspended.
A 
 statement is executed as follows:
If the 
 statement is enclosed by one or more 
 blocks with associated 
 blocks, control is initially transferred to the 
 block of the innermost 
 statement. When and if control reaches the end point of a 
 block, control is transferred to the 
 block of the next enclosing 
 statement. This process is repeated until the 
 blocks of all enclosing 
 statements have been executed.
Control is returned to the caller of the iterator block. This is either the 
 method or 
 method of the enumerator object.
Because a 
 statement unconditionally transfers control elsewhere, the end point of a 
 statement is never reachable.
Namespaces
C# programs are organized using namespaces. Namespaces are used both as an ""internal"" organization system for a program, and as an ""external"" organization system—a way of presenting program elements that are exposed to other programs.
Using directives (
) are provided to facilitate the use of namespaces.
Compilation units
A 
 defines the overall structure of a source file. A compilation unit consists of zero or more 
s followed by zero or more 
 followed by zero or more 
s.
A C# program consists of one or more compilation units, each contained in a separate source file. When a C# program is compiled, all of the compilation units are processed together. Thus, compilation units can depend on each other, possibly in a circular fashion.
The 
s of a compilation unit affect the 
 and 
s of that compilation unit, but have no effect on other compilation units.
The 
 (
) of a compilation unit permit the specification of attributes for the target assembly and module. Assemblies and modules act as physical containers for types. An assembly may consist of several physically separate modules.
The 
s of each compilation unit of a program contribute members to a single declaration space called the global namespace. For example:
File 
:
File 
:
The two compilation units contribute to the single global namespace, in this case declaring two classes with the fully qualified names 
 and 
. Because the two compilation units contribute to the same declaration space, it would have been an error if each contained a declaration of a member with the same name.
Namespace declarations
A 
 consists of the keyword 
, followed by a namespace name and body, optionally followed by a semicolon.
A 
 may occur as a top-level declaration in a 
 or as a member declaration within another 
. When a 
 occurs as a top-level declaration in a 
, the namespace becomes a member of the global namespace. When a 
 occurs within another 
, the inner namespace becomes a member of the outer namespace. In either case, the name of a namespace must be unique within the containing namespace.
Namespaces are implicitly 
 and the declaration of a namespace cannot include any access modifiers.
Within a 
, the optional 
s import the names of other namespaces, types and members, allowing them to be referenced directly instead of through qualified names. The optional 
s contribute members to the declaration space of the namespace. Note that all 
s must appear before any member declarations.
The 
 of a 
 may be a single identifier or a sequence of identifiers separated by ""
"" tokens. The latter form permits a program to define a nested namespace without lexically nesting several namespace declarations. For example,
is semantically equivalent to
Namespaces are open-ended, and two namespace declarations with the same fully qualified name contribute to the same declaration space (
). In the example
the two namespace declarations above contribute to the same declaration space, in this case declaring two classes with the fully qualified names 
 and 
. Because the two declarations contribute to the same declaration space, it would have been an error if each contained a declaration of a member with the same name.
Extern aliases
An 
 introduces an identifier that serves as an alias for a namespace. The specification of the aliased namespace is external to the source code of the program and applies also to nested namespaces of the aliased namespace.
The scope of an 
 extends over the 
s, 
 and 
s of its immediately containing compilation unit or namespace body.
Within a compilation unit or namespace body that contains an 
, the identifier introduced by the 
 can be used to reference the aliased namespace. It is a compile-time error for the 
 to be the word 
.
An 
 makes an alias available within a particular compilation unit or namespace body, but it does not contribute any new members to the underlying declaration space. In other words, an 
 is not transitive, but, rather, affects only the compilation unit or namespace body in which it occurs.
The following program declares and uses two extern aliases, 
 and 
, each of which represent the root of a distinct namespace hierarchy:
The program declares the existence of the extern aliases 
 and 
, but the actual definitions of the aliases are external to the program. The identically named 
 classes can now be referenced as 
 and 
, or, using the namespace alias qualifier, 
 and 
. An error occurs if a program declares an extern alias for which no external definition is provided.
Using directives
 facilitate the use of namespaces and types defined in other namespaces. Using directives impact the name resolution process of 
s (
) and 
s (
), but unlike declarations, using directives do not contribute new members to the underlying declaration spaces of the compilation units or namespaces within which they are used.
A 
 (
) introduces an alias for a namespace or type.
A 
 (
) imports the type members of a namespace.
A 
 (
) imports the nested types and static members of a type.
The scope of a 
 extends over the 
s of its immediately containing compilation unit or namespace body. The scope of a 
 specifically does not include its peer 
s. Thus, peer 
s do not affect each other, and the order in which they are written is insignificant.
Using alias directives
A 
 introduces an identifier that serves as an alias for a namespace or type within the immediately enclosing compilation unit or namespace body.
Within member declarations in a compilation unit or namespace body that contains a 
, the identifier introduced by the 
 can be used to reference the given namespace or type. For example:
Above, within member declarations in the 
 namespace, 
 is an alias for 
, and thus class 
 derives from class 
. The same effect can be obtained by creating an alias 
 for 
 and then referencing 
:
The 
 of a 
 must be unique within the declaration space of the compilation unit or namespace that immediately contains the 
. For example:
Above, 
 already contains a member 
, so it is a compile-time error for a 
 to use that identifier. Likewise, it is a compile-time error for two or more 
s in the same compilation unit or namespace body to declare aliases by the same name.
A 
 makes an alias available within a particular compilation unit or namespace body, but it does not contribute any new members to the underlying declaration space. In other words, a 
 is not transitive but rather affects only the compilation unit or namespace body in which it occurs. In the example
the scope of the 
 that introduces 
 only extends to member declarations in the namespace body in which it is contained, so 
 is unknown in the second namespace declaration. However, placing the 
 in the containing compilation unit causes the alias to become available within both namespace declarations:
Just like regular members, names introduced by 
s are hidden by similarly named members in nested scopes. In the example
the reference to 
 in the declaration of 
 causes a compile-time error because 
 refers to 
, not 
.
The order in which 
s are written has no significance, and resolution of the 
 referenced by a 
 is not affected by the 
 itself or by other 
s in the immediately containing compilation unit or namespace body. In other words, the 
 of a 
 is resolved as if the immediately containing compilation unit or namespace body had no 
s. A 
 may however be affected by 
s in the immediately containing compilation unit or namespace body. In the example
the last 
 results in a compile-time error because it is not affected by the first 
. The first 
 does not result in an error since the scope of the extern alias 
 includes the 
.
A 
 can create an alias for any namespace or type, including the namespace within which it appears and any namespace or type nested within that namespace.
Accessing a namespace or type through an alias yields exactly the same result as accessing that namespace or type through its declared name. For example, given
the names 
, 
, and 
 are equivalent and all refer to the class whose fully qualified name is 
.
Using aliases can name a closed constructed type, but cannot name an unbound generic type declaration without supplying type arguments. For example:
Using namespace directives
A 
 imports the types contained in a namespace into the immediately enclosing compilation unit or namespace body, enabling the identifier of each type to be used without qualification.
Within member declarations in a compilation unit or namespace body that contains a 
, the types contained in the given namespace can be referenced directly. For example:
Above, within member declarations in the 
 namespace, the type members of 
 are directly available, and thus class 
 derives from class 
.
A 
 imports the types contained in the given namespace, but specifically does not import nested namespaces. In the example
the 
 imports the types contained in 
, but not the namespaces nested in 
. Thus, the reference to 
 in the declaration of 
 results in a compile-time error because no members named 
 are in scope.
Unlike a 
, a 
 may import types whose identifiers are already defined within the enclosing compilation unit or namespace body. In effect, names imported by a 
 are hidden by similarly named members in the enclosing compilation unit or namespace body. For example:
Here, within member declarations in the 
 namespace, 
 refers to 
 rather than 
.
When more than one namespace or type imported by 
s or 
s in the same compilation unit or namespace body contain types by the same name, references to that name as a 
 are considered ambiguous. In the example
both 
 and 
 contain a member 
, and because 
 imports both, referencing 
 in 
 is a compile-time error. In this situation, the conflict can be resolved either through qualification of references to 
, or by introducing a 
 that picks a particular 
. For example:
Furthermore, when more than one namespace or type imported by 
s or 
s in the same compilation unit or namespace body contain types or members by the same name, references to that name as a 
 are considered ambiguous. In the example
 contains a type member 
, and 
 contains a static method 
, and because 
 imports both, referencing 
 as a 
 is ambiguous and a compile-time error.
Like a 
, a 
 does not contribute any new members to the underlying declaration space of the compilation unit or namespace, but rather affects only the compilation unit or namespace body in which it appears.
The 
 referenced by a 
 is resolved in the same way as the 
 referenced by a 
. Thus, 
s in the same compilation unit or namespace body do not affect each other and can be written in any order.
Using static directives
A 
 imports the nested types and static members contained directly in a type declaration into the immediately enclosing compilation unit or namespace body, enabling the identifier of each member and type to be used without qualification.
Within member declarations in a compilation unit or namespace body that contains a 
, the accessible nested types and static members (except extension methods) contained directly in the declaration of the given type can be referenced directly. For example:
Above, within member declarations in the 
 namespace, the static members and nested types of 
 are directly available, and thus the method 
 is able to reference both the 
 and 
 members of 
.
A *using
static
directive` specifically does not import extension methods directly as static methods, but makes them available for extension method invocation (
). In the example
the 
 imports the extension method 
 contained in 
, but only as an extension method. Thus, the first reference to 
 in the body of 
 results in a compile-time error because no members named 
 are in scope.
A 
 only imports members and types declared directly in the given type, not members and types declared in base classes.
TODO: Example
Ambiguities between multiple 
using_namespace_directives
 and 
using_static_directives
 are discussed in 
.
Namespace members
A 
 is either a 
 (
) or a 
 (
).
A compilation unit or a namespace body can contain 
s, and such declarations contribute new members to the underlying declaration space of the containing compilation unit or namespace body.
Type declarations
A 
 is a 
 (
), a 
 (
), an 
 (
), an 
 (
), or a 
 (
).
A 
 can occur as a top-level declaration in a compilation unit or as a member declaration within a namespace, class, or struct.
When a type declaration for a type 
 occurs as a top-level declaration in a compilation unit, the fully qualified name of the newly declared type is simply 
. When a type declaration for a type 
 occurs within a namespace, class, or struct, the fully qualified name of the newly declared type is 
, where 
 is the fully qualified name of the containing namespace, class, or struct.
A type declared within a class or struct is called a nested type (
).
The permitted access modifiers and the default access for a type declaration depend on the context in which the declaration takes place (
):
Types declared in compilation units or namespaces can have 
 or 
 access. The default is 
 access.
Types declared in classes can have 
, 
, 
, 
, or 
 access. The default is 
 access.
Types declared in structs can have 
, 
, or 
 access. The default is 
 access.
Namespace alias qualifiers
The 
 
 makes it possible to guarantee that type name lookups are unaffected by the introduction of new types and members. The namespace alias qualifier always appears between two identifiers referred to as the left-hand and right-hand identifiers. Unlike the regular 
 qualifier, the left-hand identifier of the 
 qualifier is looked up only as an extern or using alias.
A 
 is defined as follows:
A 
 can be used as a 
 (
) or as the left operand in a 
 (
).
A 
 has one of two forms:
, where 
 and 
 represent identifiers, and 
 is a type argument list. (
 is always at least one.)
, where 
 and 
 represent identifiers. (In this case, 
 is considered to be zero.)
Using this notation, the meaning of a 
 is determined as follows:
If 
 is the identifier 
, then the global namespace is searched for 
:
If the global namespace contains a namespace named 
 and 
 is zero, then the 
 refers to that namespace.
Otherwise, if the global namespace contains a non-generic type named 
 and 
 is zero, then the 
 refers to that type.
Otherwise, if the global namespace contains a type named 
 that has 
 type parameters, then the 
 refers to that type constructed with the given type arguments.
Otherwise, the 
 is undefined and a compile-time error occurs.
Otherwise, starting with the namespace declaration (
) immediately containing the 
 (if any), continuing with each enclosing namespace declaration (if any), and ending with the compilation unit containing the 
, the following steps are evaluated until an entity is located:
If the namespace declaration or compilation unit contains a 
 that associates 
 with a type, then the 
 is undefined and a compile-time error occurs.
Otherwise, if the namespace declaration or compilation unit contains an 
 or 
 that associates 
 with a namespace, then:
If the namespace associated with 
 contains a namespace named 
 and 
 is zero, then the 
 refers to that namespace.
Otherwise, if the namespace associated with 
 contains a non-generic type named 
 and 
 is zero, then the 
 refers to that type.
Otherwise, if the namespace associated with 
 contains a type named 
 that has 
 type parameters, then the 
 refers to that type constructed with the given type arguments.
Otherwise, the 
 is undefined and a compile-time error occurs.
Otherwise, the 
 is undefined and a compile-time error occurs.
Note that using the namespace alias qualifier with an alias that references a type causes a compile-time error. Also note that if the identifier 
 is 
, then lookup is performed in the global namespace, even if there is a using alias associating 
 with a type or namespace.
Uniqueness of aliases
Each compilation unit and namespace body has a separate declaration space for extern aliases and using aliases. Thus, while the name of an extern alias or using alias must be unique within the set of extern aliases and using aliases declared in the immediately containing compilation unit or namespace body, an alias is permitted to have the same name as a type or namespace as long as it is used only with the 
 qualifier.
In the example
the name 
 has two possible meanings in the second namespace body because both the class 
 and the using alias 
 are in scope. For this reason, use of 
 in the qualified name 
 is ambiguous and causes a compile-time error to occur. However, use of 
 with the 
 qualifier is not an error because 
 is looked up only as a namespace alias.
Classes
A class is a data structure that may contain data members (constants and fields), function members (methods, properties, events, indexers, operators, instance constructors, destructors and static constructors), and nested types. Class types support inheritance, a mechanism whereby a derived class can extend and specialize a base class.
Class declarations
A 
 is a 
 (
) that declares a new class.
A 
 consists of an optional set of 
 (
), followed by an optional set of 
s (
), followed by an optional 
 modifier, followed by the keyword 
 and an 
 that names the class, followed by an optional 
 (
), followed by an optional 
 specification (
) , followed by an optional set of 
s (
), followed by a 
 (
), optionally followed by a semicolon.
A class declaration cannot supply 
s unless it also supplies a 
.
A class declaration that supplies a 
 is a 
. Additionally, any class nested inside a generic class declaration or a generic struct declaration is itself a generic class declaration, since type parameters for the containing type must be supplied to create a constructed type.
Class modifiers
A 
 may optionally include a sequence of class modifiers:
It is a compile-time error for the same modifier to appear multiple times in a class declaration.
The 
 modifier is permitted on nested classes. It specifies that the class hides an inherited member by the same name, as described in 
. It is a compile-time error for the 
 modifier to appear on a class declaration that is not a nested class declaration.
The 
, 
, 
, and 
 modifiers control the accessibility of the class. Depending on the context in which the class declaration occurs, some of these modifiers may not be permitted (
).
The 
, 
 and 
 modifiers are discussed in the following sections.
Abstract classes
The 
 modifier is used to indicate that a class is incomplete and that it is intended to be used only as a base class. An abstract class differs from a non-abstract class in the following ways:
An abstract class cannot be instantiated directly, and it is a compile-time error to use the 
 operator on an abstract class. While it is possible to have variables and values whose compile-time types are abstract, such variables and values will necessarily either be 
 or contain references to instances of non-abstract classes derived from the abstract types.
An abstract class is permitted (but not required) to contain abstract members.
An abstract class cannot be sealed.
When a non-abstract class is derived from an abstract class, the non-abstract class must include actual implementations of all inherited abstract members, thereby overriding those abstract members. In the example
the abstract class 
 introduces an abstract method 
. Class 
 introduces an additional method 
, but since it doesn't provide an implementation of 
, 
 must also be declared abstract. Class 
 overrides 
 and provides an actual implementation. Since there are no abstract members in 
, 
 is permitted (but not required) to be non-abstract.
Sealed classes
The 
 modifier is used to prevent derivation from a class. A compile-time error occurs if a sealed class is specified as the base class of another class.
A sealed class cannot also be an abstract class.
The 
 modifier is primarily used to prevent unintended derivation, but it also enables certain run-time optimizations. In particular, because a sealed class is known to never have any derived classes, it is possible to transform virtual function member invocations on sealed class instances into non-virtual invocations.
Static classes
The 
 modifier is used to mark the class being declared as a 
. A static class cannot be instantiated, cannot be used as a type and can contain only static members. Only a static class can contain declarations of extension methods (
).
A static class declaration is subject to the following restrictions:
A static class may not include a 
 or 
 modifier. Note, however, that since a static class cannot be instantiated or derived from, it behaves as if it was both sealed and abstract.
A static class may not include a 
 specification (
) and cannot explicitly specify a base class or a list of implemented interfaces. A static class implicitly inherits from type 
.
A static class can only contain static members (
). Note that constants and nested types are classified as static members.
A static class cannot have members with 
 or 
 declared accessibility.
It is a compile-time error to violate any of these restrictions.
A static class has no instance constructors. It is not possible to declare an instance constructor in a static class, and no default instance constructor (
) is provided for a static class.
The members of a static class are not automatically static, and the member declarations must explicitly include a 
 modifier (except for constants and nested types). When a class is nested within a static outer class, the nested class is not a static class unless it explicitly includes a 
 modifier.
Referencing static class types
A 
 (
) is permitted to reference a static class if
The 
 is the 
 in a 
 of the form 
, or
The 
 is the 
 in a 
 (
1) of the form 
.
A 
 (
) is permitted to reference a static class if
The 
 is the 
 in a 
 (
) of the form 
.
In any other context it is a compile-time error to reference a static class. For example, it is an error for a static class to be used as a base class, a constituent type (
) of a member, a generic type argument, or a type parameter constraint. Likewise, a static class cannot be used in an array type, a pointer type, a 
 expression, a cast expression, an 
 expression, an 
 expression, a 
 expression, or a default value expression.
Partial modifier
The 
 modifier is used to indicate that this 
 is a partial type declaration. Multiple partial type declarations with the same name within an enclosing namespace or type declaration combine to form one type declaration, following the rules specified in 
.
Having the declaration of a class distributed over separate segments of program text can be useful if these segments are produced or maintained in different contexts. For instance, one part of a class declaration may be machine generated, whereas the other is manually authored. Textual separation of the two prevents updates by one from conflicting with updates by the other.
Type parameters
A type parameter is a simple identifier that denotes a placeholder for a type argument supplied to create a constructed type. A type parameter is a formal placeholder for a type that will be supplied later. By constrast, a type argument (
) is the actual type that is substituted for the type parameter when a constructed type is created.
Each type parameter in a class declaration defines a name in the declaration space (
) of that class. Thus, it cannot have the same name as another type parameter or a member declared in that class. A type parameter cannot have the same name as the type itself.
Class base specification
A class declaration may include a 
 specification, which defines the direct base class of the class and the interfaces (
) directly implemented by the class.
The base class specified in a class declaration can be a constructed class type (
). A base class cannot be a type parameter on its own, though it can involve the type parameters that are in scope.
Base classes
When a 
 is included in the 
, it specifies the direct base class of the class being declared. If a class declaration has no 
, or if the 
 lists only interface types, the direct base class is assumed to be 
. A class inherits members from its direct base class, as described in 
.
In the example
class 
 is said to be the direct base class of 
, and 
 is said to be derived from 
. Since 
 does not explicitly specify a direct base class, its direct base class is implicitly 
.
For a constructed class type, if a base class is specified in the generic class declaration, the base class of the constructed type is obtained by substituting, for each 
 in the base class declaration, the corresponding 
 of the constructed type. Given the generic class declarations
the base class of the constructed type 
 would be 
.
The direct base class of a class type must be at least as accessible as the class type itself (
). For example, it is a compile-time error for a 
 class to derive from a 
 or 
 class.
The direct base class of a class type must not be any of the following types: 
, 
, 
, 
, or 
. Furthermore, a generic class declaration cannot use 
 as a direct or indirect base class.
While determining the meaning of the direct base class specification 
 of a class 
, the direct base class of 
 is temporarily assumed to be 
. Intuitively this ensures that the meaning of a base class specification cannot recursively depend on itself. The example:
is in error since in the base class specification 
 the direct base class of 
 is considered to be 
, and hence (by the rules of 
)  
 is not considered to have a member 
.
The base classes of a class type are the direct base class and its base classes. In other words, the set of base classes is the transitive closure of the direct base class relationship. Referring to the example above, the base classes of 
 are 
 and 
. In the example
the base classes of 
 are 
, 
, 
, and 
.
Except for class 
, every class type has exactly one direct base class. The 
 class has no direct base class and is the ultimate base class of all other classes.
When a class 
 derives from a class 
, it is a compile-time error for 
 to depend on 
. A class 
 its direct base class (if any) and 
 the class within which it is immediately nested (if any). Given this definition, the complete set of classes upon which a class depends is the reflexive and transitive closure of the 
 relationship.
The example
is erroneous because the class depends on itself. Likewise, the example
is in error because the classes circularly depend on themselves. Finally, the example
results in a compile-time error because 
 depends on 
 (its direct base class), which depends on 
 (its immediately enclosing class), which circularly depends on 
.
Note that a class does not depend on the classes that are nested within it. In the example
 depends on 
 (because 
 is both its direct base class and its immediately enclosing class), but 
 does not depend on 
 (since 
 is neither a base class nor an enclosing class of 
). Thus, the example is valid.
It is not possible to derive from a 
 class. In the example
class 
 is in error because it attempts to derive from the 
 class 
.
Interface implementations
A 
 specification may include a list of interface types, in which case the class is said to directly implement the given interface types. Interface implementations are discussed further in 
.
Type parameter constraints
Generic type and method declarations can optionally specify type parameter constraints by including 
s.
Each 
 consists of the token 
, followed by the name of a type parameter, followed by a colon and the list of constraints for that type parameter. There can be at most one 
 clause for each type parameter, and the 
 clauses can be listed in any order. Like the 
 and 
 tokens in a property accessor, the 
 token is not a keyword.
The list of constraints given in a 
 clause can include any of the following components, in this order: a single primary constraint, one or more secondary constraints, and the constructor constraint, 
.
A primary constraint can be a class type or the 
 
 or the 
 
. A secondary constraint can be a 
 or 
.
The reference type constraint specifies that a type argument used for the type parameter must be a reference type. All class types, interface types, delegate types, array types, and type parameters known to be a reference type (as defined below) satisfy this constraint.
The value type constraint specifies that a type argument used for the type parameter must be a non-nullable value type. All non-nullable struct types, enum types, and type parameters having the value type constraint satisfy this constraint. Note that although classified as a value type, a nullable type (
) does not satisfy the value type constraint. A type parameter having the value type constraint cannot also have the 
.
Pointer types are never allowed to be type arguments and are not considered to satisfy either the reference type or value type constraints.
If a constraint is a class type, an interface type, or a type parameter, that type specifies a minimal ""base type"" that every type argument used for that type parameter must support. Whenever a constructed type or generic method is used, the type argument is checked against the constraints on the type parameter at compile-time. The type argument supplied must satisfy the conditions described in 
.
A 
 constraint must satisfy the following rules:
The type must be a class type.
The type must not be 
.
The type must not be one of the following types: 
, 
, 
, or 
.
The type must not be 
. Because all types derive from 
, such a constraint would have no effect if it were permitted.
At most one constraint for a given type parameter can be a class type.
A type specified as an 
 constraint must satisfy the following rules:
The type must be an interface type.
A type must not be specified more than once in a given 
 clause.
In either case, the constraint can involve any of the type parameters of the associated type or method declaration as part of a constructed type, and can involve the type being declared.
Any class or interface type specified as a type parameter constraint must be at least as accessible (
) as the generic type or method being declared.
A type specified as a 
 constraint must satisfy the following rules:
The type must be a type parameter.
A type must not be specified more than once in a given 
 clause.
In addition there must be no cycles in the dependency graph of type parameters, where dependency is a transitive relation defined by:
If a type parameter 
 is used as a constraint for type parameter 
 then 
 
 
.
If a type parameter 
 depends on a type parameter 
 and 
 depends on a type parameter 
 then 
 
 
.
Given this relation, it is a compile-time error for a type parameter to depend on itself (directly or indirectly).
Any constraints must be consistent among dependent type parameters. If type parameter 
 depends on type parameter 
 then:
 must not have the value type constraint. Otherwise, 
 is effectively sealed so 
 would be forced to be the same type as 
, eliminating the need for two type parameters.
If 
 has the value type constraint then 
 must not have a 
 constraint.
If 
 has a 
 constraint 
 and 
 has a 
 constraint 
 then there must be an identity conversion or implicit reference conversion from 
 to 
 or an implicit reference conversion from 
 to 
.
If 
 also depends on type parameter 
 and 
 has a 
 constraint 
 and 
 has a 
 constraint 
 then there must be an identity conversion or implicit reference conversion from 
 to 
 or an implicit reference conversion from 
 to 
.
It is valid for 
 to have the value type constraint and 
 to have the reference type constraint. Effectively this limits 
 to the types 
, 
, 
, and any interface type.
If the 
 clause for a type parameter includes a constructor constraint (which has the form 
), it is possible to use the 
 operator to create instances of the type (
). Any type argument used for a type parameter with a constructor constraint must have a public parameterless constructor (this constructor implicitly exists for any value type) or be a type parameter having the value type constraint or constructor constraint (see 
 for details).
The following are examples of constraints:
The following example is in error because it causes a circularity in the dependency graph of the type parameters:
The following examples illustrate additional invalid situations:
The 
 of a type parameter 
 is defined as follows:
If 
 has no primary constraints or type parameter constraints, its effective base class is 
.
If 
 has the value type constraint, its effective base class is 
.
If 
 has a 
 constraint 
 but no 
 constraints, its effective base class is 
.
If 
 has no 
 constraint but has one or more 
 constraints, its effective base class is the most encompassed type (
) in the set of effective base classes of its 
 constraints. The consistency rules ensure that such a most encompassed type exists.
If 
 has both a 
 constraint and one or more 
 constraints, its effective base class is the most encompassed type (
) in the set consisting of the 
 constraint of 
 and the effective base classes of its 
 constraints. The consistency rules ensure that such a most encompassed type exists.
If 
 has the reference type constraint but no 
 constraints, its effective base class is 
.
For the purpose of these rules, if T has a constraint 
 that is a 
, use instead the most specific base type of 
 that is a 
. This can never happen in an explicitly given constraint, but may occur when the constraints of a generic method are implicitly inherited by an overriding method declaration or an explicit implementation of an interface method.
These rules ensure that the effective base class is always a 
.
The 
 of a type parameter 
 is defined as follows:
If 
 has no 
, its effective interface set is empty.
If 
 has 
 constraints but no 
 constraints, its effective interface set is its set of 
 constraints.
If 
 has no 
 constraints but has 
 constraints, its effective interface set is the union of the effective interface sets of its 
 constraints.
If 
 has both 
 constraints and 
 constraints, its effective interface set is the union of its set of 
 constraints and the effective interface sets of its 
 constraints.
A type parameter is 
 if it has the reference type constraint or its effective base class is not 
 or 
.
Values of a constrained type parameter type can be used to access the instance members implied by the constraints. In the example
the methods of 
 can be invoked directly on 
 because 
 is constrained to always implement 
.
Class body
The 
 of a class defines the members of that class.
Partial types
A type declaration can be split across multiple 
. The type declaration is constructed from its parts by following the rules in this section, whereupon it is treated as a single declaration during the remainder of the compile-time and run-time processing of the program.
A 
, 
 or 
 represents a partial type declaration if it includes a 
 modifier. 
 is not a keyword, and only acts as a modifier if it appears immediately before one of the keywords 
, 
 or 
 in a type declaration, or before the type 
 in a method declaration. In other contexts it can be used as a normal identifier.
Each part of a partial type declaration must include a 
 modifier. It must have the same name  and be declared in the same namespace or type declaration as the other parts. The 
 modifier indicates that additional parts of the type declaration may exist elsewhere, but the existence of such additional parts is not a requirement; it is valid for a type with a single declaration to include the 
 modifier.
All parts of a partial type must be compiled together such that the parts can be merged at compile-time into a single type declaration. Partial types specifically do not allow already compiled types to be extended.
Nested types may be declared in multiple parts by using the 
 modifier. Typically, the containing type is declared using 
 as well, and each part of the nested type is declared in a different part of the containing type.
The 
 modifier is not permitted on delegate or enum declarations.
Attributes
The attributes of a partial type are determined by combining, in an unspecified order, the attributes of each of the parts. If an attribute is placed on multiple parts, it is equivalent to specifying the attribute multiple times on the type. For example, the two parts:
are equivalent to a declaration such as:
Attributes on type parameters combine in a similar fashion.
Modifiers
When a partial type declaration includes an accessibility specification (the 
, 
, 
, and 
 modifiers) it must agree with all other parts that include an accessibility specification. If no part of a partial type includes an accessibility specification, the type is given the appropriate default accessibility (
).
If one or more partial declarations of a nested type include a 
 modifier, no warning is reported if the nested type hides an inherited member (
).
If one or more partial declarations of a class include an 
 modifier, the class is considered abstract (
). Otherwise, the class is considered non-abstract.
If one or more partial declarations of a class include a 
 modifier, the class is considered sealed (
). Otherwise, the class is considered unsealed.
Note that a class cannot be both abstract and sealed.
When the 
 modifier is used on a partial type declaration, only that particular part is considered an unsafe context (
).
Type parameters and constraints
If a generic type is declared in multiple parts, each part must state the type parameters. Each part must have the same number of type parameters, and the same name for each type parameter, in order.
When a partial generic type declaration includes constraints (
 clauses), the constraints must agree with all other parts that include constraints. Specifically, each part that includes constraints must have constraints for the same set of type parameters, and for each type parameter the sets of primary, secondary, and constructor constraints must be equivalent. Two sets of constraints are equivalent if they contain the same members. If no part of a partial generic type specifies type parameter constraints, the type parameters are considered unconstrained.
The example
is correct because those parts that include constraints (the first two) effectively specify the same set of primary, secondary, and constructor constraints for the same set of type parameters, respectively.
Base class
When a partial class declaration includes a base class specification it must agree with all other parts that include a base class specification. If no part of a partial class includes a base class specification, the base class becomes 
 (
).
Base interfaces
The set of base interfaces for a type declared in multiple parts is the union of the base interfaces specified on each part. A particular base interface may only be named once on each part, but it is permitted for multiple parts to name the same base interface(s). There must only be one implementation of the members of any given base interface.
In the example
the set of base interfaces for class 
 is 
, 
, and 
.
Typically, each part provides an implementation of the interface(s) declared on that part; however, this is not a requirement. A part may provide the implementation for an interface declared on a different part:
Members
With the exception of partial methods (
), the set of members of a type declared in multiple parts is simply the union of the set of members declared in each part. The bodies of all parts of the type declaration share the same declaration space (
), and the scope of each member (
) extends to the bodies of all the parts. The accessibility domain of any member always includes all the parts of the enclosing type; a 
 member declared in one part is freely accessible from another part. It is a compile-time error to declare the same member in more than one part of the type, unless that member is a type with the 
 modifier.
The ordering of members within a type is rarely significant to C# code, but may be significant when interfacing with other languages and environments. In these cases, the ordering of members within a type declared in multiple parts is undefined.
Partial methods
Partial methods can be defined in one part of a type declaration and implemented in another. The implementation is optional; if no part implements the partial method, the partial method declaration and all calls to it are removed from the type declaration resulting from the combination of the parts.
Partial methods cannot define access modifiers, but are implicitly 
. Their return type must be 
, and their parameters cannot have the 
 modifier. The identifier 
 is recognized as a special keyword in a method declaration only if it appears right before the 
 type; otherwise it can be used as a normal identifier. A partial method cannot explicitly implement interface methods.
There are two kinds of partial method declarations: If the body of the method declaration is a semicolon, the declaration is said to be a 
. If the body is given as a 
, the declaration is said to be an 
. Across the parts of a type declaration there can be only one defining partial method declaration with a given signature, and there can be only one implementing partial method declaration with a given signature. If an implementing partial method declaration is given, a corresponding defining partial method declaration must exist, and the declarations must match as specified in the following:
The declarations must have the same modifiers (although not necessarily in the same order), method name, number of type parameters and number of parameters.
Corresponding parameters in the declarations must have the same modifiers (although not necessarily in the same order) and the same types (modulo differences in type parameter names).
Corresponding type parameters in the declarations must have the same constraints (modulo differences in type parameter names).
An implementing partial method declaration can appear in the same part as the corresponding defining partial method declaration.
Only a defining partial method participates in overload resolution. Thus, whether or not an implementing declaration is given, invocation expressions may resolve to invocations of the partial method. Because a partial method always returns 
, such invocation expressions will always be expression statements. Furthermore, because a partial method is implicitly 
, such statements will always occur within one of the parts of the type declaration within which the partial method is declared.
If no part of a partial type declaration contains an implementing declaration for a given partial method, any expression statement invoking it is simply removed from the combined type declaration. Thus the invocation expression, including any constituent expressions, has no effect at run-time. The partial method itself is also removed and will not be a member of the combined type declaration.
If an implementing declaration exist for a given partial method, the invocations of the partial methods are retained. The partial method gives rise to a method declaration similar to the implementing partial method declaration except for the following:
The 
 modifier is not included
The attributes in the resulting method declaration are the combined attributes of the defining and the implementing partial method declaration in unspecified order. Duplicates are not removed.
The attributes on the parameters of the resulting method declaration are the combined attributes of the corresponding parameters of the defining and the implementing partial method declaration in unspecified order. Duplicates are not removed.
If a defining declaration but not an implementing declaration is given for a partial method M, the following restrictions apply:
It is a compile-time error to create a delegate to method (
).
It is a compile-time error to refer to 
 inside an anonymous function that is converted to an expression tree type (
).
Expressions occurring as part of an invocation of 
 do not affect the definite assignment state (
), which can potentially lead to compile-time errors.
 cannot be the entry point for an application (
).
Partial methods are useful for allowing one part of a type declaration to customize the behavior of another part, e.g., one that is generated by a tool. Consider the following partial class declaration:
If this class is compiled without any other parts, the defining partial method declarations and their invocations will be removed, and the resulting combined class declaration will be equivalent to the following:
Assume that another part is given, however, which provides implementing declarations of the partial methods:
Then the resulting combined class declaration will be equivalent to the following:
Name binding
Although each part of an extensible type must be declared within the same namespace, the parts are typically written within different namespace declarations. Thus, different 
 directives (
) may be present for each part. When interpreting simple names (
) within one part, only the 
 directives of the namespace declaration(s) enclosing that part are considered. This may result in the same identifier having different meanings in different parts:
Class members
The members of a class consist of the members introduced by its 
s and the members inherited from the direct base class.
The members of a class type are divided into the following categories:
Constants, which represent constant values associated with the class (
).
Fields, which are the variables of the class (
).
Methods, which implement the computations and actions that can be performed by the class (
).
Properties, which define named characteristics and the actions associated with reading and writing those characteristics (
).
Events, which define notifications that can be generated by the class (
).
Indexers, which permit instances of the class to be indexed in the same way (syntactically) as arrays (
).
Operators, which define the expression operators that can be applied to instances of the class (
).
Instance constructors, which implement the actions required to initialize instances of the class (
)
Destructors, which implement the actions to be performed before instances of the class are permanently discarded (
).
Static constructors, which implement the actions required to initialize the class itself (
).
Types, which represent the types that are local to the class (
).
Members that can contain executable code are collectively known as the 
function members
 of the class type. The function members of a class type are the methods, properties, events, indexers, operators, instance constructors,  destructors, and static constructors of that class type.
A 
 creates a new declaration space (
), and the 
s immediately contained by the 
 introduce new members into this declaration space. The following rules apply to 
s:
Instance constructors, destructors and static constructors must have the same name as the immediately enclosing class. All other members must have names that differ from the name of the immediately enclosing class.
The name of a constant, field, property, event, or type must differ from the names of all other members declared in the same class.
The name of a method must differ from the names of all other non-methods declared in the same class. In addition, the signature (
) of a method must differ from the signatures of all other methods declared in the same class, and two methods declared in the same class may not have signatures that differ solely by 
 and 
.
The signature of an instance constructor must differ from the signatures of all other instance constructors declared in the same class, and two constructors declared in the same class may not have signatures that differ solely by 
 and 
.
The signature of an indexer must differ from the signatures of all other indexers declared in the same class.
The signature of an operator must differ from the signatures of all other operators declared in the same class.
The inherited members of a class type (
) are not part of the declaration space of a class. Thus, a derived class is allowed to declare a member with the same name or signature as an inherited member (which in effect hides the inherited member).
The instance type
Each class declaration has an associated bound type (
), the 
. For a generic class declaration, the instance type is formed by creating a constructed type (
) from the type declaration, with each of the supplied type arguments being the corresponding type parameter. Since the instance type uses the type parameters, it can only be used where the type parameters are in scope; that is, inside the class declaration. The instance type is the type of 
 for code written inside the class declaration. For non-generic classes, the instance type is simply the declared class. The following shows several class declarations along with their instance types:
Members of constructed types
The non-inherited members of a constructed type are obtained by substituting, for each 
 in the member declaration, the corresponding 
 of the constructed type. The substitution process is based on the semantic meaning of type declarations, and is not simply textual substitution.
For example, given the generic class declaration
the constructed type 
 has the following members:
The type of the member 
 in the generic class declaration 
 is ""two-dimensional array of 
"", so the type of the member 
 in the constructed type above is ""two-dimensional array of one-dimensional array of 
"", or 
.
Within instance function members, the type of 
 is the instance type (
) of the containing declaration.
All members of a generic class can use type parameters from any enclosing class, either directly or as part of a constructed type. When a particular closed constructed type (
) is used at run-time, each use of a type parameter is replaced with the actual type argument supplied to the constructed type. For example:
Inheritance
A class 
 the members of its direct base class type. Inheritance means that a class implicitly contains all members of its direct base class type, except for the instance constructors, destructors and static constructors of the base class. Some important aspects of inheritance are:
Inheritance is transitive. If 
 is derived from 
, and 
 is derived from 
, then 
 inherits the members declared in 
 as well as the members declared in 
.
A derived class extends its direct base class. A derived class can add new members to those it inherits, but it cannot remove the definition of an inherited member.
Instance constructors, destructors, and static constructors are not inherited, but all other members are, regardless of their declared accessibility (
). However, depending on their declared accessibility, inherited members might not be accessible in a derived class.
A derived class can 
 (
) inherited members by declaring new members with the same name or signature. Note however that hiding an inherited member does not remove that member—it merely makes that member inaccessible directly through the derived class.
An instance of a class contains a set of all instance fields declared in the class and its base classes, and an implicit conversion (
) exists from a derived class type to any of its base class types. Thus, a reference to an instance of some derived class can be treated as a reference to an instance of any of its base classes.
A class can declare virtual methods, properties, and indexers, and derived classes can override the implementation of these function members. This enables classes to exhibit polymorphic behavior wherein the actions performed by a function member invocation varies depending on the run-time type of the instance through which that function member is invoked.
The inherited member of a constructed class type are the members of the immediate base class type (
), which is found by substituting the type arguments of the constructed type for each occurrence of the corresponding type parameters in the 
 specification. These members, in turn, are transformed by substituting, for each 
 in the member declaration, the corresponding 
 of the 
 specification.
In the above example, the constructed type 
 has a non-inherited member 
 obtained by substituting the type argument 
 for the type parameter 
. 
 also has an inherited member from the class declaration 
. This inherited member is determined by first determining the base class type 
 of 
 by substituting 
 for 
 in the base class specification 
. Then, as a type argument to 
, 
 is substituted for 
 in 
, yielding the inherited member 
.
The new modifier
A 
 is permitted to declare a member with the same name or signature as an inherited member. When this occurs, the derived class member is said to 
 the base class member. Hiding an inherited member is not considered an error, but it does cause the compiler to issue a warning. To suppress the warning, the declaration of the derived class member can include a 
 modifier to indicate that the derived member is intended to hide the base member. This topic is discussed further in 
.
If a 
 modifier is included in a declaration that doesn't hide an inherited member, a warning to that effect is issued. This warning is suppressed by removing the 
 modifier.
Access modifiers
A 
 can have any one of the five possible kinds of declared accessibility (
): 
, 
, 
, 
, or 
. Except for the 
 combination, it is a compile-time error to specify more than one access modifier. When a 
 does not include any access modifiers, 
 is assumed.
Constituent types
Types that are used in the declaration of a member are called the constituent types of that member. Possible constituent types are the type of a constant, field, property, event, or indexer, the return type of a method or operator, and the parameter types of a method, indexer, operator, or instance constructor. The constituent types of a member must be at least as accessible as that member itself (
).
Static and instance members
Members of a class are either 
 or 
. Generally speaking, it is useful to think of static members as belonging to class types and instance members as belonging to objects (instances of class types).
When a field, method, property, event, operator, or constructor declaration includes a 
 modifier, it declares a static member. In addition, a constant or type declaration implicitly declares a static member. Static members have the following characteristics:
When a static member 
 is referenced in a 
 (
) of the form 
, 
 must denote a type containing 
. It is a compile-time error for 
 to denote an instance.
A static field identifies exactly one storage location to be shared by all instances of a given closed class type. No matter how many instances of a given closed class type are created, there is only ever one copy of a static field.
A static function member (method, property, event, operator, or constructor) does not operate on a specific instance, and it is a compile-time error to refer to 
 in such a function member.
When a field, method, property, event, indexer, constructor, or destructor declaration does not include a 
 modifier, it declares an instance member. (An instance member is sometimes called a non-static member.) Instance members have the following characteristics:
When an instance member 
 is referenced in a 
 (
) of the form 
, 
 must denote an instance of a type containing 
. It is a binding-time error for 
 to denote a type.
Every instance of a class contains a separate set of all instance fields of the class.
An instance function member (method, property, indexer, instance constructor, or destructor) operates on a given instance of the class, and this instance can be accessed as 
 (
).
The following example illustrates the rules for accessing static and instance members:
The 
 method shows that in an instance function member, a 
 (
) can be used to access both instance members and static members. The 
 method shows that in a static function member, it is a compile-time error to access an instance member through a 
. The 
 method shows that in a 
 (
), instance members must be accessed through instances, and static members must be accessed through types.
Nested types
A type declared within a class or struct declaration is called a 
. A type that is declared within a compilation unit or namespace is called a 
.
In the example
class 
 is a nested type because it is declared within class 
, and class 
 is a non-nested type because it is declared within a compilation unit.
Fully qualified name
The fully qualified name (
) for a nested type is 
 where 
 is the fully qualified name of the type in which type 
 is declared.
Declared accessibility
Non-nested types can have 
 or 
 declared accessibility and have 
 declared accessibility by default. Nested types can have these forms of declared accessibility too, plus one or more additional forms of declared accessibility, depending on whether the containing type is a class or struct:
A nested type that is declared in a class can have any of five forms of declared accessibility (
, 
, 
, 
, or 
) and, like other class members, defaults to 
 declared accessibility.
A nested type that is declared in a struct can have any of three forms of declared accessibility (
, 
, or 
) and, like other struct members, defaults to 
 declared accessibility.
The example
declares a private nested class 
.
Hiding
A nested type may hide (
) a base member. The 
 modifier is permitted on nested type declarations so that hiding can be expressed explicitly. The example
shows a nested class 
 that hides the method 
 defined in 
.
this access
A nested type and its containing type do not have a special relationship with regard to 
 (
). Specifically, 
 within a nested type cannot be used to refer to instance members of the containing type. In cases where a nested type needs access to the instance members of its containing type, access can be provided by providing the 
 for the instance of the containing type as a constructor argument for the nested type. The following example
shows this technique. An instance of 
 creates an instance of 
 and passes its own 
 to 
's constructor in order to provide subsequent access to 
's instance members.
Access to private and protected members of the containing type
A nested type has access to all of the members that are accessible to its containing type, including members of the containing type that have 
 and 
 declared accessibility. The example
shows a class 
 that contains a nested class 
. Within 
, the method 
 calls the static method 
 defined in 
, and 
 has private declared accessibility.
A nested type also may access protected members defined in a base type of its containing type. In the example
the nested class 
 accesses the protected method 
 defined in 
's base class, 
, by calling through an instance of 
.
Nested types in generic classes
A generic class declaration can contain nested type declarations. The type parameters of the enclosing class can be used within the nested types. A nested type declaration can contain additional type parameters that apply only to the nested type.
Every type declaration contained within a generic class declaration is implicitly a generic type declaration. When writing a reference to a type nested within a generic type, the containing constructed type, including its type arguments, must be named. However, from within the outer class, the nested type can be used without qualification; the instance type of the outer class can be implicitly used when constructing the nested type. The following example shows three different correct ways to refer to a constructed type created from 
; the first two are equivalent:
Although it is bad programming style, a type parameter in a nested type can hide a member or type parameter declared in the outer type:
Reserved member names
To facilitate the underlying C# run-time implementation, for each source member declaration that is a property, event, or indexer, the implementation must reserve two method signatures based on the kind of the member declaration, its name, and its type. It is a compile-time error for a program to declare a member whose signature matches one of these reserved signatures, even if the underlying run-time implementation does not make use of these reservations.
The reserved names do not introduce declarations, thus they do not participate in member lookup. However, a declaration's associated reserved method signatures do participate in inheritance (
), and can be hidden with the 
 modifier (
).
The reservation of these names serves three purposes:
To allow the underlying implementation to use an ordinary identifier as a method name for get or set access to the C# language feature.
To allow other languages to interoperate using an ordinary identifier as a method name for get or set access to the C# language feature.
To help ensure that the source accepted by one conforming compiler is accepted by another, by making the specifics of reserved member names consistent across all C# implementations.
The declaration of a destructor (
) also causes a signature to be reserved (
).
Member names reserved for properties
For a property 
 (
) of type 
, the following signatures are reserved:
Both signatures are reserved, even if the property is read-only or write-only.
In the example
a class 
 defines a read-only property 
, thus reserving signatures for 
 and 
 methods. A class 
 derives from 
 and hides both of these reserved signatures. The example produces the output:
Member names reserved for events
For an event 
 (
) of delegate type 
, the following signatures are reserved:
Member names reserved for indexers
For an indexer (
) of type 
 with parameter-list 
, the following signatures are reserved:
Both signatures are reserved, even if the indexer is read-only or write-only.
Furthermore the member name 
 is reserved.
Member names reserved for destructors
For a class containing a destructor (
), the following signature is reserved:
Constants
A 
 is a class member that represents a constant value: a value that can be computed at compile-time. A 
 introduces one or more constants of a given type.
A 
 may include a set of 
 (
), a 
 modifier (
), and a valid combination of the four access modifiers (
). The attributes and modifiers apply to all of the members declared by the 
. Even though constants are considered static members, a 
 neither requires nor allows a 
 modifier. It is an error for the same modifier to appear multiple times in a constant declaration.
The 
 of a 
 specifies the type of the members introduced by the declaration. The type is followed by a list of 
s, each of which introduces a new member. A 
 consists of an 
 that names the member, followed by an ""
"" token, followed by a 
 (
) that gives the value of the member.
The 
 specified in a constant declaration must be 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, an 
, or a 
. Each 
 must yield a value of the target type or of a type that can be converted to the target type by an implicit conversion (
).
The 
 of a constant must be at least as accessible as the constant itself (
).
The value of a constant is obtained in an expression using a 
 (
) or a 
 (
).
A constant can itself participate in a 
. Thus, a constant may be used in any construct that requires a 
. Examples of such constructs include 
 labels, 
 statements, 
 member declarations, attributes, and other constant declarations.
As described in 
, a 
 is an expression that can be fully evaluated at compile-time. Since the only way to create a non-null value of a 
 other than 
 is to apply the 
 operator, and since the 
 operator is not permitted in a 
, the only possible value for constants of 
s other than 
 is 
.
When a symbolic name for a constant value is desired, but when the type of that value is not permitted in a constant declaration, or when the value cannot be computed at compile-time by a 
, a 
 field (
) may be used instead.
A constant declaration that declares multiple constants is equivalent to multiple declarations of single constants with the same attributes, modifiers, and type. For example
is equivalent to
Constants are permitted to depend on other constants within the same program as long as the dependencies are not of a circular nature. The compiler automatically arranges to evaluate the constant declarations in the appropriate order. In the example
the compiler first evaluates 
, then evaluates 
, and finally evaluates 
, producing the values 
, 
, and 
. Constant declarations may depend on constants from other programs, but such dependencies are only possible in one direction. Referring to the example above, if 
 and 
 were declared in separate programs, it would be possible for 
 to depend on 
, but 
 could then not simultaneously depend on 
.
Fields
A 
 is a member that represents a variable associated with an object or class. A 
 introduces one or more fields of a given type.
A 
 may include a set of 
 (
), a 
 modifier (
), a valid combination of the four access modifiers (
), and a 
 modifier (
). In addition, a 
 may include a 
 modifier (
) or a 
 modifier (
) but not both. The attributes and modifiers apply to all of the members declared by the 
. It is an error for the same modifier to appear multiple times in a field declaration.
The 
 of a 
 specifies the type of the members introduced by the declaration. The type is followed by a list of 
s, each of which introduces a new member. A 
 consists of an 
 that names that member, optionally followed by an ""
"" token and a 
 (
) that gives the initial value of that member.
The 
 of a field must be at least as accessible as the field itself (
).
The value of a field is obtained in an expression using a 
 (
) or a 
 (
). The value of a non-readonly field is modified using an 
 (
). The value of a non-readonly field can be both obtained and modified using postfix increment and decrement operators (
) and prefix increment and decrement operators (
).
A field declaration that declares multiple fields is equivalent to multiple declarations of single fields with the same attributes, modifiers, and type. For example
is equivalent to
Static and instance fields
When a field declaration includes a 
 modifier, the fields introduced by the declaration are 
. When no 
 modifier is present, the fields introduced by the declaration are 
. Static fields and instance fields are two of the several kinds of variables (
) supported by C#, and at times they are referred to as 
 and 
, respectively.
A static field is not part of a specific instance; instead, it is shared amongst all instances of a closed type (
). No matter how many instances of a closed class type are created, there is only ever one copy of a static field for the associated application domain.
For example:
An instance field belongs to an instance. Specifically, every instance of a class contains a separate set of all the instance fields of that class.
When a field is referenced in a 
 (
) of the form 
, if 
 is a static field, 
 must denote a type containing 
, and if 
 is an instance field, E must denote an instance of a type containing 
.
The differences between static and instance members are discussed further in 
.
Readonly fields
When a 
 includes a 
 modifier, the fields introduced by the declaration are 
. Direct assignments to readonly fields can only occur as part of that declaration or in an instance constructor or static constructor in the same class. (A readonly field can be assigned to multiple times in these contexts.) Specifically, direct assignments to a 
 field are permitted only in the following contexts:
In the 
 that introduces the field (by including a 
 in the declaration).
For an instance field, in the instance constructors of the class that contains the field declaration; for a static field, in the static constructor of the class that contains the field declaration. These are also the only contexts in which it is valid to pass a 
 field as an 
 or 
 parameter.
Attempting to assign to a 
 field or pass it as an 
 or 
 parameter in any other context is a compile-time error.
Using static readonly fields for constants
A 
 field is useful when a symbolic name for a constant value is desired, but when the type of the value is not permitted in a 
 declaration, or when the value cannot be computed at compile-time. In the example
the 
, 
, 
, 
, and 
 members cannot be declared as 
 members because their values cannot be computed at compile-time. However, declaring them 
 instead has much the same effect.
Versioning of constants and static readonly fields
Constants and readonly fields have different binary versioning semantics. When an expression references a constant, the value of the constant is obtained at compile-time, but when an expression references a readonly field, the value of the field is not obtained until run-time. Consider an application that consists of two separate programs:
The 
 and 
 namespaces denote two programs that are compiled separately. Because 
 is declared as a static readonly field, the value output by the 
 statement is not known at compile-time, but rather is obtained at run-time. Thus, if the value of 
 is changed and 
 is recompiled, the 
 statement will output the new value even if 
 isn't recompiled. However, had 
 been a constant, the value of 
 would have been obtained at the time 
 was compiled, and would remain unaffected by changes in 
 until 
 is recompiled.
Volatile fields
When a 
 includes a 
 modifier, the fields introduced by that declaration are 
.
For non-volatile fields, optimization techniques that reorder instructions can lead to unexpected and unpredictable results in multi-threaded programs that access fields without synchronization such as that provided by the 
 (
). These optimizations can be performed by the compiler, by the run-time system, or by hardware. For volatile fields, such reordering optimizations are restricted:
A read of a volatile field is called a 
. A volatile read has ""acquire semantics""; that is, it is guaranteed to occur prior to any references to memory that occur after it in the instruction sequence.
A write of a volatile field is called a 
. A volatile write has ""release semantics""; that is, it is guaranteed to happen after any memory references prior to the write instruction in the instruction sequence.
These restrictions ensure that all threads will observe volatile writes performed by any other thread in the order in which they were performed. A conforming implementation is not required to provide a single total ordering of volatile writes as seen from all threads of execution. The type of a volatile field must be one of the following:
A 
.
The type 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or
.
An 
 having an enum base type of 
, 
, 
, 
, 
, or 
.
The example
produces the output:
In this example, the method 
 starts a new thread that runs the method 
. This method stores a value into a non-volatile field called 
, then stores 
 in the volatile field 
. The main thread waits for the field 
 to be set to 
, then reads the field 
. Since 
 has been declared 
, the main thread must read the value 
 from the field 
. If the field 
 had not been declared 
, then it would be permissible for the store to 
 to be visible to the main thread after the store to 
, and hence for the main thread to read the value 
 from the field 
. Declaring 
 as a 
 field prevents any such inconsistency.
Field initialization
The initial value of a field, whether it be a static field or an instance field, is the default value (
) of the field's type. It is not possible to observe the value of a field before this default initialization has occurred, and a field is thus never ""uninitialized"". The example
produces the output
because 
 and 
 are both automatically initialized to default values.
Variable initializers
Field declarations may include 
s. For static fields, variable initializers correspond to assignment statements that are executed during class initialization. For instance fields, variable initializers correspond to assignment statements that are executed when an instance of the class is created.
The example
produces the output
because an assignment to 
 occurs when static field initializers execute and assignments to 
 and 
 occur when the instance field initializers execute.
The default value initialization described in 
 occurs for all fields, including fields that have variable initializers. Thus, when a class is initialized, all static fields in that class are first initialized to their default values, and then the static field initializers are executed in textual order. Likewise, when an instance of a class is created, all instance fields in that instance are first initialized to their default values, and then the instance field initializers are executed in textual order.
It is possible for static fields with variable initializers to be observed in their default value state. However, this is strongly discouraged as a matter of style. The example
exhibits this behavior. Despite the circular definitions of a and b, the program is valid. It results in the output
because the static fields 
 and 
 are initialized to 
 (the default value for 
) before their initializers are executed. When the initializer for 
 runs, the value of 
 is zero, and so 
 is initialized to 
. When the initializer for 
 runs, the value of 
 is already 
, and so 
 is initialized to 
.
Static field initialization
The static field variable initializers of a class correspond to a sequence of assignments that are executed in the textual order in which they appear in the class declaration. If a static constructor (
) exists in the class, execution of the static field initializers occurs immediately prior to executing that static constructor. Otherwise, the static field initializers are executed at an implementation-dependent time prior to the first use of a static field of that class. The example
might produce either the output:
or the output:
because the execution of 
's initializer and 
's initializer could occur in either order; they are only constrained to occur before the references to those fields. However, in the example:
the output must be:
because the rules for when static constructors execute (as defined in 
) provide that 
's static constructor (and hence 
's static field initializers) must run before 
's static constructor and field initializers.
Instance field initialization
The instance field variable initializers of a class correspond to a sequence of assignments that are executed immediately upon entry to any one of the instance constructors (
) of that class. The variable initializers are executed in the textual order in which they appear in the class declaration. The class instance creation and initialization process is described further in 
.
A variable initializer for an instance field cannot reference the instance being created. Thus, it is a compile-time error to reference 
 in a variable initializer, as it is a compile-time error for a variable initializer to reference any instance member through a 
. In the example
the variable initializer for 
 results in a compile-time error because it references a member of the instance being created.
Methods
A 
 is a member that implements a computation or action that can be performed by an object or class. Methods are declared using 
s:
A 
 may include a set of 
 (
) and a valid combination of the four access modifiers (
), the 
 (
),  
 (
), 
 (
), 
 (
), 
 (
), 
 (
), and 
 (
) modifiers.
A declaration has a valid combination of modifiers if all of the following are true:
The declaration includes a valid combination of access modifiers (
).
The declaration does not include the same modifier multiple times.
The declaration includes at most one of the following modifiers: 
, 
, and 
.
The declaration includes at most one of the following modifiers: 
 and 
.
If the declaration includes the 
 modifier, then the declaration does not include any of the following modifiers: 
, 
, 
 or 
.
If the declaration includes the 
 modifier, then the declaration does not include any of the following modifiers: 
, 
, or 
.
If the declaration includes the 
 modifier, then the declaration also includes the 
 modifier.
If the declaration includes the 
 modifier, then it does not include any of the following modifiers: 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
A method that has the async modifier is an async function and follows the rules described in 
.
The 
 of a method declaration specifies the type of the value computed and returned by the method. The 
 is 
 if the method does not return a value. If the declaration includes the 
 modifier, then the return type must be 
.
The 
 specifies the name of the method. Unless the method is an explicit interface member implementation (
), the 
 is simply an 
. For an explicit interface member implementation, the 
 consists of an 
 followed by a ""
"" and an 
.
The optional 
 specifies the type parameters of the method (
). If a 
 is specified the method is a 
. If the method has an 
 modifier, a 
 cannot be specified.
The optional 
 specifies the parameters of the method (
).
The optional 
s specify constraints on individual type parameters (
) and may only be specified if a 
 is also supplied, and the method does not have an 
 modifier.
The 
 and each of the types referenced in the 
 of a method must be at least as accessible as the method itself (
).
The 
 is either a semicolon, a 
 or an 
. A statement body consists of a 
, which specifies the statements to execute when the method is invoked. An expression body consists of 
 followed by an 
 and a semicolon, and denotes a single expression to perform when the method is invoked.
For 
 and 
 methods, the 
 consists simply of a semicolon. For 
 methods the 
 may consist of either a semicolon, a block body or an expression body. For all other methods, the 
 is either a block body or an expression body.
If the 
 consists of a semicolon, then the declaration may not include the 
 modifier.
The name, the type parameter list and the formal parameter list of a method define the signature (
) of the method. Specifically, the signature of a method consists of its name, the number of type parameters and the number, modifiers, and types of its formal parameters. For these purposes, any type parameter of the method that occurs in the type of a formal parameter is identified not by its name, but by its ordinal position in the type argument list of the method.The return type is not part of a method's signature, nor are the names of the type parameters or the formal parameters.
The name of a method must differ from the names of all other non-methods declared in the same class. In addition, the signature of a method must differ from the signatures of all other methods declared in the same class, and two methods declared in the same class may not have signatures that differ solely by 
 and 
.
The method's 
s are in scope throughout the 
, and can be used to form types throughout that scope in 
, 
, and 
s but not in 
.
All formal parameters and type parameters must have different names.
Method parameters
The parameters of a method, if any, are declared by the method's 
.
The formal parameter list consists of one or more comma-separated parameters of which only the last may be a 
.
A 
 consists of an optional set of 
 (
), an optional 
, 
 or 
 modifier, a 
, an 
 and an optional 
. Each 
 declares a parameter of the given type with the given name. The 
 modifier designates the method as an extension method and is only allowed on the first parameter of a static method. Extension methods are further described in 
.
A 
 with a 
 is known as an 
, whereas a 
 without a 
 is a 
. A required parameter may not appear after an optional parameter in a 
.
A 
 or 
 parameter cannot have a 
. The 
 in a 
 must be one of the following:
a 
an expression of the form 
 where 
 is a value type
an expression of the form 
 where 
 is a value type
The 
 must be implicitly convertible by an identity or nullable conversion to the type of the parameter.
If optional parameters occur in an implementing partial method declaration (
) , an explicit interface member implementation (
) or in a single-parameter indexer declaration (
) the compiler should give a warning, since these members can never be invoked in a way that permits arguments to be omitted.
A 
 consists of an optional set of 
 (
), a 
 modifier, an 
, and an 
. A parameter array declares a single parameter of the given array type with the given name. The 
 of a parameter array must be a single-dimensional array type (
). In a method invocation, a parameter array permits either a single argument of the given array type to be specified, or it permits zero or more arguments of the array element type to be specified. Parameter arrays are described further in 
.
A 
 may occur after an optional parameter, but cannot have a default value -- the omission of arguments for a 
 would instead result in the creation of an empty array.
The following example illustrates different kinds of parameters:
In the 
 for 
, 
 is a required ref parameter, 
 is a required value parameter, 
, 
, 
 and 
 are optional value parameters and 
 is a parameter array.
A method declaration creates a separate declaration space for parameters, type parameters and local variables. Names are introduced into this declaration space by the type parameter list and the formal parameter list of the method and by local variable declarations in the 
 of the method. It is an error for two members of a method declaration space to have the same name. It is an error for the method declaration space and the local variable declaration space of a nested declaration space to contain elements with the same name.
A method invocation (
) creates a copy, specific to that invocation, of the formal parameters and local variables of the method, and the argument list of the invocation assigns values or variable references to the newly created formal parameters. Within the 
 of a method, formal parameters can be referenced by their identifiers in 
 expressions (
).
There are four kinds of formal parameters:
Value parameters, which are declared without any modifiers.
Reference parameters, which are declared with the 
 modifier.
Output parameters, which are declared with the 
 modifier.
Parameter arrays, which are declared with the 
 modifier.
As described in 
, the 
 and 
 modifiers are part of a method's signature, but the 
 modifier is not.
Value parameters
A parameter declared with no modifiers is a value parameter. A value parameter corresponds to a local variable that gets its initial value from the corresponding argument supplied in the method invocation.
When a formal parameter is a value parameter, the corresponding argument in a method invocation must be an expression that is implicitly convertible (
) to the formal parameter type.
A method is permitted to assign new values to a value parameter. Such assignments only affect the local storage location represented by the value parameter—they have no effect on the actual argument given in the method invocation.
Reference parameters
A parameter declared with a 
 modifier is a reference parameter. Unlike a value parameter, a reference parameter does not create a new storage location. Instead, a reference parameter represents the same storage location as the variable given as the argument in the method invocation.
When a formal parameter is a reference parameter, the corresponding argument in a method invocation must consist of the keyword 
 followed by a 
 (
) of the same type as the formal parameter. A variable must be definitely assigned before it can be passed as a reference parameter.
Within a method, a reference parameter is always considered definitely assigned.
A method declared as an iterator (
) cannot have reference parameters.
The example
produces the output
For the invocation of 
 in 
, 
 represents 
 and 
 represents 
. Thus, the invocation has the effect of swapping the values of 
 and 
.
In a method that takes reference parameters it is possible for multiple names to represent the same storage location. In the example
the invocation of 
 in 
 passes a reference to 
 for both 
 and 
. Thus, for that invocation, the names 
, 
, and 
 all refer to the same storage location, and the three assignments all modify the instance field 
.
Output parameters
A parameter declared with an 
 modifier is an output parameter. Similar to a reference parameter, an output parameter does not create a new storage location. Instead, an output parameter represents the same storage location as the variable given as the argument in the method invocation.
When a formal parameter is an output parameter, the corresponding argument in a method invocation must consist of the keyword 
 followed by a 
 (
) of the same type as the formal parameter. A variable need not be definitely assigned before it can be passed as an output parameter, but following an invocation where a variable was passed as an output parameter, the variable is considered definitely assigned.
Within a method, just like a local variable, an output parameter is initially considered unassigned and must be definitely assigned before its value is used.
Every output parameter of a method must be definitely assigned before the method returns.
A method declared as a partial method (
) or an iterator (
) cannot have output parameters.
Output parameters are typically used in methods that produce multiple return values. For example:
The example produces the output:
Note that the 
 and 
 variables can be unassigned before they are passed to 
, and that they are considered definitely assigned following the call.
Parameter arrays
A parameter declared with a 
 modifier is a parameter array. If a formal parameter list includes a parameter array, it must be the last parameter in the list and it must be of a single-dimensional array type. For example, the types 
 and 
 can be used as the type of a parameter array, but the type 
 can not. It is not possible to combine the 
 modifier with the modifiers 
 and 
.
A parameter array permits arguments to be specified in one of two ways in a method invocation:
The argument given for a parameter array can be a single expression that is implicitly convertible (
) to the parameter array type. In this case, the parameter array acts precisely like a value parameter.
Alternatively, the invocation can specify zero or more arguments for the parameter array, where each argument is an expression that is implicitly convertible (
) to the element type of the parameter array. In this case, the invocation creates an instance of the parameter array type with a length corresponding to the number of arguments, initializes the elements of the array instance with the given argument values, and uses the newly created array instance as the actual argument.
Except for allowing a variable number of arguments in an invocation, a parameter array is precisely equivalent to a value parameter (
) of the same type.
The example
produces the output
The first invocation of 
 simply passes the array 
 as a value parameter. The second invocation of 
 automatically creates a four-element 
 with the given element values and passes that array instance as a value parameter. Likewise, the third invocation of 
 creates a zero-element 
 and passes that instance as a value parameter. The second and third invocations are precisely equivalent to writing:
When performing overload resolution, a method with a parameter array may be applicable either in its normal form or in its expanded form (
). The expanded form of a method is available only if the normal form of the method is not applicable and only if an applicable method with the same signature as the expanded form is not already declared in the same type.
The example
produces the output
In the example, two of the possible expanded forms of the method with a parameter array are already included in the class as regular methods. These expanded forms are therefore not considered when performing overload resolution, and the first and third method invocations thus select the regular methods. When a class declares a method with a parameter array, it is not uncommon to also include some of the expanded forms as regular methods. By doing so it is possible to avoid the allocation of an array instance that occurs when an expanded form of a method with a parameter array is invoked.
When the type of a parameter array is 
, a potential ambiguity arises between the normal form of the method and the expended form for a single 
 parameter. The reason for the ambiguity is that an 
 is itself implicitly convertible to type 
. The ambiguity presents no problem, however, since it can be resolved by inserting a cast if needed.
The example
produces the output
In the first and last invocations of 
, the normal form of 
 is applicable because an implicit conversion exists from the argument type to the parameter type (both are of type 
). Thus, overload resolution selects the normal form of 
, and the argument is passed as a regular value parameter. In the second and third invocations, the normal form of 
 is not applicable because no implicit conversion exists from the argument type to the parameter type (type 
 cannot be implicitly converted to type 
). However, the expanded form of 
 is applicable, so it is selected by overload resolution. As a result, a one-element 
 is created by the invocation, and the single element of the array is initialized with the given argument value (which itself is a reference to an 
).
Static and instance methods
When a method declaration includes a 
 modifier, that method is said to be a static method. When no 
 modifier is present, the method is said to be an instance method.
A static method does not operate on a specific instance, and it is a compile-time error to refer to 
 in a static method.
An instance method operates on a given instance of a class, and that instance can be accessed as 
 (
).
When a method is referenced in a 
 (
) of the form 
, if 
 is a static method, 
 must denote a type containing 
, and if 
 is an instance method, 
 must denote an instance of a type containing 
.
The differences between static and instance members are discussed further in 
.
Virtual methods
When an instance method declaration includes a 
 modifier, that method is said to be a virtual method. When no 
 modifier is present, the method is said to be a non-virtual method.
The implementation of a non-virtual method is invariant: The implementation is the same whether the method is invoked on an instance of the class in which it is declared or an instance of a derived class. In contrast, the implementation of a virtual method can be superseded by derived classes. The process of superseding the implementation of an inherited virtual method is known as 
 that method (
).
In a virtual method invocation, the 
 of the instance for which that invocation takes place determines the actual method implementation to invoke. In a non-virtual method invocation, the 
 of the instance is the determining factor. In precise terms, when a method named 
 is invoked with an argument list 
 on an instance with a compile-time type 
 and a run-time type 
 (where 
 is either 
 or a class derived from 
), the invocation is processed as follows:
First, overload resolution is applied to 
, 
, and 
, to select a specific method 
 from the set of methods declared in and inherited by 
. This is described in 
.
Then, if 
 is a non-virtual method, 
 is invoked.
Otherwise, 
 is a virtual method, and the most derived implementation of 
 with respect to 
 is invoked.
For every virtual method declared in or inherited by a class, there exists a 
 of the method with respect to that class. The most derived implementation of a virtual method 
 with respect to a class 
 is determined as follows:
If 
 contains the introducing 
 declaration of 
, then this is the most derived implementation of 
.
Otherwise, if 
 contains an 
 of 
, then this is the most derived implementation of 
.
Otherwise, the most derived implementation of 
 with respect to 
 is the same as the most derived implementation of 
 with respect to the direct base class of 
.
The following example illustrates the differences between virtual and non-virtual methods:
In the example, 
 introduces a non-virtual method 
 and a virtual method 
. The class 
 introduces a new non-virtual method 
, thus hiding the inherited 
, and also overrides the inherited method 
. The example produces the output:
Notice that the statement 
 invokes 
, not 
. This is because the run-time type of the instance (which is 
), not the compile-time type of the instance (which is 
), determines the actual method implementation to invoke.
Because methods are allowed to hide inherited methods, it is possible for a class to contain several virtual methods with the same signature. This does not present an ambiguity problem, since all but the most derived method are hidden. In the example
the 
 and 
 classes contain two virtual methods with the same signature: The one introduced by 
 and the one introduced by 
. The method introduced by 
 hides the method inherited from 
. Thus, the override declaration in 
 overrides the method introduced by 
, and it is not possible for 
 to override the method introduced by 
. The example produces the output:
Note that it is possible to invoke the hidden virtual method by accessing an instance of 
 through a less derived type in which the method is not hidden.
Override methods
When an instance method declaration includes an 
 modifier, the method is said to be an 
. An override method overrides an inherited virtual method with the same signature. Whereas a virtual method declaration introduces a new method, an override method declaration specializes an existing inherited virtual method by providing a new implementation of that method.
The method overridden by an 
 declaration is known as the 
. For an override method 
 declared in a class 
, the overridden base method is determined by examining each base class type of 
, starting with the direct base class type of 
 and continuing with each successive direct base class type, until in a given base class type at least one accessible method is located which has the same signature as 
 after substitution of type arguments. For the purposes of locating the overridden base method, a method is considered accessible if it is 
, if it is 
, if it is 
, or if it is 
 and declared in the same program as 
.
A compile-time error occurs unless all of the following are true for an override declaration:
An overridden base method can be located as described above.
There is exactly one such overridden base method. This restriction has effect only if the base class type is a constructed type where the substitution of type arguments makes the signature of two methods the same.
The overridden base method is a virtual, abstract, or override method. In other words, the overridden base method cannot be static or non-virtual.
The overridden base method is not a sealed method.
The override method and the overridden base method have the same return type.
The override declaration and the overridden base method have the same declared accessibility. In other words, an override declaration cannot change the accessibility of the virtual method. However, if the overridden base method is protected internal and it is declared in a different assembly than the assembly containing the override method then the override method's declared accessibility must be protected.
The override declaration does not specify type-parameter-constraints-clauses. Instead the constraints are inherited from the overridden base method. Note that constraints that are type parameters in the overridden method may be replaced by type arguments in the inherited constraint. This can lead to constraints that are not legal when explicitly specified, such as value types or sealed types.
The following example demonstrates how the overriding rules work for generic classes:
An override declaration can access the overridden base method using a 
 (
). In the example
the 
 invocation in 
 invokes the 
 method declared in 
. A 
 disables the virtual invocation mechanism and simply treats the base method as a non-virtual method. Had the invocation in 
 been written 
, it would recursively invoke the 
 method declared in 
, not the one declared in 
, since 
 is virtual and the run-time type of 
 is 
.
Only by including an 
 modifier can a method override another method. In all other cases, a method with the same signature as an inherited method simply hides the inherited method. In the example
the 
 method in 
 does not include an 
 modifier and therefore does not override the 
 method in 
. Rather, the 
 method in 
 hides the method in 
, and a warning is reported because the declaration does not include a 
 modifier.
In the example
the 
 method in 
 hides the virtual 
 method inherited from 
. Since the new 
 in 
 has private access, its scope only includes the class body of 
 and does not extend to 
. Therefore, the declaration of 
 in 
 is permitted to override the 
 inherited from 
.
Sealed methods
When an instance method declaration includes a 
 modifier, that method is said to be a 
. If an instance method declaration includes the  
 modifier, it must also include the 
 modifier. Use of the 
 modifier prevents a derived class from further overriding the method.
In the example
the class 
 provides two override methods: an 
 method that has the 
 modifier and a 
 method that does not. 
's use of the sealed 
 prevents 
 from further overriding 
.
Abstract methods
When an instance method declaration includes an 
 modifier, that method is said to be an 
. Although an abstract method is implicitly also a virtual method, it cannot have the modifier 
.
An abstract method declaration introduces a new virtual method but does not provide an implementation of that method. Instead, non-abstract derived classes are required to provide their own implementation by overriding that method. Because an abstract method provides no actual implementation, the 
 of an abstract method simply consists of a semicolon.
Abstract method declarations are only permitted in abstract classes (
).
In the example
the 
 class defines the abstract notion of a geometrical shape object that can paint itself. The 
 method is abstract because there is no meaningful default implementation. The 
 and 
 classes are concrete 
 implementations. Because these classes are non-abstract, they are required to override the 
 method and provide an actual implementation.
It is a compile-time error for a 
 (
) to reference an abstract method. In the example
a compile-time error is reported for the 
 invocation because it references an abstract method.
An abstract method declaration is permitted to override a virtual method. This allows an abstract class to force re-implementation of the method in derived classes, and makes the original implementation of the method unavailable. In the example
class 
 declares a virtual method, class 
 overrides this method with an abstract method, and class 
 overrides the abstract method to provide its own implementation.
External methods
When a method declaration includes an 
 modifier, that method is said to be an 
. External methods are implemented externally, typically using a language other than C#. Because an external method declaration provides no actual implementation, the 
 of an external method simply consists of a semicolon. An external method may not be generic.
The 
 modifier is typically used in conjunction with a 
 attribute (
), allowing external methods to be implemented by DLLs (Dynamic Link Libraries). The execution environment may support other mechanisms whereby implementations of external methods can be provided.
When an external method includes a 
 attribute, the method declaration must also include a 
 modifier. This example demonstrates the use of the 
 modifier and the 
 attribute:
Partial methods (recap)
When a method declaration includes a 
 modifier, that method is said to be a 
. Partial methods can only be declared as members of partial types (
), and are subject to a number of restrictions. Partial methods are further described in 
.
Extension methods
When the first parameter of a method includes the 
 modifier, that method is said to be an 
. Extension methods can only be declared in non-generic, non-nested static classes. The first parameter of an extension method can have no modifiers other than 
, and the parameter type cannot be a pointer type.
The following is an example of a static class that declares two extension methods:
An extension method is a regular static method. In addition, where its enclosing static class is in scope, an extension method can be invoked using instance method invocation syntax (
), using the receiver expression as the first argument.
The following program uses the extension methods declared above:
The 
 method is available on the 
, and the 
 method is available on 
, because they have been declared as extension methods. The meaning of the program is the same as the following, using ordinary static method calls:
Method body
The 
 of a method declaration consists of either a block body, an expression body or a semicolon.
The 
 of a method is 
 if the return type is 
, or if the method is async and the return type is 
. Otherwise, the result type of a non-async method is its return type, and the result type of an async method with return type 
 is 
.
When a method has a 
 result type and a block body, 
 statements (
) in the block are not permitted to specify an expression. If execution of the block of a void method completes normally (that is, control flows off the end of the method body), that method simply returns to its current caller.
When a method has a 
 result and an expression body, the expression 
 must be a 
, and the body is exactly equivalent to a block body of the form 
.
When a method has a non-void result type and a block body, each 
 statement in the block must specify an expression that is implicitly convertible to the result type. The endpoint of a block body of a value-returning method must not be reachable. In other words, in a value-returning method with a block body, control is not permitted to flow off the end of the method body.
When a method has a non-void result type and an expression body, the expression must be implicitly convertible to the result type, and the body is exactly equivalent to a block body of the form 
.
In the example
the value-returning 
 method results in a compile-time error because control can flow off the end of the method body. The 
 and 
 methods are correct because all possible execution paths end in a return statement that specifies a return value. The 
 method is correct, because its body is equivalent to a statement block with just a single return statement in it.
Method overloading
The method overload resolution rules are described in 
.
Properties
A 
 is a member that provides access to a characteristic of an object or a class. Examples of properties include the length of a string, the size of a font, the caption of a window, the name of a customer, and so on. Properties are a natural extension of fields—both are named members with associated types, and the syntax for accessing fields and properties is the same. However, unlike fields, properties do not denote storage locations. Instead, properties have 
 that specify the statements to be executed when their values are read or written. Properties thus provide a mechanism for associating actions with the reading and writing of an object's attributes; furthermore, they permit such attributes to be computed.
Properties are declared using 
s:
A 
 may include a set of 
 (
) and a valid combination of the four access modifiers (
), the 
 (
),  
 (
), 
 (
), 
 (
), 
 (
), 
 (
), and 
 (
) modifiers.
Property declarations are subject to the same rules as method declarations (
) with regard to valid combinations of modifiers.
The 
 of a property declaration specifies the type of the property introduced by the declaration, and the 
 specifies the name of the property. Unless the property is an explicit interface member implementation, the 
 is simply an 
. For an explicit interface member implementation (
), the 
 consists of an 
 followed by a ""
"" and an 
.
The 
 of a property must be at least as accessible as the property itself (
).
A 
 may either consist of an 
 or an 
. In an accessor body,  
, which must be enclosed in ""
"" and ""
"" tokens, declare the accessors (
) of the property. The accessors specify the executable statements associated with reading and writing the property.
An expression body consisting of 
 followed by an 
 
 and a semicolon is exactly equivalent to the statement body 
, and can therefore only be used to specify getter-only properties where the result of the getter is given by a single expression.
A 
 may only be given for an automatically implemented property (
), and causes the initialization of the underlying field of such properties with the value given by the 
.
Even though the syntax for accessing a property is the same as that for a field, a property is not classified as a variable. Thus, it is not possible to pass a property as a 
 or 
 argument.
When a property declaration includes an 
 modifier, the property is said to be an 
. Because an external property declaration provides no actual implementation, each of its 
 consists of a semicolon.
Static and instance properties
When a property declaration includes a 
 modifier, the property is said to be a 
. When no 
 modifier is present, the property is said to be an 
.
A static property is not associated with a specific instance, and it is a compile-time error to refer to 
 in the accessors of a static property.
An instance property is associated with a given instance of a class, and that instance can be accessed as 
 (
) in the accessors of that property.
When a property is referenced in a 
 (
) of the form 
, if 
 is a static property, 
 must denote a type containing 
, and if 
 is an instance property, E must denote an instance of a type containing 
.
The differences between static and instance members are discussed further in 
.
Accessors
The 
 of a property specify the executable statements associated with reading and writing that property.
The accessor declarations consist of a 
, a 
, or both. Each accessor declaration consists of the token 
 or 
 followed by an optional 
 and an 
.
The use of 
s is governed by the following restrictions:
An 
 may not be used in an interface or in an explicit interface member implementation.
For a property or indexer that has no 
 modifer, an 
 is permitted only if the property or indexer has both a 
 and 
 accessor, and then is permitted only on one of those accessors.
For a property or indexer that includes an 
 modifer, an accessor must match the 
, if any, of the accessor being overridden.
The 
 must declare an accessibility that is strictly more restrictive than the declared accessibility of the property or indexer itself. To be precise:
If the property or indexer has a declared accessibility of 
, the 
 may be either 
, 
, 
, or 
.
If the property or indexer has a declared accessibility of 
, the 
 may be either 
, 
, or 
.
If the property or indexer has a declared accessibility of 
 or 
, the 
 must be 
.
If the property or indexer has a declared accessibility of 
, no 
 may be used.
For 
 and 
 properties, the 
 for each accessor specified is simply a semicolon. A non-abstract, non-extern property may have each 
 be a semicolon, in which case it is an 
 (
). An automatically implemented property must have at least a get accessor. For the accessors of any other non-abstract, non-extern property, the 
 is a 
 which specifies the statements to be executed when the corresponding accessor is invoked.
A 
 accessor corresponds to a parameterless method with a return value of the property type. Except as the target of an assignment, when a property is referenced in an expression, the 
 accessor of the property is invoked to compute the value of the property (
). The body of a 
 accessor must conform to the rules for value-returning methods described in 
. In particular, all 
 statements in the body of a 
 accessor must specify an expression that is implicitly convertible to the property type. Furthermore, the endpoint of a 
 accessor must not be reachable.
A 
 accessor corresponds to a method with a single value parameter of the property type and a 
 return type. The implicit parameter of a 
 accessor is always named 
. When a property is referenced as the target of an assignment (
), or as the operand of 
 or 
 (
, 
), the 
 accessor is invoked with an argument (whose value is that of the right-hand side of the assignment or the operand of the 
 or 
 operator) that provides the new value (
). The body of a 
 accessor must conform to the rules for 
 methods described in 
. In particular, 
 statements in the 
 accessor body are not permitted to specify an expression. Since a 
 accessor implicitly has a parameter named 
, it is a compile-time error for a local variable or constant declaration in a 
 accessor to have that name.
Based on the presence or absence of the 
 and 
 accessors, a property is classified as follows:
A property that includes both a 
 accessor and a 
 accessor is said to be a 
 property.
A property that has only a 
 accessor is said to be a 
 property. It is a compile-time error for a read-only property to be the target of an assignment.
A property that has only a 
 accessor is said to be a 
 property. Except as the target of an assignment, it is a compile-time error to reference a write-only property in an expression.
In the example
the 
 control declares a public 
 property. The 
 accessor of the 
 property returns the string stored in the private 
 field. The 
 accessor checks if the new value is different from the current value, and if so, it stores the new value and repaints the control. Properties often follow the pattern shown above: The 
 accessor simply returns a value stored in a private field, and the 
 accessor modifies that private field and then performs any additional actions required to fully update the state of the object.
Given the 
 class above, the following is an example of use of the 
 property:
Here, the 
 accessor is invoked by assigning a value to the property, and the 
 accessor is invoked by referencing the property in an expression.
The 
 and 
 accessors of a property are not distinct members, and it is not possible to declare the accessors of a property separately. As such, it is not possible for the two accessors of a read-write property to have different accessibility. The example
does not declare a single read-write property. Rather, it declares two properties with the same name, one read-only and one write-only. Since two members declared in the same class cannot have the same name, the example causes a compile-time error to occur.
When a derived class declares a property by the same name as an inherited property, the derived property hides the inherited property with respect to both reading and writing. In the example
the 
 property in 
 hides the 
 property in 
 with respect to both reading and writing. Thus, in the statements
the assignment to 
 causes a compile-time error to be reported, since the read-only 
 property in 
 hides the write-only 
 property in 
. Note, however, that a cast can be used to access the hidden 
 property.
Unlike public fields, properties provide a separation between an object's internal state and its public interface. Consider the example:
Here, the 
 class uses two 
 fields, 
 and 
, to store its location. The location is publicly exposed both as an 
 and a 
 property and as a 
 property of type 
. If, in a future version of 
, it becomes more convenient to store the location as a 
 internally, the change can be made without affecting the public interface of the class:
Had 
 and 
 instead been 
 fields, it would have been impossible to make such a change to the 
 class.
Exposing state through properties is not necessarily any less efficient than exposing fields directly. In particular, when a property is non-virtual and contains only a small amount of code, the execution environment may replace calls to accessors with the actual code of the accessors. This process is known as 
, and it makes property access as efficient as field access, yet preserves the increased flexibility of properties.
Since invoking a 
 accessor is conceptually equivalent to reading the value of a field, it is considered bad programming style for 
 accessors to have observable side-effects. In the example
the value of the 
 property depends on the number of times the property has previously been accessed. Thus, accessing the property produces an observable side-effect, and the property should be implemented as a method instead.
The ""no side-effects"" convention for 
 accessors doesn't mean that 
 accessors should always be written to simply return values stored in fields. Indeed, 
 accessors often compute the value of a property by accessing multiple fields or invoking methods. However, a properly designed 
 accessor performs no actions that cause observable changes in the state of the object.
Properties can be used to delay initialization of a resource until the moment it is first referenced. For example:
The 
 class contains three properties, 
, 
, and 
, that represent the standard input, output, and error devices, respectively. By exposing these members as properties, the 
 class can delay their initialization until they are actually used. For example, upon first referencing the 
 property, as in
the underlying 
 for the output device is created. But if the application makes no reference to the 
 and 
 properties, then no objects are created for those devices.
Automatically implemented properties
An automatically implemented property (or 
 for short), is a non-abstract non-extern property with semicolon-only accessor bodies. Auto-properties must have a get accessor and can optionally have a set accessor.
When a property is specified as an automatically implemented property, a hidden backing field is automatically available for the property, and the accessors are implemented to read from and write to that backing field. If the auto-property has no set accessor, the backing field is considered 
 (
). Just like a 
 field, a getter-only auto-property can also be assigned to in the body of a constructor of the enclosing class. Such an assignment assigns directly to the readonly backing field of the property.
An auto-property may optionally have a 
, which is applied directly to the backing field as a 
 (
).
The following example:
is equivalent to the following declaration:
The following example:
is equivalent to the following declaration:
Notice that the assignments to the readonly field are legal, because they occur within the constructor.
Accessibility
If an accessor has an 
, the accessibility domain (
) of the accessor is determined using the declared accessibility of the 
. If an accessor does not have an 
, the accessibility domain of the accessor is determined from the declared accessibility of the property or indexer.
The presence of an 
 never affects member lookup (
) or overload resolution (
). The modifiers on the property or indexer always determine which property or indexer is bound to, regardless of the context of the access.
Once a particular property or indexer has been selected, the accessibility domains of the specific accessors involved are used to determine if that usage is valid:
If the usage is as a value (
), the 
 accessor must exist and be accessible.
If the usage is as the target of a simple assignment (
), the 
 accessor must exist and be accessible.
If the usage is as the target of compound assignment (
), or as the target of the 
 or 
 operators (
.9, 
), both the 
 accessors and the 
 accessor must exist and be accessible.
In the following example, the property 
 is hidden by the property 
, even in contexts where only the 
 accessor is called. In contrast, the property 
 is not accessible to class 
, so the accessible property 
 is used instead.
An accessor that is used to implement an interface may not have an 
. If only one accessor is used to implement an interface, the other accessor may be declared with an 
:
Virtual, sealed, override, and abstract property accessors
A 
 property declaration specifies that the accessors of the property are virtual. The 
 modifier applies to both accessors of a read-write property—it is not possible for only one accessor of a read-write property to be virtual.
An 
 property declaration specifies that the accessors of the property are virtual, but does not provide an actual implementation of the accessors. Instead, non-abstract derived classes are required to provide their own implementation for the accessors by overriding the property. Because an accessor for an abstract property declaration provides no actual implementation, its 
 simply consists of a semicolon.
A property declaration that includes both the 
 and 
 modifiers specifies that the property is abstract and overrides a base property. The accessors of such a property are also abstract.
Abstract property declarations are only permitted in abstract classes (
).The accessors of an inherited virtual property can be overridden in a derived class by including a property declaration that specifies an 
 directive. This is known as an 
. An overriding property declaration does not declare a new property. Instead, it simply specializes the implementations of the accessors of an existing virtual property.
An overriding property declaration must specify the exact same accessibility modifiers, type, and name as the inherited property. If the inherited property has only a single accessor (i.e., if the inherited property is read-only or write-only), the overriding property must include only that accessor. If the inherited property includes both accessors (i.e., if the inherited property is read-write), the overriding property can include either a single accessor or both accessors.
An overriding property declaration may include the 
 modifier. Use of this modifier prevents a derived class from further overriding the property. The accessors of a sealed property are also sealed.
Except for differences in declaration and invocation syntax, virtual, sealed, override, and abstract accessors behave exactly like virtual, sealed, override and abstract methods. Specifically, the rules described in 
, 
, 
, and 
 apply as if accessors were methods of a corresponding form:
A 
 accessor corresponds to a parameterless method with a return value of the property type and the same modifiers as the containing property.
A 
 accessor corresponds to a method with a single value parameter of the property type, a 
 return type, and the same modifiers as the containing property.
In the example
 is a virtual read-only property, 
 is a virtual read-write property, and 
 is an abstract read-write property. Because 
 is abstract, the containing class 
 must also be declared abstract.
A class that derives from 
 is show below:
Here, the declarations of 
, 
, and 
 are overriding property declarations. Each property declaration exactly matches the accessibility modifiers, type, and name of the corresponding inherited property. The 
 accessor of 
 and the 
 accessor of 
 use the 
 keyword to access the inherited accessors. The declaration of 
 overrides both abstract accessors—thus, there are no outstanding abstract function members in 
, and 
 is permitted to be a non-abstract class.
When a property is declared as an 
, any overridden accessors must be accessible to the overriding code. In addition, the declared accessibility of both the property or indexer itself, and of the accessors, must match that of the overridden member and accessors. For example:
Events
An 
 is a member that enables an object or class to provide notifications. Clients can attach executable code for events by supplying 
.
Events are declared using 
s:
An 
 may include a set of 
 (
) and a valid combination of the four access modifiers (
), the 
 (
),  
 (
), 
 (
), 
 (
), 
 (
), 
 (
), and 
 (
) modifiers.
Event declarations are subject to the same rules as method declarations (
) with regard to valid combinations of modifiers.
The 
 of an event declaration must be a 
 (
), and that 
 must be at least as accessible as the event itself (
).
An event declaration may include 
. However, if it does not, for non-extern, non-abstract events, the compiler supplies them automatically (
); for extern events, the accessors are provided externally.
An event declaration that omits 
 defines one or more events—one for each of the 
s. The attributes and modifiers apply to all of the members declared by such an 
.
It is a compile-time error for an 
 to include both the 
 modifier and brace-delimited 
.
When an event declaration includes an 
 modifier, the event is said to be an 
. Because an external event declaration provides no actual implementation, it is an error for it to include both the 
 modifier and 
.
It is a compile-time error for a 
 of an event declaration with an 
 or 
 modifier to include a 
.
An event can be used as the left-hand operand of the 
 and 
 operators (
). These operators are used, respectively, to attach event handlers to or to remove event handlers from an event, and the access modifiers of the event control the contexts in which such operations are permitted.
Since 
 and 
 are the only operations that are permitted on an event outside the type that declares the event, external code can add and remove handlers for an event, but cannot in any other way obtain or modify the underlying list of event handlers.
In an operation of the form 
 or 
, when 
 is an event and the reference takes place outside the type that contains the declaration of 
, the result of the operation has type 
 (as opposed to having the type of 
, with the value of 
 after the assignment). This rule prohibits external code from indirectly examining the underlying delegate of an event.
The following example shows how event handlers are attached to instances of the 
 class:
Here, the 
 instance constructor creates two 
 instances and attaches event handlers to the 
 events.
Field-like events
Within the program text of the class or struct that contains the declaration of an event, certain events can be used like fields. To be used in this way, an event must not be 
 or 
, and must not explicitly include 
. Such an event can be used in any context that permits a field. The field contains a delegate (
) which refers to the list of event handlers that have been added to the event. If no event handlers have been added, the field contains 
.
In the example
 is used as a field within the 
 class. As the example demonstrates, the field can be examined, modified, and used in delegate invocation expressions. The 
 method in the 
 class ""raises"" the 
 event. The notion of raising an event is precisely equivalent to invoking the delegate represented by the event—thus, there are no special language constructs for raising events. Note that the delegate invocation is preceded by a check that ensures the delegate is non-null.
Outside the declaration of the 
 class, the 
 member can only be used on the left-hand side of the 
 and 
 operators, as in
which appends a delegate to the invocation list of the 
 event, and
which removes a delegate from the invocation list of the 
 event.
When compiling a field-like event, the compiler automatically creates storage to hold the delegate, and creates accessors for the event that add or remove event handlers to the delegate field. The addition and removal operations are thread safe, and may (but are not required to) be done while holding the lock (
) on the containing object for an instance event, or the type object (
) for a static event.
Thus, an instance event declaration of the form:
will be compiled to something equivalent to:
Within the class 
, references to 
 on
+=
-=
Ev
Ev
Ev`"" is arbitrary; the hidden field could have any name or no name at all.
Event accessors
Event declarations typically omit 
, as in the 
 example above. One situation for doing so involves the case in which the storage cost of one field per event is not acceptable. In such cases, a class can include 
 and use a private mechanism for storing the list of event handlers.
The 
 of an event specify the executable statements associated with adding and removing event handlers.
The accessor declarations consist of an 
 and a 
. Each accessor declaration consists of the token 
 or 
 followed by a 
. The 
 associated with an 
 specifies the statements to execute when an event handler is added, and the 
 associated with a 
 specifies the statements to execute when an event handler is removed.
Each 
 and 
 corresponds to a method with a single value parameter of the event type and a 
 return type. The implicit parameter of an event accessor is named 
. When an event is used in an event assignment, the appropriate event accessor is used. Specifically, if the assignment operator is 
 then the add accessor is used, and if the assignment operator is 
 then the remove accessor is used. In either case, the right-hand operand of the assignment operator is used as the argument to the event accessor. The block of an 
 or a 
 must conform to the rules for 
 methods described in 
. In particular, 
 statements in such a block are not permitted to specify an expression.
Since an event accessor implicitly has a parameter named 
, it is a compile-time error for a local variable or constant declared in an event accessor to have that name.
In the example
the 
 class implements an internal storage mechanism for events. The 
 method associates a delegate value with a key, the 
 method returns the delegate currently associated with a key, and the 
 method removes a delegate as an event handler for the specified event. Presumably, the underlying storage mechanism is designed such that there is no cost for associating a 
 delegate value with a key, and thus unhandled events consume no storage.
Static and instance events
When an event declaration includes a 
 modifier, the event is said to be a 
. When no 
 modifier is present, the event is said to be an 
.
A static event is not associated with a specific instance, and it is a compile-time error to refer to 
 in the accessors of a static event.
An instance event is associated with a given instance of a class, and this instance can be accessed as 
 (
) in the accessors of that event.
When an event is referenced in a 
 (
) of the form 
, if 
 is a static event, 
 must denote a type containing 
, and if 
 is an instance event, E must denote an instance of a type containing 
.
The differences between static and instance members are discussed further in 
.
Virtual, sealed, override, and abstract event accessors
A 
 event declaration specifies that the accessors of that event are virtual. The 
 modifier applies to both accessors of an event.
An 
 event declaration specifies that the accessors of the event are virtual, but does not provide an actual implementation of the accessors. Instead, non-abstract derived classes are required to provide their own implementation for the accessors by overriding the event. Because an abstract event declaration provides no actual implementation, it cannot provide brace-delimited 
.
An event declaration that includes both the 
 and 
 modifiers specifies that the event is abstract and overrides a base event. The accessors of such an event are also abstract.
Abstract event declarations are only permitted in abstract classes (
).
The accessors of an inherited virtual event can be overridden in a derived class by including an event declaration that specifies an 
 modifier. This is known as an 
. An overriding event declaration does not declare a new event. Instead, it simply specializes the implementations of the accessors of an existing virtual event.
An overriding event declaration must specify the exact same accessibility modifiers, type, and name as the overridden event.
An overriding event declaration may include the 
 modifier. Use of this modifier prevents a derived class from further overriding the event. The accessors of a sealed event are also sealed.
It is a compile-time error for an overriding event declaration to include a 
 modifier.
Except for differences in declaration and invocation syntax, virtual, sealed, override, and abstract accessors behave exactly like virtual, sealed, override and abstract methods. Specifically, the rules described in 
, 
, 
, and 
 apply as if accessors were methods of a corresponding form. Each accessor corresponds to a method with a single value parameter of the event type, a 
 return type, and the same modifiers as the containing event.
Indexers
An 
 is a member that enables an object to be indexed in the same way as an array. Indexers are declared using 
s:
An 
 may include a set of 
 (
) and a valid combination of the four access modifiers (
), the 
 (
), 
 (
), 
 (
), 
 (
), 
 (
), and 
 (
) modifiers.
Indexer declarations are subject to the same rules as method declarations (
) with regard to valid combinations of modifiers, with the one exception being that the static modifier is not permitted on an indexer declaration.
The modifiers 
, 
, and 
 are mutually exclusive except in one case. The 
 and 
 modifiers may be used together so that an abstract indexer can override a virtual one.
The 
 of an indexer declaration specifies the element type of the indexer introduced by the declaration. Unless the indexer is an explicit interface member implementation, the 
 is followed by the keyword 
. For an explicit interface member implementation, the 
 is followed by an 
, a ""
"", and the keyword 
. Unlike other members, indexers do not have user-defined names.
The 
 specifies the parameters of the indexer. The formal parameter list of an indexer corresponds to that of a method (
), except that at least one parameter must be specified, and that the 
 and 
 parameter modifiers are not permitted.
The 
 of an indexer and each of the types referenced in the 
 must be at least as accessible as the indexer itself (
).
An 
 may either consist of an 
 or an 
. In an accessor body, 
, which must be enclosed in ""
"" and ""
"" tokens, declare the accessors (
) of the property. The accessors specify the executable statements associated with reading and writing the property.
An expression body consisting of ""
"" followed by an expression 
 and a semicolon is exactly equivalent to the statement body 
, and can therefore only be used to specify getter-only indexers where the result of the getter is given by a single expression.
Even though the syntax for accessing an indexer element is the same as that for an array element, an indexer element is not classified as a variable. Thus, it is not possible to pass an indexer element as a 
 or 
 argument.
The formal parameter list of an indexer defines the signature (
) of the indexer. Specifically, the signature of an indexer consists of the number and types of its formal parameters. The element type and names of the formal parameters are not part of an indexer's signature.
The signature of an indexer must differ from the signatures of all other indexers declared in the same class.
Indexers and properties are very similar in concept, but differ in the following ways:
A property is identified by its name, whereas an indexer is identified by its signature.
A property is accessed through a 
 (
) or a 
 (
), whereas an indexer element is accessed through an 
 (
).
A property can be a 
 member, whereas an indexer is always an instance member.
A 
 accessor of a property corresponds to a method with no parameters, whereas a 
 accessor of an indexer corresponds to a method with the same formal parameter list as the indexer.
A 
 accessor of a property corresponds to a method with a single parameter named 
, whereas a 
 accessor of an indexer corresponds to a method with the same formal parameter list as the indexer, plus an additional parameter named 
.
It is a compile-time error for an indexer accessor to declare a local variable with the same name as an indexer parameter.
In an overriding property declaration, the inherited property is accessed using the syntax 
, where 
 is the property name. In an overriding indexer declaration, the inherited indexer is accessed using the syntax 
, where 
 is a comma separated list of expressions.
There is no concept of an ""automatically implemented indexer"". It is an error to have a non-abstract, non-external indexer with semicolon accessors.
Aside from these differences, all rules defined in 
 and 
 apply to indexer accessors as well as to property accessors.
When an indexer declaration includes an 
 modifier, the indexer is said to be an 
. Because an external indexer declaration provides no actual implementation, each of its 
 consists of a semicolon.
The example below declares a 
 class that implements an indexer for accessing the individual bits in the bit array.
An instance of the 
 class consumes substantially less memory than a corresponding 
 (since each value of the former occupies only one bit instead of the latter's one byte), but it permits the same operations as a 
.
The following 
 class uses a 
 and the classical ""sieve"" algorithm to compute the number of primes between 1 and a given maximum:
Note that the syntax for accessing elements of the 
 is precisely the same as for a 
.
The following example shows a 26 * 10 grid class that has an indexer with two parameters. The first parameter is required to be an upper- or lowercase letter in the range A-Z, and the second is required to be an integer in the range 0-9.
Indexer overloading
The indexer overload resolution rules are described in 
.
Operators
An 
 is a member that defines the meaning of an expression operator that can be applied to instances of the class. Operators are declared using 
s:
There are three categories of overloadable operators: Unary operators (
), binary operators (
), and conversion operators (
).
The 
 is either a semicolon, a 
 or an 
. A statement body consists of a 
, which specifies the statements to execute when the operator is invoked. The 
 must conform to the rules for value-returning methods described in 
. An expression body consists of 
 followed by an expression and a semicolon, and denotes a single expression to perform when the operator is invoked.
For 
 operators, the 
 consists simply of a semicolon. For all other operators, the 
 is either a block body or an expression body.
The following rules apply to all operator declarations:
An operator declaration must include both a 
 and a 
 modifier.
The parameter(s) of an operator must be value parameters (
). It is a compile-time error for an operator declaration to specify 
 or 
 parameters.
The signature of an operator (
, 
, 
) must differ from the signatures of all other operators declared in the same class.
All types referenced in an operator declaration must be at least as accessible as the operator itself (
).
It is an error for the same modifier to appear multiple times in an operator declaration.
Each operator category imposes additional restrictions, as described in the following sections.
Like other members, operators declared in a base class are inherited by derived classes. Because operator declarations always require the class or struct in which the operator is declared to participate in the signature of the operator, it is not possible for an operator declared in a derived class to hide an operator declared in a base class. Thus, the 
 modifier is never required, and therefore never permitted, in an operator declaration.
Additional information on unary and binary operators can be found in 
.
Additional information on conversion operators can be found in 
.
Unary operators
The following rules apply to unary operator declarations, where 
 denotes the instance type of the class or struct that contains the operator declaration:
A unary 
, 
, 
, or 
 operator must take a single parameter of type 
 or 
 and can return any type.
A unary 
 or 
 operator must take a single parameter of type 
 or 
 and must return that same type or a type derived from it.
A unary 
 or 
 operator must take a single parameter of type 
 or 
 and must return type 
.
The signature of a unary operator consists of the operator token (
, 
, 
, 
, 
, 
, 
, or 
) and the type of the single formal parameter. The return type is not part of a unary operator's signature, nor is the name of the formal parameter.
The 
 and 
 unary operators require pair-wise declaration. A compile-time error occurs if a class declares one of these operators without also declaring the other. The 
 and 
 operators are described further in 
 and 
.
The following example shows an implementation and subsequent usage of 
 for an integer vector class:
Note how the operator method returns the value produced by adding 1 to the operand, just like the  postfix increment and decrement operators (
), and the prefix increment and decrement operators (
). Unlike in C++, this method need not modify the value of its operand directly. In fact, modifying the operand value would violate the standard semantics of the postfix increment operator.
Binary operators
The following rules apply to binary operator declarations, where 
 denotes the instance type of the class or struct that contains the operator declaration:
A binary non-shift operator must take two parameters, at least one of which must have type 
 or 
, and can return any type.
A binary 
 or 
 operator must take two parameters, the first of which must have type 
 or 
 and the second of which must have type 
 or 
, and can return any type.
The signature of a binary operator consists of the operator token (
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
) and the types of the two formal parameters. The return type and the names of the formal parameters are not part of a binary operator's signature.
Certain binary operators require pair-wise declaration. For every declaration of either operator of a pair, there must be a matching declaration of the other operator of the pair. Two operator declarations match when they have the same return type and the same type for each parameter. The following operators require pair-wise declaration:
 and 
 and 
 and 
Conversion operators
A conversion operator declaration introduces a 
 (
) which augments the pre-defined implicit and explicit conversions.
A conversion operator declaration that includes the 
 keyword introduces a user-defined implicit conversion. Implicit conversions can occur in a variety of situations, including function member invocations, cast expressions, and assignments. This is described further in 
.
A conversion operator declaration that includes the 
 keyword introduces a user-defined explicit conversion. Explicit conversions can occur in cast expressions, and are described further in 
.
A conversion operator converts from a source type, indicated by the parameter type of the conversion operator, to a target type, indicated by the return type of the conversion operator.
For a given source type 
 and target type 
, if 
 or 
 are nullable types, let 
 and 
 refer to their underlying types, otherwise 
 and 
 are equal to 
 and 
 respectively. A class or struct is permitted to declare a conversion from a source type 
 to a target type 
 only if all of the following are true:
 and 
 are different types.
Either 
 or 
 is the class or struct type in which the operator declaration takes place.
Neither 
 nor 
 is an 
.
Excluding user-defined conversions, a conversion does not exist from 
 to 
 or from 
 to 
.
For the purposes of these rules, any type parameters associated with 
 or 
 are considered to be unique types that have no inheritance relationship with other types, and any constraints on those type parameters are ignored.
In the example
the first two operator declarations are permitted because, for the purposes of 
.3, 
 and 
 and 
 respectively are considered unique types with no relationship. However, the third operator is an error because 
 is the base class of 
.
From the second rule it follows that a conversion operator must convert either to or from the class or struct type in which the operator is declared. For example, it is possible for a class or struct type 
 to define a conversion from 
 to 
 and from 
 to 
, but not from 
 to 
.
It is not possible to directly redefine a pre-defined conversion. Thus, conversion operators are not allowed to convert from or to 
 because implicit and explicit conversions already exist between 
 and all other types. Likewise, neither the source nor the target types of a conversion can be a base type of the other, since a conversion would then already exist.
However, it is possible to declare operators on generic types that, for particular type arguments, specify conversions that already exist as pre-defined conversions. In the example
when type 
 is specified as a type argument for 
, the second operator declares a conversion that already exists (an implicit, and therefore also an explicit, conversion exists from any type to type 
).
In cases where a pre-defined conversion exists between two types, any user-defined conversions between those types are ignored. Specifically:
If a pre-defined implicit conversion (
) exists from type 
 to type 
, all user-defined conversions (implicit or explicit) from 
 to 
 are ignored.
If a pre-defined explicit conversion (
) exists from type 
 to type 
, any user-defined explicit conversions from 
 to 
 are ignored. Furthermore:
If 
 is an interface type, user-defined implicit conversions from 
 to 
 are ignored.
Otherwise, user-defined implicit conversions from 
 to 
 are still considered.
For all types but 
, the operators declared by the 
 type above do not conflict with pre-defined conversions. For example:
However, for type 
, pre-defined conversions hide the user-defined conversions in all cases but one:
User-defined conversions are not allowed to convert from or to 
s. In particular, this restriction ensures that no user-defined transformations occur when converting to an 
, and that a conversion to an 
 succeeds only if the object being converted actually implements the specified 
.
The signature of a conversion operator consists of the source type and the target type. (Note that this is the only form of member for which the return type participates in the signature.) The 
 or 
 classification of a conversion operator is not part of the operator's signature. Thus, a class or struct cannot declare both an 
 and an 
 conversion operator with the same source and target types.
In general, user-defined implicit conversions should be designed to never throw exceptions and never lose information. If a user-defined conversion can give rise to exceptions (for example, because the source argument is out of range) or loss of information (such as discarding high-order bits), then that conversion should be defined as an explicit conversion.
In the example
the conversion from 
 to 
 is implicit because it never throws exceptions or loses information, but the conversion from 
 to 
 is explicit since 
 can only represent a subset of the possible values of a 
.
Instance constructors
An 
 is a member that implements the actions required to initialize an instance of a class. Instance constructors are declared using 
s:
A 
 may include a set of 
 (
), a valid combination of the four access modifiers (
), and an 
 (
) modifier. A constructor declaration is not permitted to include the same modifier multiple times.
The 
 of a 
 must name the class in which the instance constructor is declared. If any other name is specified, a compile-time error occurs.
The optional 
 of an instance constructor is subject to the same rules as the 
 of a method (
). The formal parameter list defines the signature (
) of an instance constructor and governs the process whereby overload resolution (
) selects a particular instance constructor in an invocation.
Each of the types referenced in the 
 of an instance constructor must be at least as accessible as the constructor itself (
).
The optional 
 specifies another instance constructor to invoke before executing the statements given in the 
 of this instance constructor. This is described further in 
.
When a constructor declaration includes an 
 modifier, the constructor is said to be an 
. Because an external constructor declaration provides no actual implementation, its 
 consists of a semicolon. For all other constructors, the 
 consists of a 
 which specifies the statements to initialize a new instance of the class. This corresponds exactly to the 
 of an instance method with a 
 return type (
).
Instance constructors are not inherited. Thus, a class has no instance constructors other than those actually declared in the class. If a class contains no instance constructor declarations, a default instance constructor is automatically provided (
).
Instance constructors are invoked by 
s (
) and through 
s.
Constructor initializers
All instance constructors (except those for class 
) implicitly include an invocation of another instance constructor immediately before the 
. The constructor to implicitly invoke is determined by the 
:
An instance constructor initializer of the form 
 or 
 causes an instance constructor from the direct base class to be invoked. That constructor is selected using 
 if present and the overload resolution rules of 
. The set of candidate instance constructors consists of all accessible instance constructors contained in the direct base class, or the default constructor (
), if no instance constructors are declared in the direct base class. If this set is empty, or if a single best instance constructor cannot be identified, a compile-time error occurs.
An instance constructor initializer of the form 
 or 
 causes an instance constructor from the class itself to be invoked. The constructor is selected using 
 if present and the overload resolution rules of 
. The set of candidate instance constructors consists of all accessible instance constructors declared in the class itself. If this set is empty, or if a single best instance constructor cannot be identified, a compile-time error occurs. If an instance constructor declaration includes a constructor initializer that invokes the constructor itself, a compile-time error occurs.
If an instance constructor has no constructor initializer, a constructor initializer of the form 
 is implicitly provided. Thus, an instance constructor declaration of the form
is exactly equivalent to
The scope of the parameters given by the 
 of an instance constructor declaration includes the constructor initializer of that declaration. Thus, a constructor initializer is permitted to access the parameters of the constructor. For example:
An instance constructor initializer cannot access the instance being created. Therefore it is a compile-time error to reference 
 in an argument expression of the constructor initializer, as is it a compile-time error for an argument expression to reference any instance member through a 
.
Instance variable initializers
When an instance constructor has no constructor initializer, or it has a constructor initializer of the form 
, that constructor implicitly performs the initializations specified by the 
s of the instance fields declared in its class. This corresponds to a sequence of assignments that are executed immediately upon entry to the constructor and before the implicit invocation of the direct base class constructor. The variable initializers are executed in the textual order in which they appear in the class declaration.
Constructor execution
Variable initializers are transformed into assignment statements, and these assignment statements are executed before the invocation of the base class instance constructor. This ordering ensures that all instance fields are initialized by their variable initializers before any statements that have access to that instance are executed.
Given the example
when 
 is used to create an instance of 
, the following output is produced:
The value of 
 is 1 because the variable initializer is executed before the base class instance constructor is invoked. However, the value of 
 is 0 (the default value of an 
) because the assignment to 
 is not executed until after the base class constructor returns.
It is useful to think of instance variable initializers and constructor initializers as statements that are automatically inserted before the 
. The example
contains several variable initializers; it also contains constructor initializers of both forms (
 and 
). The example corresponds to the code shown below, where each comment indicates an automatically inserted statement (the syntax used for the automatically inserted constructor invocations isn't valid, but merely serves to illustrate the mechanism).
Default constructors
If a class contains no instance constructor declarations, a default instance constructor is automatically provided. That default constructor simply invokes the parameterless constructor of the direct base class. If the class is abstract then the declared accessibility for the default constructor is protected. Otherwise, the declared accessibility for the default constructor is public. Thus, the default constructor is always of the form
or
where 
 is the name of the class. If overload resolution is unable to determine a unique best candidate for the base class constructor initializer then a compile-time error occurs.
In the example
a default constructor is provided because the class contains no instance constructor declarations. Thus, the example is precisely equivalent to
Private constructors
When a class 
 declares only private instance constructors, it is not possible for classes outside the program text of 
 to derive from 
 or to directly create instances of 
. Thus, if a class contains only static members and isn't intended to be instantiated, adding an empty private instance constructor will prevent instantiation. For example:
The 
 class groups related methods and constants, but is not intended to be instantiated. Therefore it declares a single empty private instance constructor. At least one instance constructor must be declared to suppress the automatic generation of a default constructor.
Optional instance constructor parameters
The 
 form of constructor initializer is commonly used in conjunction with overloading to implement optional instance constructor parameters. In the example
the first two instance constructors merely provide the default values for the missing arguments. Both use a 
 constructor initializer to invoke the third instance constructor, which actually does the work of initializing the new instance. The effect is that of optional constructor parameters:
Static constructors
A 
 is a member that implements the actions required to initialize a closed class type. Static constructors are declared using 
s:
A 
 may include a set of 
 (
) and an 
 modifier (
).
The 
 of a 
 must name the class in which the static constructor is declared. If any other name is specified, a compile-time error occurs.
When a static constructor declaration includes an 
 modifier, the static constructor is said to be an 
. Because an external static constructor declaration provides no actual implementation, its 
 consists of a semicolon. For all other static constructor declarations, the 
 consists of a 
 which specifies the statements to execute in order to initialize the class. This corresponds exactly to the 
 of a static method with a 
 return type (
).
Static constructors are not inherited, and cannot be called directly.
The static constructor for a closed class type executes at most once in a given application domain. The execution of a static constructor is triggered by the first of the following events to occur within an application domain:
An instance of the class type is created.
Any of the static members of the class type are referenced.
If a class contains the 
 method (
) in which execution begins, the static constructor for that class executes before the 
 method is called.
To initialize a new closed class type, first a new set of static fields (
) for that particular closed type is created. Each of the static fields is initialized to its default value (
). Next, the static field initializers (
) are executed for those static fields. Finally, the static constructor is executed.
The example
must produce the output:
because the execution of 
's static constructor is triggered by the call to 
, and the execution of 
's static constructor is triggered by the call to 
.
It is possible to construct circular dependencies that allow static fields with variable initializers to be observed in their default value state.
The example
produces the output
To execute the 
 method, the system first runs the initializer for 
, prior to class 
's static constructor. 
's initializer causes 
's static constructor to be run because the value of 
 is referenced. The static constructor of 
 in turn proceeds to compute the value of 
, and in doing so fetches the default value of 
, which is zero. 
 is thus initialized to 1. The process of running 
's static field initializers and static constructor then completes, returning to the calculation of the initial value of 
, the result of which becomes 2.
Because the static constructor is executed exactly once for each closed constructed class type, it is a convenient place to enforce run-time checks on the type parameter that cannot be checked at compile-time via constraints (
). For example, the following type uses a static constructor to enforce that the type argument is an enum:
Destructors
A 
 is a member that implements the actions required to destruct an instance of a class. A destructor is declared using a 
:
A 
 may include a set of 
 (
).
The 
 of a 
 must name the class in which the destructor is declared. If any other name is specified, a compile-time error occurs.
When a destructor declaration includes an 
 modifier, the destructor is said to be an 
. Because an external destructor declaration provides no actual implementation, its 
 consists of a semicolon. For all other destructors, the 
 consists of a 
 which specifies the statements to execute in order to destruct an instance of the class. A 
 corresponds exactly to the 
 of an instance method with a 
 return type (
).
Destructors are not inherited. Thus, a class has no destructors other than the one which may be declared in that class.
Since a destructor is required to have no parameters, it cannot be overloaded, so a class can have, at most, one destructor.
Destructors are invoked automatically, and cannot be invoked explicitly. An instance becomes eligible for destruction when it is no longer possible for any code to use that instance. Execution of the destructor for the instance may occur at any time after the instance becomes eligible for destruction. When an instance is destructed, the destructors in that instance's inheritance chain are called, in order, from most derived to least derived. A destructor may be executed on any thread. For further discussion of the rules that govern when and how a destructor is executed, see 
.
The output of the example
is
since destructors in an inheritance chain are called in order, from most derived to least derived.
Destructors are implemented by overriding the virtual method 
 on 
. C# programs are not permitted to override this method or call it (or overrides of it) directly. For instance, the program
contains two errors.
The compiler behaves as if this method, and overrides of it, do not exist at all. Thus, this program:
is valid, and the method shown hides 
's 
 method.
For a discussion of the behavior when an exception is thrown from a destructor, see 
.
Iterators
A function member (
) implemented using an iterator block (
) is called an 
.
An iterator block may be used as the body of a function member as long as the return type of the corresponding function member is one of the enumerator interfaces (
) or one of the enumerable interfaces (
). It can occur as a 
, 
 or 
, whereas events, instance constructors, static constructors and destructors cannot be implemented as iterators.
When a function member is implemented using an iterator block, it is a compile-time error for the formal parameter list of the function member to specify any 
 or 
 parameters.
Enumerator interfaces
The 
 are the non-generic interface 
 and all instantiations of the generic interface 
. For the sake of brevity, in this chapter these interfaces are referenced as 
 and 
, respectively.
Enumerable interfaces
The 
 are the non-generic interface 
 and all instantiations of the generic interface 
. For the sake of brevity, in this chapter these interfaces are referenced as 
 and 
, respectively.
Yield type
An iterator produces a sequence of values, all of the same type. This type is called the 
 of the iterator.
The yield type of an iterator that returns 
 or 
 is 
.
The yield type of an iterator that returns 
 or 
 is 
.
Enumerator objects
When a function member returning an enumerator interface type is implemented using an iterator block, invoking the function member does not immediately execute the code in the iterator block. Instead, an 
 is created and returned. This object encapsulates the code specified in the iterator block, and execution of the code in the iterator block occurs when the enumerator object's 
 method is invoked. An enumerator object has the following characteristics:
It implements 
 and 
, where 
 is the yield type of the iterator.
It implements 
.
It is initialized with a copy of the argument values (if any) and instance value passed to the function member.
It has four potential states, 
, 
, 
, and 
, and is initially in the 
 state.
An enumerator object is typically an instance of a compiler-generated enumerator class that encapsulates the code in the iterator block and implements the enumerator interfaces, but other methods of implementation are possible. If an enumerator class is generated by the compiler, that class will be nested, directly or indirectly, in the class containing the function member, it will have private accessibility, and it will have a name reserved for compiler use (
).
An enumerator object may implement more interfaces than those specified above.
The following sections describe the exact behavior of the 
, 
, and 
 members of the 
 and 
 interface implementations provided by an enumerator object.
Note that enumerator objects do not support the 
 method. Invoking this method causes a 
 to be thrown.
The MoveNext method
The 
 method of an enumerator object encapsulates the code of an iterator block. Invoking the 
 method executes code in the iterator block and sets the 
 property of the enumerator object as appropriate. The precise action performed by 
 depends on the state of the enumerator object when 
 is invoked:
If the state of the enumerator object is 
, invoking 
:
Changes the state to 
.
Initializes the parameters (including 
) of the iterator block to the argument values and instance value saved when the enumerator object was initialized.
Executes the iterator block from the beginning until execution is interrupted (as described below).
If the state of the enumerator object is 
, the result of invoking 
 is unspecified.
If the state of the enumerator object is 
, invoking 
:
Changes the state to 
.
Restores the values of all local variables and parameters (including this) to the values saved when execution of the iterator block was last suspended. Note that the contents of any objects referenced by these variables may have changed since the previous call to MoveNext.
Resumes execution of the iterator block immediately following the 
 statement that caused the suspension of execution and continues until execution is interrupted (as described below).
If the state of the enumerator object is 
, invoking 
 returns 
.
When 
 executes the iterator block, execution can be interrupted in four ways: By a 
 statement, by a 
 statement, by encountering the end of the iterator block, and by an exception being thrown and propagated out of the iterator block.
When a 
 statement is encountered (
):
The expression given in the statement is evaluated, implicitly converted to the yield type, and assigned to the 
 property of the enumerator object.
Execution of the iterator body is suspended. The values of all local variables and parameters (including 
) are saved, as is the location of this 
 statement. If the 
 statement is within one or more 
 blocks, the associated 
 blocks are not executed at this time.
The state of the enumerator object is changed to 
.
The 
 method returns 
 to its caller, indicating that the iteration successfully advanced to the next value.
When a 
 statement is encountered (
):
If the 
 statement is within one or more 
 blocks, the associated 
 blocks are executed.
The state of the enumerator object is changed to 
.
The 
 method returns 
 to its caller, indicating that the iteration is complete.
When the end of the iterator body is encountered:
The state of the enumerator object is changed to 
.
The 
 method returns 
 to its caller, indicating that the iteration is complete.
When an exception is thrown and propagated out of the iterator block:
Appropriate 
 blocks in the iterator body will have been executed by the exception propagation.
The state of the enumerator object is changed to 
.
The exception propagation continues to the caller of the 
 method.
The Current property
An enumerator object's 
 property is affected by 
 statements in the iterator block.
When an enumerator object is in the 
 state, the value of 
 is the value set by the previous call to 
. When an enumerator object is in the 
, 
, or 
 states, the result of accessing 
 is unspecified.
For an iterator with a yield type other than 
, the result of accessing 
 through the enumerator object's 
 implementation corresponds to accessing 
 through the enumerator object's 
 implementation and casting the result to 
.
The Dispose method
The 
 method is used to clean up the iteration by bringing the enumerator object to the 
 state.
If the state of the enumerator object is 
, invoking 
 changes the state to 
.
If the state of the enumerator object is 
, the result of invoking 
 is unspecified.
If the state of the enumerator object is 
, invoking 
:
Changes the state to 
.
Executes any finally blocks as if the last executed 
 statement were a 
 statement. If this causes an exception to be thrown and propagated out of the iterator body, the state of the enumerator object is set to 
 and the exception is propagated to the caller of the 
 method.
Changes the state to 
.
If the state of the enumerator object is 
, invoking 
 has no affect.
Enumerable objects
When a function member returning an enumerable interface type is implemented using an iterator block, invoking the function member does not immediately execute the code in the iterator block. Instead, an 
 is created and returned. The enumerable object's 
 method returns an enumerator object that encapsulates the code specified in the iterator block, and execution of the code in the iterator block occurs when the enumerator object's 
 method is invoked. An enumerable object has the following characteristics:
It implements 
 and 
, where 
 is the yield type of the iterator.
It is initialized with a copy of the argument values (if any) and instance value passed to the function member.
An enumerable object is typically an instance of a compiler-generated enumerable class that encapsulates the code in the iterator block and implements the enumerable interfaces, but other methods of implementation are possible. If an enumerable class is generated by the compiler, that class will be nested, directly or indirectly, in the class containing the function member, it will have private accessibility, and it will have a name reserved for compiler use (
).
An enumerable object may implement more interfaces than those specified above. In particular, an enumerable object may also implement 
 and 
, enabling it to serve as both an enumerable and an enumerator. In that type of implementation, the first time an enumerable object's 
 method is invoked, the enumerable object itself is returned. Subsequent invocations of the enumerable object's 
, if any, return a copy of the enumerable object. Thus, each returned enumerator has its own state and changes in one enumerator will not affect another.
The GetEnumerator method
An enumerable object provides an implementation of the 
 methods of the 
 and 
 interfaces. The two 
 methods share a common implementation that acquires and returns an available enumerator object. The enumerator object is initialized with the argument values and instance value saved when the enumerable object was initialized, but otherwise the enumerator object functions as described in 
.
Implementation example
This section describes a possible implementation of iterators in terms of standard C# constructs. The implementation described here is based on the same principles used by the Microsoft C# compiler, but it is by no means a mandated implementation or the only one possible.
The following 
 class implements its 
 method using an iterator. The iterator enumerates the elements of the stack in top to bottom order.
The 
 method can be translated into an instantiation of a compiler-generated enumerator class that encapsulates the code in the iterator block, as shown in the following.
In the preceding translation, the code in the iterator block is turned into a state machine and placed in the 
 method of the enumerator class. Furthermore, the local variable 
 is turned into a field in the enumerator object so it can continue to exist across invocations of 
.
The following example prints a simple multiplication table of the integers 1 through 10. The 
 method in the example returns an enumerable object and is implemented using an iterator.
The 
 method can be translated into an instantiation of a compiler-generated enumerable class that encapsulates the code in the iterator block, as shown in the following.
The enumerable class implements both the enumerable interfaces and the enumerator interfaces, enabling it to serve as both an enumerable and an enumerator. The first time the 
 method is invoked, the enumerable object itself is returned. Subsequent invocations of the enumerable object's 
, if any, return a copy of the enumerable object. Thus, each returned enumerator has its own state and changes in one enumerator will not affect another. The 
 method is used to ensure thread-safe operation.
The 
 and 
 parameters are turned into fields in the enumerable class. Because 
 is modified in the iterator block, an additional 
 field is introduced to hold the initial value given to 
 in each enumerator.
The 
 method throws an 
 if it is called when 
 is 
. This protects against use of the enumerable object as an enumerator object without first calling 
.
The following example shows a simple tree class. The 
 class implements its 
 method using an iterator. The iterator enumerates the elements of the tree in infix order.
The 
 method can be translated into an instantiation of a compiler-generated enumerator class that encapsulates the code in the iterator block, as shown in the following.
The compiler generated temporaries used in the 
 statements are lifted into the 
 and 
 fields of the enumerator object. The 
 field of the enumerator object is carefully updated so that the correct 
 method will be called correctly if an exception is thrown. Note that it is not possible to write the translated code with simple 
 statements.
Async functions
A method (
) or anonymous function (
) with the 
 modifier is called an 
. In general, the term 
 is used to describe any kind of function that has the 
 modifier.
It is a compile-time error for the formal parameter list of an async function to specify any 
 or 
 parameters.
The 
 of an async method must be either 
 or a 
. The task types are 
 and types constructed from 
. For the sake of brevity, in this chapter these types are referenced as 
 and 
, respectively. An async method returning a task type is said to be task-returning.
The exact definition of the task types is implementation defined, but from the language's point of view a task type is in one of the states incomplete, succeeded or faulted. A faulted task records a pertinent exception. A succeeded 
 records a result of type 
. Task types are awaitable, and can therefore be the operands of await expressions (
).
An async function invocation has the ability to suspend evaluation by means of await expressions (
) in its body. Evaluation may later be resumed at the point of the suspending await expression by means of a 
. The resumption delegate is of type 
, and when it is invoked, evaluation of the async function invocation will resume from the await expression where it left off. The 
 of an async function invocation is the original caller if the function invocation has never been suspended, or the most recent caller of the resumption delegate otherwise.
Evaluation of a task-returning async function
Invocation of a task-returning async function causes an instance of the returned task type to be generated. This is called the 
 of the async function. The task is initially in an incomplete state.
The async function body is then evaluated until it is either suspended (by reaching an await expression) or terminates, at which point control is returned to the caller, along with the return task.
When the body of the async function terminates, the return task is moved out of the incomplete state:
If the function body terminates as the result of reaching a return statement or the end of the body, any result value is recorded in the return task, which is put into a succeeded state.
If the function body terminates as the result of an uncaught exception (
) the exception is recorded in the return task which is put into a faulted state.
Evaluation of a void-returning async function
If the return type of the async function is 
, evaluation differs from the above in the following way: Because no task is returned, the function instead communicates completion and exceptions to the current thread's 
. The exact definition of synchronization context is implementation-dependent, but is a representation of ""where"" the current thread is running. The synchronization context is notified when evaluation of a void-returning async function commences, completes successfully, or causes an uncaught exception to be thrown.
This allows the context to keep track of how many void-returning async functions are running under it, and to decide how to propagate exceptions coming out of them.
Structs
Structs are similar to classes in that they represent data structures that can contain data members and function members. However, unlike classes, structs are value types and do not require heap allocation. A variable of a struct type directly contains the data of the struct, whereas a variable of a class type contains a reference to the data, the latter known as an object.
Structs are particularly useful for small data structures that have value semantics. Complex numbers, points in a coordinate system, or key-value pairs in a dictionary are all good examples of structs. Key to these data structures is that they have few data members, that they do not require use of inheritance or referential identity, and that they can be conveniently implemented using value semantics where assignment copies the value instead of the reference.
As described in 
, the simple types provided by C#, such as 
, 
, and 
, are in fact all struct types. Just as these predefined types are structs, it is also possible to use structs and operator overloading to implement new ""primitive"" types in the C# language. Two examples of such types are given at the end of this chapter (
).
Struct declarations
A 
 is a 
 (
) that declares a new struct:
A 
 consists of an optional set of 
 (
), followed by an optional set of 
s (
), followed by an optional 
 modifier, followed by the keyword 
 and an 
 that names the struct, followed by an optional 
 specification (
), followed by an optional 
 specification (
) ), followed by an optional 
s specification (
), followed by a 
 (
), optionally followed by a semicolon.
Struct modifiers
A 
 may optionally include a sequence of struct modifiers:
It is a compile-time error for the same modifier to appear multiple times in a struct declaration.
The modifiers of a struct declaration have the same meaning as those of a class declaration (
).
Partial modifier
The 
 modifier indicates that this 
 is a partial type declaration. Multiple partial struct declarations with the same name within an enclosing namespace or type declaration combine to form one struct declaration, following the rules specified in 
.
Struct interfaces
A struct declaration may include a 
 specification, in which case the struct is said to directly implement the given interface types.
Interface implementations are discussed further in 
.
Struct body
The 
 of a struct defines the members of the struct.
Struct members
The members of a struct consist of the members introduced by its 
s and the members inherited from the type 
.
Except for the differences noted in 
, the descriptions of class members provided in 
 through 
 apply to struct members as well.
Class and struct differences
Structs differ from classes in several important ways:
Structs are value types (
).
All struct types implicitly inherit from the class 
 (
).
Assignment to a variable of a struct type creates a copy of the value being assigned (
).
The default value of a struct is the value produced by setting all value type fields to their default value and all reference type fields to 
 (
).
Boxing and unboxing operations are used to convert between a struct type and 
 (
).
The meaning of 
 is different for structs (
).
Instance field declarations for a struct are not permitted to include variable initializers (
).
A struct is not permitted to declare a parameterless instance constructor (
).
A struct is not permitted to declare a destructor (
).
Value semantics
Structs are value types (
) and are said to have value semantics. Classes, on the other hand, are reference types (
) and are said to have reference semantics.
A variable of a struct type directly contains the data of the struct, whereas a variable of a class type contains a reference to the data, the latter known as an object. When a struct 
 contains an instance field of type 
 and 
 is a struct type, it is a compile-time error for 
 to depend on 
 or a type constructed from 
. A struct 
 
 a struct 
 if 
 contains an instance field of type 
. Given this definition, the complete set of structs upon which a struct depends is the transitive closure of the 
 relationship.  For example
is an error because 
 contains an instance field of its own type.  Another example
is an error because each of the types 
, 
, and 
 depend on each other.
With classes, it is possible for two variables to reference the same object, and thus possible for operations on one variable to affect the object referenced by the other variable. With structs, the variables each have their own copy of the data (except in the case of 
 and 
 parameter variables), and it is not possible for operations on one to affect the other. Furthermore, because structs are not reference types, it is not possible for values of a struct type to be 
.
Given the declaration
the code fragment
outputs the value 
. The assignment of 
 to 
 creates a copy of the value, and 
 is thus unaffected by the assignment to 
. Had 
 instead been declared as a class, the output would be 
 because 
 and 
 would reference the same object.
Inheritance
All struct types implicitly inherit from the class 
, which, in turn, inherits from class 
. A struct declaration may specify a list of implemented interfaces, but it is not possible for a struct declaration to specify a base class.
Struct types are never abstract and are always implicitly sealed. The 
 and 
 modifiers are therefore not permitted in a struct declaration.
Since inheritance isn't supported for structs, the declared accessibility of a struct member cannot be 
 or 
.
Function members in a struct cannot be 
 or 
, and the 
 modifier is allowed only to override methods inherited from 
.
Assignment
Assignment to a variable of a struct type creates a copy of the value being assigned. This differs from assignment to a variable of a class type, which copies the reference but not the object identified by the reference.
Similar to an assignment, when a struct is passed as a value parameter or returned as the result of a function member, a copy of the struct is created. A struct may be passed by reference to a function member using a 
 or 
 parameter.
When a property or indexer of a struct is the target of an assignment, the instance expression associated with the property or indexer access must be classified as a variable. If the instance expression is classified as a value, a compile-time error occurs. This is described in further detail in 
.
Default values
As described in 
, several kinds of variables are automatically initialized to their default value when they are created. For variables of class types and other reference types, this default value is 
. However, since structs are value types that cannot be 
, the default value of a struct is the value produced by setting all value type fields to their default value and all reference type fields to 
.
Referring to the 
 struct declared above, the example
initializes each 
 in the array to the value produced by setting the 
 and 
 fields to zero.
The default value of a struct corresponds to the value returned by the default constructor of the struct (
). Unlike a class, a struct is not permitted to declare a parameterless instance constructor. Instead, every struct implicitly has a parameterless instance constructor which always returns the value that results from setting all value type fields to their default value and all reference type fields to 
.
Structs should be designed to consider the default initialization state a valid state. In the example
the user-defined instance constructor protects against null values only where it is explicitly called. In cases where a 
 variable is subject to default value initialization, the 
 and 
 fields will be null, and the struct must be prepared to handle this state.
Boxing and unboxing
A value of a class type can be converted to type 
 or to an interface type that is implemented by the class simply by treating the reference as another type at compile-time. Likewise, a value of type 
 or a value of an interface type can be converted back to a class type without changing the reference (but of course a run-time type check is required in this case).
Since structs are not reference types, these operations are implemented differently for struct types. When a value of a struct type is converted to type 
 or to an interface type that is implemented by the struct, a boxing operation takes place. Likewise, when a value of type 
 or a value of an interface type is converted back to a struct type, an unboxing operation takes place. A key difference from the same operations on class types is that boxing and unboxing copies the struct value either into or out of the boxed instance. Thus, following a boxing or unboxing operation, changes made to the unboxed struct are not reflected in the boxed struct.
When a struct type overrides a virtual method inherited from 
 (such as 
, 
, or 
), invocation of the virtual method through an instance of the struct type does not cause boxing to occur. This is true even when the struct is used as a type parameter and the invocation occurs through an instance of the type parameter type. For example:
The output of the program is:
Although it is bad style for 
 to have side effects, the example demonstrates that no boxing occurred for the three invocations of 
.
Similarly, boxing never implicitly occurs when accessing a member on a constrained type parameter. For example, suppose an interface 
 contains a method 
 which can be used to modify a value. If 
 is used as a constraint, the implementation of the 
 method is called with a reference to the variable that 
 was called on, never a boxed copy.
The first call to 
 modifies the value in the variable 
. This is not equivalent to the second call to 
, which modifies the value in a boxed copy of 
. Thus, the output of the program is:
For further details on boxing and unboxing, see 
.
Meaning of this
Within an instance constructor or instance function member of a class, 
 is classified as a value. Thus, while 
 can be used to refer to the instance for which the function member was invoked, it is not possible to assign to 
 in a function member of a class.
Within an instance constructor of a struct, 
 corresponds to an 
 parameter of the struct type, and within an instance function member of a struct, 
 corresponds to a 
 parameter of the struct type. In both cases, 
 is classified as a variable, and it is possible to modify the entire struct for which the function member was invoked by assigning to 
 or by passing this as a 
 or 
 parameter.
Field initializers
As described in 
, the default value of a struct consists of the value that results from setting all value type fields to their default value and all reference type fields to 
. For this reason, a struct does not permit instance field declarations to include variable initializers. This restriction applies only to instance fields. Static fields of a struct are permitted to include variable initializers.
The example
is in error because the instance field declarations include variable initializers.
Constructors
Unlike a class, a struct is not permitted to declare a parameterless instance constructor. Instead, every struct implicitly has a parameterless instance constructor which always returns the value that results from setting all value type fields to their default value and all reference type fields to null (
). A struct can declare instance constructors having parameters. For example
Given the above declaration, the statements
both create a 
 with 
 and 
 initialized to zero.
A struct instance constructor is not permitted to include a constructor initializer of the form 
.
If the struct instance constructor doesn't specify a constructor initializer, the 
 variable corresponds to an 
 parameter of the struct type, and similar to an 
 parameter, 
 must be definitely assigned (
) at every location where the constructor returns. If the struct instance constructor specifies a constructor initializer, the 
 variable corresponds to a 
 parameter of the struct type, and similar to a 
 parameter, 
 is considered definitely assigned on entry to the constructor body. Consider the instance constructor implementation below:
No instance member function (including the set accessors for the properties 
 and 
) can be called until all fields of the struct being constructed have been definitely assigned. The only exception involves automatically implemented properties (
). The definite assignment rules (
) specifically exempt assignment to an auto-property of a struct type within an instance constructor of that struct type: such an assignment is considered a definite assignment of the hidden backing field of the auto-property. Thus, the following is allowed:
Destructors
A struct is not permitted to declare a destructor.
Static constructors
Static constructors for structs follow most of the same rules as for classes. The execution of a static constructor for a struct type is triggered by the first of the following events to occur within an application domain:
A static member of the struct type is referenced.
An explicitly declared constructor of the struct type is called.
The creation of default values (
) of struct types does not trigger the static constructor. (An example of this is the initial value of elements in an array.)
Struct examples
The following shows two significant examples of using 
 types to create types that can be used similarly to the predefined types of the language, but with modified semantics.
Database integer type
The 
 struct below implements an integer type that can represent the complete set of values of the 
 type, plus an additional state that indicates an unknown value. A type with these characteristics is commonly used in databases.
Database boolean type
The 
 struct below implements a three-valued logical type. The possible values of this type are 
, 
, and 
, where the 
 member indicates an unknown value. Such three-valued logical types are commonly used in databases.
Arrays
An array is a data structure that contains a number of variables which are accessed through computed indices. The variables contained in an array, also called the elements of the array, are all of the same type, and this type is called the element type of the array.
An array has a rank which determines the number of indices associated with each array element. The rank of an array is also referred to as the dimensions of the array. An array with a rank of one is called a 
. An array with a rank greater than one is called a 
. Specific sized multi-dimensional arrays are often referred to as two-dimensional arrays, three-dimensional arrays, and so on.
Each dimension of an array has an associated length which is an integral number greater than or equal to zero. The dimension lengths are not part of the type of the array, but rather are established when an instance of the array type is created at run-time. The length of a dimension determines the valid range of indices for that dimension: For a dimension of length 
, indices can range from 
 to 
 inclusive. The total number of elements in an array is the product of the lengths of each dimension in the array. If one or more of the dimensions of an array have a length of zero, the array is said to be empty.
The element type of an array can be any type, including an array type.
Array types
An array type is written as a 
 followed by one or more 
s:
A 
 is any 
 that is not itself an 
.
The rank of an array type is given by the leftmost 
 in the 
: A 
 indicates that the array is an array with a rank of one plus the number of ""
"" tokens in the 
.
The element type of an array type is the type that results from deleting the leftmost 
:
An array type of the form 
 is an array with rank 
 and a non-array element type 
.
An array type of the form 
 is an array with rank 
 and an element type 
.
In effect, the 
s are read from left to right before the final non-array element type. The type 
 is a single-dimensional array of three-dimensional arrays of two-dimensional arrays of 
.
At run-time, a value of an array type can be 
 or a reference to an instance of that array type.
The System.Array type
The type 
 is the abstract base type of all array types. An implicit reference conversion (
) exists from any array type to 
, and an explicit reference conversion (
) exists from 
 to any array type. Note that 
 is not itself an 
. Rather, it is a 
 from which all 
s are derived.
At run-time, a value of type 
 can be 
 or a reference to an instance of any array type.
Arrays and the generic IList interface
A one-dimensional array 
 implements the interface 
 (
 for short) and its base interfaces. Accordingly, there is an implicit conversion from 
 to 
 and its base interfaces. In addition, if there is an implicit reference conversion from 
 to 
 then 
 implements 
 and there is an implicit reference conversion from 
 to 
 and its base interfaces (
). If there is an explicit reference conversion from 
 to 
 then there is an explicit reference conversion from 
 to 
 and its base interfaces (
). For example:
The assignment 
 generates a compile-time error since the conversion from 
 to 
 is an explicit conversion, not implicit. The cast 
 will cause an exception to be thrown at run-time since 
 references an 
 and not a 
. However the cast 
 will not cause an exception to be thrown since 
 references a 
.
Whenever there is an implicit or explicit reference conversion from 
 to 
, there is also an explicit reference conversion from 
 and its base interfaces to 
 (
).
When an array type 
 implements 
, some of the members of the implemented interface may throw exceptions. The precise behavior of the implementation of the interface is beyond the scope of this specification.
Array creation
Array instances are created by 
s (
) or by field or local variable declarations that include an 
 (
).
When an array instance is created, the rank and length of each dimension are established and then remain constant for the entire lifetime of the instance. In other words, it is not possible to change the rank of an existing array instance, nor is it possible to resize its dimensions.
An array instance is always of an array type. The 
 type is an abstract type that cannot be instantiated.
Elements of arrays created by 
s are always initialized to their default value (
).
Array element access
Array elements are accessed using 
 expressions (
) of the form 
, where 
 is an expression of an array type and each 
 is an expression of type 
, 
, 
, 
, or can be implicitly converted to one or more of these types. The result of an array element access is a variable, namely the array element selected by the indices.
The elements of an array can be enumerated using a 
 statement (
).
Array members
Every array type inherits the members declared by the 
 type.
Array covariance
For any two 
s 
 and 
, if an implicit reference conversion (
) or explicit reference conversion (
) exists from 
 to 
, then the same reference conversion also exists from the array type 
 to the array type 
, where 
 is any given 
 (but the same for both array types). This relationship is known as 
. Array covariance in particular means that a value of an array type 
 may actually be a reference to an instance of an array type 
, provided an implicit reference conversion exists from 
 to 
.
Because of array covariance, assignments to elements of reference type arrays include a run-time check which ensures that the value being assigned to the array element is actually of a permitted type (
). For example:
The assignment to 
 in the 
 method implicitly includes a run-time check which ensures that the object referenced by 
 is either 
 or an instance that is compatible with the actual element type of 
. In 
, the first two invocations of 
 succeed, but the third invocation causes a 
 to be thrown upon executing the first assignment to 
. The exception occurs because a boxed 
 cannot be stored in a 
 array.
Array covariance specifically does not extend to arrays of 
s. For example, no conversion exists that permits an 
 to be treated as an 
.
Array initializers
Array initializers may be specified in field declarations (
), local variable declarations (
), and array creation expressions (
):
An array initializer consists of a sequence of variable initializers, enclosed by ""
"" and ""
"" tokens and separated by ""
"" tokens. Each variable initializer is an expression or, in the case of a multi-dimensional array, a nested array initializer.
The context in which an array initializer is used determines the type of the array being initialized. In an array creation expression, the array type immediately precedes the initializer, or is inferred from the expressions in the array initializer. In a field or variable declaration, the array type is the type of the field or variable being declared. When an array initializer is used in a field or variable declaration, such as:
it is simply shorthand for an equivalent array creation expression:
For a single-dimensional array, the array initializer must consist of a sequence of expressions that are assignment compatible with the element type of the array. The expressions initialize array elements in increasing order, starting with the element at index zero. The number of expressions in the array initializer determines the length of the array instance being created. For example, the array initializer above creates an 
 instance of length 5 and then initializes the instance with the following values:
For a multi-dimensional array, the array initializer must have as many levels of nesting as there are dimensions in the array. The outermost nesting level corresponds to the leftmost dimension and the innermost nesting level corresponds to the rightmost dimension. The length of each dimension of the array is determined by the number of elements at the corresponding nesting level in the array initializer. For each nested array initializer, the number of elements must be the same as the other array initializers at the same level. The example:
creates a two-dimensional array with a length of five for the leftmost dimension and a length of two for the rightmost dimension:
and then initializes the array instance with the following values:
If a dimension other than the rightmost is given with length zero, the subsequent dimensions are assumed to also have length zero. The example:
creates a two-dimensional array with a length of zero for both the leftmost and the rightmost dimension:
When an array creation expression includes both explicit dimension lengths and an array initializer, the lengths must be constant expressions and the number of elements at each nesting level must match the corresponding dimension length. Here are some examples:
Here, the initializer for 
 results in a compile-time error because the dimension length expression is not a constant, and the initializer for 
 results in a compile-time error because the length and the number of elements in the initializer do not agree.
Interfaces
An interface defines a contract. A class or struct that implements an interface must adhere to its contract. An interface may inherit from multiple base interfaces, and a class or struct may implement multiple interfaces.
Interfaces can contain methods, properties, events, and indexers. The interface itself does not provide implementations for the members that it defines. The interface merely specifies the members that must be supplied by classes or structs that implement the interface.
Interface declarations
An 
 is a 
 (
) that declares a new interface type.
An 
 consists of an optional set of 
 (
), followed by an optional set of 
s (
), followed by an optional 
 modifier, followed by the keyword 
 and an 
 that names the interface, followed by an optional 
 specification (
), followed by an optional 
 specification (
), followed by an optional 
s specification (
), followed by an 
 (
), optionally followed by a semicolon.
Interface modifiers
An 
 may optionally include a sequence of interface modifiers:
It is a compile-time error for the same modifier to appear multiple times in an interface declaration.
The 
 modifier is only permitted on interfaces defined within a class. It specifies that the interface hides an inherited member by the same name, as described in 
.
The 
, 
, 
, and 
 modifiers control the accessibility of the interface. Depending on the context in which the interface declaration occurs, only some of these modifiers may be permitted (
).
Partial modifier
The 
 modifier indicates that this 
 is a partial type declaration. Multiple partial interface declarations with the same name within an enclosing namespace or type declaration combine to form one interface declaration, following the rules specified in 
.
Variant type parameter lists
Variant type parameter lists can only occur on interface and delegate types. The difference from ordinary 
s is the optional 
 on each type parameter.
If the variance annotation is 
, the type parameter is said to be 
. If the variance annotation is 
, the type parameter is said to be 
. If there is no variance annotation, the type parameter is said to be 
.
In the example
 is covariant, 
 is contravariant and 
 is invariant.
Variance safety
The occurrence of variance annotations in the type parameter list of a type restricts the places where types can occur within the type declaration.
A type 
 is 
 if one of the following holds:
 is a contravariant type parameter
 is an array type with an output-unsafe element type
 is an interface or delegate type 
 constructed from a generic type 
 where for at least one 
 one of the following holds:
 is covariant or invariant and 
 is output-unsafe.
 is contravariant or invariant and 
 is input-safe.
A type 
 is 
 if one of the following holds:
 is a covariant type parameter
 is an array type with an input-unsafe element type
 is an interface or delegate type 
 constructed from a generic type 
 where for at least one 
 one of the following holds:
 is covariant or invariant and 
 is input-unsafe.
 is contravariant or invariant and 
 is output-unsafe.
Intuitively, an output-unsafe type is prohibited in an output position, and an input-unsafe type is prohibited in an input position.
A type is 
 if it is not output-unsafe, and 
 if it is not input-unsafe.
Variance conversion
The purpose of variance annotations is to provide for more lenient (but still type safe) conversions to interface and delegate types. To this end the definitions of implicit (
) and explicit conversions (
) make use of the notion of variance-convertibility, which is defined as follows:
A type 
 is variance-convertible to a type 
 if 
 is either an interface or a delegate type declared with the variant type parameters 
, and for each variant type parameter 
 one of the following holds:
 is covariant and an implicit reference or identity conversion exists from 
 to 
 is contravariant and an implicit reference or identity conversion exists from 
 to 
 is invariant and an identity conversion exists from 
 to 
Base interfaces
An interface can inherit from zero or more interface types, which are called the 
 of the interface. When an interface has one or more explicit base interfaces, then in the declaration of that interface, the interface identifier is followed by a colon and a comma separated list of base interface types.
For a constructed interface type, the explicit base interfaces are formed by taking the explicit base interface declarations on the generic type declaration, and substituting, for each 
 in the base interface declaration, the corresponding 
 of the constructed type.
The explicit base interfaces of an interface must be at least as accessible as the interface itself (
). For example, it is a compile-time error to specify a 
 or 
 interface in the 
 of a 
 interface.
It is a compile-time error for an interface to directly or indirectly inherit from itself.
The 
 of an interface are the explicit base interfaces and their base interfaces. In other words, the set of base interfaces is the complete transitive closure of the explicit base interfaces, their explicit base interfaces, and so on. An interface inherits all members of its base interfaces. In the example
the base interfaces of 
 are 
, 
, and 
.
In other words, the 
 interface above inherits members 
 and 
 as well as 
.
Every base interface of an interface must be output-safe (
). A class or struct that implements an interface also implicitly implements all of the interface's base interfaces.
Interface body
The 
 of an interface defines the members of the interface.
Interface members
The members of an interface are the members inherited from the base interfaces and the members declared by the interface itself.
An interface declaration may declare zero or more members. The members of an interface must be methods, properties, events, or indexers. An interface cannot contain constants, fields, operators, instance constructors, destructors, or types, nor can an interface contain static members of any kind.
All interface members implicitly have public access. It is a compile-time error for interface member declarations to include any modifiers. In particular, interfaces members cannot be declared with the modifiers 
, 
, 
, 
, 
, 
, 
, or 
.
The example
declares an interface that contains one each of the possible kinds of members: A method, a property, an event, and an indexer.
An 
 creates a new declaration space (
), and the 
s immediately contained by the 
 introduce new members into this declaration space. The following rules apply to 
s:
The name of a method must differ from the names of all properties and events declared in the same interface. In addition, the signature (
) of a method must differ from the signatures of all other methods declared in the same interface, and two methods declared in the same interface may not have signatures that differ solely by 
 and 
.
The name of a property or event must differ from the names of all other members declared in the same interface.
The signature of an indexer must differ from the signatures of all other indexers declared in the same interface.
The inherited members of an interface are specifically not part of the declaration space of the interface. Thus, an interface is allowed to declare a member with the same name or signature as an inherited member. When this occurs, the derived interface member is said to hide the base interface member. Hiding an inherited member is not considered an error, but it does cause the compiler to issue a warning. To suppress the warning, the declaration of the derived interface member must include a 
 modifier to indicate that the derived member is intended to hide the base member. This topic is discussed further in 
.
If a 
 modifier is included in a declaration that doesn't hide an inherited member, a warning is issued to that effect. This warning is suppressed by removing the 
 modifier.
Note that the members in class 
 are not, strictly speaking, members of any interface (
). However, the members in class 
 are available via member lookup in any interface type (
).
Interface methods
Interface methods are declared using 
s:
The 
, 
, 
, and 
 of an interface method declaration have the same meaning as those of a method declaration in a class (
). An interface method declaration is not permitted to specify a method body, and the declaration therefore always ends with a semicolon.
Each formal parameter type of an interface method must be input-safe (
), and the return type must be either 
 or output-safe. Furthermore, each class type constraint, interface type constraint and type parameter constraint on any type parameter of the method must be input-safe.
These rules ensure that any covariant or contravariant usage of the interface remains typesafe. For example,
is illegal because the usage of 
 as a type parameter constraint on 
 is not input-safe.
Were this restriction not in place it would be possible to violate type safety in the following manner:
This is actually a call to 
. But that call requires that 
 derive from 
, so type safety would be violated here.
Interface properties
Interface properties are declared using 
s:
The 
, 
, and 
 of an interface property declaration have the same meaning as those of a property declaration in a class (
).
The accessors of an interface property declaration correspond to the accessors of a class property declaration (
), except that the accessor body must always be a semicolon. Thus, the accessors simply indicate whether the property is read-write, read-only, or write-only.
The type of an interface property must be output-safe if there is a get accessor, and must be input-safe if there is a set accessor.
Interface events
Interface events are declared using 
s:
The 
, 
, and 
 of an interface event declaration have the same meaning as those of an event declaration in a class (
).
The type of an interface event must be input-safe.
Interface indexers
Interface indexers are declared using 
s:
The 
, 
, and 
 of an interface indexer declaration have the same meaning as those of an indexer declaration in a class (
).
The accessors of an interface indexer declaration correspond to the accessors of a class indexer declaration (
), except that the accessor body must always be a semicolon. Thus, the accessors simply indicate whether the indexer is read-write, read-only, or write-only.
All the formal parameter types of an interface indexer must be input-safe . In addition, any 
 or 
 formal parameter types must also be output-safe. Note that even 
 parameters are required to be input-safe, due to a limitiation of the underlying execution platform.
The type of an interface indexer must be output-safe if there is a get accessor, and must be input-safe if there is a set accessor.
Interface member access
Interface members are accessed through member access (
) and indexer access (
) expressions of the form 
 and 
, where 
 is an interface type, 
 is a method, property, or event of that interface type, and 
 is an indexer argument list.
For interfaces that are strictly single-inheritance (each interface in the inheritance chain has exactly zero or one direct base interface), the effects of the member lookup (
), method invocation (
), and indexer access (
) rules are exactly the same as for classes and structs: More derived members hide less derived members with the same name or signature. However, for multiple-inheritance interfaces, ambiguities can occur when two or more unrelated base interfaces declare members with the same name or signature. This section shows several examples of such situations. In all cases, explicit casts can be used to resolve the ambiguities.
In the example
the first two statements cause compile-time errors because the member lookup (
) of 
 in 
 is ambiguous. As illustrated by the example, the ambiguity is resolved by casting 
 to the appropriate base interface type. Such casts have no run-time costs—they merely consist of viewing the instance as a less derived type at compile-time.
In the example
the invocation 
 selects 
 by applying the overload resolution rules of 
. Similarly the invocation 
 selects 
. When explicit casts are inserted, there is only one candidate method, and thus no ambiguity.
In the example
the 
 member is hidden by the 
 member. The invocation 
 thus selects 
, even though 
 appears to not be hidden in the access path that leads through 
.
The intuitive rule for hiding in multiple-inheritance interfaces is simply this: If a member is hidden in any access path, it is hidden in all access paths. Because the access path from 
 to 
 to 
 hides 
, the member is also hidden in the access path from 
 to 
 to 
.
Fully qualified interface member names
An interface member is sometimes referred to by its 
. The fully qualified name of an interface member consists of the name of the interface in which the member is declared, followed by a dot, followed by the name of the member. The fully qualified name of a member references the interface in which the member is declared. For example, given the declarations
the fully qualified name of 
 is 
 and the fully qualified name of 
 is 
.
In the example above, it is not possible to refer to 
 as 
.
When an interface is part of a namespace, the fully qualified name of an interface member includes the namespace name. For example
Here, the fully qualified name of the 
 method is 
.
Interface implementations
Interfaces may be implemented by classes and structs. To indicate that a class or struct directly implements an interface, the interface identifier is included in the base class list of the class or struct. For example:
A class or struct that directly implements an interface also directly implements all of the interface's base interfaces implicitly. This is true even if the class or struct doesn't explicitly list all base interfaces in the base class list. For example:
Here, class 
 implements both 
 and 
.
When a class 
 directly implements an interface, all classes derived from C also implement the interface implicitly. The base interfaces specified in a class declaration can be constructed interface types (
). A base interface cannot be a type parameter on its own, though it can involve the type parameters that are in scope. The following code illustrates how a class can implement and extend constructed types:
The base interfaces of a generic class declaration must satisfy the uniqueness rule described in 
.
Explicit interface member implementations
For purposes of implementing interfaces, a class or struct may declare 
. An explicit interface member implementation is a method, property, event, or indexer declaration that references a fully qualified interface member name. For example
Here 
 and 
 are explicit interface member implementations.
In some cases, the name of an interface member may not be appropriate for the implementing class, in which case the interface member may be implemented using explicit interface member implementation. A class implementing a file abstraction, for example, would likely implement a 
 member function that has the effect of releasing the file resource, and implement the 
 method of the 
 interface using explicit interface member implementation:
It is not possible to access an explicit interface member implementation through its fully qualified name in a method invocation, property access, or indexer access. An explicit interface member implementation can only be accessed through an interface instance, and is in that case referenced simply by its member name.
It is a compile-time error for an explicit interface member implementation to include access modifiers, and it is a compile-time error to include the modifiers 
, 
, 
, or 
.
Explicit interface member implementations have different accessibility characteristics than other members. Because explicit interface member implementations are never accessible through their fully qualified name in a method invocation or a property access, they are in a sense private. However, since they can be accessed through an interface instance, they are in a sense also public.
Explicit interface member implementations serve two primary purposes:
Because explicit interface member implementations are not accessible through class or struct instances, they allow interface implementations to be excluded from the public interface of a class or struct. This is particularly useful when a class or struct implements an internal interface that is of no interest to a consumer of that class or struct.
Explicit interface member implementations allow disambiguation of interface members with the same signature. Without explicit interface member implementations it would be impossible for a class or struct to have different implementations of interface members with the same signature and return type, as would it be impossible for a class or struct to have any implementation at all of interface members with the same signature but with different return types.
For an explicit interface member implementation to be valid, the class or struct must name an interface in its base class list that contains a member whose fully qualified name, type, and parameter types exactly match those of the explicit interface member implementation. Thus, in the following class
the declaration of 
 results in a compile-time error because 
 is not listed in the base class list of 
 and is not a base interface of 
. Likewise, in the declarations
the declaration of 
 in 
 results in a compile-time error because 
 is not explicitly listed in the base class list of 
.
The fully qualified name of an interface member must reference the interface in which the member was declared. Thus, in the declarations
the explicit interface member implementation of 
 must be written as 
.
Uniqueness of implemented interfaces
The interfaces implemented by a generic type declaration must remain unique for all possible constructed types. Without this rule, it would be impossible to determine the correct method to call for certain constructed types. For example, suppose a generic class declaration were permitted to be written as follows:
Were this permitted, it would be impossible to determine which code to execute in the following case:
To determine if the interface list of a generic type declaration is valid, the following steps are performed:
Let 
 be the list of interfaces directly specified in a generic class, struct, or interface declaration 
.
Add to 
 any base interfaces of the interfaces already in 
.
Remove any duplicates from 
.
If any possible constructed type created from 
 would, after type arguments are substituted into 
, cause two interfaces in 
 to be identical, then the declaration of 
 is invalid. Constraint declarations are not considered when determining all possible constructed types.
In the class declaration 
 above, the interface list 
 consists of 
 and 
. The declaration is invalid because any constructed type with 
 and 
 being the same type would cause these two interfaces to be identical types.
It is possible for interfaces specified at different inheritance levels to unify:
This code is valid even though 
 implements both 
 and 
. The code
invokes the method in 
, since 
 effectively re-implements 
 (
).
Implementation of generic methods
When a generic method implicitly implements an interface method, the constraints given for each method type parameter must be equivalent in both declarations (after any interface type parameters are replaced with the appropriate type arguments), where method type parameters are identified by ordinal positions, left to right.
When a generic method explicitly implements an interface method, however, no constraints are allowed on the implementing method. Instead, the constraints are inherited from the interface method
The method 
 implicitly implements 
. In this case, 
 is not required (nor permitted) to specify the constraint 
 since 
 is an implicit constraint on all type parameters. The method 
 implicitly implements 
 because the constraints match those in the interface, after the interface type parameters are replaced with the corresponding type arguments. The constraint for method 
 is an error because sealed types (
 in this case) cannot be used as constraints. Omitting the constraint would also be an error since constraints of implicit interface method implementations are required to match. Thus, it is impossible to implicitly implement 
. This interface method can only be implemented using an explicit interface member implementation:
In this example, the explicit interface member implementation invokes a public method having strictly weaker constraints. Note that the assignment from 
 to 
 is valid since 
 inherits a constraint of 
, even though this constraint is not expressible in source code.
Interface mapping
A class or struct must provide implementations of all members of the interfaces that are listed in the base class list of the class or struct. The process of locating implementations of interface members in an implementing class or struct is known as 
.
Interface mapping for a class or struct 
 locates an implementation for each member of each interface specified in the base class list of 
. The implementation of a particular interface member 
, where 
 is the interface in which the member 
 is declared, is determined by examining each class or struct 
, starting with 
 and repeating for each successive base class of 
, until a match is located:
If 
 contains a declaration of an explicit interface member implementation that matches 
 and 
, then this member is the implementation of 
.
Otherwise, if 
 contains a declaration of a non-static public member that matches 
, then this member is the implementation of 
. If more than one member matches, it is unspecified which member is the implementation of 
. This situation can only occur if 
 is a constructed type where the two members as declared in the generic type have different signatures, but the type arguments make their signatures identical.
A compile-time error occurs if implementations cannot be located for all members of all interfaces specified in the base class list of 
. Note that the members of an interface include those members that are inherited from base interfaces.
For purposes of interface mapping, a class member 
 matches an interface member 
 when:
 and 
 are methods, and the name, type, and formal parameter lists of 
 and 
 are identical.
 and 
 are properties, the name and type of 
 and 
 are identical, and 
 has the same accessors as 
 (
 is permitted to have additional accessors if it is not an explicit interface member implementation).
 and 
 are events, and the name and type of 
 and 
 are identical.
 and 
 are indexers, the type and formal parameter lists of 
 and 
 are identical, and 
 has the same accessors as 
 (
 is permitted to have additional accessors if it is not an explicit interface member implementation).
Notable implications of the interface mapping algorithm are:
Explicit interface member implementations take precedence over other members in the same class or struct when determining the class or struct member that implements an interface member.
Neither non-public nor static members participate in interface mapping.
In the example
the 
 member of 
 becomes the implementation of 
 in 
 because explicit interface member implementations take precedence over other members.
If a class or struct implements two or more interfaces containing a member with the same name, type, and parameter types, it is possible to map each of those interface members onto a single class or struct member. For example
Here, the 
 methods of both 
 and 
 are mapped onto the 
 method in 
. It is of course also possible to have separate explicit interface member implementations for the two methods.
If a class or struct implements an interface that contains hidden members, then some members must necessarily be implemented through explicit interface member implementations. For example
An implementation of this interface would require at least one explicit interface member implementation, and would take one of the following forms
When a class implements multiple interfaces that have the same base interface, there can be only one implementation of the base interface. In the example
it is not possible to have separate implementations for the 
 named in the base class list, the 
 inherited by 
, and the 
 inherited by 
. Indeed, there is no notion of a separate identity for these interfaces. Rather, the implementations of 
 and 
 share the same implementation of 
, and 
 is simply considered to implement three interfaces, 
, 
, and 
.
The members of a base class participate in interface mapping. In the example
the method 
 in 
 is used in 
's implementation of 
.
Interface implementation inheritance
A class inherits all interface implementations provided by its base classes.
Without explicitly 
 an interface, a derived class cannot in any way alter the interface mappings it inherits from its base classes. For example, in the declarations
the 
 method in 
 hides the 
 method in 
, but it does not alter the mapping of 
 onto 
, and calls to 
 through class instances and interface instances will have the following effects
However, when an interface method is mapped onto a virtual method in a class, it is possible for derived classes to override the virtual method and alter the implementation of the interface. For example, rewriting the declarations above to
the following effects will now be observed
Since explicit interface member implementations cannot be declared virtual, it is not possible to override an explicit interface member implementation. However, it is perfectly valid for an explicit interface member implementation to call another method, and that other method can be declared virtual to allow derived classes to override it. For example
Here, classes derived from 
 can specialize the implementation of 
 by overriding the 
 method.
Interface re-implementation
A class that inherits an interface implementation is permitted to 
 the interface by including it in the base class list.
A re-implementation of an interface follows exactly the same interface mapping rules as an initial implementation of an interface. Thus, the inherited interface mapping has no effect whatsoever on the interface mapping established for the re-implementation of the interface. For example, in the declarations
the fact that 
 maps 
 onto 
 doesn't affect the re-implementation in 
, which maps 
 onto 
.
Inherited public member declarations and inherited explicit interface member declarations participate in the interface mapping process for re-implemented interfaces. For example
Here, the implementation of 
 in 
 maps the interface methods onto 
, 
, 
, and 
.
When a class implements an interface, it implicitly also implements all of that interface's base interfaces. Likewise, a re-implementation of an interface is also implicitly a re-implementation of all of the interface's base interfaces. For example
Here, the re-implementation of 
 also re-implements 
, mapping 
 onto 
.
Abstract classes and interfaces
Like a non-abstract class, an abstract class must provide implementations of all members of the interfaces that are listed in the base class list of the class. However, an abstract class is permitted to map interface methods onto abstract methods. For example
Here, the implementation of 
 maps 
 and 
 onto abstract methods, which must be overridden in non-abstract classes that derive from 
.
Note that explicit interface member implementations cannot be abstract, but explicit interface member implementations are of course permitted to call abstract methods. For example
Here, non-abstract classes that derive from 
 would be required to override 
 and 
, thus providing the actual implementation of 
.
Enums
An 
 is a distinct value type (
) that declares a set of named constants.
The example
declares an enum type named 
 with members 
, 
, and 
.
Enum declarations
An enum declaration declares a new enum type. An enum declaration begins with the keyword 
, and defines the name, accessibility, underlying type, and members of the enum.
Each enum type has a corresponding integral type called the 
 of the enum type. This underlying type must be able to represent all the enumerator values defined in the enumeration. An enum declaration may explicitly declare an underlying type of 
, 
, 
, 
, 
, 
, 
 or 
. Note that 
 cannot be used as an underlying type. An enum declaration that does not explicitly declare an underlying type has an underlying type of 
.
The example
declares an enum with an underlying type of 
. A developer might choose to use an underlying type of 
, as in the example, to enable the use of values that are in the range of 
 but not in the range of 
, or to preserve this option for the future.
Enum modifiers
An 
 may optionally include a sequence of enum modifiers:
It is a compile-time error for the same modifier to appear multiple times in an enum declaration.
The modifiers of an enum declaration have the same meaning as those of a class declaration (
). Note, however, that the 
 and 
 modifiers are not permitted in an enum declaration. Enums cannot be abstract and do not permit derivation.
Enum members
The body of an enum type declaration defines zero or more enum members, which are the named constants of the enum type. No two enum members can have the same name.
Each enum member has an associated constant value. The type of this value is the underlying type for the containing enum. The constant value for each enum member must be in the range of the underlying type for the enum. The example
results in a compile-time error because the constant values 
, 
, and 
 are not in the range of the underlying integral type 
.
Multiple enum members may share the same associated value. The example
shows an enum in which two enum members -- 
 and 
 -- have the same associated value.
The associated value of an enum member is assigned either implicitly or explicitly. If the declaration of the enum member has a 
 initializer, the value of that constant expression, implicitly converted to the underlying type of the enum, is the associated value of the enum member. If the declaration of the enum member has no initializer, its associated value is set implicitly, as follows:
If the enum member is the first enum member declared in the enum type, its associated value is zero.
Otherwise, the associated value of the enum member is obtained by increasing the associated value of the textually preceding enum member by one. This increased value must be within the range of values that can be represented by the underlying type, otherwise a compile-time error occurs.
The example
prints out the enum member names and their associated values. The output is:
for the following reasons:
the enum member 
 is automatically assigned the value zero (since it has no initializer and is the first enum member);
the enum member 
 is explicitly given the value 
;
and the enum member 
 is automatically assigned the value one greater than the member that textually precedes it.
The associated value of an enum member may not, directly or indirectly, use the value of its own associated enum member. Other than this circularity restriction, enum member initializers may freely refer to other enum member initializers, regardless of their textual position. Within an enum member initializer, values of other enum members are always treated as having the type of their underlying type, so that casts are not necessary when referring to other enum members.
The example
results in a compile-time error because the declarations of 
 and 
 are circular. 
 depends on 
 explicitly, and 
 depends on 
 implicitly.
Enum members are named and scoped in a manner exactly analogous to fields within classes. The scope of an enum member is the body of its containing enum type. Within that scope, enum members can be referred to by their simple name. From all other code, the name of an enum member must be qualified with the name of its enum type. Enum members do not have any declared accessibility -- an enum member is accessible if its containing enum type is accessible.
The System.Enum type
The type 
 is the abstract base class of all enum types (this is distinct and different from the underlying type of the enum type), and the members inherited from 
 are available in any enum type. A boxing conversion (
) exists from any enum type to 
, and an unboxing conversion (
) exists from 
 to any enum type.
Note that 
 is not itself an 
. Rather, it is a 
 from which all 
s are derived. The type 
 inherits from the type 
 (
), which, in turn, inherits from type 
. At run-time, a value of type 
 can be 
 or a reference to a boxed value of any enum type.
Enum values and operations
Each enum type defines a distinct type; an explicit enumeration conversion (
) is required to convert between an enum type and an integral type, or between two enum types. The set of values that an enum type can take on is not limited by its enum members. In particular, any value of the underlying type of an enum can be cast to the enum type, and is a distinct valid value of that enum type.
Enum members have the type of their containing enum type (except within other enum member initializers: see 
). The value of an enum member declared in enum type 
 with associated value 
 is 
.
The following operators can be used on values of enum types: 
, 
, 
, 
, 
, 
 (
), binary 
 (
), binary 
 (
), 
, 
, 
 (
), 
 (
), 
 and 
 (
 and 
).
Every enum type automatically derives from the class 
 (which, in turn, derives from 
 and 
). Thus, inherited methods and properties of this class can be used on values of an enum type.
Delegates
Delegates enable scenarios that other languages—such as C++, Pascal, and Modula -- have addressed with function pointers. Unlike C++ function pointers, however, delegates are fully object oriented, and unlike C++ pointers to member functions, delegates encapsulate both an object instance and a method.
A delegate declaration defines a class that is derived from the class 
. A delegate instance encapsulates an invocation list, which is a list of one or more methods, each of which is referred to as a callable entity. For instance methods, a callable entity consists of an instance and a method on that instance. For static methods, a callable entity consists of just a method. Invoking a delegate instance with an appropriate set of arguments causes each of the delegate's callable entities to be invoked with the given set of arguments.
An interesting and useful property of a delegate instance is that it does not know or care about the classes of the methods it encapsulates; all that matters is that those methods be compatible (
) with the delegate's type. This makes delegates perfectly suited for ""anonymous"" invocation.
Delegate declarations
A 
 is a 
 (
) that declares a new delegate type.
It is a compile-time error for the same modifier to appear multiple times in a delegate declaration.
The 
 modifier is only permitted on delegates declared within another type, in which case it specifies that such a delegate hides an inherited member by the same name, as described in 
.
The 
, 
, 
, and 
 modifiers control the accessibility of the delegate type. Depending on the context in which the delegate declaration occurs, some of these modifiers may not be permitted (
).
The delegate's type name is 
.
The optional 
 specifies the parameters of the delegate, and 
 indicates the return type of the delegate.
The optional 
 (
) specifies the type parameters to the delegate itself.
The return type of a delegate type must be either 
, or output-safe (
).
All the formal parameter types of a delegate type must be input-safe. Additionally, any 
 or 
 parameter types must also be output-safe. Note that even 
 parameters are required to be input-safe, due to a limitiation of the underlying execution platform.
Delegate types in C# are name equivalent, not structurally equivalent. Specifically, two different delegate types that have the same parameter lists and return type are considered different delegate types. However, instances of two distinct but structurally equivalent delegate types may compare as equal (
).
For example:
The methods 
 and 
are compatible with both the delegate types 
 and 
 , since they have the same return type and parameter list; however, these delegate types are two different types, so they are not interchangeable. The methods 
, 
, and 
 are incompatible with the delegate types 
 and 
, since they have different return types or parameter lists.
Like other generic type declarations, type arguments must be given to create a constructed delegate type. The parameter types and return type of a constructed delegate type are created by substituting, for each type parameter in the delegate declaration, the corresponding type argument of the constructed delegate type. The resulting return type and parameter types are used in determining what methods are compatible with a constructed delegate type. For example:
The method 
 is compatible with the delegate type 
 and the method 
 is compatible with the delegate type 
 .
The only way to declare a delegate type is via a 
. A delegate type is a class type that is derived from 
. Delegate types are implicitly 
, so it is not permissible to derive any type from a delegate type. It is also not permissible to derive a non-delegate class type from 
. Note that 
 is not itself a delegate type; it is a class type from which all delegate types are derived.
C# provides special syntax for delegate instantiation and invocation. Except for instantiation, any operation that can be applied to a class or class instance can also be applied to a delegate class or instance, respectively. In particular, it is possible to access members of the 
 type via the usual member access syntax.
The set of methods encapsulated by a delegate instance is called an invocation list. When a delegate instance is created (
) from a single method, it encapsulates that method, and its invocation list contains only one entry. However, when two non-null delegate instances are combined, their invocation lists are concatenated -- in the order left operand then right operand -- to form a new invocation list, which contains two or more entries.
Delegates are combined using the binary 
 (
) and 
 operators (
). A delegate can be removed from a combination of delegates, using the binary 
 (
) and 
 operators (
). Delegates can be compared for equality (
).
The following example shows the instantiation of a number of delegates, and their corresponding invocation lists:
When 
 and 
 are instantiated, they each encapsulate one method. When 
 is instantiated, it has an invocation list of two methods, 
 and 
, in that order. 
's invocation list contains 
, 
, and 
, in that order. Finally, 
's invocation list contains 
, 
, 
, 
, and 
, in that order. For more examples of combining (as well as removing) delegates, see 
.
Delegate compatibility
A method or delegate 
 is 
 with a delegate type 
 if all of the following are true:
 and 
 have the same number of parameters, and each parameter in 
 has the same 
 or 
 modifiers as the corresponding parameter in 
.
For each value parameter (a parameter with no 
 or 
 modifier), an identity conversion (
) or implicit reference conversion (
) exists from the parameter type in 
 to the corresponding parameter type in 
.
For each 
 or 
 parameter, the parameter type in 
 is the same as the parameter type in 
.
An identity or implicit reference conversion exists from the return type of 
 to the return type of 
.
Delegate instantiation
An instance of a delegate is created by a 
 (
) or a conversion to a delegate type. The newly created delegate instance then refers to either:
The static method referenced in the 
, or
The target object (which cannot be 
) and instance method referenced in the 
, or
Another delegate.
For example:
Once instantiated, delegate instances always refer to the same target object and method. Remember, when two delegates are combined, or one is removed from another, a new delegate results with its own invocation list; the invocation lists of the delegates combined or removed remain unchanged.
Delegate invocation
C# provides special syntax for invoking a delegate. When a non-null delegate instance whose invocation list contains one entry is invoked, it invokes the one method with the same arguments it was given, and returns the same value as the referred to method. (See 
 for detailed information on delegate invocation.) If an exception occurs during the invocation of such a delegate, and that exception is not caught within the method that was invoked, the search for an exception catch clause continues in the method that called the delegate, as if that method had directly called the method to which that delegate referred.
Invocation of a delegate instance whose invocation list contains multiple entries proceeds by invoking each of the methods in the invocation list, synchronously, in order. Each method so called is passed the same set of arguments as was given to the delegate instance. If such a delegate invocation includes reference parameters (
), each method invocation will occur with a reference to the same variable; changes to that variable by one method in the invocation list will be visible to methods further down the invocation list. If the delegate invocation includes output parameters or a return value, their final value will come from the invocation of the last delegate in the list.
If an exception occurs during processing of the invocation of such a delegate, and that exception is not caught within the method that was invoked, the search for an exception catch clause continues in the method that called the delegate, and any methods further down the invocation list are not invoked.
Attempting to invoke a delegate instance whose value is null results in an exception of type 
.
The following example shows how to instantiate, combine, remove, and invoke delegates:
As shown in the statement 
, a delegate can be present in an invocation list multiple times. In this case, it is simply invoked once per occurrence. In an invocation list such as this, when that delegate is removed, the last occurrence in the invocation list is the one actually removed.
Immediately prior to the execution of the final statement, 
, the delegate 
 refers to an empty invocation list. Attempting to remove a delegate from an empty list (or to remove a non-existent delegate from a non-empty list) is not an error.
The output produced is:
Exceptions
Exceptions in C# provide a structured, uniform, and type-safe way of handling both system level and application level error conditions. The exception mechanism in C# is quite similar to that of C++, with a few important differences:
In C#, all exceptions must be represented by an instance of a class type derived from 
. In C++, any value of any type can be used to represent an exception.
In C#, a finally block (
) can be used to write termination code that executes in both normal execution and exceptional conditions. Such code is difficult to write in C++ without duplicating code.
In C#, system-level exceptions such as overflow, divide-by-zero, and null dereferences have well defined exception classes and are on a par with application-level error conditions.
Causes of exceptions
Exception can be thrown in two different ways.
A 
 statement (
) throws an exception immediately and unconditionally. Control never reaches the statement immediately following the 
.
Certain exceptional conditions that arise during the processing of C# statements and expression cause an exception in certain circumstances when the operation cannot be completed normally. For example, an integer division operation (
) throws a 
 if the denominator is zero. See 
 for a list of the various exceptions that can occur in this way.
The System.Exception class
The 
 class is the base type of all exceptions. This class has a few notable properties that all exceptions share:
 is a read-only property of type 
 that contains a human-readable description of the reason for the exception.
 is a read-only property of type 
. If its value is non-null, it refers to the exception that caused the current exception—that is, the current exception was raised in a catch block handling the 
. Otherwise, its value is null, indicating that this exception was not caused by another exception. The number of exception objects chained together in this manner can be arbitrary.
The value of these properties can be specified in calls to the instance constructor for 
.
How exceptions are handled
Exceptions are handled by a 
 statement (
).
When an exception occurs, the system searches for the nearest 
 clause that can handle the exception, as determined by the run-time type of the exception. First, the current method is searched for a lexically enclosing 
 statement, and the associated catch clauses of the try statement are considered in order. If that fails, the method that called the current method is searched for a lexically enclosing 
 statement that encloses the point of the call to the current method. This search continues until a 
 clause is found that can handle the current exception, by naming an exception class that is of the same class, or a base class, of the run-time type of the exception being thrown. A 
 clause that doesn't name an exception class can handle any exception.
Once a matching catch clause is found, the system prepares to transfer control to the first statement of the catch clause. Before execution of the catch clause begins, the system first executes, in order, any 
 clauses that were associated with try statements more nested that than the one that caught the exception.
If no matching catch clause is found, one of two things occurs:
If the search for a matching catch clause reaches a static constructor (
) or static field initializer, then a 
 is thrown at the point that triggered the invocation of the static constructor. The inner exception of the 
 contains the exception that was originally thrown.
If the search for matching catch clauses reaches the code that initially started the thread, then execution of the thread is terminated. The impact of such termination is implementation-defined.
Exceptions that occur during destructor execution are worth special mention. If an exception occurs during destructor execution, and that exception is not caught, then the execution of that destructor is terminated and the destructor of the base class (if any) is called. If there is no base class (as in the case of the 
 type) or if there is no base class destructor, then the exception is discarded.
Common Exception Classes
The following exceptions are thrown by certain C# operations.
A base class for exceptions that occur during arithmetic operations, such as 
 and 
.
Thrown when a store into an array fails because the actual type of the stored element is incompatible with the actual type of the array.
Thrown when an attempt to divide an integral value by zero occurs.
Thrown when an attempt to index an array via an index that is less than zero or outside the bounds of the array.
Thrown when an explicit conversion from a base type or interface to a derived type fails at run time.
Thrown when a 
 reference is used in a way that causes the referenced object to be required.
Thrown when an attempt to allocate memory (via 
) fails.
Thrown when an arithmetic operation in a 
 context overflows.
Thrown when the execution stack is exhausted by having too many pending method calls; typically indicative of very deep or unbounded recursion.
Thrown when a static constructor throws an exception, and no 
 clauses exists to catch it.
Attributes
Much of the C# language enables the programmer to specify declarative information about the entities defined in the program. For example, the accessibility of a method in a class is specified by decorating it with the 
s 
, 
, 
, and 
.
C# enables programmers to invent new kinds of declarative information, called 
. Programmers can then attach attributes to various program entities, and retrieve attribute information in a run-time environment. For instance, a framework might define a 
 attribute that can be placed on certain program elements (such as classes and methods) to provide a mapping from those program elements to their documentation.
Attributes are defined through the declaration of attribute classes (
), which may have positional and named parameters (
). Attributes are attached to entities in a C# program using attribute specifications (
), and can be retrieved at run-time as attribute instances (
).
Attribute classes
A class that derives from the abstract class 
, whether directly or indirectly, is an 
. The declaration of an attribute class defines a new kind of 
 that can be placed on a declaration. By convention, attribute classes are named with a suffix of 
. Uses of an attribute may either include or omit this suffix.
Attribute usage
The attribute 
 (
) is used to describe how an attribute class can be used.
 has a positional parameter (
) that enables an attribute class to specify the kinds of declarations on which it can be used. The example
defines an attribute class named 
 that can be placed on 
s and 
s only. The example
shows several uses of the 
 attribute. Although this attribute is defined with the name 
, when this attribute is used, the 
 suffix may be omitted, resulting in the short name 
. Thus, the example above is semantically equivalent to the following:
 has a named parameter (
) called 
, which indicates whether the attribute can be specified more than once for a given entity. If 
 for an attribute class is true, then that attribute class is a 
, and can be specified more than once on an entity. If 
 for an attribute class is false or it is unspecified, then that attribute class is a 
, and can be specified at most once on an entity.
The example
defines a multi-use attribute class named 
. The example
shows a class declaration with two uses of the 
 attribute.
 has another named parameter called 
, which indicates whether the attribute, when specified on a base class, is also inherited by classes that derive from that base class. If 
 for an attribute class is true, then that attribute is inherited. If 
 for an attribute class is false then that attribute is not inherited. If it is unspecified, its default value is true.
An attribute class 
 not having an 
 attribute attached to it, as in
is equivalent to the following:
Positional and named parameters
Attribute classes can have 
 and 
. Each public instance constructor for an attribute class defines a valid sequence of positional parameters for that attribute class. Each non-static public read-write field and property for an attribute class defines a named parameter for the attribute class.
The example
defines an attribute class named 
 that has one positional parameter, 
, and one named parameter, 
. Although it is non-static and public, the property 
 does not define a named parameter, since it is not read-write.
This attribute class might be used as follows:
Attribute parameter types
The types of positional and named parameters for an attribute class are limited to the 
, which are:
One of the following types: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
.
The type 
.
The type 
.
An enum type, provided it has public accessibility and the types in which it is nested (if any) also have public accessibility (
).
Single-dimensional arrays of the above types.
A constructor argument or public field which does not have one of these types, cannot be used as a positional or named parameter in an attribute specification.
Attribute specification
 is the application of a previously defined attribute to a declaration. An attribute is a piece of additional declarative information that is specified for a declaration. Attributes can be specified at global scope (to specify attributes on the containing assembly or module) and for 
s (
), 
s (
), 
s (
), 
s (
), 
s (
), 
 (
), 
 (
), and 
s (
).
Attributes are specified in 
. An attribute section consists of a pair of square brackets, which surround a comma-separated list of one or more attributes. The order in which attributes are specified in such a list, and the order in which sections attached to the same program entity are arranged, is not significant. For instance, the attribute specifications 
, 
, 
, and 
 are equivalent.
An attribute consists of an 
 and an optional list of positional and named arguments. The positional arguments (if any) precede the named arguments. A positional argument consists of an 
; a named argument consists of a name, followed by an equal sign, followed by an 
, which, together, are constrained by the same rules as simple assignment. The order of named arguments is not significant.
The 
 identifies an attribute class. If the form of 
 is 
 then this name must refer to an attribute class. Otherwise, a compile-time error occurs. The example
results in a compile-time error because it attempts to use 
 as an attribute class when 
 is not an attribute class.
Certain contexts permit the specification of an attribute on more than one target. A program can explicitly specify the target by including an 
. When an attribute is placed at the global level, a 
 is required. In all other locations, a reasonable default is applied, but an 
 can be used to affirm or override the default in certain ambiguous cases (or to just affirm the default in non-ambiguous cases). Thus, typically, 
s can be omitted except at the global level. The potentially ambiguous contexts are resolved as follows:
An attribute specified at global scope can apply either to the target assembly or the target module. No default exists for this context, so an 
 is always required in this context. The presence of the 
 
 indicates that the attribute applies to the target assembly; the presence of the 
 
 indicates that the attribute applies to the target module.
An attribute specified on a delegate declaration can apply either to the delegate being declared or to its return value. In the absence of an 
, the attribute applies to the delegate. The presence of the 
 
 indicates that the attribute applies to the delegate; the presence of the 
 
 indicates that the attribute applies to the return value.
An attribute specified on a method declaration can apply either to the method being declared or to its return value. In the absence of an 
, the attribute applies to the method. The presence of the 
 
 indicates that the attribute applies to the method; the presence of the 
 
 indicates that the attribute applies to the return value.
An attribute specified on an operator declaration can apply either to the operator being declared or to its return value. In the absence of an 
, the attribute applies to the operator. The presence of the 
 
 indicates that the attribute applies to the operator; the presence of the 
 
 indicates that the attribute applies to the return value.
An attribute specified on an event declaration that omits event accessors can apply to the event being declared, to the associated field (if the event is not abstract), or to the associated add and remove methods. In the absence of an 
, the attribute applies to the event. The presence of the 
 
 indicates that the attribute applies to the event; the presence of the 
 
 indicates that the attribute applies to the field; and the presence of the 
 
 indicates that the attribute applies to the methods.
An attribute specified on a get accessor declaration for a property or indexer declaration can apply either to the associated method or to its return value. In the absence of an 
, the attribute applies to the method. The presence of the 
 
 indicates that the attribute applies to the method; the presence of the 
 
 indicates that the attribute applies to the return value.
An attribute specified on a set accessor for a property or indexer declaration can apply either to the associated method or to its lone implicit parameter. In the absence of an 
, the attribute applies to the method. The presence of the 
 
 indicates that the attribute applies to the method; the presence of the 
 
 indicates that the attribute applies to the parameter; the presence of the 
 
 indicates that the attribute applies to the return value.
An attribute specified on an add or remove accessor declaration for an event declaration can apply either to the associated method or to its lone parameter. In the absence of an 
, the attribute applies to the method. The presence of the 
 
 indicates that the attribute applies to the method; the presence of the 
 
 indicates that the attribute applies to the parameter; the presence of the 
 
 indicates that the attribute applies to the return value.
In other contexts, inclusion of an 
 is permitted but unnecessary. For instance, a class declaration may either include or omit the specifier 
:
It is an error to specify an invalid 
. For instance, the specifier 
 cannot be used on a class declaration:
By convention, attribute classes are named with a suffix of 
. An 
 of the form 
 may either include or omit this suffix. If an attribute class is found both with and without this suffix, an ambiguity is present, and a compile-time error results. If the 
 is spelled such that its right-most 
 is a verbatim identifier (
), then only an attribute without a suffix is matched, thus enabling such an ambiguity to be resolved. The example
shows two attribute classes named 
 and 
. The attribute 
 is ambiguous, since it could refer to either 
 or 
. Using a verbatim identifier allows the exact intent to be specified in such rare cases. The attribute 
 is not ambiguous (although it would be if there was an attribute class named 
!). If the declaration for class 
 is removed, then both attributes refer to the attribute class named 
, as follows:
It is a compile-time error to use a single-use attribute class more than once on the same entity. The example
results in a compile-time error because it attempts to use 
, which is a single-use attribute class, more than once on the declaration of 
.
An expression 
 is an 
 if all of the following statements are true:
The type of 
 is an attribute parameter type (
).
At compile-time, the value of 
 can be resolved to one of the following:
A constant value.
A 
 object.
A one-dimensional array of 
s.
For example:
A 
 (
) used as an attribute argument expression can reference a non-generic type, a closed constructed type, or an unbound generic type, but it cannot reference an open type. This is to ensure that the expression can be resolved at compile-time.
Attribute instances
An 
 is an instance that represents an attribute at run-time. An attribute is defined with an attribute class, positional arguments, and named arguments. An attribute instance is an instance of the attribute class that is initialized with the positional and named arguments.
Retrieval of an attribute instance involves both compile-time and run-time processing, as described in the following sections.
Compilation of an attribute
The compilation of an 
 with attribute class 
, 
 
 and 
 
, consists of the following steps:
Follow the compile-time processing steps for compiling an 
 of the form 
. These steps either result in a compile-time error, or determine an instance constructor 
 on 
 that can be invoked at run-time.
If 
 does not have public accessibility, then a compile-time error occurs.
For each 
 
 in 
:
Let 
 be the 
 of the 
 
.
 must identify a non-static read-write public field or property on 
. If 
 has no such field or property, then a compile-time error occurs.
Keep the following information for run-time instantiation of the attribute: the attribute class 
, the instance constructor 
 on 
, the 
 
 and the 
 
.
Run-time retrieval of an attribute instance
Compilation of an 
 yields an attribute class 
, an instance constructor 
 on 
, a 
 
, and a 
 
. Given this information, an attribute instance can be retrieved at run-time using the following steps:
Follow the run-time processing steps for executing an 
 of the form 
, using the instance constructor 
 as determined at compile-time. These steps either result in an exception, or produce an instance 
 of 
.
For each 
 
 in 
, in order:
Let 
 be the 
 of the 
 
. If 
 does not identify a non-static public read-write field or property on 
, then an exception is thrown.
Let 
 be the result of evaluating the 
 of 
.
If 
 identifies a field on 
, then set this field to 
.
Otherwise, 
 identifies a property on 
. Set this property to 
.
The result is 
, an instance of the attribute class 
 that has been initialized with the 
 
 and the 
 
.
Reserved attributes
A small number of attributes affect the language in some way. These attributes include:
 (
), which is used to describe the ways in which an attribute class can be used.
 (
), which is used to define conditional methods.
 (
), which is used to mark a member as obsolete.
, 
 and 
 (
), which are used to supply information about the calling context to optional parameters.
The AttributeUsage attribute
The attribute 
 is used to describe the manner in which the attribute class can be used.
A class that is decorated with the 
 attribute must derive from 
, either directly or indirectly. Otherwise, a compile-time error occurs.
The Conditional attribute
The attribute 
 enables the definition of 
 and 
.
Conditional methods
A method decorated with the 
 attribute is a conditional method. The 
 attribute indicates a condition by testing a conditional compilation symbol. Calls to a conditional method are either included or omitted depending on whether this symbol is defined at the point of the call. If the symbol is defined, the call is included; otherwise, the call (including evaluation of the receiver and parameters of the call) is omitted.
A conditional method is subject to the following restrictions:
The conditional method must be a method in a 
 or 
. A compile-time error occurs if the 
 attribute is specified on a method in an interface declaration.
The conditional method must have a return type of 
.
The conditional method must not be marked with the 
 modifier. A conditional method may be marked with the 
 modifier, however. Overrides of such a method are implicitly conditional, and must not be explicitly marked with a 
 attribute.
The conditional method must not be an implementation of an interface method. Otherwise, a compile-time error occurs.
In addition, a compile-time error occurs if a conditional method is used in a 
. The example
declares 
 as a conditional method. 
's 
 method calls this method. Since the conditional compilation symbol 
 is defined, if 
 is called, it will call 
. If the symbol 
 had not been defined, then 
 would not call 
.
It is important to note that the inclusion or exclusion of a call to a conditional method is controlled by the conditional compilation symbols at the point of the call. In the example
File 
:
File 
:
File 
:
the classes 
 and 
 each contain calls to the conditional method 
, which is conditional based on whether or not 
 is defined. Since this symbol is defined in the context of 
 but not 
, the call to 
 in 
 is included, while the call to 
 in 
 is omitted.
The use of conditional methods in an inheritance chain can be confusing. Calls made to a conditional method through 
, of the form 
, are subject to the normal conditional method call rules. In the example
File 
:
File 
:
File 
:
 includes a call to the 
 defined in its base class. This call is omitted because the base method is conditional based on the presence of the symbol 
, which is undefined. Thus, the method writes to the console ""
"" only. Judicious use of 
s can eliminate such problems.
Conditional attribute classes
An attribute class (
) decorated with one or more 
 attributes is a 
. A conditional attribute class is thus associated with the conditional compilation symbols declared in its 
 attributes. This example:
declares 
 as a conditional attribute class associated with the conditional compilations symbols 
 and 
.
Attribute specifications (
) of a conditional attribute are included if one or more of its associated conditional compilation symbols is defined at the point of specification, otherwise the attribute specification is omitted.
It is important to note that the inclusion or exclusion of an attribute specification of a conditional attribute class is controlled by the conditional compilation symbols at the point of the specification. In the example
File 
:
File 
:
File 
:
the classes 
 and 
 are each decorated with attribute 
, which is conditional based on whether or not 
 is defined. Since this symbol is defined in the context of 
 but not 
, the specification of the 
 attribute on 
 is included, while the specification of the 
 attribute on 
 is omitted.
The Obsolete attribute
The attribute 
 is used to mark types and members of types that should no longer be used.
If a program uses a type or member that is decorated with the 
 attribute, the compiler issues a warning or an error. Specifically, the compiler issues a warning if no error parameter is provided, or if the error parameter is provided and has the value 
. The compiler issues an error if the error parameter is specified and has the value 
.
In the example
the class 
 is decorated with the 
 attribute. Each use of 
 in 
 results in a warning that includes the specified message, ""This class is obsolete; use class B instead.""
Caller info attributes
For purposes such as logging and reporting, it is sometimes useful for a function member to obtain certain compile-time information about the calling code. The caller info attributes provide a way to pass such information transparently.
When an optional parameter is annotated with one of the caller info attributes, omitting the corresponding argument in a call does not necessarily cause the default parameter value to be substituted. Instead, if the specified information about the calling context is available, that information will be passed as the argument value.
For example:
A call to 
 with no arguments would print the line number and file path of the call, as well as the name of the member within which the call occurred.
Caller info attributes can occur on optional parameters anywhere, including in delegate declarations. However, the specific caller info attributes have restrictions on the types of the parameters they can attribute, so that there will always be an implicit conversion from a substituted value to the parameter type.
It is an error to have the same caller info attribute on a parameter of both the defining and implementing part of a partial method declaration. Only caller info attributes in the defining part are applied, whereas caller info attributes occurring only in the implementing part are ignored.
Caller information does not affect overload resolution. As the attributed optional parameters are still omitted from the source code of the caller, overload resolution ignores those parameters in the same way it ignores other omitted optional parameters (
).
Caller information is only substituted when a function is explicitly invoked in source code. Implicit invocations such as implicit parent constructor calls do not have a source location and will not substitute caller information. Also, calls that are dynamically bound will not substitute caller information. When a caller info attributed parameter is omitted in such cases, the specified default value of the parameter is used instead.
One exception is query-expressions. These are considered syntactic expansions, and if the calls they expand to omit optional parameters with caller info attributes, caller information will be substituted. The location used is the location of the query clause which the call was generated from.
If more than one caller info attribute is specified on a given parameter, they are preferred in the following order: 
, 
, 
.
The CallerLineNumber attribute
The 
 is allowed on optional parameters when there is a standard implicit conversion (
) from the constant value 
 to the parameter's type. This ensures that any non-negative line number up to that value can be passed without error.
If a function invocation from a location in source code omits an optional parameter with the 
, then a numeric literal representing that location's line number is used as an argument to the invocation instead of the default parameter value.
If the invocation spans multiple lines, the line chosen is implementation-dependent.
Note that the line number may be affected by 
 directives (
).
The CallerFilePath attribute
The 
 is allowed on optional parameters when there is a standard implicit conversion (
) from 
 to the parameter's type.
If a function invocation from a location in source code omits an optional parameter with the 
, then a string literal representing that location's file path is used as an argument to the invocation instead of the default parameter value.
The format of the file path is implementation-dependent.
Note that the file path may be affected by 
 directives (
).
The CallerMemberName attribute
The 
 is allowed on optional parameters when there is a standard implicit conversion (
) from 
 to the parameter's type.
If a function invocation from a location within the body of a function member or within an attribute applied to the function member itself or its return type, parameters or type parameters in source code omits an optional parameter with the 
, then a string literal representing the name of that member is used as an argument to the invocation instead of the default parameter value.
For invocations that occur within generic methods, only the method name itself is used, without the type parameter list.
For invocations that occur within explicit interface member implementations, only the method name itself is used, without the preceding interface qualification.
For invocations that occur within property or event accessors, the member name used is that of the property or event itself.
For invocations that occur within indexer accessors, the member name used is that supplied by an 
 (
) on the indexer member, if present, or the default name 
 otherwise.
For invocations that occur within declarations of instance constructors, static constructors, destructors and operators the member name used is implementation-dependent.
Attributes for Interoperation
Note: This section is applicable only to the Microsoft .NET implementation of C#.
Interoperation with COM and Win32 components
The .NET run-time provides a large number of attributes that enable C# programs to interoperate with components written using COM and Win32 DLLs. For example, the 
 attribute can be used on a 
 method to indicate that the implementation of the method is to be found in a Win32 DLL. These attributes are found in the 
 namespace, and detailed documentation for these attributes is found in the .NET runtime documentation.
Interoperation with other .NET languages
The IndexerName attribute
Indexers are implemented in .NET using indexed properties, and have a name in the .NET metadata. If no 
 attribute is present for an indexer, then the name 
 is used by default. The 
 attribute enables a developer to override this default and specify a different name.
Unsafe code
The core C# language, as defined in the preceding chapters, differs notably from C and C++ in its omission of pointers as a data type. Instead, C# provides references and the ability to create objects that are managed by a garbage collector. This design, coupled with other features, makes C# a much safer language than C or C++. In the core C# language it is simply not possible to have an uninitialized variable, a ""dangling"" pointer, or an expression that indexes an array beyond its bounds. Whole categories of bugs that routinely plague C and C++ programs are thus eliminated.
While practically every pointer type construct in C or C++ has a reference type counterpart in C#, nonetheless, there are situations where access to pointer types becomes a necessity. For example, interfacing with the underlying operating system, accessing a memory-mapped device, or implementing a time-critical algorithm may not be possible or practical without access to pointers. To address this need, C# provides the ability to write 
.
In unsafe code it is possible to declare and operate on pointers, to perform conversions between pointers and integral types, to take the address of variables, and so forth. In a sense, writing unsafe code is much like writing C code within a C# program.
Unsafe code is in fact a ""safe"" feature from the perspective of both developers and users. Unsafe code must be clearly marked with the modifier 
, so developers can't possibly use unsafe features accidentally, and the execution engine works to ensure that unsafe code cannot be executed in an untrusted environment.
Unsafe contexts
The unsafe features of C# are available only in unsafe contexts. An unsafe context is introduced by including an 
 modifier in the declaration of a type or member, or by employing an 
:
A declaration of a class, struct, interface, or delegate may include an 
 modifier, in which case the entire textual extent of that type declaration (including the body of the class, struct, or interface) is considered an unsafe context.
A declaration of a field, method, property, event, indexer, operator, instance constructor, destructor, or static constructor may include an 
 modifier, in which case the entire textual extent of that member declaration is considered an unsafe context.
An 
 enables the use of an unsafe context within a 
. The entire textual extent of the associated 
 is considered an unsafe context.
The associated grammar productions are shown below.
In the example
the 
 modifier specified in the struct declaration causes the entire textual extent of the struct declaration to become an unsafe context. Thus, it is possible to declare the 
 and 
 fields to be of a pointer type. The example above could also be written
Here, the 
 modifiers in the field declarations cause those declarations to be considered unsafe contexts.
Other than establishing an unsafe context, thus permitting the use of pointer types, the 
 modifier has no effect on a type or a member. In the example
the 
 modifier on the 
 method in 
 simply causes the textual extent of 
 to become an unsafe context in which the unsafe features of the language can be used. In the override of 
 in 
, there is no need to re-specify the 
 modifier -- unless, of course, the 
 method in 
 itself needs access to unsafe features.
The situation is slightly different when a pointer type is part of the method's signature
Here, because 
's signature includes a pointer type, it can only be written in an unsafe context. However, the unsafe context can be introduced by either making the entire class unsafe, as is the case in 
, or by including an 
 modifier in the method declaration, as is the case in 
.
Pointer types
In an unsafe context, a 
 (
) may be a 
 as well as a 
 or a 
. However, a 
 may also be used in a 
 expression (
) outside of an unsafe context as such usage is not unsafe.
A 
 is written as an 
 or the keyword 
, followed by a 
 token:
The type specified before the 
 in a pointer type is called the 
 of the pointer type. It represents the type of the variable to which a value of the pointer type points.
Unlike references (values of reference types), pointers are not tracked by the garbage collector -- the garbage collector has no knowledge of pointers and the data to which they point. For this reason a pointer is not permitted to point to a reference or to a struct that contains references, and the referent type of a pointer must be an 
.
An 
 is any type that isn't a 
 or constructed type, and doesn't contain 
 or constructed type fields at any level of nesting. In other words, an 
 is one of the following:
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
Any 
.
Any 
.
Any user-defined 
 that is not a constructed type and contains fields of 
s only.
The intuitive rule for mixing of pointers and references is that referents of references (objects) are permitted to contain pointers, but referents of pointers are not permitted to contain references.
Some examples of pointer types are given in the table below:
Example
Description
Pointer to 
Pointer to 
Pointer to pointer to 
Single-dimensional array of pointers to 
Pointer to unknown type
For a given implementation, all pointer types must have the same size and representation.
Unlike C and C++, when multiple pointers are declared in the same declaration, in C# the 
 is written along with the underlying type only, not as a prefix punctuator on each pointer name. For example
The value of a pointer having type 
 represents the address of a variable of type 
. The pointer indirection operator 
 (
) may be used to access this variable. For example, given a variable 
 of type 
, the expression 
 denotes the 
 variable found at the address contained in 
.
Like an object reference, a pointer may be 
. Applying the indirection operator to a 
 pointer results in implementation-defined behavior. A pointer with value 
 is represented by all-bits-zero.
The 
 type represents a pointer to an unknown type. Because the referent type is unknown, the indirection operator cannot be applied to a pointer of type 
, nor can any arithmetic be performed on such a pointer. However, a pointer of type 
 can be cast to any other pointer type (and vice versa).
Pointer types are a separate category of types. Unlike reference types and value types, pointer types do not inherit from 
 and no conversions exist between pointer types and 
. In particular, boxing and unboxing (
) are not supported for pointers. However, conversions are permitted between different pointer types and between pointer types and the integral types. This is described in 
.
A 
 cannot be used as a type argument (
), and type inference (
) fails on generic method calls that would have inferred a type argument to be a pointer type.
A 
 may be used as the type of a volatile field (
).
Although pointers can be passed as 
 or 
 parameters, doing so can cause undefined behavior, since the pointer may well be set to point to a local variable which no longer exists when the called method returns, or the fixed object to which it used to point, is no longer fixed. For example:
A method can return a value of some type, and that type can be a pointer. For example, when given a pointer to a contiguous sequence of 
s, that sequence's element count, and some other 
 value, the following method returns the address of that value in that sequence, if a match occurs; otherwise it returns 
:
In an unsafe context, several constructs are available for operating on pointers:
The 
 operator may be used to perform pointer indirection (
).
The 
 operator may be used to access a member of a struct through a pointer (
).
The 
 operator may be used to index a pointer (
).
The 
 operator may be used to obtain the address of a variable (
).
The 
 and 
 operators may be used to increment and decrement pointers (
).
The 
 and 
 operators may be used to perform pointer arithmetic (
).
The 
, 
, 
, 
, 
, and 
 operators may be used to compare pointers (
).
The 
 operator may be used to allocate memory from the call stack (
).
The 
 statement may be used to temporarily fix a variable so its address can be obtained (
).
Fixed and moveable variables
The address-of operator (
) and the 
 statement (
) divide variables into two categories: 
 and 
.
Fixed variables reside in storage locations that are unaffected by operation of the garbage collector. (Examples of fixed variables include local variables, value parameters, and variables created by dereferencing pointers.) On the other hand, moveable variables reside in storage locations that are subject to relocation or disposal by the garbage collector. (Examples of moveable variables include fields in objects and elements of arrays.)
The 
 operator (
) permits the address of a fixed variable to be obtained without restrictions. However, because a moveable variable is subject to relocation or disposal by the garbage collector, the address of a moveable variable can only be obtained using a 
 statement (
), and that address remains valid only for the duration of that 
 statement.
In precise terms, a fixed variable is one of the following:
A variable resulting from a 
 (
) that refers to a local variable or a value parameter, unless the variable is captured by an anonymous function.
A variable resulting from a 
 (
) of the form 
, where 
 is a fixed variable of a 
.
A variable resulting from a 
 (
) of the form 
, a 
 (
) of the form 
, or a 
 (
) of the form 
.
All other variables are classified as moveable variables.
Note that a static field is classified as a moveable variable. Also note that a 
 or 
 parameter is classified as a moveable variable, even if the argument given for the parameter is a fixed variable. Finally, note that a variable produced by dereferencing a pointer is always classified as a fixed variable.
Pointer conversions
In an unsafe context, the set of available implicit conversions (
) is extended to include the following implicit pointer conversions:
From any 
 to the type 
.
From the 
 literal to any 
.
Additionally, in an unsafe context, the set of available explicit conversions (
) is extended to include the following explicit pointer conversions:
From any 
 to any other 
.
From 
, 
, 
, 
, 
, 
, 
, or 
 to any 
.
From any 
 to 
, 
, 
, 
, 
, 
, 
, or 
.
Finally, in an unsafe context, the set of standard implicit conversions (
) includes the following pointer conversion:
From any 
 to the type 
.
Conversions between two pointer types never change the actual pointer value. In other words, a conversion from one pointer type to another has no effect on the underlying address given by the pointer.
When one pointer type is converted to another, if the resulting pointer is not correctly aligned for the pointed-to type, the behavior is undefined if the result is dereferenced. In general, the concept ""correctly aligned"" is transitive: if a pointer to type 
 is correctly aligned for a pointer to type 
, which, in turn, is correctly aligned for a pointer to type 
, then a pointer to type 
 is correctly aligned for a pointer to type 
.
Consider the following case in which a variable having one type is accessed via a pointer to a different type:
When a pointer type is converted to a pointer to byte, the result points to the lowest addressed byte of the variable. Successive increments of the result, up to the size of the variable, yield pointers to the remaining bytes of that variable. For example, the following method displays each of the eight bytes in a double as a hexadecimal value:
Of course, the output produced depends on endianness.
Mappings between pointers and integers are implementation-defined. However, on 32* and 64-bit CPU architectures with a linear address space, conversions of pointers to or from integral types typically behave exactly like conversions of 
 or 
 values, respectively, to or from those integral types.
Pointer arrays
In an unsafe context, arrays of pointers can be constructed. Only some of the conversions that apply to other array types are allowed on pointer arrays:
The implicit reference conversion (
) from any 
 to 
 and the interfaces it implements also applies to pointer arrays. However, any attempt to access the array elements through 
 or the interfaces it implements will result in an exception at run-time, as pointer types are not convertible to 
.
The implicit and explicit reference conversions (
, 
) from a single-dimensional array type 
 to 
 and its generic base interfaces never apply to pointer arrays, since pointer types cannot be used as type arguments, and there are no conversions from pointer types to non-pointer types.
The explicit reference conversion (
) from 
 and the interfaces it implements to any 
 applies to pointer arrays.
The explicit reference conversions (
) from 
 and its base interfaces to a single-dimensional array type 
 never applies to pointer arrays, since pointer types cannot be used as type arguments, and there are no conversions from pointer types to non-pointer types.
These restrictions mean that the expansion for the 
 statement over arrays described in 
 cannot be applied to pointer arrays. Instead, a foreach statement of the form
where the type of 
 is an array type of the form 
, 
 is the number of dimensions minus 1 and 
 or 
 is a pointer type, is expanded using nested for-loops as follows:
The variables 
, 
, 
, ..., 
 are not visible to or accessible to 
 or the 
 or any other source code of the program. The variable 
 is read-only in the embedded statement. If there is not an explicit conversion (
) from 
 (the element type) to 
, an error is produced and no further steps are taken. If 
 has the value 
, a 
 is thrown at run-time.
Pointers in expressions
In an unsafe context, an expression may yield a result of a pointer type, but outside an unsafe context it is a compile-time error for an expression to be of a pointer type. In precise terms, outside an unsafe context a compile-time error occurs if any 
 (
), 
 (
), 
 (
), or 
 (
) is of a pointer type.
In an unsafe context, the 
 (
) and 
 (
) productions permit the following additional constructs:
These constructs are described in the following sections. The precedence and associativity of the unsafe operators is implied by the grammar.
Pointer indirection
A 
 consists of an asterisk (
) followed by a 
.
The unary 
 operator denotes pointer indirection and is used to obtain the variable to which a pointer points. The result of evaluating 
, where 
 is an expression of a pointer type 
, is a variable of type 
. It is a compile-time error to apply the unary 
 operator to an expression of type 
 or to an expression that isn't of a pointer type.
The effect of applying the unary 
 operator to a 
 pointer is implementation-defined. In particular, there is no guarantee that this operation throws a 
.
If an invalid value has been assigned to the pointer, the behavior of the unary 
 operator is undefined. Among the invalid values for dereferencing a pointer by the unary 
 operator are an address inappropriately aligned for the type pointed to (see example in 
), and the address of a variable after the end of its lifetime.
For purposes of definite assignment analysis, a variable produced by evaluating an expression of the form 
 is considered initially assigned (
).
Pointer member access
A 
 consists of a 
, followed by a ""
"" token, followed by an 
 and an optional 
.
In a pointer member access of the form 
, 
 must be an expression of a pointer type other than 
, and 
 must denote an accessible member of the type to which 
 points.
A pointer member access of the form 
 is evaluated exactly as 
. For a description of the pointer indirection operator (
), see 
. For a description of the member access operator (
), see 
.
In the example
the 
 operator is used to access fields and invoke a method of a struct through a pointer. Because the operation 
 is precisely equivalent to 
, the 
 method could equally well have been written:
Pointer element access
A 
 consists of a 
 followed by an expression enclosed in ""
"" and ""
"".
In a pointer element access of the form 
, 
 must be an expression of a pointer type other than 
, and 
 must be an expression that can be implicitly converted to 
, 
, 
, or 
.
A pointer element access of the form 
 is evaluated exactly as 
. For a description of the pointer indirection operator (
), see 
. For a description of the pointer addition operator (
), see 
.
In the example
a pointer element access is used to initialize the character buffer in a 
 loop. Because the operation 
 is precisely equivalent to 
, the example could equally well have been written:
The pointer element access operator does not check for out-of-bounds errors and the behavior when accessing an out-of-bounds element is undefined. This is the same as C and C++.
The address-of operator
An 
 consists of an ampersand (
) followed by a 
.
Given an expression 
 which is of a type 
 and is classified as a fixed variable (
), the construct 
 computes the address of the variable given by 
. The type of the result is 
 and is classified as a value. A compile-time error occurs if 
 is not classified as a variable, if 
 is classified as a read-only local variable, or if 
 denotes a moveable variable. In the last case, a fixed statement (
) can be used to temporarily ""fix"" the variable before obtaining its address. As stated in 
, outside an instance constructor or static constructor for a struct or class that defines a 
 field, that field is considered a value, not a variable. As such, its address cannot be taken. Similarly, the address of a constant cannot be taken.
The 
 operator does not require its argument to be definitely assigned, but following an 
 operation, the variable to which the operator is applied is considered definitely assigned in the execution path in which the operation occurs. It is the responsibility of the programmer to ensure that correct initialization of the variable actually does take place in this situation.
In the example
 is considered definitely assigned following the 
 operation used to initialize 
. The assignment to 
 in effect initializes 
, but the inclusion of this initialization is the responsibility of the programmer, and no compile-time error would occur if the assignment was removed.
The rules of definite assignment for the 
 operator exist such that redundant initialization of local variables can be avoided. For example, many external APIs take a pointer to a structure which is filled in by the API. Calls to such APIs typically pass the address of a local struct variable, and without the rule, redundant initialization of the struct variable would be required.
Pointer increment and decrement
In an unsafe context, the 
 and 
 operators (
 and 
) can be applied to pointer variables of all types except 
. Thus, for every pointer type 
, the following operators are implicitly defined:
The operators produce the same results as 
 and 
, respectively (
). In other words, for a pointer variable of type 
, the 
 operator adds 
 to the address contained in the variable, and the 
 operator subtracts 
 from the address contained in the variable.
If a pointer increment or decrement operation overflows the domain of the pointer type, the result is implementation-defined, but no exceptions are produced.
Pointer arithmetic
In an unsafe context, the 
 and 
 operators (
 and 
) can be applied to values of all pointer types except 
. Thus, for every pointer type 
, the following operators are implicitly defined:
Given an expression 
 of a pointer type 
 and an expression 
 of type 
, 
, 
, or 
, the expressions 
 and 
 compute the pointer value of type 
 that results from adding 
 to the address given by 
. Likewise, the expression 
 computes the pointer value of type 
 that results from subtracting 
 from the address given by 
.
Given two expressions, 
 and 
, of a pointer type 
, the expression 
 computes the difference between the addresses given by 
 and 
 and then divides that difference by 
. The type of the result is always 
. In effect, 
 is computed as 
.
For example:
which produces the output:
If a pointer arithmetic operation overflows the domain of the pointer type, the result is truncated in an implementation-defined fashion, but no exceptions are produced.
Pointer comparison
In an unsafe context, the 
, 
, 
, 
, 
, and 
 operators (
) can be applied to values of all pointer types. The pointer comparison operators are:
Because an implicit conversion exists from any pointer type to the 
 type, operands of any pointer type can be compared using these operators. The comparison operators compare the addresses given by the two operands as if they were unsigned integers.
The sizeof operator
The 
 operator returns the number of bytes occupied by a variable of a given type. The type specified as an operand to 
 must be an 
 (
).
The result of the 
 operator is a value of type 
. For certain predefined types, the 
 operator yields a constant value as shown in the table below.
Expression
Result
For all other types, the result of the 
 operator is implementation-defined and is classified as a value, not a constant.
The order in which members are packed into a struct is unspecified.
For alignment purposes, there may be unnamed padding at the beginning of a struct, within a struct, and at the end of the struct. The contents of the bits used as padding are indeterminate.
When applied to an operand that has struct type, the result is the total number of bytes in a variable of that type, including any padding.
The fixed statement
In an unsafe context, the 
 (
) production permits an additional construct, the 
 statement, which is used to ""fix"" a moveable variable such that its address remains constant for the duration of the statement.
Each 
 declares a local variable of the given 
 and initializes that local variable with the address computed by the corresponding 
. A local variable declared in a 
 statement is accessible in any 
s occurring to the right of that variable's declaration, and in the 
 of the 
 statement. A local variable declared by a 
 statement is considered read-only. A compile-time error occurs if the embedded statement attempts to modify this local variable (via assignment or the 
 and 
 operators) or pass it as a 
 or 
 parameter.
A 
 can be one of the following:
The token ""
"" followed by a 
 (
) to a moveable variable (
) of an unmanaged type 
, provided the type 
 is implicitly convertible to the pointer type given in the 
 statement. In this case, the initializer computes the address of the given variable, and the variable is guaranteed to remain at a fixed address for the duration of the 
 statement.
An expression of an 
 with elements of an unmanaged type 
, provided the type 
 is implicitly convertible to the pointer type given in the 
 statement. In this case, the initializer computes the address of the first element in the array, and the entire array is guaranteed to remain at a fixed address for the duration of the 
 statement. The behavior of the 
 statement is implementation-defined if the array expression is null or if the array has zero elements.
An expression of type 
, provided the type 
 is implicitly convertible to the pointer type given in the 
 statement. In this case, the initializer computes the address of the first character in the string, and the entire string is guaranteed to remain at a fixed address for the duration of the 
 statement. The behavior of the 
 statement is implementation-defined if the string expression is null.
A 
 or 
 that references a fixed size buffer member of a moveable variable, provided the type of the fixed size buffer member is implicitly convertible to the pointer type given in the 
 statement. In this case, the initializer computes a pointer to the first element of the fixed size buffer (
), and the fixed size buffer is guaranteed to remain at a fixed address for the duration of the 
 statement.
For each address computed by a 
 the 
 statement ensures that the variable referenced by the address is not subject to relocation or disposal by the garbage collector for the duration of the 
 statement. For example, if the address computed by a 
 references a field of an object or an element of an array instance, the 
 statement guarantees that the containing object instance is not relocated or disposed of during the lifetime of the statement.
It is the programmer's responsibility to ensure that pointers created by 
 statements do not survive beyond execution of those statements. For example, when pointers created by 
 statements are passed to external APIs, it is the programmer's responsibility to ensure that the APIs retain no memory of these pointers.
Fixed objects may cause fragmentation of the heap (because they can't be moved). For that reason, objects should be fixed only when absolutely necessary and then only for the shortest amount of time possible.
The example
demonstrates several uses of the 
 statement. The first statement fixes and obtains the address of a static field, the second statement fixes and obtains the address of an instance field, and the third statement fixes and obtains the address of an array element. In each case it would have been an error to use the regular 
 operator since the variables are all classified as moveable variables.
The fourth 
 statement in the example above produces a similar result to the third.
This example of the 
 statement uses 
:
In an unsafe context array elements of single-dimensional arrays are stored in increasing index order, starting with index 
 and ending with index 
. For multi-dimensional arrays, array elements are stored such that the indices of the rightmost dimension are increased first, then the next left dimension, and so on to the left. Within a 
 statement that obtains a pointer 
 to an array instance 
, the pointer values ranging from 
 to 
 represent addresses of the elements in the array. Likewise, the variables ranging from 
 to 
 represent the actual array elements. Given the way in which arrays are stored, we can treat an array of any dimension as though it were linear.
For example:
which produces the output:
In the example
a 
 statement is used to fix an array so its address can be passed to a method that takes a pointer.
In the example:
a fixed statement is used to fix a fixed size buffer of a struct so its address can be used as a pointer.
A 
 value produced by fixing a string instance always points to a null-terminated string. Within a fixed statement that obtains a pointer 
 to a string instance 
, the pointer values ranging from 
 to 
 represent addresses of the characters in the string, and the pointer value 
 always points to a null character (the character with value 
).
Modifying objects of managed type through fixed pointers can results in undefined behavior. For example, because strings are immutable, it is the programmer's responsibility to ensure that the characters referenced by a pointer to a fixed string are not modified.
The automatic null-termination of strings is particularly convenient when calling external APIs that expect ""C-style"" strings. Note, however, that a string instance is permitted to contain null characters. If such null characters are present, the string will appear truncated when treated as a null-terminated 
.
Fixed size buffers
Fixed size buffers are used to declare ""C style"" in-line arrays as members of structs, and are primarily useful for interfacing with unmanaged APIs.
Fixed size buffer declarations
A 
 is a member that represents storage for a fixed length buffer of variables of a given type. A fixed size buffer declaration introduces one or more fixed size buffers of a given element type. Fixed size buffers are only permitted in struct declarations and can only occur in unsafe contexts (
).
A fixed size buffer declaration may include a set of attributes (
), a 
 modifier (
), a valid combination of the four access modifiers (
) and an 
 modifier (
). The attributes and modifiers apply to all of the members declared by the fixed size buffer declaration. It is an error for the same modifier to appear multiple times in a fixed size buffer declaration.
A fixed size buffer declaration is not permitted to include the 
 modifier.
The buffer element type of a fixed size buffer declaration specifies the element type of the buffer(s) introduced by the declaration. The buffer element type must be one of the predefined types 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, or 
.
The buffer element type is followed by a list of fixed size buffer declarators, each of which introduces a new member. A fixed size buffer declarator consists of an identifier that names the member, followed by a constant expression enclosed in 
 and 
 tokens. The constant expression denotes the number of elements in the member introduced by that fixed size buffer declarator. The type of the constant expression must be implicitly convertible to type 
, and the value must be a non-zero positive integer.
The elements of a fixed size buffer are guaranteed to be laid out sequentially in memory.
A fixed size buffer declaration that declares multiple fixed size buffers is equivalent to multiple declarations of a single fixed size buffer declation with the same attributes, and element types. For example
is equivalent to
Fixed size buffers in expressions
Member lookup (
) of a fixed size buffer member proceeds exactly like member lookup of a field.
A fixed size buffer can be referenced in an expression using a 
 (
) or a 
 (
).
When a fixed size buffer member is referenced as a simple name, the effect is the same as a member access of the form 
, where 
 is the fixed size buffer member.
In a member access of the form 
, if 
 is of a struct type and a member lookup of 
 in that struct type identifies a fixed size member, then 
 is evaluated an classified as follows:
If the expression 
 does not occur in an unsafe context, a compile-time error occurs.
If 
 is classified as a value, a compile-time error occurs.
Otherwise, if 
 is a moveable variable (
) and the expression 
 is not a 
 (
), a compile-time error occurs.
Otherwise, 
 references a fixed variable and the result of the expression is a pointer to the first element of the fixed size buffer member 
 in 
. The result is of type 
, where 
 is the element type of 
, and is classified as a value.
The subsequent elements of the fixed size buffer can be accessed using pointer operations from the first element. Unlike access to arrays, access to the elements of a fixed size buffer is an unsafe operation and is not range checked.
The following example declares and uses a struct with a fixed size buffer member.
Definite assignment checking
Fixed size buffers are not subject to definite assignment checking (
), and fixed size buffer members are ignored for purposes of definite assignment checking of struct type variables.
When the outermost containing struct variable of a fixed size buffer member is a static variable, an instance variable of a class instance, or an array element, the elements of the fixed size buffer are automatically initialized to their default values (
). In all other cases, the initial content of a fixed size buffer is undefined.
Stack allocation
In an unsafe context, a local variable declaration (
) may include a stack allocation initializer which allocates memory from the call stack.
The 
 indicates the type of the items that will be stored in the newly allocated location, and the 
 indicates the number of these items. Taken together, these specify the required allocation size. Since the size of a stack allocation cannot be negative, it is a compile-time error to specify the number of items as a 
 that evaluates to a negative value.
A stack allocation initializer of the form 
 requires 
 to be an unmanaged type (
) and 
 to be an expression of type 
. The construct allocates 
 bytes from the call stack and returns a pointer, of type 
, to the newly allocated block. If 
 is a negative value, then the behavior is undefined. If 
 is zero, then no allocation is made, and the pointer returned is implementation-defined. If there is not enough memory available to allocate a block of the given size, a 
 is thrown.
The content of the newly allocated memory is undefined.
Stack allocation initializers are not permitted in 
 or 
 blocks (
).
There is no way to explicitly free memory allocated using 
. All stack allocated memory blocks created during the execution of a function member are automatically discarded when that function member returns. This corresponds to the 
 function, an extension commonly found in C and C++ implementations.
In the example
a 
 initializer is used in the 
 method to allocate a buffer of 16 characters on the stack. The buffer is automatically discarded when the method returns.
Dynamic memory allocation
Except for the 
 operator, C# provides no predefined constructs for managing non-garbage collected memory. Such services are typically provided by supporting class libraries or imported directly from the underlying operating system. For example, the 
 class below illustrates how the heap functions of an underlying operating system might be accessed from C#:
An example that uses the 
 class is given below:
The example allocates 256 bytes of memory through 
 and initializes the memory block with values increasing from 0 to 255. It then allocates a 256 element byte array and uses 
 to copy the contents of the memory block into the byte array. Finally, the memory block is freed using 
 and the contents of the byte array are output on the console.
Documentation comments
C# provides a mechanism for programmers to document their code using a special comment syntax that contains XML text. In source code files, comments having a certain form can be used to direct a tool to produce XML from those comments and the source code elements, which they precede. Comments using such syntax are called 
. They must immediately precede a user-defined type (such as a class, delegate, or interface) or a member (such as a field, event, property, or method). The XML generation tool is called the 
. (This generator could be, but need not be, the C# compiler itself.) The output produced by the documentation generator is called the 
. A documentation file is used as input to a 
; a tool intended to produce some sort of visual display of type information and its associated documentation.
This specification suggests a set of tags to be used in documentation comments, but use of these tags is not required, and other tags may be used if desired, as long the rules of well-formed XML are followed.
Introduction
Comments having a special form can be used to direct a tool to produce XML from those comments and the source code elements, which they precede. Such comments are single-line comments that start with three slashes (
), or delimited comments that start with a slash and two stars (
). They must immediately precede a user-defined type (such as a class, delegate, or interface) or a member (such as a field, event, property, or method) that they annotate. Attribute sections (
) are considered part of declarations, so documentation comments must precede attributes applied to a type or member.
Syntax:
In a 
, if there is a 
 character following the 
 characters on each of the 
s adjacent to the current 
, then that 
 character is not included in the XML output.
In a delimited-doc-comment, if the first non-whitespace character on the second line is an asterisk and the same pattern of optional whitespace characters and an asterisk character is repeated at the beginning of each of the line within the delimited-doc-comment, then the characters of the repeated pattern are not included in the XML output. The pattern may include whitespace characters after, as well as before, the asterisk character.
Example:
The text within documentation comments must be well formed according to the rules of XML (
http://www.w3.org/TR/REC-xml).
 If the XML is ill formed, a warning is generated and the documentation file will contain a comment saying that an error was encountered.
Although developers are free to create their own set of tags, a recommended set is defined in 
. Some of the recommended tags have special meanings:
The 
 tag is used to describe parameters. If such a tag is used, the documentation generator must verify that the specified parameter exists and that all parameters are described in documentation comments. If such verification fails, the documentation generator issues a warning.
The 
 attribute can be attached to any tag to provide a reference to a code element. The documentation generator must verify that this code element exists. If the verification fails, the documentation generator issues a warning. When looking for a name described in a 
 attribute, the documentation generator must respect namespace visibility according to 
 statements appearing within the source code. For code elements that are generic, the normal generic syntax (ie ""
"") cannot be used because it produces invalid XML. Braces can be used instead of brackets (ie ""
""), or the XML escape syntax can be used (ie ""
"").
The 
 tag is intended to be used by a documentation viewer to display additional information about a type or member.
The 
 tag includes information from an external XML file.
Note carefully that the documentation file does not provide full information about the type and members (for example, it does not contain any type information). To get such information about a type or member, the documentation file must be used in conjunction with reflection on the actual type or member.
Recommended tags
The documentation generator must accept and process any tag that is valid according to the rules of XML. The following tags provide commonly used functionality in user documentation. (Of course, other tags are possible.)
Tag
Section
Purpose
Set text in a code-like font
Set one or more lines of source code or program output
Indicate an example
Identifies the exceptions a method can throw
Includes XML from an external file
Create a list or table
Permit structure to be added to text
Describe a parameter for a method or constructor
Identify that a word is a parameter name
Document the security accessibility of a member
Describe additional information about a type
Describe the return value of a method
Specify a link
Generate a See Also entry
Describe a type or a member of a type
Describe a property
Describe a generic type parameter
Identify that a word is a type parameter name
This tag provides a mechanism to indicate that a fragment of text within a description should be set in a special font such as that used for a block of code. For lines of actual code, use 
 (
).
Syntax:
Example:
This tag is used to set one or more lines of source code or program output in some special font. For small code fragments in narrative, use 
 (
).
Syntax:
Example:
This tag allows example code within a comment, to specify how a method or other library member may be used. Ordinarily, this would also involve use of the tag 
 (
) as well.
Syntax:
Example:
See 
 (
) for an example.
This tag provides a way to document the exceptions a method can throw.
Syntax:
where
 is the name of a member. The documentation generator checks that the given member exists and translates 
 to the canonical element name in the documentation file.
 is a description of the circumstances in which the exception is thrown.
Example:
This tag allows including information from an XML document that is external to the source code file. The external file must be a well-formed XML document, and an XPath expression is applied to that document to specify what XML from that document to include. The 
 tag is then replaced with the selected XML from the external document.
Syntax:
where
 is the file name of an external XML file. The file name is interpreted relative to the file that contains the include tag.
 is an XPath expression that selects some of the XML in the external XML file.
Example:
If the source code contained a declaration like:
and the external file ""docs.xml"" had the following contents:
then the same documentation is output as if the source code contained:
This tag is used to create a list or table of items. It may contain a 
 block to define the heading row of either a table or definition list. (When defining a table, only an entry for 
 in the heading need be supplied.)
Each item in the list is specified with an 
 block. When creating a definition list, both 
 and 
 must be specified. However, for a table, bulleted list, or numbered list, only 
 need be specified.
Syntax:
where
 is the term to define, whose definition is in 
.
 is either an item in a bullet or numbered list, or the definition of a 
.
Example:
This tag is for use inside other tags, such as 
 (
) or 
 (
), and permits structure to be added to text.
Syntax:
where 
 is the text of the paragraph.
Example:
This tag is used to describe a parameter for a method, constructor, or indexer.
Syntax:
where
 is the name of the parameter.
 is a description of the parameter.
Example:
This tag is used to indicate that a word is a parameter. The documentation file can be processed to format this parameter in some distinct way.
Syntax:
where 
 is the name of the parameter.
Example:
This tag allows the security accessibility of a member to be documented.
Syntax:
where
 is the name of a member. The documentation generator checks that the given code element exists and translates 
member
 to the canonical element name in the documentation file.
 is a description of the access to the member.
Example:
This tag is used to specify extra information about a type. (Use 
 (
) to describe the type itself and the members of a type.)
Syntax:
where 
 is the text of the remark.
Example:
This tag is used to describe the return value of a method.
Syntax:
where 
 is a description of the return value.
Example:
This tag allows a link to be specified within text. Use 
 (
) to indicate text that is to appear in a See Also section.
Syntax:
where 
 is the name of a member. The documentation generator checks that the given code element exists and changes 
member
 to the element name in the generated documentation file.
Example:
This tag allows an entry to be generated for the See Also section. Use 
 (
) to specify a link from within text.
Syntax:
where 
 is the name of a member. The documentation generator checks that the given code element exists and changes 
member
 to the element name in the generated documentation file.
Example:
This tag can be used to describe a type or a member of a type. Use 
 (
) to describe the type itself.
Syntax:
where 
 is a summary of the type or member.
Example:
This tag allows a property to be described.
Syntax:
where 
 is a description for the property.
Example:
This tag is used to describe a generic type parameter for a class, struct, interface, delegate, or method.
Syntax:
where 
 is the name of the type parameter, and 
 is its description.
Example:
This tag is used to indicate that a word is a type parameter. The documentation file can be processed to format this type parameter in some distinct way.
Syntax:
where 
 is the name of the type parameter.
Example:
Processing the documentation file
The documentation generator generates an ID string for each element in the source code that is tagged with a documentation comment. This ID string uniquely identifies a source element. A documentation viewer can use an ID string to identify the corresponding metadata/reflection item to which the documentation applies.
The documentation file is not a hierarchical representation of the source code; rather, it is a flat list with a generated ID string for each element.
ID string format
The documentation generator observes the following rules when it generates the ID strings:
No white space is placed in the string.
The first part of the string identifies the kind of member being documented, via a single character followed by a colon. The following kinds of members are defined:
Character
Description
E
Event
F
Field
M
Method (including constructors, destructors, and operators)
N
Namespace
P
Property (including indexers)
T
Type (such as class, delegate, enum, interface, and struct)
!
Error string; the rest of the string provides information about the error. For example, the documentation generator generates error information for links that cannot be resolved.
The second part of the string is the fully qualified name of the element, starting at the root of the namespace. The name of the element, its enclosing type(s), and namespace are separated by periods. If the name of the item itself has periods, they are replaced by 
 characters. (It is assumed that no element has this character in its name.)
For methods and properties with arguments, the argument list follows, enclosed in parentheses. For those without arguments, the parentheses are omitted. The arguments are separated by commas. The encoding of each argument is the same as a CLI signature, as follows:
Arguments are represented by their documentation name, which is based on their fully qualified name, modified as follows:
Arguments that represent generic types have an appended ""'"" character followed by the number of type parameters
Arguments having the 
 or 
 modifier have an 
 following their type name. Arguments passed by value or via 
 have no special notation.
Arguments that are arrays are represented as 
 where the number of commas is the rank less one, and the lower bounds and size of each dimension, if known, are represented in decimal. If a lower bound or size is not specified, it is omitted. If the lower bound and size for a particular dimension are omitted, the ""
"" is omitted as well. Jagged arrays are represented by one ""
"" per level.
Arguments that have pointer types other than void are represented using a 
 following the type name. A void pointer is represented using a type name of 
.
Arguments that refer to generic type parameters defined on types are encoded using the ""`"" character followed by the zero-based index of the type parameter.
Arguments that use generic type parameters defined in methods use a double-backtick ""``"" instead of the ""`"" used for types.
Arguments that refer to constructed generic types are encoded using the generic type, followed by ""{"", followed by a comma-separated list of type arguments, followed by ""}"".
ID string examples
The following examples each show a fragment of C# code, along with the ID string produced from each source element capable of having a documentation comment:
Types are represented using their fully qualified name, augmented with generic information:
Fields are represented by their fully qualified name:
Constructors.
Destructors.
Methods.
Properties and indexers.
Events.
Unary operators.
The complete set of unary operator function names used is as follows: 
, 
, 
, 
, 
, 
, 
, and 
.
Binary operators.
The complete set of binary operator function names used is as follows: 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, 
, and 
.
Conversion operators have a trailing ""
"" followed by the return type.
An example
C# source code
The following example shows the source code of a 
 class:
Resulting XML
Here is the output produced by one documentation generator when given the source code for class 
, shown above:
";
}

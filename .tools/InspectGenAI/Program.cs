using System;
using System.Linq;
using System.Reflection;

var nuget = "/Users/nuthanreddysurukunti/.nuget/packages";
var dll = Assembly.LoadFrom($"{nuget}/google.genai/1.6.1/lib/net8.0/Google.GenAI.dll");

var t = dll.GetType("Google.GenAI.Types.Model");
Console.WriteLine($"=== Google.GenAI.Types.Model ===");
foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    Console.WriteLine($"  prop: {prop.PropertyType.Name} {prop.Name}");

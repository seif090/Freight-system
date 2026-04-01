using System;
using Microsoft.OpenApi;
class Program { static void Main(){ var wt = typeof(OpenApiOperation).Assembly.GetType("Microsoft.OpenApi.IOpenApiWriter"); Console.WriteLine(wt); foreach(var m in wt.GetMethods()){ Console.WriteLine(m.Name + ": " + string.Join(",", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name))); }}}

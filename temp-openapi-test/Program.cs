using System;
using Microsoft.OpenApi;
class Program { static void Main() { var a = typeof(OpenApiOperation).Assembly; foreach(var t in a.GetTypes()) { if (t.Namespace!=null && t.Namespace.StartsWith("Microsoft.OpenApi")) Console.WriteLine(t.FullName);} }}

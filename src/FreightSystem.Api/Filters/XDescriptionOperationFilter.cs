using System;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FreightSystem.Api.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class XDescriptionAttribute : Attribute
    {
        public string Summary { get; }
        public string Arabic { get; }

        public XDescriptionAttribute(string summary, string arabic)
        {
            Summary = summary;
            Arabic = arabic;
        }
    }

    public class XDescriptionOperationFilter : IOperationFilter
    {
        public void Apply(Microsoft.OpenApi.OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttributes(true).OfType<XDescriptionAttribute>().FirstOrDefault();
            if (attr == null) return;

            operation.Description = attr.Summary + "\n" + attr.Arabic;
            operation.Extensions["x-description"] = new OpenApiStringExtension(attr.Summary, attr.Arabic);
        }
    }

    public class OpenApiStringExtension : IOpenApiExtension
    {
        private readonly string _en;
        private readonly string _ar;

        public OpenApiStringExtension(string en, string ar)
        {
            _en = en;
            _ar = ar;
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("en");
            writer.WriteValue(_en);
            writer.WritePropertyName("ar");
            writer.WriteValue(_ar);
            writer.WriteEndObject();
        }
    }
}

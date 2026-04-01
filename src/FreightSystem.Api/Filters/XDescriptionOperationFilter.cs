using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context.MethodInfo.GetCustomAttributes(true).OfType<XDescriptionAttribute>().FirstOrDefault();
            if (attr == null) return;

            operation.Description = attr.Summary + "\n" + attr.Arabic;
            operation.Extensions["x-description"] = new OpenApiObject
            {
                ["en"] = new OpenApiString(attr.Summary),
                ["ar"] = new OpenApiString(attr.Arabic)
            };
        }
    }
}

using FreightSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreightSystem.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/assistant")]
    [ApiVersion("1.0")]
    public class AssistantController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        [HttpPost("execute")]
        [Authorize(Policy = "OperationPolicy")]
        public async Task<IActionResult> Execute([FromBody] AssistantRequest request)
        {
            var result = await _assistantService.ExecuteAsync(request);
            return Ok(result);
        }

        [HttpPost("webhook")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Webhook([FromQuery] string type, [FromBody] object payload)
        {
            var result = await _assistantService.ProcessWebhookCommandAsync(type, payload);
            return Ok(result);
        }
    }
}

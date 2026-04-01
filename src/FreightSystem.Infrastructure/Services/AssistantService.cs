using FreightSystem.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace FreightSystem.Infrastructure.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly List<string> _history = new();

        public Task<AssistantResponse> ExecuteAsync(AssistantRequest request)
        {
            // Simple context window + rule engine
            if (!string.IsNullOrEmpty(request.ContextHash))
            {
                _history.Add(request.ContextHash);
                if (_history.Count > 20) _history.RemoveAt(0);
            }

            var input = request.Input.Trim().ToLowerInvariant();
            var response = new AssistantResponse { Success = true };

            if (input.Contains("reroute"))
            {
                response.Command = "reroute";
                response.Results = "Route optimization triggered.";
                response.NextAction = "call /api/v1.0/advancedoperations/shipments/{id}/optimize-route";
            }
            else if (input.Contains("gantt"))
            {
                response.Command = "establish_gantt";
                response.Results = "Project timeline created in the scheduling engine.";
                response.NextAction = "navigate to /analytics/operations-cockpit";
            }
            else if (input.Contains("crew"))
            {
                response.Command = "crew_dispatch";
                response.Results = "Dispatch team assigned to the latest maintenance alert.";
                response.NextAction = "monitor /api/v1.0/advancedoperations/dispatch-actions";
            }
            else
            {
                response.Command = "chat";
                response.Results = "I can perform reroute, establish Gantt, or crew dispatch commands. Please specify.";
                response.NextAction = "Speak in plain text or type 'reroute'.";
            }

            return Task.FromResult(response);
        }

        public Task<AssistantResponse> ProcessWebhookCommandAsync(string webhookType, object payload)
        {
            var response = new AssistantResponse { Success = true };
            response.Command = webhookType;
            response.Results = "Received webhook command: " + webhookType;

            switch (webhookType.ToLowerInvariant())
            {
                case "reroute":
                    response.NextAction = "optimization queue post-processed";
                    break;
                case "establish_gantt":
                    response.NextAction = "schedule milestones updated";
                    break;
                case "crew_dispatch":
                    response.NextAction = "crew alerted";
                    break;
                default:
                    response.NextAction = "unknown webhook";
                    response.Success = false;
                    break;
            }

            return Task.FromResult(response);
        }
    }
}

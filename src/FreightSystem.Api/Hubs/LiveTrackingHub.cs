using Microsoft.AspNetCore.SignalR;

namespace FreightSystem.Api.Hubs;

public class LiveTrackingHub : Hub
{
    public async Task SubscribeToShipment(int shipmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetShipmentGroupName(shipmentId));
    }

    public async Task UnsubscribeFromShipment(int shipmentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetShipmentGroupName(shipmentId));
    }

    public async Task SubscribeToDispatchers() 
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dispatchers");
    }

    public async Task UnsubscribeFromDispatchers()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Dispatchers");
    }

    private static string GetShipmentGroupName(int shipmentId) => $"Shipment_{shipmentId}";
}

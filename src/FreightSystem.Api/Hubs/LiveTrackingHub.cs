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

    private static string GetShipmentGroupName(int shipmentId) => $"Shipment_{shipmentId}";
}

using Microsoft.AspNetCore.SignalR;

namespace ResilienciaNorte.Web.Hubs
{
    public class EmergenciaHub : Hub
    {
        public async Task SuscribirDistrito(int distritoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Distrito_{distritoId}");
        }

        public async Task DesuscribirDistrito(int distritoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Distrito_{distritoId}");
        }
    }
}
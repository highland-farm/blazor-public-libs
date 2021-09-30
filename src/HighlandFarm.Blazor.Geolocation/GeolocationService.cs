using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.JSInterop;

namespace HighlandFarm.Blazor.Geolocation
{
    public class GeolocationService : IAsyncDisposable
    {
        private const string ModuleFile = "./_content/HighlandFarm.Blazor.Geolocation/tslib.esm.min.js";
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public GeolocationService(IJSRuntime jsRuntime)
        {
            // deferred loading of scanner JavaScript lib
            moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", ModuleFile).AsTask());
        }

        public async ValueTask<GeoPosition> GetCurrentPosition(
            bool highAccuracy = false,
            TimeSpan? timeout = null,
            TimeSpan? maximumAge = null)
        {
            var module = await moduleTask.Value;

            return await module.InvokeAsync<GeoPosition>("BlazorGeo.getPosition", highAccuracy, timeout?.TotalMilliseconds, maximumAge?.TotalMilliseconds);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}

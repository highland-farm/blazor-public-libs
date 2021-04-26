using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HighlandFarm.Blazor.BarcodeScanner
{
    public class SimpleQuaggaInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public SimpleQuaggaInterop(IJSRuntime jsRuntime)
        {
            // deferred loading of scanner JavaScript lib
            moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/HighlandFarm.Blazor.BarcodeScanner/tslib.esm.min.js").AsTask());
        }

        private async ValueTask<TValue> CallJs<TValue>(string identifier, params object[] args)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<TValue>(identifier, args);
        }

        private async ValueTask CallJsVoid(string identifier, params object[] args)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync(identifier, args);
        }

        public async ValueTask Init(ElementReference viewport) => await CallJsVoid("init", viewport);

        public async ValueTask Start() => await CallJsVoid("start");

        public async ValueTask Stop() => await CallJsVoid("stop");

        public async ValueTask Hide() => await CallJsVoid("hide");

        public async ValueTask Show() => await CallJsVoid("show");


        public async ValueTask<string> ScanCode() => await CallJs<string>("scanCode");

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await Stop();
                await module.DisposeAsync();
            }
        }
    }
}

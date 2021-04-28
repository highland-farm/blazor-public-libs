using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;

namespace HighlandFarm.Blazor.BarcodeScanner
{
    internal class SimpleQuaggaInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private readonly Lazy<Task<IJSUnmarshalledObjectReference>> unmarshalledModuleTask;

        public SimpleQuaggaInterop(IJSRuntime jsRuntime)
        {
            // deferred loading of scanner JavaScript lib
            moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/HighlandFarm.Blazor.BarcodeScanner/tslib.esm.min.js").AsTask());
            unmarshalledModuleTask = new (async () => GetUnmarshalledModule(jsRuntime, await moduleTask.Value));
        }

        // https://github.com/dotnet/aspnetcore/issues/26588#issuecomment-735932257
        // hack needed for now; this is likely fixed in .NET 6
        public static IJSUnmarshalledObjectReference GetUnmarshalledModule(IJSRuntime jsRuntime, IJSObjectReference module)
        {
            if (module is IJSUnmarshalledObjectReference uModule) return uModule;

            var wasm = typeof(WebAssemblyJSRuntime).Assembly;

            var moduleId = module.GetType()
                .GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(module);

            return wasm.GetType("Microsoft.JSInterop.WebAssembly.WebAssemblyJSObjectReference")
                .GetConstructor(new[] { typeof(WebAssemblyJSRuntime), typeof(long) })
                .Invoke(new[] { jsRuntime, moduleId }) as IJSUnmarshalledObjectReference;
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

        public async ValueTask Init(ElementReference viewport) => await CallJsVoid("SimpleQuaggaWrapper.init", viewport);

        public async ValueTask Start() => await CallJsVoid("SimpleQuaggaWrapper.start");

        public async ValueTask Stop() => await CallJsVoid("SimpleQuaggaWrapper.stop");

        public async ValueTask Hide() => await CallJsVoid("SimpleQuaggaWrapper.hide");

        public async ValueTask Show() => await CallJsVoid("SimpleQuaggaWrapper.show");

        public async ValueTask<string> ScanCode() => await CallJs<string>("SimpleQuaggaWrapper.scanCode");

        public async Task<byte[]> GetImage() => (await unmarshalledModuleTask.Value).InvokeUnmarshalled<byte[]>("SimpleQuaggaWrapper.getImageUnmarshalled");

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

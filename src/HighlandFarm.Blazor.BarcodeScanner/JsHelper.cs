using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;

namespace HighlandFarm.Blazor.BarcodeScanner
{
    public class JsHelper : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private readonly Lazy<Task<IJSUnmarshalledObjectReference>> unmarshalledModuleTask;

        public JsHelper(IJSRuntime jsRuntime, string moduleFile)
        {
            // deferred loading of scanner JavaScript lib
            moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", moduleFile).AsTask());

            // unmarshalled version for higher performance interop
            // NOTE: currently no fallback (this breaks) for non-WebAssembly i.e. server-side Blazor
            unmarshalledModuleTask = new (async () => GetUnmarshalledModule(jsRuntime, await moduleTask.Value));
        }

        // https://github.com/dotnet/aspnetcore/issues/26588#issuecomment-735932257
        // hack needed for now; this is likely fixed in .NET 6
        private static IJSUnmarshalledObjectReference GetUnmarshalledModule(IJSRuntime jsRuntime, IJSObjectReference module)
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

        // deferred module wrappers for JS interop

        public async ValueTask<TValue> CallJs<TValue>(string identifier, params object[] args)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<TValue>(identifier, args);
        }

        public async ValueTask CallJsVoid(string identifier, params object[] args)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync(identifier, args);
        }

        public async ValueTask<TResult> CallJsUnmarshalled<TResult>(string identifier)
        {
            var module = await unmarshalledModuleTask.Value;
            return module.InvokeUnmarshalled<TResult>(identifier);
        }

        public async ValueTask<TResult> CallJsUnmarshalled<T0, TResult>(string identifier, T0 arg0)
        {
            var module = await unmarshalledModuleTask.Value;
            return module.InvokeUnmarshalled<T0, TResult>(identifier, arg0);
        }

        public async ValueTask<TResult> CallJsUnmarshalled<T0, T1, TResult>(string identifier, T0 arg0, T1 arg1)
        {
            var module = await unmarshalledModuleTask.Value;
            return module.InvokeUnmarshalled<T0, T1, TResult>(identifier, arg0, arg1);
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

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HighlandFarm.Blazor.BarcodeScanner
{
    public abstract class JsComponentBase : ComponentBase, IAsyncDisposable
    {
        private const string ModuleFile = "./_content/HighlandFarm.Blazor.BarcodeScanner/tslib.esm.min.js";

        // create JsHelper when framework injects JSRuntime dependency
        [Inject]
        protected IJSRuntime JSRuntime { set => JsLib = new JsHelper(value, ModuleFile); }

        protected JsHelper JsLib { get; private set; }

        // dispose of module
        public virtual async ValueTask DisposeAsync()
        {
            if (JsLib != null)
                await JsLib.DisposeAsync();
            JsLib = null;
        }
    }
}

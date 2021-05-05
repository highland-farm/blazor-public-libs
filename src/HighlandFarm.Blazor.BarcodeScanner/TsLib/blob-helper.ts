// https://stackoverflow.com/a/59025746
// https://stackoverflow.com/a/30881444


declare var BINDING: any;
declare var Blazor: any;

// FIXME: try to use TS type definition... blob is actually a System_Array<byte> etc
/**
 * Create a Blob object and return an ObjectURL reference using Blazor unmarshalled interop for better performance.
 * @param contentTypeRaw Mime type of blob, ex. 'image/png'.
 * @param blobRaw Blob bytes as a .NET byte[].
 * @returns String ObjectURL for accessing blob within DOM (remember to free after use).
 */
export function buildObjectURLUnmarshalled(contentTypeRaw: string, blobRaw: any): string
{
  // https://github.com/dotnet/aspnetcore/blob/v5.0.5/src/Components/Web.JS/src/Platform/Mono/MonoTypes.ts
  const contentType: string = BINDING.conv_string(contentTypeRaw);

  // https://github.com/dotnet/aspnetcore/blob/v5.0.5/src/Components/Web.JS/src/Platform/Mono/MonoPlatform.ts
  // https://stackoverflow.com/a/44148694
  const blob = new Blob([Blazor.platform.toUint8Array(blobRaw)]);

  // NOTE: make sure to call URL.revokeObjectURL to free memory after image is loaded into the DOM!
  const url = URL.createObjectURL(blob);

  // https://github.com/mono/mono/blob/main/sdks/wasm/src/binding_support.js
  return BINDING.js_string_to_mono_string(url);
}

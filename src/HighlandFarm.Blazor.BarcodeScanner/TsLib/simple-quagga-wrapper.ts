import {
  BarcodeScannerBuilder,
  BarcodeScanner,
  ReaderType,
  ScanResult,
} from "@highlandfarm/simple-quagga";

let scanner: BarcodeScanner | undefined = undefined;

/**
 * Initialize internal SimpleQuagga BarcodeScanner.
 * @param viewport Viewport element reference, ex. a container div.
 */
export function init(viewport: HTMLElement): void {
  if (scanner) {
    return;
  }

  scanner = new BarcodeScannerBuilder(viewport)
    .addReader(ReaderType.CODE_128)
    .withResultImages()
    .withDrawLocated()
    .withDrawDetected()
    .withDrawScanline()
    .build();
}

/**
 * Setup scanner and start camera video stream (without locating or detecting).
 * @returns Promise that resolves after camera video stream is running.
 */
export async function start(): Promise<void> {
  return await scanner?.start();
}

/**
 * Stop camera video stream (can be restarted).
 * @returns Promise that resolves after camera video stream is stopped.
 */
export async function stop(): Promise<void> {
  return await scanner?.stop();
}

export function hide(): void {
  return scanner?.hide();
}

export function show(): void {
  return scanner?.show();
}

interface ImageData {
  mimeType: string,
  data: Uint8Array,
}
let images: Record<string, ImageData> = {};
let overlays: Record<string, ImageData> = {};

async function packBlob(blob: Blob): Promise<ImageData> {
  let bytes = await blob.arrayBuffer();
  return { mimeType: blob.type, data: new Uint8Array(bytes) };
}

// provided by blazor interop
declare var BINDING: any;

// remote and return a blob record from an internal record set
function getBlobUnnmarshalled(scanIdRaw: string, records: Record<string, ImageData>): any {
  // convert from .NET to JS string
  const scanId = BINDING.conv_string(scanIdRaw);

  if (!records.hasOwnProperty(scanId)) {
    return undefined;
  }

  // for now we ignore mimeType (only allow default 'image/png')
  const dotNetBlob = BINDING.js_typed_array_to_array(records[scanId].data);
  delete records[scanId];
  return dotNetBlob;
}

/**
 * Get an image blob using Blazor unmarshalled interop for better performance.
 * @param scanId Temporary ID passed to scanCode method when scan was requested.
 * @returns Blob bytes as a .NET byte array, or undefined if not found.
 */
export function getImageUnmarshalled(scanId: string): any {
  return getBlobUnnmarshalled(scanId, images);
}

/**
 * Get an overlay blob using Blazor unmarshalled interop for better performance.
 * @param scanId Temporary ID passed to scanCode method when scan was requested.
 * @returns Blob bytes as a .NET byte array, or undefined if not found.
 */
export function getOverlayUnmarshalled(scanId: string): any {
  return getBlobUnnmarshalled(scanId, overlays);
}

/**
 * Request a barcode scan. Scanner must be started (video is streaming).
 * @param scanId Temporary ID to use when retrieving stored images.
 * @returns Promise that resolves with code when it is detected (and validated if configured).
 */
export async function scanCode(scanId?: string): Promise<string | undefined> {
  if (!scanner) {
    return undefined;
  }

  // request ScanResult
  let r = await scanner?.scanCode();

  // capture and transfer blobs (image & overlay) into internal record sets, if requested
  let blobPromises = [];
  if (scanId && r.image) {
    blobPromises.push(r.image.then(packBlob).then((v: ImageData) => images[scanId] = v));
  }
  if (scanId && r.overlay) {
    blobPromises.push(r.overlay.then(packBlob).then((v: ImageData) => overlays[scanId] = v));
  }
  await Promise.all(blobPromises);

  return r.code;
}

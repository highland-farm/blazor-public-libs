import {
  BarcodeScannerBuilder,
  BarcodeScanner,
  ReaderType,
  ScanResult,
} from "@highlandfarm/simple-quagga";

let scanner: BarcodeScanner | undefined = undefined;

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

export async function start(): Promise<void> {
  return await scanner?.start();
}

export async function stop(): Promise<void> {
  return await scanner?.stop();
}

export function hide(): void {
  return scanner?.hide();
}

export function show(): void {
  return scanner?.show();
}

let images: ImageData[] = [];
let overlays: ImageData[] = [];

interface ImageData {
  mimeType: string,
  data: Uint8Array,
}
async function packBlob(blob: Blob): Promise<ImageData> {
  let bytes = await blob.arrayBuffer();
  return { mimeType: blob.type, data: new Uint8Array(bytes) };
}

declare var BINDING: any;
export function getImageUnmarshalled(): any {
  // FIXME: hack hack hack hack
  if (images.length == 0 && overlays.length == 0) {
    return null;
  }
  return BINDING.js_typed_array_to_array((images.shift() ?? overlays.shift())?.data);
}

export async function scanCode(): Promise<string | undefined> {
  if (!scanner) {
    return undefined;
  }

  let r = await scanner?.scanCode();
  await Promise.all([
    r.image?.then(packBlob).then((v) => images.push(v)),
    r.overlay?.then(packBlob).then((v) => overlays.push(v))]);

  return r.code;
}

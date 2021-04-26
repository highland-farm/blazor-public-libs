import {
  BarcodeScannerBuilder,
  BarcodeScanner,
  ReaderType,
} from "@highlandfarm/simple-quagga";

let scanner: BarcodeScanner | undefined = undefined;

export function init(viewport: HTMLElement): void {
  if (scanner) {
    return;
  }

  scanner = new BarcodeScannerBuilder(viewport)
    .addReader(ReaderType.CODE_128)
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

export async function scanCode(): Promise<string | undefined> {
  return await scanner?.scanCode().then(r => r.code);
}

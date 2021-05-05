# Blazor Barcode Scanner Lib

Early work in progress.

BarcodeScanner RCL builds an internal JS bundle (TypeScript) that includes [simple-quagga](https://github.com/highland-farm/simple-quagga) as a dependency.

1. ScannerViewport component - scans barcodes from live camera view
1. ImageScan component - displays previous scans & overlays

This project demonstrates including JS as part of a Razor class lib, along with using it in components with Blazor JS isolation. Also shows how to use unmarshalled interop to move byte arrays between Blazor and JS faster - in this case the captured barcode image as a blob.

WASM example uses this RCL to demo scanning barcodes (currently hardcoded to code 128) and displaying the codes and captured images as a list.

export function hasGeo(): boolean {
  return navigator.geolocation != null;
}

export interface GeoPosition {
  ErrorCode?: number;
  ErrorMessage?: string;
  Timestamp?: number;
  Latitude?: number;
  Longitude?: number;
  Accuracy?: number;
  Altitude?: number | null;
  AltitudeAccuracy?: number | null;
  Heading?: number | null;
  Speed?: number | null;
}

export function getPosition(
  enableHighAccuracy?: boolean,
  timeout?: number,
  maximumAge?: number
): Promise<GeoPosition> {
  const options: PositionOptions = {
    enableHighAccuracy,
    timeout: timeout === -1 ? Infinity : timeout,
    maximumAge: maximumAge === -1 ? Infinity : maximumAge,
  };

  // blazor doesn't play nice with promise rejections and exceptions yet
  return new Promise((resolve) => {
    if (!hasGeo)
      return {
        ErrorCode: 0,
        ErrorMessage: "Browser does not implement the geolocation API.",
      };
    navigator.geolocation.getCurrentPosition(
      (p) => {
        resolve({
          Timestamp: p.timestamp,
          Latitude: p.coords.latitude,
          Longitude: p.coords.longitude,
          Accuracy: p.coords.accuracy,
          Altitude: p.coords.altitude,
          AltitudeAccuracy: p.coords.altitudeAccuracy,
          Heading: p.coords.heading,
          Speed: p.coords.speed,
        });
      },
      (e) => {
        resolve({ ErrorCode: e.code, ErrorMessage: e.message });
      },
      options
    );
  });
}

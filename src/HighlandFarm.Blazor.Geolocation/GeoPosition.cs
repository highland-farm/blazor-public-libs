using System;

namespace HighlandFarm.Blazor.Geolocation
{
    public record GeoPosition
    {
        public ushort? ErrorCode { get; init; } 
        public string ErrorMessage { get; init; }
        public long? Timestamp { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public double? Accuracy { get; init; }
        public double? Altitude { get; init; }
        public double? AltitudeAccuracy { get; init; }
        public double? Heading { get; init; }
        public double? Speed { get; init; }

        public bool IsError => ErrorCode != null;
        public DateTime? DateTime =>
            Timestamp.HasValue
                ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(Timestamp.Value)
                : null;
        public string GMapsUrl =>
            (!IsError & Latitude.HasValue && Longitude.HasValue)
                ? $"https://www.google.com/maps/search/?api=1&query={Latitude},{Longitude}"
                : null;
    }
}

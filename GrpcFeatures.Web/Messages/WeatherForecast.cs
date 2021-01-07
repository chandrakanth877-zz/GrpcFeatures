
using Google.Protobuf.WellKnownTypes;
using System;

namespace Weather
{
    public partial class WeatherForecast
    {

        public DateTime DateTime
        {
            get => Date.ToDateTime();
            set { Date = Timestamp.FromDateTime(value.ToUniversalTime()); }
        }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}

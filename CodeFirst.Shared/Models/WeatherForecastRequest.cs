using System.Runtime.Serialization;

namespace CodeFirst.Shared
{
    [DataContract]
    public class WeatherForecastRequest
    {
        [DataMember(Order = 1)]
        public int ReturnCount { get; set; }
    }
}

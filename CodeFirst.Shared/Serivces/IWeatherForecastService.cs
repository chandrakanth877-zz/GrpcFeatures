using ProtoBuf.Grpc;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CodeFirst.Shared.Serivces
{
    [ServiceContract]
    public interface IWeatherForecastService
    {
        [OperationContract]
        Task<List<WeatherForecasrResponse>> GetWeatherForecasts(WeatherForecastRequest request, CallContext context = default);
    }
}

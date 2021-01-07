using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Reflection.V1Alpha;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static Weather.WeatherForecasts;
using ServerReflectionClient = Grpc.Reflection.V1Alpha.ServerReflection.ServerReflectionClient;


namespace GrpcFeatures.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:6001");

            //await InterceptorMethod(channel);

            //await ExceptionHandling(channel);

            //await ServiceReflection(channel);

            await HealthCheck(channel);

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private async static Task HealthCheck(GrpcChannel channel)
        {
            var client = new Health.HealthClient(channel);
            Console.WriteLine("Watching health status");
            Console.WriteLine("Press any key to exit...");

            var cts = new CancellationTokenSource();
            using var call = client.Watch(new HealthCheckRequest { Service = "HealthCheck" }, cancellationToken: cts.Token);
            var watchTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"{DateTime.Now}: Service is {message.Status}");
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine(ex.Status.Detail);
                }
            });

            Console.ReadKey();
            Console.WriteLine("Finished");

            cts.Cancel();
            await watchTask;
        }

        private async static Task InterceptorMethod(GrpcChannel channel)
        {
            var invoker = channel.Intercept(new ClientLoggerInterceptor());
            var client = new WeatherForecastsClient(invoker);
            var result = await client.GetWeatherForecastsAsync(new Weather.GetWeatherForecastsRequest { ReturnCount = 100 });
        }

        private async static Task ExceptionHandling(GrpcChannel channel)
        {
            var client = new WeatherForecastsClient(channel);
            try
            {
                await client.GetWeatherForecastsAsync(new Weather.GetWeatherForecastsRequest { ReturnCount = 1000000 });
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }
        }

        public async static Task ServiceReflection(GrpcChannel channel)
        {
            var client = new ServerReflectionClient(channel);

            Console.WriteLine("Calling reflection service:");
            var response = await SingleRequestAsync(client, new ServerReflectionRequest
            {
                ListServices = "" // Get all services
            });

            Console.WriteLine("Services:");
            foreach (var item in response.ListServicesResponse.Service)
            {
                Console.WriteLine("- " + item.Name);
            }
        }

        private static async Task<ServerReflectionResponse> SingleRequestAsync(ServerReflectionClient client, ServerReflectionRequest request)
        {
            using var call = client.ServerReflectionInfo();
            await call.RequestStream.WriteAsync(request);
            Debug.Assert(await call.ResponseStream.MoveNext());

            var response = call.ResponseStream.Current;
            await call.RequestStream.CompleteAsync();
            return response;
        }
    }

    public class ClientLoggerInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            var call = continuation(request, context);

            return new AsyncUnaryCall<TResponse>(HandleResponse(call.ResponseAsync), call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> t)
        {
            try
            {
                var response = await t;
                Console.WriteLine($"Response received: {response}");
                return response;
            }
            catch (Exception ex)
            {
                // Log error to the console.
                // Note: Configuring .NET Core logging is the recommended way to log errors
                // https://docs.microsoft.com/aspnet/core/grpc/diagnostics#grpc-client-logging
                var initialColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Call error: {ex.Message}");
                Console.ForegroundColor = initialColor;

                throw;
            }
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(context);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(request, context);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(context);
        }

        private void LogCall<TRequest, TResponse>(Method<TRequest, TResponse> method)
            where TRequest : class
            where TResponse : class
        {
            var initialColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Starting call. Type: {method.Type}. Request: {typeof(TRequest)}. Response: {typeof(TResponse)}");
            Console.ForegroundColor = initialColor;
        }

        private void AddCallerMetadata<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var headers = context.Options.Headers;

            // Call doesn't have a headers collection to add to.
            // Need to create a new context with headers for the call.
            if (headers == null)
            {
                headers = new Metadata();
                var options = context.Options.WithHeaders(headers);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
            }

            // Add caller metadata to call headers
            headers.Add("caller-user", Environment.UserName);
            headers.Add("caller-machine", Environment.MachineName);
            headers.Add("caller-os", Environment.OSVersion.ToString());
        }
    }

}

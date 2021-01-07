using GrpcFeatures.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcFeatures.Service
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddGrpc(o =>
            {
                o.Interceptors.Add<ServerLoggerInterceptor>();
            });
            services.AddGrpcHealthChecks()
                            .AddAsyncCheck("HealthCheck", () =>
                            {

                                var result = VerifyDbConnection()
                                    ? HealthCheckResult.Unhealthy()
                                    : HealthCheckResult.Healthy();

                                return Task.FromResult(result);
                            }, Array.Empty<string>());

            services.AddGrpcReflection();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles();

            app.UseRouting();

            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<WeatherService>().EnableGrpcWeb();
                endpoints.MapGrpcReflectionService();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private bool VerifyDbConnection()
        {
            var r = new Random().Next();
            Console.WriteLine(r);
            return r % 5 == 0;
        }
    }
}

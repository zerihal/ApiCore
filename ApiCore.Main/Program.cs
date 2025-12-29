using ApiCore.Common.Interfaces;
using ApiCore.Main.ApiKey;
using ApiCore.Main.Interfaces;
using System.Text.Json.Serialization;

namespace ApiCore.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add local controllers (if any) first.
            var mvcBuilder = builder.Services.AddControllers();

            // Get API modules from referenced assemblies and add these to the MVC builder.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetTypes().Any(t => 
                typeof(IApiModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)).ToArray();

            foreach (var assembly in assemblies)
            {
                mvcBuilder.AddApplicationPart(assembly).AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

                // There should only be one IApiModule implementation per assembly. Load this so that
                // the service can be used within the main controller.
                var moduleType = assembly.GetTypes().FirstOrDefault(t => typeof(IApiModule).IsAssignableFrom(t) && 
                    !t.IsInterface && !t.IsAbstract);

                // Add services to the container.
                if (moduleType != null)
                    builder.Services.AddTransient(typeof(IApiModule), moduleType);
            }

            // Register API key store.
            builder.Services.AddTransient<IApiKeyStore, SqliteApiKeyStore>();

            builder.Services.AddEndpointsApiExplorer(); 
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            // Uncomment the below to enable API key checking middleware. Note that this requires a database to be setup
            // with at least one valid API key (see ApiKeyGenerator project - to be packaged or integrated in future).
            //app.UseMiddleware<ApiKeyMiddleware>();
            app.MapControllers();
            app.Run();
        }
    }
}

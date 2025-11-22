
using ApiCore.Common.Interfaces;
using System.Text.Json.Serialization;

namespace ApiCore.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

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
            }

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
            app.MapControllers();
            app.Run();
        }
    }
}


using ApiCore.Common.Interfaces;

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
                mvcBuilder.AddApplicationPart(assembly);

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}

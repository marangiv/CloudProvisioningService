using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProvisioningFramework.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AWSCommon;

namespace CloudProvisioningService
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
          Version = "v1",
          Title = "Cloud Provisioning Service API",
          Description = "API for Cloud Provisioning Service"
        });

        // Include XML comments if available
        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
          c.IncludeXmlComments(xmlPath);
        }
      });

      // Loads plugin assemblies from the "plugins" folder.
      string pluginFolder = Path.Combine(AppContext.BaseDirectory, "plugins");

      IEnumerable<Assembly> pluginAssemblies = Directory.GetFiles(pluginFolder, "*.dll")
          .Select(file =>
          {
            try
            {
              return Assembly.LoadFrom(file);
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Failed to load assembly {file}: {ex.Message}");
              return null;
            }
          })
          .Where(assembly => assembly != null);

      IEnumerable<IPlugin> plugins = pluginAssemblies
          .SelectMany(a =>
          {
            try
            {
              return a.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
              Console.WriteLine($"Failed to get types from assembly {a.FullName}: {ex.Message}");
              return Enumerable.Empty<Type>();
            }
          })
          .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
          .Select(t =>
          {
            try
            {
              return Activator.CreateInstance(t) as IPlugin;
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Failed to create instance of {t.FullName}: {ex.Message}");
              return null;
            }
          })
          .Where(plugin => plugin != null);

      foreach (var plugin in plugins)
      {
        try
        {
          plugin.ConfigureServices(services);
          Console.WriteLine($"Configured services for plugin: {plugin.GetType().FullName}");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to configure services for plugin {plugin.GetType().FullName}: {ex.Message}");
        }
      }
    }


    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseRouting();
      app.UseAuthorization();
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudProvisioningService v1");
      });
    }
  }
}

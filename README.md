# TemplateAPI

## Serilog

Install dependence

```
Install-Package Serilog
Install-Package Serilog.AspNetCore
Install-Package Serilog.Sinks.MSSqlServer
Install-Package Serilog.Settings.Configuration
```

Configuration appsetting.json

```
 "Serilog": {
    "MinimumLevel": {
      "WriteTo": [
        {
          "Name": "MSSqlServer",
          "Args": {
            "connectionString": "Server=localhost;Database=IdentityAPI;User Id=sa;Password=jonathan05*",
            "tableName": "Log_API",
            "autoCreateSqlTable": true
          }
        }
      ]
    }
  },
```

Configuration Program 

```

public class Program
{
  public static void Main(string[] args)
  {
   
             //Configuration
               IConfigurationRoot configuration = new
                ConfigurationBuilder().AddJsonFile("appsettings.json",
                optional: false, reloadOnChange: true).Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration).CreateLogger();
  
                CreateHostBuilder(args)
                    .Build()
                    .Run();
  }

     public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseUrls("http://localhost:5001");
                }).UseSerilog();

}
```

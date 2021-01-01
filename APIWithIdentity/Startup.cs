using System;
using System.Collections.Generic;
using APIWithIdentity.DomainModel;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.Extensions;
using APIWithIdentity.Persistence;
using APIWithIdentity.Persistence.UnitOfWork;
using APIWithIdentity.Services;
using APIWithIdentity.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using AutoMapper;
using FluentValidation.AspNetCore;

namespace APIWithIdentity
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
            
            services.Configure<JwtSettings>(Configuration.GetSection("Jwt"));
            var jwtSettings = Configuration.GetSection("Jwt").Get<JwtSettings>();

           
            services.AddControllers()
                .AddFluentValidation(fv => { fv.RegisterValidatorsFromAssemblyContaining<Startup>();});
            
            var dataAssemblyName = typeof(AppDbContext).Assembly.GetName().Name;
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("dev"), 
                x => x.MigrationsAssembly(dataAssemblyName)));
            
            services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequiredLength = 5;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1d);
                    options.Lockout.MaxFailedAccessAttempts = 20;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IMusicServices, MusicServices>();
            services.AddTransient<IArtistServices, ArtistServices>();
            services.AddTransient<IAuthServices, AuthServices>();

            services.AddApiVersioning(op => op.ReportApiVersions = true);

            services.AddVersionedApiExplorer(op =>
            {
                op.GroupNameFormat = "'v'VVV";
                op.SubstituteApiVersionInUrl = true;
            });

            services.AddStackExchangeRedisCache(op =>
            {
                op.Configuration = $"{Configuration["Redis:Port"]}:{Configuration["Redis:Host"]}";
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Music", Version = "v1" });
                options.SwaggerDoc("v2", new OpenApiInfo { Title = "My Music", Version = "v2" });
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT containing userid claim",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                var security =
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                },
                                UnresolvedReference = true
                            },
                            new List<string>()
                        }
                    };
                options.AddSecurityRequirement(security);
            });

            services.AddAutoMapper(typeof(Startup));

            services.AddAuth(jwtSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors("AllowAll");
            
            app.UseAuth();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            
           
           
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Music V1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "My Music API v2");
            });
        }
    }
}
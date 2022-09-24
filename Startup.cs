using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiPeliculas.Data;
using ApiPeliculas.Repository;
using ApiPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ApiPeliculas.PeliculasMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using ApiPeliculas.helpers;
using Microsoft.OpenApi.Models;

namespace ApiPeliculas
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(Options => Options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IPeliculaRepository, PeliculaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            /*Agregar dependencia del token*/
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddAutoMapper(typeof(PeliculasMappers));

            //De aquí en adelante configuración de documentación de nuestra API
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ApiPeliculasCategorias", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "API Categorías Películas",
                    Version = "1",
                    Description = "Backend películas",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Email = "admin@render2web.com",
                        Name = "render2web",
                        Url = new Uri("https://render2web.com")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://en.wikipedia.org/wiki/MIT_License")
                    }
                });

                options.SwaggerDoc("ApiPeliculas", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "API Películas",
                    Version = "1",
                    Description = "Backend películas",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Email = "admin@render2web.com",
                        Name = "render2web",
                        Url = new Uri("https://render2web.com")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://en.wikipedia.org/wiki/MIT_License")
                    }
                });

                options.SwaggerDoc("ApiPeliculasUsuarios", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "API Usuarios Películas",
                    Version = "1",
                    Description = "Backend películas",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Email = "admin@render2web.com",
                        Name = "render2web",
                        Url = new Uri("https://render2web.com")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://en.wikipedia.org/wiki/MIT_License")
                    }
                });

                var archivoXmlComentarios = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaApiComentarios = Path.Combine(AppContext.BaseDirectory, archivoXmlComentarios);
                options.IncludeXmlComments(rutaApiComentarios);

                //Primero definir el esquema de seguridad
                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "Autenticación JWT (Bearer)",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    { 
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }, new List<string>()
                    }
                  });


            });

           
            services.AddControllers();
            /*Damos soporte para CORS*/
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(builder => {
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }

            app.UseHttpsRedirection();
            //Línea para documentación api
            app.UseSwagger();
            app.UseSwaggerUI(options => 
            {
                options.SwaggerEndpoint("/swagger/ApiPeliculasCategorias/swagger.json", "API Categorías Películas");
                options.SwaggerEndpoint("/swagger/ApiPeliculas/swagger.json", "API Películas");
                options.SwaggerEndpoint("/swagger/ApiPeliculasUsuarios/swagger.json", "API Usuarios Películas");
                
                /*Para la publicación en IIS descomentar estas líneas y comentar las de arriba
                options.SwaggerEndpoint("/apiPeliculas/swagger/ApiPeliculasCategorias/swagger.json", "API Categorías Películas");
                options.SwaggerEndpoint("/apiPeliculas/swagger/ApiPeliculas/swagger.json", "API Películas");
                options.SwaggerEndpoint("/apiPeliculas/swagger/ApiPeliculasUsuarios/swagger.json", "API Usuarios Películas");
                */
                options.RoutePrefix = "";
            });

            app.UseRouting();

            /*Estos dos son para la autenticación y autorización*/
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            /*Damos soporte para CORS*/
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        }
    }
}

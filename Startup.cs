using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using JWTDemoAPI.Models;
using JWTDemoAPI.Services;
//using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

namespace JWTDemoAPI
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
			string con = Configuration.GetConnectionString("Con");
			services.AddControllers();

			var appSettingsSection = Configuration.GetSection("AppSettings");
			services.Configure<AppSettings>(appSettingsSection);

			//JWT Authentication
			var appSettigs = appSettingsSection.Get<AppSettings>();
			var key = Encoding.ASCII.GetBytes(appSettigs.Key);

			services.AddAuthentication(au => {
				au.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				au.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(jwt => {
				jwt.RequireHttpsMetadata = false;
				jwt.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});
			services.AddScoped<IAuthenticationService, AuthenticateService>();

			services.AddCors(c =>
			{
				c.AddPolicy("AllowSpecified", options => options.WithOrigins("https://localhost:4200", "http://localhost:4200", "https://*.opticloud.app").AllowAnyMethod().AllowCredentials().AllowAnyHeader().SetIsOriginAllowedToAllowWildcardSubdomains().WithExposedHeaders("Access-Control-Allow-Origin"));
			});

			//services.AddCors(); // Make sure you call this previous to AddMvc
			//services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			//services.AddCors();
			//services.AddCors(options =>
			//{
			//	options.AddPolicy("Policy1",
			//		builder =>
			//		{
			//			builder.WithOrigins("http://localhost:4200/",
			//								"http://www.contoso.com");
			//		});

			//	options.AddPolicy("AnotherPolicy",
			//		builder =>
			//		{
			//			builder.WithOrigins("http://localhost:4200/")
			//								.AllowAnyHeader()
			//								.AllowAnyMethod();
			//		});
			//});

			//services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
			//	.AddJsonOptions(options => {
			//		var resolver = options.JsonSerializerOptions.Converters;
			//		if (resolver != null)
			//		{
			//			(resolver as DefaultContractResolver).NamingStrategy = null;
			//		}
			//	});
			//services.AddDbContext<ConContext>(options =>
			//options.UseModel((Configuration.GetConnectionString("Con")));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			//app.UseAuthorization();

			//app.UseEndpoints(endpoints =>
			//{
			//	endpoints.MapControllers();
			//});

			app.UseHttpsRedirection();
			//app.UseMvc();
			//app.UseCors(options => options.WithOrigins("http://localhost:4200").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
			//app.UseMvc();

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseCors("AllowSpecified");
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers().RequireCors("AllowSpecified");
			});
		}
	}
}

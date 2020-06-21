using Adapters;
using Adapters.Interfaces;
using Data;
using Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Swashbuckle.AspNetCore.Swagger;

namespace RelationalRedisAPI
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Superhero API", Version = "v1"}); });

            if (false)
            {
                var redisConnectionString = "localhost";

                services.AddTransient<IPowerAdapter>(_services => new PowerRedisAdapter(redisConnectionString));
                services.AddTransient<IHeroAdapter>(_services =>
                {
                    var powerAdapter = _services.GetRequiredService<IPowerAdapter>();
                    return new HeroRedisAdapter(redisConnectionString, powerAdapter);
                });
                services.AddTransient<ITeamAdapter>(_services =>
                {
                    var heroAdapter = _services.GetRequiredService<IHeroAdapter>();
                    return new TeamRedisAdapter(redisConnectionString, heroAdapter);
                });
            }
            else
            {
                var sqlConnectionString = "Data Source=localhost;Initial Catalog=Superheroes;Persist Security Info=True;User ID=sa;Password=adminpass";
                services.AddTransient<IPowerAdapter>(_services => new PowerSqlAdapter(sqlConnectionString));
                services.AddTransient<IHeroAdapter>(_services =>
                {
                    var powerAdapter = _services.GetRequiredService<IPowerAdapter>();
                    return new HeroSqlAdapter(sqlConnectionString, powerAdapter);
                });
                services.AddTransient<ITeamAdapter>(_services =>
                {
                    var heroAdapter = _services.GetRequiredService<IHeroAdapter>();
                    return new TeamSqlAdapter(sqlConnectionString, heroAdapter);
                });
            }
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Superhero API v1"); });
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

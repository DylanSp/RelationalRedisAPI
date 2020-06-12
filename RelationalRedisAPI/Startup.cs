using Adapters;
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

            var redisConnectionString = "localhost";

            services.AddTransient<IEntityAdapter<Power>>(_services => new PowerRedisAdapter(redisConnectionString));
            services.AddTransient<IEntityAdapter<Hero>>(_services =>
            {
                var powerAdapter = _services.GetRequiredService<IEntityAdapter<Power>>();
                return new HeroRedisAdapter(redisConnectionString, powerAdapter);
            });
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

using Adapters;
using Data;
using GraphiQl;
using GraphQL;
using GraphQL.Server;
using GraphQL.Types;
using Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RelationalRedisGraphQLAPI.GraphQL;

namespace RelationalRedisGraphQLAPI
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

            services.AddSingleton<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

            services.AddTransient<IEntityAdapter<Power>>(s => new PowerRedisAdapter("localhost"));

            services.AddSingleton<SuperheroQuery>();
            services.AddSingleton<SuperheroMutation>();

            services.AddSingleton<PowerType>();
            services.AddSingleton<PowerInputType>();

            services.AddSingleton<ISchema, SuperheroSchema>();

            services.AddGraphQL(_ => { _.ExposeExceptions = true; });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseGraphQL<ISchema>("/graphql");
            app.UseGraphiQl("/graphiql", "/graphql");
            app.UseMvc();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NCI.OCPL.Api.Common.Models.Options;

using Nest;
using Elasticsearch.Net;

using NSwag.AspNetCore;
using NJsonSchema;

namespace NCI.OCPL.Api.Common
{
    /// <summary>
    /// Serves as a base for Startup classes for Drug Dictionary microservices.
    /// </summary>
    public abstract class NciStartupBase : IStartup
    {
        public NciStartupBase(IHostingEnvironment env)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <returns>The services.</returns>
        /// <param name="services">Services.</param>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            //Turn on the OptionsManager that supports IOptions
            services.AddOptions();
            services.AddCors();


            //This allows us to easily generate URLs to routes
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                           .ActionContext;
                return new UrlHelper(actionContext);
            });


            //Add Configuration mappings.
            // services.Configure<NSwagOptions>(Configuration.GetSection("NSwag"));
            services.Configure<ElasticsearchOptions>(Configuration.GetSection("Elasticsearch"));
            AddAdditionalConfigurationMappings(services);

            //Add HttpClient singleton, which is used by the display service.
            services.AddSingleton<HttpClient, HttpClient>();


            // This will inject an IElasticClient using our configuration into any
            // controllers that take an IElasticClient parameter into its constructor.
            //
            // AddTransient means that it will instantiate a new instance of our client
            // for each instance of the controller.  So the function below will be called
            // on each request.
            services.AddTransient<IElasticClient>(p => {

                // Get the ElasticSearch credentials.
                string username = Configuration["Elasticsearch:Userid"];
                string password = Configuration["Elasticsearch:Password"];

                //Get the ElasticSearch servers that we will be connecting to.
                List<Uri> uris = GetServerUriList();

                // Create the connection pool, the SniffingConnectionPool will
                // keep tabs on the health of the servers in the cluster and
                // probe them to ensure they are healthy.  This is how we handle
                // redundancy and load balancing.
                var connectionPool = new SniffingConnectionPool(uris);

                //Return a new instance of an ElasticClient with our settings
                ConnectionSettings settings = new ConnectionSettings(connectionPool);

                //Let's only try and use credentials if the username is set.
                if (!string.IsNullOrWhiteSpace(username))
                {
                    settings.BasicAuthentication(username, password);
                }

                return new ElasticClient(settings);
            });

            //Add in Application specific services
            AddAppServices(services);

            // Create CORS policies.
            services.AddCors();

            // Add framework services.
            services.AddMvc();

            // Enable Swagger
            // This creates the Swagger Json
            services.AddOpenApiDocument(config => {
                if (!string.IsNullOrEmpty(Configuration["NSwag:Title"]))
                {
                    config.Title = Configuration["NSwag:Title"];
                }

                if (!string.IsNullOrEmpty(Configuration["NSwag:Description"]))
                {
                    config.Description = Configuration["NSwag:Description"];
                }
            });

            // Add framework services
            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Configure the app.
        /// </summary>
        /// <param name="app">App.</param>
        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Setup logging
            ConfigureLogging(app, env, loggerFactory);

            app.UseStaticFiles();

            // Enable the Swagger UI middleware and the Swagger generator
            // This serves the Swagger.json
            app.UseOpenApi(settings => {
                settings.PostProcess = (document , request) => {
                    document.Servers.Clear();

                    // Force the document's base path to match the request.
                    // This is important as the request that generated the document won't
                    // match when IIS is acting as a proxy.
                    document.BasePath = request.PathBase;
                };
            });
            // This serves the Swagger UI
            app.UseSwaggerUi3(settings => {
                // Set this as the default path.
                settings.Path = "";
            });

            // Allow use from anywhere.
            app.UseCors(builder => builder.AllowAnyOrigin());

            // This is equivelant to the old Global.asax OnError event handler.
            // It will handle any unhandled exception and return a status code to the
            // caller.  IF the error is of type APIErrorException then we will also return
            // a message along with the status code.  (Otherwise we )
            app.UseExceptionHandler(errorApp => {
                errorApp.Run(async context => {
                    context.Response.StatusCode = 500; // or another Status accordingly to Exception Type
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<IExceptionHandlerFeature>();

                    if (error != null)
                    {
                        var ex = error.Error;

                        //Unhandled exceptions may not be sanitized, so we will not
                        //display the issue.
                        string message = "Errors have occurred.  Type: " + ex.GetType().ToString();

                        //Our own exceptions should be sanitized enough.
                        if (ex is APIErrorException)
                        {
                            context.Response.StatusCode = ((APIErrorException)ex).HttpStatusCode;
                            message = ex.Message;
                        }

                        byte[] contents = Encoding.UTF8.GetBytes(new ErrorMessage()
                        {
                            Message = message
                        }.ToString());

                        // THIS IS A HACK!!
                        // When the pull request that fixes the timing of setting the CORS header (https://github.com/aspnet/CORS/pull/163) goes through,
                        // we should remove this and test to see if it works without the hack.
                        if (context.Request.Headers.ContainsKey("Origin"))
                        {
                            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                        }

                        await context.Response.Body.WriteAsync(contents, 0, contents.Length);
                    }
                });
            });

            //Call derrived class' method for configuring additional
            //functionality
            ConfigureAppSpecific(app, env, loggerFactory);

            app.UseMvc();

        }

        /*****************************
         * ConfigureServices methods *
         *****************************/

        /// <summary>
        /// Adds the configuration mappings.
        /// </summary>
        /// <param name="services">Services.</param>
        protected abstract void AddAdditionalConfigurationMappings(IServiceCollection services);

        /// <summary>
        /// Adds in the application specific services
        /// </summary>
        /// <param name="services">Services.</param>
        protected abstract void AddAppServices(IServiceCollection services);

        /// <summary>
        /// Sets up the logging for the application. Currently it includes:
        /// <ul>
        ///   <li>Console logging based on Logging configuration</li>
        ///   <li>Debugging if enabled</li>
        /// </ul>
        /// </summary>
        /// <remarks>You should really call base() if you override this method.</remarks>
        /// <param name="app">The application</param>
        /// <param name="env">The hosting environment</param>
        /// <param name="loggerFactory">The logger factory</param>
        protected virtual void ConfigureLogging(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
        }

        /*****************************
         *     Configure methods     *
         *****************************/

        /// <summary>
        /// Configure the specified app and env.
        /// </summary>
        /// <returns>The configure.</returns>
        /// <param name="app">App.</param>
        /// <param name="env">Env.</param>
        /// <param name="loggerFactory">Logger.</param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        protected abstract void ConfigureAppSpecific(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory);


        /// <summary>
        /// Retrieves a list of Elasticsearch server URIs from the configuration's Elasticsearch:Servers setting.
        /// </summary>
        /// <returns>Returns a list of one or more Uri objects representing the configured set of Elasticsearch servers</returns>
        /// <remarks>
        /// The configuration's Elasticsearch:Servers property is required to contain URIs for one or more Elasticsearch servers.
        /// Each URI must include a protocol (http or https), a server name, and optionally, a port number.
        /// Multiple URIs are separated by a comma.  (e.g. "https://fred:9200, https://george:9201, https://ginny:9202")
        ///
        /// Throws ConfigurationException if no servers are configured.
        ///
        /// Throws UriFormatException if any of the configured server URIs are not formatted correctly.
        /// </remarks>
        private List<Uri> GetServerUriList()
        {
            List<Uri> uris = new List<Uri>();

            string serverList = Configuration["Elasticsearch:Servers"];
            if (!String.IsNullOrWhiteSpace(serverList))
            {
                // Convert the list of servers into a list of Uris.
                string[] names = serverList.Split(',');
                uris.AddRange(names.Select(server => new Uri(server)));
            }
            else
            {
                throw new Exception("No servers configured");
            }

            return uris;
        }

    }
}
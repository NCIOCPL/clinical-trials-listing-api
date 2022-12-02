using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Models;
using NCI.OCPL.Api.CTSListingPages.Services;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Defines the configuration of the Listing Page API.
    /// </summary>
    public class Startup : NciStartupBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:NCI.OCPL.Api.CTSListingPages.Startup"/> class.
    /// </summary>
    /// <param name="configuration">Injected configuration object</param>
    public Startup(IConfiguration configuration)
            : base(configuration) { }


        /*****************************
         * ConfigureServices methods *
         *****************************/

        /// <summary>
        /// Adds the configuration mappings.
        /// </summary>
        /// <param name="services">Services.</param>
        protected override void AddAdditionalConfigurationMappings(IServiceCollection services)
        {
        }

        /// <summary>
        /// Adds in the application specific services
        /// </summary>
        /// <param name="services">Services.</param>
        protected override void AddAppServices(IServiceCollection services)
        {
            // Add our Query Services.
            services.AddTransient<ITrialTypeQueryService, ESTrialTypeQueryService>();
            services.AddTransient<IListingInfoQueryService, ESListingInfoQueryService>();

            services.Configure<ListingPageAPIOptions>(Configuration.GetSection("ListingPageAPI"));
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
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        protected override void ConfigureAppSpecific(IApplicationBuilder app, IWebHostEnvironment env)
        {
            return;
        }
    }
}

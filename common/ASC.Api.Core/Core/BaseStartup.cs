﻿// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace ASC.Api.Core;

public abstract class BaseStartup
{
    public IConfiguration Configuration { get; }
    public IHostEnvironment HostEnvironment { get; }
    public virtual JsonConverter[] Converters { get; }
    public virtual bool AddControllersAsServices { get; } = false;
    public virtual bool ConfirmAddScheme { get; } = false;
    public virtual bool AddAndUseSession { get; } = false;
    protected DIHelper DIHelper { get; }
    protected bool LoadProducts { get; set; } = true;
    protected bool LoadConsumers { get; } = true;

    public BaseStartup(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
        DIHelper = new DIHelper();

        if (bool.TryParse(Configuration["core:products"], out var loadProducts))
        {
            LoadProducts = loadProducts;
        }
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddCustomHealthCheck(Configuration);
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddHttpClient();

        if (AddAndUseSession)
        {
            services.AddSession();
        }

        DIHelper.Configure(services);

        Action<JsonOptions> jsonOptions = options =>
            {
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new ApiDateTimeConverter());

                if (Converters != null)
                {
                    foreach (var c in Converters)
                    {
                        options.JsonSerializerOptions.Converters.Add(c);
                    }
                }
            };

        services.AddControllers()
            .AddXmlSerializerFormatters()
            .AddJsonOptions(jsonOptions);

        services.AddSingleton(jsonOptions);

        DIHelper.AddControllers();
        DIHelper.TryAdd<DisposeMiddleware>();
        DIHelper.TryAdd<CultureMiddleware>();
        DIHelper.TryAdd<IpSecurityFilter>();
        DIHelper.TryAdd<PaymentFilter>();
        DIHelper.TryAdd<ProductSecurityFilter>();
        DIHelper.TryAdd<TenantStatusFilter>();
        DIHelper.TryAdd<ConfirmAuthHandler>();
        DIHelper.TryAdd<CookieAuthHandler>();
        DIHelper.TryAdd<WebhooksGlobalFilterAttribute>();

        var redisConfiguration = Configuration.GetSection("Redis").Get<RedisConfiguration>();
        var kafkaConfiguration = Configuration.GetSection("kafka").Get<KafkaSettings>();

        if (kafkaConfiguration != null)
        {
            DIHelper.TryAdd(typeof(ICacheNotify<>), typeof(KafkaCacheNotify<>));
        }
        else if (redisConfiguration != null)
        {
            DIHelper.TryAdd(typeof(ICacheNotify<>), typeof(RedisCacheNotify<>));

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(redisConfiguration);
        }
        else
        {
            DIHelper.TryAdd(typeof(ICacheNotify<>), typeof(MemoryCacheNotify<>));
        }

        DIHelper.TryAdd(typeof(IWebhookPublisher), typeof(WebhookPublisher));

        if (LoadProducts)
        {
            DIHelper.RegisterProducts(Configuration, HostEnvironment.ContentRootPath);
        }

        services.AddMvcCore(config =>
        {
            config.Conventions.Add(new ControllerNameAttributeConvention());

            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            config.Filters.Add(new AuthorizeFilter(policy));
            config.Filters.Add(new TypeFilterAttribute(typeof(TenantStatusFilter)));
            config.Filters.Add(new TypeFilterAttribute(typeof(PaymentFilter)));
            config.Filters.Add(new TypeFilterAttribute(typeof(IpSecurityFilter)));
            config.Filters.Add(new TypeFilterAttribute(typeof(ProductSecurityFilter)));
            config.Filters.Add(new CustomResponseFilterAttribute());
            config.Filters.Add(new CustomExceptionFilterAttribute());
            config.Filters.Add(new TypeFilterAttribute(typeof(FormatFilter)));
            config.Filters.Add(new TypeFilterAttribute(typeof(WebhooksGlobalFilterAttribute)));

            config.OutputFormatters.RemoveType<XmlSerializerOutputFormatter>();
            config.OutputFormatters.Add(new XmlOutputFormatter());
        });


        var authBuilder = services.AddAuthentication("cookie")
            .AddScheme<AuthenticationSchemeOptions, CookieAuthHandler>("cookie", a => { });

        if (ConfirmAddScheme)
        {
            authBuilder.AddScheme<AuthenticationSchemeOptions, ConfirmAuthHandler>("confirm", a => { });
        }

        services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseRouting();

        if (AddAndUseSession)
        {
            app.UseSession();
        }

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseCultureMiddleware();

        app.UseDisposeMiddleware();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapCustom();

            endpoints.MapHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.Register(Configuration, LoadProducts, LoadConsumers);
    }
}

public static class LogNLogConfigureExtenstion
{
    public static IHostBuilder ConfigureNLogLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureLogging((hostBuildexContext, r) =>
        {
            _ = new ConfigureLogNLog(hostBuildexContext.Configuration,
                    new ConfigurationExtension(hostBuildexContext.Configuration), hostBuildexContext.HostingEnvironment);
            r.AddNLog(LogManager.Configuration);
        });
    }
}
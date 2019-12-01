using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;

namespace WiredBrain.Logging
{
    public static class SerilogHelper
    {
        public static void WithWiredBrainConfiguration(this LoggerConfiguration loggerConfig,
            IServiceProvider provider, IConfiguration config)
        {
            var elasticsearchUri = config["Logging:ElasticsearchUri"];
            var elasticIndexRoot = config["Logging:ElasticIndexFormatRoot"];
            var rollingFileName = config["Logging:RollingFileName"];
            var elasticBufferRoot = config["Logging:ElasticBufferRoot"];

            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

            loggerConfig
                .ReadFrom.Configuration(config) // minimum levels defined per project in json files 
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", assemblyName)
                .Enrich.WithAspnetcoreHttpcontext(provider, GetContextInfo)
                .WriteTo.File(rollingFileName)
                .WriteTo.Logger(lc =>
                    lc.Filter.ByExcluding(Matching.WithProperty<bool>("Security", p => p))
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                            IndexFormat = elasticIndexRoot + "-{0:yyyy.MM.dd}",
                            InlineFields = true
                        }))
                .WriteTo.Logger(lc =>
                    lc.Filter.ByIncludingOnly(Matching.WithProperty<bool>("Security", p => p))
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                            IndexFormat = "security-{0:yyyy.MM.dd}",
                            InlineFields = true
                        }));
        }

        private static ContextInformation GetContextInfo(IHttpContextAccessor hca)
        {
            var ctx = hca.HttpContext;
            if (ctx == null) return null;

            return new ContextInformation
            {
                RemoteIpAddress = ctx.Connection.RemoteIpAddress.ToString(),
                Host = ctx.Request.Host.ToString(),
                Method = ctx.Request.Method,
                Protocol = ctx.Request.Protocol,
                UserInfo = GetUserInfo(ctx.User),
            };
        }

        private static UserInformation GetUserInfo(ClaimsPrincipal ctxUser)
        {
            var user = ctxUser.Identity;
            if (user?.IsAuthenticated != true) return null;

            var excludedClaims = new List<string>
            { "nbf", "exp", "auth_time", "amr", "sub", "at_hash",
                "s_hash", "sid", "name", "preferred_username" };

            const string userIdClaimType = "sub";
            const string userNameClaimType = "name";

            var userInfo = new UserInformation
            {
                UserId = ctxUser.Claims.FirstOrDefault(a => a.Type == userIdClaimType)?.Value,
                UserName = ctxUser.Claims.FirstOrDefault(a => a.Type == userNameClaimType)?.Value,
                UserClaims = new Dictionary<string, List<string>>()
            };
            foreach (var distinctClaimType in ctxUser.Claims
                .Where(a => excludedClaims.All(ex => ex != a.Type))
                .Select(a => a.Type)
                .Distinct())
            {
                userInfo.UserClaims[distinctClaimType] = ctxUser.Claims
                    .Where(a => a.Type == distinctClaimType)
                    .Select(c => c.Value)
                    .ToList();
            }

            return userInfo;
        }
    }
}

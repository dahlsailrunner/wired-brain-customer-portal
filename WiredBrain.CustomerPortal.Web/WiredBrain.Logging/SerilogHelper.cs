using System;
using Microsoft.Extensions.Configuration;
using Serilog;
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

            loggerConfig
                .ReadFrom.Configuration(config) // minimum levels defined per project in json files 
                .Enrich.FromLogContext()
                .WriteTo.File(rollingFileName)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = elasticIndexRoot + "-{0:yyyy.MM.dd}",
                    BufferBaseFilename = elasticBufferRoot,
                    InlineFields = true
                });
        }
    }
}

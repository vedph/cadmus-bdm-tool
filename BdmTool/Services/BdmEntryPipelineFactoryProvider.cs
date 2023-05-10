using CadmusBdm.Import;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Hosting;
using Proteus.Entries.Config;
using Proteus.Entries.Pdcx;
using Proteus.Entries.Regions;
using System;

namespace CadmusBdm.Cli.Services;

internal sealed class BdmEntryPipelineFactoryProvider :
    IEntryPipelineFactoryProvider
{
    private static IHost GetHost(string config)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                EntryPipelineFactory.ConfigureServices(services,
                // Proteus.Entries
                typeof(ExcelDumpEntryRegionParser).Assembly,
                // Proteus.Entries.Pdcx
                typeof(PdcxEntryReader).Assembly,
                // CadmusBdm.Import
                typeof(BdmTextEntryRegionParser).Assembly);
            })
            // extension method from Fusi library
            .AddInMemoryJson(config)
            .Build();
    }

    public IEntryPipelineFactory GetFactory(string config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        return new EntryPipelineFactory(GetHost(config));
    }
}

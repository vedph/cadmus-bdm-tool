using CadmusBdm.Cli.Services;
using Fusi.Cli;
using Fusi.Cli.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Proteus.Core.Regions;
using Proteus.Entries.Config;
using Proteus.Entries.Pipeline;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// slightly adapted from Proteus RunEntryPipelineCommand
// as a temporary way for developing BDM components in a unique solution

namespace CadmusBdm.Cli.Commands;

internal sealed class ImportCommand : ICommand
{
    private readonly ImportCommandOptions _options;

    private ImportCommand(ImportCommandOptions options)
    {
        _options = options;
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Run entry pipeline.";
        app.HelpOption("-?|-h|--help");

        CommandArgument pipelineArgument = app.Argument("[pipelinePath]",
           "The path to the pipeline configuration JSON file.");
        CommandArgument outputDirArgument = app.Argument("[outputDir]",
           "The output directory.");

        app.OnExecute(() =>
        {
            context.Command = new ImportCommand(
                new ImportCommandOptions(context)
                {
                    PipelinePath = pipelineArgument.Value,
                    OutputDirectory = outputDirArgument.Value
                });
            return 0;
        });
    }

    private static string LoadFileContent(string path)
    {
        using var reader = new StreamReader(path);
        return reader.ReadToEnd();
    }

    public Task<int> Run()
    {
        ColorConsole.WriteWrappedHeader("IMPORT",
            headerColor: ConsoleColor.Magenta);
        Console.WriteLine($"Pipeline: {_options.PipelinePath}");
        Console.WriteLine($"Output: {_options.OutputDirectory}\n");

        if (!Directory.Exists(_options.OutputDirectory))
            Directory.CreateDirectory(_options.OutputDirectory!);

        // load pipeline config
        ColorConsole.WriteInfo("Building pipeline...");
        string config = LoadFileContent(_options.PipelinePath!);

        // build pipeline (limited to "stock" components)
        IEntryPipelineFactoryProvider? factoryProvider =
            new BdmEntryPipelineFactoryProvider();

        IEntryPipelineFactory factory = factoryProvider.GetFactory(config);
        factory.ConnectionString =
            _options!.Configuration!.GetConnectionString("Default");
        EntryPipeline pipeline = new();
        pipeline.Configure(factory);

        // get context
        ColorConsole.WriteInfo("Building context...");
        IEntrySetContext context = factory.GetPipelineContext()
            ?? new EntrySetContext();

        // get entry reader
        ColorConsole.WriteInfo("Getting entries set reader...");
        EntrySetReader setReader = factory.GetEntrySetReader();

        Console.Write("Reading entry sets: ");

        int count = 0;
        pipeline.Start();
        try
        {
            foreach (EntrySet set in setReader.ReadSets(
                CancellationToken.None, context))
            {
                count++;
                Console.Write('.');
                pipeline.Execute(set);
            }
        }
        finally
        {
            Console.WriteLine();
            pipeline.End();
        }
        Console.WriteLine($"\nSets read: {count}");

        return Task.FromResult(0);
    }
}

internal class ImportCommandOptions : CommandOptions<BdmCliAppContext>
{
    public ImportCommandOptions(ICliAppContext options)
        : base((BdmCliAppContext)options)
    {
    }

    /// <summary>
    /// Gets or sets the path to the JSON pipeline configuration file.
    /// </summary>
    public string? PipelinePath { get; set; }

    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    public string? OutputDirectory { get; set; }
}

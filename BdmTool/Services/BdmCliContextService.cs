namespace CadmusBdm.Cli.Services;

/// <summary>
/// CLI context service.
/// </summary>
public class BdmCliContextService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BdmCliContextService"/>
    /// class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public BdmCliContextService(BdmCliContextServiceConfig config)
    {
    }

    // TODO your provider methods here...
}

/// <summary>
/// Configuration for <see cref="BdmCliContextService"/>.
/// </summary>
public class BdmCliContextServiceConfig
{
    /// <summary>
    /// Gets or sets the connection string to the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the local directory to use when loading resources
    /// from the local file system.
    /// </summary>
    public string? LocalDirectory { get; set; }
}

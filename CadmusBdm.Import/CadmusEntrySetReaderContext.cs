using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using Fusi.Tools.Config;
using Proteus.Core.Regions;

namespace CadmusBdm.Import
{
    /// <summary>
    /// Cadmus entry set context.
    /// Tag: <c>entry-set-reader-context.cadmus</c>.
    /// </summary>
    /// <seealso cref="EntrySetReaderContext" />
    [Tag("entry-set-reader-context.cadmus")]
    public sealed class CadmusEntrySetReaderContext : EntrySetReaderContext,
        IConfigurable<CadmusEntrySetReaderContextOptions>
    {
        private ICadmusRepository? _repository;
        private string? _cs;

        /// <summary>
        /// Gets or sets the item being built.
        /// </summary>
        public IItem? Item { get; set; }

        /// <summary>
        /// Gets or sets the Cadmus repository used to store parsed items.
        /// </summary>
        public ICadmusRepository? Repository
        {
            get
            {
                if (_repository == null)
                {
                    TagAttributeToTypeMap map = new();
                    map.Add(new[]
                    {
                        typeof(NotePart).Assembly
                    });
                    MongoCadmusRepository repository = new(
                        new StandardPartTypeProvider(map),
                        new StandardItemSortKeyBuilder());
                    repository.Configure(new MongoCadmusRepositoryOptions
                    {
                        ConnectionString = _cs
                    });

                    _repository = new MongoCadmusRepository(
                        new StandardPartTypeProvider(map),
                        new StandardItemSortKeyBuilder());
                }
                return _repository;
            }
        }

        public void Configure(CadmusEntrySetReaderContextOptions options)
        {
            _cs = options.ConnectionString;
        }
    }

    /// <summary>
    /// Options for <see cref="CadmusEntrySetReaderContext"/>.
    /// </summary>
    public class CadmusEntrySetReaderContextOptions
    {
        /// <summary>
        /// Gets or sets the connection string to the Cadmus database to write to.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}

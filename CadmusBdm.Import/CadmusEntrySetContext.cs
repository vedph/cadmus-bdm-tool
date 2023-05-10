using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;
using System;

namespace CadmusBdm.Import
{
    /// <summary>
    /// Cadmus entry set context.
    /// Tag: <c>entry-set-context.cadmus</c>.
    /// </summary>
    /// <seealso cref="EntrySetReaderContext" />
    [Tag("entry-set-context.cadmus")]
    public sealed class CadmusEntrySetContext : EntrySetContext,
        IConfigurable<CadmusEntrySetReaderContextOptions>
    {
        private ICadmusRepository? _repository;
        private string? _cs;

        /// <summary>
        /// Gets or sets the item being built.
        /// </summary>
        public IItem? Item { get; set; }

        /// <summary>
        /// Gets the Cadmus repository used to store parsed items.
        /// </summary>
        public ICadmusRepository? Repository
        {
            get
            {
                if (_repository == null)
                {
                    if (_cs == null)
                    {
                        throw new ArgumentNullException(
                            "No connection string configured for CadmusEntrySetContext");
                    }

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

                    _repository = repository;
                }
                return _repository;
            }
        }

        public void Configure(CadmusEntrySetReaderContextOptions options)
        {
            _cs = options.ConnectionString;
        }

        public override IEntrySetContext Clone()
        {
            CadmusEntrySetContext context = (CadmusEntrySetContext)base.Clone();
            context._cs = _cs;
            context._repository = _repository;
            return context;
        }
    }

    /// <summary>
    /// Options for <see cref="CadmusEntrySetContext"/>.
    /// </summary>
    public class CadmusEntrySetReaderContextOptions
    {
        /// <summary>
        /// Gets or sets the connection string to the Cadmus database to write to.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}

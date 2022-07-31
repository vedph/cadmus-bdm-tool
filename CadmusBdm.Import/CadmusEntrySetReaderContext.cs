using Cadmus.Core;
using Cadmus.Core.Storage;
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
    public class CadmusEntrySetReaderContext : EntrySetReaderContext
    {
        /// <summary>
        /// Gets or sets the item being built.
        /// </summary>
        public IItem? Item { get; set; }

        /// <summary>
        /// Gets or sets the Cadmus repository used to store parsed items.
        /// </summary>
        public ICadmusRepository? Repository { get; set; }
    }
}

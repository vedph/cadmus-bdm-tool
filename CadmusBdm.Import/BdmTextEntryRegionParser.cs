using Cadmus.Core;
using Cadmus.General.Parts;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Config;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CadmusBdm.Import
{
    /// <summary>
    /// Region parser for BDM lat/eng regions. These regions represent the
    /// text in an entry set.
    /// Tag: <c>entry-region-parser.bdm-text</c>.
    /// </summary>
    /// <seealso cref="EntryRegionParser" />
    /// <seealso cref="IEntryRegionParser" />
    [Tag("entry-region-parser.bdm-text")]
    public sealed class BdmTextEntryRegionParser : EntryRegionParser, IEntryRegionParser
    {
        private readonly StandardItemSortKeyBuilder _sortKeyBuilder;
        private readonly Regex _wsRegex;
        private readonly Regex _keywordRegex;

        /// <summary>
        /// The source title global datum. This is used by this parser to create
        /// the target Cadmus item, and should be set by client code when
        /// opening the source PDCX file.
        /// </summary>
        public const string K_SOURCE_TITLE = "*source_title";

        /// <summary>
        /// Initializes a new instance of the <see cref="BdmTextEntryRegionParser"/>
        /// class.
        /// </summary>
        public BdmTextEntryRegionParser()
        {
            _sortKeyBuilder = new();
            _wsRegex = new Regex(@"\s+", RegexOptions.Compiled);
            _keywordRegex = new Regex("^%(?:(?<l>[^:]+):)?(?:(?<i>[^:]+):)?(?<v>.+)",
                RegexOptions.Compiled);
        }

        /// <summary>
        /// Determines whether this parser is applicable to the specified
        /// region. Typically, the applicability is determined via a configurable
        /// nested object, having parameters like region tag(s) and paths.
        /// </summary>
        /// <param name="set">The entries set.</param>
        /// <param name="regions">The regions.</param>
        /// <param name="regionIndex">Index of the region.</param>
        /// <returns><c>true</c> if applicable; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">set or regions</exception>
        public bool IsApplicable(EntrySet set, IReadOnlyList<EntryRegion> regions,
            int regionIndex)
        {
            if (set is null) throw new ArgumentNullException(nameof(set));
            if (regions is null) throw new ArgumentNullException(nameof(regions));

            return regions[regionIndex].Tag == "lat" ||
                   regions[regionIndex].Tag == "eng";
        }

        private IItem CreateItem(CadmusEntrySetReaderContext context,
            EntryRegion region)
        {
            string source = context.GetData<string>(K_SOURCE_TITLE)!;
            if (source is null)
            {
                throw new InvalidOperationException(
                    "Source title metadatum not set for TextEntryRegionParser");
            }

            int nr = ((context.Number - 1) / 2) + 1;

            IItem item = new Item()
            {
                FacetId = "text",
                Description = "",   // set later
                Flags = region.Tag == "eng"? 1 : 0,
                GroupId = source,
                Title = $"{source} {nr:00000}-{region.Tag}",
                CreatorId = "zeus",
                UserId = "zeus",
            };

            item.SortKey = _sortKeyBuilder.BuildKey(item, context.Repository);
            return item;
        }

        private string NormalizeWS(string text)
            => _wsRegex.Replace(text, " ").Trim();

        private static void StoreItem(CadmusEntrySetReaderContext context)
        {
            if (context.Repository == null) return;

            context.Repository.AddItem(context.Item);

            // save item's parts
            foreach (IPart part in context.Item!.Parts)
                context.Repository.AddPart(part);
        }

        private static string GetLocation(StringBuilder text)
        {
            int i = text.Length - 1;
            while (i > -1 && !char.IsWhiteSpace(text[i])) i--;
            return $"1.{i + 1}";
        }

        private static void AddExternalIds(string? text, CommentLayerFragment fr)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            fr.ExternalIds.AddRange(text
                .Split(';',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Where(url => fr.ExternalIds.All(id => id.Value != url))
                .Select(url => new ExternalId { Value = url }));
        }

        private static void AddReference(string? a, string? w, string? l, string? y,
            CommentLayerFragment fr)
        {
            StringBuilder sb = new();
            if (a != null) sb.Append(a);
            if (w != null)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(w);
            }
            if (y != null)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(y);
            }
            if (l != null)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(l);
            }
            if (sb.Length == 0) return;

            DocReference r = new()
            {
                Type = y != null ? "m" : "a",
                Citation = sb.ToString()
            };
            fr.References.Add(r);
        }

        private void AddTags(string? text, CommentLayerFragment fr)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            foreach (string tag in text.Split(';',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => NormalizeWS(s)))
            {
                Match m = _keywordRegex.Match(tag);
                if (m.Success)
                {
                    IndexKeyword k = new()
                    {
                        Language = m.Groups["l"].Length > 0
                            ? m.Groups["l"].Value : "eng",
                        IndexId = m.Groups["i"].Length > 0
                            ? m.Groups["i"].Value : null,
                        Value = m.Groups["v"].Value
                    };
                    if (fr.Keywords.All(old => old.Value != k.Value &&
                        old.Language != k.Language &&
                        old.IndexId != k.IndexId))
                    {
                        fr.Keywords.Add(k);
                    }
                }
                else
                {
                    string cat = tag.ToLowerInvariant();
                    if (!fr.Categories.Contains(cat)) fr.Categories.Add(cat);
                }
            }
        }

        private int ParseFn(EntrySet set, int index, string loc,
            CadmusEntrySetReaderContext context)
        {
            // get or create comments layer part
            TokenTextLayerPart<CommentLayerFragment>? commPart =
                context.Item!.Parts.OfType<TokenTextLayerPart<CommentLayerFragment>>()
                .FirstOrDefault();

            if (commPart == null)
            {
                commPart = new TokenTextLayerPart<CommentLayerFragment>
                {
                    CreatorId = context.Item.CreatorId,
                    UserId = context.Item.UserId,
                    ItemId = context.Item.Id,
                    RoleId = "fr.it.vedph.comments"
                };
                context.Item.Parts.Add(commPart);
            }

            // create fragment for that layer
            CommentLayerFragment fr = new()
            {
                Location = loc
                // Text is set later
            };

            // parse note region
            StringBuilder text = new();
            while (index < set.Entries.Count)
            {
                DecodedEntry entry = set.Entries[index];

                // txt: append
                if (entry is DecodedTextEntry txt)
                {
                    text.Append(txt.Value);
                    index++;
                }

                // cmd: process:
                // urls (list, space delimited)
                // aref:
                // - awl
                // - al (implicit work)
                // - ayl (modern)
                // tags: list, semicolon delimited (prefix k: for keywords)
                if (entry is DecodedCommandEntry cmd)
                {
                    // fn(open=0) = end of footnote
                    if (cmd.Name == "fn" && cmd.Type == MarkerType.Close) break;

                    switch (cmd.Name)
                    {
                        case "urls":
                            AddExternalIds(cmd.GetArgument("l"), fr);
                            break;
                        case "aref":
                            AddReference(cmd.GetArgument("a"),
                                cmd.GetArgument("w"),
                                cmd.GetArgument("l"),
                                cmd.GetArgument("y"), fr);
                            break;
                        case "tags":
                            AddTags(cmd.GetArgument("l"), fr);
                            break;
                        default:
                            throw new ArgumentException("Unknown command entry: " + cmd);
                    }
                }

                index++;
            }

            // add fragment
            fr.Text = NormalizeWS(text.ToString());
            commPart.AddFragment(fr);

            return index;
        }

        /// <summary>
        /// Parses the region of entries at <paramref name="regionIndex" />
        /// in the specified <paramref name="regions" />.
        /// </summary>
        /// <param name="set">The entries set.</param>
        /// <param name="regions">The regions.</param>
        /// <param name="regionIndex">Index of the region in the set.</param>
        /// <returns>
        /// The index to the next region to be parsed.
        /// </returns>
        /// <exception cref="ArgumentNullException">set or regions</exception>
        public int Parse(EntrySet set, IReadOnlyList<EntryRegion> regions,
            int regionIndex)
        {
            if (set is null) throw new ArgumentNullException(nameof(set));
            if (regions is null) throw new ArgumentNullException(nameof(regions));

            CadmusEntrySetReaderContext context =
                (CadmusEntrySetReaderContext)set.Context;

            // create item
            context.Item = CreateItem(context, regions[regionIndex]);

            // create text part
            TokenTextPart textPart = new()
            {
                ItemId = context.Item.Id,
                CreatorId = context.Item.CreatorId,
                UserId = context.Item.UserId,
                Citation = context.Item.Title[..^4]
            };
            context.Item.Parts.Add(textPart);

            // parse entries
            StringBuilder text = new();
            int index = 0;
            while (index < set.Entries.Count)
            {
                DecodedEntry entry = set.Entries[index];

                // txt: append
                if (entry is DecodedTextEntry txt)
                {
                    text.Append(txt.Value);
                    index++;
                }

                // cmd: process fn
                else if (entry is DecodedCommandEntry cmd &&
                    cmd.Name == "fn" && cmd.Type == MarkerType.Open)
                {
                    // refer the note to the last graphical word
                    index = ParseFn(set, index, GetLocation(text), context);
                }

                // prp: ignore
                else index++;
            }

            // set text and item's description from it
            textPart.Lines.Add(new TextLine
            {
                Y = 1,
                Text = NormalizeWS(text.ToString())
            });
            // TODO set description by cutting text at middle

            // store
            StoreItem(context);

            return regionIndex + 1;
        }
    }
}

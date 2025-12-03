using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Storage;

namespace Adriva.Common.Core
{
    public class MimeUtilities
    {
        private static readonly Lazy<MimeUtilities> DefaultLazy;
        private static readonly Lazy<MimeUtilities> VideoLazy;
        private static readonly Lazy<MimeUtilities> AudioLazy;
        private static readonly Lazy<MimeUtilities> ImageLazy;
        private static readonly Lazy<MimeUtilities> DocumentsLazy;
        private static readonly Lazy<MimeUtilities> BusinessLazy;

        private readonly ContentInspector Inspector;

        static MimeUtilities()
        {
            MimeUtilities.DefaultLazy = new(() => new(Default.All()), LazyThreadSafetyMode.ExecutionAndPublication);
            MimeUtilities.VideoLazy = new(() => new(Default.FileTypes.Video.All()), LazyThreadSafetyMode.ExecutionAndPublication);
            MimeUtilities.AudioLazy = new(() => new(Default.FileTypes.Audio.All()), LazyThreadSafetyMode.ExecutionAndPublication);
            MimeUtilities.ImageLazy = new(() => new(Default.FileTypes.Images.All()), LazyThreadSafetyMode.ExecutionAndPublication);
            MimeUtilities.DocumentsLazy = new(() => new(Default.FileTypes.Documents.All()), LazyThreadSafetyMode.ExecutionAndPublication);
            MimeUtilities.BusinessLazy = new(() => new(
                [
                    "jpg",
                    "jpeg",
                    "png",
                    "pdf",
                    "doc",
                    "docx",
                    "ppt",
                    "pptx",
                    "xls",
                    "xlsx",
                    "txt",
                    "rtf",
                    "mp4",
                    "mpg",
                    "mpeg"
                ]), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static MimeUtilities All => MimeUtilities.DefaultLazy.Value;

        public static MimeUtilities Video => MimeUtilities.VideoLazy.Value;

        public static MimeUtilities Audio => MimeUtilities.AudioLazy.Value;

        public static MimeUtilities Image => MimeUtilities.ImageLazy.Value;

        public static MimeUtilities Documents => MimeUtilities.DocumentsLazy.Value;

        public static MimeUtilities Business => MimeUtilities.BusinessLazy.Value;

        private MimeUtilities(ImmutableArray<Definition> definitions)
        {
            this.Inspector = new ContentInspectorBuilder()
            {
                Definitions = definitions
            }.Build();
        }

        public MimeUtilities(string[] extensions)
        {
            if (extensions is null || 0 == extensions.Length)
            {
                throw new ArgumentException("At least one extension must be provided.");
            }

            this.Inspector = new ContentInspectorBuilder()
            {
                Definitions = [.. Default
                                .All()
                                .ScopeExtensions(extensions.ToHashSet(StringComparer.OrdinalIgnoreCase))
                                .TrimMeta()
                                .TrimDescription()]
            }.Build();
        }

        public bool TryResolveMimeType(string fileName, out string[] mimeTypes)
        {
            mimeTypes = [];

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var results = this.Inspector.Inspect(fileName);

            if (0 == results.Length)
            {
                return false;
            }

            mimeTypes = [.. results.ByMimeType().Select(x => x.MimeType).Distinct(StringComparer.OrdinalIgnoreCase)];
            return true;
        }

        public bool TryResolveMimeType(Stream stream, out string[] mimeTypes)
        {
            mimeTypes = [];

            if (stream is null || !stream.CanRead)
            {
                return false;
            }

            var results = this.Inspector.Inspect(stream);

            if (0 == results.Length)
            {
                return false;
            }

            mimeTypes = [.. results.ByMimeType().Select(x => x.MimeType).Distinct(StringComparer.OrdinalIgnoreCase)];
            return true;
        }

        public bool TryResolveMimeType(ReadOnlySpan<byte> content, out string[] mimeTypes)
        {
            mimeTypes = [];

            if (0 == content.Length)
            {
                return false;
            }

            var results = this.Inspector.Inspect(content.ToImmutableArray());

            if (0 == results.Length)
            {
                return false;
            }

            mimeTypes = [.. results.ByMimeType().Select(x => x.MimeType).Distinct(StringComparer.OrdinalIgnoreCase)];
            return true;
        }
    }
}
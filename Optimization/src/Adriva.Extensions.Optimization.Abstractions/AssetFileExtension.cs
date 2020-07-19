using System;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class AssetFileExtension
    {

        public static readonly AssetFileExtension Javascript = "js";
        public static readonly AssetFileExtension Stylesheet = "css";
        public static readonly AssetFileExtension Png = "png";
        public static readonly AssetFileExtension Jpg = "jpg";
        public static readonly AssetFileExtension Jpeg = "jpeg";
        public static readonly AssetFileExtension Gif = "gif";
        public static readonly AssetFileExtension Html = "html";
        public static readonly AssetFileExtension Htm = "htm";

        private readonly string FileExtension;

        protected AssetFileExtension(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileExtension)) throw new ArgumentNullException(nameof(fileExtension));

            if (!fileExtension.StartsWith(".", StringComparison.Ordinal)) fileExtension = "." + fileExtension;

            this.FileExtension = fileExtension.ToLowerInvariant();
        }

        public static implicit operator AssetFileExtension(string extension)
        {
            return new AssetFileExtension(extension);
        }

        public static implicit operator string(AssetFileExtension assetFileExtension)
        {
            return assetFileExtension.FileExtension;
        }

        public static bool operator ==(AssetFileExtension first, AssetFileExtension second)
        {
            if (null == (object)first || null == (object)second) return false;
            return 0 == string.Compare(first, second, StringComparison.Ordinal);
        }

        public static bool operator !=(AssetFileExtension first, AssetFileExtension second)
        {
            if (null == (object)first || null == (object)second) return true;
            return 0 != string.Compare(first, second, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AssetFileExtension assetFileExtension)) return false;
            return 0 == string.Compare(this.FileExtension, assetFileExtension.FileExtension, StringComparison.Ordinal);
        }

        public override int GetHashCode() => this.FileExtension.GetHashCode();

        public override string ToString() => $"*{this.FileExtension}, [AssetFileExtension]";
    }
}
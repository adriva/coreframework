using System.Text;
using Adriva.Common.Core;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;

namespace Adriva.Storage.Azure
{
    public static class AzureStorageUtilities
    {

        public static TableContinuationToken DeserializeTableContinuationToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            byte[] tokenBytes = Utilities.FromBaseString(token, Utilities.Base63Alphabet);
            string json = Encoding.UTF8.GetString(tokenBytes);
            return Utilities.SafeDeserialize<TableContinuationToken>(json);
        }

        public static BlobContinuationToken DeserializeBlobContinuationToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            byte[] tokenBytes = Utilities.FromBaseString(token, Utilities.Base63Alphabet);
            string json = Encoding.UTF8.GetString(tokenBytes);
            return Utilities.SafeDeserialize<BlobContinuationToken>(json);
        }

        #region Extension Methods

        public static string Serialize(this TableContinuationToken token)
        {
            if (null == token) return null;

            string json = Utilities.SafeSerialize(token);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return Utilities.GetBaseString(buffer, Utilities.Base63Alphabet);
        }

        public static string Serialize(this BlobContinuationToken token)
        {
            if (null == token) return null;

            string json = Utilities.SafeSerialize(token);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return Utilities.GetBaseString(buffer, Utilities.Base63Alphabet);
        }

        #endregion
    }
}

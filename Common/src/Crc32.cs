using System.Text;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Provides methods to calculate the 32 bit CRC value for a given input.
    /// </summary>
    public static class Crc32
    {
        public const uint DefaultPolynomial = 0xedb88320u;
        public const uint DefaultSeed = 0xffffffffu;

        private static uint[] DefaultTable;

        static Crc32()
        {
            Crc32.DefaultTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ Crc32.DefaultPolynomial;
                    else
                        entry = entry >> 1;
                Crc32.DefaultTable[i] = entry;
            }
        }

        /// <summary>
        /// Computes the 32 bit CRC value for a given string.
        /// </summary>
        /// <param name="text">The string, for which the CRC value will be computed for.</param>
        /// <returns>0 if the given string is Empty or Null, otherwise an unsigned 32 bit integer representing the CRC value.</returns>
        public static uint Compute(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0U;
            var bytes = Encoding.UTF8.GetBytes(text);
            return Crc32.Compute(bytes);
        }

        /// <summary>
        /// Computes the 32 bit CRC value for a given byte array.
        /// </summary>
        /// <param name="data">The byte array, for which the CRC value will be computed for.</param>
        /// <returns>0 if the given array is Null, otherwise an unsigned 32 bit integer representing the CRC value.</returns>
        public static uint Compute(byte[] data)
        {
            if (null == data) return 0U;

            var hash = Crc32.DefaultSeed;
            for (var i = 0; i < data.Length; i++)
                hash = (hash >> 8) ^ Crc32.DefaultTable[data[i] ^ hash & 0xff];
            return ~hash;
        }
    }
}
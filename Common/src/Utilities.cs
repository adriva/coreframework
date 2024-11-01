using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonFormatting = Newtonsoft.Json.Formatting;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Provides static utility methods to help with common and repetitive operations.
    /// </summary>
    public static class Utilities
    {
        private static readonly JsonSerializerSettings defaultSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        private static readonly XmlWriterSettings defaultXmlWriterSettings = new XmlWriterSettings() { Async = true, Indent = false, Encoding = Encoding.UTF8 };
        private static readonly List<string> publicSuffixes = new List<string>();

        private static readonly IDictionary<char, char> CharacterMappings = new Dictionary<char, char>{
                {'Á','A'},                {'á','a'},
                {'Č','C'},                {'č','c'},
                {'ď','d'},                {'é','e'},
                {'ě','e'},                {'É','E'},
                {'Ě','E'},                {'í','i'},
                {'Í','i'},                {'Ň','N'},
                {'ň','n'},                {'Ó','O'},
                {'ó','o'},                {'Ř','R'},
                {'ř','r'},                {'Š','S'},
                {'š','s'},                {'ť','t'},
                {'Ú','U'},                {'ú','u'},
                {'Ů','U'},                {'ů','u'},
                {'Ý','Y'},                {'ý','y'},
                {'Ž','Z'},                {'ž','z'},
                {'ç','c'},                {'ğ','g'},
                {'ı','i'},                {'İ','i'},
                {'ö','o'},                {'ş','s'},
                {'ü','u'},                {'Ç','C'},
                {'Ğ','G'},                {'I','I'},
                {'Ö','O'},                {'Ş','S'},
                {'Ü','U'},                {'â','a'}
            };

        /// <summary>
        /// The Unix Epoch time.
        /// </summary>
        /// <returns>A DateTime value representing the Unix Epoch time.</returns>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The minimum DateTimeOffset value supported by Microsoft Azure platform.
        /// </summary>
        /// <returns>A DateTimeOffset instance that represents the minimum value supported by Microsoft Azure platform.</returns>
        public static readonly DateTimeOffset AzureMinDateTimeOffset = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// An alphabet composed of 63 characters which are upper and lower case letters, digits and underscore.
        /// </summary>
        public static readonly string Base63Alphabet = "_0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// An alphabet composed of 36 characters which are lower case letters and digits.
        /// </summary>
        public static readonly string Base36Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Initializes the common settings and loads the domain suffix list for Url parsing into the memory.
        /// </summary>
        static Utilities()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            List<string> suffixes = new List<string>();

            var assembly = typeof(Utilities).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream("Adriva.Common.Core.suffix_list.dat"))
            using (var reader = new StreamReader(stream))
            {
                string line = null;
                do
                {
                    line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//", StringComparison.Ordinal)) continue;
                    else suffixes.Add("." + line.Trim().ToLowerInvariant());
                } while (null != line);
            }

            suffixes.Add(".sys.local");

            Utilities.publicSuffixes.AddRange(suffixes.OrderByDescending(x => x.Length));

#if THROTTLE
            Microsoft.AspNetCore.Hosting.ThrottlingContext.Run();
#endif
        }

        /// <summary>
        /// Extracts the main domain name from a given url.
        /// </summary>
        /// <param name="url">The Url, for which the main domain name will be extracted.</param>
        /// <returns>A string value representing the main domain name.</returns>
        public static string GetMainDomainName(string url)
        {
            string hostName = null;

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                hostName = uri.Host;
            }
            else if (Uri.TryCreate(string.Concat("http://", url), UriKind.Absolute, out uri))
            {
                hostName = uri.Host;
            }
            else
            {
                if (null == url) return null;

                hostName = url;
            }

            if (!string.IsNullOrWhiteSpace(hostName))
            {
                var matchingSuffix = Utilities.publicSuffixes.First(suf => hostName.EndsWith(suf, StringComparison.OrdinalIgnoreCase));

                hostName = hostName.Substring(0, hostName.Length - matchingSuffix.Length);
                var hostParts = hostName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                return string.Concat(hostParts[hostParts.Length - 1], matchingSuffix).ToLowerInvariant();
            }
            return null;
        }

        #region String Utilities

        /// <summary>
        /// Converts a given latin based string to its ASCII equivalent. Such as 'sığ' is converted to 'sig'.
        /// </summary>
        /// <param name="input">The input string that will be converted to ASCII.</param>
        /// <returns>The ASCII representation of the given string.</returns>
        public static string ConvertToAscii(string input)
        {
            StringBuilder buffer = new StringBuilder();
            if (input.Any(c => Utilities.CharacterMappings.ContainsKey(c)))
            {
                for (int loop = 0; loop < input.Length; loop++)
                {
                    if (Utilities.CharacterMappings.ContainsKey(input[loop]))
                    {
                        buffer.Append(Utilities.CharacterMappings[input[loop]]);
                    }
                    else
                    {
                        //output += input[loop];
                        buffer.Append(input[loop]);
                    }
                }
            }
            else
            {
                return input;
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Truncates a string only if the length of the string exceeds the maximum length provided.
        /// </summary>
        /// <param name="input">The input string that may be truncated.</param>
        /// <param name="maxLength">The maximum number of characters allowed in the final string.</param>
        /// <returns>A string that is the truncated copy of the original string.</returns>
        public static string TruncateString(string input, int maxLength)
        {
            maxLength = Math.Max(0, maxLength);

            if (string.IsNullOrWhiteSpace(input)) return null;

            input = input.Trim();

            if (maxLength >= input.Length) return input;
            return input.Substring(0, maxLength);
        }

        /// <summary>
        /// Compresses the whitespaces in a string by replacing consecutive whitespaces with one space character.
        /// </summary>
        /// <param name="input">The string that will be compressed.</param>
        /// <returns>A string value that is the whitespace compressed copy of the original string.</returns>
        public static string CompressWhitespaces(string input)
        {
            if (null == input) return null;
            else if (0 == input.Length) return string.Empty;

            StringBuilder buffer = new StringBuilder();
            int loop = 0;

            while (loop < input.Length)
            {
                while (loop < input.Length && !char.IsWhiteSpace(input[loop]))
                {
                    buffer.Append(input[loop]);
                    loop++;
                }

                if (loop < input.Length - 1 && 0 < buffer.Length) buffer.Append(" ");

                while (loop < input.Length && char.IsWhiteSpace(input[loop]))
                {
                    loop++;
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Removes all whitespace characters from a string.
        /// </summary>
        /// <param name="input">The string that the whitespaces will be removed.</param>
        /// <returns>A string value which is a copy of the original string without any whitespace characters.</returns>
        public static string RemoveWhitespaces(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            StringBuilder buffer = new StringBuilder();

            foreach (char character in input)
            {
                if (!char.IsWhiteSpace(character)) buffer.Append(character);
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Gets the string representation of a byte array using the alphabet provided.
        /// <remarks>
        /// An alphabet is a string of characters. Such as <see cref="Utilities.Base63Alphabet" /> or <see cref="Utilities.Base36Alphabet" />
        /// </remarks>
        /// <remarks>
        /// If the given different byte[] data is comparable then the output string will be comparable as well assuming the alphabet is declared in byte ascending order.
        /// </remarks>
        /// </summary>
        /// <param name="data">The byte array that will be used to construct a base string for.</param>
        /// <param name="alphabet">The alphabet that will be used to translate the input data.</param>
        /// <returns>A string representing the translated value of the given byte array.</returns>
        public static string GetBaseString(byte[] data, string alphabet)
        {
            if (null == data) return string.Empty;

            StringBuilder buffer = new StringBuilder();

            Array.Resize(ref data, 1 + data.Length);
            BigInteger bigint = new BigInteger(data);

            do
            {
                buffer.Insert(0, alphabet[(int)(bigint % alphabet.Length)]);
                bigint = bigint / (ulong)alphabet.Length;
            } while (0 < bigint);

            return buffer.ToString();
        }

        /// <summary>
        /// Gets the string representation of a ulong value using the alphabet provided.
        /// <remarks>
        /// An alphabet is a string of characters. Such as <see cref="Utilities.Base63Alphabet" /> or <see cref="Utilities.Base36Alphabet" />
        /// </remarks>
        /// </summary>
        /// <param name="number">The ulong value that will be used to construct a base string for.</param>
        /// <param name="alphabet">The alphabet that will be used to translate the input data.</param>
        /// <returns>A string representing the translated value of the given ulong value.</returns>
        public static string GetBaseString(ulong number, string alphabet)
        {
            StringBuilder buffer = new StringBuilder();

            do
            {
                buffer.Insert(0, alphabet[(int)(number % (ulong)alphabet.Length)]);
                number = number / (ulong)alphabet.Length;
            } while (0 < number);

            return buffer.ToString();
        }

        public static byte[] FromBaseString(string baseString, string alphabet)
        {
            if (string.IsNullOrWhiteSpace(baseString)) return null;
            if (string.IsNullOrEmpty(alphabet)) throw new ArgumentNullException("Specified alphabet is null of empty string.");

            BigInteger bigint = new BigInteger(0);

            for (int loop = 0; loop < baseString.Length; loop++)
            {
                bigint *= alphabet.Length;
                int mod = alphabet.IndexOf(baseString[loop]);
                bigint += mod;
            }

            return bigint.ToByteArray(true);
        }



        public static string RestoreNegatedString(string input)
        {
            int count = input.Length / 2;
            int loop = 0; int index = 0;
            List<byte> buffer = new List<byte>();

            while (index < count)
            {
                var byteValue = (16 * "0123456789ABCDEF".IndexOf(input[loop])) + "0123456789ABCDEF".IndexOf(input[1 + loop]);
                var normalizedValue = (byte)(0xFF - byteValue);
                if (0 < normalizedValue) buffer.Add(normalizedValue);
                loop += 2; index++;
            }

            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        public static string NegateString(string input, int maxLength)
        {
            if (0 > maxLength) return string.Empty;
            if (maxLength < input.Length) maxLength = input.Length;

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] diffBytes = new byte[maxLength];

            int loop = 0; int delta = 0;

            while (loop < maxLength - input.Length)
            {
                diffBytes[loop] = 0xFF;
                loop++;
            }

            delta = loop;

            while (loop < maxLength)
            {
                diffBytes[loop] = (byte)(0xFF - inputBytes[loop - delta]);
                loop++;
            }

            return BitConverter.ToString(diffBytes).Replace("-", null);
        }

        /// <summary>
        /// Slugifies a string by converting it to its ASCII representation and replacing non alphanumeric values with the '-' character.
        /// </summary>
        /// <param name="input">The string that will be slugified.</param>
        /// <param name="wordCount">The maximum number of '-' seperated words allowed in the output.</param>
        /// <returns>A string value representing the slugified copy of the source string.</returns>
        public static string Slugify(string input, int wordCount)
        {
            wordCount = Math.Max(1, wordCount);
            var dataParts = Utilities.Slugify(input).Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries).Take(wordCount);
            return string.Join("-", dataParts);
        }

        /// <summary>
        /// Slugifies a string by converting it to its ASCII representation and replacing non alphanumeric values with the '-' character.
        /// </summary>
        /// <param name="input">The string that will be slugified.</param>
        /// <returns>A string value representing the slugified copy of the source string.</returns>
        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            input = Utilities.ConvertToAscii(input);

            StringBuilder buffer = new StringBuilder(input.Length);
            bool lastWhitespace = false;
            char lastChar = '\0';

            foreach (char character in input)
            {
                if (' ' == character && lastWhitespace)
                {
                    if ('-' != lastChar)
                    {
                        buffer.Append("-");
                        lastChar = '-';
                    }
                    lastWhitespace = true;
                }
                else
                {
                    if (char.IsLetterOrDigit(character))
                    {
                        buffer.Append(character);
                        lastChar = character;
                    }
                    else if ('-' != lastChar)
                    {
                        buffer.Append("-");
                        lastChar = '-';
                    }
                    lastWhitespace = false;
                }
            }
            if (0 < buffer.Length && '-' == buffer[buffer.Length - 1]) buffer.Remove(buffer.Length - 1, 1);
            return buffer.ToString().ToLowerInvariant();
        }

        #endregion

        // isreversed : can be used to keep rowkey in order (azuretable)
        //				since row keys are by design, ordered asc
        /// <summary>
        /// Converts a given DateTimeOffset to its ticks representation as a string and can be negated using DateTimeOffset.MaxValue if needed.
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset instance that the ticks will be calculated for.</param>
        /// <param name="isReversed">True, if the difference between DateTimeOffset.MaxValue and the given DateTimeOffset will be used, otherwise False.</param>
        /// <returns>A string value representing the numeric ticks value.</returns>
        public static string GetDateTicksString(DateTimeOffset dateTimeOffset, bool isReversed)
        {
            if (!isReversed)
            {
                return Convert.ToString(dateTimeOffset.UtcTicks);
            }

            return Convert.ToString(DateTimeOffset.MaxValue.Subtract(dateTimeOffset).Ticks);
        }

        /// <summary>
        /// Parses and constructs a DateTimeOffset instance from the given string representing the ticks value.
        /// </summary>
        /// <param name="ticksString">Ticks value of the DateTimeOffset that will be parsed.</param>
        /// <param name="isReversed">True, if the ticks represents the difference between the DateTimeOffset.MaxValue and another DateTimeOffset, otherwise False.</param>
        /// <returns>A DateTimeOffset value representing the parsed ticks value.</returns>
        public static DateTimeOffset ParseDateTicks(string ticksString, bool isReversed)
        {
            if (!long.TryParse(ticksString, out long longTicks)) return DateTimeOffset.MinValue;
            if (!isReversed) return new DateTimeOffset(longTicks, TimeSpan.Zero);
            var deltaTime = TimeSpan.FromTicks(longTicks);
            return DateTimeOffset.MaxValue.Subtract(deltaTime);
        }

        /// <summary>
        /// Converts a DateTimeOffset value to another DateTimeOffset which can be used safely in Azure storage.
        /// <remarks>Values less than 1900-01-01 are not supported in Azure storage.</remarks>
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset valiue that will be validated.</param>
        /// <returns>A DateTimeOffset value that can used with Azure storage.</returns>
        public static DateTimeOffset GetAzureDateTime(DateTimeOffset dateTimeOffset)
        {
            if (dateTimeOffset < Utilities.AzureMinDateTimeOffset) dateTimeOffset = Utilities.AzureMinDateTimeOffset;
            return dateTimeOffset;
        }

        #region Currency Utilities

        public static decimal ExtractDecimal(string input, CultureInfo primaryCulture = null, CultureInfo fallbackCulture = null)
        {
            input = Utilities.RemoveWhitespaces(input);
            var match = Regex.Match(input, @"((\d{1,3}((\,\d{3}|\.\d{3})*))+(\,|\.)*\d{0,2})");
            if (!match.Success) return -1;
            return Utilities.ParseDecimal(match.Value, primaryCulture, fallbackCulture);
        }

        public static decimal ParseDecimal(string input, CultureInfo primaryCulture = null, CultureInfo fallbackCulture = null)
        {
            primaryCulture = primaryCulture ?? CultureInfo.GetCultureInfo("tr-TR");
            fallbackCulture = fallbackCulture ?? CultureInfo.InvariantCulture;
            int count = input.Length;

            for (int loop = count - 1; loop >= 0; loop--)
            {
                if (input[loop] == '.' && 4 > count - loop)
                {
                    input = input.Replace(",", string.Empty).Replace(".", ",");
                }
            }

            if (!decimal.TryParse(input, NumberStyles.Any, primaryCulture, out decimal output))
            {
                if (!decimal.TryParse(input, NumberStyles.Any, fallbackCulture, out output))
                {
                }
            }

            return output;
        }

        public static string ParseCurrencyCode(string text, IDictionary<string, string> codeCurrencyMap = null)
        {
            var currenyCodeMap = new Dictionary<string, string> { { "try", "TRY" }, { "tl", "TRY" }, { "usd", "USD" }, { "eur", "EUR" }, { "$", "USD" }, { "€", "EUR" }, { "₺", "TRY" }, { "cny", "CNY" }, { "rub", "RUB" }, { "azn", "AZN" }, { "gbp", "GBP" } };

            if (null != codeCurrencyMap)
            {
                foreach (var entry in codeCurrencyMap)
                {
                    if (!currenyCodeMap.ContainsKey(entry.Key.ToLower()))
                    {
                        currenyCodeMap.Add(entry.Key.ToLower(), entry.Value);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(text)) return null;

            string pattern = string.Join('|', currenyCodeMap.Select(e => e.Key).Distinct());
            var match = Regex.Match(text, pattern);

            if (!match.Success || !currenyCodeMap.ContainsKey(match.Value.ToLowerInvariant())) return string.Empty;

            return currenyCodeMap[match.Value.ToLowerInvariant()];
        }
        #endregion

        #region Misc Utilities

        public static string GetRandomId(int sizeInBytes = 8)
        {
            sizeInBytes = Math.Max(sizeInBytes, 1);
            using (var generator = RandomNumberGenerator.Create())
            {
                byte[] buffer = Utilities.GetRandomBytes(sizeInBytes);
                return Utilities.GetBaseString(buffer, Utilities.Base63Alphabet);
            }
        }

        public static byte[] GetRandomBytes(int size)
        {
            byte[] buffer = new byte[size];

            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(buffer);
            }

            return buffer;
        }

        public static T[] GetFlags<T>(Enum value, Enum ignoredValue)
        {
            List<T> items = new List<T>();

            var enumValues = Enum.GetValues(typeof(T));

            foreach (Enum enumValue in enumValues)
            {
                if (value.HasFlag(enumValue) && (null == ignoredValue || (null != ignoredValue && 0 != enumValue.CompareTo(ignoredValue))))
                {
                    items.Add((T)(object)enumValue);
                }
            }

            return items.ToArray();
        }

        public static T CastObject<T>(object data)
        {
            if (data is JObject jData)
            {
                return jData.ToObject<T>();
            }
            else if (data is T concreteData)
            {
                return concreteData;
            }
            else if (null == data)
            {
                return default(T);
            }
            else
            {
                Type typeofTData = typeof(T);

                if (typeofTData.IsEnum)
                {
                    if (Enum.IsDefined(typeofTData, data))
                    {
                        return (T)Enum.ToObject(typeofTData, data);
                    }
                }
                else
                {
                    return (T)Convert.ChangeType(data, typeofTData);
                }
            }

            throw new InvalidCastException();
        }

        #endregion

        #region Serialization Utilities

        public static string SafeSerialize(object item)
        {
            if (null == item) return null;
            return JsonConvert.SerializeObject(item, JsonFormatting.None, Utilities.defaultSerializerSettings);
        }

        public static string SafeSerialize(object item, JsonSerializerSettings settings)
        {
            if (null == item) return null;
            return JsonConvert.SerializeObject(item, settings);
        }

        public static T SafeDeserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            return JsonConvert.DeserializeObject<T>(json, Utilities.defaultSerializerSettings);
        }

        public static T SafeDeserialize<T>(string json, JsonSerializerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static T SafeDeserialize<T>(byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer);
            return Utilities.SafeDeserialize<T>(json);
        }

        #endregion

        #region Crypto Utilities

        public static string CalculateHash(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            using (var hashAlgorithm = SHA256.Create())
            {
                hashAlgorithm.Initialize();
                var hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Utilities.GetBaseString(hashBytes, Utilities.Base63Alphabet);
            }
        }

        public static string CalculateBinaryHash(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            using (var hashAlgorithm = SHA256.Create())
            {
                hashAlgorithm.Initialize();
                var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        #endregion

        #region Http Utilities

        public static bool TryParseCookie(string cookieString, out Dictionary<string, string> cookieProperties)
        {
            cookieProperties = null;

            if (string.IsNullOrWhiteSpace(cookieString)) return false;

            var cookieParts = cookieString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            cookieProperties = new Dictionary<string, string>();
            bool isFirst = true;

            foreach (var cookiePart in cookieParts)
            {
                var cookiePairs = cookiePart.Split(new[] { '=' }, StringSplitOptions.None);
                if (2 == cookiePairs.Length)
                {
                    if (isFirst)
                    {
                        cookieProperties.Add("name", cookiePairs[0]);
                        cookieProperties.Add("value", cookiePairs[1]);
                        isFirst = false;
                    }
                    else
                    {
                        cookieProperties.Add(cookiePairs[0].Trim().ToLowerInvariant(), cookiePairs[1].Trim());
                    }
                }
            }

            if (0 == cookieProperties.Count)
            {
                cookieProperties = null;
                return false;
            }


            return true;
        }

        public static bool TryGetCookieValue(HttpResponseHeaders headers, string cookieName, out string value)
        {
            value = null;

            if (null == headers || string.IsNullOrWhiteSpace(cookieName)) return false;

            if (!headers.TryGetValues("set-cookie", out IEnumerable<string> cookieStrings)) return false;

            foreach (var cookieString in cookieStrings)
            {
                if (Utilities.TryParseCookie(cookieString, out Dictionary<string, string> cookieProperties))
                {
                    if (cookieProperties.ContainsKey("name") && 0 == string.Compare(cookieProperties["name"], cookieName, StringComparison.Ordinal))
                    {
                        value = cookieProperties.ContainsKey("value") ? cookieProperties["value"] : null;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsHttpSuccess(int statusCode)
        {
            return 200 <= statusCode && 299 >= statusCode;
        }

        /// <summary>
        /// Checks if the given Uri represents a valid http or https url.
        /// </summary>
        /// <param name="uri">The uri to be checked for.</param>
        /// <returns>True is the given uri is a valid http or https Url, otherwise False.</returns>
        public static bool IsValidHttpUri(Uri uri)
        {
            if (null == uri) return false;
            if (!uri.IsAbsoluteUri) return false;
            return 0 == string.Compare(Uri.UriSchemeHttp, uri.Scheme, StringComparison.Ordinal)
                || 0 == string.Compare(Uri.UriSchemeHttps, uri.Scheme, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if the given Uri string represents a valid http or https url.
        /// </summary>
        /// <param name="uri">The string uri to be checked for.</param>
        /// <returns>True is the given string is a valid http or https Url, otherwise False.</returns>
        public static bool IsValidHttpUri(string uri)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri tempUri)) return false;
            return Utilities.IsValidHttpUri(tempUri);
        }

        #endregion

        #region File Utilities

        public static IEnumerable<IFileInfo> EnumerateFiles(IFileProvider fileProvider, string rootDirectory)
        {
            fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));

            var directoryContents = fileProvider.GetDirectoryContents(rootDirectory);

            var fileEnumerator = directoryContents.GetEnumerator();

            while (fileEnumerator.MoveNext())
            {
                if (fileEnumerator.Current.IsDirectory)
                {
                    var childEnumerable = Utilities.EnumerateFiles(fileProvider, Path.Combine(rootDirectory, fileEnumerator.Current.Name));
                    var childEnumerator = childEnumerable.GetEnumerator();

                    while (childEnumerator.MoveNext())
                    {
                        yield return childEnumerator.Current;
                    }
                }
                else
                {
                    yield return fileEnumerator.Current;
                }
            }
        }

        public static byte[] CalculateFileHash(IFileInfo fileInfo)
        {
            using (var md5 = MD5.Create())
            {
                return Utilities.CalculateFileHash(fileInfo, md5);
            }
        }

        public static byte[] CalculateFileHash(IFileInfo fileInfo, HashAlgorithm hashAlgorithm)
        {
            if (null == fileInfo) return Array.Empty<byte>();

            using (var stream = fileInfo.CreateReadStream())
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }

        public static byte[] CalculateStreamHash(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return Utilities.CalculateStreamHash(stream, md5);
            }
        }

        public static byte[] CalculateStreamHash(Stream stream, HashAlgorithm hashAlgorithm)
        {
            if (null == stream) return Array.Empty<byte>();
            return hashAlgorithm.ComputeHash(stream);
        }

        #endregion
    }
}

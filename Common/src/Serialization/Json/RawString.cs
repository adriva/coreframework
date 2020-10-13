using Newtonsoft.Json;

namespace Adriva.Common.Core.Serialization.Json
{
    [JsonConverter(typeof(RawStringConverter))]
    public sealed class RawString
    {
        private readonly string Text;

        public RawString(string text)
        {
            this.Text = text;
        }

        public override string ToString()
        {
            return this.Text;
        }

        public static implicit operator RawString(string text)
        {
            return new RawString(text);
        }

        public static implicit operator string(RawString rawString)
        {
            return rawString.Text;
        }
    }
}
using System.Collections.Generic;

namespace Adriva.Storage.Azure
{
    public interface IAzureTableMapper
    {
        void Read(IDictionary<string, object> properties);

        void Write(IDictionary<string, object> properties);
    }
}

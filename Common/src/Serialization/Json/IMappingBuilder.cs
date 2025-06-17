using System.Collections.Generic;

namespace Adriva.Common.Core.Serialization.Json
{
    public interface IMappingBuilder
    {
        IReadOnlyDictionary<string, PropertyContract> PropertyContracts { get; }
    }
}
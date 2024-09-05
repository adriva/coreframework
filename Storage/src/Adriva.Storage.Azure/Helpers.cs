using System.Linq;
using System.Reflection;
using Azure.Data.Tables;

namespace Adriva.Storage.Azure
{
    internal static class Helpers
    {
        public static string GetPropertyName(MemberInfo memberInfo, out bool isBaseTypeName)
        {
            isBaseTypeName = false;

            if (null == memberInfo)
            {
                return string.Empty;
            }

            if (memberInfo.GetCustomAttributes<RowKeyAttribute>().Any())
            {
                isBaseTypeName = true;
                return nameof(TableEntity.RowKey);
            }
            else if (memberInfo.GetCustomAttributes<PartitionKeyAttribute>().Any())
            {
                isBaseTypeName = true;
                return nameof(TableEntity.PartitionKey);
            }
            else if (memberInfo.GetCustomAttributes<ETagAttribute>().Any())
            {
                isBaseTypeName = true;
                return nameof(TableEntity.ETag);
            }

            return memberInfo.Name;
        }
    }
}

using System;
using FASTER.core;

namespace Adriva.Extensions.Faster
{
    internal class CallbackFunctions : SimpleFunctions<string, StorageDataEntry>
    {
        public override bool SingleReader(ref string key, ref StorageDataEntry input, ref StorageDataEntry value, ref StorageDataEntry dst, ref ReadInfo readInfo)
        {
            var baseResult = base.SingleReader(ref key, ref input, ref value, ref dst, ref readInfo);

            if (baseResult)
            {
                if (dst.ExpiresAt < DateTime.UtcNow)
                {
                    readInfo.Action = ReadAction.Expire;
                    return false;
                }
            }

            return baseResult;
        }

        public override bool SingleWriter(ref string key, ref StorageDataEntry input, ref StorageDataEntry src, ref StorageDataEntry dst, ref StorageDataEntry output, ref UpsertInfo upsertInfo, WriteReason reason)
        {
            StorageDataEntry newEntry = new StorageDataEntry(src.Data, ETagValue.Create(), src.ExpiresAt);
            return base.SingleWriter(ref key, ref input, ref newEntry, ref dst, ref output, ref upsertInfo, reason);
        }

        public override bool ConcurrentReader(ref string key, ref StorageDataEntry input, ref StorageDataEntry value, ref StorageDataEntry dst, ref ReadInfo readInfo)
        {
            var baseResult = base.ConcurrentReader(ref key, ref input, ref value, ref dst, ref readInfo);

            if (baseResult)
            {
                if (dst.ExpiresAt < DateTime.UtcNow)
                {
                    readInfo.Action = ReadAction.Expire;
                    return false;
                }
            }

            return baseResult;
        }

        public override bool ConcurrentWriter(ref string key, ref StorageDataEntry input, ref StorageDataEntry src, ref StorageDataEntry dst, ref StorageDataEntry output, ref UpsertInfo upsertInfo)
        {
            bool canProceed = false;

            if (ETagValue.IsAny(src.ETag))
            {
                canProceed = true;
            }
            else if (string.IsNullOrWhiteSpace(src.ETag))
            {
                canProceed = false;
            }
            else if (string.Equals(src.ETag, dst.ETag, StringComparison.Ordinal))
            {
                canProceed = true;
            }


            if (dst.ExpiresAt <= DateTime.UtcNow)
            {
                canProceed = true;
            }

            if (canProceed)
            {
                StorageDataEntry newEntry = new StorageDataEntry(src.Data, ETagValue.Create(), src.ExpiresAt);
                return base.ConcurrentWriter(ref key, ref input, ref newEntry, ref dst, ref output, ref upsertInfo);
            }
            else
            {
                throw new FasterConflictException();
            }
        }

        public override bool InitialUpdater(ref string key, ref StorageDataEntry input, ref StorageDataEntry value, ref StorageDataEntry output, ref RMWInfo rmwInfo)
        {
            if (!string.IsNullOrWhiteSpace(input.LockToken) && StorageDataEntry.Empty == value && input.ExpiresAt.HasValue && input.ExpiresAt < DateTime.UtcNow)
            {
                rmwInfo.Action = RMWAction.CancelOperation;
                return false;
            }
            return base.InitialUpdater(ref key, ref input, ref value, ref output, ref rmwInfo);
        }

        public override bool InPlaceUpdater(ref string key, ref StorageDataEntry input, ref StorageDataEntry value, ref StorageDataEntry output, ref RMWInfo rmwInfo)
        {
            if (!string.IsNullOrWhiteSpace(input.LockToken))
            {
                if (input.ExpiresAt < DateTime.UtcNow)
                {
                    // this is a release lock request
                    if (!input.LockToken.Equals(value.LockToken, StringComparison.Ordinal))
                    {
                        rmwInfo.Action = RMWAction.CancelOperation;
                        return false;
                    }
                    else
                    {
                        rmwInfo.Action = RMWAction.ExpireAndStop;
                        return false;
                    }
                }
                // this is an acquire lock request
                else
                {
                    if (StorageDataEntry.Empty != value && value.ExpiresAt < DateTime.UtcNow)
                    {
                        rmwInfo.Action = RMWAction.Default;
                    }
                    else if (!input.LockToken.Equals(value.LockToken, StringComparison.Ordinal))
                    {
                        rmwInfo.Action = RMWAction.CancelOperation;
                        return false;
                    }
                }
            }

            return base.InPlaceUpdater(ref key, ref input, ref value, ref output, ref rmwInfo);
        }
    }
}
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    internal class NullControlRendererEvents : IControlRendererEvents
    {
        private static object SyncLock = new object();
        private static NullControlRendererEvents StaticInstance;

        public static NullControlRendererEvents Current
        {
            get
            {
                if (null == NullControlRendererEvents.StaticInstance)
                {
                    lock (NullControlRendererEvents.SyncLock)
                    {
                        if (null == NullControlRendererEvents.StaticInstance)
                        {
                            NullControlRendererEvents.StaticInstance = new NullControlRendererEvents();
                        }
                    }
                }

                return NullControlRendererEvents.StaticInstance;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<string>> OnAssetPathsResolved(IEnumerable<string> assetPaths)
        {
            return Task.FromResult(assetPaths);
        }
    }
}
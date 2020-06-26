using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Adriva.Web.Controls.Abstractions
{
    public class WebControlsOptions
    {
        public AssetDeliveryMethod AssetDeliveryMethod { get; set; }

        public string ContainerName { get; set; }

        public PathString AssetsRootPath { get; set; } = "/webcontrols/assets";

        internal IList<Assembly> ControlLibraries { get; } = new List<Assembly>();
    }
}

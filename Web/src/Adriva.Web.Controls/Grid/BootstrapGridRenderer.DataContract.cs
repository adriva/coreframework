using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Adriva.Common.Core.Serialization.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace Adriva.Web.Controls
{
    partial class BootstrapGridRenderer
    {
        private class GridContractResolver : ControlContractResolver
        {
            protected override IList<JsonProperty> GetDynamicProperties(Type ownerType)
            {
                if (typeof(Grid) == ownerType)
                {
                    return new[] {
                        new JsonProperty(){
                            PropertyName = "pagination",
                            PropertyType = typeof(bool),
                            ValueProvider = new DynamicValueProvider<Grid, bool>(x => null != x.Pager, null),
                            Readable = true
                        },
                        new JsonProperty(){
                            PropertyName = "sidePagination",
                            PropertyType = typeof(ProcessLocation),
                            ValueProvider = new DynamicValueProvider<Grid, ProcessLocation>(x => x.Pager.ProcessLocation, null),
                            Readable = true,
                            ShouldSerialize = x => null != ((Grid)x).Pager,
                            Converter = new StringEnumConverter(new CamelCaseNamingStrategy(), true)
                        },
                        new JsonProperty(){
                            PropertyName = "queryParamsType",
                            PropertyType = typeof(string),
                            ValueProvider = new DynamicValueProvider<Grid, string>(x => string.Empty, null),
                            Readable = true,
                            ShouldSerialize = x => true
                        },
                        new JsonProperty(){
                            PropertyName = "detailViewIcon",
                            PropertyType = typeof(bool),
                            ValueProvider = new DynamicValueProvider<Grid, bool>(x => x.ShowDetails, null),
                            Readable = true,
                            ShouldSerialize = x => ((Grid)x).ShowDetails
                        },
                        new JsonProperty(){
                            PropertyName = "detailFormatter",
                            PropertyType = typeof(RawString),
                            ValueProvider = new DynamicValueProvider<Grid, RawString>(x => x.DetailsFormatter, null),
                            Readable = true,
                            ShouldSerialize = x => ((Grid)x).ShowDetails
                        },
                        new JsonProperty(){
                            PropertyName = "pageList",
                            PropertyType = typeof(string),
                            ValueProvider = new DynamicValueProvider<Grid, string>(x => $"[{x.Pager?.PageSizes ?? "10,50,100"}]", null),
                            Readable = true,
                            ShouldSerialize = x => null != ((Grid)x).Pager
                        }
                    };
                }

                return null;
            }
        }
    }
}
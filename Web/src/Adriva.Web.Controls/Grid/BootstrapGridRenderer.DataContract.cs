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
                        // new JsonProperty(){
                        //     PropertyName = "queryParams",
                        //     PropertyType = typeof(RawString),
                        //     ValueProvider = new DynamicValueProvider<Grid, RawString>(x => "function hede(p){debugger;}", null),
                        //     Readable = true,
                        //     ShouldSerialize = x => null != ((Grid)x).Pager,
                        // },
                    };
                }

                return null;
            }
        }
    }
}
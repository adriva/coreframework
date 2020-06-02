using Microsoft.Extensions.DependencyInjection;
using Adriva.Storage.Abstractions;
using System;

namespace Adriva.Storage.Azure
{
    public sealed class AzureBlobConfiguration
    {
        public string ContainerName { get; set; }
    }
}

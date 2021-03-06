# Adriva Core Framework

[![Build status](https://adriva.visualstudio.com/NetCoreLibs/_apis/build/status/Publish%20Framework%20To%20GitHub)](https://adriva.visualstudio.com/NetCoreLibs/_build/latest?definitionId=48)
&nbsp;&nbsp;&nbsp;
[![.NET Core](https://github.com/adriva/coreframework/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/adriva/coreframework/)
&nbsp;&nbsp;&nbsp;
[![GitHub](https://img.shields.io/github/license/adriva/coreframework)](https://github.com/adriva/coreframework/blob/master/LICENSE#blob-path)
\
&nbsp;
\
Adriva Core Framework is a set of common libraries that can be reused in .NET projects.\
This project targets ***netstandard2.1***.
\
&nbsp;
\
[Documentation](https://adriva.github.io/coreframework/)
\
&nbsp;
## Projects in Solution

| Project | Description |
|-|-|
| Adriva.Common.Core        | Provides shared utility methods, global enums and common classes that can be used in any project type targeting netstandard2.1 and higher. Also consumed by other framework libraries within this repository.|
| Adriva.Extensions.Caching.Abstractions | Caching abstractions for memory, distributed or any other custom cache.  |
| Adriva.Extensions.Caching.Memory | In memory cache storage. |
| Adriva.Extensions.Optimization.Abstractions |  Optimization abstractions for file optimization modules. |
|Adriva.Extensions.Optimization.Transforms|Shared transforms that can be used with Adriva.Extensions.Optimization.Abstractions.|
|Adriva.Extensions.Optimization&period;Web | Customizable transforms and tag helpers to create and consume optimized resources in <span>asp.net<span> Core applications.|
|Adriva.Extensions.Analytics.AppInsights |Microsoft AppInsights client wrapper library that can use custom server implementations.|
|Adriva.Extensions.Analytics.Server |Base analytics server that can be used with virtually any analytics client and store incoming analytics data in virtually any repository.|
|Adriva.Extensions.Analytics.Server.AppInsights |Analytics server services that is used to parse and store incoming Microsoft AppInsights data in virtually any data storage including on-premise systems.|
|Adriva.Extensions.Analytics.Repository.EntityFramework|Base implementation of an analytics data repository using Entity Framework Core which other EF Core repositories can derive from. Also provides an in-memory repository for development and testing purposes.|
|Adriva.Extensions.Analytics.Repository.EntityFramework.Sqlite|Entity Framework Core based Sqlite analytics data repository.|

### Projects Running On Adriva Core Framework
* [adriva.com](https://adriva.com)
* [jarrt.com](https://jarrt.com)
* [zargan.com](http://www.zargan.com)
* [Tayf](https://tayf.adriva.com)
* [Petrol Ofisi](#) - Url not disclosed
* [Protel](#) - Url not disclosed
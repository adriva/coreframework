# Adriva Analytics Server

## Overview

```Adriva.Extensions.Analytics.Server``` library provides a base server implementation that provides the common functionality to listen for incoming analytics data, parse and store the data in virtually any repository.

```Adriva.Extensions.Analytics.Server.AppInsights``` library provides the functionality to parse incoming Microsoft AppInsights data and convert it to a common format. You can then store the output data in your custom on-premise repository or any other repository of your choice.

## Flow

This section describes the basic flow of the ```Adriva.Extensions.Analytics.Server``` in order to customize the analytics server and add support for any incoming analytics data or persisting analytics data in your own repository.

There are 3 main components in the server side flow. Which are:

- Middleware
- Handler
- Repository

### Middleware

Analytics middleware captures the incoming requests and tries to parse and queue them for further processing. Middleware should return Http status code 204 for success. The default middleware is an internal class and cannot be customized.

### Handler

Handler is a concrete implementation of ```Adriva.Extensions.Analytics.Server.IAnalyticsHandler``` interface and is responsible of processing the incoming ```HttpRequest``` object and returning a collection of ```Adriva.Extensions.Analytics.Server.Entities.AnalyticsItem``` objects. Please take a look at the default implementation of [AppInsightsHandler](https://github.com/adriva/coreframework/blob/master/Analytics/src/Adriva.Extensions.Analytics.Server.AppInsights/AppInsightsHandler.cs) class. A handler is basically is a parser. The default implementation of ```AppInsightsHandler``` parses the incoming **Microsoft AppInsights** requests and converts them to a common format to be stored in a repository.

Common entities (common format) is defined in the ```Adriva.Extensions.Analytics.Server.Entities``` namespace which can be found [here](https://github.com/adriva/coreframework/tree/master/Analytics/src/Adriva.Extensions.Analytics.Server/Entities).

In the common data model the root object is called ```AnalyticsItem``` all other types are linked to a parent ```AnalyticsItem``` class. For example a ```RequestItem``` which stores the data for a request analytics data is the child of an ```AnalyticsItem``` class. Please take a look at the definition of the [AnalyticsItem](https://github.com/adriva/coreframework/blob/master/Analytics/src/Adriva.Extensions.Analytics.Server/Entities/AnalyticsItem.cs) class.

### Repository

Once the incoming data is parsed and converted to the common format successfully and added to the buffer, the system will try to persist it in a repository. All default repository libraries provided in this frame work can be found [here](https://github.com/adriva/coreframework/tree/master/Analytics/src) and all repository libraries start with the prefix ```Adriva.Extensions.Analytics.Repository```. Support for custom repositories can be very useful when you need to store analytics data on your own servers, such as storing Microsoft AppInsights data on an on-premise database.
A custom repostitory should implement the ```Adriva.Extensions.Analytics.Server.IAnalyticsRepository``` interface and the simplest repository implementation, which actually doesn't do anything, can be found [here](https://github.com/adriva/coreframework/blob/master/Analytics/src/Adriva.Extensions.Analytics.Server/NullRepository.cs).

## asp&period;net Core Service Configuration

In order to use the analytics server-side services we need to register them in the application service collection and add the middleware to the application pipeline.

Here is a sample that uses the default AppInsights handler and an in-memory entity framework repository.

The following code registers the required services:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddAppInsightsAnalyticsServer(builder =>
    {
        ...
        builder
            .SetProcessorThreadCount(1)
            .SetBufferCapacity(100)
            .UseInMemoryRepository()
        ;
        ...
    });
}
```

>```SetProcessorThreadCount``` tells the system how many threads will be allocated to verify and store the buffered data on the server. This value cannot be less than **1**.

>```SetBufferCapacity``` tells the system how many items can be buffered berfore they are persisted in the repository. This value is not a hard limit and system may decide to flush the buffer before it is full.

The following code adds the middleware to the pipeline:
```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseAnalyticsServer("/analytics");
    ...
}
```
>```UseAnalyticsServer``` method accepts a string parameter which is the endpoint (path) it will listen for incoming requests. The system will add a ```/track``` suffix to the given base path so that the final relative url to send the analytics data will be ```[/prefix]/track``` which is ```/analytics/track``` in our example.
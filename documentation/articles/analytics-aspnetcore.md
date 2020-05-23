# Using Analytics in Asp&period;net Core

```Adriva.Extensions.Analytics.AppInsights``` library provides extension methods and wrapper services around ```Microsoft.AppInsights``` client. The client library supports both generic hosting model and asp&period;net core hosting model.

You can use the client in your own asp&period;net core application to persist AppInsights data in your own repository including any custom on-premise systems.

## Service Configuration

To use the client services in your application, you need to register the required analytics services first.
Since this is a web application we need to use the ```AddAppInsightsWebAnalytics``` method to register all required services in our application.

```
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddAppInsightsWebAnalytics(options =>
    {
        options.InstrumentationKey = "<YOUR KEY GOES HERE>";
        options.IsDeveloperMode = true | false;
        options.Capacity = 5000;
        options.EndPointAddress = "https://localhost:5001/analytics/track";
    });

    ...
}
```
|Property|Description|
|-|-|
|InstrumentationKey|The key value that uniquely identifies the application. The value of this key depends on the server implementation and the default implementation doesn't provide any obfuscation or security on this key. |
|Capacity|The number of items that can be stored locally before trasnmitting to the server.|
|EndPointAddress|Your http or https analytics server endpoint url.|

This is all you need to start capturing and transmitting ```Microsoft.AppInsights``` formatted analytics data to your server implementation.

Since the services used in this example are wrappers around the existing AppInsights library, you can still use any ```Microsoft.AppInsights``` features and services in your application.

## Advanced Usage

```AnalyticsOptions``` class using in ```AddAppInsightsWebAnalytics``` method also have two actions that you can use to initialze analytics data or filter out unwanted data so that they will never be submitted to the server.

```Initialize``` means populating analytics data with some extra data that's meaningful to you.
The action provided will be called by the system right after analytics data (in this example, AppInsights telemetry data) is created. The system will pass the current ```IServiceProvider``` instance and created ```ITelmetry``` objects to your action so you can modify the data if you need so.

Example:

```
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddAppInsightsWebAnalytics(options =>
    {        
        options.Initializer = (serviceProvider, analyticsData) => {
            
        };
    });

    ...
}
```

```Filter``` is a function such as  Func<ITelemetry, bool> which is called by the system to see if the given data is allowed to be transmitted to the server. If this function returns ```false``` then the analytics data will be ignored.

Example:

```
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddAppInsightsWebAnalytics(options =>
    {        
        options.Filter = (analyticsData) => {
            if (<YOUR LOGIC HERE>) return false;
            return true;
        };
    });

    ...
}
```
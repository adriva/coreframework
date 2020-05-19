# Optimization in ASP&period;net Core 

Adriva Core Framework provides the base infrastructure to optimize resources (called assets) and deliver them to the client in runtime. In other words, asset optimization in web applications, such as bundling and minification happens in runtime rather than compile or build time.

In the default implementation Adriva.Extensions.Optimization.Web library optimizes the assets when requested, optionally stores them on the file system and relies on static file middleware to deliver them to the client once requested or renders them inline.

To get started you can either clone and build the [Adriva Core Framework](https://adriva.github.io/coreframework/) repository or download packages from [Nuget](https://www.nuget.org/profiles/adriva).


## Setting Things Up

To use static asset optimization in a web application (such as javascript and css files) first we need to register the optimization services using the `AddOptimization` extension method.

```csharp
public void ConfigureServices(IServiceCollection services){
    services.AddOptimization(options =>
    {
        options.BundleStylesheets = true; // enable bundling of CSS files
        options.MinifyStylesheets = true; // enable minifaction of css files
        options.BundleJavascripts = true; // enable bundling of js files
        options.MinifyJavascripts = true; // enable minification of js files
        options.MinifyHtml = true; // enable minification of html response
    });
}
```

If HTML response minification is enabled then we need to add the `OptimizationMiddleware` to the ASP&period;net pipeline.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseResponseCompression();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    // we need to make sure the optimization middleware is registered 
    // before any other middleware that produces HTML output such as MVC    
    app.UseOptimization(); 

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

For static assets we need to provide the optimization module with some information about which assets to use per request. In order to be able to do that we will rely on the `IOptimizationContext` service provided by the optimization module. Each optimization context is a part of a parent optimization scope. So in other words, one optimization scope may have 1 or more contexts inside it.
This feature is particularly useful when you need to have multiple output resources that are reused in some other parts of your application.

So wee need to get a reference to the current ```IOptimizationScope``` and either use the default context or create child contexes.

Also we'll need a mechanism to render (deliver) the optimized assets, which is managed by tag helpers provided. So in the _ViewImports.cshtml file we can add the following lines:

```csharp
// inject the optimization scope into the views
@inject Adriva.Extensions.Optimization.Abstractions.IOptimizationScope OptimizationScope

// allow using optimization tag helpers in the views
@addTagHelper *, Adriva.Extensions.Optimization.Web
```

## Using The Optimization System 

After setting thing up, in the view files we can tell the system what static assets we will need by adding them to the default (or custom) `OptimizationContext`. `OptimizationScope` is a scoped service that lives through a single request so are all ```OptimizationContexts```. For example in the `_Layout.cshtml` view file we can write the following:

```csharp
IOptimizationContext oc = this.OptimizationScope.AddOrGetContext("MainContext");
    oc.AddAsset("~/js/jquery.js");
    oc.AddAsset("~/js/site.js");
    this.OptimizationScope.DefaultContext.AddAsset("https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.css");
```
>To make sure the custom contexts are accessible everywehere in your view, place the sample code at the top of your view file, before any Html tags.

As you can see, we can include assets from the local file system or any valid *http* or *https* url.
`OptimizationContext` makes sure the same asset is not included twice, so in a scenario where you include `jquery.js` in the layout and re-include the same asset in a sub view, there will be only one `jquery.js` included in the current `OptimizationContext`.

The final part is the rendering of optimized assets. And in order to achieve that we will use the `<optimizedresource>` tag helper in our view file.

For example to deliver the optimized `js` files we can write:

```csharp
<optimizedresource context="oc" extension="js" output="StaticFile" />
```

or for css files:

```csharp
<optimizedresource context="@this.OptimizationScope.DefaultContext" extension="css" output="StaticFile" />    
```
The ```context``` attribute tells the Optimization module which context of the current ```OptimizationScope``` to use. You can think of contexts as bundles and scope as a bundle repository.

The `extension` attribute tells the Optimization module what type of resources are to be delivered by the current optimized resource tag and `output` attribute tells *HOW* to deliver them.

The valid values for the `output` attribute can be found [here](https://adriva.github.io/coreframework/api/Adriva.Extensions.Optimization.Web.OptimizationTagOutput.html) .

If you prefer to use the `StaticFiles` method as the output delivery option then the system will create asset files for all different `OptimizationContext`s.
\
\
*Note that, if two different `OptimizationContext`s include the same assets they are considered to be the same.*
\
\
Static asset output files will be stored in the web root path in a folder named *assets*.
If you prefer to use a different folder you can do it by setting the options. 

```csharp
services.AddOptimization(options =>
{
    // this the storage folder that will be used to serve optimized output files
    // it's relative to the web content path and must start with '/'.
    options.StaticAssetsPath = "/some/other/folder";
    options.BundleStylesheets = true;
    options.MinifyStylesheets = true;
    options.BundleJavascripts = true;
    options.MinifyJavascripts = true;
    options.MinifyHtml = true;
});
```

You can find the full source code in [Adriva Core Framework repository](https://github.com/adriva/coreframework/tree/master/Optimization/src/Adriva.Extensions.Optimization.Web) on github.
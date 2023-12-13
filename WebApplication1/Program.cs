using Contentful.Core.Configuration;
using Contentful.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IContentfulClient>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();

    return new ContentfulClient(
        httpClient,
        builder.Configuration["ContentfulOptions:DeliveryApiKey"],
        builder.Configuration["ContentfulOptions:PreviewApiKey"],
        builder.Configuration["ContentfulOptions:SpaceId"]
    );
});


// Configure Contentful Management API Client
builder.Services.AddSingleton<IContentfulManagementClient>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    return new ContentfulManagementClient(
        httpClient,
        builder.Configuration["ContentfulOptions:ManagementApiKey"],
        builder.Configuration["ContentfulOptions:SpaceId"]
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

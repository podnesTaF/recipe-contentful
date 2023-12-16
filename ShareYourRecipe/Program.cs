using Contentful.Core.Configuration;
using Contentful.Core;
using System.Net;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Configure Contentful Delivery API Client
builder.Services.AddSingleton<IContentfulClient>(serviceProvider =>
{
  var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
  var httpClient = httpClientFactory.CreateClient();

  var deliveryApiKey = builder.Configuration["ContentfulOptions:DeliveryApiKey"];
  var previewApiKey = builder.Configuration["ContentfulOptions:PreviewApiKey"];
  var spaceId = builder.Configuration["ContentfulOptions:SpaceId"];

  return new ContentfulClient(
      httpClient,
      deliveryApiKey,
      previewApiKey,
      spaceId
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

builder.WebHost.ConfigureKestrel(serverOptions =>
{
  serverOptions.ListenAnyIP(80);
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
    pattern: "{controller=Recipe}/{action=Index}/{id?}");

app.Run();

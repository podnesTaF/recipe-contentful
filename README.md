# ASP.NET web app using Contentful headless CMS

### Author: Oleksii Pidnebesnyi | r0934777

### Technologies

1. Figma
2. Contentful
3. Contentful content delivery and content management APIs
4. ASP.NET
5. Tailwind
6. Docker

### GitHub repo:

Navigate to the GitHub Repo of the [project](https://github.com/podnesTaF/recipe-contentful)

### Demo and explanation

The link to youtube video: [Using contentful CDA and CMA to create a Recipe Website]()

## Table of contents

- [Intoduction](#introduction)
- [Using Contentful CMS to manage API](#using-contentful-cms-to-manage-api)
- [Figma UI design](#figma-ui-desing)
- [ASP.NET with Contentful](#aspnet-with-contentful)
- [Recipe and details pages: using contentful CDA](#recipe-and-details-pages-using-contentful-cda)
- [Create Recipe: using contentful CMA](#create-recipe-using-contentful-cma)
- [Additional features](#additional-features)
- [Dockerization](#dockerization)
- [CI/CD with GitHub Actions](#cicd-with-github-actions)
- [Conclusion](#conclusion)
- [References](#references)

## Introduction

"Share Your Recipe" – a simple and fun website where you can discover new recipes, see detailed cooking instructions, and share your own favorite recipes with others. It's a friendly spot for everyone who loves to cook and wants to explore more in the kitchen.

## Using Contentful CMS to manage API

### Overview

This section outlines the integration of Contentful, a powerful headless Content Management System (CMS), into our application. Contentful offers a flexible and developer-friendly platform to manage and deliver content through APIs, making it an ideal choice for dynamic, content-driven web applications.

### Setting Up Contentful

1. **Account Creation**: Registering on the Contentful website to gain access to the Contentful Management Console.
2. **Workspace Initialization**: Creating a new workspace within Contentful to house my project's content structures and data.

### Creating a content type

The only entity I needed for the website is recipe.
It has this fields:

- **Title** (String): The name of the recipe.
- **Description** (String): A detailed description of the recipe, including - - **preparation** steps and other relevant information.
- **PreparationTime** (String): The estimated time required to prepare the dish.
- **Energy** (String): Caloric information or energy content of the recipe.
- **GramsPerServing** (String): The weight or quantity per serving.
- **Image** (Media): A media field to upload and associate an image with each recipe. This visual representation enhances user engagement.
- **Author** (String): The name of the individual who created or submitted the recipe.

To demonstrate the functionality of the application, approximately six recipes were created and populated within Contentful.

## Figma UI desing

To create a simple but visually appealing website I used Figma.
The main three pages:

- Recipes (main) page: Shows recipes previews, seach and pagination
- Recipe Details: Shows full recipe details
- Create Recipe: A form to create recipe.

the link to figma: [ShareYourRecipe](https://www.figma.com/file/GYQk3KQDTKIbBsLNc9jzSO/ShareYourRecipe?type=design&node-id=0%3A1&mode=design&t=xCbnn90VxhcqLRxy-1)

## ASP.NET with Contentful

### Setting Up the Project

- **Initial Setup**: Created an ASP.NET MVC project in Visual Studio named "ShareYourRecipe".
- **NuGet Package**: Installed the contentful.aspnetcore package via NuGet Package Manager to facilitate the integration with Contentful.

### Recipe and details pages: using contentful CDA

Configuring Contentful CDA Client

**API Keys**: Added the following API keys to appsettings.json for accessing Contentful's Content Delivery API (CDA):

- DeliveryApiKey
- ManagementApiKey
- PreviewApiKey

**Program.cs Configuration**: Set up the IContentfulClient in Program.cs to utilize these keys, enabling API communication with Contentful:

```cs
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
```

#### Main Page Implementation

I created a RecipeController in the Controllers. Wraped it in the `ShareYourRecipe.Controllers`namespace and pass `IContentfulClient client` in the arguments of the class. I can now send get requests to my contentful workspace.

Before creating the first controller, I had to create **recipe model**. While reading contentful documentation, I found that all models has a `SystemProperties Sys`, so I add the property too:

```cs
public class Recipe
{
  public required SystemProperties Sys { get; set; }
  public required string Title { get; set; }
  public required string Description { get; set; }
  public string? PreparationTime { get; set; }
  public string? Energy { get; set; }
  public required string GramsPerServing { get; set; }
  public required string Author { get; set; }
  public Asset? Image { get; set; }
}
```

**Index Controller**: Implemented the Index action in RecipeController to fetch and display recipes:

```cs
public async Task<IActionResult> Index()
  {
    // Build a request with query builder to road the Image relation of Recipe
    var queryBuilder = new QueryBuilder<Recipe>().ContentTypeIs("recipe");

    var recipes = await client.GetEntries(queryBuilder);

    return View(recipes);
  }
```

**View Customization**

- Modified \_Layout.cshtml to include Tailwind CSS instead of Bootstrap.
- Adjusted the site title format for consistency.
- Developed the Index.cshtml view for the Recipe model, displaying recipe cards with details:

```cs
@model RecipeView

@{
    ViewData["Title"] = "Main";
}
```

```html
@foreach (var recipe in Model.Recipes)
  {
    <div class="w-full md:w-[300px] border-[1px] border-gray-300 shadow-md rounded-md flex flex-col items-center gap-4">
    <img src="@recipe.Image?.File?.Url" alt="@recipe.Image?.Title" class='object-cover object-center w-full h-48' />
    <div class="py-2 px-4 w-full">
        <h2 class="text-xl font-semibold">@recipe.Title</h2>
        <p class="text-[#64C9C9] mb-2">@StringHelper.TruncateDescription(recipe.Description, 80)</p>
        <div class="flex justify-end">
          <a href="@Url.Action("Details", "Recipe", new { id = recipe.Sys.Id })" class="font-semibold text-black px-3 py-2 border-2 border-[#64C9C9] hover:opacity-90 rounded-md">View Recipe</a>
        </div>
    </div>
    </div>
  }
```

I want to see the result on the `/` route, but now It's accessible on the `/recipe` route. So I mapped the controller route in my `Program.cs` file:

```cs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Recipe}/{action=Index}/{id?}");
```

Finally, I can see the main page with multiple cards of recipes.

I think you mention than I added link to the details page in cards, so let see how I created recipe page.

#### Details page

To get the details page, I first had to add Details method to the Recipe controller and pass `id` as an argument.
To get a single recipe with image populated, I had to take several steps:

- Create a query builer and filter by Id.
- Get recipes by queryBuilder
- See if recipes array is not empty
- Since the id is the primary key on the entry there can be only one recipe in the array. So I put it in the variable recipe and push the recipe to the view.

```cs
 public async Task<IActionResult> Details(string id)
    {
      var queryBuilder = new QueryBuilder<Recipe>().ContentTypeIs("recipe").FieldEquals("sys.id", id);
      var recipes = await client.GetEntries(queryBuilder);

      if (recipes == null || recipes.Count() == 0)
      {
        return NotFound();
      }

      var recipe = recipes.First();

      return View(recipe);
    }
```

- **Details View**: Created Details.cshtml in the Recipe views folder to display the full details of a selected recipe.

Now I can navigate to the recipe details page by clicking "Details" button on a recipe card.

#### Conclusion

The ASP.NET MVC application, "ShareYourRecipe," successfully integrates with Contentful CMS to fetch and display recipes. The site features a main page with a list of recipes and individual detail pages for each recipe.

### Create Recipe: using contentful CMA

The Contentful Management API (CMA) offers programmatic control over content within the CMS. The setup involved adding a management key to appsettings.json and configuring the **ContentfulManagementClient** in **Program.cs**.

```cs
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
```

I can now use the management client to controll the content directly from my website.

#### Implementing the Recipe Creation Form

First of all I created a RecipeViewModel to validate fields:

```cs
public class RecipeViewModel
  {
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string PreparationTime { get; set; }
    public required string Energy { get; set; }
    public required string GramsPerServing { get; set; }
    public required string Author { get; set; }
    public IFormFile? Image { get; set; }
  }
```

The Create controller method displays a form that allows users to input recipe details. The form is designed to handle various data types, including media files for images.

```cs
public IActionResult Create()
{
      return View();
}
```

- I have created a `Create.cshtml` view inside Recipe.
- I added validation and title:

```cs
@{
    ViewData["Title"] = "Create Recipe";
}

@if (!ViewData.ModelState.IsValid)
{
    <div class="alert alert-danger">
        <ul>
            @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
            {
                <li>@error.ErrorMessage</li>
            }
        </ul>
    </div>
}
```

Then I made quite large form to collect all necessary data for the view. Here is the preview:

```html
<form asp-action="CreateRecipe" asp-controller="Recipe" method="post"  enctype="multipart/form-data">
  <div class="flex flex-col md:flex-row justify-between gap-8">
    <div class="w-full md:w-1/2 flex flex-col gap-4">
    <div class="w-full flex flex-col">
        <label class="input-label" asp-for="Title">Recipe Title</label>
        <input asp-for="Title" class="form-input" placeholder="e.g pasta carbonara" />
    </div>
    <!-- other fields -->
</form>
```

As you see the encryption type is `multipart/form-data` to correcly collect the image file for the recipe.

#### Backend Logic for Recipe Creation

The **CreateRecipe** method in the Recipe controller handles the creation and publication of a new recipe. It involves generating a new entry with a unique ID, assigning fields to the entry, and handling the image upload and publication process.
I specified the type of request as `HttpPost`. I passed the model of type `RecipeViewModel` as an argument of the method.

I checked If the ModelState is valid, otherwise return view with invalid state:

```cs
[HttpPost]
public async Task<IActionResult> CreateRecipe(RecipeViewModel model)
{
  if (!ModelState.IsValid)
  {
    return View("Create", model);
  }
}
```

In the Contentful CMA documentation I read about the way to implement the entry creation. It's not straightforward at all:

```cs
  // create an entry, generate a random id
  var entry = new Entry<dynamic>();
  entry.SystemProperties = new SystemProperties
  {
    Id = Guid.NewGuid().ToString(),
    Type = "Entry",
    Version = 1
  };

  // specify fields
  dynamic fields = new ExpandoObject();
  fields.Title = new Dictionary<string, string> { { "en-US", model.Title } };
  fields.Description = new Dictionary<string, string> { { "en-US", model.Description } };
  // and others beside Image
```

Another complicated thing was to create and publish an Image. In the comments I explained each piece of the code to create and publish file in contentful:

```cs
try
  {

    // Convert the uploaded image to a byte array.
    if (model.Image != null)
    {
      byte[] imageBytes;
      using (var ms = new MemoryStream())
      {
        await model.Image.CopyToAsync(ms);
        imageBytes = ms.ToArray();
      }

      // Generate a unique ID for the new asset in Contentful
      var assetId = Guid.NewGuid().ToString();

      // Create a new instance of ManagementAsset to hold the image details.
      var managementAsset = new ManagementAsset();

      // Initialize SystemProperties for the management asset and set its ID and version.
      managementAsset.SystemProperties = new SystemProperties();
      managementAsset.SystemProperties.Id = assetId;
      managementAsset.SystemProperties.Version = 1;

      // Extract the file name from the uploaded image.
      var fileName = Path.GetFileName(model.Image.FileName);

      // Extract the content type (MIME type) of the uploaded image.
      var contentType = model.Image.ContentType;


      // Set the title of the asset (in this case, using the file name).
      managementAsset.Title = new Dictionary<string, string> {
            { "en-US", fileName}
        };

      // Set the file details for the asset, including content type and file name.
      managementAsset.Files = new Dictionary<string, File>
        {
            { "en-US", new File() {
                    ContentType = contentType,
                    FileName = fileName
                }
            }
        };

      // Upload the image file to Contentful and create a corresponding asset.
      var newImage = await managementClient.UploadFileAndCreateAsset(managementAsset, imageBytes);

      // Retrieve the ID of the newly created asset.
      var imageId = newImage.SystemProperties.Id;

      // Publish the asset.
      var res = await managementClient.PublishAsset(imageId, 2);

      // Assign the published asset to the 'fields.Image' property.
      fields.Image = new Dictionary<string, Asset>()
      {
        { "en-US", new Asset
      {
        SystemProperties = new SystemProperties
        {
          Id = imageId,
          Type = "Link",
          LinkType = "Asset"
        }
      }}
      };
    }
    else
    {
      // If no image is uploaded, add an error message to the ModelState.
      ModelState.AddModelError(string.Empty, "Each recipe has to have an Image");
      return View("Create", model);
    }
  }
  catch (Exception ex)
  {
      // Catch any exceptions during the process, add error messages to ModelState.
    ModelState.AddModelError(string.Empty, "An error occurred while uploading image" + ex.Message);
    // Return to the view with the current model to display the error
    return View("Create", model);
  }
```

Having Image inside fields of the recipe entry, I next finishing the creation and publishing of a recipe.

```cs
try
  {
    // assign fields
    entry.Fields = fields;

    //
    var response = await managementClient.CreateOrUpdateEntry(entry, contentTypeId: "recipe");

    // retrieve entryId and version to publish entry
    var entryId = response.SystemProperties.Id;
    var version = 1;

    // Publishing entry
    var publishedEntry = await managementClient.PublishEntry(entryId, version);

    // in case of success - redirect to the main page
    return RedirectToAction("Index");
  }
  catch (Exception ex)
  {
    ModelState.AddModelError(string.Empty, "An error occurred while creating entity: " + ex.Message);
    return View("Create", model);
  }
```

The newly created recipe immediately appeared on the main page.

### Additional Features

To enhance the application, features like recipe search and pagination were implemented on the main page.

#### Search Recipe

The recipe search feature was integrated into the Index method of the Recipe controller. It allows filtering recipes based on the title.

First of all I added `string searchString = ""` as an argument of the Index method.

I checked If the searchString is empty and if not the filtering will be applied to the queryBuilder:

```cs
if (!string.IsNullOrEmpty(searchString))
{
  queryBuilder = queryBuilder.FieldMatches("fields.title", searchString);
}
```

I created RecipeView model and put there two attributes for now. The model was passed to the view:

```cs
 var viewModel = new RecipeView
  {
    Recipes = entries,
    SearchString = searchString
  };

  return View(viewModel);
```

I edited Recipe view by adding a search field and button to seach and reset:

```cs
<form method="get" class="flex items-center gap-2" action="@Url.Action("Index", "Recipe")">
  <input type="text" name="searchString" class="form-input" value="@Model.SearchString" placeholder="Search by title..." />
  <button type="submit" class="py-1 px-2 rounded bg-[#64C9C9] shadow-md text-xl font-semibold text-white hover:opacity-90 active:scale-95">Search</button>
  @if (!string.IsNullOrWhiteSpace(Model.SearchString))
  {
      <button type="button" onclick="window.location.href='@Url.Action("Index", "Recipe")'">Reset</button>
  }
</form>
```

#### Pagination

Pagination was implemented to manage the display of recipes efficiently. This involved modifying the Index method to handle page numbers and limit the number of recipes displayed per page.

To implement pagination I had to add several things to my controller:

1. Add `int page = 1` to the props of recipe controller.
2. Define the limit of recipes I want to see for a page and find out how many recipes should I skip:

```cs
const int pageSize = 6;
var skip = (page - 1) * pageSize;
```

3. Apply them on the query builder:

```cs
var queryBuilder = new QueryBuilder<Recipe>().ContentTypeIs("recipe")
.Limit(pageSize)
.Skip(skip);
```

4. Get the Total pages:

```cs
  var totalRecipes = entries.Total;
  var totalPages = (int)Math.Ceiling((double)totalRecipes / pageSize);
```

5. Update the RecipeView model to contain currentPage and Total pages attributes:

```cs
var viewModel = new RecipeView
{
  Recipes = entries,
  CurrentPage = page,
  TotalPages = totalPages,
  SearchString = searchString
};
```

I added some styling to the view of the main page and handle clicks on the pages. Example:

```html
@for (int i = 1; i <= Model.TotalPages; i++)
{
  <a asp-action="Index"
    asp-route-page="@i"
    asp-route-searchString="@Model.SearchString"
    class="@(i == Model.CurrentPage ? "px-4 py-2 text-white bg-[#64C9C9] rounded-md" : "px-4 py-2 text-blue-600 bg-white hover:bg-blue-100 rounded-md")">
      @i
  </a>
}
```

## Dockerization

Dockerization refers to the process of packaging an application along with its dependencies and configurations into a Docker container. This chapter describes how the "ShareYourRecipe" ASP.NET MVC application was containerized using Docker, ensuring a consistent and isolated environment for deployment.
Below is a breakdown of the **Dockerfile** created for the project, explaining how each part contributes to the application's Dockerization.

### Dockerfil explained

- base image

```docker
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
```

- SDK Image for Building the Application

```docker
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShareYourRecipe.csproj", "./"]
RUN dotnet restore "./ShareYourRecipe.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ShareYourRecipe.csproj" -c Release -o /app/build
```

- Publishing the Application

```docker
FROM build AS publish
RUN dotnet publish "ShareYourRecipe.csproj" -c Release -o /app/publish
```

- Final Runtime Image

```docker
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShareYourRecipe.dll"]
```

### Building and Running the Docker Container

- Building the Docker Image

`docker build -t shareyourrecipe .`

- Running the Docker Container

`docker run -d -p 8080:80 shareyourrecipe`

With the container running, the "ShareYourRecipe" application should be accessible through the web browser at http://localhost:8080.

### Conclusion

By following these steps, the "ShareYourRecipe" application is fully dockerized, allowing for a consistent and isolated environment.

## CI/CD with GitHub Actions

### Setting Up GitHub Actions

GitHub Repository and Workflow Creation

- **GitHub Repository**: The code for the "ShareYourRecipe" application was stored in a GitHub repository, which allows the integration of GitHub Actions for CI/CD.
- **Workflow Directory**: Created a .github/workflows directory in the repository to store the workflow configuration files.
- **CI/CD Configuration File**: Developed a CI/CD pipeline configuration named ci-cd.yaml within the workflows directory.

**The CI/CD Workflow**:

```yaml
# This section specifies when the workflow will run.
# The workflow triggers on every push to the 'main' branch.
on:
  push:
    branches:
      - main

# Defines a job named 'build-and-push'.
jobs:
  build-and-push:
    # Specifies that the job will run on the latest Ubuntu runner.
    runs-on: ubuntu-latest
    steps:
      # Checks out the repository code, making it available to the workflow.
      - name: Check Out Repo
        uses: actions/checkout@v2

      # Logs into Docker Hub using the secrets stored in the GitHub repository.
      # DOCKER_HUB_USERNAME and DOCKER_HUB_ACCESS_TOKEN need to be set in the repository's secrets.
      - name: Log in to Docker Hub
        uses: docker/login-action@v1
        with:
          username: ${{ secrets.DOCKER_HUB_USERNAME }}
          password: ${{ secrets.DOCKER_HUB_ACCESS_TOKEN }}

      # Determines if there are any changes in the 'ShareYourRecipe' directory.
      # This step helps to avoid unnecessary builds when there are no changes in the application code.
      - name: Determine Changed Paths
        id: changed-files
        uses: dorny/paths-filter@v2
        with:
          filters: |
            project:
              - 'ShareYourRecipe/**'

      # Builds and pushes the Docker image only if there are changes in the 'ShareYourRecipe' directory.
      - name: Build and Push Project Docker Image
        if: steps.changed-files.outputs.project == 'true' # Conditional step based on the output of the previous step.
        run: |
          # Builds a Docker image from the Dockerfile in the 'ShareYourRecipe' directory and tags it.
          docker build -t podnes/share-your-recipe ./ShareYourRecipe
          # Pushes the built Docker image to Docker Hub.
          docker push podnes/share-your-recipe:latest
```

## Conclusion

Contentful stands out as a powerful and flexible tool for content management. Its headless, API-first approach aligns perfectly with modern web development practices, offering scalability, security, and a seamless content management experience. Whether for small projects or large-scale enterprise applications, Contentful provides the foundation for efficient and innovative content strategies.

## References

1. [Using Contentful CMS with ASP.Net Core](https://www.youtube.com/watch?v=9EX0uB9dcbM)
2. [Content Management API](https://www.contentful.com/developers/docs/references/content-management-api/)
3. [Getting Started with Contentful and .NET](https://www.contentful.com/developers/docs/net/tutorials/using-net-cda-sdk/)
4. [Using the Management API with Contentful and .NET](https://www.contentful.com/developers/docs/net/tutorials/management-api/)

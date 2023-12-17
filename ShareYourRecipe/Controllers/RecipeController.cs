using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Core.Search;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Net.Mime;
using ShareYourRecipe.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ContentType = Contentful.Core.Models.ContentType;
using File = Contentful.Core.Models.File;

namespace ShareYourRecipe.Controllers
{

  public class RecipeController(IContentfulClient client, IContentfulManagementClient managementClient) : Controller
  {
    public async Task<IActionResult> Index(string searchString = "", int page = 1)
    {
      const int pageSize = 6;
      var skip = (page - 1) * pageSize;

      var queryBuilder = new QueryBuilder<Recipe>().ContentTypeIs("recipe")
                                                 .Limit(pageSize)
                                                 .Skip(skip);

      if (!string.IsNullOrEmpty(searchString))
      {
        queryBuilder = queryBuilder.FieldMatches("fields.title", searchString);
      }

      var entries = await client.GetEntries(queryBuilder);
      var totalRecipes = entries.Total;

      var totalPages = (int)Math.Ceiling((double)totalRecipes / pageSize);

      var viewModel = new RecipeView
      {
        Recipes = entries,
        CurrentPage = page,
        TotalPages = totalPages,
        SearchString = searchString
      };

      return View(viewModel);
    }
    public IActionResult Create()
    {
      return View();
    }

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

    [HttpPost]
    public async Task<IActionResult> CreateRecipe(RecipeViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View("Create", model);
      }

      var entry = new Entry<dynamic>();
      entry.SystemProperties = new SystemProperties
      {
        Id = Guid.NewGuid().ToString(),
        Type = "Entry",
        Version = 1
      };

      dynamic fields = new ExpandoObject();
      fields.Title = new Dictionary<string, string> { { "en-US", model.Title } };
      fields.Description = new Dictionary<string, string> { { "en-US", model.Description } };
      fields.PreparationTime = new Dictionary<string, string> { { "en-US", model.PreparationTime } };
      fields.Energy = new Dictionary<string, string> { { "en-US", model.Energy } };
      fields.GramsPerServing = new Dictionary<string, string> { { "en-US", model.GramsPerServing } };
      fields.Author = new Dictionary<string, string> { { "en-US", model.Author } };


      // Handle image uploading
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
        ModelState.AddModelError(string.Empty, "An error occurred while uploading image " + ex.Message);
        // Return to the view with the current model to display the error
        return View("Create", model);
      }
      try
      {
        entry.Fields = fields;
        var response = await managementClient.CreateOrUpdateEntry(entry, contentTypeId: "recipe");
        var entryId = response.SystemProperties.Id;
        var version = 2;
        var publishedEntry = await managementClient.PublishEntry(entryId, version);

        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError(string.Empty, "An error occurred while creating entity: " + ex.Message);
        return View("Create", model);
      }
    }
  }
}

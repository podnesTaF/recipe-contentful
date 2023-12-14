using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Core.Search;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Net.Mime;
using WebApplication1.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ContentType = Contentful.Core.Models.ContentType;
using File = Contentful.Core.Models.File;

namespace WebApplication1.Controllers
{

  public class RecipeController(IContentfulClient client, IContentfulManagementClient managementClient) : Controller
  {

    public async Task<IActionResult> Index()
    {
      var queryBuilder = new QueryBuilder<Recipe>().ContentTypeIs("recipe");
      var entries = await client.GetEntries(queryBuilder);

      return View(entries);
    }
    public IActionResult Create()
    {
      return View();
    }

    public async Task<IActionResult> Details(string id)
    {
      var queryBuilder = new QueryBuilder<Recipe>().ContentTypeIs("recipe").FieldEquals("sys.id", id);
      var recipe = await client.GetEntry<Recipe>(id);

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


      // Handle image uploading
      try
      {
        if (model.Image != null)
        {
          byte[] imageBytes;
          using (var ms = new MemoryStream())
          {
            await model.Image.CopyToAsync(ms);
            imageBytes = ms.ToArray();
          }

          var assetId = Guid.NewGuid().ToString();

          var managementAsset = new ManagementAsset();

          managementAsset.SystemProperties = new SystemProperties();
          managementAsset.SystemProperties.Id = assetId;
          managementAsset.SystemProperties.Version = 1;

          var fileName = Path.GetFileName(model.Image.FileName);

          var contentType = model.Image.ContentType;

          managementAsset.Title = new Dictionary<string, string> {
                { "en-US", fileName}
            };



          managementAsset.Files = new Dictionary<string, File>
            {
                { "en-US", new File() {
                        ContentType = contentType,
                        FileName = fileName
                    }
                }
            };

          var newImage = await managementClient.UploadFileAndCreateAsset(managementAsset, imageBytes);

          var imageId = newImage.SystemProperties.Id;

          var asset = await managementClient.GetAsset(imageId);

          var res = await managementClient.PublishAsset(imageId, 2);
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


          System.Diagnostics.Debug.WriteLine(res);

        }
        else
        {
          ModelState.AddModelError(string.Empty, "An error occurred while uploading image no image");
          return View("Create", model);
        }
      }
      catch (Exception ex)
      {
        ModelState.AddModelError(string.Empty, "An error occurred while uploading image" + ex);
        ModelState.AddModelError(string.Empty, "An error occurred while uploading image" + ex.Message);

        // Return to the view with the current model to display the error
        return View("Create", model);
      }
      try
      {
        entry.Fields = fields;
        var response = await managementClient.CreateOrUpdateEntry(entry, contentTypeId: "recipe");
        if (response != null && response.SystemProperties != null)
        {
          var entryId = response.SystemProperties.Id;
          var version = 1;
          var publishedEntry = await managementClient.PublishEntry(entryId, version);
        }
        else
        {

          // Handle the scenario where the entry creation didn't return a valid response

          ModelState.AddModelError(string.Empty, "An error occurred while creating entity" + '\n' + response?.SystemProperties);
          return View("Create", model);
        }

        System.Diagnostics.Debug.WriteLine(response);

        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError(string.Empty, "An error occurred while creating entity 12: " + ex.Message);


        // Return to the view with the current model to display the error
        return View("Create", model);
      }
    }
  }
}

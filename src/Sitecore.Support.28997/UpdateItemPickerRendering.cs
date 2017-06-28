using System.Collections.Specialized;

namespace Sitecore.Support.Hooks
{

  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using System;
  using Sitecore.Text;

  public class UpdateItemPickerRendering : Events.Hooks.IHook
  {
    private System.Collections.Specialized.NameValueCollection GetParameters(Layouts.RenderingDefinition renderingDefinition)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      System.Collections.Generic.Dictionary<string, string> dictionary = new System.Collections.Generic.Dictionary<string, string>();
      System.Collections.Specialized.NameValueCollection nameValueCollection = Web.WebUtil.ParseUrlParameters(renderingDefinition.Parameters ?? string.Empty);
      return nameValueCollection;
    }

    public void Initialize()
    {
      using (new SecurityModel.SecurityDisabler())
      {
        var databaseName = "core";
        var itemPath = "/sitecore/client/Commerce/Layouts/Renderings/ItemPicker";
        var assemblyName = typeof(UpdateItemPickerRendering).Assembly.GetName().Name;
        var renderingUID = "{AF91FC57-20FC-4C46-A1F9-E96B4600A19D}";
        var fieldsParameterName = "Fields";
        var fieldsParameterValue = "itemId|_displayname|name|catalogdisplayname|catalogname";

        // protects from refactoring-related mistakes
        var database = Factory.GetDatabase(databaseName);
        var item = database.GetItem(itemPath);
        var layoutField = item.Fields[Sitecore.FieldIDs.LayoutField];
        Layouts.LayoutDefinition layout = Layouts.LayoutDefinition.Parse(layoutField.Value);

        foreach (Layouts.DeviceDefinition dev in layout.Devices)
        {
          var renderings = dev?.Renderings;
          if (renderings != null)
          {
            foreach (Layouts.RenderingDefinition rendering in renderings)
            {
              if (rendering.UniqueId.Equals(renderingUID, StringComparison.OrdinalIgnoreCase))
              {
                var parameters = GetParameters(rendering);
                parameters[fieldsParameterName] = fieldsParameterValue;
                UrlString urlString = new UrlString(parameters);
                rendering.Parameters = urlString.ToString();
                break;
              }
            }
          }
        }

        Diagnostics.Log.Info($"Installing {assemblyName}", this);
        item.Editing.BeginEdit();
        var xml = layout.ToXml();
        layoutField.Value = xml;
        item.Editing.EndEdit();
      }
    }
  }
}
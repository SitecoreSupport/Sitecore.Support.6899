// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisableHiddenPlaceholders.cs" company="Sitecore Corporation A/S">
//     © 2016 Sitecore Corporation A/S. All rights reserved.
// </copyright>
// <summary>
//     Defines the DisableHiddenPlaceholders type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetChromeData;

namespace Sitecore.Support.XA.Foundation.Mvc.Pipelines.GetChromeData
{
  /// <summary>
  /// Procesor used to disable editing of placeholders being added to Layout item as hidden placeholders
  /// </summary>
  public class DisableHiddenPlaceholders : GetPlaceholderChromeData
  {
    private static Dictionary<string, List<string>> _layoutsWithHiddenPlaceholders;
    protected static Dictionary<string, List<string>> LayoutsWithHiddenPlaceholders => _layoutsWithHiddenPlaceholders ?? GetLayoutsWithHiddenPlaceholders();

    public override void Process(GetChromeDataArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.IsNotNull(args.ChromeData, "Chrome Data");

      if (args.Item != null && args.Item.Paths.FullPath.Contains("/sitecore/system/Marketing Control Panel/FXM"))
      {
        return;
      }

      if (!"placeholder".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase))
      {
        return;
      }

      var layoutItem = args.Item.Visualization.GetLayout(Context.Device);

      var currentLayoutConfiguration = LayoutsWithHiddenPlaceholders.FirstOrDefault(c => c.Key == layoutItem.ID.ToString());
      if (!currentLayoutConfiguration.Equals(default(KeyValuePair<string, List<string>>)))
      {
        string placeholderKey = args.CustomData["placeHolderKey"] as string;
        if (currentLayoutConfiguration.Value.Any(x => x == placeholderKey))
        {
          args.ChromeData.Custom["editable"] = "false";
        }
      }
    }

    public static Dictionary<string, List<string>> GetLayoutsWithHiddenPlaceholders()
    {
      if (_layoutsWithHiddenPlaceholders == null || _layoutsWithHiddenPlaceholders.Count == 0)
      {
        _layoutsWithHiddenPlaceholders = new Dictionary<string, List<string>>();
      }
      else
      {
        return _layoutsWithHiddenPlaceholders;
      }

      XmlNodeList layoutsWithHiddenPlaceholders = Factory.GetConfigNodes("//hiddenPlaceholders/*");

      if (layoutsWithHiddenPlaceholders == null)
      {
        return _layoutsWithHiddenPlaceholders;
      }

      foreach (XmlNode layout in layoutsWithHiddenPlaceholders)
      {
        var id = layout.Attributes?["id"];
        if (id != null)
        {
          List<string> hiddenPlaceholders = new List<string>();
          foreach (XmlNode placeholderNode in layout.ChildNodes)
          {
            if (placeholderNode.Attributes != null)
            {
              var xmlAttribute = placeholderNode.Attributes["name"];
              hiddenPlaceholders.Add(xmlAttribute.Value);
            }
          }
          _layoutsWithHiddenPlaceholders.Add(id.Value, hiddenPlaceholders);
        }
      }

      return _layoutsWithHiddenPlaceholders;
    }
  }
}
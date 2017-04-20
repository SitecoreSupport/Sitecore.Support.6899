// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisableEditingOnInjectedRenderings.cs" company="Sitecore Corporation A/S">
//     © 2016 Sitecore Corporation A/S. All rights reserved.
// </copyright>
// <summary>
//     Defines the DisableEditingOnInjectedRenderings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.GetChromeData;
using Sitecore.XA.Foundation.Presentation.Extensions;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Support.XA.Foundation.Editing.Pipelines.GetChromeData
{
  public class DisableEditingOnInjectedRenderings : GetChromeDataProcessor
  {
    public override void Process(GetChromeDataArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (args.Item != null && args.Item.Paths.FullPath.Contains("/sitecore/system/Marketing Control Panel/FXM"))
      {
        return;
      }
      if ("rendering".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase) && args.ChromeData.Custom.ContainsKey("editable"))
      {
        string referenceId = args.CommandContext.Parameters["referenceId"];
        if (string.IsNullOrEmpty(referenceId) || Context.Item == null || (!Context.Item.IsInCurrentItemRenderings(referenceId, Context.Device) && !Context.Device.IsInDesignerRenderings(referenceId)))
        {
          RenderingReference renderingReference = (RenderingReference)args.CustomData["renderingReference"];
          string originalDatasource = renderingReference.GetOriginalDatasource() ?? string.Empty;

          WebEditButton dummyCommand = args.ChromeData.Commands.FirstOrDefault(c => c.Click == "chrome:dummy");
          if (originalDatasource.StartsWith("field:") && dummyCommand != null)
          {
            int dummyCommandIndex = args.ChromeData.Commands.IndexOf(dummyCommand);
            args.ChromeData.Commands.RemoveRange(dummyCommandIndex, args.ChromeData.Commands.Count - dummyCommandIndex);
          }
          else
          {
            args.ChromeData.Custom["editable"] = false.ToString().ToLowerInvariant();
          }
        }

        Dictionary<string, string> sources = HttpContext.Current.Items["SXA-RENDERING-SOURCES"] as Dictionary<string, string>;
        if (sources != null && sources.ContainsKey(referenceId))
        {
          // add info about components from partial designs - will be used to highlight components in page editor
          args.ChromeData.Custom["sxaSource"] = sources[referenceId];
        }
      }
    }
  }
}
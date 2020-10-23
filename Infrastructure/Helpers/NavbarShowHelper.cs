﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Venjix.Infrastructure.Helpers
{
    [HtmlTargetElement("div", Attributes = "show-when")]
    public class NavbarShowHelper : TagHelper
    {
        public string ShowWhen { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ShowWhen == null)
                return;

            var targetController = ShowWhen.Split("/")[1];
            var targetAction = ShowWhen.Split("/")[2];
            var actions = new List<string>();
            if (targetAction.Contains("|"))
            {
                actions.AddRange(targetAction.Split('|'));
            }
            else
            {
                actions.Add(targetAction);
            }

            var currentController = ViewContextData.RouteData.Values["controller"].ToString();
            var currentAction = ViewContextData.RouteData.Values["action"].ToString();

            if (!currentController.Equals(targetController)) return;
            if (!actions.Any(x => x.Equals(currentAction)))
            {
                Debug.Print("Navbar SHOW RUN");
                output.Attributes.SetAttribute("class", "collapse show");
            }
            else
            {
                output.Attributes.SetAttribute("class", "collapsed");
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garfielder.Extension
{
    /// <summary>
    /// ajax aware redirect result
    /// </summary>
    public class RedirectResultX:RedirectResult
    {
        public RedirectResultX(string url)
        : base(url)
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                var destinationUrl = UrlHelper.GenerateContentUrl(Url, context.HttpContext);

                var result = new JavaScriptResult()
                {
                    Script = "window.location='" + destinationUrl + "';"
                };
                result.ExecuteResult(context);
            }
            else
                base.ExecuteResult(context);
        }

    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Blog.Services.Identity.API.Pages.Account.Manage;

public static class ManageNavPages
{
    public static string Index => "Index";
    public static string Email => "Email";
    public static string ChangePassword => "ChangePassword";
    public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
    public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);
    public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}

using Microsoft.AspNetCore.Components;
using SpotifyAnalysis.Data.Common;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SpotifyAnalysis.Components {
    public static class LimitedPageManager {
        private const string pageNamespace = "SpotifyAnalysis.Pages";
        private static readonly string[] limitedPages;

        public static bool IsCurrentPageLimited(NavigationManager navigation) => 
            limitedPages.Contains(navigation.ToBaseRelativePath(navigation.Uri));

        static LimitedPageManager() {
            // 1. Get all page types - must have RouteAttribute
            var pages = Assembly.GetExecutingAssembly().GetTypes().Where(t => 
                t.IsClass &&
                t.Namespace == pageNamespace &&
                t.GetCustomAttribute(typeof(RouteAttribute), false) is not null
                );
            // 2. Select the pages that use injected ScopedData
            var pagesUsingScopedData = pages.Where(
                page => page.GetRuntimeProperties().Any(property => 
                    property.PropertyType == typeof(ScopedData) &&
                    property.GetCustomAttribute(typeof(InjectAttribute), false) is not null)
                );
            // 3. Save the route attributes - effective URIs - of these pages
            limitedPages = pagesUsingScopedData.Select(p => 
            (p.GetCustomAttribute(typeof(RouteAttribute), false) as RouteAttribute).Template)
                .Select(route => route.Remove(0, 1)) // remove the leading '/'
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (limitedPages.Length == 0)
                throw new DataException($"Page reflection error - {nameof(limitedPages)} is empty");
        }
    }
}

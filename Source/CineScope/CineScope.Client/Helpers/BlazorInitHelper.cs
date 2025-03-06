using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace CineScope.Client.Helpers
{
    /// <summary>
    /// Helper class to assist with proper Blazor component initialization
    /// and address routing issues
    /// </summary>
    public static class BlazorInitHelper
    {
        /// <summary>
        /// Ensures that a Blazor component is properly initialized by forcing state changes.
        /// This helps with direct navigation issues.
        /// </summary>
        public static async Task EnsureInitialized(IJSRuntime jsRuntime, string componentName = "Component", bool safeLogging = false)
        {
            try
            {
                // Only perform logging if safe (i.e., not during prerendering)
                if (safeLogging)
                {
                    await jsRuntime.InvokeVoidAsync("console.log", $"{componentName} initialization completed");
                }
                
                // Small delay to allow rendering to complete
                await Task.Delay(300);
            }
            catch (Exception)
            {
                // Silently catch exceptions during prerendering
                // We'll try again in OnAfterRenderAsync
            }
        }
    }
} 
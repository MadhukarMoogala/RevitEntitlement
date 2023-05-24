using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;

using System.Diagnostics;

using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace RevitEntitlement
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {

        
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            bool isEntitled = false;
            const string appId = @"2024453975166401172";
            UIApplication revitUI = commandData.Application;
            Application revitApplication = revitUI.Application;
            string userId = revitApplication.LoginUserId;

            Task.Run(async () =>
            {
                try
                {
                                   
                    isEntitled = await IsEntitled(userId, appId);
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);

                }

            });

            TaskDialog.Show("Entitlement", $" Is {appId} Entitled to {userId} : {isEntitled}");

            return Result.Succeeded;
        }
        public static async Task<bool> IsEntitled(string userId, string appId)
        {

            var url = string.Format("https://apps.autodesk.com/webservices/checkentitlement?userid={0}&appid={1}",
                            Uri.EscapeUriString(userId),
                            Uri.EscapeUriString(appId));

            using (var httpResponse = await new HttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    try
                    {

                        EntitlementResponse entitlementResponse = JsonConvert.DeserializeObject<EntitlementResponse>(content);
                        return entitlementResponse.IsValid;
                    }
                    catch (JsonException ex) // Invalid JSON
                    {
                        throw ex;
                    }
                }

            }
            return false;
        }
    }

    public class EntitlementResponse
    {
        public string UserId { get; set; }
        public string AppId { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    public class App : IExternalApplication
    {
        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

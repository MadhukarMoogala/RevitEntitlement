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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the application and document objects
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            bool isEntitled = false;
            const string appId = @"1234567890987654321";
            string userId = app.LoginUserId;

            // Create a ManualResetEventSlim to wait for the async task to complete
            var resetEvent = new ManualResetEventSlim(false);

            // Run the async task to check entitlement
            Task.Run(async () =>
            {
                try
                {
                    // ConfigureAwait(false) prevents capturing the current synchronization context, 
                    // which avoids potential UI thread blocking and responsiveness issues
                    isEntitled = await IsEntitled(userId, appId).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // Log any exception that occurs during the async operation
                    Debug.Print($"Exception: {e.Message}");
                }
                finally
                {
                    // Signal that the async task is complete
                    resetEvent.Set();
                }
            });

            // Block the main thread until the async task is completed
            resetEvent.Wait();

            // Display the entitlement status
            if (isEntitled)
            {

                //Write your business logic here

                TaskDialog.Show("Entitlement Status", $"User ID '{userId}' is entitled to use the application with ID '{appId}'");
            }
            else
            {
                TaskDialog.Show("Entitlement Status", $"User ID '{userId}' is not entitled to use the application with ID '{appId}'"
                    + ". Please check the entitlement details or contact support if you believe this is an error.");
            }

            return Result.Succeeded;
        }

        public static async Task<bool> IsEntitled(string userId, string appId)
        {
            // Construct the URL for the entitlement check API
            var url = $"https://apps.autodesk.com/webservices/checkentitlement?userid={Uri.EscapeDataString(userId)}&appid={Uri.EscapeDataString(appId)}";

            using (var httpClient = new HttpClient())
            {
                // Send the GET request and read the response
                var httpResponse = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                httpResponse.EnsureSuccessStatusCode(); // Throws if the status code is not 200-299

                if (httpResponse.Content != null && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    try
                    {
                        // Deserialize the JSON response
                        var entitlementResponse = JsonConvert.DeserializeObject<EntitlementResponse>(content);
                        return entitlementResponse.IsValid;
                    }
                    catch (JsonException ex)
                    {
                        throw new Exception("Failed to deserialize the JSON response.", ex);
                    }
                }
            }
            return false;
        }

        public class EntitlementResponse
        {
            public string UserId { get; set; }
            public string AppId { get; set; }
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }
    }
}
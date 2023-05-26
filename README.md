# RevitEntitlement
A revit test application to run Entitlement API from the Revit Addin code.
For this test to succeed or fail, the test assumes following [App](https://apps.autodesk.com/RVT/en/Detail/Index?id=1257010617153832891&appLang=en&os=Win64&autostart=true) has installed on Revit application and it will cross verify if the logged in user has access to installed app.

## Steps to Build and Test

Project is designed to configure the paths to  %ProgramData%\Autodesk\Revit\Addins\2024 for debug builds and ./RevitEntitlement.bundle for release builds.

### Preqs
 
- Revit 2024
- Visual Studio 2022
- .NET 4.8

```
git clone https://git.autodesk.com/moogalm/RevitEntitlement.git
cd RevitEntitlement\RevitEntitlement
devenv RevitEntitlement.sln
add reference RevitAPI.dll and RevitAPIUI.dll
Nuget install Newtonsoft.Json --version 13.0.3
```

### Test

- Go to Revit Ribbon Bar\Addins\External Tool\
- Click on Commands RevitEntitlement




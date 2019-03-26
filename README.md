Online Class Schedule
=====================

## Overview

The online Class Schedule application was developed to replace a previous manual process of creating a static HTML schedule from print resources. This application dynamically displays course catalog and class schedule information and serves as the source for such data.

## Requirements

+ .NET Framework 4.7
+ ASP.NET MVC 4
+ Visual Studio 2017
+ [CtcApi and CtcOdsApi](https://github.com/BellevueCollege/CtcApi) - included via [the BC Azure Devops artifacts feed](https://pkgs.dev.azure.com/bcintegration/_packaging/BCFeed/nuget/v3/index.json).

### Optional

The following are not required, strictly speaking, but may improve your development experience with this project. Some features and/or functionality may also be unavailable without the following.

+ [Red-Gate's SQL Developer Bundle](https://www.red-gate.com/products/sql-development/sql-developer-bundle/) - Visual Studio will complain that it cannot open the **ClassSchedule.DB** project without this installed, but the files can - **and should** - still be modified and managed via source control. The included **.sdc* data migration project files also require these tools. Database installation, setup, deployment and synchronization will need to be managed manually.
+ [ReSharper](https://www.jetbrains.com/resharper/) - Some of the source code contains *ReSharper* comments that temporarily disable specific code suggestions. Please do not remove these.

## Setting up the ClassSchedule.Web project for development

#### Clone project

Clone [the Class Schedule project](https://github.com/BellevueCollege/ClassSchedule) to a local repo. Ensure you are using the dev branch for development (or create a new one for your updates, depending on how extensive they are).

#### Use NuGet package manager to download additional project libraries 

Additional libraries are needed for the project, including CtcApi and CtcOdsApi. You can see which ones are required by view the _packages.config_ file. 

> **Note:** Before downloading packages, you will want to ensure you have BC's Azure DevOps artifacts feed added as a package source. You can do so under Tools > NuGet Package Manager > Package Manager Settings. The name for the package source can be whatever is recognizable to you. The source URL is: **https://pkgs.dev.azure.com/bcintegration/_packaging/BCFeed/nuget/v3/index.json**

To download the libraries required, go to Project > Manage NuGet Packages. This will open the NuGet package manager GUI and will likely also prompt you to restore packages.

#### Create config files 
Create the necessary *.config* files for your environment in the _configSource folder. These files will be automatically included via the *web.config*. Example files are provided for a reference.

#### Update globals location
Update `Globals_UrlRoot` and `Globals_LocalPath` in `appSettings.config`. `Globals_UrlRoot` should be a URL to a version of globals, i.e. on shoes or s. `Globals_LocalPath` should be a file path to your local copy of globals.

### Generate DB designer file
Right click on `Models\ClassScheduleDb.edmx` and select "Run Custom Tool" to generate file.

#### Build project

Now build the ClassSchedule.Web project. This should theoretically successfully build error free, but if not work through each error (usually a missing reference/package). Once built, you should be able to run it (with or without debugging) from Visual Studio.  It is recommended, however, to set up your own local IIS server and set up the project application there.

## Testing with the Test.ClassSchedule.Web project

The Test project currently contains some unit tests for the API portion of the Class Schedule. To use the project:

 - Create `AppSettings.config` and `ConnectionStrings.config` files using the included example files.
 - Build project, if not already built. You may also need to build the main ClassSchedule project as the Test project depends on it.
 - To run the existing tests, go to Test > Windows > Test Explorer. Then you can select to run all tests (or a portion).

Note: Visual Studio uses the account Visual Studio was opened under (likely the account you used to log into your computer) to connect to the database. If you need to use a different account that has database access, you will need to open Visual Studio under that user before running the tests.

## Setting up the ClassSchedule.DB project

TODO
## See also

+ [CtcApi.Ods](https://github.com/BellevueCollege/CtcApi/wiki#what-is-it)
+ [Class Schedule web api reference](https://github.com/BellevueCollege/ClassSchedule/wiki/Class-schedule-web-api-reference)

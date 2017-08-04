Online Class Schedule
=====================
(for [WA SBCTC](http://www.sbctc.ctc.edu/) colleges)

## Overview

The online Class Schedule application was developed to replace a previous manual process of creating a static HTML schedule from print resources. This applicaton dynamically displays course catalog and class schedule information and serves as the source for such data.

## Requirements

+ .NET Framework 4.6.2 (the full platform, not the client)
+ ASP.NET MVC4
+ Visual Studio 2015 (later versions will probably work)
+ NuGet Package Manager (v3.3.x, v3.4.x is hopelessly broken) - for various other 3rd-party libraries
+ [CtcApi.Ods](https://github.com/BellevueCollege/CtcApi) - included via [Bellevue College's NuGet package server](http://www.bellevuecollege.edu/dev/nuget).
+ [ods-legacy](https://github.com/ctcdev/ods-legacy) (private repo) - this application was designed to run off the CTCODS, while supporting the possibility that this data source will change in the future.

### Optional

The following are not required, strictly speaking, but may improve your development experience with this project. Some features and/or functionality may also be unavailable without the following.

+ [Red-Gate's SQL Developer Bundle](https://www.red-gate.com/products/sql-development/sql-developer-bundle/) - Visual Studio will complain that it cannot open the **ClassSchedule.DB** project without this installed, but the files can - **and should** - still be modified and managed via source control. The included **.sdc* data migration project files also require these tools. Database installation, setup, deployment and synchronization will need to be managed manually.
+ [ReSharper](https://www.jetbrains.com/resharper/) - Some of the source code contains *ReSharper* comments that temporarily disable specific code suggestions. Please do not remove these.

## Setting up the ClassSchedule.Web project for development

#### Clone project

Clone [the Class Schedule project](https://github.com/BellevueCollege/ClassSchedule) to a local repo. Ensure you are using the dev branch for development (or create a new one for your updates, depending on how extensive they are).

#### Use NuGet package manager to download additional project libraries 

Additional libraries are needed for the project, including CtcApi and CtcOdsApi. You can see which ones are required by view the _packages.config_ file. 

> **Note:** Before downloading packages, you will want to ensure you have Bellevue College's NuGet server added as a package source. You can do so under Tools > Options > NuGet Package Manager > Package Sources. The name for the package source can be whatever is recognizable to you. The source URL is: **http://www.bellevuecollege.edu/dev/nuget**

To download the libraries required, go to Project > Manage NuGet Packages. This will open the NuGet package manager GUI and will likely also prompt you to restore packages.

#### Create config files 
Create the necessary *.config* files for your environment in the _configSource folder. These files will be automatically included via the *web.config*. Example files are provided for a reference.

#### Update globals location
Update `Globals_UrlRoot` and `Globals_LocalPath` in `appSettings.config`. `Globals_UrlRoot` should be a URL to a version of globals, i.e. on shoes or s. `Globals_LocalPath` should be a file path to your local copy of globals.

#### Build project

Now build the ClassSchedule.Web project. This should theoretically successfully build error free, but if not work through each error (usually a missing reference/package). Once built, you should be able to run it (with or without debugging) from Visual Studio.  It is recommended, however, to set up your own local IIS server and set up the project application there.

### Setting up the ClassSchedule.DB project

TODO

### Setting up the Test.ClassSchedule project

TODO

## See also

+ [CtcApi.Ods](https://github.com/BellevueCollege/CtcApi/wiki#what-is-it)
+ [Class Schedule web api reference](https://github.com/BellevueCollege/ClassSchedule/wiki/Class-schedule-web-api-reference)

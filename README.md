Online Class Schedule
=====================
(for [WA SBCTC](http://www.sbctc.ctc.edu/) colleges)

## Overview

The online Class Schedule application was developed to replace a previous manual process of creating a static HTML schedule from print resources. This applicaton dynamically displays course catalog and class schedule information, and serves as the master source for such data.

## Requirements

+ .NET Framework 4 (the **Full** platform, not the **Client**)
+ [ASP.NET MVC3](http://www.microsoft.com/en-us/download/details.aspx?displaylang=en&id=1491&WT.mc_id=aff-n-in-loc--hr) (you may also need to see [this article](http://geekswithblogs.net/ranganh/archive/2011/10/26/installing-mvc-3-for-visual-studio-2010-on-windows-developer.aspx))
+ Visual Studio 2012 (later versions will probably work)
+ NuGet Package Manager (included in Visual Studio) - for various other 3rd-party libraries
+ [CtcApi.Ods](https://github.com/BellevueCollege/CtcApi) - included via [Bellevue College's NuGet package server](http://www.bellevuecollege.edu/dev/).
+ [ods-legacy](https://github.com/ctcdev/ods-legacy) (private repo) - this application was designed to run off the CTCODS, while supporting the possibility that this data source will change in the future.

### Optional

The following are not required, stricly speaking, but will greatly improve your development experience with this project. Some features and/or functionality may also be unavailable without the following.

+ [Red-Gate's SQL Developer Bundle](https://www.red-gate.com/products/sql-development/sql-developer-bundle/) - Visual Studio will complain that it cannot open the **ClassSchedule.DB** project without this installed, but the files can - **and should** - still be modified and managed via source control. The included **.sdc* data migration project files also require these tools. Database installation, setup, deployment and synchronization will need to be managed manually.
+ [ReSharper](https://www.jetbrains.com/resharper/) - Some of the source code contains *ReSharper* comments that temporarily disable specific code suggestions. Please do not remove these.
+ [Git Source Control Provider](http://visualstudiogallery.msdn.microsoft.com/63a7e40d-4d71-4fbb-a23b-d262124b8f4c) - This *solution* uses this 3rd-party provider instead of the Git support build into Visual Studio 2012. (This is primarily a legacy decision.)

## Getting started

### Setting up the ClassSchedule.Web project

1.  If you are using CAS for Single Sign-On (our current configuration), you will need to set up IIS on your local machine, so that the app is hosted at http://localhost/classes. **CAS login WILL NOT work with Visual Studio's built-in web server.**
1.  If you did the previous step, you need to set up another IIS Virtual Folder at http://localhost/globals which points to the *globals* folder.
1.  Create the necessary *.config* files for your organization/environment in the *_ConfigSource* folder. These files will be automatically included via the *web.config*. EXAMPLE files are provided for a reference.

### Setting up the ClassSchedule.DB project

TODO
## See also

+ [CtcApi.Ods](https://github.com/BellevueCollege/CtcApi/wiki#what-is-it)
+ [Class Schedule web api reference](https://github.com/BellevueCollege/ClassSchedule/wiki/Class-schedule-web-api-reference)
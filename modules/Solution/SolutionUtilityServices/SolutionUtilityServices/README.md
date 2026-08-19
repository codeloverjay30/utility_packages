# Description
Utility Service to extract one or more projects in a solution to other solution (.slnx)

# Requirements
## IDE  
+ VS IDE: VS 2026+ to work when opening new solution as it generates .slnx file (only supported in VS 2026+)

# P.S.
+ It directly manipulates the xml tag of new solution (.slnx), so it is not neccessary to have VS 2026 on targeting machine when the API is used.

# Notes
+ It is still in early stage, so expect some bugs and missing features.

# Features
## 1.0.0-alpha-1.0.0
### Added
+ Extract one or more projects into other solutions

## 1.1.0-alpha-1.0.0
### Fixed
Only can auto upgrade the .NET version of project to latest version if needed

=> 

NOT ONLY can auto upgrade the .NET version of project to latest version if needed,

BUT ALSO auto upgrade the package reference of project to latest version if needed

## 2.0.0-alpha-1.0.0
### Major updates
+ Rename project name

+ Rename namespace

+ Make a documentation.

## 3.0.0-preview-1.0.0
### Major updates
+ Use interface (for easily mocking) and DI.

+ Use POCO instead of many arguments in `SolutionExtractor` class

+ Use `System.IO.Abstraction.IFileSystem` instead of `System.IO.` for mocking and unit testing.

### Added test
+ Added unit test

## 4.0.0-preview-1.0.0
### Major updates
+ Use MSBuild (`Microsoft.Build` packages) to upgrade project.

`ProjectUpgrader` => `CSharpProjectUpgrader`

## 5.0.0-preview-1.0.0
### Major updates
+ For easily to perform unit tests with mock, 

extract the core logic of upgrading project (using `Microsoft.Build` packages) to a utility class.


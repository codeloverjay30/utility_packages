# Description
Utility method to configure settings about Serilog.

# Features
## 1.0.0-preview-1.0.0
### Added
+ wrapper to easier get the format of log file name.

Format:

- Directory:

The file will be placed at `{BaseDirectory}/Logs`

where

`{BaseDirectory}` is the input of utility method

- File name:

Case 1: When running app exists,
              
Then the file name will `{ImportingProjectName}_{Version}_{yyyyMMDDhhmm}.log`

where

`{ImportingProjectName}` indicates the name of project that uses this package.

`{Version}` indicates the version number of the `{ImportingProjectName}`.

`{yyyyMMDDhhmm}` indicates the datetime format in CSharp

For example:

When you develop a project named `OmniAppium.EngineUtilityService` (with version `1.0.0-preview-1.0.0`) that uses this package,

and you run the project (or its executable) at `2026/03/03 14:58`  

Then the log file name will be look like this

`OmniAppium.EngineUtilityService_1.0.0-preview-1.0.0_202603031458.log`

Case 2: Otherwise

Then the file name will `UnknownAssembly_{yyyyMMDDhhmm}.log`

## 1.1.0-preview-1.0.0
### Minor changes
+ Update the format.

Format:

- Directory:

The file will be placed at `{BaseDirectory}/Logs`

where

`{BaseDirectory}` is the input of utility method

- File name:

Case 1: When running app exists,
              
Then the file name will `{clientDeviceName}_{ImportingProjectName}_{Version}_{yyyyMMDDhhmm}.log`

where

`{clientDeviceName}` indicates the device name that running the app.

`{ImportingProjectName}` indicates the name of project that uses this package.

`{Version}` indicates the version number of the `{ImportingProjectName}`.

`{yyyyMMDDhhmm}` indicates the datetime format in CSharp

For example:

When you develop a project named `OmniAppium.EngineUtilityService` (with version `1.0.0-preview-1.0.0`) that uses this package,

and you run the project (or its executable) at `2026/03/03 14:58`  

Then the log file name will be look like this

`MyClientDeviceName_OmniAppium.EngineUtilityService_1.0.0-preview-1.0.0_202603031458.log`

Case 2: Otherwise

Then the file name will `{clientDeviceName}_UnknownAssembly_{yyyyMMDDhhmm}.log`

## 1.1.1-preview-1.0.0
### Added constraints
+ constraint the MSBuild version.

## 2.0.0
### Major updates
+ Use CPM to manage packages.

+ Update some packages.

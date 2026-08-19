# Description
Wrapper of Factory pattern and instance (between ILoggerFactory and ILogger)

# Features
## 1.0.0
### Added
+ Wrapper of Factory pattern and instance (between ILoggerFactory and ILogger)

## 1.1.0
### Changed
+ Make LoggerFactoryBaseUtilityService.Logger property public, let one can more easily define a proxy property (ILogger type) in other utility class

## 1.2.0
### Changed
+ Make all property, method, constructor public, let one can instantiate it in other place, making one can easily pass this class as primary constructor in other utility class, avoiding boilerplate code.

## 1.3.0
### Changed
+ mark LoggerFactoryBaseUtilityService.Logger property with virtual modifier so that the Moq (Unit Test) can find the property.

+ use interfaces.

## 1.4.0
### Changed
+ Define ILoggerFactory LoggerFactory { get; init; } in ILoggerFactoryUtilityService, let one can easily pass ILoggerFactory instance when instantiating the utility class, avoiding boilerplate code.

## 2.0.0-preview-1.0.0
### Fixed
+ Solve the issue that order to initialize the `init`-property.

### Changed
+ Rename the project name from `LoggerFactoryUtilityServices` to `LoggerFactoryUtilityServices` (but not changed the folder name and namespace)

## 3.0.0-preview-1.0.0
### Fixed
+ Adjust the namespace from `LoggerFactoryUtilityServices` to `LoggerFactoryUtilityServices` (but not changed the folder name and namespace)

to solve the issue that ambiguous namespace when using this utility class in other project.

## 3.1.0-preview-1.0.0
### Fixed
+ Correct the typo in project file (`*.csproj`)

## 4.0.0-preview-1.0.0
### Fixed
+ Correct the property modifier

## 5.0.0-preview-1.0.0
### Major updates
+ Update the package

### Supports on
.NET 8.0 to .NET 10.0

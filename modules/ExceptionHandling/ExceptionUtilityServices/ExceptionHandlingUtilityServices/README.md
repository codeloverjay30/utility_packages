# Description
A simple (and easily used) wrapper and logging when failure.

# Features
## 2.0.0-preview-1.0.0
### Fixed
+ Use `Lazy&gt;T&lt;` instead of `??=` that uses lazy loading technique since

NOT ONLY `Lazy&gt;T&lt;` uses lazy loading technique BUT ALSO it ensures execution safety on the multithread process.

## Changed
+ For `SafeExecute&gt;T&lt;` method, it is much easier to mark the operation in the log.

## 2.1.0-preview-1.0.0
### Updated
+ Just update the `LoggerFactoryUtilityServices` NuGet package from 1.1.0 to 1.2.0

## 2.2.0-preview-1.0.0
### Changed
+ Use interface `ILoggerFactoryUtilityService` instead of concrete class `LoggerFactoryUtilityServices` to make it more flexible and testable.

## 2.3.0-preview-1.0.0
### Added
+ Utility method to flatten the exception and inner exceptions and process delegates.

## 2.3.1-preview-1.0.0
### Updated
+ Update the `LoggerFactoryUtilityServices` NuGet package to `LoggerFactoryUtilityServices` 2.0.0

## 2.4.0-preview-1.0.0
### Updated
+ Update the `LoggerFactoryUtilityServices` NuGet package to `LoggerFactoryUtilityServices` 3.0.0

## 3.0.0-preview-1.0.0
### Major updates
+ Use CPM to manage packages.

+ Update the `LoggerFactoryUtilityServices` NuGet package from `3.0.0` to `4.0.0`
### Updated
+ Add missing `<PackageReadmeFile>` tag.



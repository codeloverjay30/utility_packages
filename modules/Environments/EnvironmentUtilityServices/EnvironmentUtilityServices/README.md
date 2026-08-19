# Description
Utility service to check OS Environment and perform some actions about OS.

# Features
## 1.0.0
### Added
+ Check OS Environment

+ Get comparison of path (`StringComparison`)

## 2.0.0
### Optimizations
+ Ensure the info of OS is immutability by checking OS Environment in constructor.

+ Improve the performance  by checking OS Environment in constructor.

### Major Updates
+ Use facade pattern to wrap `IOsUtilityService` and `EnvironmentService`

## 3.0.0
### Added
+ Check Path is Unc format.
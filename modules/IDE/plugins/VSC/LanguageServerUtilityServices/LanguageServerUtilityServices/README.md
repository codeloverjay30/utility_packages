# Description
Utility package that interacts with Language Server.

# Features
## 1.0.0-preview-1.0.0
### Added
+ can interact with VSC Language Server 

(used for developing utility packages or plugins about `VSC` which will be placed in `VSC extensions marketplace`).

## 2.0.0-preview-1.0.0
### Major changes
Refactor to resolve 

+ unconsisent behaviour

due to may use different `IEnvironmentService` instances when initialization of `EnvironmentService` property of `CommandLineInput` instance.

## 2.1.0-preview-1.0.0
### Added
API to

+ install a VSC plugin or extension by its extension id.
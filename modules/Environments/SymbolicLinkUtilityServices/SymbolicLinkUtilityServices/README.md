# Description
Utility service to update symbolic link with options.

# Features
## 1.0.0-preview-1.0.0
### Added
+ update symbolic link with options

### Implementation
Use `Builder pattern`, `Strategy option`, `Static factory method`

(reference the design pattern used in `Polly v8`)

## 2.0.0-preview-1.0.0
### Added
+ Given a symbolic link, check there is circular occurred when reparsing, or not.

### Major Updates
+ Interfacify

- Extact the behavior of get ACL and set ACL to an interface to easily mock for unit tests.

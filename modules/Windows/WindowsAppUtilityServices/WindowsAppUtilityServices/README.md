# Description
Service to move a directory to other place and create a junction

that directs the source directory to destination directory.

# Note
+ It can't be a class library since it executes commands in this app (thus it must be a console app)

+ It is only supported on `Windows 10/11` or above.

# Prequisite
+ These one of following terminal can work:

    - `DOS`

# Features
## 1.0.0
### Added
+ Move files and create a junction from source to target.

## 2.0.0-preview-1.0.0
### Major updates
+ Rename project name

+ Rename namespace

+ Use interface and DI.

### Added tests
+ Added Unit Test

## 3.0.0-preview-1.0.0
### Major Updates
+ Decoupling these services

    - command runner `CommandRunner`
    
    - Processor `ProcessUtilityService`

    - Apps mover `WindowsAppsMover`

## 4.0.0-preview-1.0.0
### Major Updates
According to [rule of Clean Architecture](../GEMINI.md), I've

+ Apply the shorthand style for namespaces

+ Write comments for API in English.

+ Refactor (see below)

### Refactor

1. Use `System.Diagnostics.Abstractions.IProcess` and `System.Diagnostics.Abstractions.ProcessFactory` to replace

my custom wrapper `IPorcessWrapper` 

since `System.Diagnostics.Abstractions` packages **not only** abstracts the logic of process `System.Diagnostics.Process`

**but also**  mock a `Process` using factory (`System.Diagnostics.Abstractions.ProcessFactory`) 

and create an actual `Process` (`System.Diagnostics.Process`).

> [!P.S.]
> 
> About code smell (before refactor),
>
> 1. although I had used `System.Diagnostics.Process` in the project before,
>
> I didn't know there is a factory (`System.Diagnostics.Abstractions.ProcessFactory`) that can
> 
> + mock a `Process` and
>
> + create an actual `Process` (`System.Diagnostics.Process`)
>
> At that time, I just simply pasted the code that AI Agent (Gemini) provides, and
> 
> I didn't ask AI Agent for better alternatives (even though, I want to abstract `Process` (`System.Diagnostics.Process`) as simple as I can).

2. Extract the logic of safe killing process into an utility service (from `MoveOneApp` method of `WindowAppMover` class) 
# Description
Utility of custom Exceptions
# Features
## 1.0.0
### Added
+ custom Exceptions

`MismatchedDataStructureException`

## 1.1.0
### Added
+ Check its type is expected type

### Added API
+ Adds a static factory method

For those DO NOT know their actual type.

```
public static MismatchedDataStructureException<TExpected, TActual> Create(
        Type expectedType,
        Type actualType,
        string format
    )
```

## 2.0.0
### Fixed bugs
+ compiler error of `ReadOnlyMemory<char> readOnlyMemory; ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(readOnlyMemory,nameof(readOnlyMemory));`

+ 
# Interrogator
Interrogator is a collection of packages to support integration testing. Using Interrogator, complex relationships between unit tests can be defined.

<!-- TOC -->

- [Usage](#usage)
    - [Defining An Interrogator Test](#defining-an-interrogator-test)
    - [DependsOn](#dependson)
    - [NotConcurrent](#notconcurrent)
        - [Parameters](#parameters)
    - [FromAttribute](#fromattribute)
- [Packages](#packages)
    - [Interrogator.Http](#interrogatorhttp)
    - [Interrogator.xUnit](#interrogatorxunit)
- [Gotchas](#gotchas)

<!-- /TOC -->

## Usage
### Defining An Interrogator Test
- Add `Interrogator.xUnit` package to your test project
- Add the `[IntegrationTest]` attribute to the top of any test method

```C#
[IntegrationTest]
public Task Test1()
{
    // Do something
}
```

### DependsOn
The `DependsOn` attribute can be used to ensure that all dependencies are executed before the tagged method. Multiple `DependsOn` attributes can exist on a single test. Best practice is to use `nameof` to indicate the dependency.

When running explicit tests that is further in the dependency tree, all ancestor dependencies will be executed prior to the test or tests run.

Note: Failing tests earlier in the dependency tree will result in later tests not being run. When running a single test, this may result in the appearance that the test being run was skipped without other tests failing.

```C#
[IntegrationTest]
public Task Test1()
{
    // Do something
}

[IntegrationTest]
[DependsOn(nameof(Test1))]
public Task Test2()
{
    // Do something
}
```

### NotConcurrent
Tests that must not be executed at the same time can be marked with the `NotConcurrent` attribute. Multiple `NotConcurrent` attriutes can exist on a single test.
#### Parameters
- `groupName` (string) 
    - When provided, all `NotConcurrent` attributes with the same `groupName` will not be executed concurrently. It is possible for `NotConcurrent` groups with a different `groupName` to execute at the same time.
- ConcurrencyScope (enum)
    - Class - Default - All `groupName`s within the same class will be considered a single `NotConcurrent` group.
    - ClassHierarchy - All `groupName`s within the same class hierarchy will be considered a single `NotConcurrent` group.
    - Namespace - All `groupName`s within the same namespace will be considered a single `NotConcurrent` group.
    - Assembly - All `groupName`s within the same assembly will be consider a single `NotConcurrent` group.

```C#
[IntegrationTest]
[NotConcurrent]
public Task Test1()
{
    // Do something
}

[IntegrationTest]
[NotConcurrent]
public Task Test2()
{
    // Do something
}
```

### FromAttribute
The `FromAttribute` can be used to indicate a parameter dependency. This dependency can be the output of another test, or a static method.

Note: If this is the output of a static method, the static method will be run once the output will be used for all instances of that `FromAttribute`

#### Static Method
```C#
public static InitialData CreateInitialData()
    => new InitialData();

[IntegrationTest]
public Task Test([From(nameof(CreateInitialData))]InitialData initialData)
{
    // Do something
}
```

#### Method Output
```C#
[IntegrationTest]
public Task Test1()
{
    // Do something
    return "A string";
}

[IntegrationTest]
public Task Test2([From(nameof(Test1))]string test1String)
{
    // Do something
}
```


## Packages

### Interrogator.Http

### Interrogator.xUnit
xUnit specific package to allow integration with xUnit tests.

## Gotchas
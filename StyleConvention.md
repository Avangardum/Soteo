## C#

### Naming

| Object                                     | Naming               |
|--------------------------------------------|----------------------|
| Namespace                                  | `SomeNamespace`      |
| Non-interface type                         | `SomeType`           |
| Interface                                  | `ISomeInterface`     |
| Non-local constant / static readonly field | `SomeConstant`       |
| Private field                              | `_someField`         |
| Non-private field                          | `SomeField`          |
| Event                                      | `SomeEvent`          |
| Property                                   | `SomeProperty`       |
| Function that doesn't return Task          | `SomeFunction`       |
| Function that returns Task                 | `SomeFunctionAsync`  |
| Record primary constructor parameter       | `SomeParameter`      |
| Other parameter                            | `someParameter`      |
| Local variable / constant                  | `someVariable`       |
| Region                                     | `SomeRegion`         |
| Goto label                                 | `SomeGotoLabel`      |
| Tuple / anonymous type member              | `SomeMember`         |
| Generic type parameter                     | `TSomeTypeParameter` |

### Comments

Comments should be written in English.

Comments should be written using `//`, not `/* */`.

`//` should always be followed by a space.

If `//` is not in the beginning of a line, it should also be preceded by a space.

Comments documenting a type or member should be in the `<summary>` format.

Use `<inheritdoc />` where possible to make it obvious that there is documentation available.

### Namespaces

Use file-scoped namespaces.

Namespaces should correspond to the folder structure (use automatic namespace adjustment in your IDE).

### Types

Types with the same name that differ only by number of generic parameters should be in one file.

Extension classes for a single type declared in the same codebase should be in the same file with the extended type.

Otherwise, each non-nested type should be in its own file.

Classes that are not meant to be inherited from, but are meant to be instanced, should be sealed.

Classes that are not meant to be instanced, but are meant to be inherited from, should be abstract.

Classes that are meant to be neither instanced, nor inherited from, should be static.

Inside a constructor, when choosing between reading a parameter and reading a field, prefer reading a parameter.

When a type is instanced using a parameterless constructor with property initializers, the parentheses
should be omitted.

### Delegates, events

Prefer using `Action`, `Func` or custom delegate types, avoid `Predicate`, `EventArgs`.

Event argument DTOs should follow one of the following patterns: *event* + Args (`UpdatingArgs`),
*sender* + *event* + Args (`DataSourceUpdatingArgs`).

Event handler method names should follow one of the following patterns: On + *event* (`OnUpdating`),
On + *sender* + *event* (`OnDataSourceUpdating`).

Events should not be nullable, they should be initialized with an empty delegate.

Events that are invoked before something happens should be named as verbs in present continuous (`DataSourceUpdating`).

Events that are invoked after something happens should be named as verbs in past simple. (`DataSourceUpdated`).

### Functions, properties, indexers

Do not use expression body declaration if it is more than one line long. Having a signature and an expression body on
different lines is still acceptable.

Do not mutate primary constructor arguments.

In `Task` returning functions, don't directly return a `Task` returned by another function.
Instead, await it and return its result.

### Collections

Use collection expressions where possible (`ImmutableList<int> a = [0, 1, 2];`).

Use immutable collections (`ImmutableList`, `ImmutableHashSet`, etc.) where possible.

Functions should both accept and return interfaces of a specific collection type (`IReadOnlyList`, `IDictionary`),
preferably read only versions.

### Formatting

Indentation: 4 spaces.

There should be no space between a method name and its parameters.

In a `new` expression there should be no space between an object type and constructor arguments. (`new Foo()`)

In a target-typed `new` expression there should be no space between `new` and constructor arguments. (`new()`)

In a `new` expression there should be a single space between constructor parameters and field initializers, or, in
case if there are no constructor parameters, between an object type and field initializers. (`new Foo { Value = 1 }`)

There should be a single space between `do`, `while`, `if`, `using`, `fixed` and following parentheses.

Use the Allman style for any block of `{}`, `()`, `[]` or `<>`, spanning multiple lines.

```csharp
public class Foo
<
    TBar,
    TBaz,
    TQux
>
(
    TBar bar,
    TBaz baz,
    TQux qux
)
{
    private object[] _objects = 
    [
        bar,
        baz,
        qux
    ];
}
```

Having a single newline between members of a type is always acceptable. However, it is possible to omit it if
both members occupy only 1 line each (including comments, attributes, initializers).

Use trailing commas in multiline comma-separated lists, avoid them in inline lists.

In a comma-separated or semicolon-separated list, each delimiter should be followed by a space or a newline.

When doing a line break on an operator, the operator should be at the end of a line, not at the beginning.
The exceptions are the `.`, `?.`, `!.` operators, which should be at the beginning of a line.

When having an inline block of `()`, `[]` or `<>`, there should be no space between
the opening and closing symbols and the content.

When having an inline block of `{}`, there should be a space between the opening and closing braces and the content.

There should be a space or a newline between a binary/ternary operator and its operands (except `.`, `?.`, `!.`).

There should be no space between a unary operator and its operand.

`#region` directive and its content should have the same indentation level as surrounding code.

Other preprocessor directives should have zero indentation.

Goto labels should be indented one level less than surrounding code.

### Miscellaneous

When comparing a value with boundary values, the values should be sorted in the ascending order from left to right.
(`min <= value && value <= max`).

Use `var` only when the type is explicitly named on the right-hand side.

Use target-typed `new` only when the type is explicitly named on the left-hand side.

When choosing between `var` and target-typed `new`, prefer `var`. (`var obj = new object();`)

Use explicit access modifiers where possible. The exception are interface members, they should be implicitly public.

Use modifiers in the following order (modifiers on the same line are mutually exclusive):

- `private`, `private protected`, `protected`, `internal`, `protected internal`, `public`
- `new`
- `static`, `sealed`, `abstract`, `virtual`, `override`
- `extern`, `unsafe`, `async`, `const`, `readonly`, `volatile`
- `partial`

Declare type members in the following order:

- Static state (fields, auto-properties, events) and constants
- Static constructor
- Instance state
- Constructors
- Finalizer
- Methods, computed properties, indexers, operators
- Nested types

Within a category primarily prefer locality and order members conceptually, such as by placing a low level method
definition after a high level method that calls it. Secondarily, sort from `public` to `private`.

Nullable reference types should be enabled everywhere. Generated code, where they are disabled by default, should
include the `#nullable enable` directive.

When possible, use the `Required` extension property to assert that a value is not null
instead of the null forgiving operator `!`.

Avoid using special values like `0`, `-1`, `NaN`, `Guid.Empty` to encode a special meaning like "nothing", "unknown",
"invalid", "unchanged". Instead use `null` or a custom wrapper like `Delta`.

## Files

| Object | Naming     |
|--------|------------|
| File   | `SomeFile` |

## Git

| Object      | Naming           |
|-------------|------------------|
| Repository  | `SomeRepository` |
| Branch      | `some-branch`    |
| Main branch | `main`           |

## JSON

| Object        | Naming         |
|---------------|----------------|
| Property name | `someProperty` |

Indentation: 4 spaces.

## XML, HTML, XAML

Inline self-closing tag should have a space before `/>`.

A tag should either occupy one line with all its attributes, or have each attribute on a separate line,
with the tag name and the closing angle bracket on its own line.

Indentation: 4 spaces.

## Miscellaneous

Abbreviations follow the same naming rules as if they were regular words (`JsonParser`, not `JSONParser`).

When possible, format code to avoid line width exceeding 120 symbols.

Empty lines should have the same indentation level as the surrounding code.

Non-empty lines should have no trailing spaces.

All files should end with an empty line.

In special cases where fully following this convention would be impractical, it's possible to deviate from it.
Reasons for such a deviation should be documented in a comment, unless they are obvious from the context.

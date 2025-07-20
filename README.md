# Library.UnitTesting

![Build](https://github.com/Rmkrs/Library.UnitTesting/actions/workflows/ci.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/Library.UnitTesting.svg)](https://www.nuget.org/packages/Library.UnitTesting)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A lightweight, opinionated library for writing high-quality unit tests in .NET.

It wraps up common testing patterns using:
- [AutoFixture](https://github.com/AutoFixture/AutoFixture)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq](https://github.com/moq/moq4)
- [NUnit](https://nunit.org/)

## ✨ Features

- 🚀 Easy object instantiation with `Instantiator` (AutoFixture under the hood)
- ✅ Null-guard assertions for constructors and methods
- ⛔ Async `void` method detection (and test enforcement)
- 🔍 Custom `It.Is<T>` helpers using FluentAssertions exposed with `ItExt`
- 🧱 Abstract base test class to reduce boilerplate

---

## 📦 Installation

Available on NuGet:
```
dotnet add package Library.UnitTesting
```

---

## 🧪 Example

### Sample code:
```csharp
public class SampleService(IDateTimeProvider dateTimeProvider) : ISampleService
{
    public string Greet(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var now = dateTimeProvider.Now;
        return $"Hello, {name}. It's {now:HH:mm}.";
    }
}
```

### Test using `Library.UnitTesting`:
```csharp
[TestFixture]
public class SampleServiceTests : UnitTestBase<SampleService>
{
    private readonly Mock<IDateTimeProvider> dateTimeProviderMock = new();

    [Test]
    public void Greet_Valid_ReturnsCorrectResult()
    {
        // Arrange
        var name = this.Instantiator.Random<string>();
        var dateTime = this.Instantiator.Random<DateTime>();
        var expected = $"Hello, {name}. It's {dateTime:HH:mm}.";

        this.dateTimeProviderMock.Setup(x => x.Now).Returns(dateTime);

        var target = this.GetTarget();

        // Act
        var actual = target.Greet(name);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    protected override SampleService GetTarget()
    {
        return new SampleService(this.dateTimeProviderMock.Object);
    }
}
```

---

## 🙌 Contributing

Pull requests and issue reports welcome.  
This library reflects real-world patterns — so improvements from others are encouraged.

---

## 📄 License

MIT © [Rmkrs](https://github.com/Rmkrs)

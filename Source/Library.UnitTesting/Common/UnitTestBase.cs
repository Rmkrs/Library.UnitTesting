namespace Library.UnitTesting.Common;

using System;
using System.Linq;
using System.Threading.Tasks;
using Library.UnitTesting.Extensions;

using NUnit.Framework;

[TestFixture]
public abstract class UnitTestBase<T>
{
    public Instantiator Instantiator { get; private set; }

    public UnitTestBaseOptions Options { get; } = new();

    [SetUp]
    public void BaseSetUp()
    {
        this.Instantiator = new Instantiator();
        this.Setup();
    }

    [Test]
    public void Constructor_NullArgument_ThrowsArgumentNullException()
    {
        if (this.Options.ShouldConstructorArgumentsBeNullGuarded)
        {
            typeof(T).AssertConstructorGuard(this.Options);
        }
    }

    [Test]
    public async Task Methods_NullArgument_ThrowsArgumentNullException()
    {
        if (this.Options.ShouldMethodArgumentsBeNullGuarded)
        {
            await this.TestMethodsNullGuard().ConfigureAwait(false);
        }
    }

    [Test]
    public void AsyncMethods_WithReturnTypeVoid_ShouldBePrevented()
    {
        if (!this.Options.ShouldAsyncVoidMethodsBePrevented)
        {
            return;
        }

        var messages = typeof(T).Assembly.GetAsyncVoidMethods()
            .Select(method => $"{TypeTestingExtensions.PrettyTypeName(method.DeclaringType)}.{method.Name} is an async void method.")
            .ToList();

        Assert.That(messages.Count == 0, $"Async void methods found!{Environment.NewLine}{String.Join(Environment.NewLine, messages)}");
    }

    protected virtual void Setup()
    {
    }

    protected abstract T GetTarget();

    protected virtual Task TestMethodsNullGuard() => this.GetTarget().AssertMethodsNullGuard(this.Options);
}
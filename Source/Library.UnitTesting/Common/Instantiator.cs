namespace Library.UnitTesting.Common;

using System;
using System.Linq;

using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;

public class Instantiator
{
    private readonly Fixture fixture;

    public Instantiator()
    {
        this.fixture = new Fixture();
        this.fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        this.fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public T Random<T>()
    {
        return (T)this.Random(typeof(T));
    }

    public ICustomizationComposer<T> Build<T>()
    {
        return this.fixture.Build<T>();
    }

    public object Random(Type type)
    {
        return new SpecimenContext(this.fixture).Resolve(type);
    }

    public T[] Random<T>(int count)
    {
        return Enumerable.Range(1, count).Select(_ => this.Random<T>()).ToArray();
    }

    public void RegisterCreationFunction<T>(Func<T> func)
    {
        this.fixture.Register(func);
    }

    public void AddBehavior(ISpecimenBuilderTransformation behavior)
    {
        this.fixture.Behaviors.Add(behavior);
    }

    public void RemoveBehavior(ISpecimenBuilderTransformation behavior)
    {
        this.fixture.Behaviors.Remove(behavior);
    }

    public void AddCustomization(ISpecimenBuilder customization)
    {
        this.fixture.Customizations.Add(customization);
    }

    public void RemoveCustomization(ISpecimenBuilder customization)
    {
        this.fixture.Customizations.Remove(customization);
    }
}
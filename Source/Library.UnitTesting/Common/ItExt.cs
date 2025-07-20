namespace Library.UnitTesting.Common;

using System;
using FluentAssertions;
using Moq;

public static class ItExt
{
    public static T IsEquivalent<T>(T expected)
    {
        return It.Is<T>(a => Verify(a, expected));
    }

    public static DateTime IsCloseTo(DateTime expected, TimeSpan precision)
    {
        return It.Is<DateTime>(a => VerifyCloseTo(a, expected, precision));
    }

    public static T IsEquivalent<T>(T expected, Func<FluentAssertions.Equivalency.EquivalencyOptions<T>, FluentAssertions.Equivalency.EquivalencyOptions<T>> config)
    {
        return It.Is<T>(a => Verify(a, expected, config));
    }

    private static bool VerifyCloseTo(DateTime actual, DateTime expected, TimeSpan precision)
    {
        try
        {
            actual.Should().BeCloseTo(expected, precision);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool Verify<T>(T actual, T expected)
    {
        try
        {
            actual.Should().BeEquivalentTo(expected);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool Verify<T>(T actual, T expected, Func<FluentAssertions.Equivalency.EquivalencyOptions<T>, FluentAssertions.Equivalency.EquivalencyOptions<T>> config)
    {
        try
        {
            actual.Should().BeEquivalentTo(expected, config);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
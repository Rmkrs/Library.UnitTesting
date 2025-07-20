namespace Library.UnitTesting.Extensions;

using System;
using Library.UnitTesting.Common;

public static class InstantiatorExtensions
{
    private const string AllowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";

    private static readonly Random Random = new();

    public static string RandomString(this Instantiator _, int length)
    {
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = AllowedChars[Random.Next(0, AllowedChars.Length - 1)];
        }

        return new string(chars);
    }
}
namespace Library.UnitTesting.Common;

using System;
using System.Collections.Generic;

public class UnitTestBaseOptions
{
    public bool ShouldConstructorArgumentsBeNullGuarded { get; set; } = true;

    public bool ShouldMethodArgumentsBeNullGuarded { get; set; } = true;

    public bool ShouldAsyncVoidMethodsBePrevented { get; set; } = true;

    public Dictionary<Type, Func<object>> TypeCreationOverrides { get; set; } = [];

    public List<string> ConstructorArgumentsToBeExcludedForNullGuarding { get; set; } = [];

    public List<string> MethodsToBeExcludedForNullGuarding { get; set; } = [];

    public Dictionary<string, List<string>> MethodParametersToBeExcludedForNullGuarding { get; set; } = [];

    public void ExcludeConstructorArgumentForNullGuarding(string constructorArgumentName)
    {
        if (this.ConstructorArgumentsToBeExcludedForNullGuarding.Contains(constructorArgumentName))
        {
            return;
        }

        this.ConstructorArgumentsToBeExcludedForNullGuarding.Add(constructorArgumentName);
    }

    public void ExcludeMethodForNullGuarding(string methodName)
    {
        if (this.MethodsToBeExcludedForNullGuarding.Contains(methodName))
        {
            return;
        }

        this.MethodsToBeExcludedForNullGuarding.Add(methodName);
    }

    public void ExcludeMethodArgumentForNullGuarding(string methodName, string argumentName)
    {
        if (!this.MethodParametersToBeExcludedForNullGuarding.TryGetValue(methodName, out var value))
        {
            value = [];
            this.MethodParametersToBeExcludedForNullGuarding.Add(methodName, value);
        }

        if (value.Contains(argumentName))
        {
            return;
        }

        value.Add(argumentName);
    }
}
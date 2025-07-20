namespace Library.UnitTesting.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;

using Library.UnitTesting.Common;

using Moq;

using NUnit.Framework;

public static class TypeTestingExtensions
{
    private static readonly Random Random = new();

    public static void AssertConstructorGuard(this Type type, UnitTestBaseOptions options)
    {
        foreach (var constructor in type.GetConstructors())
        {
            var parameters = constructor.GetParameters();

            var mocks = parameters.Select(p => CreateParameter(p.ParameterType, options.TypeCreationOverrides)).ToArray();

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsValueType)
                {
                    continue;
                }

                if (options.ConstructorArgumentsToBeExcludedForNullGuarding.Contains(parameters[i].Name))
                {
                    continue;
                }

                var mocksCopy = mocks.ToArray();
                mocksCopy[i] = null;
                try
                {
                    constructor.Invoke(mocksCopy);
                    Assert.Fail($"ArgumentNullException expected for parameter {parameters[i].Name} of constructor, but no exception was thrown");
                }
                catch (TargetInvocationException ex)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var errorMessage = $"ArgumentNullException expected for parameter {parameters[i].Name} of constructor, but exception of type {ex.InnerException.GetType()} was thrown";
                    ex.InnerException.Should().BeOfType<ArgumentNullException>(errorMessage);
                }
            }
        }
    }

    public static async Task AssertMethodsNullGuard(this object target, UnitTestBaseOptions options)
    {
        var nullabilityInfoContext = new NullabilityInfoContext();

        foreach (var method in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            if (options.MethodsToBeExcludedForNullGuarding.Contains(method.Name))
            {
                continue;
            }

            var specificMethod = method.IsGenericMethod
                ? method.MakeGenericMethod(method.GetGenericArguments().Select(a => a.GetGenericParameterConstraints().FirstOrDefault() ?? typeof(List<string>)).ToArray())
                : method;

            var parameters = specificMethod.GetParameters();

            var mocks = parameters.Length > 1 ? parameters.Select(p => CreateParameter(p.ParameterType, options.TypeCreationOverrides)).ToArray() : [null];

            for (int i = 0; i < parameters.Length; i++)
            {
                var nullabilityInfo = nullabilityInfoContext.Create(parameters[i]);
                if (nullabilityInfo.WriteState == NullabilityState.Nullable)
                {
                    continue;
                }

                if (parameters[i].ParameterType.IsValueType)
                {
                    continue;
                }

                if (options.MethodParametersToBeExcludedForNullGuarding.TryGetValue(method.Name, out var value) && value.Contains(parameters[i].Name))
                {
                    continue;
                }

                var mocksCopy = mocks.ToArray();
                mocksCopy[i] = null;

                try
                {
                    var result = specificMethod.Invoke(target, mocksCopy);

                    if (result is Task resultTask)
                    {
                        await resultTask.ConfigureAwait(false);
                    }

                    if (result is ValueTask resultValueTask)
                    {
                        await resultValueTask.ConfigureAwait(false);
                    }

                    if (specificMethod.ReturnType.IsGenericType &&
                        specificMethod.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        var asTaskMethod = specificMethod.ReturnType.GetMethod(nameof(ValueTask<object>.AsTask), []);
                        var asTaskResult = asTaskMethod?.Invoke(result, []);
                        if (asTaskResult is Task asTask)
                        {
                            await asTask.ConfigureAwait(false);
                        }
                    }

                    Assert.Fail(
                        $"ArgumentNullException expected for parameter {parameters[i].Name} of {method.Name}, but no exception was thrown");
                }
                catch (ArgumentNullException)
                {
                    // We don't want an exception to bubble up when it's the ArgumentNullException.
                }
                catch (TargetInvocationException ex)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var errorMessage = $"ArgumentNullException expected for parameter {parameters[i].Name} of {method.Name}, but exception of type {ex.InnerException.GetType()} was thrown";
                    ex.InnerException.Should().BeOfType<ArgumentNullException>(errorMessage);
                }
            }
        }
    }

    public static IEnumerable<MethodInfo> GetAsyncVoidMethods(this Assembly assembly)
    {
        return assembly.GetLoadableTypes()
            .SelectMany(m => m.GetMethods(
                            BindingFlags.NonPublic
                            | BindingFlags.Public
                            | BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.DeclaredOnly))
            .Where(method => method.HasAttribute<AsyncStateMachineAttribute>())
            .Where(method => method.ReturnType == typeof(void))
            .Where(method => !IsEventHandler(method));
    }

    public static string PrettyTypeName(Type t)
    {
        return t.IsGenericType
            ? $"{t.Name[..t.Name.LastIndexOf('`')]}<{string.Join(", ", t.GetGenericArguments().Select(PrettyTypeName))}>" 
            : t.Name;
    }

    private static bool HasAttribute<TAttribute>(this MethodInfo method)
        where TAttribute : Attribute
    {
        return method.GetCustomAttributes(typeof(TAttribute), false).Length != 0;
    }

    private static bool IsEventHandler(MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
        if (parameters.Length != 2)
        {
            return false;
        }

        return parameters[0].ParameterType == typeof(Object) && parameters[1].ParameterType.BaseType == typeof(EventArgs);
    }

    private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }

    private static object CreateParameter(Type parameterType, Dictionary<Type, Func<object>> typeCreationOverrides)
    {
        if (typeCreationOverrides.TryGetValue(parameterType, out var typeCreationOverride))
        {
            return typeCreationOverride.Invoke();
        }

        var specialType = CreateSpecialType(parameterType);
        if (specialType != null)
        {
            return specialType;
        }

        if (IsMockableType(parameterType))
        {
            return CreateMock(parameterType);
        }

        return CreateType(parameterType, typeCreationOverrides);
    }

    private static object CreateSpecialType(Type parameterType)
    {
        if (parameterType.IsEnum)
        {
            return CreateEnumValue(parameterType);
        }

        if (parameterType.IsValueType)
        {
            return CreateValueType(parameterType);
        }

        return null;
    }

    private static object CreateMock(Type type)
    {
        Type mockType = typeof(Mock<>).MakeGenericType(type);

        // ReSharper disable once PossibleNullReferenceException
        return ((Mock)Activator.CreateInstance(mockType)).Object;
    }

    private static bool IsMockableType(Type type)
    {
        if (type.Name.StartsWith("Func`"))
        {
            return true;
        }

        if (type.FullName?.StartsWith("System.Action") == true)
        {
            return true;
        }

        if (type.IsInterface)
        {
            return true;
        }

        if (type.IsSubclassOf(typeof(Delegate)) || type.IsSubclassOf(typeof(MulticastDelegate)))
        {
            return true;
        }

        if (type.IsSealed || (type.IsClass && !type.IsAbstract))
        {
            return false;
        }

        return true;
    }

    private static object CreateEnumValue(Type parameterType)
    {
        Array values = Enum.GetValues(parameterType);
        return values.Length == 0 ? null : values.GetValue(Random.Next(values.Length));
    }

    private static object CreateValueType(Type parameterType)
    {
        Func<object> f = GetDefault<object>;
        return f.Method.GetGenericMethodDefinition().MakeGenericMethod(parameterType).Invoke(null, null);
    }

    private static T GetDefault<T>()
    {
        return default;
    }

    private static object CreateType(Type type, Dictionary<Type, Func<object>> typeCreationOverrides)
    {
        if (type == typeof(string))
        {
            return Guid.NewGuid().ToString();
        }

        ConstructorInfo[] constructors = type.GetConstructors();

        ParameterInfo[] parameters = constructors[0].GetParameters();
        int argumentCount = parameters.Length;

        object[] constructorArguments = new object[argumentCount];
        for (int index = 0; index < argumentCount; ++index)
        {
            object instance = CreateParameter(parameters[index].ParameterType, typeCreationOverrides);
            constructorArguments[index] = instance;
        }

        try
        {
            return Activator.CreateInstance(type, constructorArguments);
        }
        catch (Exception)
        {
            return RuntimeHelpers.GetUninitializedObject(type);
        }
    }
}
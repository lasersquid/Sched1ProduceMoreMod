﻿using MelonLoader;
using System;
using System.Reflection;

internal static class MethodBaseHelper
{
    private static Type RuntimeMethodHandleInternal;
    private static ConstructorInfo RuntimeMethodHandleInternal_Constructor;
    private static Type RuntimeType;
    private static MethodInfo RuntimeType_GetMethodBase;

    public static MethodBase GetMethodBaseFromHandle(IntPtr handle)
    {
        try
        {
            RuntimeMethodHandleInternal ??= typeof(RuntimeMethodHandle).Assembly.GetType("System.RuntimeMethodHandleInternal", throwOnError: true)!;
            RuntimeMethodHandleInternal_Constructor ??= RuntimeMethodHandleInternal.GetConstructor
            (
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DoNotWrapExceptions,
                binder: null,
                new[] { typeof(IntPtr) },
                modifiers: null
            ) ?? throw new InvalidOperationException("RuntimeMethodHandleInternal constructor is missing!");

            RuntimeType ??= typeof(Type).Assembly.GetType("System.RuntimeType", throwOnError: true)!;
            RuntimeType_GetMethodBase ??= RuntimeType.GetMethod
            (
                "GetMethodBase",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DoNotWrapExceptions,
                binder: null,
                new[] { RuntimeType, RuntimeMethodHandleInternal },
                modifiers: null
            ) ?? throw new InvalidOperationException("RuntimeType.GetMethodBase is missing!");

            // Wrap the handle
            object runtimeHandle = RuntimeMethodHandleInternal_Constructor.Invoke(new[] { (object)handle });
            return (MethodBase)RuntimeType_GetMethodBase.Invoke(null, new[] { null, runtimeHandle });
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"Caught exception {ex.ToString()}");
            return null;
        }
    }
}

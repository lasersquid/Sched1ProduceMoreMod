using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime;
using System.Linq;
using System.Reflection;

public static unsafe class Program
{
    public static void Main(string[] args)
    {
        IntPtr user32 = NativeLibrary.Load("User32.dll");

        delegate*<void> managed1 = &TestMethod;
        delegate*<void> managed2 = &AnotherMethod;
        delegate*<void> managed3 = &MessageBoxW;
        delegate* unmanaged<void> unmanaged1 = &UnmanagedMethod;
        delegate* unmanaged<void> unmanaged2 = (delegate* unmanaged<void>)NativeLibrary.GetExport(user32, "MessageBoxW");

        // Note that this means any methods JITted after we create the snapshot won't be available
        // You can use AttachToProcess on yourself, but it's not supported.
        // https://github.com/microsoft/clrmd/blob/master/doc/FAQ.md#can-i-use-this-api-to-inspect-my-own-process
        using DataTarget target = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id);
        ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();

        void Test(string testName, void* functionPointer)
        {
            ClrMethod? method = runtime.GetMethodByInstructionPointer((ulong)functionPointer);

            if (method is null)
            {
                Console.WriteLine($"{testName}: Not found");
                return;
            }

            Console.WriteLine($"{testName}: {method.Signature}");
            MethodBase? methodBase = MethodBaseHelper.GetMethodBaseFromHandle((IntPtr)method.MethodDesc);
            Console.WriteLine($"    MethodBase: {methodBase?.Name ?? "Not Found"}");
}

        Test("managed1", managed1);
        Test("managed2", managed2);
        Test("managed3", managed3);
        Test("unmanaged1", unmanaged1);
        Test("unmanaged2", unmanaged2); // This is expected to not be found because it's a native method

          }

    public static void TestMethod()
    { }

    public static void AnotherMethod()
    { }

    [UnmanagedCallersOnly]
    public static void UnmanagedMethod()
    { }

    [DllImport("User32.dll")]
    public static extern void MessageBoxW(); // Obviously this won't be callable, it's just here so it resolves
}
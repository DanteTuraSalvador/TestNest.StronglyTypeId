using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestNest.StronglyTypeId.StronglyTypeIds;

namespace GuestIdConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    DisplayMenu();

                    var choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1": RunBasicTests(); break;
                        case "2": RunErrorHandlingTests(); break;
                        case "3": return;
                        default: Console.WriteLine("Invalid choice"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n⚠️ Unexpected error: {ex.Message}");
                }
                finally
                {
                    if (!IsRunningInCI()) // Only wait for key press when not in CI
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
        }

        static void DisplayMenu()
        {
            Console.WriteLine("=== GuestId Class Tester ===");
            Console.WriteLine("1. Basic Functionality Tests");
            Console.WriteLine("2. Error Handling Tests");
            Console.WriteLine("3. Exit");
            Console.Write("Choose test mode (1-3): ");
        }

        static void RunBasicTests()
        {
            Console.WriteLine("\n=== Basic Functionality ===");
            var testGuid = Guid.NewGuid();

            ExecuteTestCases(new()
            {
                ["Construction from Guid"] = () => new GuestId(testGuid),
                ["New() creation"] = GuestId.New,
                ["Parse() valid string"] = () => GuestId.Parse(testGuid.ToString()),
                ["Explicit conversion"] = () => (GuestId)testGuid.ToString(),
                ["Empty instance"] = () => GuestId.Empty
            });
        }

        static void RunErrorHandlingTests()
        {
            Console.WriteLine("\n=== Error Handling ===");

            ExecuteTestCases(new()
            {
                ["Empty Guid construction"] = () => new GuestId(Guid.Empty),
                ["Invalid string Parse()"] = () => GuestId.Parse("invalid-guid"),
                ["Null explicit conversion"] = () => (GuestId)null,
                ["TryParse invalid"] = () => GuestId.TryParse("bad-input", out _)
            }, expectError: true);
        }

        static void ExecuteTestCases(Dictionary<string, Func<object>> testCases, bool expectError = false)
        {
            foreach (var test in testCases)
            {
                Console.Write($"{test.Key}: ");
                try
                {
                    var result = test.Value();
                    Console.WriteLine(expectError ? "UNEXPECTED SUCCESS" : $"✅ {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(expectError ? $"✅ Expected error: {ex.Message}" : $"❌ Unexpected error: {ex.Message}");
                }
            }
        }

        static bool IsRunningInCI() =>
            Environment.GetEnvironmentVariable("CI")?.ToLower() == "true";
    }
}
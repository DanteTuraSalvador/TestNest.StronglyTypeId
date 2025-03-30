using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestNest.StronglyTypeId.StronglyTypeIds;

namespace GuestIdConsoleTester
{
    class Program
    {
        static async Task Main(string[] args)
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
                        case "3": await RunConcurrencyTests(); break;
                        case "4": RunBenchmarks(); break;
                        case "5": return;
                        default: Console.WriteLine("Invalid choice"); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n⚠️ Unexpected error: {ex.Message}");
                    Console.ResetColor();
                }
                finally
                {
                    if (!IsRunningInCI())
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
        }

        static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== GuestId Class Tester ===");
            Console.ResetColor();
            Console.WriteLine("1. Basic Functionality Tests");
            Console.WriteLine("2. Error Handling Tests");
            Console.WriteLine("3. Concurrency Tests");
            Console.WriteLine("4. Benchmark Tests");
            Console.WriteLine("5. Exit");
            Console.Write("Choose test mode (1-5): ");
        }

        static void RunBasicTests()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Basic Functionality ===");
            Console.ResetColor();

            var testGuid = Guid.NewGuid();

            ExecuteTestCases(new()
            {
                ["Construction from Guid"] = () => new GuestId(testGuid),
                ["New() creation"] = GuestId.New,
                ["Parse() valid string"] = () => GuestId.Parse(testGuid.ToString()),
                ["Explicit conversion"] = () => (GuestId)testGuid.ToString(),
                ["Empty instance"] = () => GuestId.Empty(),
                ["Empty instance singleton"] = () =>
                {
                    var id1 = GuestId.Empty();
                    var id2 = GuestId.Empty();
                    return $"Same instance: {object.ReferenceEquals(id1, id2)}";
                },
                ["Value equality"] = () =>
                {
                    var id1 = new GuestId(testGuid);
                    var id2 = new GuestId(testGuid);
                    return $"Equal: {id1 == id2}, Same: {object.ReferenceEquals(id1, id2)}";
                },
                ["TryParse valid"] = () =>
                {
                    GuestId.TryParse(testGuid.ToString(), out var result);
                    return result?.ToString() ?? "null";
                }
            });
        }

        static void RunErrorHandlingTests()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Error Handling ===");
            Console.ResetColor();

            ExecuteTestCases(new()
            {
                ["Empty Guid construction"] = () => new GuestId(Guid.Empty),
                ["Invalid string Parse()"] = () => GuestId.Parse("invalid-guid"),
                ["Null explicit conversion"] = () => (GuestId)null,
                ["Empty string Parse()"] = () => GuestId.Parse(""),
                ["TryParse invalid"] = () => GuestId.TryParse("bad-input", out _),
                ["TryParse empty"] = () => GuestId.TryParse(Guid.Empty.ToString(), out _)
            }, expectError: true);
        }

        static async Task RunConcurrencyTests()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Concurrency Tests ===");
            Console.ResetColor();

            // Test New() concurrency
            var newTasks = new List<Task<GuestId>>();
            for (int i = 0; i < 10; i++)
            {
                newTasks.Add(Task.Run(() => GuestId.New()));
            }
            var newResults = await Task.WhenAll(newTasks);
            Console.WriteLine($"New() - Unique IDs: {newResults.Distinct().Count()}/{newResults.Length}");

            // Test Empty() concurrency
            var emptyTasks = new List<Task<GuestId>>();
            for (int i = 0; i < 10; i++)
            {
                emptyTasks.Add(Task.Run(() => GuestId.Empty()));
            }
            var emptyResults = await Task.WhenAll(emptyTasks);
            Console.WriteLine($"Empty() - Same instance: {emptyResults.Distinct().Count() == 1}");
        }

        static void RunBenchmarks()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Benchmark Tests ===");
            Console.ResetColor();

            BenchmarkOperation("Creating 100,000 IDs with New()", () =>
            {
                for (int i = 0; i < 100_000; i++)
                {
                    var _ = GuestId.New();
                }
            });

            BenchmarkOperation("Creating 100,000 IDs with Empty()", () =>
            {
                for (int i = 0; i < 100_000; i++)
                {
                    var _ = GuestId.Empty();
                }
            });

            BenchmarkOperation("Parsing 10,000 valid GUIDs", () =>
            {
                var guid = Guid.NewGuid().ToString();
                for (int i = 0; i < 10_000; i++)
                {
                    var _ = GuestId.Parse(guid);
                }
            });
        }

        static void BenchmarkOperation(string description, Action operation)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            operation();
            sw.Stop();
            Console.WriteLine($"{description}: {sw.ElapsedMilliseconds}ms");
        }

        static void ExecuteTestCases(Dictionary<string, Func<object>> testCases, bool expectError = false)
        {
            foreach (var test in testCases)
            {
                Console.Write($"{test.Key}: ");
                try
                {
                    var result = test.Value();
                    if (expectError)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("UNEXPECTED SUCCESS");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✅ {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = expectError ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine(expectError ? $"✅ Expected error: {ex.Message}" : $"❌ Unexpected error: {ex.Message}");
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        static bool IsRunningInCI() =>
            Environment.GetEnvironmentVariable("CI")?.ToLower() == "true";
    }
}
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TestNest.StronglyTypeId.Common;
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
                        case "3": RunJsonTests(); break;
                        case "4": await RunModelBindingTests(); break;
                        case "5": return;
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
            Console.WriteLine("3. JSON Serialization Tests");
            Console.WriteLine("4. Model Binding Tests");
            Console.WriteLine("5. Exit");
            Console.Write("Choose test mode (1-5): ");
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

        static void RunJsonTests()
        {
            Console.WriteLine("\n=== JSON Serialization ===");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new StronglyTypedIdJsonConverter<GuestId>() }
            };

            // Successful serialization
            var originalId = GuestId.New();
            var json = JsonSerializer.Serialize(originalId, options);
            Console.WriteLine($"✅ Serialized: {json}");

            // Successful deserialization
            var deserialized = JsonSerializer.Deserialize<GuestId>(json, options);
            Console.WriteLine($"✅ Deserialized matches original: {originalId == deserialized}");

            // Failed deserialization
            ExecuteTestCases(new()
            {
                ["Invalid JSON"] = () => JsonSerializer.Deserialize<GuestId>("\"invalid\"", options)
            }, expectError: true);
        }

        static async Task RunModelBindingTests()
        {
            Console.WriteLine("\n=== Model Binding Tests ===");

            var testCases = new Dictionary<string, Func<Task>>
            {
                ["Valid GUID"] = () => TestModelBinder(Guid.NewGuid().ToString()),
                ["Empty string"] = () => TestModelBinder(""),
                ["Invalid format"] = () => TestModelBinder("not-a-guid"),
                ["Empty GUID"] = () => TestModelBinder(Guid.Empty.ToString()),
                ["Null value"] = () => TestModelBinder(null)
            };

            foreach (var test in testCases)
            {
                Console.WriteLine($"\nTest Case: {test.Key}");
                await test.Value();
            }
        }

        static async Task TestModelBinder(string testValue)
        {
            try
            {
                var binder = new StronglyTypedIdModelBinder<GuestId>();
                var provider = new TestValueProvider { Value = testValue };

                var context = DefaultModelBindingContext.CreateBindingContext(
                    new ActionContext(),
                    provider,
                    new EmptyModelMetadataProvider().GetMetadataForType(typeof(GuestId)),
                    new BindingInfo(),
                    "testField");

                await binder.BindModelAsync(context);

                if (context.Result.IsModelSet)
                {
                    Console.WriteLine($"✅ Success: {context.Result.Model}");
                }
                else
                {
                    Console.WriteLine("❌ Failed");
                    if (context.ModelState.TryGetValue("testField", out var entry))
                    {
                        foreach (var error in entry.Errors)
                        {
                            Console.WriteLine($"  - {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Unexpected error: {ex.Message}");
            }
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

    class TestValueProvider : IValueProvider
    {
        public string Value { get; set; }

        public bool ContainsPrefix(string prefix) => true;

        public ValueProviderResult GetValue(string key) =>
            Value != null ? new ValueProviderResult(Value) : ValueProviderResult.None;
    }
}
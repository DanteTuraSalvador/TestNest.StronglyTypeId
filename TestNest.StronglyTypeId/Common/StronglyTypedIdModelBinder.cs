using Microsoft.AspNetCore.Mvc.ModelBinding; // Importing the ASP.NET Core model binding library.
using TestNest.StronglyTypeId.Exceptions; // Importing our custom exceptions for strongly typed IDs.


namespace TestNest.StronglyTypeId.Common;

// This is a custom model binder for ASP.NET Core that helps convert request values into strongly typed IDs.
public class StronglyTypedIdModelBinder<T> : IModelBinder where T : StronglyTypedId<T>
{
    // This method is called when ASP.NET tries to bind a request value to a strongly typed ID.
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // First, we check if the binding context is null. If it is, that's a big problem!
        // It means something went very wrong with model binding, so we throw an exception.
        if (bindingContext is null)
            throw StronglyTypedIdException.ModelBindingNullContext();

        // Get the value from the request (e.g., from query params, route values, or body).
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        // If the value is missing (not provided in the request), we throw an exception.
        if (valueProviderResult == ValueProviderResult.None)
            throw StronglyTypedIdException.ModelBindingMissingValue();

        // Extract the actual string value from the request.
        var value = valueProviderResult.FirstValue;

        // Try to convert the string into a valid GUID.
        if (Guid.TryParse(value, out var guid))
        {
            // If parsing succeeds, we attempt to create an instance of our strongly typed ID.
            var idInstance = Activator.CreateInstance(typeof(T), guid) as T;

            // If we couldn't create an instance (Activator.CreateInstance returned null), something went wrong.
            // Maybe the strongly typed ID class doesn't have the right constructor.
            if (idInstance is null)
                throw StronglyTypedIdException.ModelBindingCreationFailure();

            // If everything went well, we set the result to a successful model binding.
            bindingContext.Result = ModelBindingResult.Success(idInstance);
            return Task.CompletedTask; // We're done!
        }

        // If the string wasn't a valid GUID, we throw an exception because we can't bind it.
        throw StronglyTypedIdException.ModelBindingInvalidFormat();
    }
}
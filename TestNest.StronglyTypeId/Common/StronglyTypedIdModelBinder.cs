using Microsoft.AspNetCore.Mvc.ModelBinding;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Common;
public class StronglyTypedIdModelBinder<T> : IModelBinder where T : StronglyTypedId<T>
{

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        // Handle missing value
        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"A value is required for {typeof(T).Name}");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;

        // Handle null/empty
        if (string.IsNullOrEmpty(value))
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"Value cannot be null or empty for {typeof(T).Name}");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Try parsing GUID
        if (!Guid.TryParse(value, out var guid))
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"Invalid GUID format for {typeof(T).Name}");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Handle empty GUID
        if (guid == Guid.Empty)
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"GUID cannot be empty for {typeof(T).Name}");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Create instance
        try
        {
            var idInstance = Activator.CreateInstance(typeof(T), guid) as T;
            bindingContext.Result = ModelBindingResult.Success(idInstance);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"Failed to create {typeof(T).Name}: {ex.Message}");
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

    //public Task BindModelAsync(ModelBindingContext bindingContext)
    //{
    //    if (bindingContext == null)
    //        throw StronglyTypedIdException.NullBindingContext();

    //    var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

    //    if (valueProviderResult == ValueProviderResult.None)
    //        throw StronglyTypedIdException.MissingValue(bindingContext.ModelName);

    //    var value = valueProviderResult.FirstValue;

    //    if (string.IsNullOrEmpty(value))
    //        throw StronglyTypedIdException.InvalidModelValue(bindingContext.ModelName, value);

    //    if (!Guid.TryParse(value, out var guid))
    //        throw StronglyTypedIdException.InvalidModelValue(bindingContext.ModelName, value);

    //    var idInstance = Activator.CreateInstance(typeof(T), guid) as T;
    //    if (idInstance is null)
    //        throw StronglyTypedIdException.ModelCreationFailed(typeof(T));

    //    bindingContext.Result = ModelBindingResult.Success(idInstance);
    //    return Task.CompletedTask;
    //}
}
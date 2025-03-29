using Microsoft.AspNetCore.Mvc.ModelBinding;
using TestNest.Domain.Exceptions;

namespace TestNest.StronglyTypeId.Common;

public class StronglyTypedIdModelBinder<T> : IModelBinder where T : StronglyTypedId<T>
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new StronglyTypedIdException.ModelBinder.NullBindingContext();

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
            throw new StronglyTypedIdException.ModelBinder.MissingValue(bindingContext.ModelName);

        var value = valueProviderResult.FirstValue;

        if (Guid.TryParse(value, out var guid))
        {
            var idInstance = Activator.CreateInstance(typeof(T), guid) as T;
            if (idInstance is null)
                throw new StronglyTypedIdException.ModelBinder.ModelCreationFailure(typeof(T));

            bindingContext.Result = ModelBindingResult.Success(idInstance);
            return Task.CompletedTask;
        }

        throw new StronglyTypedIdException.ModelBinder.InvalidFormat(bindingContext.ModelName, value);
    }
}


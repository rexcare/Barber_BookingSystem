using Base.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Helpers.WebApp;

public class LangStrBinderProvider : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;
        if (value == null)
        {
            return Task.CompletedTask;
        }
        
        bindingContext.Result = ModelBindingResult.Success(new LangStr(value));

        return Task.CompletedTask;
    }
}
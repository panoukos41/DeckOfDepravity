using Core.Commons.Behaviors;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

/// <summary>
/// Adds the following behaviors/services in the following order:
/// <br/> - <see cref="LogRequestBehavior{TMessage, TResponse}"/> Logs requests.
/// <br/> - <see cref="RunnerBehavior{TMessage, TResponse}"/> Ensures the pipeline runs or transforms it to an Er result.
/// <br/> - <see cref="ValidBehavior{TMessage, TResponse}"/> Runs the <see cref="IValid.Validator"/> of the message.
/// <br/> - <see cref="FluentValidationBehavior{TMessage, TResponse}"/> Runs any registered <see cref="IValidator{T}"/> for the message.
/// <br/> - <see cref="TimeProvider"/> tries to add the <see cref="TimeProvider.System"/> as singleton.
/// </summary>
public sealed class CoreAppModule : IAppModule<CoreAppModule>
{
    public static void Add(IServiceCollection services, IConfiguration configuration, CoreAppModule module)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LogRequestBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(RunnerBehavior<,>));

        services.TryAddSingleton(TimeProvider.System);
    }
}

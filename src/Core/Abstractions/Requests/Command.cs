﻿using FluentValidation;
using Mediator;

namespace Core.Abstractions.Requests;

public interface IRequestId
{
    Guid RequestId { get; init; }
}

/// <summary>
/// Represents a base command.
/// </summary>
/// <typeparam name="TResult">The type of the result object.</typeparam>
public abstract record Command<TResult> : ICommand<Result<TResult>>, IRequestId
    where TResult : notnull
{
    public Guid RequestId { get; init; } = Guid.NewGuid();
}

/// <summary>
/// Represents a <see cref="Command{TResult}"/> with <see cref="IValid"/> data.
/// </summary>
/// <typeparam name="TData">The type of the data.</typeparam>
public abstract record Command<TData, TResult> : Command<TResult>, IValid
    where TData : notnull, IValid
    where TResult : notnull
{
    public TData Data { get; }

    protected Command(TData data)
    {
        Data = data;
    }

    public static IValidator Validator { get; } = InlineValidator.For<Command<TData, TResult>>(data =>
    {
        data.RuleFor(x => x.Data).SetValidator((IValidator<TData>)TData.Validator);
    });
}

/// <summary>
/// Represents a delete command. It accepts a <see cref="Uuid"/> of the resource to delete.
/// </summary>
public abstract record DeleteCommand : Command<Void>, IValid
{
    public Uuid Id { get; }

    protected DeleteCommand(Uuid id)
    {
        Id = id;
    }

    protected DeleteCommand(IEntity model)
    {
        Id = model.Id;
    }

    public static IValidator Validator { get; } = InlineValidator.For<DeleteCommand>(data =>
    {
        data.RuleFor(x => x.Id)
            .NotEmpty();
    });
}
using FluentResults;
using Mediator;

namespace Application.Interfaces;

public interface IResultQuery<T> : IQuery<Result<T>> { }
public interface IResultCommand<T> : ICommand<Result<T>> { }
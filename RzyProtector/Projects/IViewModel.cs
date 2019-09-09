using System;

namespace RzyProtector
{
    public interface IViewModel<TModel>
    {
        TModel Model { get; }
    }
}
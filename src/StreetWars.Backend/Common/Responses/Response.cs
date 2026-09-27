using System;

namespace StreetWars.Backend.Common.Responses;


public sealed record Response<T>(T? Data, IEnumerable<string> Errors = null!) where T : class, new()
{
    public static Response<T> Success(T? data) => new(data);
    public static Response<T> Failure(IEnumerable<string> errors) => new(null!, errors);
}
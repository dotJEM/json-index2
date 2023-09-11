namespace DotJEM.Json.Index2.Configuration;

public interface IServiceCollection
{
    bool TryGet<TService>(out TService value);
    TService Get<TService>();
}
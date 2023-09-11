namespace DotJEM.Json.Index2.Configuration;

public static class JsonIndexConfigurationExt
{
    public static TService Get<TService>(this IJsonIndexConfiguration self) => self.Services.Get<TService>();
    public static bool TryGet<TService>(this IJsonIndexConfiguration self, out TService service) => self.Services.TryGet(out service);
}
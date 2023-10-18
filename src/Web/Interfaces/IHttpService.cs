namespace Web.Interfaces;

public interface IHttpService
{
    public Task Send(HttpMethod method, string uri, object? body = null);
    public Task<T> Send<T>(HttpMethod method, string uri, object? body = null);
}

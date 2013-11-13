using Nancy.Bootstrapper;
using Nancy.Routing;
using NewRelicAgent = NewRelic.Api.Agent.NewRelic;

public class NewRelicStartup : IApplicationStartup
{
    private readonly IRouteResolver routeResolver;

    public NewRelicStartup(IRouteResolver routeResolver)
    {
        this.routeResolver = routeResolver;
    }

    public void Initialize(IPipelines pipelines)
    {
        pipelines.BeforeRequest.AddItemToStartOfPipeline(
        context =>
        {
            var route = routeResolver.Resolve(context);

            if (route == null || route.Route == null || route.Route.Description == null) // probably not necessary but don't want the chance of losing visibility on anything
            {
                NewRelicAgent.SetTransactionName(
                    context.Request.Method,
                    context.Request.Url.ToString());
            }
            else
            {
                NewRelicAgent.SetTransactionName(
                    route.Route.Description.Method,
                    route.Route.Description.Path);
            }
            return null;
        });
        pipelines.OnError.AddItemToEndOfPipeline(
            (context, ex) =>
            {
                NewRelicAgent.NoticeError(
                    ex);
                return null;
            });
    }
}
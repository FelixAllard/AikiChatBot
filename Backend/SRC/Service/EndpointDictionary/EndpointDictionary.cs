using System.Reflection;
using Backend.Endpoints;
using Backend.Model.Endpoints;

namespace Backend.Service.EndpointDictionary;

public class EndpointDictionary : IEndpointDictionary
{
    private List<IEndpoint> Endpoints { get; set; }
    private readonly ILogger<EndpointDictionary> _logger;

    EndpointDictionary(
        ILogger<EndpointDictionary> logger
        )
    {
        _logger = logger;
        
    }
    
    public int GenerateAllEndpoints()
    {
        Endpoints = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IEndpoint)Activator.CreateInstance(t))
            .ToList();
        _logger.LogInformation("Got all the endpoints and binded them to memory! Found {0} endpoints", Endpoints.Count);
        foreach (var endpoint in Endpoints)
        {
            _logger.LogInformation("Making sure parameter are in the right order for {0}", nameof(endpoint));
            for (int i = 0; i < endpoint.ParameterTypes.Length; i++)
            {
                if (i == endpoint.ParameterTypes[0].Order)
                    continue;
                _logger.LogWarning("A parameter was not at the right place, fixing for runtime, but you should make sure everything is in the right order");
                for (int j = i; j < endpoint.ParameterTypes.Length ; j++)
                {
                    
                    if (i == endpoint.ParameterTypes[j].Order)
                    {
                        
                        _logger.LogWarning("A parameter was not at the right place, fixing for runtime");
                        
                        
                    }
                    
                }
            }
            
        }
        
        return Endpoints.Count;
    }
    
}
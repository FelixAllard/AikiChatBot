namespace Backend.Service.EndpointDictionary;

/// <summary>
/// This class is responsible for the system to be able to associate each http call to a certain String
/// it will also serve the purpose of defining every single endpoint and in which context should they be used.
/// </summary>
public interface IEndpointDictionary
{

    public int GenerateAllEndpoints();

}
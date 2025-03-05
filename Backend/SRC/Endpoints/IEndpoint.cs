using Backend.Model.Endpoints;

namespace Backend.Endpoints;

/// <summary>
/// This class will be implemented for every single endpoint. This way, we will be able to get all endpoint loaded into assembly without manually getting all of them
/// </summary>
public interface IEndpoint
{
    ///This name must be exact because it will be used in order to find the exact 
    string MethodName { get; set; }
    /// <summary>
    /// All the parameters that are part of the function and their description
    /// </summary>
    Parameter[] ParameterTypes { get; set; }
    /// <summary>
    /// The purpose of this endpoint. THIS WILL BE FED IN THE MODEL
    /// </summary>
    string Purpose { get; set; }
    /// <summary>
    /// The platform on which the endpoint is provided
    /// </summary>
    string Platform { get; set; }
}
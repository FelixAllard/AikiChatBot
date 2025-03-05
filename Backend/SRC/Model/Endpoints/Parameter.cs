namespace Backend.Model.Endpoints;
/// <summary>
/// This class will be used in hold parameters
/// </summary>
public class Parameter
{
    /// <summary>
    /// The order in which this parameter is 
    /// </summary>
    public int Order { get; set; }
    /// <summary>
    /// What the parameter is, this will be fed into the Model so make sure the description is accurate
    /// </summary>
    public string ParameterDescription { get; set; }
    /// <summary>
    /// The type of the parameter
    /// </summary>
    public Type ParameterType { get; set; }
}
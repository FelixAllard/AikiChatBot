using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer;

public class Customer
{
    [Key]
    private string id { get; set; }
    private string displayName { get; set; }
    private string suspendedOn { get; set; }
    private string[] path { get; set; }
    
    
}
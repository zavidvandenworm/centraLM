using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class User: Base
{
    public required string ProviderId { get; set; }
    public required string ProviderName { get; set; }
    
    public string DisplayName { get; set; } = "User";
}
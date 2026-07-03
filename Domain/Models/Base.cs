using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class Base
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
}
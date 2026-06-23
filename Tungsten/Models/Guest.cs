using System.ComponentModel.DataAnnotations.Schema;

namespace Tungsten.Models;

public class Guest {
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? FamilyId { get; set; }
    public string? EmailAddress { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    public string? Zip { get; set; }

    public bool? IsAttending { get; set; }
    public string? MealChoice { get; set; }
    public string? MealComments { get; set; }
    public string? SongRequest { get; set; }
    public string? Comments { get; set; }
    public bool? IsVegan { get; set; }
    public bool? IsGlutenFree { get; set; }
    public bool? IsKosher { get; set; }
    public DateTime? RsvpResponseDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastChangeDate { get; set; }

    [NotMapped]
    public int Num { get; internal set; }
}

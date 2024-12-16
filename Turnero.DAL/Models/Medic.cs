using System.ComponentModel.DataAnnotations.Schema;

namespace Turnero.DAL.Models;

public class Medic
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Display(Name = "Nombre")]
    public string Name { get; set; }
    [Display(Name = "Usuario")]
    public string UserGuid { get; set; }

    public ICollection<Turn> Turns { get; set; }
}

public class MedicDto
{
    public Guid? Id { get; set; }
    [Display(Name = "Nombre")]
    public string Name { get; set; }
}

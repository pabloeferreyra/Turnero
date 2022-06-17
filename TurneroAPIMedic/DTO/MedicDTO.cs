using System.ComponentModel.DataAnnotations;

namespace TurneroAPI.DTO
{
    public class MedicDTO
{
        public Guid Id { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        [Display(Name = "Usuario")]
        public string UserGuid { get; set; }
    }
}

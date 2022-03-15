using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnero.Models
{
    public class Turn
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        [Display(Name = "Nombre"), Required]
        public string Name { get; set; }

        [StringLength(10, MinimumLength = 6), Required]
        public string Dni { get; set; }
        
        [Display(Name = "Médico")]
        public Medic Medic { get; set; }
        public Guid MedicId { get; set; }
        
        [Display(Name = "Fecha"), Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateTurn { get; set; }

        [Display(Name = "Hora")]
        public TimeTurnViewModel Time { get; set; }
        public Guid TimeId { get; set; }
        
        [Display(Name = "Obra Social")]
        public string SocialWork { get; set; }
        
        [Display(Name ="Motivo")]
        public string Reason { get; set; }

        [Display(Name = "Ingresado")]
        public bool Accessed { get; set; }
    }
}

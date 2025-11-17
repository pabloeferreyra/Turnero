using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnero.DAL.Models;

public class PatientFKEntity
{
    [Key]
    [ForeignKey(nameof(Patient))]
    public Guid Id { get; set; }
}

using System;
using System.Collections.Generic;

namespace Turnero.Models; public class TimeTurnViewModel { public Guid Id { get; set; } public string Time { get; set; } public ICollection<Turn> Turns { get; set; } }

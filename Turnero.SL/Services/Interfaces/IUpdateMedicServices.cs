﻿namespace Turnero.SL.Services.Interfaces; public interface IUpdateMedicServices { Task<bool> Update(Medic medic); void Delete(Medic medic); }
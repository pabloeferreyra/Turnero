﻿using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IDeleteTimeTurnServices
    {
        Task Delete(TimeTurnViewModel timeTurn);
    }
}
﻿using System.Collections.Generic;

namespace Turnero.DAL.Models;

public class UserClaimsViewModel
{
    public UserClaimsViewModel()
    {
        Claims = new List<UserClaim>();
    }

    public string UserId { get; set; }
    public List<UserClaim> Claims { get; set; }
}

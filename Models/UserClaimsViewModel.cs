﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turnero.Models
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }

        public string UserId { get; set; }
        public List<UserClaim> Claims { get; set; }
    }
}

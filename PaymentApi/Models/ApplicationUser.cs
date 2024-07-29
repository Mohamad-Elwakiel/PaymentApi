﻿using Microsoft.AspNetCore.Identity;

namespace PaymentApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
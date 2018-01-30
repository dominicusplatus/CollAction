﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CollAction.Models.AccountViewModels
{
    public class LoginViewModel
    {
        // Email
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        // Password
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; }

        // Remember me?
        [Display(Name = "Onthoud gegevens?")]
        public bool RememberMe { get; set; }
    }
}

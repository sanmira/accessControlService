using System;
using System.ComponentModel.DataAnnotations;

namespace accessControlService.Models
{
    public class User
    {
        [Key]
        public string key { get; set; }

        public string name { get; set; }
    }
}

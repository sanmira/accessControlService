using System;
using System.ComponentModel.DataAnnotations;

namespace accessControlService.Models
{
    public class AccessRequest
    {
        [Key]
        public int id { get; set; }

        public DateTime date { get; set; }

        //[ForeignKey(nameof(User))]
        public string user { get; set; }

        public bool result { get; set; }
    }
}

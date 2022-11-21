using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace StarLink_Blog.Models
{
    public class EmailData
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }

        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Message { get; set; }
    }
}

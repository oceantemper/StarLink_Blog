using System.ComponentModel.DataAnnotations;

namespace StarLink_Blog.Models
{
    public class Comment
    {
        //Primary key
        public int Id { get; set; }

        // foreing key 
        public int BlogPostId { get; set; }

        [Required]
        public string? AuthorId { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime? LastUpdated { get; set; }

        public string? UpdateReason { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Body { get; set; }


        // Navigation properties 


        public virtual BlogPost? BlogPost { get; set; }

        //add the realtionship to the BlogUser

        public virtual BlogUser? Author { get; set; }
    }
}

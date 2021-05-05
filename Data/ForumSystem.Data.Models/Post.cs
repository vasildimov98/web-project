﻿namespace ForumSystem.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using ForumSystem.Data.Common.Models;

    public class Post : BaseDeletableModel<int>
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Category
    {

        private ICollection<Article> article;

        public Category()
        {
            this.article = new HashSet<Article>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Category description")]
        public string Description { get; set; }

        public string DateCreated { get; set; }

        public virtual ICollection<Article> Articles
        {
            get { return this.article; }
            set { this.article = value; }
        }

    }
}
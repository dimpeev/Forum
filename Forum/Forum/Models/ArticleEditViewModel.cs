using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class ArticleEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public List<Category> Categories { get; set; }

        public bool IsImportant { get; set; }

        [Display(Name = "Tags")]
        public string Tags { get; set; }
    }
}
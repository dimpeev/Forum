using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class EditAnswerViewModel
    {
        [Required]
        public string Content { get; set; }

        public int ArticleId { get; set; }
    }
}
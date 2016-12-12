﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Article
    {

        private ICollection<Tag> tags;

        public Article()
        {
            this.tags = new HashSet<Tag>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("CategoryName")]
        public int CategoryId { get; set; }

        public virtual Category CategoryName { get; set; }

        public int ParentPostId { get; set; }

        [Required]
        [ForeignKey("Author")]
        public string AuthorID { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string DateCreated { get; set; }

        public string LastEdited { get; set; }

        public bool IsImportant { get; set; }

        public virtual ICollection<Tag> Tags
        {
            get { return this.tags; }
            set { this.tags = value; }
        }

        public bool IsUserAuthor (string username)
        {
            return this.Author.UserName.Equals(username);
        }
    }
}
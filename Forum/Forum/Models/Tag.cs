using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Forum.Models
{

    public class Tag
    {
        private ICollection<Article> articles;

        public Tag()
        {
            this.articles = new HashSet<Article>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Article> Articles
        {
            get { return this.articles; }
            set { this.articles = value; }
        }

        public bool GetTagId(int id)
        {
            return this.Id == id;
        }
    }
}
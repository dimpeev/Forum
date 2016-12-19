using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Answer
    {

        [Key]
        public int Id { get; set; }

        [ForeignKey("Author")]
        public string AuthorID { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        public virtual Article Article { get; set; }

        public string Content { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? LastEdited { get; set; }

        public bool IsUserAuthor(string username)
        {
            return this.Author.UserName.Equals(username);
        }
    }
}
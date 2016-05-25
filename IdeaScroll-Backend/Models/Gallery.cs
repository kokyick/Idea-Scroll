using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IdeaScroll_Backend.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public string ImgUri { get; set; }
        public bool Visibility { get; set; }
        public string Name { get; set; }
        public int IdeaId { get; set; }
        [ForeignKey("IdeaId")]
        public virtual Idea Idea { get; set; }
    }
}
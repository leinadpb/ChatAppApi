using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAppApi.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public int Likes { get; set; }

    }
}

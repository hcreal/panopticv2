using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace panopticApp.Models
{
    public class FileMeta
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public Attachment Attachment { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Keywords { get; set; }
        public string IndexedCharacters { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public int Content_length { get; set; }
        public byte[] File { get; set; }
        public string longID { get; set; }
    }
}
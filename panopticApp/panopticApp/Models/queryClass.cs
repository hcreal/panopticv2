using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace panopticApp.Models
{
    public class queryClass
    {
        public string Query { get; set; } = null;
        public List<string> docList { get; set; }
        public List<string> documentList { get; set; }
        public List<string> authorList { get; set; }
        public List<string> dateList { get; set; }
        public List<string> typeList { get; set; }
        public List<string> contentList { get; set; }
        public List<string> nameList { get; set; }
        public List<string> pathList { get; set; }
        public bool authCheck { get; set; }
        public bool fileCheck { get; set; }

    }
}
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace panopticApp.Models.POCOs
{
    public class docs
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
        public int content_length { get; set; }
        public string longId { get; set; }

 
        public void setup() //taken from elastic.co documentation
        {
            
            var documentsIndex = "documents";
            var node = new Uri("http://10.115.2.126:9200");
            var connectionSettings = new ConnectionSettings(node).InferMappingFor<docs>(m => m.IndexName(documentsIndex));
            var client = new ElasticClient(connectionSettings);

            //builds an index
            var indexResponse = client.CreateIndex(documentsIndex, c => c
              .Settings(s => s
                .Analysis(a => a
                  .Analyzers(ad => ad
                    .Custom("windows_path_hierarchy_analyzer", ca => ca
                      .Tokenizer("windows_path_hierarchy_tokenizer")
                    )
                  )
                  .Tokenizers(t => t
                    .PathHierarchy("windows_path_hierarchy_tokenizer", ph => ph
                      .Delimiter('\\')
                    )
                  )
                )
              )
              //builds a mapping of type doc for attachments
              .Mappings(m => m
                .Map<docs>(mp => mp
                  .AutoMap()
                  .AllField(all => all
                    .Enabled(false)
                  )
                  .Properties(ps => ps
                    .Text(s => s
                      .Name(n => n.Path)
                      .Analyzer("windows_path_hierarchy_analyzer")
                    )
                    .Object<Attachment>(a => a
                      .Name(n => n.Attachment)
                      .AutoMap()
                    )
                  )
                )
              )
            );

            //Creates a Pipeline called attachments
            client.PutPipeline("attachments", p => p
              .Description("Document attachment pipeline")
              .Processors(pr => pr
                .Attachment<docs>(a => a
                  .Field(f => f.Content)
                  .TargetField(f => f.Attachment)
                )
                .Remove<docs>(r => r
                  .Field(f => f.Content)
                )
              )
            );
        }
        public List<string> upload(/*HttpPostedFileBase file, string filepath*/)
        {
            this.setup();
            //connect to server
            var node = new Uri("http://10.115.2.126:9200");
            var docIndex = "documents";
            var settings = new ConnectionSettings(node).InferMappingFor<docs>(m => m.IndexName(docIndex));
            var client = new ElasticClient(settings);



            //encodefile
            //var directory = Directory.GetCurrentDirectory();
            var directory = "C:\\Users\\swaldrep16\\Desktop\\Temp";
            List<string> filenames = new List<string>();
            int j = 1;

            var test = Directory.GetFiles(directory);
            foreach (string item in test)
            {
                var base64File = Convert.ToBase64String(File.ReadAllBytes(item));
                string path = item;
                filenames.Add(item);
                //MemoryStream target = new MemoryStream();
                //file.InputStream.CopyTo(target);
                // byte[] data = target.ToArray();

                //byte[] foo = File.ReadAllBytes(System.IO.Path.Combine(filepath, file.FileName));

                //var base64File = Convert.ToBase64String(data);
                client.Index(new docs
                {
                    Id = j,
                    //Path = @"\\share\documents\examples\example_one.docx",
                    Path = @path,
                    //Path = @filepath,
                    Content = base64File
                }, i => i.Pipeline("attachments"));
                j++;

            }
            return filenames;
        }

        public void uploadDirect(FileMeta document)
        {
            this.setup(); //calls setup to build index and pipeline if they don't already exist
            //connect to server
            var node = new Uri("http://10.115.2.126:9200");
            var docIndex = "documents";
            var settings = new ConnectionSettings(node).InferMappingFor<docs>(m => m.IndexName(docIndex));
            var client = new ElasticClient(settings);
            var base64File = Convert.ToBase64String(document.File);//convert file to type required by pipeline


            //upload file into Elasticsearch
            client.Index(new docs
            {
                Id = document.Id,
                Path = @document.Path,
                Content = base64File,
                Name = @document.Name,
                longId = @document.longID
            }, i => i.Pipeline("attachments"));

            return;
        }
    }

   
}


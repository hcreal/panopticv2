using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace panopticApp.Models.POCOs
{
    public class Sharepoint
    {


        public List<string> pull()
        {
            string serverUrl = "https://sharepoint.eil-server.cba.ua.edu"; //set this to the url of your sharepoint site
            string sharepointUrl = "https://sharepoint.eil-server.cba.ua.edu/Project/Capstone/parivedacapstoneS17"; //url of the specific Sharepoint site you want to access
            string sharepointList = "Documents"; //name of the folder in SharePoint you want to target with the pull
            List<string> files = new List<string>(); //list to hold all of the file names uploaded in this call
            docs elasticSearch = new docs(); //setting up the future connection to elasticsearch

            //build connection
            ClientContext context = new ClientContext(sharepointUrl);
            List list = context.Web.Lists.GetByTitle(sharepointList);

            // This creates a CamlQuery that has a RowLimit of 100, and also specifies Scope="RecursiveAll" 
            // so that it grabs all list items, regardless of the folder they are in. 
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            query.ViewXml = @"<View Scope='Recursive'><Query><Where><Eq><FieldRef Name='FSObjType' /><Value Type='Lookup'>0</Value></Eq></Where></Query></View>";
            ListItemCollection items = list.GetItems(query); 

            // Retrieve all items in the ListItemCollection from List.GetItems(Query). 
            context.Load(items);//grabbing folder metadata dump from sharepoint
            context.ExecuteQuery();

            foreach (ListItem item in items) //loop through each item in the folder
            {
                var value = item.FieldValues.Values.ToList();
                int id = ((int)(value[0]));
                string name = ((string)(value[20])); //grabbing the filename
                string docPath = ((string)(value[10])); //grabbing the document path

                var author = value[3] as FieldUserValue;
                string authorName = author.LookupValue.ToString();
                DateTime dateCreated = ((DateTime)(value[2]));

                var editor = value[5] as FieldUserValue;
                string editorName = author.LookupValue.ToString();
                DateTime dateModified = ((DateTime)(value[4]));

                var url = value[57] as FieldUrlValue;
                string webUrl = url.Url.ToString();
                

                string strurl = (serverUrl+docPath); //building the download url

                //FileStream fstream = null;
                string fileName = Path.GetFileName(strurl);
                if (!string.IsNullOrEmpty(fileName))
                {
                    byte[] data;
                    byte[] buffer = new byte[1024 * 64];
                    WebRequest request = WebRequest.Create(strurl);
                    request.UseDefaultCredentials = true;
                    request.PreAuthenticate = true; 
                    request.Credentials = CredentialCache.DefaultCredentials; //grabs the id of the local session
                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int count = 0;
                                do
                                {
                                    count = responseStream.Read(buffer, 0, buffer.Length);
                                    ms.Write(buffer, 0, count);
                                } while (count > 0);
                                data = ms.ToArray();
                                FileMeta tempFile = new FileMeta();
                                tempFile.Path = webUrl;
                                tempFile.File = data; //load metadata for elasticsearch
                                tempFile.Id = id; //load metadata for elasticsearch
                                tempFile.Name = name; //load metadata for elasticsearch
                                tempFile.Author = authorName;
                                tempFile.Date = dateCreated.ToString();
                                //tempFile.longID = item.Id;
                                elasticSearch.uploadDirect(tempFile); //upload the file in memory stream to elasticsearch as id of j
                            }
                        }

                        //used for saving files to the local machine
                        //string filePath = "C:\\Users\\swaldrep16\\Desktop\\Temp\\" + name;

                        //using (fstream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                        //{
                        //    fstream.Write(data, 0, data.Length);
                        //    fstream.Close();
                        //}

                    }
                }
                files.Add(name); //add file name to the list of files uploaded this call
                
            }
            return files;
        }
       
    }
}
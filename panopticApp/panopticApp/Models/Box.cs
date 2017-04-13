using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Models;
using Microsoft.SharePoint.Client;
using panopticApp.Models.POCOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace panopticApp.Models
{
    public class Box
    {
        //all api calls built from https://github.com/box/box-windows-sdk-v2

        public async Task<List<string>> Download()
        {
            const string BOXSiteURL = "https://panopticsearch.app.box.com/file/";
            const string clientid = "14rntjfz6tnlot2vsihcsb97wo07b97j"; //id of the box app
            const string client_secret = "TfZ1NU58xWt67PYNx1PYtmipLToHQYmj"; //secret of the box app taken from the box edit page
            string developer_Token = "HPz2ptyCQalnCMVt0l26DqkRYp71B4vM"; //expires every hour....
            //"https://panopticsearch.app.box.com/developers/services" go here for updating developer token
            //string fileId = "157807155826";
 
            var config = new BoxConfig(clientid, client_secret, new Uri("http://localhost")); //build the config
            var session = new OAuthSession(developer_Token, "", 3600, "bearer"); //get auth session built
            BoxClient client = new BoxClient(config, session); //connect
            BoxCollection<BoxItem> items = await client.FoldersManager.GetFolderItemsAsync("0", 500); //grab root folder and all it contains
            List<string> files = new List<string>(); //will contain names of all files loaded
            //BoxFile im = await client.FilesManager.GetInformationAsync(fileId);
            files = await searchFolder(client, items, BOXSiteURL);
            
            return files;
        }


        //recursive function that pulls all files from all folders
        public async Task<List<string>> searchFolder(BoxClient client, BoxCollection<BoxItem> items, string BoxURL)
        {
            List<string> files = new List<string>(); //stores the file names for future display
            List<string> tempFiles = new List<string>();
            int id = 0;  //temp placeholder for id
            foreach (BoxItem item in items.Entries) //loop through all items in a folder
            {
                
                if (item.Type == "folder") //if the item is a folder dive into it
                {
                    BoxCollection<BoxItem> folder = await client.FoldersManager.GetFolderItemsAsync(item.Id, 500); //get files from new folder
                    tempFiles = await searchFolder(client, folder, BoxURL); //recursive call
                    foreach (string i in tempFiles) //load file names into list
                    {
                        files.Add(i);
                    }
                    continue; //skip to next item
                }

                //converts the box field id from string to int
                Int32.TryParse(item.Id.Substring(item.Id.Length - 5, 4), out id); //id is actually a long so for our purposes i just grabbed some of it
                Stream stream = await client.FilesManager.DownloadStreamAsync(item.Id); //grab the actual file
                docs elasticSearch = new docs(); //build elasticSearch Connection

                byte[] data; 
                byte[] buffer = new byte[1024 * 64];
                using (MemoryStream ms = new MemoryStream()) //grab file stream and pass it into elasticsearch
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                        ms.Write(buffer, 0, count);
                    } while (count > 0);
                    data = ms.ToArray();
                    FileMeta tempFile = new FileMeta();

                    tempFile.Path = BoxURL + item.Id;
                    tempFile.File = data; //load metadata for elasticsearch
                    tempFile.Id = id; //load metadata for elasticsearch
                    tempFile.Name = item.Name; //load metadata for elasticsearch
                    tempFile.longID = item.Id;
                    elasticSearch.uploadDirect(tempFile); //upload the file in memory stream to elasticsearch as id of j
                }
                files.Add(item.Name); //append the name of the file to the list for future display on results page
            }
            return files; //return a list of all files
        }
    }
}
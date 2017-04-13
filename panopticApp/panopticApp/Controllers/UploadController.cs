using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nest;
using panopticApp.Models.POCOs;
using Elasticsearch.Net;
using Microsoft.Owin;
using System.IO;
using System.Net;
using System.Text;
using panopticApp.Models;
using Microsoft.SharePoint.Client;
using System.Threading.Tasks;

namespace panopticApp.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Index()
        {

            return View();
        }

        //Post
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, docs doc)
        {

            //foreach(string in box.Download)
            List<string> files = new List<string>();
            Sharepoint share = new Sharepoint();
            TempData["sharepointFiles"] = share.pull(); //downloading all of the files from sharepoint and storing names locally
            return RedirectToAction("BoxAsync");
        }

        public ActionResult Results(queryClass newList)
        {
            queryClass files = new queryClass();
            List<string> sharepointFiles = new List<string>();
            List<string> boxFiles = new List<string>();
            sharepointFiles = (List<string>)TempData["sharepointFiles"]; //retreiving from local storage
            boxFiles = (List<string>)TempData["boxFiles"]; //retreiving from local storage

            if (boxFiles.Count >= sharepointFiles.Count) //adding the shorter list into the longerlist 
            {


                foreach (string i in sharepointFiles) //string together all of the names into one list
                {
                    boxFiles.Add(i);
                }
                files.documentList = boxFiles;
            }
            else
            {
                foreach (string i in boxFiles) //string together all of the names into one list
                {
                    sharepointFiles.Add(i);
                }
                files.documentList = sharepointFiles;
            }


            return View(files); //updating the user that files are downloaded and displaying
        }

        public async Task<ActionResult> BoxAsync()
        {

            Models.Box box = new Models.Box();
            //String valid = null;
            TempData["boxFiles"] = await box.Download(); //downloading all of the files from sharepoint and storing names locally

            return RedirectToAction("Results");
        }
    }
}
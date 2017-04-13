using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Elasticsearch.Net;
using Nest;
using System.Web.Services.Description;
using panopticApp.Models;
using AjaxControlToolkit.Bundling;
using panopticApp.Models.POCOs;
using Microsoft.SharePoint.Client;
using System.Runtime.InteropServices;

namespace panopticApp.Controllers
{
    public class HomeController : Controller
    {
        private string docList;

        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Index(queryClass model)
        {
            //USING TEMPDATA TO PASS TO RESULTS VIEW
            TempData["Query"] = model.Query;
            TempData["check"] = model.fileCheck;
            return RedirectToAction("Results");


        }

        public ActionResult Results(queryClass model)
        {
            //SETTING INPUT TO SEARCH FROM TEMP DATA
            string input = TempData["Query"].ToString();
            //SETTING INPUT TO THE MODEL
            model.Query = input;
            //INTIALIZE CHECK BOX AND TURN IT IN
            string check = null;
            check = TempData["check"].ToString();

            //ERROR HANDLING FOR NULL REFRENCE
            if (input != null)
            {


                //IF COMMENTED OUT THE SERVER IS DOWN
                var node = new Uri("http://10.115.2.126:9200"); 
                var settings = new ConnectionSettings(node).DefaultIndex("documents");
                var client = new ElasticClient(settings);
                //UNCOMMENT IF THE SERVER IS DOWN
                //var settings = new ConnectionSettings().DefaultIndex("shakespeare");
                var config = new ConnectionConfiguration().PrettyJson();

                //CHECKS IF SEARCHING BY AUTHOR
                if (check == "True")
                {
                    //THIS IS THE USER INPUTED LINQ QUERY CALLING DOCS WHICH IS PART OF OUR PIPELINE
                    var test = client.Search<docs>(s => s.Query(q => q.Match(m => m.Field(f => f.Attachment.Author).Query(input)))); //CLIENT IS TO CONNECT TO ELASTICSEARCH. SEARCH IS A METHOD TO CREATE YOUR QUERY. MATCH IS TO FIND A SIMILAR AND FIELD IS TO FIND SOMETHING THAT WAS UP. FIELD.ATTACHMENT IS IS THE PIPELINE THAT WE CALL SO WE CAN GO THROUGH  

                    //INTIALIZING ALL THE LISTS TO DISPLAY THE RESULTS
                    model.docList = new List<string>();
                    model.contentList = new List<string>();
                    model.authorList = new List<string>();
                    model.dateList = new List<string>();
                    model.typeList = new List<string>();
                    model.nameList = new List<string>();
                    model.pathList = new List<string>();
                    //VIEWBAG CREATION FOR THE HREF USED TO DISPLAY THE URL
                    ViewBag.Path = "";

                    //MAKES SURE THAT THE RESULTS ARE NOT NULL
                    if (test.Fields.Count != 0)
                    {
                        //FOREACH THAT ADDS EACH ENTRY TO THE RESPECTED LIST
                        foreach (var doc in test.Documents)
                        {
                            model.docList.Add(doc.Attachment.ToString());               //DOCUMENT LIST
                            model.authorList.Add(doc.Attachment.Author.ToString());     //AUTHOR LIST
                            model.dateList.Add(doc.Attachment.Date.ToString());         // DATE CREATION LIST
                            model.typeList.Add(doc.Attachment.ContentType.ToString());  // FILE TYPE LIST
                            model.contentList.Add(doc.Attachment.Content.ToString());   // THE CONTENT STRING LIST
                            model.pathList.Add(doc.Path.ToString());                    // LIST OF ALL THE FILE PATHS
                            var count = model.pathList.Count;                           // USED AS A COUNT FOR TOTAL DOCUMENTS IN THE TEST LINQ QUERY RESULTS
                            var path = model.pathList.ToList();                         // A LIST OF ALL THE PATHS TO ADD TO THE VIEWBAG 
                            ViewBag.Paths = path;                                       // SETTING THE LIST OFPATH EQUAL TO THE VIEWBAG
                        }
                        return View(model); 
                    }

                    return View(model);
                }
                else // SEARCH BY KEYWORDS IN THE CONTENT
                {
                    var test = client.Search<docs>(s => s.Query(q => q.Match(m => m.Field(f => f.Attachment.Content).Query(input))));

                    //INTIALIZING ALL THE LISTS TO DISPLAY THE RESULTS
                    model.docList = new List<string>();
                    model.contentList = new List<string>();
                    model.authorList = new List<string>();
                    model.dateList = new List<string>();
                    model.typeList = new List<string>();
                    model.nameList = new List<string>();
                    model.pathList = new List<string>();
                    ViewBag.Path = "";

                    //MAKES SURE THAT THE RESULTS ARE NOT NULL
                    if (test.Fields.Count != 0)
                    {
                        //FOREACH THAT ADDS EACH ENTRY TO THE RESPECTED LIST
                        foreach (var doc in test.Documents)
                        {
                            if (doc.Attachment.Author == null)
                            {
                                model.docList.Add(doc.Attachment.ToString());                  //DOCUMENT LIST
                                model.dateList.Add(doc.Attachment.Date.ToString());            //AUTHOR LIST
                                model.typeList.Add(doc.Attachment.ContentType.ToString());     //DATE LIST
                                model.contentList.Add(doc.Attachment.Content.ToString());      //FILE TYPE LIST 
                                model.pathList.Add(doc.Path.ToString());                       //LIST OF FILE PATHS 
                                doc.Attachment.Author = "None";                                //SETS AUTHORTO NONE IF NULL

                            }
                            else //IF THE AUTHOR IS NOT NULL 
                            {
                                model.docList.Add(doc.Attachment.ToString());                  //DOC LIST
                                model.authorList.Add(doc.Attachment.Author.ToString());        //AUTHOR LIST 
                                model.dateList.Add(doc.Attachment.Date.ToString());            //DATE LIST 
                                model.typeList.Add(doc.Attachment.ContentType.ToString());     //TYPE LIST 
                                model.contentList.Add(doc.Attachment.Content.ToString());      //CONTENT LIST 
                                model.pathList.Add(doc.Path.ToString());                       //PATH LIST 
                            }
                        }
                        return View(model);
                    }

                    return View(model);
                }

            }
            else

                return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
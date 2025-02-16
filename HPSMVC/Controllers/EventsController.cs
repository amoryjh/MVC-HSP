﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HPSMVC.DAL;
using HPSMVC.Models;
using System.IO;
//[Everyone] The evenets controller is used to manage the events on the HPS site 
namespace HPSMVC.Controllers
{
    public class EventsController : Controller
    {
        private HPSMVCEntities db = new HPSMVCEntities();

        //Get Index 
        public ActionResult Index()
        {

            return View(db.Events.ToList().OrderBy( s=> s.Date));
            
        }

        //Get BoardCal
        [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        public ActionResult BoardCal()
        {
            return View(db.Events.ToList().OrderBy(s => s.Date));
        }

        //Get Admin Index 
        [Authorize(Roles = "Admin")]
        public ActionResult Admin(string sortOrder, string searchString)
        {
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date" : "";
            ViewBag.TitleSortParm = sortOrder == "Title" ? "title" : "Title";
            ViewBag.BySortParm = sortOrder == "By" ? "by" : "By";
            ViewBag.ViewerSortParm = sortOrder == "Viewer" ? "viewer" : "Viewer";
            var events = from s in db.Events
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                events = events.Where(s => s.Title.Contains(searchString)
                                       || s.Viewer.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "title":
                    events = events.OrderBy(s => s.Title);
                    break;
                case "date":
                    events = events.OrderByDescending(s => s.Date);
                    break;
                case "viewer":
                    events = events.OrderBy(s => s.Viewer);
                    break;
                default:
                    events = events.OrderBy(s => s.Date);
                    break;
            }

            return View(events.ToList());
        }

        
        // GET: Events/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        
        // POST: Events/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Title,Content,Time,Date,Viewer,LinkText,Link,fileName,fileType,fileContent")] Event @event)
        {
            if (ModelState.IsValid)
            {
                foreach (string fName in Request.Files)
              {
                HttpPostedFileBase f = Request.Files[fName];
                string mimeType = f.ContentType;
                int fileLength = f.ContentLength;
                if (!(mimeType == "" || fileLength == 0))
                {
                  string fileName = Path.GetFileName(f.FileName);
                  Stream fileStream = Request.Files[fName].InputStream;
                  byte[] fileData = new Byte[fileLength];
                  fileStream.Read(fileData, 0, fileLength);

                  @event.fileContent = fileData;
                  @event.fileType = mimeType;
                  @event.fileName = fileName;

                
               }
            }
            try
            {
                db.Events.Add(@event);
                db.SaveChanges();
                TempData["ValidationMessage"] = @event.Title += "   Event Successfully Added!";
                return RedirectToAction("Admin");
            }
            catch
            {
                TempData["ValidationMessage"] = @event.Title += "   Error: Event Not Successfully Added!";
            }
                
        }

            return View(@event);
                
        }

        public FileContentResult Download(int id)
        {
            var theFile = db.Events.Where(f => f.ID == id).SingleOrDefault();
            return File(theFile.fileContent, theFile.fileType, theFile.fileName);
        }

        // GET: Events/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id, string[] chkRemoveFile)//[Bind(Include = "ID,Name,Dated,imageContent,imageMimeType,imageFileName,MovementID,ArtistID")] Painting painting, string chkRemoveImage)
        {
            var FilesToUpdate = db.Events
            .Where(f => f.ID == id)
            .Single();
            if (TryUpdateModel(FilesToUpdate, "",
                new string[] { "ID", "Title", "Content", "Time", "Date", "Viewer", "LinkText", "Link", "fileName", "fileType", "fileContent" }))
            {
                if (chkRemoveFile != null)//Remove the File
                {
                    FilesToUpdate.fileContent = null;
                    FilesToUpdate.fileType = null;
                    FilesToUpdate.fileName = null;
                }
                foreach (string fName in Request.Files)
                {
                    HttpPostedFileBase f = Request.Files[fName];
                    string mimeType = f.ContentType;
                    int fileLength = f.ContentLength;
                    if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        string fileName = Path.GetFileName(f.FileName);
                        Stream fileStream = Request.Files[fName].InputStream;
                        byte[] fileData = new byte[fileLength];
                        fileStream.Read(fileData, 0, fileLength);

                        FilesToUpdate.fileContent = fileData;
                        FilesToUpdate.fileType = mimeType;
                        FilesToUpdate.fileName = fileName;

                    }
                }
                try
                {
                    db.Entry(FilesToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["ValidationMessage"] = "Successfully Edited!";
                    return RedirectToAction("Admin");
                }
                catch
                {
                    TempData["ValidationMessage"] = "Error: Not Successfully Edited!";
                }

            }
            return View(FilesToUpdate);
        }

        // GET: Events/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event @event = db.Events.Find(id);
            try
            {
                db.Events.Remove(@event);
                db.SaveChanges();
                TempData["ValidationMessage"] = @event.Title += "   Event Successfully Deleted!";
            }
            catch
            {
                TempData["ValidationMessage"] = @event.Title += "   Error: Event Not Successfully Deleted!";
            }
            
            return RedirectToAction("Admin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

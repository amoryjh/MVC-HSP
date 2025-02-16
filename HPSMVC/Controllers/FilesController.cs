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
//[Everyone] The Files controller is used to manage all files on the HPS website 
namespace HPSMVC.Controllers
{
    public class FilesController : Controller
    {
        private HPSMVCEntities db = new HPSMVCEntities();

        // GET: Files
        [Authorize(Roles = "Admin,BoardDirector,Client,FamilyAssoc")]
        public ActionResult Index(string sortOrder, string searchString)
        {           

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date" : "Date";
            ViewBag.ViewerSortParm = sortOrder == "Viewer" ? "viewer" : "Viewer";
            ViewBag.CategorySortParm = sortOrder == "Category" ? "category" : "Category";
            var Files = from s in db.Files
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                Files = Files.Where(s => s.fileName.Contains(searchString)
                                      || s.Viewer.Contains(searchString)
                                      || s.Category.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name":
                    Files = Files.OrderByDescending(s => s.fileName);
                    break;
                case "date":
                    Files = Files.OrderBy(s => s.Date);
                    break;
                case "category":
                    Files = Files.OrderBy(s => s.Category);
                    break;
                case "viewer":
                    Files = Files.OrderBy(s => s.Viewer);
                    break;
                default:
                    Files = Files.OrderBy(s => s.fileName);
                    break;
            }
            return View(Files.ToList());            
        }

        // GET: Files/Create
        [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Files/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        public ActionResult Create([Bind(Include = "ID,fileName,fileType,fileContent,Date,Category,Viewer")] HPSMVC.Models.File file)
        
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

                    file.fileContent = fileData;
                    file.fileType = mimeType;
                    file.fileName = fileName;

                    
                  
                }
              }
              try
              {
                  db.Files.Add(file);
                  db.SaveChanges();
                  TempData["ValidationMessage"] = file.fileName += "   Successfully Added!";
                  return RedirectToAction("Index");
              }
              catch
                {
                    TempData["ValidationMessage"] = file.fileName += "   Error: Not Successfully Added!";
                }
            }

            return View(file);
        }
        public FileContentResult Download(int id)
        {
            var theFile = db.Files.Where(f => f.ID == id).SingleOrDefault();
            return File(theFile.fileContent, theFile.fileType, theFile.fileName);
        }

        // GET: Files/Edit/5
         [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HPSMVC.Models.File file = db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            return View(file);
        }

        // POST: Files/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        //public ActionResult Edit([Bind(Include = "ID,fileName,fileType,fileContent,Date,Category,Viewer")] HPSMVC.Models.File file)
        public ActionResult Edit(int? id, string[] chkRemoveFile)//[Bind(Include = "ID,Name,Dated,imageContent,imageMimeType,imageFileName,MovementID,ArtistID")] Painting painting, string chkRemoveImage)
        {
            var FilesToUpdate = db.Files
            .Where(f => f.ID == id)
            .Single();
            if (TryUpdateModel(FilesToUpdate, "",
                new string[] { "ID","fileName","fileType","fileContent","Date","Category","Viewer" }))
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
                    TempData["ValidationMessage"] = FilesToUpdate.fileName += "   Successfully Edited!";
                    return RedirectToAction("Index");
                }
                catch
                {
                    TempData["ValidationMessage"] = FilesToUpdate.fileName += "   Error: Not Successfully Edited!";
                }

            }
            return View(FilesToUpdate);
        }

        // GET: Files/Delete/5
         [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HPSMVC.Models.File file = db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            return View(file);
        }

        // POST: Files/Delete/5
         [Authorize(Roles = "Admin,BoardDirector,FamilyAssoc")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
          HPSMVC.Models.File file = db.Files.Find(id);
            try
            {
                db.Files.Remove(file);
                db.SaveChanges();
                TempData["ValidationMessage"] = file.fileName += "   Successfully Deleted!";
            }
            catch
            {
                TempData["ValidationMessage"] = file.fileName += "   Error: Not Successfully Deleted!";
            }

            return RedirectToAction("Index");
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

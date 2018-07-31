using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using JobApplicationSpam.Models;
using System.Reflection;
using System;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace JobApplicationSpam.Controllers
{
    public class HomeController : Controller
    {
        JobApplicationSpamDbContext dbContext { get; }
        public HomeController(JobApplicationSpamDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ViewResult Index() =>
            View(
                new Dictionary<string, object>
                { ["Placeholder"] = "Placeholder" });

        public ViewResult J()
        {
            var appUser = GetAppUser();
            var document = GetDocument(appUser, "documentName");
            return
                View(
                    "JobApplicationSpam",
                    new JobApplicationSpamState
                    {
                        Document = document,
                        UserValues = (UserValues)GetDbObject("UserValues", appUser, document),
                        Employer = (Employer)GetDbObject("Employer", appUser, document),
                        DocumentEmail = (DocumentEmail)GetDbObject("DocumentEmail", appUser, document),
                        CustomVariables = (IEnumerable<CustomVariable>)GetDbObject("CustomVariables", appUser, document),
                        DocumentFiles = (IEnumerable<DocumentFile>)GetDbObject("DocumentFiles", appUser, document)
                    }
                );
        }

        [HttpPost]
        public void Save()
        {
            if (!dbContext.AppUsers.Any())
            {
                dbContext.Add(new AppUser { Id = "538b98c6-e34a-4fb4-8ee5-0731d8720569" });
                dbContext.SaveChanges();
            }
            var appUser = GetAppUser();
            var document = GetDocument(appUser, "documentName");
            foreach (var kv in Request.Form)
            {
                var split = kv.Key.Split(new char[] { '_' });
                var table = split[0];
                var column = split[1];
                object index = split.Length > 2 ? (object)Int32.Parse(split[2]) : null;
                /*MethodInfo method =
                    typeof(JobApplicationSpamDbContext)
                        .GetMethod(
                            "Find",
                            new Type[] { (new object[] { }).GetType() });
                MethodInfo generic =
                    method.MakeGenericMethod(Type.GetType($"JobApplicationSpam.Models.{table}"));
                    */
                var dbObject = GetDbObject(table, appUser, document, index);
                dbObject.GetType().GetProperty(column).SetValue(dbObject, kv.Value.ToString());
                dbContext.Update(dbObject);
            }
            dbContext.SaveChanges();
        }

        [HttpPost]
        public void ApplyNow()
        {
            var appUser = GetAppUser();
            var document = GetDocument(appUser, "documentName");
            var documentEmail = (DocumentEmail)GetDbObject("DocumentEmail", appUser, document);
            var employer = (Employer)GetDbObject("Employer", appUser, document);
            var userValues = (UserValues)GetDbObject("UserValues", appUser, document);
            var customVariables = (IEnumerable<CustomVariable>)GetDbObject("CustomVariables", appUser, document);
            var documentFiles = (IEnumerable<DocumentFile>)GetDbObject("DocumentFiles", appUser, document);
            var sentApplication =
                new SentApplication
                {
                    Document = document,
                    Employer = employer,
                    UserValues = userValues,
                    SentDate = DateTime.Now
                };
            dbContext.Add(sentApplication);

            var documentCopy = new Document { JobName = document.JobName, AppUser = appUser };
            dbContext.Add(documentCopy);
            dbContext.Add(new Employer() { AppUser = appUser });
            dbContext.Add(new UserValues(userValues) { AppUser = appUser });
            dbContext.Add(new DocumentEmail(documentEmail) { Document = documentCopy });

            foreach (var customVariable in customVariables)
            {
                dbContext.Add(new CustomVariable(customVariable) { Document = documentCopy });
            }
            foreach (var documentFile in documentFiles)
            {
                dbContext.Add(new DocumentFile(documentFile) { Document = documentCopy });
            }
            dbContext.SaveChanges();
        }

        private AppUser GetAppUser()
        {
            var appUser = dbContext.AppUsers.SingleOrDefault(x => x.Id == "538b98c6-e34a-4fb4-8ee5-0731d8720569");
            if(appUser == null)
            {
                appUser = new AppUser { Id = "538b98c6-e34a-4fb4-8ee5-0731d8720569" };
                dbContext.Add(appUser);
                dbContext.SaveChanges();
            }
            return appUser;
        }

        [HttpPost]
        public string ConvertToPdf()
        {
            var path = Request.Form["path"];
            var fileName = Path.GetFileName(path);
            var tmpPaths = new TmpPaths(path, GetAppUser().Id);
            var extension = Path.GetExtension(path);
            switch(extension)
            {
                case ".pdf":
                    return path;
                case ".odt":
                    ZipFile.ExtractToDirectory(path, tmpPaths.UnzipTo);
                    ReplaceInDirectory(tmpPaths.UnzipTo, new Dictionary<string, string> { ["$meinVorname"] = "Rene" });
                    ZipFile.CreateFromDirectory(tmpPaths.UnzipTo, Path.Combine(tmpPaths.ZipTo, fileName));
                    return tmpPaths.ZipTo;
                default:
                    throw new Exception($"Unknown file extension: {extension}");
            }
        }

        private void ReplaceInDirectory(string path, Dictionary<string, string> dict)
        {
            foreach(var currentDir in Directory.EnumerateDirectories(path))
            {
                ReplaceInDirectory(Path.Combine(path, currentDir), dict);
            }
            foreach(var currentFile in Directory.EnumerateFiles(path))
            {
                var fullFilePath = Path.Combine(path, currentFile);
                if(Path.GetExtension(fullFilePath).ToLower() == ".xml")
                {
                    var content = System.IO.File.ReadAllText(fullFilePath);
                    foreach(var kv in dict)
                    {
                        content = content.Replace(kv.Key, kv.Value);
                    }
                    System.IO.File.WriteAllText(fullFilePath, content);
                }
            }
        }

        [HttpPost]
        public void UploadFile()
        {
            var appUser = dbContext.AppUsers.Single(x => x.Id == "538b98c6-e34a-4fb4-8ee5-0731d8720569");
            var document = GetDocument(appUser, "documentName");
            var files = Request.Form.Files;
            Directory.CreateDirectory("c:/users/rene/uploadtest/");
            foreach(var file in Request.Form.Files)
            {
                var savePath = $"c:/users/rene/uploadtest/{file.FileName}";
                using (var fileStream = System.IO.File.Create(savePath))
                {
                    file.OpenReadStream().CopyTo(fileStream);
                }
                var documentFile = new DocumentFile { Document = document, Index = -1, Name = file.FileName, Path = savePath, SizeInBytes = -1 };
                dbContext.Add(documentFile);
            }
            dbContext.SaveChanges();
            RedirectToAction("Index");
        }

        private Document GetDocument(AppUser appUser, string documentName)
        {
            var document =
                dbContext.Documents
                    .Where(x => x.AppUser.Id == appUser.Id)
                    .OrderByDescending(x => x.Id).FirstOrDefault();
            if (document == null)
            {
                document = new Document { AppUser = appUser };
                dbContext.Add(document);
                dbContext.SaveChanges();
            }
            return document;
        }

        private object GetDbObject(string table, AppUser appUser, Document document, object index = null)
        {
            switch(table)
            {
                case "UserValues":
                    {
                        var userValues =
                            dbContext.UserValues
                                .Where(x => x.AppUser.Id == appUser.Id)
                                .OrderByDescending(x => x.Id).FirstOrDefault();
                        if (userValues == null)
                        {
                            userValues = new UserValues { AppUser = appUser };
                            dbContext.Add(userValues);
                            dbContext.SaveChanges();
                        }
                        return userValues;
                    }
                case "DocumentEmail":
                    {
                        var documentEmail =
                            dbContext.DocumentEmail
                                .Where(x => x.Document.Id == document.Id)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();
                        if(documentEmail == null)
                        {
                            documentEmail = new DocumentEmail { Document = document };
                            dbContext.Add(documentEmail);
                            dbContext.SaveChanges();
                        }
                        return documentEmail;
                    }
                case "Employer":
                    {
                        var employer =
                            dbContext.Employers
                                .Where(x => x.AppUser.Id == appUser.Id)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();
                        if(employer == null)
                        {
                            employer = new Employer { AppUser = appUser };
                            dbContext.Add(employer);
                            dbContext.SaveChanges();
                        }
                        return employer;
                    }
                case "CustomVariables":
                    {
                        if (index != null)
                        {
                            var customVariable =
                                dbContext.CustomVariables
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id)
                                    .Skip((int)index - 1)
                                    .Take(1)
                                    .FirstOrDefault();
                            if (customVariable == null)
                            {
                                customVariable = new CustomVariable { Document = document };
                                dbContext.Add(customVariable);
                                dbContext.SaveChanges();
                            }
                            return customVariable;
                        }
                        else
                        {
                            var customVariables =
                                dbContext.CustomVariables
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id);
                            return customVariables;
                        }
                    }
                case "DocumentFiles":
                    {
                        if (index != null)
                        {
                            var documentFile =
                                dbContext.DocumentFiles
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id)
                                    .Skip((int)index - 1)
                                    .Take(1)
                                    .FirstOrDefault();
                            if (documentFile == null)
                            {
                                documentFile = new DocumentFile { Document = document };
                                dbContext.Add(documentFile);
                                dbContext.SaveChanges();
                            }
                            return documentFile;
                        }
                        else
                        {
                            var documentFiles =
                                dbContext.DocumentFiles
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id);
                            return documentFiles;
                        }
                    }

                default:
                    throw new Exception($"Table not found: {table}");
            }
        }

    }
}

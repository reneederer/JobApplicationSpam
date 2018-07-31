using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using JobApplicationSpam.Models;
using System.Reflection;
using System;
using System.Linq;

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
            var appUser =
                dbContext.AppUsers.Single(x => x.Id == "538b98c6-e34a-4fb4-8ee5-0731d8720569");
            var document = (Document)GetDbObject("Document", appUser);
            return
                View(
                    "JobApplicationSpam",
                    new JobApplicationSpamState
                    {
                        Document = document,
                        UserValues = (UserValues)GetDbObject("UserValues", appUser),
                        Employer = (Employer)GetDbObject("Employer", appUser),
                        DocumentEmail = (DocumentEmail)GetDbObject("DocumentEmail", appUser),
                        CustomVariables = (IEnumerable<CustomVariable>)GetDbObject("CustomVariables", appUser),
                        DocumentFiles = (IEnumerable<DocumentFile>)GetDbObject("DocumentFiles", appUser),
                    }
                );
        }

        [HttpPost]
        public string Save()
        {
            if (!dbContext.AppUsers.Any())
            {
                dbContext.Add(new AppUser { Id = "538b98c6-e34a-4fb4-8ee5-0731d8720569" });
                dbContext.SaveChanges();
            }
            var appUser = dbContext.AppUsers.Single(x => x.Id == "538b98c6-e34a-4fb4-8ee5-0731d8720569");
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
                var dbObject = GetDbObject(table, appUser, index);
                dbObject.GetType().GetProperty(column).SetValue(dbObject, kv.Value.ToString());
                dbContext.Update(dbObject);
            }
            dbContext.SaveChanges();
            return "";
        }

        private object GetDbObject(string table, AppUser appUser, object index = null)
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
                case "Document":
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
                case "DocumentEmail":
                    {
                        var document = (Document)GetDbObject("Document", appUser);
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
                        var document = (Document)GetDbObject("Document", appUser);
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
                        var document = (Document)GetDbObject("Document", appUser);
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

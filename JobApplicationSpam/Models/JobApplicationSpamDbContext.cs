using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JobApplicationSpam.Models
{
    public class JobApplicationSpamDbContext : IdentityDbContext<AppUser>
    {
        public JobApplicationSpamDbContext(DbContextOptions<JobApplicationSpamDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<DocumentEmail> DocumentEmail { get; set; }
        public virtual DbSet<CustomVariable> CustomVariables { get; set; }
        public virtual DbSet<DocumentFile> DocumentFiles { get; set; }
        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<SentApplication> SentApplications { get; set; }
        public virtual DbSet<UserValues> UserValues { get; set; }
        public virtual DbSet<Employer> Employers { get; set; }


        public JobApplicationSpamState GetJobApplicationSpamState(AppUser appUser)
        {
            var document = GetDocument(appUser, "someDocumentName");
            return
                new JobApplicationSpamState
                {
                    Document = document,
                    UserValues = (UserValues)GetDbObject("UserValues", appUser, document),
                    Employer = (Employer)GetDbObject("Employer", appUser, document),
                    DocumentEmail = (DocumentEmail)GetDbObject("DocumentEmail", appUser, document),
                    CustomVariables = (IEnumerable<CustomVariable>)GetDbObject("CustomVariables", appUser, document),
                    DocumentFiles = (IEnumerable<DocumentFile>)GetDbObject("DocumentFiles", appUser, document),
                    User = appUser
                };
        }

        public Document GetDocument(AppUser appUser, string documentName)
        {
            var document =
                Documents
                    .Where(x => x.AppUser.Id == appUser.Id)
                    .OrderByDescending(x => x.Id).FirstOrDefault();
            if (document == null)
            {
                document = new Document { AppUser = appUser };
                Add(document);
                var customVariables =
                    new List<CustomVariable>()
                    {
                        new CustomVariable
                        {
                            Text = "$chefAnrede =\n\tmatch $chefGeschlecht with\n\t| \"m\" -> \"Herr\"\n\t| \"f\" -> \"Frau\"\n\t| \"u\" -> \"\"",
                            Document = document
                        },
                        new CustomVariable
                        {
                            Text = "$anredeZeile =\n\tmatch $chefGeschlecht with\n\t| \"m\" -> \"Sehr geehrter Herr $chefTitel $chefNachname,\n\t| \"f\" -> \"Sehr geehrte Frau $chefTitel $chefNachname,\"\n\t| \"u\" -> \"Sehr geehrte Damen und Herren,\"",
                            Document = document
                        },
                        new CustomVariable
                        {
                            Text = "$chefAnredeBriefkopf =\n\tmatch $chefGeschlecht with\n\t| \"m\" -> \"Herrn\"\n\t| \"f\" -> \"Frau\"\n\t| \"u\" -> \"\"",
                            Document = document
                        },
                        new CustomVariable
                        {
                            Text = "$datumHeute = $tagHeute + \".\" + $monatHeute + \".\" + $jahrHeute",
                            Document = document
                        }
                    };
                AddRange(customVariables);
                SaveChanges();
            }
            return document;
        }

        public object GetDbObject(string table, AppUser appUser, Document document, int index = -1)
        {
            switch(table)
            {
                case "UserValues":
                    {
                        var userValues =
                            UserValues
                                .Where(x => x.AppUser.Id == appUser.Id)
                                .OrderByDescending(x => x.Id).FirstOrDefault();
                        if (userValues == null)
                        {
                            userValues = new UserValues { AppUser = appUser };
                            Add(userValues);
                            SaveChanges();
                        }
                        return userValues;
                    }
                case "DocumentEmail":
                    {
                        var documentEmail =
                            DocumentEmail
                                .Where(x => x.Document.Id == document.Id)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();
                        if(documentEmail == null)
                        {
                            documentEmail = new DocumentEmail { Document = document };
                            Add(documentEmail);
                            SaveChanges();
                        }
                        return documentEmail;
                    }
                case "Employer":
                    {
                        var employer =
                            Employers
                                .Where(x => x.AppUser.Id == appUser.Id)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();
                        if(employer == null)
                        {
                            employer = new Employer { AppUser = appUser };
                            Add(employer);
                            SaveChanges();
                        }
                        return employer;
                    }
                case "CustomVariables":
                    {
                        if (index >= 1)
                        {
                            var customVariable =
                                CustomVariables
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id)
                                    .Skip((int)index - 1)
                                    .Take(1)
                                    .FirstOrDefault();
                            if (customVariable == null)
                            {
                                customVariable = new CustomVariable { Document = document };
                                Add(customVariable);
                                SaveChanges();
                            }
                            return customVariable;
                        }
                        else
                        {
                            var customVariables =
                                CustomVariables
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id);
                            return customVariables;
                        }
                    }
                case "DocumentFiles":
                    {
                        if (index >= 1)
                        {
                            var documentFile =
                                DocumentFiles
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id)
                                    .Skip((int)index - 1)
                                    .Take(1)
                                    .FirstOrDefault();
                            if (documentFile == null)
                            {
                                documentFile = new DocumentFile { Document = document };
                                Add(documentFile);
                                SaveChanges();
                            }
                            return documentFile;
                        }
                        else
                        {
                            var documentFiles =
                                DocumentFiles
                                    .Where(x => x.Document.Id == document.Id)
                                    .OrderBy(x => x.Id);
                            return documentFiles.AsEnumerable<DocumentFile>();
                        }
                    }
                case "Document":
                    return document;

                default:
                    throw new Exception($"Table not found: {table}");
            }
        }

        public void UpdateDbObject(AppUser appUser, string documentName, string table, string column, int index, object value)
        {
            var dbObject = GetDbObject(table, appUser, GetDocument(appUser, documentName), index);
            dbObject.GetType().GetProperty(column).SetValue(dbObject, value);
            Update(dbObject);
        }
    }
}

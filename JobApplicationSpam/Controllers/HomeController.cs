using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using JobApplicationSpam.Models;
using System.Reflection;
using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using PdfSharp.Pdf;
using PdfSharp;
using PdfSharp.Pdf.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JobApplicationSpam.CustomVariableParser;

namespace JobApplicationSpam.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static readonly log4net.ILog log =
             log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        JobApplicationSpamDbContext dbContext { get; }
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;

        public HomeController(JobApplicationSpamDbContext dbContext, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var appUser1 = await userManager.FindByEmailAsync("q@q.de");
            if (appUser1 != null)
            {
                var r2 = await signInManager.PasswordSignInAsync(appUser1, "1234", false, false);
                if(r2.Succeeded)
                {
                }
            }
            return RedirectToAction("J", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> J()
        {
            var appUser = await GetOrCreateAppUser();
            var document = GetDocument(appUser, "documentName");
            return View("JobApplicationSpam", GetJobApplicationSpamState(appUser));
        }

        [HttpPost]
        public async Task Save()
        {
            var appUser = await GetOrCreateAppUser();
            var document = GetDocument(appUser, "documentName");
            foreach (var kv in Request.Form)
            {

                var split = kv.Key.Split(new char[] { '_' });
                var table = split[0];
                var column = split[1];
                object index = split.Length > 2 ? (object)Int32.Parse(split[2]) : null;
                var dbObject = GetDbObject(table, appUser, document, index);
                dbObject.GetType().GetProperty(column).SetValue(dbObject, kv.Value.ToString());
                dbContext.Update(dbObject);
            }
            dbContext.SaveChanges();
        }

        [HttpPost]
        public async Task PerformFileActions()
        {
            var appUser = await GetOrCreateAppUser();
            var document = GetDocument(appUser, "documentName");
            var documentFiles = ((IEnumerable<DocumentFile>)GetDbObject("DocumentFiles", appUser, document)).OrderBy(x => x.Id).AsEnumerable();
            Action<IEnumerable<DocumentFile>, int, int> swap =
                (files, index1, index2) =>
                {
                    int count = documentFiles.Count();
                    if (index1 < 0 || index2 < 0 || index1 >= count || index2 >= count)
                    {
                        return;
                    }
                    var file1 = files.ElementAt(index1);
                    var file2 = files.ElementAt(index2);
                    var file1Copy = new DocumentFile(file1);

                    file1.Index = file2.Index;
                    file1.Name = file2.Name;
                    file1.Path = file2.Path;
                    file1.SizeInBytes = file2.SizeInBytes;

                    file2.Index = file1Copy.Index;
                    file2.Name = file1Copy.Name;
                    file2.Path = file1Copy.Path;
                    file2.SizeInBytes = file1Copy.SizeInBytes;
                };
            foreach(var kv in Request.Form)
            {
                var index = Int32.Parse(kv.Value);
                if(kv.Key.StartsWith("MoveUp"))
                {
                    swap(documentFiles, index, index - 1);
                }
                else if(kv.Key.StartsWith("MoveDown"))
                {
                    swap(documentFiles, index, index + 1);
                }
                else if(kv.Key.StartsWith("Delete"))
                {
                    try
                    {
                        var rowToRemove = dbContext.Find<DocumentFile>(documentFiles.ElementAt(index).Id);
                        dbContext.Remove(rowToRemove);
                        documentFiles = documentFiles.Where(x => x.Id != rowToRemove.Id);
                    }
                    catch(Exception e)
                    {
                    }
                }
            }
            dbContext.UpdateRange(documentFiles);
            dbContext.SaveChanges();
        }

        [HttpPost]
        public async Task<string> ApplyNow()
        {
            try
            {
                var appUser = await GetOrCreateAppUser();
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

                var pdfFilePaths = new List<string>();
                var dict = getVariableDict(employer, userValues, documentEmail, customVariables, document.JobName);
                foreach (var documentFile in documentFiles)
                {
                    pdfFilePaths.Add(ConvertToPdf(documentFile.Path, dict));
                }

                var mergedPath = Path.Combine(new TmpPath().Path, "mypdf.pdf");
                using (var outputDocument = new PdfDocument())
                {
                    foreach (var pdfFilePath in pdfFilePaths)
                    {
                        var inputDocument = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
                        for (int i = 0; i < inputDocument.Pages.Count; ++i)
                        {
                            outputDocument.AddPage(inputDocument.Pages[i]);
                        }
                    }
                    if (outputDocument.PageCount >= 1)
                    {
                        outputDocument.Save(mergedPath);
                    }
                }

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
                var attachments = new List<EmailAttachment>();
                if (pdfFilePaths.Count >= 1)
                {
                    attachments.Add(new EmailAttachment { Path = mergedPath, Name = ReplaceInString(Path.GetFileName(mergedPath), dict) });
                }
                HelperFunctions.SendEmail(
                    new EmailData
                    {
                        Attachments = attachments,
                        Body = ReplaceInString(documentEmail.Body, dict),
                        Subject = ReplaceInString(documentEmail.Subject, dict),
                        ToEmail = userValues.Email,
                        FromEmail = "info@bewerbungsspam.de",
                        FromName = "Bewerbungsspam"
                    }
                );
                return @"{ ""result"": ""succeeded"" }";
            }
            catch(Exception err)
            {
                log.Error("", err);
                return @"{ ""result"": ""failed"" }";
            }
        }

        private async Task<AppUser> GetOrCreateAppUser()
        {
            var appUser = await userManager.GetUserAsync(HttpContext.User);
            if(appUser != null)
            {
                return appUser;
            }
            appUser = new AppUser();
            var createResult = await userManager.CreateAsync(appUser);
            if(!createResult.Succeeded)
            {
                throw new Exception("Couldn't create user");
            }
            await signInManager.SignInAsync(appUser, false);
            return appUser;
        }

        private IDictionary<string, string> getVariableDict(Employer employer, UserValues userValues, DocumentEmail documentEmail, IEnumerable<CustomVariable> customVariables, string jobName)
        {
            var dict =
                new Dictionary<string, string>
                {
                    ["$chefFirma"] = employer.Company,
                    ["$chefGeschlecht"] = employer.Gender,
                    ["$chefTitel"] = employer.FirstName,
                    ["$chefVorname"] = employer.FirstName,
                    ["$chefNachname"] = employer.LastName,
                    ["$chefStrasse"] = employer.Street,
                    ["$chefPostleitzahl"] = employer.Postcode,
                    ["$chefStadt"] = employer.City,
                    ["$chefEmail"] = employer.Email,
                    ["$chefTelefonnummer"] = employer.Phone,
                    ["$chefMobilnummer"] = employer.MobilePhone,
                    ["$meinGeschlecht"] = userValues.Gender,
                    ["$meinTitel"] = userValues.FirstName,
                    ["$meinVorname"] = userValues.FirstName,
                    ["$meinNachname"] = userValues.LastName,
                    ["$meineStrasse"] = userValues.Street,
                    ["$meinPostleitzahl"] = userValues.Postcode,
                    ["$meineStadt"] = userValues.City,
                    ["$meineEmail"] = userValues.FirstName,
                    ["$meineTelefonnummer"] = userValues.Phone,
                    ["$meineMobilnummer"] = userValues.MobilePhone,
                    ["$beruf"] = jobName,
                    ["$emailBetreff"] = documentEmail.Subject,
                    ["$emailText"] = documentEmail.Body,
                    ["$emailAnhang"] = documentEmail.AttachmentName,
                    ["$tagHeute"] = DateTime.Today.Day.ToString("00"),
                    ["$monatHeute"] = DateTime.Today.Month.ToString("00"),
                    ["$jahrHeute"] = DateTime.Today.Year.ToString("0000"),
                };
            Variables.addVariablesToDict(customVariables.Select(x => x.Text), dict);
            return dict;
        }

        public string ConvertToPdf(string path, IDictionary<string, string> dict)
        {
            var fileName = Path.GetFileName(path);
            var tmpPaths = new UnzipPaths();
            var extension = Path.GetExtension(path);
            switch(extension)
            {
                case ".pdf":
                    return path;
                case ".odt":
                    ZipFile.ExtractToDirectory(path, tmpPaths.UnzipTo);
                    ReplaceInDirectory(tmpPaths.UnzipTo, dict);
                    ZipFile.CreateFromDirectory(tmpPaths.UnzipTo, Path.Combine(tmpPaths.ZipTo, fileName));

                    var pdfOutputPath = Path.ChangeExtension(path, ".pdf");
                    System.IO.File.Delete(pdfOutputPath);
                    using (var process1 = new System.Diagnostics.Process())
                    {
                        process1.StartInfo.FileName = "C:/Program Files/LibreOffice 5/program/python.exe";
                        process1.StartInfo.UseShellExecute = false;
                        process1.StartInfo.Arguments =
                        String.Format(@" ""{0}"" --format pdf --output=""{1}"" ""{2}"" ", "c:/Program Files/unoconv/unoconv", pdfOutputPath, path);
                        process1.StartInfo.CreateNoWindow = true;
                        process1.Start();
                        process1.WaitForExit();
                    }
                    if (System.IO.File.Exists(pdfOutputPath))
                    {
                        return pdfOutputPath;
                    }
                    else
                    {
                        throw new Exception("File was not converted");
                    }
                default:
                    throw new Exception($"Unknown file extension: {extension}");
            }
        }

        private string ReplaceInString(string s, IDictionary<string, string> dict)
        {
            foreach(var kv in dict)
            {
                if(kv.Value == "")
                {
                    s.Replace(kv.Key + " ", "");
                }
                s = s.Replace(kv.Key, kv.Value);
            }
            return s;
        }

        private void ReplaceInDirectory(string path, IDictionary<string, string> dict)
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
                    content = ReplaceInString(content, dict);
                    System.IO.File.WriteAllText(fullFilePath, content);
                }
            }
        }

        [HttpPost]
        public async Task UploadFile()
        {
            var appUser = await GetOrCreateAppUser();
            var document = GetDocument(appUser, "documentName");
            var files = Request.Form.Files;
            foreach(var file in Request.Form.Files)
            {
                var userPaths = new UserPaths(file.FileName, appUser.Id);
                var savePath = Path.Combine(userPaths.UserDirectory, file.FileName);
                using (var fileStream = System.IO.File.Create(savePath))
                {
                    file.OpenReadStream().CopyTo(fileStream);
                }
                var documentFile = new DocumentFile { Document = document, Index = -1, Name = file.FileName, Path = savePath, SizeInBytes = -1 };
                dbContext.Add(documentFile);
            }
            dbContext.SaveChanges();
        }

        private JobApplicationSpamState GetJobApplicationSpamState(AppUser appUser)
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
                dbContext.AddRange(customVariables);
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
                            return documentFiles.AsEnumerable<DocumentFile>();
                        }
                    }
                case "Document":
                    return document;

                default:
                    throw new Exception($"Table not found: {table}");
            }
        }
    }
}

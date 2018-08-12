using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using JobApplicationSpam.Models;
using System.Reflection;
using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using JobApplicationSpam.CustomVariableParser;
using Microsoft.AspNetCore.Http;

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
            var appUser = await GetOrCreateAppUser();
            return View("JobApplicationSpam", dbContext.GetJobApplicationSpamState(appUser));
        }

        [HttpPost]
        public async Task Save()
        {
            var appUser = await userManager.GetUserAsync(HttpContext.User);
            foreach (var kv in Request.Form)
            {
                var split = kv.Key.Split(new char[] { '_' });
                var table = split[0];
                var column = split[1];
                int index = -1;
                if (split.Length > 2)
                {
                    Int32.TryParse(split[2], out index);
                }
                dbContext.UpdateDbObject(appUser, "documentName", table, column, index, kv.Value.ToString());
            }
            dbContext.SaveChanges();
        }

        [HttpPost]
        public async Task PerformFileActions()
        {
            var appUser = await userManager.GetUserAsync(HttpContext.User);
            var document = dbContext.GetDocument(appUser, "documentName");
            var documentFiles = ((IEnumerable<DocumentFile>)dbContext.GetDbObject("DocumentFiles", appUser, document)).OrderBy(x => x.Id).AsEnumerable();
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
                var appUser = await userManager.GetUserAsync(HttpContext.User);
                var document = dbContext.GetDocument(appUser, "documentName");
                var documentEmail = (DocumentEmail)dbContext.GetDbObject("DocumentEmail", appUser, document);
                var employer = (Employer)dbContext.GetDbObject("Employer", appUser, document);
                var userValues = (UserValues)dbContext.GetDbObject("UserValues", appUser, document);
                var customVariables = (IEnumerable<CustomVariable>)dbContext.GetDbObject("CustomVariables", appUser, document);
                var documentFiles = (IEnumerable<DocumentFile>)dbContext.GetDbObject("DocumentFiles", appUser, document);
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
        public async Task<JsonResult> UploadFile(IList<IFormFile> files)
        {
            if(!files.Any())
            {
                return Json(new { state = 0, message = "" });
            }
            var appUser = await userManager.GetUserAsync(HttpContext.User);
            var document = dbContext.GetDocument(appUser, "documentName");
            var file = files.ElementAt(0);
            if (file.FileName.EndsWith(".3gp"))
            {
                return Json(new { state = 1, message = "Sorry, the upload failed" });
            }
            var userPaths = new UserPaths(file.FileName, appUser.Id);
            var savePath = Path.Combine(userPaths.UserDirectory, file.FileName);
            using (var fileStream = System.IO.File.Create(savePath))
            {
                file.OpenReadStream().CopyTo(fileStream);
            }
            var documentFile = new DocumentFile { Document = document, Index = -1, Name = file.FileName, Path = savePath, SizeInBytes = -1 };
            dbContext.Add(documentFile);
            dbContext.SaveChanges();
            return Json(new { state = 0, message = file.FileName });
        }

    }
}

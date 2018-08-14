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

                    file1.Name = file2.Name;
                    file1.Path = file2.Path;

                    file2.Name = file1Copy.Name;
                    file2.Path = file1Copy.Path;
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
        public async Task<JsonResult> ApplyNow()
        {
            try
            {
                var appUser = await userManager.GetUserAsync(HttpContext.User);
                var document = dbContext.GetDocument(appUser, "documentName");
                var documentEmail = (DocumentEmail)dbContext.GetDbObject("DocumentEmail", appUser, document);
                var userValues = (UserValues)dbContext.GetDbObject("UserValues", appUser, document);
                var customVariables = (IEnumerable<CustomVariable>)dbContext.GetDbObject("CustomVariables", appUser, document);
                var documentFiles = (IEnumerable<DocumentFile>)dbContext.GetDbObject("DocumentFiles", appUser, document);
                var sentApplication =
                    new SentApplication
                    {
                        Document = document,
                        UserValues = userValues,
                        SentDate = DateTime.Now
                    };
                dbContext.Add(sentApplication);

                var pdfFilePaths = new List<string>();
                var dict = getVariableDict(document.Employer, userValues, documentEmail, customVariables, document.JobName);
                var tmpDirectory = new TmpPath().Path;
                var userDirectory = new UserPath(appUser.Id).UserDirectory;
                foreach (var documentFile in documentFiles)
                {
                    var currentFile = Path.Combine(userDirectory, documentFile.Path);
                    var extension = Path.GetExtension(currentFile).ToLower();
                    var convertedToPdfPath = Path.Combine(tmpDirectory, $"{Path.GetFileNameWithoutExtension(documentFile.Path)}_{Guid.NewGuid().ToString()}.pdf");
                    if(extension == ".odt")
                    {
                        var tmpPath2 = Path.Combine(tmpDirectory, $"{Path.GetFileNameWithoutExtension(documentFile.Path)}_replaced_{Guid.NewGuid().ToString()}{extension}");

                        FileConverter.ReplaceInOdt(currentFile, tmpPath2, dict);
                        currentFile = tmpPath2;
                    }
                    if (FileConverter.ConvertTo(currentFile, convertedToPdfPath))
                    {
                        pdfFilePaths.Add(convertedToPdfPath);
                    }
                    else throw new Exception("Failed to convert file " + documentFile.Name);
                }

                var mergedPath = Path.Combine(new TmpPath().Path, "merged_" + Guid.NewGuid().ToString() + ".pdf");
                if(!FileConverter.MergePdfs(pdfFilePaths, mergedPath))
                {
                    throw new Exception("Failed to merge pdfs.");
                }

                var newEmployer = new Employer();
                dbContext.Add(newEmployer);
                var documentCopy = new Document(document) { Employer = newEmployer };
                dbContext.Add(documentCopy);
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
                var attachments = new List<EmailAttachment>();
                if (pdfFilePaths.Count >= 1)
                {
                    var attachmentName = FileConverter.ReplaceInString(documentEmail.AttachmentName, dict);
                    if(attachmentName.Length >= 1 && !attachmentName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
                    {
                        attachmentName = attachmentName + ".pdf";
                    }
                    else if (!(attachmentName.Length >= 4 && attachmentName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        attachmentName = "Bewerbung.pdf";
                    }
                    attachments.Add(new EmailAttachment { Path = mergedPath, Name = attachmentName });
                }
                HelperFunctions.SendEmail(
                    new EmailData
                    {
                        Attachments = attachments,
                        Body = FileConverter.ReplaceInString(documentEmail.Body, dict),
                        Subject = FileConverter.ReplaceInString(documentEmail.Subject, dict),
                        ToEmail = document.Employer.Email,
                        FromEmail = userValues.Email,
                        FromName =
                            (userValues.Degree == "" ? "" : userValues.Degree + " ") + 
                            userValues.FirstName + " " + userValues.LastName
                    }
                );
                dbContext.SaveChanges();
                return Json(new { status = 0 });
            }
            catch(Exception err)
            {
                log.Error("", err);
                return Json(new { status = 1 });
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

        public class VariableComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                int lengthCompareResult = -1 * x.Length.CompareTo(y.Length);
                if(lengthCompareResult == 0)
                {
                    return x.CompareTo(y);
                }
                else
                {
                    return lengthCompareResult;
                }
            }
        }

        private SortedDictionary<string, string> getVariableDict(
            Employer employer,
            UserValues userValues,
            DocumentEmail documentEmail,
            IEnumerable<CustomVariable> customVariables,
            string jobName)
        {
            var dict =
                new SortedDictionary<string, string>(new VariableComparer())
                {
                    ["$chefFirma"] = employer.Company ?? "",
                    ["$chefGeschlecht"] = employer.Gender ?? "",
                    ["$chefTitel"] = employer.Degree ?? "",
                    ["$chefVorname"] = employer.FirstName ?? "",
                    ["$chefNachname"] = employer.LastName ?? "",
                    ["$chefStrasse"] = employer.Street ?? "",
                    ["$chefPostleitzahl"] = employer.Postcode ?? "",
                    ["$chefStadt"] = employer.City ?? "",
                    ["$chefEmail"] = employer.Email ?? "",
                    ["$chefTelefonnummer"] = employer.Phone ?? "",
                    ["$chefMobilnummer"] = employer.MobilePhone ?? "",
                    ["$meinGeschlecht"] = userValues.Gender ?? "",
                    ["$meinTitel"] = userValues.Degree ?? "",
                    ["$meinVorname"] = userValues.FirstName ?? "",
                    ["$meinNachname"] = userValues.LastName ?? "",
                    ["$meineStrasse"] = userValues.Street ?? "",
                    ["$meinePostleitzahl"] = userValues.Postcode ?? "",
                    ["$meineStadt"] = userValues.City ?? "",
                    ["$meineEmail"] = userValues.Email ?? "",
                    ["$meineTelefonnummer"] = userValues.Phone ?? "",
                    ["$meineMobilnummer"] = userValues.MobilePhone ?? "",
                    ["$beruf"] = jobName ?? "",
                    ["$emailBetreff"] = documentEmail.Subject ?? "",
                    ["$emailText"] = documentEmail.Body ?? "",
                    ["$emailAnhang"] = documentEmail.AttachmentName ?? "",
                    ["$tagHeute"] = DateTime.Today.Day.ToString("00"),
                    ["$monatHeute"] = DateTime.Today.Month.ToString("00"),
                    ["$jahrHeute"] = DateTime.Today.Year.ToString("0000"),
                };
            Variables.addVariablesToDict(customVariables.Select(x => x.Text), dict);
            return dict;
        }

        [HttpPost]
        public async Task<JsonResult> UploadFile(IList<IFormFile> files)
        {
            try
            {
                if (!files.Any())
                {
                    return Json(new { state = 0, message = "There were no files to upload." });
                }
                var appUser = await userManager.GetUserAsync(HttpContext.User);
                var document = dbContext.GetDocument(appUser, "documentName");
                var file = files.ElementAt(0);

                if (file.Length > FileConverter.GetMaxUploadSizeInBytes())
                {
                    return Json(new { state = 1, message = "Sorry, the maximum file size is " + FileConverter.GetMaxUploadSizeAsString() + "." });
                }

                if (!FileConverter.IsUploadFileTypeValid(Path.GetExtension(file.FileName)))
                {
                    return Json(new { state = 1, message = "Sorry, only pdf, odt, doc and docx files can be upload." });
                }
                var userPath = new UserPath(appUser.Id);
                var savePath = Path.Combine(new TmpPath().Path, file.FileName);

                using (var originalFileStream = file.OpenReadStream())
                {
                    using (var saveFileStream = System.IO.File.Create(savePath))
                    {
                        originalFileStream.CopyTo(saveFileStream);
                    }
                }

                var dbFileNames =
                    dbContext.DocumentFiles
                        .Where(x => x.Document.Name == "documentName" || true)
                        .Select(x => x.Name);
                var diskFileNames =
                    System.IO.Directory.EnumerateFiles(new UserPath(appUser.Id).UserDirectory)
                        .Select(x => Path.GetFileName(x));

                var uploadedFileData = FileConverter.GetUploadedFileData(appUser.Id, savePath, dbFileNames, diskFileNames);
                if (uploadedFileData.ConvertAndSave())
                {
                    dbContext.Add(
                        new DocumentFile
                        {
                            Document = document,
                            Name = uploadedFileData.DisplayedFileName,
                            Path = uploadedFileData.SavedFileName,
                        }
                    );
                    dbContext.SaveChanges();
                    return Json(new { state = 0, message = uploadedFileData.DisplayedFileName });
                }
                else
                {
                    return Json(new { state = 1, message = "Sorry, the upload failed." });
                }
            }
            catch(Exception err)
            {
                log.Error("", err);
                return Json(new { state = 1, message = "Sorry, an error occurred." });
            }
        }
    }
}

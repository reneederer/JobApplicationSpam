using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace JobApplicationSpam.Models
{
    public class UnzipPaths
    {
        public string UnzipTo { get; set; }
        public string ZipTo { get; set; }
        public UnzipPaths()
        {
            var guid = UnzipTo = Guid.NewGuid().ToString();
            UnzipTo = $"c:/users/rene/JobApplicationSpam/tmp/{guid}/unzipped/";
            ZipTo =   $"c:/users/rene/JobApplicationSpam/tmp/{guid}/zipped/";
            Directory.CreateDirectory(UnzipTo);
            Directory.CreateDirectory(ZipTo);
        }
    }

    public class TmpPath
    {
        public string Path { get; set; }
        public TmpPath()
        {
            Path = $"c:/users/rene/JobApplicationSpam/tmp/{Guid.NewGuid().ToString()}/";
            Directory.CreateDirectory(Path);
        }
    }

    public class UserPaths
    {
        public string SaveTmp { get; set; }
        public string UserDirectory { get; set; }
        public UserPaths(string path, string userId)
        {
            UserDirectory = $"c:/users/rene/JobApplicationSpam/data/{userId}/";
            SaveTmp = $"c:/users/rene/JobApplicationSpam/tmp/{Guid.NewGuid().ToString()}/";
            Directory.CreateDirectory(UserDirectory);
        }
    }

    public class AppUser : IdentityUser
    {
        public string ConfirmEmailGuid { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string KeepLoggedInGuid { get; set; }
        public DateTime? KeepLoggedInExpiresOn { get; set; } = null;
        public string ChangePasswordGuid { get; set; } = null;
        public DateTime? ChangePasswordExpiresOn = null;
    }

    public class AccountModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; }

        [Required]
        [UIHint("password")]
        public string Password { get; set; }

        public bool ModalVisible { get; set; } = false;

        public string CurrentTab { get; set; } = "Login";
    }

    public class LoginModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; }

        [Required]
        [UIHint("password")]
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; }

        [Required]
        [UIHint("password")]
        public string Password { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        [UIHint("password")]
        public string Password { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; }
    }






    public class DocumentEmail
    {
        public DocumentEmail() { }
        public DocumentEmail(DocumentEmail other)
        {
            Subject = other.Subject;
            Body = other.Body;
            AttachmentName = other.AttachmentName;
        }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachmentName { get; set; }
    }

    public class DocumentFile
    {
        public DocumentFile() { }
        public DocumentFile(DocumentFile other)
        {
            Name = other.Name;
            Path = other.Path;
            SizeInBytes = other.SizeInBytes;
            Index = other.Index;
            Document = other.Document;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int SizeInBytes { get; set; }
        public int Index { get; set; }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
    }

    public class CustomVariable
    {
        public CustomVariable() { }
        public CustomVariable(CustomVariable other)
        {
            Text = other.Text;
            Document = other.Document;
        }
        public int Id { get; set; }
        public string Text { get; set; }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
    }

    public class UserValues
    {
        public UserValues() { }
        public UserValues(UserValues other)
        {
            Gender = other.Gender;
            Degree = other.Degree;
            FirstName = other.FirstName;
            LastName = other.LastName;
            Street = other.Street;
            Postcode = other.Postcode;
            City = other.City;
            Email = other.Email;
            Phone = other.Phone;
            MobilePhone = other.MobilePhone;
        }
        [ForeignKey("UserId")]
        public AppUser AppUser { get; set; }
        public int Id { get; set; }
        public string Gender { get; set; }
        public string Degree { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string Postcode { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
    }

    public class Employer
    {
        public int Id { get; set; }
        [ForeignKey("UserId")]
        public AppUser AppUser { get; set; }
        public string Company { get; set; }
        public string Gender { get; set; }
        public string Degree { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string Postcode { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
    }


    public class Document
    {
        public Document() { }
        public Document(Document other)
        {
            AppUser = other.AppUser;
            JobName = other.JobName;
        }
        public int Id { get; set; }
        public string AppUserId { get; set; }
        [ForeignKey ("AppUserId")]
        public AppUser AppUser { get; set; }
        [ForeignKey ("EmployerId")]
        public string JobName { get; set; }
    }

    public enum SentApplicationStatus
    {
        NotYetSent,
        SentWaitingForReply,
        Rejected,
        Accepted,
        Invited
    }

    public class SentApplication
    {
        public int Id { get; set; }
        [ForeignKey ("UserValuesId")]
        public UserValues UserValues { get; set; }
        public Document Document { get; set; }
        [ForeignKey ("EmployerId")]
        public Employer Employer { get; set; }
        public DateTime SentDate { get; set; }
    }
}

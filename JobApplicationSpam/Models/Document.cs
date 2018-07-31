using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JobApplicationSpam.Models
{
    public class AppUser : IdentityUser
    {
    }

    public class DocumentEmail
    {
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachmentName { get; set; }
    }

    public class DocumentFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int sizeInBytes { get; set; }
        public int Index { get; set; }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
    }

    public class CustomVariable
    {
        public int Id { get; set; }
        public string Text { get; set; }
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }
    }

    public class UserValues
    {
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
        public int Id { get; set; }
        public string AppUserId { get; set; }
        [ForeignKey ("AppUserId")]
        public AppUser AppUser { get; set; }
        [ForeignKey ("EmployerId")]
        public Employer Employer { get; set; }
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
        public Document Document { get; set; }
        public UserValues UserValues { get; set; }
        public Employer Employer { get; set; }
        public DateTime SentDate { get; set; }
    }
}

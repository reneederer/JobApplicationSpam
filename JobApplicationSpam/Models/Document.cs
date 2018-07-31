﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace JobApplicationSpam.Models
{
    public class TmpPaths
    {
        public string UnzipTo { get; set; }
        public string ZipTo { get; set; }
        public string UserDirectory { get; set; }
        public TmpPaths(string path, string userId)
        {
            var guid = UnzipTo = Guid.NewGuid().ToString();
            UnzipTo = $"c:/users/rene/JobApplicationSpam/tmp/{guid}/unzipped/";
            ZipTo =   $"c:/users/rene/JobApplicationSpam/tmp/{guid}/zipped/";
            UserDirectory = $"c:/users/rene/JobApplicationSpam/data/{userId}/";
            Directory.CreateDirectory(UnzipTo);
            Directory.CreateDirectory(ZipTo);
            Directory.CreateDirectory(UserDirectory);
        }
    }

    public class AppUser : IdentityUser
    {
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

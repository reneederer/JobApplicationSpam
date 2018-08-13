using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace JobApplicationSpam.Models
{
    public interface ISignInManager
    {
        AppUser CurrentUser { get; set; }
        Task<SignInResult> PasswordSignInAsync(AppUser user, string password, bool isPersistent, bool lockoutOnFailure);
        Task SignInAsync(AppUser user, bool isPersistent);
        Task SignOutAsync();
    }

    public interface IUserManager
    {
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> GetUserAsync(ClaimsPrincipal claimsPrincipal);
        IPasswordHasher<AppUser> PasswordHasher { get; set; }
        Task<IdentityResult> CreateAsync(AppUser user);
        Task<IdentityResult> UpdateAsync(AppUser user);
    }

    public class RealSignInManager
        : Microsoft.AspNetCore.Identity.SignInManager<AppUser>,
          ISignInManager
    {
        public AppUser CurrentUser { get; set; }

        public RealSignInManager(
            UserManager<AppUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<AppUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<Microsoft.AspNetCore.Identity.SignInManager<AppUser>> logger,
            IAuthenticationSchemeProvider schemes)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
        }


        public Task<SignInResult> PasswordSignInAsync(
            AppUser user,
            string password,
            bool isPersistent,
            bool lockoutOnFailure)
        {
            return PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

        public Task SignInAsync(AppUser user, bool isPersistent)
        {
            return base.SignInAsync(user, isPersistent);
        }

        public override Task SignOutAsync()
        {
            return base.SignOutAsync();
        }

    }

    public class FakeSignInManager : ISignInManager
    {
        public AppUser CurrentUser { get; set; }

        public async Task<SignInResult> PasswordSignInAsync(AppUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var dict = new Dictionary<string, string>
            {
                ["q@q.de"] = "1234"
            };
            if(dict.ContainsKey(user.Id))
            {
                if(dict[user.Id] == password)
                {
                    CurrentUser = user;
                    return SignInResult.Success;
                }
                else
                {
                    return SignInResult.Failed;
                }
            }
            else
            {
                return SignInResult.Failed;
            }
        }

        public async Task SignInAsync(AppUser user, bool isPersistent)
        {
            CurrentUser = user;
        }

        public async Task SignOutAsync()
        {
            CurrentUser = null;
        }
    }

    public class RealUserManager :UserManager<AppUser>, IUserManager
    {
        public RealUserManager(
            IUserStore<AppUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<AppUser> passwordHasher,
            IEnumerable<IUserValidator<AppUser>> userValidators,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<AppUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        IPasswordHasher<AppUser> IUserManager.PasswordHasher { get => PasswordHasher; set => this.PasswordHasher = value; }

        public override Task<AppUser> GetUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            return base.GetUserAsync(claimsPrincipal);
        }

        Task<IdentityResult> IUserManager.UpdateAsync(AppUser user)
        {
            return base.UpdateAsync(user);
        }
    }

    public class FakeUserManager : IUserManager
    {
        private AppUser CurrentUser { get; set; }
        IPasswordHasher<AppUser> IUserManager.PasswordHasher { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<IdentityResult> CreateAsync(AppUser user)
        {
            throw new NotImplementedException();
        }

        public Task<AppUser> FindByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<AppUser> GetUserAsync(ClaimsPrincipal claimsPrincipal)
        {
            return Task.FromResult(CurrentUser);
        }

        Task<IdentityResult> IUserManager.UpdateAsync(AppUser user)
        {
            throw new NotImplementedException();
        }
    }

    public class UnzipPaths
    {
        public string UnzipTo { get; set; } = "";
        public string ZipTo { get; set; } = "";
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
        public string Path { get; set; } = "";
        public TmpPath()
        {
            Path = $"c:/users/rene/JobApplicationSpam/tmp/{Guid.NewGuid().ToString()}/";
            Directory.CreateDirectory(Path);
        }
    }

    public class UploadedFileData
    {
        public string OriginalFilePath { get; set; } = "";
        public string SavedFileName { get; set; } = "";
        public string DisplayedFileName { get; set; } = "";

        public Func<bool> ConvertAndSave { get; set; }
    }

    public class UserPath
    {
        public string UserDirectory { get; set; } = "";
        public UserPath(string userId)
        {
            UserDirectory = $"c:/users/rene/JobApplicationSpam/data/{userId}/";
            Directory.CreateDirectory(UserDirectory);
        }
    }

    public class AppUser : IdentityUser
    {
        public string ConfirmEmailGuid { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string KeepLoggedInGuid { get; set; } = Guid.NewGuid().ToString();
        public DateTime? KeepLoggedInExpiresOn { get; set; } = DateTime.Now + TimeSpan.FromDays(7);
        public string ChangePasswordGuid { get; set; } = Guid.NewGuid().ToString();
        public DateTime? ChangePasswordExpiresOn = DateTime.Now + TimeSpan.FromDays(7);
        public AppUser()
        {
            this.Id = Guid.NewGuid().ToString();
            this.UserName = Guid.NewGuid().ToString();
            this.Email = "guest@bewerbungsspam.de";
        }
    }

    public class AccountModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; } = "";

        [Required]
        [UIHint("password")]
        public string Password { get; set; } = "";

        public bool ModalVisible { get; set; } = false;

        public string CurrentTab { get; set; } = "Login";
    }

    public class LoginModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; } = "";

        [Required]
        [UIHint("password")]
        public string Password { get; set; } = "";
    }

    public class RegisterModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; } = "";

        [Required]
        [UIHint("password")]
        public string Password { get; set; } = "";
    }

    public class ChangePasswordModel
    {
        [Required]
        [UIHint("password")]
        public string Password { get; set; } = "";
    }

    public class ForgotPasswordModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; } = "";
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
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public string AttachmentName { get; set; } = "";
    }

    public class DocumentFile
    {
        public DocumentFile() { }
        public DocumentFile(DocumentFile other)
        {
            Name = other.Name;
            Path = other.Path;
            Document = other.Document;
        }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
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
        public string Text { get; set; } = "";
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
        public string Degree { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Street { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string City { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string MobilePhone { get; set; } = "";
    }

    public class Employer
    {
        public int Id { get; set; }
        public string Company { get; set; } = "";
        public string Gender { get; set; }
        public string Degree { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Street { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string City { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string MobilePhone { get; set; } = "";
    }


    public class Document
    {
        public Document() { }
        public Document(Document other)
        {
            AppUser = other.AppUser;
            Employer = other.Employer;
            JobName = other.JobName;
            Name = other.Name;
        }
        public int Id { get; set; }
        [ForeignKey ("UserId")]
        public AppUser AppUser { get; set; }
        [ForeignKey ("EmployerId")]
        public Employer Employer { get; set; }
        public string JobName { get; set; } = "";
        public string Name { get; set; } = "";
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
        [ForeignKey ("DocumentId")]
        public Document Document { get; set; }
        public DateTime SentDate { get; set; }
    }
}

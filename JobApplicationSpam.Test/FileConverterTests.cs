using System;
using Xunit;
using JobApplicationSpam.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Npgsql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Transactions;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore;
using Moq;
using System.Security.Claims;

namespace JobApplicationSpam
{
    public class FileConverterTests
    {
        [Fact]
        public void AreFilesEqualWithUnequalPdfFilesReturnsFalse()
        {
            string filePath1 = @"C:\Users\rene\JobApplicationSpam\data\9f27b9b8-b4ff-42f1-abbc-379a002470c8\bewerbung1.pdf";
            string filePath2 = @"C:\Windows\InfusedApps\Packages\ScreenovateTechnologies.DellMobileConnect_1.1.3750.0_x64__0vhbc3ng4wbp0\app\Resources\l_terms_es.pdf";
            Assert.False(FileConverter.AreFilesEqual(filePath1, filePath2));
        }

        [Fact]
        public void AreFilesEqualWithEqualPdfFilesReturnsTrue()
        {
            string filePath1 = @"C:\Users\rene\JobApplicationSpam\data\9f27b9b8-b4ff-42f1-abbc-379a002470c8\bewerbung1.pdf";
            string filePath2 = @"C:\Users\rene\JobApplicationSpam\data\9f27b9b8-b4ff-42f1-abbc-379a002470c8\bewerbung2.pdf";
            Assert.True(FileConverter.AreFilesEqual(filePath1, filePath2));
        }

        [Fact]
        public void AreFilesEqualWithEqualOdtFilesReturnsTrue()
        {
            string filePath1 = @"C:\Users\rene\Desktop\bewerbung1.odt";
            string filePath2 = @"C:\Users\rene\Desktop\bewerbung2.odt";
            Assert.True(FileConverter.AreFilesEqual(filePath1, filePath2));
        }
    }
}
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

        public ViewResult J() =>
            View(
                "JobApplicationSpam",
                new Document
                {
                    JobName = "Fachinformatiker für Anwendungsentwicklung"
                }
            );

        [HttpPost]
        public string Save()
        {
            foreach (var kv in Request.Form)
            {
                var split = kv.Key.Split(new char[] { '_' });
                var table = split[0];
                var column = split[1];
                MethodInfo method =
                    typeof(JobApplicationSpamDbContext)
                        .GetMethod(
                            "Find",
                            new Type[] { (new object[] { }).GetType() });
                MethodInfo generic =
                    method.MakeGenericMethod(Type.GetType($"JobApplicationSpam.Models.{table}"));
                var dbObject = generic.Invoke(dbContext, new object[] { new object[] { getCurrentTableId(table, "538b98c6-e34a-4fb4-8ee5-0731d8720569") } });
                dbObject.GetType().GetProperty(column).SetValue(dbObject, kv.Value.ToString());
                dbContext.Update(dbObject);
            }
            dbContext.SaveChanges();
            return "";
        }

        private object getCurrentTableId(string table, object userId)
        {
            switch(table)
            {
                case "UserValues":
                    var userValues =
                        dbContext.UserValues
                            .Where(x => x.AppUser.Id == (string)userId)
                            .OrderByDescending(x => x.Id).FirstOrDefault();
                    if(userValues == null)
                    {
                        userValues = new UserValues { AppUser = dbContext.AppUsers.Find(new[] { userId }) };
                        dbContext.Add(userValues);
                        dbContext.SaveChanges();
                    }
                    return userValues;
                case "Document":
                default:
                    throw new Exception($"Table not found: {table}");
            }
        }

    }
}

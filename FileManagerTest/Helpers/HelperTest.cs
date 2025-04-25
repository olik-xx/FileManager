namespace FileManagerTest.Helpers
{
    using System;
    using FileManager.Helpers;
    using FileManager.Options;
    using Microsoft.AspNetCore.Builder;
    using Newtonsoft.Json;

    [TestFixture]
    internal class HelperTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Rebind_Positive()
        {
            var options = new FileManageOptions();
            options.Count += 10;
            options.TimeoutIdle += 10;
            options.Buffer = 4096;
            options.Dir = Guid.NewGuid().ToString("N");
            options.Outdir = Guid.NewGuid().ToString("N");

            ParamHelper.FileManageOptions = options;
            Console.WriteLine(JsonConvert.SerializeObject(options, Formatting.Indented));
            Console.WriteLine();
    
            var builder = WebApplication.CreateBuilder();
            ParamHelper.Configuration = builder.Configuration;
            Helper.Rebind();

            Console.WriteLine(JsonConvert.SerializeObject(options, Formatting.Indented));
        }

       
    }
}

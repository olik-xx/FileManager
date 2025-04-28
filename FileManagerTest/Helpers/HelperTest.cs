namespace FileManagerTest.Helpers
{
    using System;
    using FileManager.Helpers;
    using FileManager.Options;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
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
            var builder = WebApplication.CreateBuilder();
            ParamHelper.Configuration = builder.Configuration;
            builder.Configuration.GetSection("FileManage").Bind(ParamHelper.FileManageOptions);

            FileManageOptions expected = ParamHelper.FileManageOptions.Clone();

            ParamHelper.FileManageOptions.Count += 5;
            ParamHelper.FileManageOptions.TimeoutIdle += 10;
            ParamHelper.FileManageOptions.InterFileTimeoutIdle += 10;
            ParamHelper.FileManageOptions.Buffer = 4096;
            ParamHelper.FileManageOptions.Dir = Guid.NewGuid().ToString("N");
            ParamHelper.FileManageOptions.Outdir = Guid.NewGuid().ToString("N");

            Console.WriteLine(JsonConvert.SerializeObject(ParamHelper.FileManageOptions, Formatting.Indented));
            Console.WriteLine();
           
            //
            Helper.Rebind();
            Console.WriteLine(JsonConvert.SerializeObject(ParamHelper.FileManageOptions, Formatting.Indented));

            Assert.That(ParamHelper.FileManageOptions.Count, Is.EqualTo(expected.Count));
            Assert.That(ParamHelper.FileManageOptions.TimeoutIdle, Is.EqualTo(expected.TimeoutIdle));
            Assert.That(ParamHelper.FileManageOptions.InterFileTimeoutIdle, Is.EqualTo(expected.InterFileTimeoutIdle));
            Assert.That(ParamHelper.FileManageOptions.Buffer, Is.EqualTo(expected.Buffer));
            Assert.That(ParamHelper.FileManageOptions.Dir, Is.EqualTo(expected.Dir));
            Assert.That(ParamHelper.FileManageOptions.Outdir, Is.EqualTo(expected.Outdir));
        }

       
    }
}

namespace FileManagerTest.Options
{
    using FileManager.Options;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    [TestFixture]
    internal class FileManageOptionsTest
    {
        FileManageOptions _options = new FileManageOptions();

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Bind_Positive()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.GetSection("FileManage").Bind(_options);

            Console.WriteLine(JsonConvert.SerializeObject(_options, Formatting.Indented));
        }
    }
}

namespace FileManagerTest.Scheduler
{
    using System;
    using System.Linq;
    using FileManager.Helpers;
    using FileManager.Scheduler.FindFiles;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;

    [TestFixture]
    internal class FindFilesServiceTest
    {
        /// <summary>
        /// Представляет сервис получения конфигурации.
        /// </summary>
        private FindFilesService _service;

        [SetUp]
        public void SetUp()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.GetSection("FileManage").Bind(ParamHelper.FileManageOptions);

            ILogger<FindFilesService> logger = Mock.Of<ILogger<FindFilesService>>();
            _service = new FindFilesService(logger);
        }

        [TearDown]
        public void TearDown()
        {
            _service = null;
        }

        [Test]
        public void Execute_Positive()
        {
            FileInfo[] rc = _service.Execute();
            Console.WriteLine(JsonConvert.SerializeObject(rc.Select(x=> x.FullName), Formatting.Indented));

            CollectionAssert.IsNotEmpty(rc, "Не найдены файлы");
            Assert.That(rc.Length, Is.LessThanOrEqualTo(ParamHelper.FileManageOptions.Count), "Неверное количество файлов");
        }
    }
}

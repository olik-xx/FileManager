namespace FileManagerTest.Scheduler
{
    using System;
    using System.Linq;
    using FileManager.Helpers;
    using FileManager.Resources;
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
            ParamHelper.FileManageOptions.Count = 3;

            FileInfo[] rc = _service.Execute();
            Console.WriteLine(JsonConvert.SerializeObject(rc.Select(x=> x.FullName), Formatting.Indented));

            CollectionAssert.IsNotEmpty(rc, "Не найдены файлы");
            Assert.That(rc.Length, Is.LessThanOrEqualTo(ParamHelper.FileManageOptions.Count), "Неверное количество файлов");
        }

        [Test]
        public void Execute_DirectoryNotFound_Negative()
        {
            string path = ParamHelper.FileManageOptions.Dir = Guid.NewGuid().ToString();

            var ex = Assert.Throws<DirectoryNotFoundException>(() => _service.Execute());
            Console.WriteLine(ex.ToString());

            Assert.That(ex.Message, Is.EqualTo(string.Format(ExceptionResource.DirectoryNotFound, path)));
        }

        [Test]
        public void Execute_FilesNotFound_Positive()
        {
            string path = ParamHelper.FileManageOptions.Dir = Guid.NewGuid().ToString();
            Directory.CreateDirectory(path);

            FileInfo[] rc = _service.Execute();
            CollectionAssert.IsEmpty(rc);
        }
    }
}

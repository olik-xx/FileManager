namespace FileManagerTest.Scheduler
{
    using System.Text;
    using FileManager.Cache;
    using FileManager.Helpers;
    using FileManager.Resources;
    using FileManager.Scheduler;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;

    [TestFixture]
    internal class DistinctFileServiceTest
    {
        /// <summary>
        /// Представляет сервис получения конфигурации.
        /// </summary>
        private DistinctFileService _service;

        [SetUp]
        public void SetUp()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.GetSection("FileManage").Bind(ParamHelper.FileManageOptions);

            ILogger<DistinctFileService> logger = Mock.Of<ILogger<DistinctFileService>>();
            _service = new DistinctFileService(ParamHelper.FileManageOptions, logger);
        }

        [TearDown]
        public void TearDown()
        {
            _service = null;
        }

        [TestCase(@"Fakes\Mona Lisa.txt")]
        [TestCase(@"Fakes\Neva Play.txt")]
        [TestCase(@"Fakes\Not Like Us.txt")]
        [TestCase(@"Fakes\Old Town Road.txt")]
        [TestCase(@"Fakes\Sweet Dreams.txt")]
        [TestCase(@"Fakes\Waste It On Me.txt")]
        [TestCase(@"Fakes\Winter Ahead.txt")]
        [TestCase(@"Fakes\Young Man.txt")]
        public async Task Execute_Positive(string filePath)
        {
            Console.WriteLine(filePath);

            int countBefore = ParamHelper.UnionFileCache.Count;

            FileInfo fileInfo = new FileInfo(filePath);
            string jobName = $"{SchedulerResource.DistinctFile} {fileInfo.Name}";
            byte[][] rc = await _service.Execute(fileInfo, jobName);

            Console.WriteLine("Найдено дубликатов: {0}", rc.Length);
            for (int i = 0; i < rc.Length; i++)
                Console.WriteLine(Encoding.UTF8.GetString(rc[i]));

            Assert.That(ParamHelper.UnionFileCache.Count, Is.EqualTo(countBefore + 1), $"Не добавлен в кеш пересечений {nameof(UnionFileCache)}");
        }
    }
}

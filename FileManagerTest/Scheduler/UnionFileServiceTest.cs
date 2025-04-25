namespace FileManagerTest.Scheduler
{
    using FileManager.Cache;
    using System.Text;
    using FileManager.Helpers;
    using FileManager.Resources;
    using FileManager.Scheduler;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;

    [TestFixture]
    internal class UnionFileServiceTest
    {
        /// <summary>
        /// Представляет сервис получения конфигурации.
        /// </summary>
        private UnionFileService _service;

        [SetUp]
        public void SetUp()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.GetSection("FileManage").Bind(ParamHelper.FileManageOptions);

            ILogger<UnionFileService> logger = Mock.Of<ILogger<UnionFileService>>();
            _service = new UnionFileService(ParamHelper.FileManageOptions, logger);

            ParamHelper.UnionFileCache.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _service = null;
        }

        [TestCase(@"Fakes")]
        public async Task Execute_Positive(string directory)
        {
            Assert.That(ParamHelper.UnionFileCache.Count, Is.EqualTo(0), $"Непустой кеш пересечений {nameof(UnionFileCache)}");

            ILogger<DistinctFileService> logger = Mock.Of<ILogger<DistinctFileService>>();
            var fileService = new DistinctFileService(ParamHelper.FileManageOptions, logger);

            FileInfo[] data = new DirectoryInfo(directory).GetFiles();
            foreach (FileInfo item in data)
                await fileService.Execute(item, item.FullName);

            Assert.That(ParamHelper.UnionFileCache.Count, Is.GreaterThan(0), $"Не сформирован кеш пересечений {nameof(UnionFileCache)}");

            byte[][] rc = _service.Execute();
            Console.WriteLine("Найдено пересечений: {0} для кол-ва файлов: {1}", rc.Length, ParamHelper.UnionFileCache.Count);
            for (int i = 0; i < rc.Length; i++)
                Console.WriteLine(Encoding.UTF8.GetString(rc[i]));

            
        }
    }
}

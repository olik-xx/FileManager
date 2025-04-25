namespace FileManager.Scheduler
{
    using Quartz;

    /// <summary>
    /// Интерфейс, предоставляющий методы доступа к планировщику заданий.
    /// </summary>
    public interface ISchedulerDetail
    {
        /// <summary>
        /// Интерфейс, предоставляющий методы доступа к заданию.
        /// </summary>
        IJobDetail JobDetail { get; set; }

        /// <summary>
        /// Интерфейс, предоставляющий методы доступа к запуску задания.
        /// </summary>
        ITrigger Trigger { get; set; }

        /// <summary>
        /// Интерфейс, предоставляющий методы доступа к инициализации планировщика.
        /// </summary>
        /// <returns>Экземпляр планировщика заданий.</returns>
        ISchedulerDetail Init();
    }
}

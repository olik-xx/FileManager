namespace FileManager.Scheduler
{
    using System.Collections.Generic;
    using Quartz;

    /// <summary>
    /// Представляет интерфейс для хранения заданий планировщика.
    /// </summary>
    public interface ITaskStorage : IEnumerable<ISchedulerDetail>
    {
        /// <summary>
        /// Представляет коллекцию прослушивателей заданий.
        /// </summary>
        Dictionary<IJobListener, JobKey> JobListeners { get; }

        /// <summary>
        /// Возвращает количество заданий в хранилище.
        /// </summary>
        int Count { get; }
    }
}

namespace FileManager.Scheduler
{
    using System;
    using Quartz;
    using Quartz.Impl.Triggers;

    /// <summary>
    /// Вспомогательный класс, предоставляющий методы расширения триггера запуска задания.
    /// </summary>
    public static class ITriggerExtension
    {
        /// <summary>
        /// Возвращает строитель триггера запуска задания для указанного триггера <paramref name="trigger"/>.
        /// </summary>
        /// <param name="builder">Строитель триггера.</param>
        /// <param name="trigger">Триггер</param>
        /// <returns>Построитель триггера.</returns>
        public static TriggerBuilder GetTriggerBuilder(this TriggerBuilder builder, ITrigger trigger)
        {
            // todo уйти от частной реализации
            JobDataMap dataMap = trigger.JobDataMap;

            if (trigger is SimpleTriggerImpl)
                return GetTriggerBuilder(builder, dataMap, (SimpleTriggerImpl)trigger);
            if (trigger is DailyTimeIntervalTriggerImpl)
                return GetTriggerBuilder(builder, dataMap, (DailyTimeIntervalTriggerImpl)trigger);
            return null;
        }

        /// <summary>
        /// Возвращает строитель триггера запуска задания по указанным данным задания <paramref name="dataMap"/> и триггеру типа <see cref="SimpleTriggerImpl"/>.
        /// </summary>
        /// <param name="builder">Строитель.</param>
        /// <param name="dataMap">Данные задания.</param>
        /// <param name="trigger">Триггер запуска.</param>
        /// <returns>Строитель триггера.</returns>
        public static TriggerBuilder GetTriggerBuilder(this TriggerBuilder builder, JobDataMap dataMap, SimpleTriggerImpl trigger)
        {
            // todo уйти от частной реализации
            builder.WithSimpleSchedule
                (x =>
                    x.WithIntervalInSeconds(1)
                     .RepeatForever()
                );
            return builder;
        }

        /// <summary>
        /// Возвращает строитель триггера запуска задания по указанным данным задания <paramref name="dataMap"/> и триггеру типа <see cref="DailyTimeIntervalTriggerImpl"/>.
        /// </summary>
        /// <param name="builder">Строитель.</param>
        /// <param name="dataMap">Данные задания.</param>
        /// <param name="trigger">Триггер запуска.</param>
        /// <returns>Строитель триггера.</returns>
        public static TriggerBuilder GetTriggerBuilder(this TriggerBuilder builder, JobDataMap dataMap, DailyTimeIntervalTriggerImpl trigger)
        {
            //var helper = dataMap["Helper"] as IConfigBaseHelper;
            //var paramName = dataMap.GetString("ParamName");
            var time = new TimeSpan(10, 0, 0);  // todo уйти от частной реализации

            builder.WithDailyTimeIntervalSchedule
                (x =>
                    x.WithIntervalInHours(24)
                        .OnEveryDay()
                        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(time.Hours, time.Minutes))
                        //.InTimeZone(TimeZoneInfo.Utc)
                );
            return builder;
        }
    }
}
﻿using System.Collections.Concurrent;
using TaskService.Libraries.ScheduleTask;
using static TaskService.Libraries.ScheduleTask.ScheduleTaskBuilder;

namespace TaskService.Libraries.QueueTask
{
    public class ScheduleTaskBackgroundService(ILogger<ScheduleTaskBackgroundService> logger) : BackgroundService
    {

        private readonly ConcurrentDictionary<string, string?> historyList = new();

        private readonly ILogger logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG
            await Task.Delay(5000, stoppingToken);
#else
            await Task.Delay(10000, stoppingToken);
#endif

            if (scheduleMethodList.Count != 0)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        foreach (var item in scheduleMethodList.Values.Where(t => t.IsEnable).ToList())
                        {
                            var nowTime = DateTimeOffset.Parse(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zzz"));

                            if (item.LastTime == null)
                            {
                                item.LastTime = nowTime.AddSeconds(5);
                            }

                            var nextTime = DateTimeOffset.Parse(CronHelper.GetNextOccurrence(item.Cron, item.LastTime.Value).ToString("yyyy-MM-dd HH:mm:ss zzz"));

                            if (nextTime < nowTime)
                            {
                                item.LastTime = null;
                            }

                            if (nextTime == nowTime)
                            {
                                string key = nowTime.ToUnixTimeSeconds() + item.Name;

                                if (historyList.TryAdd(key, null))
                                {
                                    item.LastTime = nowTime;
                                    RunAction(item,key);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"ExecuteAsync：{ex.Message}");
                    }

                    await Task.Delay(900, stoppingToken);
                }
            }
        }



        private void RunAction(ScheduleTaskInfo scheduleTaskInfo,string key)
        {
            Task.Run(() =>
            {
                try
                {
                    scheduleTaskInfo.Method.Invoke(scheduleTaskInfo.Context, null);
                }
                catch (Exception ex)
                {
                    logger.LogError($"RunAction-{scheduleTaskInfo.Method.Name};{ex.Message}");
                }
                finally
                {
                    historyList.TryRemove(key, out _);
                }
            });
        }
    }
}

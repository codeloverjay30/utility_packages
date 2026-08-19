using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressBarUtilityServices
{
    public class SpectreTracker<T> : ITaskProgressTracker
    {
        private double _percentage;
        private string _message;
        private bool _isCompleted;
        private readonly Task _renderTask;

        public SpectreTracker(string description,Action<T> action)
        {
            _message = description;
            // 開啟背景工作來維持 Spectre 的進度條渲染
            _renderTask = Task.Run(() =>
            {
                AnsiConsole.Progress()
                    .AutoClear(false)
                    .Start(ctx =>
                    {
                        var task = ctx.AddTask(description);
                        while(!_isCompleted)
                        {
                            task.Value = _percentage * 100;
                            task.Description = _message;
                            Thread.Sleep(100); // 控制刷新率，避免過度消耗 CPU
                        }
                        task.StopTask();
                    });
            }).ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    
                }
            } , TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Update(double percentage , string? message = null)
        {
            _percentage = percentage;
            if(message != null)
            {
                _message = message;
            }
        }
        public void Complete(string? message = null)
        {
            if(message != null)
            {
                _message = message;
            }
            _isCompleted = true;
            _renderTask.Wait(); // 確保渲染區塊正常關閉
        }

        public void Dispose() => Complete();
    }
}

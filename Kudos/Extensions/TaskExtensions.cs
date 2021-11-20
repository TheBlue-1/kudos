#region

using Discord;
using Kudos.Bot;
using System;
using System.Threading.Tasks;

#endregion

namespace Kudos.Extensions {

    public static class TaskExtensions {

        public static void RunAsyncSave(this Func<Task> task, IMessageChannel channel = null, bool sendMessages = false) {
            Task.Run(async () => {
                try {
                    await task();
                } catch (Exception e) {
                    new ExceptionHandler(e, channel).Handle(sendMessages);
                }
            });
        }

        public static void RunAsyncSave(this Task task, IMessageChannel channel = null, bool sendMessages = false) {
            Task.Run(async () => {
                try {
                    await task;
                } catch (Exception e) {
                    new ExceptionHandler(e, channel).Handle(sendMessages);
                }
            });
        }

        public static void RunAsyncSave(this Action task, IMessageChannel channel = null, bool sendMessages = false) {
            Task.Run(() => {
                try {
                    task();
                } catch (Exception e) {
                    new ExceptionHandler(e, channel).Handle(sendMessages);
                }
            });
        }

        public static T WaitForResult<T>(this Task<T> task) {
            task.Wait();
            return task.Result;
        }
    }
}
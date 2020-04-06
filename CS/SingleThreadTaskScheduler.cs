using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfiniteAsyncSourceSingleThreadSample {
    public sealed class SingleThreadTaskScheduler : TaskScheduler {
        readonly CancellationToken cancellationToken;
        readonly BlockingCollection<Task> taskQueue;

        public SingleThreadTaskScheduler() {
            var cancellationSource = new CancellationTokenSource();
            cancellationToken = cancellationSource.Token;
            taskQueue = new BlockingCollection<Task>();

            new Thread(() => {
                try {
                    foreach(var task in taskQueue.GetConsumingEnumerable(cancellationToken)) {
                        TryExecuteTask(task);
                    }
                } catch(OperationCanceledException) { 
                } finally {
                }
            }) {
                Name = "SingleThreadTaskScheduler Thread"
            }.Start();
        }

        public void Complete() { 
            taskQueue.CompleteAdding(); 
        }

        protected override IEnumerable<Task> GetScheduledTasks() { return null; }
        protected override void QueueTask(Task task) {
            try {
                taskQueue.Add(task, cancellationToken);
            } catch(OperationCanceledException) { }
        }
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            return false;
        }
    }
}

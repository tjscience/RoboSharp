using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp
{
	public class PriorityScheduler : TaskScheduler
	{
		public static PriorityScheduler AboveNormal = new PriorityScheduler(ThreadPriority.AboveNormal);
		public static PriorityScheduler BelowNormal = new PriorityScheduler(ThreadPriority.BelowNormal);
		public static PriorityScheduler Lowest = new PriorityScheduler(ThreadPriority.Lowest);

		private BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
		private Thread[] _threads;
		private ThreadPriority _priority;
		private readonly int _maximumConcurrencyLevel = Math.Max(1, Environment.ProcessorCount);

		public PriorityScheduler(ThreadPriority priority)
		{
			_priority = priority;
		}

		public override int MaximumConcurrencyLevel
		{
			get { return _maximumConcurrencyLevel; }
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return _tasks;
		}

		protected override void QueueTask(Task task)
		{
			_tasks.Add(task);

			if (_threads == null)
			{
				_threads = new Thread[_maximumConcurrencyLevel];
				for (int i = 0; i < _threads.Length; i++)
				{
					int local = i;
					_threads[i] = new Thread(() =>
					{
						foreach (Task t in _tasks.GetConsumingEnumerable())
							base.TryExecuteTask(t);
					});
					_threads[i].Name = string.Format("PriorityScheduler: ", i);
					_threads[i].Priority = _priority;
					_threads[i].IsBackground = true;
					_threads[i].Start();
				}
			}
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return false; // we might not want to execute task that should schedule as high or low priority inline
		}
	}
}
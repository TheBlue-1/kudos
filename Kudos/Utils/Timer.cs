#region
using System;
using System.Threading.Tasks;
using Kudos.Bot;
using Kudos.DatabaseModels;
#endregion

namespace Kudos.Utils {
	public class Timer {
		private TimerData Data { get; }

		public Timer(TimerData data) {
			Data = data;
			TimerEnded += TimerEnd;
		}

		public event EventHandler<TimerData> TimerDead;
		private event EventHandler<TimerData> TimerEnded;
		public event EventHandler<TimerData> TimerEvent;

		private bool AdjustData() {
			if (Data.End >= DateTime.Now) {
				return true;
			}
			if (!(Data.Repeat is TimeSpan repeat)) {
				KillTimer();
				return false;
			}
			do {
				Data.End += repeat;
			} while (Data.End < DateTime.Now);
			return true;
		}

		private void EndTimer() {
			TimerEnded?.Invoke(this, Data);
		}

		private void KillTimer() {
			TimerDead?.Invoke(this, Data);
		}

		private void Run() {
			Task.Run(async () => {
				try {
					await Task.Delay(Data.End - DateTime.Now);
					EndTimer();
				}
				catch (Exception e) {
					new ExceptionHandler(e, null).Handle(false);
				}
			});
		}

		public void Start() {
			if (AdjustData()) {
				Run();
			}
		}

		private void TimerEnd(object sender, TimerData data) {
			TriggerTimerListener();
			if (AdjustData()) {
				Run();
			}
		}

		private void TriggerTimerListener() {
			TimerEvent?.Invoke(this, Data);
		}
	}
}

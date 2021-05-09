#region
using System;
using System.Threading;
using System.Threading.Tasks;
using Kudos.Bot;
using Kudos.DatabaseModels;
#endregion

namespace Kudos.Utils {
	public class Timer {
		private bool _dead;
		private CancellationTokenSource Canceler { get; } = new();
		private TimerData Data { get; }
		public ulong Id => Data.Id;

		public Timer(TimerData data) => Data = data;

		public event EventHandler<TimerData> SkippedTimerEvent;
		public event EventHandler<TimerData> TimerDataChanged;
		public event EventHandler<TimerData> TimerDead;
		public event EventHandler<TimerData> TimerEvent;

		private bool AdjustData(bool allowUnadjusted = true) {
			if (allowUnadjusted && Data.End > DateTime.UtcNow) {
				return true;
			}
			if (Data.Repeat <= TimeSpan.Zero) {
				KillTimer();
				return false;
			}
			do {
				Data.End += Data.Repeat;
			} while (Data.End < DateTime.UtcNow);
			DataChanged();
			return true;
		}

		private void DataChanged() {
			TimerDataChanged?.Invoke(this, Data);
		}

		private void EndTimer() {
			TriggerTimerListener();
			if (AdjustData(false)) {
				Run();
			}
		}

		public void Kill() {
			_dead = true;
			Canceler.Cancel();
			KillTimer();
		}

		private void KillTimer() {
			TimerDead?.Invoke(this, Data);
		}

		private void Run() {
			if (_dead) {
				return;
			}
			Task.Run(async () => {
				try {
					TimeSpan time = Data.End - DateTime.UtcNow;
					while (time.TotalMilliseconds > int.MaxValue) {
						await Task.Delay(int.MaxValue, Canceler.Token);
						time = Data.End - DateTime.UtcNow;
					}
					await Task.Delay(time, Canceler.Token);

					EndTimer();
				}
				catch (Exception e) {
					new ExceptionHandler(e, null).Handle(false);
				}
			}, Canceler.Token);
		}

		public void Start() {
			if (Data.End < DateTime.UtcNow) {
				TriggerSkippedTimerListener();
			}
			if (AdjustData()) {
				Run();
			}
		}

		private void TriggerSkippedTimerListener() {
			SkippedTimerEvent?.Invoke(this, Data);
		}

		private void TriggerTimerListener() {
			TimerEvent?.Invoke(this, Data);
		}
	}
}

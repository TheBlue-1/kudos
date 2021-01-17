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
		private CancellationTokenSource Canceler { get; } = new CancellationTokenSource();
		private TimerData Data { get; }
		public string Id => Data.Id;

		public Timer(TimerData data) {
			Data = data;
			TimerEnded += TimerEnd;
		}

		public event EventHandler<TimerData> SkippedTimerEvent;

		public event EventHandler<TimerData> TimerDataChanged;
		public event EventHandler<TimerData> TimerDead;
		private event EventHandler<TimerData> TimerEnded;
		public event EventHandler<TimerData> TimerEvent;

		private bool AdjustData() {
			if (Data.End > DateTime.Now) {
				return true;
			}
			if (Data.Repeat <= TimeSpan.Zero) {
				KillTimer();
				return false;
			}
			do {
				Data.End += Data.Repeat;
			} while (Data.End < DateTime.Now);
			DataChanged();
			return true;
		}

		private void DataChanged() {
			TimerDataChanged?.Invoke(this, Data);
		}

		private void EndTimer() {
			TimerEnded?.Invoke(this, Data);
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
					TimeSpan time = Data.End - DateTime.Now;
					while (time.TotalMilliseconds > int.MaxValue) {
						await Task.Delay(int.MaxValue);
						time = Data.End - DateTime.Now;
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
			if (Data.End < DateTime.Now) {
				TriggerSkippedTimerListener();
			}
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

		private void TriggerSkippedTimerListener() {
			SkippedTimerEvent?.Invoke(this, Data);
		}

		private void TriggerTimerListener() {
			TimerEvent?.Invoke(this, Data);
		}
	}
}

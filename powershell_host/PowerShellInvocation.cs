using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace shell.ViewModel
{
	/// <summary>
	/// PowerShell呼び出しのパック
	/// Windows PowerShell おけるコマンド入力から実行までの 1 サイクルの操作を公開します。
	/// </summary>
	public class PowerShellInvocation : IShellInvocation
	{
		#region フィールド
		/// <summary>
		/// 呼び出しを受け付けて発火するメソッド
		/// </summary>
		private readonly Action<PowerShellInvocation> invocationAction;

		/// <summary>
		/// スクリプトの履歴
		/// </summary>
		private readonly IReadOnlyList<string> history;

		/// <summary>
		/// カレントの履歴インデックス
		/// </summary>
		private int currentHistoryIndex;
		#endregion

		#region プロパティ
		/// <summary>
		/// ステータス
		/// </summary>
		public Constants.InvocationStatus Status
		{
			get { return this._status; }
			set
			{
				if (this._status != value)
				{
					this._status = value;
					this.RaisePropertyChanged();
				}
			}
		}
		private Constants.InvocationStatus _status = Constants.InvocationStatus.Ready;

		public InvocationResult Result
		{
			get { return this._result; }
			set
			{
				if (this._result != value)
				{
					this._result = value;
					this.RaisePropertyChanged();
					this.Status = Constants.InvocationStatus.Invoked;
				}
			}
		}
		private InvocationResult _result;

		/// <summary>
		/// 入力エリアに入力したスクリプト
		/// </summary>
		public string Script
		{
			get { return this._script; }
			set
			{
				if (this._script != value)
				{
					this._script = value;
					this.RaisePropertyChanged();
				}
			}
		}
		private string _script;

		/// <summary>
		/// プロンプトの番号
		/// </summary>
		public int Number { get; }
		#endregion

		#region コンストラクタ
		internal PowerShellInvocation(
			int p_number, 
			Action<PowerShellInvocation> p_invocationAction, 
			IReadOnlyList<string> p_history
			)
		{
			this.Number = p_number;
			this.invocationAction = p_invocationAction;
			this.history = p_history;
			this.currentHistoryIndex = this.history.Count;
		}
		#endregion

		#region メソッド
		public void Invoke()
		{
			this.Status = Constants.InvocationStatus.Invoking;
			this.invocationAction?.Invoke(this);
		}

		internal void SetResult(InvocationResult p_result)
		{
			this.Result = p_result;
			this.Status = Constants.InvocationStatus.Invoked;
		}

		public bool SetNextHistory()
		{
			var l_index = this.currentHistoryIndex - 1;
			if (l_index >= 0 && l_index < this.history.Count)
			{
				this.Script = this.history[l_index];
				this.currentHistoryIndex = l_index;
				return true;
			}

			return false;
		}

		public bool SetPreviousHistory()
		{
			var l_index = this.currentHistoryIndex + 1;
			if (l_index >= 0 && l_index < this.history.Count)
			{
				this.Script = this.history[l_index];
				this.currentHistoryIndex = l_index;
				return true;
			}

			this.Script = "";
			this.currentHistoryIndex = this.history.Count;
			return false;
		}
		#endregion

		#region INotifyPropertyChanged members
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}

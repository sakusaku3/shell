using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace shell.ViewModel
{
	public class PowerShellMessage : IShellInvocation
	{
		#region プロパティ
		/// <summary>
		/// ステータス
		/// </summary>
		public Constants.InvocationStatus Status { get; } = Constants.InvocationStatus.Invoked;

		/// <summary>
		/// 結果
		/// </summary>
		public InvocationResult Result { get; } 

		/// <summary>
		/// 番号
		/// </summary>
		int IShellInvocation.Number
		{
			get { throw new NotSupportedException(); }
		}

		/// <summary>
		/// スクリプト
		/// </summary>
		string IShellInvocation.Script
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		#endregion

		#region コンストラクタ
		public PowerShellMessage(string p_message)
		{
			this.Result = new InvocationResult(Constants.InvocationResultKind.Normal, p_message);
		}
		#endregion

		#region override メソッド
		bool IShellInvocation.SetNextHistory()
		{
			throw new NotSupportedException();
		}

		bool IShellInvocation.SetPreviousHistory()
		{
			throw new NotSupportedException();
		}

		void IShellInvocation.Invoke()
		{
			throw new NotSupportedException();
		}
		#endregion

		#region イベント
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { throw new NotSupportedException(); }
			remove { throw new NotSupportedException(); }
		}
		#endregion
	}
}

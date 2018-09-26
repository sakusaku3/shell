using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace shell.ViewModel
{
   	/// <summary>
	/// PowerShell呼び出し インターフェース
	/// Windows PowerShell おけるコマンド入力から実行までの 1 サイクルの操作を公開します。
	/// </summary>
	public interface IShellInvocation : INotifyPropertyChanged
	{
		#region プロパティ
		/// <summary>
		/// 呼び出しステータス
		/// </summary>
		Constants.InvocationStatus Status { get; }

		/// <summary>
		/// 呼び出し結果
		/// </summary>
		InvocationResult Result { get; }

		/// <summary>
		/// 呼び出し番号
		/// </summary>
		int Number { get; }

		string Script { get; set; }
		#endregion

		#region メソッド
		bool SetNextHistory();

		bool SetPreviousHistory();

		void Invoke();
		#endregion
	}
}

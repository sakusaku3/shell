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
	/// Shell呼び出し インターフェース
	/// </summary>
	public interface IShellInvocation : INotifyPropertyChanged
	{
		#region properties
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

		/// <summary>
		/// 実行するスクリプト
		/// </summary>
		string Script { get; set; }
		#endregion

		#region methods
		/// <summary>
		/// 次履歴をセット
		/// </summary>
		/// <returns></returns>
		bool SetNextHistory();

		/// <summary>
		/// 前履歴をセット
		/// </summary>
		/// <returns></returns>
		bool SetPreviousHistory();

		/// <summary>
		/// コマンド実行
		/// </summary>
		void Invoke();
		#endregion
	}
}

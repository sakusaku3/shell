using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shell.ViewModel
{
	/// <summary>
	/// 呼び出し結果クラス
	/// </summary>
    public class InvocationResult
    {
		#region properties
		/// <summary>
		/// 呼び出し結果種別
		/// </summary>
		public Constants.InvocationResultKind Kind { get; }

		/// <summary>
		/// メッセージ
		/// </summary>
		public string Message { get; }
		#endregion

		#region constructor
		public InvocationResult() : this(Constants.InvocationResultKind.Empty, null) { }

		public InvocationResult(Constants.InvocationResultKind kind, string message)
		{
			this.Kind = kind;
			this.Message = message;
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shell.ViewModel
{
	/// <summary>
	/// シェルホストのインターフェース
	/// </summary>
	public interface IShellHost
	{
		/// <summary>
		/// 呼び出しオブジェクトリスト
		/// </summary>
		ReadOnlyObservableCollection<IShellInvocation> Invocations { get; }
	}
}

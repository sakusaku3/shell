using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace shell.ViewModel
{
	/// <summary>
	/// PowerShellのホストクラス
	/// </summary>
	public class PowerShellHost : IShellHost, IDisposable
	{
		#region 定数
		/// <summary>
		/// エラーメッセージテンプレート
		/// </summary>
		private const string ErrorMessage = "{0}\r\n    + CategoryInfo          : {1}\r\n    + FullyQualifiedErrorId : {2}";

		/// <summary>
		/// 位置付きエラーメッセージテンプレート
		/// </summary>
		private const string ErrorMessageWithPosition = "{0}\r\n{1}\r\n    + CategoryInfo          : {2}\r\n    + FullyQualifiedErrorId : {3}";
		#endregion

		#region フィールド
		/// <summary>
		/// ランスペース
		/// </summary>
		/// <remarks>
		/// 制約付きの実行領域
		/// </remarks>
		private readonly Runspace runspace;

		/// <summary>
		/// コマンド履歴
		/// </summary>
		private readonly List<string> history = new List<string>();

		/// <summary>
		/// 呼び出しリスト
		/// </summary>
		private readonly ObservableCollection<IShellInvocation> invocations 
			= new ObservableCollection<IShellInvocation>();

		/// <summary>
		/// 叩いたコマンドの数
		/// </summary>
		private int count;

		/// <summary>
		/// 追加モジュールリスト
		/// </summary>
        private IEnumerable<string> additionalModules;
		#endregion

		#region プロパティ
		ReadOnlyObservableCollection<IShellInvocation> IShellHost.Invocations
			=> this._readonlyInvocations ?? (this._readonlyInvocations = new ReadOnlyObservableCollection<IShellInvocation>(this.invocations));

		private ReadOnlyObservableCollection<IShellInvocation> _readonlyInvocations;
		#endregion

		#region コンストラクタ
		public PowerShellHost(IEnumerable<string> additionalModules)
		{
			this.runspace = RunspaceFactory.CreateRunspace();
            this.additionalModules = additionalModules;
		}
		#endregion

		#region メソッド
		public void Open()
		{
			this.runspace.Open();

            this.Initialize();
			this.invocations.Add(new PowerShellMessage("Custom PowerShell Host - version 0.1"));
			this.invocations.Add(new PowerShellInvocation(++this.count, x => this.HandleInvocationRequested(x), this.history));
		}

		private void Initialize()
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.Runspace = this.runspace;

                foreach(var l_m in this.additionalModules)
                {
                    powershell.AddCommand("Import-Module").AddParameter("Name", l_m);
                    powershell.Invoke();
                }
            }
        }

        protected async void HandleInvocationRequested(PowerShellInvocation p_sender)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(p_sender.Script))
				{
					p_sender.SetResult(new InvocationResult());
				}
				else
				{
					using (var l_powershell = PowerShell.Create())
					{
						l_powershell.Runspace = this.runspace;
						l_powershell.AddScript(p_sender.Script);

						// ReSharper disable once AccessToDisposedClosure
						var l_results 
							= await Task.Factory
							.FromAsync(l_powershell.BeginInvoke(), x => l_powershell.EndInvoke(x))
							.ConfigureAwait(false);

						var l_error = this.CreateResultIfError(l_powershell);

						p_sender.SetResult(l_error ?? await this.HandleResult(l_results));
					}

					this.history.Add(p_sender.Script);
				}
			}
			catch (Exception l_ex)
			{
				this.createErrorMessage(l_ex);
			}
			finally
			{
				// 新しい呼び出しを作って終わる
				this.invocations.Add(new PowerShellInvocation(++this.count, x => this.HandleInvocationRequested(x), this.history));
			}
		}

		protected virtual Task<InvocationResult> HandleResult(PSDataCollection<PSObject> p_results)
		{
			return Task.Run(() => this.OutString(p_results));
		}

		protected InvocationResult OutString<T>(IEnumerable<T> p_input)
		{
			try
			{
				var l_sb = new StringBuilder();

				using (var l_powershell = PowerShell.Create())
				{
					l_powershell.Runspace = this.runspace;

					// 出力オブジェクトを文字列にする
					l_powershell.AddCommand("Out-String");

					foreach (var l_result in l_powershell.Invoke(p_input))
					{
						l_sb.AppendLine(l_result.ToString());
					}
				}

				return new InvocationResult(Constants.InvocationResultKind.Normal, l_sb.ToString());
			}
			catch (Exception l_ex)
			{
				return new InvocationResult(Constants.InvocationResultKind.Error, this.createErrorMessage(l_ex));
			}
		}

		protected InvocationResult CreateResultIfError(PowerShell p_powershell)
		{
			if (p_powershell.Streams.Error == null || p_powershell.Streams.Error.Count == 0) return null;

			var l_sb = new StringBuilder();
			foreach (var l_error in p_powershell.Streams.Error)
			{
				l_sb.AppendLine(string.Format(ErrorMessageWithPosition, l_error, l_error.InvocationInfo.PositionMessage, l_error.CategoryInfo, l_error.FullyQualifiedErrorId));
			}

			return new InvocationResult(Constants.InvocationResultKind.Error, l_sb.ToString());
		}

		/// <summary>
		/// エラーメッセージ生成
		/// </summary>
		/// <param name="p_ex">例外</param>
		/// <returns></returns>
		protected string createErrorMessage(Exception p_ex)
		{
			var l_container = p_ex as IContainsErrorRecord;
			if (l_container?.ErrorRecord == null)
			{
				return p_ex.Message;
			}

			var l_invocationInfo = l_container.ErrorRecord.InvocationInfo;

			if (l_invocationInfo == null)
			{
				return string.Format(ErrorMessage, l_container.ErrorRecord, l_container.ErrorRecord.CategoryInfo, l_container.ErrorRecord.FullyQualifiedErrorId);
			}

			if (l_invocationInfo.PositionMessage != null && ErrorMessage.IndexOf(l_invocationInfo.PositionMessage, StringComparison.Ordinal) != -1)
			{
				return string.Format(ErrorMessage, l_container.ErrorRecord, l_container.ErrorRecord.CategoryInfo, l_container.ErrorRecord.FullyQualifiedErrorId);
			}

			return string.Format(ErrorMessageWithPosition, l_container.ErrorRecord, l_invocationInfo.PositionMessage, l_container.ErrorRecord.CategoryInfo, l_container.ErrorRecord.FullyQualifiedErrorId);
		}
		#endregion

		#region IDispose override メソッド
		public void Dispose()
		{
			this.runspace?.Dispose();
		}
		#endregion
	}
}

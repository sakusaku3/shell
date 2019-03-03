using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using shell.ViewModel;

namespace myshell
{
	public class MyShellHost : IShellHost
	{
		public ReadOnlyObservableCollection<IShellInvocation> Invocations { get; }

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

		private readonly Type[] types = new Type[]
		{
			typeof(Options.CommandB),
			typeof(Options.CommandC),
		};

		public MyShellHost()
		{
			this.Invocations = new ReadOnlyObservableCollection<IShellInvocation>(this.invocations);
		}

		public void Open()
		{
			this.invocations.Add(new MyShellMessage("スタート"));
			this.invocations.Add(new MyShellInvocation(this.count++, x => this.HandleInvocationRequested(x), this.history));
		}

		protected void HandleInvocationRequested(MyShellInvocation p_sender)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(p_sender.Script))
				{
					p_sender.SetResult(new InvocationResult());
					return;
				}

				var l_args = p_sender.Script.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

				using (var l_parser = new Parser(x => x.HelpWriter = null))
				{
					var l_parsed = l_parser.ParseArguments(l_args, this.types);

					if (l_parsed.Tag == ParserResultType.Parsed)
					{
						if (l_parsed is Parsed<object> l_p && l_p.Value is ICommand l_c)
							p_sender.SetResult(l_c.Execute());

						this.history.Add(p_sender.Script);
					}
					else
					{
						var l_helpText = HelpText.AutoBuild(l_parsed);
						p_sender.SetResult(new InvocationResult(shell.Constants.InvocationResultKind.Error, l_helpText));
					}
				}
			}
			catch (Exception l_ex)
			{
				p_sender.SetResult(new InvocationResult(shell.Constants.InvocationResultKind.Error, l_ex.Message));
			}
			finally
			{
				// 新しい呼び出しを作って終わる
				this.invocations.Add(new MyShellInvocation(this.count++, x => this.HandleInvocationRequested(x), this.history));
			}
		}
	}
}

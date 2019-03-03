using CommandLine;
using shell.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshell.Options
{
	[Verb("sub", Hidden = true, HelpText = "ファイルアクセス")]
	class CommandB : ICommand
	{
		[Option('l', "list", Required = true, HelpText = "出力するファイル")]
		public IEnumerable<string> Args { get; set; }

		public InvocationResult Execute()
		{
			var l_message = string.Join(" ", this.Args);
			return new InvocationResult(shell.Constants.InvocationResultKind.Normal, l_message);
		}
	}
}

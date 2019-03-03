using CommandLine;
using shell.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshell.Options
{
	[Verb("CommandC", Hidden = false, HelpText = "ファイルアクセス")]
	class CommandC : ICommand
	{
		[Option('n', "nnn", Required = true, HelpText = "入力するファイル")]
		public string InputFile { get; set; }

		[Option('y', "yuu", Required = true, HelpText = "出力するファイル")]
		public string OutputFile { get; set; }

		public InvocationResult Execute()
		{
			var l_message = string.Join(" ", new string[] { this.InputFile, this.OutputFile });
			return new InvocationResult(shell.Constants.InvocationResultKind.Normal, l_message);
		}
	}
}

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshell.Options
{
	// using CommandLine;
	enum Hoge
	{
		X, Y, Z
	}
	class CommandA
	{
		// 基本的な形式
		[Option('a', "aaa", Required = false, HelpText = "AAAA")]
		public string A { get; set; }
		// プリミティブ型であれば、string以外でも受け取ることが可能
		[Option('b', "bbb", Required = false, HelpText = "BBBB")]
		public bool B { get; set; }
		// 複数の値を受け取ることが可能。区切り文字はSeparatorで指定
		[Option('c', "ccc", Separator = ',')]
		public IEnumerable<string> C { get; set; }
		// enumを受け取ることも可能(指定にはenumの名前を指定する)
		[Option('d', "ddd")]
		public Hoge D { get; set; }
		// オプション以外の引数を受け取るための属性
		[Value(1, MetaName = "remaining")]
		public IEnumerable<string> Remaining { get; set; }
	}
}

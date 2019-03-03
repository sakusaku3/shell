using shell.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshell
{
	interface ICommand
	{
		InvocationResult Execute();
	}
}

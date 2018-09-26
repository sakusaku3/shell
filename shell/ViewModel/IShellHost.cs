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
	public interface IShellHost
	{
		ReadOnlyObservableCollection<IShellInvocation> Invocations { get; }
	}
}

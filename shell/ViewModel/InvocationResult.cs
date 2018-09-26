using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shell.ViewModel
{
    public class InvocationResult
    {
        public Constants.InvocationResultKind Kind { get; }

		public string Message { get; }

		public InvocationResult() : this(Constants.InvocationResultKind.Empty, null) { }

		public InvocationResult(Constants.InvocationResultKind kind, string message)
		{
			this.Kind = kind;
			this.Message = message;
		}
	}
}

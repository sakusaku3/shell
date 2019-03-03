using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shell_view_test
{
    public class MainViewModel
    {
        public shell.ViewModel.PowerShellHost ShellHost { get; set; }

        public myshell.MyShellHost MyShellHost { get; set; }

        public MainViewModel()
        {
            var modules = new List<string>
            {
                typeof(cmdlets.GetPersonCmdlet).Assembly.Location
            };

			Console.WriteLine(string.Join(",", modules));

            this.ShellHost = new shell.ViewModel.PowerShellHost(modules);
            this.ShellHost.Open();

			this.MyShellHost = new myshell.MyShellHost();
        }
    }
}
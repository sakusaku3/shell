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

        public MainViewModel()
        {
            var l_modules = new List<string>
            {
                typeof(cmdlets.GetPersonCmdlet).Assembly.Location
            };

            this.ShellHost = new shell.ViewModel.PowerShellHost(l_modules);
            this.ShellHost.Open();
        }
    }
}
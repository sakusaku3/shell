using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Person")]
    public class GetPersonCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public int Count { get; set; }

        protected override void ProcessRecord()
        {
            var l_child = new List<Person>
            {
                new Person(1),
                new Person(3),
                new Person(4),
            };
            var l_persons = Enumerable.Range(0, this.Count)
                .Select(x => new { Name = $"person{x}", No = x, Children = l_child });

            foreach (var data in l_persons)
            {
                this.WriteObject(data);
            }
        }
    }

    public class Person
    {
        public int Age { get; }

        public Person(int age)
        {
            this.Age = age;
        }

        public override string ToString()
        {
            return $"Age:{this.Age}";
        }
    }
}
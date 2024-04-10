using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiVM.Tools
{
    internal class KoiVMToolOptions
    {
        [Option("-f", IsRequired =true, Description = "protected assembly file path")]
        public string AssemblyPath { get; set; }
    }
}

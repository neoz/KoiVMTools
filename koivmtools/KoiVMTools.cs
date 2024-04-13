using dnlib.DotNet;
using dnlib.DotNet.Writer;
using koivmtools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Tool.Interface;
using Tool.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace KoiVM.Tools
{
    internal class KoiVMTools : ITool<KoiVMToolOptions>
    {
        public string Title => null;

        public void Execute(KoiVMToolOptions settings)
        {
            Logger.Level = LogLevel.Verbose1;

            // set current directory to the directory of the assembly
            var refModule = Assembly.UnsafeLoadFrom(settings.AssemblyPath).ManifestModule;
            byte[] peImageOld = File.ReadAllBytes(settings.AssemblyPath);

            using (var module = ModuleDefMD.Load(peImageOld))
            {
                StringDecryptImpl.Execute(module, refModule);
                string newFilePath = PathInsertSuffix(settings.AssemblyPath, ".final");
                module.Write(newFilePath, new ModuleWriterOptions(module) { Logger = DnlibLogger.Instance });
            }

            Logger.Flush();
        }

        private static string PathInsertSuffix(string path, string suffix)
        {
            return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + suffix + Path.GetExtension(path));
        }
    }
}

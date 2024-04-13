using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tool.Logging;

namespace KoiVM.Tools
{
    public static class StringDecryptImpl
    {
        public static int FindDecryptMethod(ModuleDef module)
        {
            Logger.Info("Find decrypt string method");
            if (module is null)
                throw new ArgumentNullException(nameof(module));

            // make sure entrypoint is not null
            if (module.EntryPoint is null)
                throw new ArgumentNullException(nameof(module.EntryPoint));

            var entryPoint = module.EntryPoint;
            // has body and instructions
            if (entryPoint.HasBody && entryPoint.Body.HasInstructions)
            {
                var instructions = entryPoint.Body.Instructions;
                // Loop entrypoint instructions to find the decrypt method
                for (int i = 0; i < instructions.Count; i++)
                {
                    if(instructions[i].OpCode.Code == dnlib.DotNet.Emit.Code.Ldstr &&
                                instructions[i + 1].OpCode.Code == dnlib.DotNet.Emit.Code.Ldloc_S &&
                                    instructions[i + 2].OpCode.Code == dnlib.DotNet.Emit.Code.Call)
                    {
                        var calltarget = instructions[i + 2].Operand as MethodDef;
                        if (calltarget == null)
                            continue;

                        if (!calltarget.IsStatic)
                            continue;

                        if (calltarget.Parameters.Count != 2)
                            continue;

                        if (calltarget.Parameters[0].Type.FullName != "System.String")
                            continue;

                        if (calltarget.Parameters[1].Type.FullName != "System.Int32")
                            continue;

                        if (calltarget.ReturnType.FullName != "System.String")
                            continue;

                        Logger.Info($"--> Found decrypt method {calltarget.FullName} 0x{calltarget.MDToken:x}");
                        return calltarget.MDToken.ToInt32();
                    }
                }
            }
            
            throw new Exception("Could not find decrypt string method");
            
        }
        public static int Execute(ModuleDef module, Module reflModule)
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module));
            if (reflModule is null)
                throw new ArgumentNullException(nameof(reflModule));

            var decryptMethodToken = FindDecryptMethod(module);
            // Find the decrypt method string(string, int)
            var decryptMethod = reflModule.ResolveMethod(decryptMethodToken); // reflModule.ResolveMethod(0x060003B3);

            foreach(var type in module.GetTypes())
            {
                foreach (var method in type.Methods) 
                {
                    //if (method.MDToken.Raw != 0x06000015)
                    //    continue;

                    var printMethod = false;
                    
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        var instructions = method.Body.Instructions;

                        var key = 0;
                        var str = "";
                        if (instructions[0].IsLdcI4())
                            key = instructions[0].GetLdcI4Value();
                        else
                            continue;

                        for (int i = 0; i < instructions.Count; i++)
                        {
                            var instr = instructions[i];
                            if (instructions[i].OpCode.Code == dnlib.DotNet.Emit.Code.Ldstr &&
                                instructions[i + 1].OpCode.Code == dnlib.DotNet.Emit.Code.Ldloc_S &&
                                    instructions[i + 2].OpCode.Code == dnlib.DotNet.Emit.Code.Call)
                            {
                                str = (string)instructions[i].Operand;
                                var callmethod = instructions[i + 2].Operand as MethodDef;
                                if (callmethod!=null && callmethod.MDToken.Raw == decryptMethod.MetadataToken)
                                {
                                    if (!printMethod)
                                    {
                                        Logger.Info($"Method {method.Name} {method.MDToken}");
                                        printMethod = true;
                                    }
                                    var decrypted = decryptMethod.Invoke(null,new object[] { str, key });
                                    Logger.Info($"--> Decrypted String {str} => {decrypted}");

                                    instructions[i].Operand = decrypted;
                                    instructions[i+1].OpCode = dnlib.DotNet.Emit.OpCodes.Nop;
                                    instructions[i+2].OpCode = dnlib.DotNet.Emit.OpCodes.Nop;
                                }
                            }
                        }
                    }
                    
                }
            }
            
            return 0;
        }
    }
}

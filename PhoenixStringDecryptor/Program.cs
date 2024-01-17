using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System.IO;

namespace StringDecryptorBlTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ModuleDefMD module = ModuleDefMD.Load("Path Of Target Exe");

            foreach (TypeDef type in module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.HasBody)
                    {
                        method.Body.KeepOldMaxStack = true;

                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            Instruction instruction = method.Body.Instructions[i];

                            if (instruction.OpCode == OpCodes.Call && instruction.Operand is IMethod && ((IMethod)instruction.Operand).Name == "?1?")
                            {
                                Instruction previousInstruction = method.Body.Instructions[i - 1];
                                if (previousInstruction.OpCode == OpCodes.Ldstr)
                                {
                                    string encryptedString = (string)previousInstruction.Operand;

                                    string decryptedString = Decrypt(encryptedString);

                                    previousInstruction.Operand = decryptedString;

                                    instruction.OpCode = OpCodes.Nop;
                                    instruction.Operand = null;
                                }
                            }
                        }
                    }
                }
            }

            string directoryPath = "OutPut Path";
            string filePath = Path.Combine(directoryPath, "modified_assembly.exe");

            Directory.CreateDirectory(directoryPath);

            module.Write(filePath);
        }

        public static string Decrypt(string encrypted)
        {
            int length = encrypted.Length;
            char[] array = new char[length];
            for (int i = 0; i < array.Length; i++)
            {
                char c = encrypted[i];
                byte lowerByte = (byte)c;
                byte higherByte = (byte)(c >> 8);
                byte decryptedLowerByte = (byte)(lowerByte ^ (length - i));
                byte decryptedHigherByte = (byte)(higherByte ^ i);
                array[i] = (char)((decryptedHigherByte << 8) | decryptedLowerByte);
            }
            return new string(array);
        }
    }
}

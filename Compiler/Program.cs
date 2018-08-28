using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Compiler
{
    public static class Program
    {
        public static readonly string Root = "C:\\Users\\Lomztein\\Documents\\GitHub\\ModularDiscordBot\\Modules\\";
        public static readonly string Target = "C:\\Users\\Lomztein\\Documents\\GitHub\\ModularDiscordBot\\Compiled Modules\\";

        public static readonly string[] ToIgnore = new string[] { "bin", "obj" };

        public static void Main(string[] args)
        {
            string moduleRootDir = Root;

            string[] subfolders = Directory.GetDirectories (moduleRootDir);
            subfolders = subfolders.Where (x => !ToIgnore.Contains (Path.GetFileName (x))).ToArray ();

            foreach (string folder in subfolders) {
                Console.WriteLine ($"Compiling: {folder}..");

                string outName = Target + Path.GetFileName (folder);
                Directory.SetCurrentDirectory (folder);

                ProcessStartInfo processStart = new ProcessStartInfo ("cmd.exe", $"csc -out:{outName} -target:library -recurse:*.cs");
                Process compileProcess = Process.Start (processStart);

                Console.WriteLine ($"{folder} succesfully compiled.");
            }

            Console.ReadKey ();
        }
    }
}

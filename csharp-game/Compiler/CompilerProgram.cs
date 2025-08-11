using System;
using System.IO;
using PixelManGame.Compiler;

namespace PixelManGame.Compiler
{
    class CompilerProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PixelMan C# to Bytecode Compiler");
            Console.WriteLine("==================================");

            var sourceFiles = new string[]
            {
                "GameConfig.cs",
                "Scene.cs", 
                "GameObject.cs",
                "Player.cs",
                "Scenes/Boot.cs",
                "Scenes/Preloader.cs",
                "Scenes/MainScene.cs"
            };

            var compiler = new CSharpCompiler();
            
            try
            {
                Console.WriteLine("Compiling C# source files...");
                var bytecode = compiler.CompileToJson(sourceFiles);
                
                var outputPath = "../compiled-game/pixelman-bytecode.json";
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, bytecode);
                
                Console.WriteLine($"Compilation successful! Bytecode written to: {outputPath}");
                Console.WriteLine($"Bytecode size: {bytecode.Length} characters");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Compilation failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

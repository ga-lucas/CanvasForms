using Mono.Cecil;

internal static class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine("Canvas.Windows.Forms.ILTranslator starting...");
        Console.WriteLine($"Args: {string.Join(" | ", args.Select(a => $"'{a}'"))}");

        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: Canvas.Windows.Forms.ILTranslator <input-assembly-path> <output-assembly-path>");
            return 2;
        }

        var inputPath = Path.GetFullPath(args[0]);
        var outputPath = Path.GetFullPath(args[1]);

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input assembly not found: {inputPath}");
            return 3;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Environment.CurrentDirectory);

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(inputPath)!);
        resolver.AddSearchDirectory(AppContext.BaseDirectory);

        var readerParameters = new ReaderParameters
        {
            ReadWrite = false,
            ReadSymbols = File.Exists(Path.ChangeExtension(inputPath, ".pdb")),
            AssemblyResolver = resolver
        };

        try
        {
            using var module = ModuleDefinition.ReadModule(inputPath, readerParameters);
            var updated = 0;

            foreach (var reference in module.AssemblyReferences)
            {
                if (reference.Name == "System.Windows.Forms")
                {
                    reference.Name = "Canvas.Windows.Forms";
                    updated++;
                }
               else if (reference.Name == "System.Windows.Forms.Primitives")
                {
                    // Modern WinForms splits some surface area into System.Windows.Forms.Primitives.
                    // Canvas.Windows.Forms implements WinForms types in the System.Windows.Forms namespace,
                    // so we retarget this reference to the same assembly.
                    reference.Name = "Canvas.Windows.Forms";
                    updated++;
                }
                else if (reference.Name == "WebForms.Canvas")
                {
                    reference.Name = "Canvas.Windows.Forms";
                    updated++;
                }
            }

            // Minimal PoC: only retarget the referenced assembly identity.
            // This is sufficient for most WinForms apps since types live in the System.Windows.Forms namespace,
            // but were previously resolved from the framework System.Windows.Forms assembly.
            //
            // Future enhancements:
            // - rewrite System.Drawing to Canvas.Windows.Forms.Drawing where feasible
            // - stub/redirect unsupported Win32 APIs
            // - patch Application.Run entrypoints for browser/remote hosting

            var writerParameters = new WriterParameters
            {
                WriteSymbols = readerParameters.ReadSymbols
            };

            Console.WriteLine($"Writing translated module to: {outputPath}");
            module.Write(outputPath, writerParameters);
            Console.WriteLine($"Wrote: {outputPath} (exists={File.Exists(outputPath)})");

            Console.WriteLine($"Translated '{Path.GetFileName(inputPath)}' -> '{Path.GetFileName(outputPath)}' (updated refs: {updated})");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }
}

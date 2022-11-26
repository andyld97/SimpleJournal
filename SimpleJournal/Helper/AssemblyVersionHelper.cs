using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace SimpleJournal.Helper
{
    public static class AssemblyVersionHelper
    {

        private static readonly List<Type> types = new List<Type>()
        {
            typeof(SimpleJournal.Common.Strings),
            typeof(SimpleJournal.Documents.Consts),
            typeof(SimpleJournal.Documents.PDF.PdfConverter),
            typeof(SimpleJournal.Documents.UI.Consts),
            typeof(SimpleJournal.SharedResources.Dummy),
            typeof(SimpleJournal.MainWindow),
        };

        private static readonly List<string> fileAssemblies = new List<string>()
        {
#if !UWP
            Consts.TouchExecutable,
#endif
            SimpleJournal.Documents.UI.Consts.AnalyzerPath
        };

        private static readonly List<Type> externalDependencies = new List<Type>()
        {
            typeof(ControlzEx.WindowChrome),
            typeof(Fluent.Ribbon),
            typeof(MahApps.Metro.Controls.SplitButton),
            typeof(ImageMagick.MagickColor),
        };

        private static readonly Dictionary<string, Version> externalStaticVersions = new Dictionary<string, Version>()
        {
            { "DotnetRuntimeBootstrapper", new Version(2, 3, 1) }
        };

        public static List<Inline> GenerateAssemblyText()
        {
            List<Inline> inlines = new List<Inline>();

            // SimpleJournal Assemblies
            inlines.Add(new Run() { Text = "SimpleJournal Assemblies:", FontWeight = FontWeights.Bold });
            inlines.Add(new LineBreak());

            foreach (var sjAssembly in types)
            {
                inlines.Add(new Run() { Text = GetAssemblyString(sjAssembly) });
                inlines.Add(new LineBreak());
            }

            // File Assemblies
            foreach (var fileAssembly in fileAssemblies)
            {
                if (System.IO.File.Exists(fileAssembly))
                {
                    try
                    {
                        // Get file assembly version
                        var versionInfo = FileVersionInfo.GetVersionInfo(fileAssembly);
                        if (versionInfo != null)
                        {
                            inlines.Add(new Run() { Text = GetAssemblyString(fileAssembly, versionInfo.FileVersion) });
                            inlines.Add(new LineBreak());
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            inlines.Add(new LineBreak());
            // External Assemblies
            inlines.Add(new Run() { Text = "External Assemblies:", FontWeight = FontWeights.Bold });
            inlines.Add(new LineBreak());

            foreach (var externalAssembly in externalDependencies)
            {
                inlines.Add(new Run() { Text = GetAssemblyString(externalAssembly) });
                inlines.Add(new LineBreak());
            }

            foreach (var externalAssembly in externalStaticVersions)
            {
                inlines.Add(new Run() { Text = GetAssemblyString(externalAssembly.Key, externalAssembly.Value.ToString(3)) });
                inlines.Add(new LineBreak());
            }

            return inlines;
        }

        private static string GetAssemblyString(Type type)
        {
            var assemblyName = type.Assembly.GetName();
            return $"{assemblyName.Name} (Version {assemblyName.Version.ToString(4)})";
        }

        private static string GetAssemblyString(string fileName, string version)
        {
            return $"{System.IO.Path.GetFileName(fileName)} (Version {version})";
        }
    }
}

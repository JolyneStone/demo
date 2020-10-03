using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace CodeGenerator.Razor
{
    public abstract class RoslynCompiler
    {
        protected static readonly IList<MetadataReference> _applicationReferences = new List<MetadataReference>();
        public abstract Task<RazorPageViewBase<TModel>> CompileAsync<TModel>(string code, string className, string[] namespaces);

        static RoslynCompiler()
        {
            InitApplicationReferences();
        }

        private static void InitApplicationReferences()
        {
            if (_applicationReferences != null && _applicationReferences.Any())
            {
                return;
            }

            var metadataReferences = new List<MetadataReference>();
            var entryAssembly = Assembly.GetEntryAssembly();
            var refMvcAssemblyNames = typeof(RoslynCompiler).Assembly.GetReferencedAssemblies();
            var refAssembies = entryAssembly.GetReferencedAssemblies().Except(refMvcAssemblyNames, new AssemblyNameCompare());
            refAssembies = refAssembies.Union(DependencyContext.Default.CompileLibraries.Where(lib => !lib.Serviceable).Select(lib => new AssemblyName(lib.Name)), new AssemblyNameCompare());
            foreach (var assemblyName in refAssembies)
            {
                var assembly = Assembly.Load(assemblyName);
                _applicationReferences.Add(CreateMetadataFileReference(assembly.Location));
            }

            _applicationReferences.Add(CreateMetadataFileReference(entryAssembly.Location));
        }

        private static MetadataReference CreateMetadataFileReference(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);
                return assemblyMetadata.GetReference(filePath: path);
            }
        }

        private class AssemblyNameCompare : IEqualityComparer<AssemblyName>
        {
            public bool Equals(AssemblyName x, AssemblyName y)
            {
                return x.FullName == y.FullName;
            }

            public int GetHashCode(AssemblyName obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}

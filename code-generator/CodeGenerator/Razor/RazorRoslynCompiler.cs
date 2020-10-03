using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Razor
{
    /// <summary>
    /// 利用Roslyn进行动态编译
    /// </summary>
    public class RazorRoslynCompiler : RoslynCompiler
    {
        /// <summary>
        /// 编译Razor代码，最终生成一个代表Razor的类
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override async Task<RazorPageViewBase<TModel>> CompileAsync<TModel>(string code, string className, string[] namespaces)
        {
            var defaultNamespaces = new List<string> { "System", "System.Threading.Tasks", "CodeGenerator", "CodeGenerator.Razor" };

            if (namespaces != null && namespaces.Length > 0)
            {
                defaultNamespaces.AddRange(namespaces.Except(namespaces));
            }

            var options = ScriptOptions.Default
                .AddImports(defaultNamespaces)
                .AddReferences(_applicationReferences);


            var result = CSharpScript.Create(code, options)
                .ContinueWith($"new {className}()");
            try
            {
                var value = (await result.RunAsync()).ReturnValue;
                var view = value as RazorPageViewBase<TModel>;
                return view;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}

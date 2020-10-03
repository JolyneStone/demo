using System.Threading.Tasks;

namespace CodeGenerator
{
    public interface ICodeCompiler<TConfig> where TConfig : ModelConfig
    {
        Task GenerateAsync(TConfig config);
    }
}

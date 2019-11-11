using Khooversoft.Toolbox.Standard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configuration.Repository
{
    public interface IConfigurationRepository
    {
        Task Delete(IWorkContext context, string key);

        Task DeleteAllRecords(IWorkContext context);

        Task<IReadOnlyList<KeyValuePair<string, string>>> List(IWorkContext context, string nameSpace);

        Task Open(IWorkContext context);

        Task<KeyValuePair<string, string>?> Get(IWorkContext context, string key);

        Task Set(IWorkContext context, string key, string value);
    }
}
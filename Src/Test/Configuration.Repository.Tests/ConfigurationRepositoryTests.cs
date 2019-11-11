using System;
using Xunit;
using Configuration.Repository;
using Khooversoft.Toolbox.Standard;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace Configuration.Repository.Tests
{
    public class ConfigurationRepositoryTests
    {
        private readonly IConfigurationRepository _repository;
        private readonly IWorkContext _workContext = WorkContext.Empty;

        public ConfigurationRepositoryTests()
        {
            _repository = new ConfigurationRepository("DefaultEndpointsProtocol=https;AccountName=eventmeshstorage;AccountKey=PYFZKB58LIWTYRkZIOoS2rNEoNJYx5YNCwdDaid+DCZB0DeTtgXX4t2BeCCRbiinmONl8YSdTRuSbKeUFSYa7g==;EndpointSuffix=core.windows.net");
        }

        [Fact]
        public async Task GivenTestStorageTable_WhenInitialize_ShouldWork()
        {
            await _repository.Open(_workContext);
            await _repository.DeleteAllRecords(_workContext);
        }

        [Fact]
        public async Task GivenTestStorageTable_WhenWriteReadDeleteForSingleNamespace_ShouldWork()
        {
            const int max = 10;
            await _repository.Open(_workContext);
            await _repository.DeleteAllRecords(_workContext);

            const string nameSpace = "NS";
            var data = Enumerable.Range(0, max)
                .Select(x => new KeyValuePair<string, string>($"{nameSpace}/ConfigKey_{x}", $"Value_{x}"))
                .ToList();

            foreach(var item in data)
            {
                await _repository.Set(_workContext, item.Key, item.Value);
            }

            IReadOnlyList<KeyValuePair<string, string>> list = await _repository.List(_workContext, nameSpace);
            list.Should().NotBeNull();
            list.Count.Should().Be(data.Count);

            list.OrderBy(x => x.Key)
                .Zip(list.OrderBy(x => x.Key), (o, i) => (o, i))
                .All(x => x.o.Key == x.i.Key && x.o.Value == x.i.Value)
                .Should().BeTrue();

            foreach (var item in data)
            {
                await _repository.Delete(_workContext, item.Key);
            }

            list = await _repository.List(_workContext, nameSpace);
            list.Should().NotBeNull();
            list.Count.Should().Be(0);
        }
    }
}

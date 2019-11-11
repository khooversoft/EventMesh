using Khooversoft.Toolbox.Standard;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configuration.Repository
{
    internal class ConfigurationEntity : TableEntity
    {
        const string _slash = "/";
        const string _dash = "-";
        private string _key;

        public ConfigurationEntity()
        {
        }

        public ConfigurationEntity(string key)
        {
            key.Verify(nameof(key)).IsNotEmpty();

            var sv = new StringVector(key, _slash);
            sv.HasRoot.Verify(nameof(sv.HasRoot)).Assert(x => x == false, "Key cannot have a root");
            sv.Count.Verify(nameof(sv.Count)).Assert(x => x > 0, "Must have one path in key");

            var partitionSv = new StringVector(sv, _dash, false);

            Key = sv;
            RowKey = partitionSv;

            PartitionKey = partitionSv[0];
        }

        public ConfigurationEntity(string key, string value)
            : this(key)
        {
            value.Verify(nameof(value)).IsNotEmpty();

            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}

/* Copyright © 2020 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

namespace Umbraco.Community.Contentment.DataEditors
{
    public class DataListConfiguration
    {
        public IDataListSource DataSource { get; internal set; }

        public Dictionary<string, object> DataSourceConfig { get; internal set; }

        public IDataListEditor ListEditor { get; internal set; }

        public Dictionary<string, object> ListEditorConfig { get; internal set; }

        public IDataListValueConverter ValueConverter { get; internal set; }

        public Dictionary<string, object> ValueConverterConfig { get; internal set; }

        public static DataListConfiguration FromConfiguration(ConfigurationEditorUtility utility, Dictionary<string, object> configuration)
        {
            var self = new DataListConfiguration();

            if (configuration.TryGetValueAs(DataListConfigurationEditor.DataSource, out JArray array1) && array1.Count > 0 && array1[0] is JObject obj1)
            {
                // NOTE: Patches a breaking-change. I'd renamed `type` to become `key`. [LK:2020-04-03]
                if (obj1.ContainsKey("key") == false && obj1.ContainsKey("type"))
                {
                    obj1.Add("key", obj1["type"]);
                    obj1.Remove("type");
                }

                self.DataSource = utility.GetConfigurationEditor<IDataListSource>(obj1.Value<string>("key"));
                self.DataSourceConfig = obj1["value"].ToObject<Dictionary<string, object>>();
            }

            if (configuration.TryGetValueAs(DataListConfigurationEditor.ListEditor, out JArray array2) && array2.Count > 0 && array2[0] is JObject obj2)
            {
                // NOTE: Patches a breaking-change. I'd renamed `type` to become `key`. [LK:2020-04-03]
                if (obj2.ContainsKey("key") == false && obj2.ContainsKey("type"))
                {
                    obj2.Add("key", obj2["type"]);
                    obj2.Remove("type");
                }

                self.ListEditor = utility.GetConfigurationEditor<IDataListEditor>(obj2.Value<string>("key"));
                self.ListEditorConfig = obj2["value"].ToObject<Dictionary<string, object>>();
            }

            if (configuration.TryGetValueAs(DataListConfigurationEditor.ValueConverter, out JArray array3) && array3.Count > 0 && array3[0] is JObject obj3)
            {
                self.ValueConverter = utility.GetConfigurationEditor<IDataListValueConverter>(obj3.Value<string>("key"));
                self.ValueConverterConfig = obj3["value"].ToObject<Dictionary<string, object>>();
            }

            return self;
        }
    }
}

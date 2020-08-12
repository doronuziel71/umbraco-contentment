﻿/* Copyright © 2019 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Community.Contentment.DataEditors
{
    internal sealed class DataListConfigurationEditor : ConfigurationEditor
    {
        internal const string DataSource = "dataSource";
        internal const string Items = "items";
        internal const string ListEditor = "listEditor";
        internal const string EditorConfig = "editorConfig";
        internal const string EditorView = "editorView";
        internal const string ValueConverter = "valueConverter";

        private readonly ConfigurationEditorUtility _utility;

        public DataListConfigurationEditor(ConfigurationEditorUtility utility)
            : base()
        {
            _utility = utility;

            var configEditorViewPath = IOHelper.ResolveUrl(ConfigurationEditorDataEditor.DataEditorViewPath);
            var defaultConfigEditorConfig = new Dictionary<string, object>
            {
                { MaxItemsConfigurationField.MaxItems, 1 },
                { DisableSortingConfigurationField.DisableSorting, Constants.Values.True },
                { Constants.Conventions.ConfigurationFieldAliases.OverlayView, IOHelper.ResolveUrl(ConfigurationEditorDataEditor.DataEditorOverlayViewPath) },
                { EnableDevModeConfigurationField.EnableDevMode, Constants.Values.True },
            };

            var dataSources = utility.GetConfigurationEditorModels<IDataListSource>();
            var listEditors = utility.GetConfigurationEditorModels<IDataListEditor>();
            var valueConverters = utility.GetConfigurationEditorModels<IDataListValueConverter>();

            Fields.Add(
                DataSource,
                "Data source",
                "Select and configure a data source.",
                configEditorViewPath,
                new Dictionary<string, object>(defaultConfigEditorConfig)
                {
                    { Constants.Conventions.ConfigurationFieldAliases.Items, dataSources }
                });

            Fields.Add(
                ListEditor,
                "List editor",
                "Select and configure a list editor.",
                configEditorViewPath,
                new Dictionary<string, object>(defaultConfigEditorConfig)
                {
                    { Constants.Conventions.ConfigurationFieldAliases.Items, listEditors }
                });

            Fields.Add(
                ValueConverter,
                "Value converter",
                "<i>(Advanced)</i> Select and configure a value converter.<br><br>If no converter is configured, the returned value will be a default <code>string</code> value.",
                configEditorViewPath,
                new Dictionary<string, object>(defaultConfigEditorConfig)
                {
                    { Constants.Conventions.ConfigurationFieldAliases.Items, valueConverters }
                });
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var config = base.ToValueEditor(configuration);

            var toValueEditor = new Dictionary<string, object>();

            if (config.TryGetValueAs(DataSource, out JArray array1) && array1.Count > 0 && array1[0] is JObject item1)
            {
                // NOTE: Patches a breaking-change. I'd renamed `type` to become `key`. [LK:2020-04-03]
                if (item1.ContainsKey("key") == false && item1.ContainsKey("type"))
                {
                    item1.Add("key", item1["type"]);
                    item1.Remove("type");
                }

                var source = _utility.GetConfigurationEditor<IDataListSource>(item1.Value<string>("key"));
                if (source != null)
                {
                    var sourceConfig = item1["value"].ToObject<Dictionary<string, object>>();
                    var items = source?.GetItems(sourceConfig) ?? Enumerable.Empty<DataListItem>();

                    toValueEditor.Add(Items, items);
                }
            }

            if (config.TryGetValueAs(ListEditor, out JArray array2) && array2.Count > 0 && array2[0] is JObject item2)
            {
                // NOTE: Patches a breaking-change. I'd renamed `type` to become `key`. [LK:2020-04-03]
                if (item2.ContainsKey("key") == false && item2.ContainsKey("type"))
                {
                    item2.Add("key", item2["type"]);
                    item2.Remove("type");
                }

                var editor = _utility.GetConfigurationEditor<IDataListEditor>(item2.Value<string>("key"));
                if (editor != null)
                {
                    var editorConfig = item2["value"].ToObject<Dictionary<string, object>>();

                    foreach (var prop in editorConfig)
                    {
                        if (toValueEditor.ContainsKey(prop.Key) == false)
                        {
                            toValueEditor.Add(prop.Key, prop.Value);
                        }
                    }

                    if (editor.DefaultConfig != null)
                    {
                        foreach (var prop in editor.DefaultConfig)
                        {
                            if (toValueEditor.ContainsKey(prop.Key) == false)
                            {
                                toValueEditor.Add(prop.Key, prop.Value);
                            }
                        }
                    }
                }
            }

            return toValueEditor;
        }
    }
}

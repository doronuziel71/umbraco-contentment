/* Copyright © 2020 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Community.Contentment.DataEditors
{
    public sealed class UniversalDataListValueConverter : IDataListValueConverter
    {
        private readonly Dictionary<string, Type> _objectTypes;
        private readonly Dictionary<string, string> _customLabels;
        private readonly IEnumerable<DataListItem> _items;

        public UniversalDataListValueConverter()
        {
            _objectTypes = new Dictionary<string, Type>
            {
                { nameof(Object), typeof(object) },
                { nameof(Boolean), typeof(bool) },
                { nameof(Char), typeof(char) },
                { nameof(DateTime), typeof(DateTime) },
                { nameof(Decimal), typeof(decimal) },
                { nameof(Double), typeof(double) },
                { nameof(Single), typeof(float) },
                { nameof(Int16), typeof(short) },
                { nameof(Int32), typeof(int) },
                { nameof(Int64), typeof(long) },
                { nameof(Guid), typeof(Guid) },
                { nameof(String), typeof(string) },
            };

            _customLabels = new Dictionary<string, string>
            {
                { nameof(Object), "Object (string)" },
                { nameof(Boolean), "Boolean (bool)" },
                { nameof(Int16), "Int 16 (short)" },
                { nameof(Int32), "Int 32 (int)" },
                { nameof(Int64), "Int 64 (long)" },
                { nameof(Single), "Single (float)" },
            };

            _items = _objectTypes.Keys.Select(x => new DataListItem
            {
                Name = _customLabels.ContainsKey(x) ? _customLabels[x] : x.SplitPascalCasing(),
                Value = x
            });
        }

        public string Name => "Universal Type Converter";

        public string Description => "Converts the value to the specified object-type.";

        public string Icon => "icon-globe-alt";

        public OverlaySize OverlaySize => OverlaySize.Small;

        public Dictionary<string, object> DefaultValues => new Dictionary<string, object>
        {
            { "objectType", nameof(String) },
        };

        public IEnumerable<ConfigurationField> Fields => new ConfigurationField[]
        {
            new ConfigurationField
            {
                Key = "objectType",
                Name = "Object type",
                Description = "Select the object type to convert to.",
                View = IOHelper.ResolveUrl(DropdownListDataListEditor.DataEditorViewPath),
                Config = new Dictionary<string, object>()
                {
                    { DropdownListDataListEditor.AllowEmpty, Constants.Values.False },
                    { Constants.Conventions.ConfigurationFieldAliases.Items, _items },
                }
            }
        };

        public Type GetValueType(DataListConfiguration config)
        {
            return config.ValueConverterConfig != null && config.ValueConverterConfig.TryGetValueAs("objectType", out string objectType) == true && _objectTypes.ContainsKey(objectType)
                ? _objectTypes[objectType]
                : _objectTypes[nameof(String)];
        }

        public object ConvertValue(DataListConfiguration config, Type type, string value)
        {
            // NOTE: Return the value directly, as `DataListValueConverter.ConvertIntermediateToObject` will attempt to use `TryConvertTo` to ensure the correct type.
            return value;
        }
    }
}

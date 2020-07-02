/* Copyright © 2019 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Community.Contentment.DataEditors
{
    public sealed class DataListValueConverter : PropertyValueConverterBase
    {
        private readonly ConfigurationEditorUtility _utility;

        public DataListValueConverter(ConfigurationEditorUtility utility)
            : base()
        {
            _utility = utility;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType) => propertyType.EditorAlias.InvariantEquals(DataListDataEditor.DataEditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            var defaultValueType = typeof(string);

            if (propertyType.DataType.Configuration is Dictionary<string, object> configuration)
            {
                var dataListConfig = DataListConfiguration.FromConfiguration(_utility, configuration);

                var valueType = default(Type);
                var hasMultipleValues = false;

                if (dataListConfig.ListEditor != null)
                {
                    hasMultipleValues = dataListConfig.ListEditor.HasMultipleValues(dataListConfig.ListEditorConfig);
                }

                if (dataListConfig.ValueConverter != null)
                {
                    valueType = dataListConfig.ValueConverter.GetValueType(dataListConfig);
                }

                return hasMultipleValues == true
                    ? typeof(IEnumerable<>).MakeGenericType(valueType ?? defaultValueType)
                    : valueType ?? defaultValueType;
            }

            return defaultValueType;
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source is string value)
            {
                if (value.DetectIsJson() == false)
                {
                    return value;
                }

                return JsonConvert.DeserializeObject<IEnumerable<string>>(value);
            }

            return base.ConvertSourceToIntermediate(owner, propertyType, source, preview);
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (propertyType.DataType.Configuration is Dictionary<string, object> configuration)
            {
                var dataListConfig = DataListConfiguration.FromConfiguration(_utility, configuration);

                var convertValue = default(Func<DataListConfiguration, Type, string, object>);
                var valueType = typeof(string);

                if (dataListConfig.ValueConverter != null)
                {
                    valueType = dataListConfig.ValueConverter.GetValueType(dataListConfig);
                    convertValue = dataListConfig.ValueConverter.ConvertValue;
                }

                if (inter is string value)
                {
                    return convertValue != null
                        ? convertValue(dataListConfig, valueType, value)
                        : value;
                }

                if (inter is IEnumerable<string> items)
                {
                    if (propertyType.ClrType.IsGenericType)
                    {
                        var objects = new List<object>();

                        foreach (var item in items)
                        {
                            var obj = convertValue != null
                                ? convertValue(dataListConfig, valueType, item)
                                : item;

                            if (obj != null)
                            {
                                objects.Add(obj);
                            }
                        }

                        var result = Array.CreateInstance(valueType, objects.Count);
                        for (var i = 0; i < objects.Count; i++)
                        {
                            var attempt = objects[i].TryConvertTo(valueType);
                            if (attempt.Success == true)
                            {
                                result.SetValue(attempt.Result, i);
                            }
                            else
                            {
                                // NOTE: At this point `TryConvertTo` can't convert to the `valueType`.
                                // This may be a case where the `valueType` is an interface.
                                // We can attempt to cast it directly, as a last resort.
                                if (valueType.IsInstanceOfType(objects[i]) == true)
                                {
                                    result.SetValue(objects[i], i);
                                }
                            }
                        }

                        return result;
                    }

                    return string.Join(",", items);
                }
            }

            return base.ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        }
    }
}

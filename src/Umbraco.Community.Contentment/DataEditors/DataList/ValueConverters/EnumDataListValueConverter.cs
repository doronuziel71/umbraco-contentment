/* Copyright © 2020 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Community.Contentment.DataEditors
{
    public sealed class EnumDataListValueConverter : IDataListValueConverter
    {
        private const string _defaultConfigKey = "enumType";

        private readonly ILogger _logger;

        public EnumDataListValueConverter(ILogger logger)
        {
            _logger = logger;
        }

        public string Name => ".NET Enumeration Type";

        public string Description => "Converts the value to a .NET enumeration object-type.";

        public string Icon => "icon-indent";

        public OverlaySize OverlaySize => OverlaySize.Small;

        public Dictionary<string, object> DefaultValues => new Dictionary<string, object>
        {
            { "configKey", _defaultConfigKey },
        };

        public IEnumerable<ConfigurationField> Fields => new ConfigurationField[]
        {
            new NotesConfigurationField(@"<div class=""alert alert-info"">
<p><strong>A note the .NET Enum value converter.</strong></p>
<p>This value converter has been designed to work with the .NET Enumeration data source.</p>
<p>If you wish to specify a different configuration key, you may do so below.</p>
</div>", true),
            new ConfigurationField
            {
                Key = "configKey",
                Name = "Configuration key",
                Description = $"Enter the name of the key that contains the configuration of the enum type, e.g. <code>{_defaultConfigKey}</code>.",
                View = "textstring",
            }
        };

        public Type GetValueType(DataListConfiguration config)
        {
            var configKey = config.ValueConverterConfig?.GetValueAs("configKey", defaultValue: _defaultConfigKey) ?? _defaultConfigKey;

            if (config.DataSourceConfig.TryGetValueAs(configKey, out JArray array))
            {
                var enumType = array.ToObject<string[]>();
                if (enumType?.Length > 1)
                {
                    var assembly = default(Assembly);
                    try { assembly = Assembly.Load(enumType[0]); } catch (Exception ex) { _logger.Error<EnumDataListSource>(ex); }
                    if (assembly != null)
                    {
                        var type = default(Type);
                        try { type = assembly.GetType(enumType[1]); } catch (Exception ex) { _logger.Error<EnumDataListSource>(ex); }
                        if (type != null && type.IsEnum)
                        {
                            return type;
                        }
                    }
                }
            }

            return null;
        }

        public object ConvertValue(DataListConfiguration config, Type type, string value)
        {
            if (string.IsNullOrWhiteSpace(value) == false && type?.IsEnum == true)
            {
                try { return Enum.Parse(type, value, true); } catch (Exception ex) { _logger.Error<EnumDataListSource>(ex); }
            }

            return type.GetDefaultValue();
        }
    }
}

/* Copyright © 2020 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;

namespace Umbraco.Community.Contentment.DataEditors
{
    public sealed class UmbracoContentDataListValueConverter : IDataListValueConverter
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public UmbracoContentDataListValueConverter(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public string Name => "Umbraco IPublishedContent";

        public string Description => "Converts the value to Umbraco's `IPublishedContent` object-type.";

        public string Icon => "icon-umbraco";

        public OverlaySize OverlaySize => OverlaySize.Small;

        public Dictionary<string, object> DefaultValues => default;

        public IEnumerable<ConfigurationField> Fields => default;

        public Type GetValueType(DataListConfiguration config)
        {
            return typeof(IPublishedContent);
        }

        public object ConvertValue(DataListConfiguration config, Type type, string value)
        {
            if (type == typeof(IPublishedContent) && Udi.TryParse(value, out var udi))
            {
                return _umbracoContextAccessor.UmbracoContext.Content.GetById(udi);
            }

            return value.TryConvertTo(type).Result;
        }
    }
}

﻿/* Copyright © 2019 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Community.Contentment.DataEditors
{
    public sealed class ContentBlocksDataEditor : IDataEditor
    {
        internal const string DataEditorAlias = Constants.Internals.DataEditorAliasPrefix + "ContentBlocks";
        internal const string DataEditorName = Constants.Internals.DataEditorNamePrefix + "Content Blocks";
        internal const string DataEditorViewPath = Constants.Internals.EditorsPathRoot + "_empty.html";
        internal const string DataEditorOverlayViewPath = Constants.Internals.EditorsPathRoot + "content-blocks.overlay.html";
        internal const string DataEditorIcon = "icon-fa fa-server";

        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly Lazy<PropertyEditorCollection> _propertyEditors;
        private readonly ConfigurationEditorUtility _utility;

        public ContentBlocksDataEditor(
            IContentService contentService,
            IContentTypeService contentTypeService,
            IDataTypeService dataTypeService,
            Lazy<PropertyEditorCollection> propertyEditors,
            ConfigurationEditorUtility utility)
        {
            _contentService = contentService;
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
            _utility = utility;
        }

        public string Alias => DataEditorAlias;

        public EditorType Type => EditorType.PropertyValue;

        public string Name => DataEditorName;

        public string Icon => DataEditorIcon;

        public string Group => Core.Constants.PropertyEditors.Groups.RichContent;

        public bool IsDeprecated => false;

        public IDictionary<string, object> DefaultConfiguration => default;

        public IPropertyIndexValueFactory PropertyIndexValueFactory => new DefaultPropertyIndexValueFactory();

        public IConfigurationEditor GetConfigurationEditor() => new ContentBlocksConfigurationEditor(_contentService, _contentTypeService, _utility);

        public IDataValueEditor GetValueEditor()
        {
            return new ContentBlocksDataValueEditor(_contentTypeService, _dataTypeService, _propertyEditors.Value)
            {
                ValueType = ValueTypes.Json,
                View = DataEditorViewPath,
            };
        }

        public IDataValueEditor GetValueEditor(object configuration)
        {
            var hideLabel = false;
            var view = DataEditorViewPath;

            if (configuration is Dictionary<string, object> config)
            {
                if (config.ContainsKey(HideLabelConfigurationField.HideLabelAlias))
                {
                    hideLabel = config[HideLabelConfigurationField.HideLabelAlias].TryConvertTo<bool>().Result;
                }

                if (config.TryGetValueAs(ContentBlocksConfigurationEditor.DisplayMode, out JArray array) &&
                    array.Count > 0 &&
                    array[0] is JObject item)
                {
                    var displayMode = _utility.GetConfigurationEditor<IContentBlocksDisplayMode>(item.Value<string>("key"));
                    if (displayMode != null)
                    {
                        view = displayMode.View;
                    }
                }
            }

            return new ContentBlocksDataValueEditor(_contentTypeService, _dataTypeService, _propertyEditors.Value)
            {
                Configuration = configuration,
                HideLabel = hideLabel,
                ValueType = ValueTypes.Json,
                View = view,
            };
        }
    }
}

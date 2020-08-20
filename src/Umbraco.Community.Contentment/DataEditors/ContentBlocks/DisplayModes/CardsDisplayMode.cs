﻿/* Copyright © 2020 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Community.Contentment.DataEditors
{
    internal class CardsDisplayMode : IContentBlocksDisplayMode
    {
        public string Name => "Cards";

        public string Description => "Blocks will be displayed as cards.";

        public string Icon => "icon-playing-cards";

        public string View => IOHelper.ResolveUrl(Constants.Internals.EditorsPathRoot + "content-cards.html");

        public Dictionary<string, object> DefaultValues => default;

        public Dictionary<string, object> DefaultConfig => new Dictionary<string, object>
        {
            { "sortableAxis", false },
            { "enablePreview", false },
        };

        public IEnumerable<ConfigurationField> Fields => new ConfigurationField[]
        {
            new NotesConfigurationField($@"<div class=""alert alert-form"">
<p><strong>A note about block type previews.</strong></p>
<p>Unfortunately, the preview feature for block types is unsupported in {Name} display mode and will be disabled.</p>
</div>", true)
        };

        public OverlaySize OverlaySize => OverlaySize.Small;
    }
}

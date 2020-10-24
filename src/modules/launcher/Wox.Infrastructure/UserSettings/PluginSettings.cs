﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Wox.Plugin;

namespace Wox.Infrastructure.UserSettings
{
    public class PluginSettings : BaseModel
    {
        public Dictionary<string, Plugin> Plugins { get; private set; } = new Dictionary<string, Plugin>();

        public void UpdatePluginSettings(List<PluginMetadata> metadatas)
        {
            if (metadatas == null)
            {
                throw new ArgumentNullException(nameof(metadatas));
            }

            foreach (var metadata in metadatas)
            {
                if (Plugins.ContainsKey(metadata.ID))
                {
                    var settings = Plugins[metadata.ID];
                    if (settings.GetActionKeywords()?.Count > 0)
                    {
                        metadata.ActionKeywords = settings.GetActionKeywords();
                        metadata.ActionKeyword = settings.GetActionKeywords()[0];
                    }

                    metadata.Disabled = settings.Disabled;
                }
                else
                {
                    Plugins[metadata.ID] = new Plugin(metadata.ActionKeywords)
                    {
                        ID = metadata.ID,
                        Name = metadata.Name,
                        Disabled = metadata.Disabled,
                    };
                }
            }
        }
    }
}

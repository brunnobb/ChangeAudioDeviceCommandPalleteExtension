// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;

namespace ChangeAudioDeviceCommandPalleteExtension;

internal sealed partial class AudioDeviceListPage : ListPage
{
    private readonly bool _isInput;
    private readonly List<ListItem> _items = new();

    public AudioDeviceListPage(bool isInput)
    {
        _isInput = isInput;
        Title = _isInput ? "Set Input Device" : "Set Output Device";
        // Microphone icon (\uE720) or Volume/Speaker icon (\uE7F6)
        Icon = new IconInfo(_isInput ? "\uE720" : "\uE7F6");
        Name = "Open";

        LoadDevices();
    }

    public override IListItem[] GetItems()
    {
        return _items.ToArray();
    }

    private void LoadDevices()
    {
        _items.Clear();
        try
        {
            var defaultId = AudioManager.GetDefaultDeviceId(_isInput);
            var devices = AudioManager.GetDevices(_isInput);

            foreach (var dev in devices)
            {
                var isDefault = string.Equals(dev.Id, defaultId, StringComparison.OrdinalIgnoreCase);
                var command = new SetAudioDeviceCommand(dev.Id, _isInput, dev.FriendlyName, this);
                
                var listItem = new ListItem(command)
                {
                    Title = dev.FriendlyName,
                    Subtitle = dev.DeviceDesc + (isDefault ? " (Active)" : "")
                };

                _items.Add(listItem);
            }

            if (devices.Count == 0)
            {
                _items.Add(new ListItem(new NoOpCommand())
                {
                    Title = $"No active audio {(_isInput ? "input" : "output")} devices found"
                });
            }
        }
        catch (Exception ex)
        {
            _items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Error loading devices",
                Subtitle = ex.Message
            });
        }
    }

    public void Refresh()
    {
        LoadDevices();
        RaiseItemsChanged();
    }
}

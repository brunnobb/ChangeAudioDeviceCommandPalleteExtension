// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;

namespace ChangeAudioDeviceCommandPalleteExtension;

internal sealed partial class SetAudioDeviceCommand : InvokableCommand
{
    private readonly string _deviceId;
    private readonly bool _isInput;
    private readonly string _deviceName;
    private readonly AudioDeviceListPage _page;

    public SetAudioDeviceCommand(string deviceId, bool isInput, string deviceName, AudioDeviceListPage page)
    {
        _deviceId = deviceId;
        _isInput = isInput;
        _deviceName = deviceName;
        _page = page;
        Name = "Set as default";
    }

    public override CommandResult Invoke()
    {
        try
        {
            AudioManager.SetDefaultDevice(_deviceId);
            _page.Refresh();
            return CommandResult.ShowToast($"{_deviceName} set as default audio {(_isInput ? "input" : "output")} device");
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"Failed to set audio device: {ex.Message}");
        }
    }
}

// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace ChangeAudioDeviceCommandPalleteExtension;

[Guid("e82d5dfa-d102-421a-a77b-12cdfb71b64c")]
public sealed partial class ChangeAudioDeviceCommandPalleteExtension : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly ChangeAudioDeviceCommandPalleteExtensionCommandsProvider _provider = new();

    public ChangeAudioDeviceCommandPalleteExtension(ManualResetEvent extensionDisposedEvent)
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}

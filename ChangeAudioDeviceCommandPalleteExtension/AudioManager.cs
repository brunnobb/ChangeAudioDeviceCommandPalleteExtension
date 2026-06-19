// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ChangeAudioDeviceCommandPalleteExtension;

public struct AudioDeviceDetails
{
    public string Id { get; set; }
    public string FriendlyName { get; set; }
    public string DeviceDesc { get; set; }
}

public static class AudioManager
{
    [DllImport("ole32.dll")]
    internal static extern int PropVariantClear(ref PROPVARIANT pvar);

    public static List<AudioDeviceDetails> GetDevices(bool isInput)
    {
        var devices = new List<AudioDeviceDetails>();

        IMMDeviceEnumerator? enumerator;
        try
        {
            enumerator = new MMDeviceEnumerator() as IMMDeviceEnumerator;
        }
        catch (Exception)
        {
            return devices;
        }

        if (enumerator == null)
        {
            return devices;
        }

        try
        {
            var flow = isInput ? EDataFlow.eCapture : EDataFlow.eRender;
            // 1 = DEVICE_STATE_ACTIVE
            int hr = enumerator.EnumAudioEndpoints(flow, 1, out IMMDeviceCollection collection);
            if (hr != 0 || collection == null)
            {
                return devices;
            }

            hr = collection.GetCount(out uint count);
            if (hr != 0)
            {
                return devices;
            }

            for (uint i = 0; i < count; i++)
            {
                hr = collection.Item(i, out IMMDevice device);
                if (hr != 0 || device == null)
                {
                    continue;
                }

                try
                {
                    hr = device.GetId(out string id);
                    if (hr != 0)
                    {
                        continue;
                    }

                    string friendlyName = "";
                    string deviceDesc = "";

                    // STGM_READ = 0
                    hr = device.OpenPropertyStore(0, out IPropertyStore store);
                    if (hr == 0 && store != null)
                    {
                        try
                        {
                            var keyName = PropertyKeys.PKEY_Device_FriendlyName;
                            hr = store.GetValue(ref keyName, out PROPVARIANT pvName);
                            // VT_LPWSTR = 31
                            if (hr == 0 && pvName.vt == 31 && pvName.pointerValue != IntPtr.Zero)
                            {
                                friendlyName = Marshal.PtrToStringUni(pvName.pointerValue) ?? "";
                                _ = PropVariantClear(ref pvName);
                            }

                            var keyDesc = PropertyKeys.PKEY_Device_DeviceDesc;
                            hr = store.GetValue(ref keyDesc, out PROPVARIANT pvDesc);
                            if (hr == 0 && pvDesc.vt == 31 && pvDesc.pointerValue != IntPtr.Zero)
                            {
                                deviceDesc = Marshal.PtrToStringUni(pvDesc.pointerValue) ?? "";
                                _ = PropVariantClear(ref pvDesc);
                            }
                        }
                        finally
                        {
                            _ = Marshal.ReleaseComObject(store);
                        }
                    }

                    devices.Add(new AudioDeviceDetails
                    {
                        Id = id,
                        FriendlyName = friendlyName,
                        DeviceDesc = deviceDesc
                    });
                }
                finally
                {
                    _ = Marshal.ReleaseComObject(device);
                }
            }
        }
        finally
        {
            _ = Marshal.ReleaseComObject(enumerator);
        }

        return devices;
    }

    public static string GetDefaultDeviceId(bool isInput)
    {
        IMMDeviceEnumerator? enumerator;
        try
        {
            enumerator = new MMDeviceEnumerator() as IMMDeviceEnumerator;
        }
        catch (Exception)
        {
            return "";
        }

        if (enumerator == null)
        {
            return "";
        }

        try
        {
            var flow = isInput ? EDataFlow.eCapture : EDataFlow.eRender;
            int hr = enumerator.GetDefaultAudioEndpoint(flow, ERole.eConsole, out IMMDevice endpoint);
            if (hr == 0 && endpoint != null)
            {
                try
                {
                    hr = endpoint.GetId(out string id);
                    if (hr == 0)
                    {
                        return id;
                    }
                }
                finally
                {
                    _ = Marshal.ReleaseComObject(endpoint);
                }
            }
        }
        catch
        {
            // Ignore
        }
        finally
        {
            _ = Marshal.ReleaseComObject(enumerator);
        }

        return "";
    }

    public static void SetDefaultDevice(string deviceId)
    {
        IPolicyConfig? policyConfig;
        try
        {
            policyConfig = new CPolicyConfigClient() as IPolicyConfig;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Could not instantiate CPolicyConfigClient COM class", ex);
        }

        if (policyConfig == null)
        {
            throw new InvalidOperationException("Could not instantiate CPolicyConfigClient");
        }

        try
        {
            // Set default for all roles
            int hr = policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            hr = policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            hr = policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }
        finally
        {
            _ = Marshal.ReleaseComObject(policyConfig);
        }
    }
}

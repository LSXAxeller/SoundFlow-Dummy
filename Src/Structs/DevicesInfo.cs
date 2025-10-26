using System.Runtime.InteropServices;
using SoundFlow.Utils;

namespace SoundFlow.Structs;

/// <summary>
/// Represents device information including ID, name, default status, and data formats.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public record struct DeviceInfo
{
    /// <summary>
    /// The unique identifier for the device.
    /// </summary>
    public IntPtr Id;

    /// <summary>
    /// The name of the device.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]  // 255 + 1 for null terminator
    public string Name;

    /// <summary>
    /// Indicates whether the device is set as default.
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool IsDefault;

    /// <summary>
    /// The count of native data formats supported by the device.
    /// </summary>
    public uint NativeDataFormatCount;

    /// <summary>
    /// Pointer to the native data formats.
    /// </summary>
    public IntPtr NativeDataFormats;

    /// <summary>
    /// Gets the supported native data formats for the device.
    /// </summary>
    public NativeDataFormat[] SupportedDataFormats => NativeDataFormats.ReadArray<NativeDataFormat>((int)NativeDataFormatCount);
}


/* Temporary Fix.
 
    /// <summary>
    /// The raw byte array for the device name UTF-8 encoded string.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] NameBytes;

    /// <summary>
    /// A helper property to get the properly decoded device name as a string.
    /// </summary>
    public string Name
    {
        get
        {
            // Find the position of the first null terminator in the byte array.
            int count = Array.IndexOf(NameBytes, (byte)0);

            // If no null terminator is found, use the whole array length.
            if (count == -1)
                count = NameBytes.Length;

            // Decode the byte array from the start up to the null terminator as UTF-8.
            return Encoding.UTF8.GetString(NameBytes, 0, count);
        }
    }
    */
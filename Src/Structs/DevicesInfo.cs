using System.Runtime.InteropServices;
using System.Text;
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
    /// The raw byte array for the device name, assumed to be UTF-8 encoded.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    private byte[] NameBytes;

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
    /// Gets the name of the device, decoded from its UTF-8 representation.
    /// This is a helper property and not part of the marshaled layout.
    /// </summary>
    public string Name
    {
        get
        {
            // Find the position of the first null terminator.
            var count = Array.IndexOf(NameBytes, (byte)0);
            if (count == -1) count = NameBytes.Length;
            
            // Decode the byte array from the start-up to the null terminator as UTF-8.
            return Encoding.UTF8.GetString(NameBytes, 0, count);
        }
    }

    /// <summary>
    /// Gets the supported native data formats for the device.
    /// This is a helper property and not part of the marshaled layout.
    /// </summary>
    public NativeDataFormat[] SupportedDataFormats => NativeDataFormats.ReadArray<NativeDataFormat>((int)NativeDataFormatCount);
}
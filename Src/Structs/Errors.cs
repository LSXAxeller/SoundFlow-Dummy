namespace SoundFlow.Structs;

/// <summary>
/// Base interface for all errors in the library.
/// </summary>
public interface IError
{
    /// <summary>
    /// Gets a message that describes the current error.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the <see cref="Exception"/> instance that caused the current error.
    /// </summary>
    Exception? InnerException { get; }
}

/// <summary>
/// A base error implementation.
/// </summary>
/// <param name="Message">A message that describes the current error.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public record Error(string Message, Exception? InnerException = null) : IError;

/// <summary>
/// Represents errors related to invalid input arguments or preconditions.
/// </summary>
/// <param name="Message">A message that describes the validation error.</param>
public record ValidationError(string Message) : Error(Message);

/// <summary>
/// Represents an error when a required resource (like a file) is not found.
/// </summary>
/// <param name="ResourceName">The name or identifier of the resource that was not found.</param>
/// <param name="Message">A message that describes the error.</param>
public record NotFoundError(string ResourceName, string Message) : Error(Message);

/// <summary>
/// Represents errors that occur during the parsing or writing of a specific file format.
/// </summary>
/// <param name="Message">A message that describes the error.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public abstract record FileFormatError(string Message, Exception? InnerException = null)
    : Error(Message, InnerException);

/// <summary>
/// The error that occurs when attempting to read an audio format that is not supported by the library.
/// </summary>
/// <param name="FormatDetails">Specific details about the unsupported format (e.g., codec name, sample format).</param>
public sealed record UnsupportedFormatError(string FormatDetails)
    : FileFormatError($"The provided audio format is not supported. Details: {FormatDetails}");

/// <summary>
/// The error that occurs when a mandatory header, marker, or chunk is missing from a file.
/// </summary>
/// <param name="HeaderDescription">A description of the mandatory header that was not found (e.g., "RIFF chunk", "ID3v2 tag").</param>
public sealed record HeaderNotFoundError(string HeaderDescription)
    : FileFormatError(
        $"Could not find the mandatory '{HeaderDescription}'. The file may be corrupt or not a valid audio file.");

/// <summary>
/// The error that occurs when a recognized audio file's structural component (chunk, atom, etc.) is malformed.
/// </summary>
/// <param name="ChunkId">The identifier of the corrupted chunk or atom (e.g., "fmt ", "data").</param>
/// <param name="Reason">The reason why the chunk is considered corrupt.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public sealed record CorruptChunkError(string ChunkId, string Reason, Exception? InnerException = null)
    : FileFormatError($"The '{ChunkId}' chunk/atom is corrupted. Reason: {Reason}", InnerException);

/// <summary>
/// The error that occurs when a specific frame within a stream-based format (like MP3 or AAC) is malformed.
/// </summary>
/// <param name="FrameDescription">A description of the type of frame that was corrupted.</param>
/// <param name="Reason">The reason why the frame is considered corrupt.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public sealed record CorruptFrameError(string FrameDescription, string Reason, Exception? InnerException = null)
    : FileFormatError($"A '{FrameDescription}' frame is corrupted. Reason: {Reason}", InnerException);

/// <summary>
/// The error that occurs when an object has been disposed.
/// </summary>
/// <param name="ObjectDescription">A description of the disposed object (e.g., class name).</param>
public sealed record ObjectDisposedError(string ObjectDescription)
    : Error($"The '{ObjectDescription}' object has been disposed.");

/// <summary>
/// Represents errors related to an audio or MIDI device.
/// </summary>
/// <param name="Message">A message that describes the device error.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public abstract record DeviceError(string Message, Exception? InnerException = null) : Error(Message, InnerException);

/// <summary>
/// The error that occurs when an operation is performed on a device that is in an invalid state for that operation.
/// </summary>
/// <param name="Reason">A message describing why the state is invalid for the operation.</param>
public sealed record DeviceStateError(string Reason)
    : DeviceError(Reason);

/// <summary>
/// The error that occurs when a core device operation (like open, start, or stop) fails.
/// </summary>
/// <param name="Operation">The name of the operation that failed (e.g., "start device").</param>
/// <param name="Reason">The underlying reason for the failure.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public sealed record DeviceOperationError(string Operation, string Reason, Exception? InnerException = null)
    : DeviceError($"Failed to {Operation}. Reason: {Reason}", InnerException);

/// <summary>
/// The error that occurs when a requested audio device cannot be found.
/// </summary>
/// <param name="DeviceIdentifier">The identifier (name, ID, etc.) of the device that was not found.</param>
public sealed record DeviceNotFoundError(string DeviceIdentifier)
    : DeviceError($"The device '{DeviceIdentifier}' could not be found.");

/// <summary>
/// The error that occurs when a required audio backend (like WASAPI, CoreAudio, etc.) is not available or enabled.
/// </summary>
/// <param name="BackendName">The name of the specific backend that was not found, if applicable.</param>
public sealed record BackendNotFoundError(string? BackendName = null)
    : Error(string.IsNullOrEmpty(BackendName)
        ? "No suitable audio backend was found."
        : $"The audio backend '{BackendName}' was not found or is not enabled.");

/// <summary>
/// The error that occurs when an operation attempts to use a resource that is already in use.
/// </summary>
/// <param name="ResourceName">The name or description of the busy resource.</param>
public sealed record ResourceBusyError(string ResourceName)
    : Error($"The resource '{ResourceName}' is busy or already in use.");

/// <summary>
/// The error that occurs when an attempt to allocate memory fails.
/// </summary>
public sealed record OutOfMemoryError()
    : Error("Insufficient memory to complete the operation.");

/// <summary>
/// The error that occurs when a method call is invalid for the object's current state.
/// This is analogous to <see cref="System.InvalidOperationException"/>.
/// </summary>
/// <param name="Message">The message that describes the error.</param>
public sealed record InvalidOperationError(string Message)
    : Error(Message);

/// <summary>
/// The error that occurs when a requested feature or method is not implemented.
/// </summary>
/// <param name="FeatureName">The name of the unimplemented feature.</param>
public sealed record NotImplementedError(string FeatureName)
    : Error($"The feature '{FeatureName}' is not implemented.");

/// <summary>
/// Represents a generic error reported by the underlying operating system or host environment.
/// </summary>
/// <param name="Reason">The description of the error provided by the host.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public sealed record HostError(string Reason, Exception? InnerException = null)
    : Error($"An operating system error occurred. Reason: {Reason}", InnerException);

/// <summary>
/// Represents an unexpected error within a wrapped native library.
/// </summary>
/// <param name="LibraryName">The name of the native library where the error originated (e.g., "PortMidi", "miniaudio").</param>
/// <param name="Reason">The description of the internal error.</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public sealed record InternalLibraryError(string LibraryName, string Reason, Exception? InnerException = null)
    : Error($"An internal error occurred in the '{LibraryName}' library. Reason: {Reason}", InnerException);

/// <summary>
/// Represents errors that occur during an I/O operation.
/// </summary>
/// <param name="OperationDescription">A description of the I/O operation that failed (e.g., "reading from file stream").</param>
/// <param name="InnerException">The exception that is the cause of the current error, or a null reference if no inner exception is specified.</param>
public sealed record IOError(string OperationDescription, Exception? InnerException = null)
    : Error($"An I/O error occurred during '{OperationDescription}'.", InnerException);

/// <summary>
/// The error that occurs when an operation times out.
/// </summary>
/// <param name="OperationDescription">A description of the operation that timed out.</param>
public sealed record TimeoutError(string OperationDescription)
    : Error($"The operation '{OperationDescription}' timed out.");

/// <summary>
/// The error that occurs when access to a requested resource is denied.
/// </summary>
/// <param name="ResourceIdentifier">The path or identifier of the resource to which access was denied.</param>
public sealed record AccessDeniedError(string ResourceIdentifier)
    : Error($"Access to the resource '{ResourceIdentifier}' was denied.");
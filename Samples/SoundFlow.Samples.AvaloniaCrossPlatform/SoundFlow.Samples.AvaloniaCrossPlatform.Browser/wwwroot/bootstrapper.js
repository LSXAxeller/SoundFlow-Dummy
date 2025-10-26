// --- Part 1: Load and Initialize Your Custom Miniaudio Library ---

// Import the factory function from the Emscripten-generated glue code.
import miniaudioFactory from './library.js';

console.log("Loading custom miniaudio library...");

// The factory is async and returns a promise. We must 'await' it.
// This downloads and compiles the .wasm file.
const miniaudioModule = await miniaudioFactory();

console.log("Miniaudio library loaded. Exposing functions to C#...");

// --- Part 2: Expose Wasm Functions for C# JSImport ---

// JSImport looks for functions on the global 'window' object.
// We will take the functions from our loaded module and attach them to 'window'.
// IMPORTANT: C# [JSImport("sf_free")] will look for 'window.sf_free'.
// The Wasm functions in our module are named with a leading underscore (e.g., '_sf_free').
// We must map them correctly.

const functionsToExpose = [
    // Your custom functions
    'sf_free', 'sf_get_devices', 'sf_allocate_encoder', 'sf_allocate_decoder',
    'sf_allocate_context', 'sf_allocate_device', 'sf_allocate_decoder_config',
    'sf_allocate_encoder_config', 'sf_allocate_device_config',

    // Native Miniaudio functions
    'ma_encoder_init_file', 'ma_encoder_uninit', 'ma_encoder_write_pcm_frames',
    'ma_decoder_init', 'ma_decoder_uninit', 'ma_decoder_read_pcm_frames',
    'ma_decoder_seek_to_pcm_frame', 'ma_decoder_get_length_in_pcm_frames',
    'ma_context_init', 'ma_context_uninit', 'ma_device_init', 'ma_device_uninit',
    'ma_device_start', 'ma_device_stop',

    // Standard memory functions
    'malloc', 'free'
];

functionsToExpose.forEach(name => {
    const wasmName = '_' + name;
    if (miniaudioModule[wasmName]) {
        // Attach to window, removing the leading underscore from the name
        window[name] = miniaudioModule[wasmName];
    } else {
        console.warn(`Attempted to expose function "${name}" but it was not found in the Wasm module.`);
    }
});

// For function pointers (like the dataCallback), you will need addFunction/removeFunction
// This exposes them globally for your C# code to call when needed.
window.addFunction = miniaudioModule.addFunction;
window.removeFunction = miniaudioModule.removeFunction;


console.log("All custom functions exposed globally.");
// Now that our library is ready, we can safely load and start the main Avalonia app.
console.log("Starting WebAssembly application...");
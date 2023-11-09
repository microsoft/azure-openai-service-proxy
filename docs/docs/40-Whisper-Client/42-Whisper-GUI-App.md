# The Whisper GUI app

Using Whisper GUI app, you can transcribe pre-recorded audio files and audio recorded from your microphone.

:::tip

If you want remote access to the Whisper Transcriber Service on WSL then you need to run a proxy. The easiest way to do this is with `ngrok`. See [Whisper Anywhere Access](/Proxies/Whisper-ngrok/) for more information.

:::

![](../media/openai_whisper_gui.png)

## Install system dependencies

Follow the instructions for your operating system.

### Install Windows 11 dependencies

1. Install the latest version of [Python 3](https://www.python.org/downloads/). At the time of writing, June 2023, Python 3.11.3.
2. Install `FFmpeg`.
   1. You can download the [latest release](https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip) from [FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds/releases).
   2. Unzip the downloaded FFmpeg file and move to your preferred app folder.
   3. From `System Properties`, select `Environment Variables`, and add the path to the FFmpeg bin folder to the path.
   4. Test FFmpeg. From a new terminal window, run `ffmpeg -version`.

### Install macOS dependencies

1. Install `FFmpeg`, `Tkinker`, and `PortAudio`.
   1. Open a terminal window.
   2. Install [Homebrew](https://docs.brew.sh/Installation).
   3. Run `brew install ffmpeg python-tk portaudio`.

### Install Ubuntu dependencies

1. Install `FFmpeg`, `pip3`, and `Tkinker`.
   1. Open a terminal window.
   2. Run `sudo apt install ffmpeg python3-pip python3-tk`

## Install the required Python libraries

1. Install the [git client](https://git-scm.com/downloads) if it's not already installed.
1. From a `Terminal` window, clone the Whisper Transcriber Sample to your preferred repo folder.
    ```bash
    git clone https://github.com/gloveboxes/OpenAI-Whisper-Transcriber-Sample.git
    ```
2. Navigate to the `client` folder.
   ```bash
   cd OpenAI-Whisper-Transcriber-Sample/client
   ```
3. Install the required libraries.

   On windows:

   ```powershell
   pip install -r requirements.txt
   ```

   On macOS and Linux:

   ```bash
   pip3 install -r requirements.txt
   ```

## Start the Whisper app

To start the Whisper GUI app, run the following command from the `client` folder.

On Windows:

```powershell
python whisper_gui.py
```

On macOS and Linux:

```bash
python3 whisper_gui.py
```

## Using the Whisper GUI app

1. The Whisper server endpoint defaults to `http://localhost:5500`. This is the endpoint to use when the Whisper server and the Whisper GUI app are running on the same system.
   
   If you are connecting to a remote Whisper server then review the [Whisper Server anywhere access](../Proxies/Whisper-ngrok Whisper-ngrok) page and use the remote endpoint provided by `ngrok`.

2. Add the Whisper server API key. The API key is displayed in the terminal window when the Whisper server is started.
3. Select `Update service config` to save the endpoint and API key. Next time you start the Whisper GUI app, the endpoint and API key will be loaded from the `config.json` file.
4. If you wish to transcribe a pre-recorded audio file, select `Audio folder` to choose the folder containing your audio files, then from the dropdown list, select the audio file to be transcribed, then select `Transcribe`. The audio file will be sent to the Whisper Transcriber Service and the transcription will be displayed in the `Transcription` text box.
5. If you wish to transcribe audio from your microphone, select `Microphone`, then record your audio. When done, select `Stop recording` and the audio will be sent to the Whisper Transcriber Service and the transcription will be displayed in the `Transcription` text box.

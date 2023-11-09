# Systems without an NVidia GPU

The Whisper Transcriber Service runs on Windows, macOS, and Linux systems without an NVidia GPU, it'll just run slower as the Whisper model run on the CPU.

From limited testing, the multilingual and the English-only OpenAI Whisper models for `tiny(.en)`, `small(.en)`, and `medium(.en)` models ran with acceptable performance on Windows 11 with a modern CPU and on a MacBook M2 Air with 16 GB of RAM.

## Install system dependencies

Follow the instructions for your operating system.

### Install Windows 11 dependencies

1. Install `FFmpeg`.
   1. You can download the [latest release](https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip) from [FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds/releases).
   2. Unzip the downloaded FFmpeg file and move to your preferred app folder.
   3. From `System Properties`, select `Environment Variables`, and add the path to the FFmpeg bin folder to the path.
   4. Test FFmpeg. From a new terminal window, run `ffmpeg -version`.

### Install macOS dependencies

1. Install `FFmpeg`
   1. Open a terminal window.
   1. Install [Homebrew](https://docs.brew.sh/Installation).
   1. Install FFmpeg. Run

        ```bash
        brew install ffmpeg
        ```

### Install Ubuntu 20.04 dependencies

1. Install `FFmpeg` and `pip3`
   1. Open a terminal window.
   2. Run:
        ```bash
        sudo apt install ffmpeg python3-pip python3-venv
        ```

## Start the Whisper Transcriber Service

1. From a terminal window.
2. Clone the Whisper Transcriber Sample to your preferred repo folder.

    ```bash
    git clone https://github.com/gloveboxes/OpenAI-Whisper-Transcriber-Sample.git
    ```

3. Navigate to the `server` folder.

    ```bash
    cd OpenAI-Whisper-Transcriber-Sample/server
    ```

4. Create a Python virtual environment.

    :::danger
    At the time of writing (June 2023), the [Whisper Python library](https://pypi.org/project/openai-whisper) is supported on Python 3.8 to 3.10. The Whisper library worked on Python 3.11.3, but not Python 3.11.4. Be sure to check the version of Python you are using `python3 --version`.
    :::

    ```bash
    python3 -m venv .whisper-venv
    ```

5. Activate the Python virtual environment.

    on Windows

    ```pwsh
    .\.whisper-venv\Scripts\activate
    ```

    on macOS and Linux

    ```bash
    source .whisper-venv/bin/activate
    ```

7. Install the required Python libraries.

    ```bash
    pip3 install -r requirements.txt
    ```

8. Review the following chart is taken from the [OpenAI Whisper Project Description](https://pypi.org/project/openai-whisper/) page and select the model that will fit in the RAM of your computer. At the time of writing, Whisper multilingual models include `tiny`, `small`, `medium`, and `large`, and English-only models include `tiny.en`, `small.en`, and `medium.en`.
   ![](../media/whisper_model_selection.png)

9.  Update the `server/config.json` file to set your desired Whisper model. For example, to use the `medium` model, set the `model` property to `medium`.

    ```json
    { "model": "medium" }
    ```

10.   Start the Whisper Transcriber Service. From the command line, run:

        ```bash
        uvicorn main:app --port 5500 --host 0.0.0.0
        ```

        Once the Whisper Transcriber Service starts, you should see output similar to the following.

        ```text
        [2023-06-04 18:53:46.194411] Whisper API Key: 17ce01e9-ac65-49c8-9cc9-18d8deb78197
        [2023-06-04 18:53:50.375244] Model: medium loaded.
        [2023-06-04 18:53:50.375565] Ready to transcribe audio files.
        ```

11.  The `Whisper API Key` will be also be displayed. Save the `Whisper API Key` somewhere safe, you'll need the key to configure the Whisper client.

    ```text
    Whisper API Key: <key>
    ```

11. To stop the Whisper Transcriber Service, press `CTRL+C` in the terminal.
12. To deactivate the Python virtual environment, run `deactivate`.

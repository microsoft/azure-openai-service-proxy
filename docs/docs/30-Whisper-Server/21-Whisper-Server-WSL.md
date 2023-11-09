# Windows with an NVidia GPU

The recommended configuration for running the OpenAI Whisper sample on Windows is with WSL 2 and an NVidia GPU. This configuration is popular and provides the best performance. The OpenAI Whisper speech to text transcription runs consistently faster on WSL 2 than natively on Windows.

Ideally, your system should have:

1. Windows 11 with WSL 2 and Ubuntu 20.04 LTS.
2. Ubuntu 20.04 includes Python 3.8.

   As At June 2023, the [OpenAI Whisper library](https://pypi.org/project/openai-whisper/) is compatible with Python 3.8-3.10

3. A modern CPU with 16 GB of RAM.
4. An NVidia GPU with 10 to 12 GB of VRAM. But you can run smaller Whisper models on GPUs with less VRAM.

## Update the NVidia drivers

Ensure the NVidia drivers are up to date. The NVidia drivers are installed in Windows. WSL includes a GPU driver that allows WSL to access the GPU, so don't install the NVidia drivers in WSL.

## Install WSL 2

1. Follow the instructions to [install WSL](https://learn.microsoft.com/en-us/windows/wsl/install).
2. This sample was tested with Ubuntu 20.04 LTS running in WSL 2. You can download Ubuntu 20.04 LTS from the [Microsoft Store](https://apps.microsoft.com/store/detail/ubuntu-2004/9N6SVWS3RX71).

## Install Ubuntu dependencies

1. Update the Ubuntu system.
   1. From a WSL terminal.
   2. Run:
        ```bash
        sudo apt update && sudo apt upgrade
        ```
   3. Restart WSL if necessary, from PowerShell, run `wsl --shutdown`.
2. Install the dependencies. 
   1. Run:
        ```bash
        sudo apt install ffmpeg python3-pip python3-venv
        ```
   2. Test FFmpeg. Run `ffmpeg -version`. The command should return the FFmpeg version.

## Start the Whisper Transcriber Service

1. From a WSL terminal.
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

    ```bash
    source .whisper-venv/bin/activate
    ```

6. Install the required Python libraries.

    ```bash
    pip3 install -r requirements.txt
    ```

7. Test that CUDA/GPU is available to PyTorch.

   Run the following command, if CUDA is available, the command will return `True`.

    ```bash
    python3 -c "import torch; print(torch.cuda.is_available())"
    ```

8. Review the following chart is taken from the [OpenAI Whisper Project Description](https://pypi.org/project/openai-whisper/) page and select the model that will fit in the VRAM of your GPU. At the time of writing, Whisper multilingual models include `tiny`, `small`, `medium`, and `large`, and English-only models include `tiny.en`, `small.en`, and `medium.en`.
   ![](../media/whisper_model_selection.png)

9.  Update the `server/config.json` file to set your desired Whisper model. For example, to use the `medium` model, set the `model` property to `medium`.

    ```json
    { "model": "medium" }
    ````

10.  Start the Whisper Transcriber Service. From the command line, run:

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

12. To stop the Whisper Transcriber Service, press `CTRL+C` in the terminal.
13. To deactivate the Python virtual environment, run `deactivate`.

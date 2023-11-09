# Start the Home Assistant

## Platform support

The Home Assistant app has been tested on the following platforms:

1. Windows 11 (x64, latest updates)
2. macOS Ventura (Apple Silicon, latest updates)
3. Ubuntu 20.04 (latest updates)

## Clone the repository

1. Install Git from https://git-scm.com/downloads.
2. Clone the repository:

    ```bash
    git clone https://github.com/gloveboxes/OpenAI-Whisper-Transcriber-Sample
    ```

## Environment file

You'll find a `.env` file in the `client` folder of the repo you cloned. This file contains the configuration settings for the Home Assistant app.

The following keys are defined in the `.env` file:

| Key | Description |
| --- | --- |
| OPENAI_API_KEY | The OpenAI API key. |
| WEATHER_API_KEY | The Weather API key. |
| WHISPER_MODE | The Whisper speech to text transcriber mode. The default mode is `local`.  |
| WHISPER_MODEL_NAME | The Whisper speech to text transcriber model name. The default model is `tiny`. This is only used when the `WHISPER_MODE` is set to `local`. See [Whisper models](#whisper-models) for more information. |
| WHISPER_ENDPOINT | The Whisper speech to text transcriber endpoint. This is only used when the `WHISPER_MODE` is set to `gpu`. |
| WHISPER_API_KEY | The Whisper speech to text transcriber API key. This is only used when the `WHISPER_MODE` is set to `gpu`. |


## Cloud API Keys

The Home Assistant uses the following cloud services:

1. [OpenAI](https://platform.openai.com) chat and depending on your configuration, the `Whisper` speech to text transcriber. The OpenAI API key is used to call the OpenAI Chat Completion API and extracting OpenAI Functions.
2. The [Weather API](https://www.weatherapi.com/) to get weather data. This data is used to `ground` the GPT prompts the assistant generates.

Next you will need to create accounts and get API keys for the cloud services. The API keys are stored in a `.env` file in the `client` folder of the repo you cloned. As you create the API keys, add them to the `.env` file.

### OpenAI API

Create an OpenAI account and get an API key.

1. Sign up for an [OpenAI account](https://platform.openai.com).
2. Create an [OpenAI API key](https://platform.openai.com/account/api-keys).
3. Update the OPENAI_API_KEY key in the `.env` file with the API key.
4. Save the updated `.env` file.

### Weather API

Create a Weather API account and get an API key.

1. Sign up for a [Weather API account](https://www.weatherapi.com/signup.aspx).
2. Create a free [Weather API key](https://www.weatherapi.com/my/).
3. Update the WEATHER_API_KEY key in the `.env` file with the API key.
4. Save the updated `.env` file.

## Install prerequisites

1. Install [Python](https://www.python.org/downloads) version 3.8 ~ 3.10. The [Whisper library](https://pypi.org/project/openai-whisper/) is supported on Python 3.8 to 3.10.

## Install OS Dependencies

### Linux

```bash
sudo apt-get install portaudio19-dev python3-pyaudio python3-tk
```


## Install the required Python packages:

1. From the command line, change to the `client` folder of the repo you cloned.

    ```bash
    cd OpenAI-Whisper-Transcriber-Sample/client
    ```

2. Create a Python virtual environment:

    Windows

    ```pwsh
    python -m venv .assistant
    ```

    Linux and macOS

    ```bash
    python3 -m venv .assistant
    ```

3. Activate the Python virtual environment:

    Windows

    ```pwsh
    ./.assistant/Scripts/activate
    ```

    Linux and macOS

    ```bash
    source .assistant/bin/activate
    ```

4. Install the required Python packages:

    ```bash
    pip install -r requirements.txt
    ```

5. On Windows, by default, the `requirements.txt` file will install the CPU version of PyTorch. If you have an NVidia GPU, you can install the CUDA accelerated version of PyTorch.

    1. First uninstall the CPU version of PyTorch:

        ```pwsh
        pip3 uninstall torch torchvision torchaudio
        ```

    2. Install the CUDA accelerated version of PyTorch:

        Review the [PyTorch website](https://pytorch.org/get-started/locally/) for the latest installation instructions.

        ```bash
        pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu117
        ```

## Set the Text to Speech Transcriber mode

There are three modes to use Whisper speech to text transcriber. The default mode is `local`. You can change the mode by updating the `WHISPER_MODE` key in the `.env` file.

1. `local`: The Whisper speech to text transcription is done locally on the device. This is the default mode and free mode. The speed will depend on the hardware capabilities of your computer. The first time the Whisper speech to text transcriber is used, it will download the transcriber model from the internet. This will take a few minutes.
2. `openai`: The Whisper speech to text transcription is done using the OpenAI API Audio service which maybe a lot faster that transcribing speech on your computer. This is a paid service, review [OpenAI Audio Model Pricing](https://openai.com/pricing/) for more information.
3. `gpu`: You can run a Whisper REST endpoint on your own NVidia GPU. For more information, review the [Whisper REST API](../Whisper-Server/Whisper-Server-Setup) docs.

## Run the home assistant app

1. Ensure the Python virtual environment is activated.
2. From the command line, change to the `client` folder of the repo you cloned.
3. Run the home assistant app:

    ```bash
    python assistant.py
    ```
4. The App will start, select your preferred microphone from the dropdown menu.

    ![Home Assistant](media/home_assistant.png)

5. You can tweak the `Energy Threshold`. The Energy Threshold represents the energy level threshold for sounds. Values below this threshold are considered silence, and values above this threshold are considered speech. For more information on this setting, review [recognizer_instance.energy_threshold](https://github.com/Uberi/speech_recognition/blob/master/reference/library-reference.rst#recognizer_instanceenergy_threshold--300---type-float).
6. Press `Microphone` button to start listening for your voice commands. Try out a few commands like:

    1. What's the weather in Seattle
    2. How can you help me
    3. Turn on the living room lights
    4. Turn in the living room lights set the color to orange and brightness to dim.
    5. Turn on the washing machine
    6. Lock the front door


## Whisper Models

The Whisper speech to text transcriber default model name is `tiny`. You can change the model name by updating the `WHISPER_MODEL_NAME` key in the `.env` file.

The following table lists the available Whisper speech to text transcriber model names. The performance of the transcriber will depend on the model name you select and the hardware capabilities of your computer. 

Remember, if you don't have the hardware to run the models with sufficient performance, you can use the OpenAI API Audio service by setting the `WHISPER_MODE` environment variable to `openai`. 

The OpenAI API Audio service is a paid service, review [OpenAI Audio Model Pricing](https://openai.com/pricing/) for more information.

![](media/whisper_model_selection.png)
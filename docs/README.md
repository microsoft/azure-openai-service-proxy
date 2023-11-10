# Website

## Docs published on GitHub Pages

Docs are published on [GitHub Pages](https://gloveboxes.github.io/OpenAI-Whisper-Transcriber-Docs/)

## Docusaurus

This website is built using [Docusaurus 2](https://docusaurus.io/), a modern static website generator.

## Contributing to the docs

1. Install the latest version of node.js from [nodejs.org](https://nodejs.org/en/download/)
2. Clone this repository
3. Navigate to the directory you just cloned
4. Install the dependencies

    ```bash
    npm install
    ```

4. Start the website

    ```bash
    npm run start
    ```

5. Build and test the website

    ```bash
    npm run build
    npm run serve
    ```

6. Add a new doc page

    - Add your .md pages to the docs folder.
    - Save
    - Review your updates
      - `npm run start`
      - the browser will open to http://localhost:3000/OpenAI-Whisper-Transcriber-Docs/

   

<!-- ```
$ yarn
```

### Local Development

```
$ yarn start
```

This command starts a local development server and opens up a browser window. Most changes are reflected live without having to restart the server.

### Build

```
$ yarn build
```

This command generates static content into the `build` directory and can be served using any static contents hosting service.

### Deployment

Using SSH:

```
$ USE_SSH=true yarn deploy
```

Not using SSH:

```
$ GIT_USER=<Your GitHub username> yarn deploy
```

If you are using GitHub pages for hosting, this command is a convenient way to build the website and push to the `gh-pages` branch. -->

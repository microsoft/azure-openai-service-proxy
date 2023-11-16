// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require("prism-react-renderer/themes/github");
const darkCodeTheme = require("prism-react-renderer/themes/dracula");

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: "Azure OpenAI Proxy Service",
  tagline: "Azure OpenAI Community Events Made Easy",

  url: "https://gloveboxes.github.io/",
  baseUrl: "/azure-openai-service-proxy/",

  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "throw",
  trailingSlash: true,

  favicon: "img/favicon.ico",

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: "gloveboxes", // Usually your GitHub org/user name.
  projectName: "azure-openai-service-proxy", // Usually your repo name.

  deploymentBranch: "gh-pages", // Branch that GitHub pages will deploy from.

  // Even if you don't use internalization, you can use this field to set useful
  // metadata like html lang. For example, if your site is Chinese, you may want
  // to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  presets: [
    [
      "classic",
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          routeBasePath: "/",
          sidebarPath: require.resolve("./sidebars.js"),
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl: "https://github.com/gloveboxes/azure-openai-service-proxy/tree/master/docs/",
        },
        blog: false,
        theme: {
          customCss: require.resolve("./src/css/custom.css"),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      navbar: {
        title: "Azure OpenAI Proxy Service",
        logo: {
          alt: "Azure OpenAI Proxy Logo",
          src: "img/Azure-OpenAI-Services.svg",
        },
        items: [],
      },
      footer: {
        style: "dark",
        links: [],
        copyright: `Copyright © ${new Date().getFullYear()} Azure OpenAI Proxy Service - Azure OpenAI Community Events Made Easy. Built with Docusaurus.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
        additionalLanguages: ["csharp"],
      },

      /* Clarity Config */
      clarity: {
        ID: "jo0nppwdka", 
      }

    }),

    plugins: [
      [
        '@docusaurus/plugin-ideal-image',
        {
          quality: 96,
          max: 1000, // max resized image's size.
          min: 420, // min resized image's size. 
          steps: 4, // #images b/w min and max (inclusive)
          disableInDev: false,
        },
      ],
      [
        'docusaurus-plugin-clarity',
        {
        }
      ],
    ],
};

module.exports = config;

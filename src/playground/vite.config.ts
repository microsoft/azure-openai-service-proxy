import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  return {
    plugins: [react()],
    define: {
      "process.env.ENVIRONMENT": JSON.stringify(mode),
    },
    server: {
      proxy: {
        "/v1": env.API_URL,
      },
    },
  };
});

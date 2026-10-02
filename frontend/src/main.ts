import { createApp } from "vue";
import App from "./app/App.vue";
import { router } from "./app/router";
import "./styles/tokens.css";
import "./styles/base.css";
createApp(App).use(router).mount("#app");

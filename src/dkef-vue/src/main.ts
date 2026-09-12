import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import 'vue-sonner/style.css'
import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/authStore'

const app = createApp(App)

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

app.use(pinia)
app.use(router)

const authStore = useAuthStore(pinia)

async function bootstrap() {
  await authStore.initializeSession()
  await router.isReady()
  app.mount('#app')
}

void bootstrap()

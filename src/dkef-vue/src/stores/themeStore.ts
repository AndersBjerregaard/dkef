import { ref, watch } from 'vue'
import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', () => {
  // Determine initial theme: localStorage → system preference → dark
  function getInitialTheme(): 'dark' | 'light' {
    const stored = localStorage.getItem('theme')
    if (stored === 'light' || stored === 'dark') return stored
    if (window.matchMedia('(prefers-color-scheme: light)').matches) return 'light'
    return 'dark'
  }

  const theme = ref<'dark' | 'light'>(getInitialTheme())

  function applyTheme(t: 'dark' | 'light') {
    if (t === 'light') {
      document.documentElement.classList.add('light')
    } else {
      document.documentElement.classList.remove('light')
    }
  }

  // Apply on store creation
  applyTheme(theme.value)

  // Watch and propagate changes
  watch(theme, (t) => {
    localStorage.setItem('theme', t)
    applyTheme(t)
  })

  function toggleTheme() {
    theme.value = theme.value === 'dark' ? 'light' : 'dark'
  }

  const isDark = () => theme.value === 'dark'

  return { theme, toggleTheme, isDark }
})

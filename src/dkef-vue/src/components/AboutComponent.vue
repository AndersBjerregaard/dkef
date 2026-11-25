<script setup lang="ts">
import { computed, onMounted } from 'vue'
import ContactFormularComponent from '@/components/ContactFormularComponent.vue'
import SiteTextEditAction from '@/components/SiteTextEditAction.vue'
import SiteImage from '@/components/SiteImage.vue'
import SiteImageEditAction from '@/components/SiteImageEditAction.vue'
import { useSiteTextStore } from '@/stores/siteTextStore'

const siteTexts = useSiteTextStore()
const texts = computed(() => siteTexts.texts.about)

onMounted(() => {
  // Public failures leave the registered defaults readable; the editor handles its own errors.
  void siteTexts.loadGroup('about').catch(() => {})
})
</script>

<template>
  <div class="about-text py-24">
    <div class="flex flex-wrap justify-center gap-3 px-4">
      <SiteTextEditAction group="about" />
      <SiteImageEditAction group="about" />
    </div>
    <div class="flex-row">
      <div class="flex justify-center items-center pb-4">
        <h1 class="text-4xl">{{ texts['page.title'] }}</h1>
      </div>
      <div class="flex justify-center items-center pb-8">
        <h2 class="text-2xl">{{ texts['page.subtitle'] }}</h2>
      </div>
    </div>
    <div class="flex justify-center items-center gap-x-4 pb-8">
      <div class="w-[45%] min-w-0">
        <div class="flex justify-center items-cemter pb-4">
          <h2 class="text-xl">{{ texts['introduction.title'] }}</h2>
        </div>
        <div class="min-h-80 flex justify-center items-center text-center">
          <p class="text-lg whitespace-pre-line">{{ texts['introduction.description'] }}</p>
        </div>
      </div>
      <div class="w-[45%] min-w-0">
        <div class="flex justify-center items-center pb-4">
          <h2 class="text-xl">{{ texts['sections.title'] }}</h2>
        </div>
        <div class="h-80 bg-theme-mute rounded-xl overflow-hidden">
          <SiteImage
            style="width: 100%; height: 100%; object-fit: contain"
            group="about"
            image-slot="sections"
          />
        </div>
      </div>
    </div>
    <div class="flex-row pb-8">
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">{{ texts['statistics.members.label'] }}</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-6xl text-theme-accent">+550</h1>
      </div>
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">{{ texts['statistics.sections.label'] }}</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-6xl text-theme-accent">5</h1>
      </div>
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">{{ texts['statistics.founded.label'] }}</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-6xl text-theme-accent">1903</h1>
      </div>
    </div>
    <div class="flex justify-center items-center px-4 gap-x-4 pb-24">
      <div class="w-[45%] min-w-0 min-h-96">
        <div class="flex justify-center items-center text-center">
          <h1 class="text-2xl pb-4">{{ texts['secretariat.title'] }}</h1>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">{{ texts['secretariat.name'] }}</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">{{ texts['secretariat.address'] }}</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">{{ texts['secretariat.postal-city'] }}</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">Tlf. {{ texts['secretariat.phone'] }}</h3>
        </div>
        <div class="flex justify-center items-center text-center pb-4">
          <h3 class="text-xl pb-2">Email: {{ texts['secretariat.email'] }}</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <div class="h-48 w-48 border-2 border-amber-500/30 rounded-full overflow-hidden">
            <SiteImage
              class="h-full w-full object-cover rounded-full"
              group="about"
              image-slot="secretariat"
            />
          </div>
        </div>
      </div>
      <div class="w-[45%] min-w-0">
        <h1 class="text-2xl pb-4">{{ texts['contact.title'] }}</h1>
        <ContactFormularComponent />
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Keep legal unbroken text inside its slot without changing the page or nested form. */
.about-text :is(h1, h2, h3, p) {
  min-width: 0;
  overflow-wrap: anywhere;
}
</style>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { computed, onMounted } from 'vue'
import MigrationBanner from '@/components/MigrationBanner.vue'
import SiteTextEditAction from '@/components/SiteTextEditAction.vue'
import { useSiteTextStore } from '@/stores/siteTextStore'

const router = useRouter()
const siteTexts = useSiteTextStore()
const texts = computed(() => siteTexts.texts.home)
const membership = computed(() => siteTexts.texts.membership)

onMounted(() => {
  // Load independently: a public failure in either group leaves its defaults readable.
  for (const group of ['home', 'membership']) void siteTexts.loadGroup(group).catch(() => {})
})

async function memberAdvantages() {
  await router.push({ name: 'advantages' })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

async function register() {
  await router.push({ name: 'register' })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <!-- Migration Banner -->
  <MigrationBanner />

  <div class="home-text">
    <div class="flex flex-wrap justify-center gap-3 px-4">
      <SiteTextEditAction group="home" label="Rediger forsidens tekster" />
      <SiteTextEditAction
        group="membership"
        label="Rediger fælles medlemsfordele"
        description="Du redigerer de fem fælles medlemsfordele. Ændringer vises både på forsiden og på siden Medlemsfordele. Forsidens øvrige tekster gemmes separat."
      />
    </div>

    <!-- Hero -->
    <div class="py-12 sm:py-20 px-4">
      <div class="flex flex-col md:flex-row justify-center items-center gap-x-12">
        <!-- Left Column -->
        <div class="w-full md:w-96 px-2 pb-8 md:pb-0">
          <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-4">
            {{ texts['hero.eyebrow'] }}
          </p>
          <h1 class="text-4xl pb-8 text-theme-heading leading-tight">{{ texts['hero.title'] }}</h1>

          <p class="text-lg pb-8 text-theme-text leading-relaxed whitespace-pre-line">
            {{ texts['hero.description'] }}
          </p>

          <p class="text-lg pb-4 text-theme-accent font-medium">
            ⭐ 4.8/5 stjerner af vores 100+ anmeldelser.
          </p>
        </div>

        <div class="w-full md:w-96 px-2">
          <img class="rounded-xl w-full" src="@/assets/el-mast.jpeg" alt="An electrical mast" />
        </div>
      </div>
    </div>

    <!-- Member companies strip -->
    <div
      class="bg-theme-mute border-y border-amber-500/15 px-4 py-12 flex justify-center items-center"
    >
      <div class="max-w-4xl mx-auto">
        <div class="pb-8 flex items-center justify-center">
          <h2 class="text-theme-text font-medium text-lg uppercase tracking-widest">
            {{ texts['companies.title'] }}
          </h2>
        </div>

        <div class="flex flex-wrap justify-center items-center gap-4">
          <div
            class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity"
          >
            <img src="@/assets/cubic.png" alt="CUBIC" />
          </div>

          <div
            class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity"
          >
            <img src="@/assets/ledvance.png" alt="LEDVANCE" />
          </div>

          <div
            class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-100 hover:opacity-100 transition-opacity"
          >
            <img src="@/assets/wexoe.svg" alt="WEXOE" />
          </div>

          <div
            class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-100 hover:opacity-100 transition-opacity"
          >
            <img src="@/assets/schneider.png" alt="Schneider" />
          </div>

          <div
            class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity"
          >
            <img src="@/assets/lauritz.svg" alt="Lauritz Knudsen" />
          </div>

          <div
            class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity"
          >
            <img src="@/assets/solar.png" alt="Solar" />
          </div>
        </div>
      </div>
    </div>

    <!-- Info section 1 -->
    <div class="bg-theme-soft border-y border-amber-500/10 px-4 py-12 sm:py-20">
      <div class="flex flex-col md:flex-row justify-center items-center gap-x-12">
        <div class="hidden sm:flex w-full md:w-96 px-2">
          <img class="rounded-xl w-full" src="@/assets/man-studying.png" alt="Man studying" />
        </div>

        <div class="w-full md:w-96 px-2 pb-8 md:pb-0">
          <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
            {{ texts['network.eyebrow'] }}
          </p>
          <h2 class="text-3xl text-theme-heading leading-tight pb-4">
            {{ texts['network.title'] }}
          </h2>

          <p class="text-lg text-theme-text leading-relaxed pb-6 whitespace-pre-line">
            {{ texts['network.description'] }}
          </p>

          <button
            class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
            @click="memberAdvantages"
          >
            {{ texts['network.action-label'] }}
          </button>
        </div>
      </div>
    </div>

    <!-- Info section 2 -->
    <div class="px-4 py-12 sm:py-20">
      <div class="flex flex-col md:flex-row justify-center items-center gap-x-12">
        <div class="w-full md:w-96 px-2 pb-8 md:pb-0">
          <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
            {{ texts['development.eyebrow'] }}
          </p>
          <h2 class="text-3xl text-theme-heading leading-tight pb-4">
            {{ texts['development.title'] }}
          </h2>

          <p class="text-lg text-theme-text leading-relaxed pb-6 whitespace-pre-line">
            {{ texts['development.description'] }}
          </p>

          <button
            class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
            @click="register"
          >
            {{ texts['development.action-label'] }}
          </button>
        </div>

        <div class="hidden sm:flex w-full md:w-96 px-2">
          <img class="rounded-xl w-full" src="@/assets/corporate-woman.png" alt="Corporate woman" />
        </div>
      </div>
    </div>

    <!-- Reasons to become a member -->
    <div class="bg-theme-soft border-y border-amber-500/10 px-4 py-12 sm:py-20">
      <div class="flex flex-col md:flex-row justify-center items-center gap-x-12">
        <div class="w-full md:w-96 px-2 pb-8 md:pb-0">
          <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
            {{ texts['reasons.eyebrow'] }}
          </p>
          <h2 class="text-3xl text-theme-heading leading-tight pb-4">
            {{ texts['reasons.title'] }}
          </h2>

          <p class="text-lg text-theme-text leading-relaxed pb-6 whitespace-pre-line">
            {{ texts['reasons.description'] }}
          </p>

          <button
            class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
            @click="register"
          >
            {{ texts['reasons.action-label'] }}
          </button>
        </div>

        <div class="flex flex-col gap-4 w-full md:w-96 px-2">
          <div
            class="bg-theme-mute border border-theme-border hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
          >
            <h3 class="text-xl text-amber-300 pb-2 font-semibold">
              {{ texts['reasons.sparring.title'] }}
            </h3>
            <p class="text-theme-text whitespace-pre-line">
              {{ texts['reasons.sparring.description'] }}
            </p>
          </div>

          <div
            class="bg-theme-mute border border-theme-border hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
          >
            <h3 class="text-xl text-amber-300 pb-2 font-semibold">
              {{ texts['reasons.network.title'] }}
            </h3>
            <p class="text-theme-text whitespace-pre-line">
              {{ texts['reasons.network.description'] }}
            </p>
          </div>

          <div
            class="bg-theme-mute border border-theme-border hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
          >
            <h3 class="text-xl text-amber-300 pb-2 font-semibold">
              {{ texts['reasons.development.title'] }}
            </h3>
            <p class="text-theme-text whitespace-pre-line">
              {{ texts['reasons.development.description'] }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Member plans -->
    <div
      class="flex justify-center items-center py-16 bg-theme-soft border-y border-amber-500/10 px-4"
    >
      <div class="max-w-3xl mx-auto">
        <div class="flex justify-center pb-12">
          <h2 class="text-4xl text-theme-heading">{{ texts['plans.title'] }}</h2>
        </div>
        <div class="flex justify-center">
          <div
            class="p-6 border border-amber-500/30 rounded-2xl bg-theme-mute shadow-xl shadow-amber-500/5 w-80 max-w-full"
          >
            <div class="h-1 w-12 bg-amber-500 rounded-full mb-6"></div>
            <div class="pb-6">
              <h3 class="text-2xl text-theme-heading">{{ texts['plan.title'] }}</h3>
            </div>
            <div class="flex items-end pb-2">
              <p class="text-4xl text-theme-accent font-bold">700,-</p>
              <span class="px-3 pb-1 text-theme-text text-sm">inkl. moms pr. år</span>
            </div>
            <p class="pt-4 text-theme-text whitespace-pre-line">
              {{ texts['plan.description'] }}
            </p>
            <p class="pt-2 pb-6 text-theme-text whitespace-pre-line">
              {{ texts['plan.activities-description'] }}
            </p>
            <button
              class="flex justify-center rounded-lg bg-amber-600 h-12 py-2 w-full cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
              @click="register"
            >
              {{ texts['plan.action-label'] }}
            </button>
            <div class="pt-6 border-t border-theme-border mt-4">
              <p class="py-2 text-theme-text">✅ {{ membership['benefits.development'] }}</p>
              <p class="pb-2 text-theme-text">✅ {{ membership['benefits.network'] }}</p>
              <p class="pb-2 text-theme-text">✅ {{ membership['benefits.mentoring'] }}</p>
              <p class="pb-2 text-theme-text">✅ {{ membership['benefits.events'] }}</p>
              <p class="text-theme-text">✅ {{ membership['benefits.magazine'] }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- News Letter temporarily disabled -->
    <!-- <div class="pt-16">
    <NewsLetterComponent />
  </div> -->
  </div>
</template>

<style scoped>
/* Editable text can grow without widening the developer-owned columns/cards. */
.home-text :is(div, h1, h2, h3, p, button) {
  min-width: 0;
}

.home-text :is(h1, h2, h3, p, button) {
  overflow-wrap: anywhere;
}

.home-text button {
  max-width: 100%;
  min-height: 3rem;
  height: auto;
}
</style>

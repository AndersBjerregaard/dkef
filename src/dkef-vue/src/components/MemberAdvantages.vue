<script setup lang="ts">
import { useRouter } from 'vue-router'
import { computed, onMounted } from 'vue'
import SiteTextEditAction from '@/components/SiteTextEditAction.vue'
import SiteImage from '@/components/SiteImage.vue'
import SiteImageEditAction from '@/components/SiteImageEditAction.vue'
import { useSiteTextStore } from '@/stores/siteTextStore'

const router = useRouter()
const siteTexts = useSiteTextStore()
const texts = computed(() => siteTexts.texts['member-advantages'])
const membership = computed(() => siteTexts.texts.membership)

onMounted(() => {
  // Load independently so either group can fall back to its readable defaults.
  for (const group of ['member-advantages', 'membership'])
    void siteTexts.loadGroup(group).catch(() => {})
})

async function goto(view: string) {
  await router.push({ name: view })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <div class="member-advantages-text">
    <!-- Title -->
    <div class="pt-24 flex justify-center items-center text-center">
      <h1 class="text-4xl">{{ texts['page.title'] }}</h1>
    </div>
    <div class="flex flex-wrap justify-center gap-3 px-4">
      <SiteTextEditAction group="member-advantages" label="Rediger sidens tekster" />
      <SiteImageEditAction group="member-advantages" />
      <SiteTextEditAction
        group="membership"
        label="Rediger fælles medlemsfordele"
        description="Du redigerer de fem fælles medlemsfordele. Ændringer vises både på forsiden og på siden Medlemsfordele. Sidens øvrige tekster gemmes separat."
      />
    </div>

    <!-- First section -->
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 px-4 py-12 sm:py-20">
      <div class="gap-4 w-full md:w-96 px-2 pb-8 md:pb-0">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
          {{ texts['introduction.eyebrow'] }}
        </p>
        <h1 class="text-3xl md:text-4xl pb-6 text-theme-heading leading-tight">
          {{ texts['introduction.title'] }}
        </h1>
        <p class="pb-8 text-theme-text leading-relaxed whitespace-pre-line">
          {{ texts['introduction.description'] }}
        </p>
        <button
          class="flex justify-center rounded-lg bg-amber-600 h-12 px-8 py-2 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/25 transition-all"
          @click="goto('register')"
        >
          {{ texts['introduction.action-label'] }}
        </button>
      </div>

      <div class="w-full md:w-96 px-2">
        <div
          class="bg-theme-mute border border-amber-500/30 rounded-2xl p-6 shadow-xl shadow-amber-500/5"
        >
          <!-- Amber top accent bar -->
          <div class="h-1 w-16 bg-amber-500 rounded-full mb-6"></div>
          <div class="flex justify-center mb-2">
            <h2 class="text-xl font-semibold text-theme-accent">{{ texts['membership.title'] }}</h2>
          </div>
          <div class="border-t border-theme-border pt-4 pb-4">
            <p class="py-2 text-theme-text">✅ {{ membership['benefits.development'] }}</p>
            <p class="pb-2 text-theme-text">✅ {{ membership['benefits.network'] }}</p>
            <p class="pb-2 text-theme-text">✅ {{ membership['benefits.mentoring'] }}</p>
            <p class="pb-2 text-theme-text">✅ {{ membership['benefits.events'] }}</p>
            <p class="pb-2 text-theme-text">✅ {{ membership['benefits.magazine'] }}</p>
          </div>
          <div class="flex justify-center mb-2">
            <h2 class="text-xl font-semibold text-theme-accent">
              {{ texts['student-offers.title'] }}
            </h2>
          </div>
          <div class="border-t border-theme-border pt-4">
            <p class="py-2 flex items-start gap-2 text-theme-text">
              <span class="text-amber-500 mt-1 shrink-0">•</span>
              <span>{{ texts['student-offers.mentoring'] }}</span>
            </p>
            <p class="pb-2 flex items-start gap-2 text-theme-text">
              <span class="text-amber-500 mt-1 shrink-0">•</span>
              <span>{{ texts['student-offers.network-meetings'] }}</span>
            </p>
            <p class="pb-2 flex items-start gap-2 text-theme-text">
              <span class="text-amber-500 mt-1 shrink-0">•</span>
              <span>{{ texts['student-offers.project-network'] }}</span>
            </p>
            <p class="pb-2 flex items-start gap-2 text-theme-text">
              <span class="text-amber-500 mt-1 shrink-0">•</span>
              <span>{{ texts['student-offers.jobs'] }}</span>
            </p>
            <p class="pb-2 flex items-start gap-2 text-theme-text">
              <span class="text-amber-500 mt-1 shrink-0">•</span>
              <span>{{ texts['student-offers.included-benefits'] }}</span>
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Divider -->
    <div class="w-full border-t border-amber-500/10"></div>

    <!-- Second section -->
    <div
      class="bg-theme-soft flex flex-col md:flex-row justify-center items-center gap-x-12 px-4 py-12 sm:py-20"
    >
      <div class="gap-4 w-full md:w-96 px-2 pb-8 md:pb-0">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
          {{ texts['network.eyebrow'] }}
        </p>
        <h1 class="text-2xl sm:text-4xl pb-6 text-theme-heading leading-tight">
          {{ texts['network.title'] }}
        </h1>
        <p class="pb-6 text-theme-text leading-relaxed whitespace-pre-line">
          {{ texts['network.description'] }}
        </p>
      </div>

      <div class="hidden sm:flex w-96 gap-4 items-start">
        <SiteImage
          class="rounded-xl object-cover w-full"
          group="member-advantages"
          image-slot="network"
        />
      </div>
    </div>

    <!-- Divider -->
    <div class="w-full border-t border-amber-500/10"></div>

    <!-- Third section -->
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 px-4 py-12 sm:py-20">
      <div class="gap-4 w-full md:w-96 px-2 pb-8 md:pb-0">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
          {{ texts['events.eyebrow'] }}
        </p>
        <h1 class="text-2xl sm:text-4xl pb-6 text-theme-heading leading-tight">
          {{ texts['events.title'] }}
        </h1>
        <p class="pb-6 text-theme-text leading-relaxed whitespace-pre-line">
          {{ texts['events.description'] }}
        </p>
      </div>

      <div class="hidden sm:flex w-96 gap-4 items-start">
        <SiteImage
          class="rounded-xl object-cover w-full"
          group="member-advantages"
          image-slot="events"
        />
      </div>
    </div>

    <!-- Divider -->
    <div class="w-full border-t border-amber-500/10"></div>

    <!-- Fourth section -->
    <div class="bg-theme-soft px-4 py-16">
      <div class="text-center mb-10">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">
          {{ texts['other-benefits.eyebrow'] }}
        </p>
        <h1 class="text-2xl sm:text-3xl text-theme-heading">
          {{ texts['other-benefits.title'] }}
        </h1>
      </div>
      <!-- Triple columns -->
      <div class="flex flex-wrap justify-center gap-6 py-4">
        <!-- First column -->
        <div
          class="w-72 bg-theme-mute rounded-2xl border border-theme-border hover:border-amber-500/40 transition-all duration-300 shadow-lg hover:-translate-y-1 overflow-hidden"
        >
          <div class="h-44 overflow-hidden">
            <SiteImage
              class="w-full h-full object-cover"
              group="member-advantages"
              image-slot="mentoring"
            />
          </div>
          <div class="p-5">
            <h2 class="text-lg font-semibold text-amber-300 pb-3">
              {{ texts['mentoring.title'] }}
            </h2>
            <p class="text-theme-text text-sm leading-relaxed whitespace-pre-line">
              {{ texts['mentoring.description'] }}
            </p>
          </div>
        </div>

        <!-- Second column -->
        <div
          class="w-72 bg-theme-mute rounded-2xl border border-theme-border hover:border-amber-500/40 transition-all duration-300 shadow-lg hover:-translate-y-1 overflow-hidden"
        >
          <div class="h-44 overflow-hidden">
            <SiteImage
              class="w-full h-full object-cover"
              group="member-advantages"
              image-slot="sparring"
            />
          </div>
          <div class="p-5">
            <h2 class="text-lg font-semibold text-amber-300 pb-3">
              {{ texts['sparring.title'] }}
            </h2>
            <p class="text-theme-text text-sm leading-relaxed whitespace-pre-line">
              {{ texts['sparring.description'] }}
            </p>
          </div>
        </div>

        <!-- Third column -->
        <div
          class="w-72 bg-theme-mute rounded-2xl border border-theme-border hover:border-amber-500/40 transition-all duration-300 shadow-lg hover:-translate-y-1 overflow-hidden"
        >
          <div class="h-44 overflow-hidden">
            <SiteImage
              class="w-full h-full object-cover"
              group="member-advantages"
              image-slot="magazine"
            />
          </div>
          <div class="p-5">
            <h2 class="text-lg font-semibold text-amber-300 pb-3">
              {{ texts['magazine.title'] }}
            </h2>
            <p class="text-theme-text text-sm leading-relaxed whitespace-pre-line">
              {{ texts['magazine.description'] }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Editable text grows within the existing developer-owned columns and cards. */
.member-advantages-text :is(div, h1, h2, p, button, span) {
  min-width: 0;
}

.member-advantages-text :is(h1, h2, p, button, span) {
  overflow-wrap: anywhere;
}

.member-advantages-text button {
  max-width: 100%;
  min-height: 3rem;
  height: auto;
}
</style>

<script setup lang="ts">
import NewsLetterComponent from './NewsLetterComponent.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

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
  <!-- Hero -->
  <div class="py-12 sm:py-20 px-4">
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <!-- Left Column -->
      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-4">
          Elektroteknisk forening
        </p>
        <h1 class="text-4xl pb-8 text-theme-heading leading-tight">Dit netværk i elbranchen</h1>

        <p class="text-lg pb-8 text-theme-text leading-relaxed">
          Udvid din faglige horisont, bliv en del af fællesskabet og modtag sparring fra erfarne
          hænder.
        </p>

        <p class="text-lg pb-4 text-theme-accent font-medium">
          ⭐ 4.8/5 stjerner af vores 100+ anmeldelser.
        </p>
      </div>

      <div class="w-full md:w-96 px-2 mx-auto">
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
          Vores medlemsvirksomheder
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
          class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity"
        >
          <img src="@/assets/wexoe.svg" alt="WEXOE" />
        </div>

        <div
          class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity"
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
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <div class="hidden sm:flex w-full md:w-96 px-2 mx-auto">
        <img class="rounded-xl w-full" src="@/assets/man-studying.png" alt="Man studying" />
      </div>

      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">Netværk</p>
        <h2 class="text-3xl text-theme-heading leading-tight pb-4">
          Sparring og relationer på tværs af branchen
        </h2>

        <p class="text-lg text-theme-text leading-relaxed pb-6">
          Med over 550 medlemmer er du garanteret nye forbindelser og indsigter inden for
          forskellige eltekniske områder.
        </p>

        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
          @click="memberAdvantages"
        >
          Medlemsfordele
        </button>
      </div>
    </div>
  </div>

  <!-- Info section 2 -->
  <div class="px-4 py-12 sm:py-20">
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">Udvikling</p>
        <h2 class="text-3xl text-theme-heading leading-tight pb-4">Fokus på faglig udvikling</h2>

        <p class="text-lg text-theme-text leading-relaxed pb-6">
          Bliv klogere på de nyeste tendenser og andre specialeområder ved at deltage i
          arrangementer og virksomhedsbesøg.
        </p>

        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
          @click="register"
        >
          Bliv medlem
        </button>
      </div>

      <div class="hidden sm:flex w-full md:w-96 px-2 mx-auto">
        <img class="rounded-xl w-full" src="@/assets/corporate-woman.png" alt="Corporate woman" />
      </div>
    </div>
  </div>

  <!-- Reasons to become a member -->
  <div class="bg-theme-soft border-y border-amber-500/10 px-4 py-12 sm:py-20">
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-theme-accent text-lg font-bold uppercase tracking-widest pb-3">Hvorfor?</p>
        <h2 class="text-3xl text-theme-heading leading-tight pb-4">
          3 gode grunde til at være medlem
        </h2>

        <p class="text-lg text-theme-text leading-relaxed pb-6">
          Stadig i tvivl? Lad os lige skitsere, hvad du får ud af et medlemskab.
        </p>

        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
          @click="register"
        >
          Bliv medlem
        </button>
      </div>

      <div class="flex flex-col gap-4 w-full md:w-96 px-2 mx-auto">
        <div
          class="bg-theme-mute border border-theme-border hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
        >
          <h3 class="text-xl text-amber-300 pb-2 font-semibold">Sparring</h3>
          <p class="text-theme-text">Diskuter og udveksl ideer med andre</p>
        </div>

        <div
          class="bg-theme-mute border border-theme-border hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
        >
          <h3 class="text-xl text-amber-300 pb-2 font-semibold">Netværk</h3>
          <p class="text-theme-text">Få nye forbindelser — og måske venskaber</p>
        </div>

        <div
          class="bg-theme-mute border border-theme-border hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
        >
          <h3 class="text-xl text-amber-300 pb-2 font-semibold">Faglig udvikling</h3>
          <p class="text-theme-text">Styrk kompetencer og opnå ny viden</p>
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
        <h2 class="text-4xl text-theme-heading">Vores medlemstyper</h2>
      </div>
      <div class="flex justify-center">
        <div
          class="p-6 border border-amber-500/30 rounded-2xl bg-theme-mute shadow-xl shadow-amber-500/5 w-80"
        >
          <div class="h-1 w-12 bg-amber-500 rounded-full mb-6"></div>
          <div class="pb-6">
            <h3 class="text-2xl text-theme-heading">Bliv medlem</h3>
          </div>
          <div class="flex items-end pb-2">
            <p class="text-4xl text-theme-accent font-bold">700,-</p>
            <span class="px-3 pb-1 text-theme-text text-sm">inkl. moms pr. år</span>
          </div>
          <p class="pt-4 text-theme-text">
            Giv dig selv de bedste betingelser for et stærkt uddannelsesforløb.
          </p>
          <p class="pt-2 pb-6 text-theme-text">
            Deltag i studieture, del ud af din viden og styrk dit arbejdsnetværk.
          </p>
          <button
            class="flex justify-center rounded-lg bg-amber-600 h-12 py-2 w-full cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
            @click="register"
          >
            Bliv medlem
          </button>
          <div class="pt-6 border-t border-theme-border mt-4">
            <p class="py-2 text-theme-text">✅ Faglig udvikling</p>
            <p class="pb-2 text-theme-text">✅ Styrk dit netværk</p>
            <p class="pb-2 text-theme-text">✅ Mentorordning</p>
            <p class="pb-2 text-theme-text">✅ Studieture og arrangementer</p>
            <p class="text-theme-text">✅ Modtag EL + Energi bladet</p>
          </div>
        </div>
      </div>
    </div>
  </div>

  <div class="pt-16">
    <NewsLetterComponent />
  </div>
</template>

<style lang="css" scoped></style>

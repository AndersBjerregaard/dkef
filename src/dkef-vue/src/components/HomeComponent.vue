<script setup lang="ts">
import NewsLetterComponent from './NewsLetterComponent.vue'
</script>

<template>
  <!-- Hero -->
  <div class="py-12 sm:py-20 px-4">
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <!-- Left Column -->
      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest pb-4">
          Elektroteknisk forening
        </p>
        <h1 class="text-4xl pb-8 text-slate-100 leading-tight">Dit netværk i elbranchen</h1>

        <p class="text-lg pb-8 text-slate-400 leading-relaxed">
          Udvid din faglige horisont, bliv en del af fællesskabet og modtag sparring fra erfarne
          hænder.
        </p>

        <p class="text-lg pb-4 text-amber-400 font-medium">
          ⭐ 4.8/5 stjerner af vores 100+ anmeldelser.
        </p>
      </div>

      <div class="w-full md:w-96 px-2 mx-auto">
        <img class="rounded-xl w-full" src="@/assets/el-mast.jpeg" alt="An electrical mast" />
      </div>
    </div>
  </div>

  <!-- Member companies strip -->
  <div class="bg-navy-900 border-y border-amber-500/15 px-4 py-12 flex justify-center items-center">
    <div class="max-w-4xl mx-auto">
      <div class="pb-8 flex items-center justify-center">
        <h2 class="text-slate-400 font-medium text-lg uppercase tracking-widest">
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

  <!-- Upcoming events -->
  <div class="py-16 px-4 flex justify-center items-center">
    <div class="max-w-3xl mx-auto">
      <div class="flex justify-center pb-8">
        <h2 class="text-4xl text-slate-100">Kommende begivenheder</h2>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-10">
        <div>
          <h3 class="text-3xl text-slate-100 pb-4">Arrangementer</h3>

          <div class="py-4">
            <div class="border border-navy-700 rounded-2xl p-4 bg-navy-800">
              <h4 class="text-xl pb-4 text-slate-100">Arrangement titel</h4>
              <div
                class="p-4 items-center flex justify-between bg-navy-900 rounded-xl text-sm md:text-base lg:text-lg text-slate-400"
              >
                <span>Dato</span>
                <span>Sektion</span>
                <span>Lokation</span>
              </div>
            </div>
          </div>

          <div class="py-4">
            <div class="border border-navy-700 rounded-2xl p-4 bg-navy-800">
              <h4 class="text-xl pb-4 text-slate-100">Arrangement titel</h4>
              <div
                class="p-4 items-center flex justify-between bg-navy-900 rounded-xl text-sm md:text-base lg:text-lg text-slate-400"
              >
                <span>Dato</span>
                <span>Sektion</span>
                <span>Lokation</span>
              </div>
            </div>
          </div>
        </div>

        <div>
          <h3 class="text-3xl text-slate-100 pb-4">Generalforsamlinger</h3>

          <div class="pt-8">
            <p class="text-xl text-slate-400">
              Der er ingen planlagte generalforsamlinger foreløbigt.
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Info section 1 -->
  <div class="bg-navy-900 border-y border-amber-500/10 px-4 py-12 sm:py-20">
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <div class="hidden sm:flex w-full md:w-96 px-2 mx-auto">
        <img class="rounded-xl w-full" src="@/assets/man-studying.png" alt="Man studying" />
      </div>

      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest pb-3">Netværk</p>
        <h2 class="text-3xl text-slate-100 leading-tight pb-4">
          Sparring og relationer på tværs af branchen
        </h2>

        <p class="text-lg text-slate-400 leading-relaxed pb-6">
          Med over 550 medlemmer er du garanteret nye forbindelser og indsigter inden for
          forskellige eltekniske områder.
        </p>

        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
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
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest pb-3">Udvikling</p>
        <h2 class="text-3xl text-slate-100 leading-tight pb-4">Fokus på faglig udvikling</h2>

        <p class="text-lg text-slate-400 leading-relaxed pb-6">
          Bliv klogere på de nyeste tendenser og andre specialeområder ved at deltage i
          arrangementer og virksomhedsbesøg.
        </p>

        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
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
  <div class="bg-navy-900 border-y border-amber-500/10 px-4 py-12 sm:py-20">
    <div class="flex flex-col md:flex-row justify-center items-center gap-x-12 mx-auto">
      <div class="w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest pb-3">Hvorfor?</p>
        <h2 class="text-3xl text-slate-100 leading-tight pb-4">3 gode grunde til at være medlem</h2>

        <p class="text-lg text-slate-400 leading-relaxed pb-6">
          Stadig i tvivl? Lad os lige skitsere, hvad du får ud af et medlemskab.
        </p>

        <button
          class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
        >
          Bliv medlem
        </button>
      </div>

      <div class="flex flex-col gap-4 w-full md:w-96 px-2 mx-auto">
        <div
          class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
        >
          <h3 class="text-xl text-amber-300 pb-2 font-semibold">Sparring</h3>
          <p class="text-slate-400">Diskuter og udveksl ideer med andre</p>
        </div>

        <div
          class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
        >
          <h3 class="text-xl text-amber-300 pb-2 font-semibold">Netværk</h3>
          <p class="text-slate-400">Få nye forbindelser — og måske venskaber</p>
        </div>

        <div
          class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl p-4 transition-colors"
        >
          <h3 class="text-xl text-amber-300 pb-2 font-semibold">Faglig udvikling</h3>
          <p class="text-slate-400">Styrk kompetencer og opnå ny viden</p>
        </div>
      </div>
    </div>
  </div>

  <!-- Member plans -->
  <div class="flex justify-center items-center py-16 bg-navy-900 border-y border-amber-500/10 px-4">
    <div class="max-w-3xl mx-auto">
      <div class="flex justify-center pb-12">
        <h2 class="text-4xl text-slate-100">Vores medlemstyper</h2>
      </div>
      <div class="flex justify-center">
        <div
          class="p-6 border border-amber-500/30 rounded-2xl bg-navy-800 shadow-xl shadow-amber-500/5 w-80"
        >
          <div class="h-1 w-12 bg-amber-500 rounded-full mb-6"></div>
          <div class="pb-6">
            <h3 class="text-2xl text-slate-100">Bliv medlem</h3>
          </div>
          <div class="flex items-end pb-2">
            <p class="text-4xl text-amber-400 font-bold">700,-</p>
            <span class="px-3 pb-1 text-slate-400 text-sm">inkl. moms pr. år</span>
          </div>
          <p class="pt-4 text-slate-400">
            Giv dig selv de bedste betingelser for et stærkt uddannelsesforløb.
          </p>
          <p class="pt-2 pb-6 text-slate-400">
            Deltag i studieture, del ud af din viden og styrk dit arbejdsnetværk.
          </p>
          <button
            class="flex justify-center rounded-lg bg-amber-600 h-12 py-2 w-full cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all"
          >
            Bliv medlem
          </button>
          <div class="pt-6 border-t border-navy-700 mt-4">
            <p class="py-2 text-slate-300">✅ Faglig udvikling</p>
            <p class="pb-2 text-slate-300">✅ Styrk dit netværk</p>
            <p class="pb-2 text-slate-300">✅ Mentorordning</p>
            <p class="pb-2 text-slate-300">✅ Studieture og arrangementer</p>
            <p class="text-slate-300">✅ Modtag EL + Energi bladet</p>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- News -->
  <div class="flex justify-center items-center py-16 border-b border-amber-500/10 px-4">
    <div class="max-w-3xl mx-auto">
      <div class="text-center pb-12">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest pb-4">
          Aktiviteter
        </p>
        <h2 class="text-4xl text-slate-100 pb-4">Nyheder og aktiviteter</h2>
        <h3 class="text-xl text-slate-400">Følg med i de seneste nyheder og aktiviteter</h3>
      </div>

      <div class="flex flex-wrap justify-center gap-6">
        <div
          class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl overflow-hidden transition-all hover:-translate-y-1 shadow-lg w-72"
        >
          <img
            class="w-full object-cover h-48"
            src="@/assets/stickman-bowling.jpeg"
            alt="activity picture"
          />
          <div class="p-4">
            <h3 class="pt-4 pb-2 text-xl text-slate-100">Arrangement titel</h3>
            <p class="text-lg pb-2 text-amber-300">Arrangement undertitel</p>
            <p class="text-sm text-slate-400">Dato</p>
            <span class="text-sm text-slate-400">Lokation</span>
          </div>
        </div>
        <div
          class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl overflow-hidden transition-all hover:-translate-y-1 shadow-lg w-72"
        >
          <img
            class="w-full object-cover h-48"
            src="@/assets/stickman-party.jpeg"
            alt="activity picture"
          />
          <div class="p-4">
            <h3 class="pt-4 pb-2 text-xl text-slate-100">Arrangement titel</h3>
            <p class="text-lg pb-2 text-amber-300">Arrangement undertitel</p>
            <p class="text-sm text-slate-400">Dato</p>
            <span class="text-sm text-slate-400">Lokation</span>
          </div>
        </div>
        <div
          class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl overflow-hidden transition-all hover:-translate-y-1 shadow-lg w-72"
        >
          <img
            class="w-full object-cover h-48"
            src="@/assets/stickmen-driving.jpeg"
            alt="activity picture"
          />
          <div class="p-4">
            <h3 class="pt-4 pb-2 text-xl text-slate-100">Arrangement titel</h3>
            <p class="text-lg pb-2 text-amber-300">Arrangement undertitel</p>
            <p class="text-sm text-slate-400">Dato</p>
            <span class="text-sm text-slate-400">Lokation</span>
          </div>
        </div>
      </div>
    </div>
  </div>

  <NewsLetterComponent />
</template>

<style lang="css" scoped></style>

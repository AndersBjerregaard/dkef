<script setup lang="ts">
import NewsLetterComponent from './NewsLetterComponent.vue'
</script>

<template>
  <div>
    <div class="grid grid-cols-2 justify-center gap-x-8">
      <!-- Left Column -->
      <div class="flex flex-col gap-4 items-end">
        <div class="w-5/6">
          <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest pt-12 pb-4">
            Elektroteknisk forening
          </p>
          <h1 class="text-4xl pb-8 text-slate-100 leading-tight">Dit netværk i elbranchen</h1>

          <p class="text-lg pb-8 text-slate-400 leading-relaxed">
            Udvid din faglige horisont, bliv en del af fællesskabet og modtag sparring fra erfarne
            hænder.
          </p>

          <p class="text-lg pb-12 text-amber-400 font-medium">⭐ 4.8/5 stjerner af vores 100+ anmeldelser.</p>
        </div>
      </div>
      <!--Right Column -->
      <div class="flex flex-col gap-4 items-start">
        <div class="w-5/6">
          <img class="rounded-xl" src="@/assets/el-mast.jpeg" alt="An electrical mast" />
        </div>
      </div>
    </div>

    <!-- Member companies strip -->
    <div class="bg-navy-900 border-y border-amber-500/15 px-8 py-12">
      <div class="text-2xl pb-8 flex items-center justify-center">
        <h2 class="text-slate-400 font-medium text-lg uppercase tracking-widest">Vores medlemsvirksomheder</h2>
      </div>

      <div class="flex justify-between items-center">
        <div class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity">
          <img src="@/assets/cubic.png" alt="CUBIC" />
        </div>

        <div class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity">
          <img src="@/assets/ledvance.png" alt="LEDVANCE" />
        </div>

        <div class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity">
          <img src="@/assets/wexoe.svg" alt="WEXOE" />
        </div>

        <div class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity">
          <img src="@/assets/schneider.png" alt="Schneider" />
        </div>

        <div class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity">
          <img src="@/assets/lauritz.svg" alt="Lauritz Knudsen" />
        </div>

        <div class="flex items-center justify-center w-24 h-24 lg:w-32 lg:h-32 opacity-70 hover:opacity-100 transition-opacity">
          <img src="@/assets/solar.png" alt="Solar" />
        </div>
      </div>
    </div>

    <!-- Upcoming events -->
    <div class="py-16 px-8">
      <div class="flex justify-center pb-8">
        <h1 class="text-4xl text-slate-100">Kommende begivenheder</h1>
      </div>

      <div class="grid grid-cols-2 gap-10 px-24">
        <div>
          <h2 class="text-3xl text-slate-100 pb-4">Arrangementer</h2>

          <div class="py-4">
            <div class="border border-navy-700 rounded-2xl p-4 bg-navy-800">
              <h3 class="text-xl pb-4 text-slate-100">Arrangement titel</h3>
              <div class="p-4 items-center flex justify-between bg-navy-900 rounded-xl text-sm md:text-base lg:text-lg text-slate-400">
                <span>Dato</span>
                <span>Sektion</span>
                <span>Lokation</span>
              </div>
            </div>
          </div>

          <div class="py-4">
            <div class="border border-navy-700 rounded-2xl p-4 bg-navy-800">
              <h3 class="text-xl pb-4 text-slate-100">Arrangement titel</h3>
              <div class="p-4 items-center flex justify-between bg-navy-900 rounded-xl text-sm lg:text-lg text-slate-400">
                <span>Dato</span>
                <span>Sektion</span>
                <span>Lokation</span>
              </div>
            </div>
          </div>
        </div>

        <div>
          <h2 class="text-3xl text-slate-100 pb-4">Generalforsamlinger</h2>

          <div class="pt-8">
            <p class="text-xl text-slate-400">Der er ingen planlagte generalforsamlinger foreløbigt.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Info section 1 -->
    <div class="py-24 px-8 bg-navy-900 border-y border-amber-500/10 grid grid-cols-2 justify-center gap-x-8">
      <div class="flex flex-col gap-4 items-end">
        <img class="rounded-xl" src="@/assets/man-studying.png" alt="Man studying" />
      </div>

      <div class="flex flex-col gap-4 items-start justify-center">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest">Netværk</p>
        <h1 class="text-3xl text-slate-100 leading-tight">Sparring og relationer på tværs af branchen</h1>

        <p class="text-lg text-slate-400 leading-relaxed">
          Med over 550 medlemmer er du garanteret nye forbindelser og indsigter inden for
          forskellige eltekniske områder.
        </p>

        <button
          class="rounded-lg bg-amber-500 h-12 py-2 px-6 cursor-pointer hover:bg-amber-400 active:bg-amber-600 text-navy-950 font-semibold text-base shadow-lg shadow-amber-500/20 transition-all"
        >
          Medlemsfordele
        </button>
      </div>
    </div>

    <!-- Info section 2 -->
    <div class="py-24 px-8 grid grid-cols-2 justify-center gap-x-8">
      <div class="flex flex-col gap-4 items-start justify-center">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest">Udvikling</p>
        <h1 class="text-3xl text-slate-100 leading-tight">Fokus på faglig udvikling</h1>

        <p class="text-lg text-slate-400 leading-relaxed">
          Bliv klogere på de nyeste tendenser og andre specialeområder ved at deltage i
          arrangementer og virksomhedsbesøg.
        </p>

        <button
          class="rounded-lg bg-amber-500 h-12 py-2 px-6 cursor-pointer hover:bg-amber-400 active:bg-amber-600 text-navy-950 font-semibold text-base shadow-lg shadow-amber-500/20 transition-all"
        >
          Bliv medlem
        </button>
      </div>

      <div class="flex flex-col gap-4 items-start">
        <img class="rounded-xl" src="@/assets/corporate-woman.png" alt="Corporate woman" />
      </div>
    </div>

    <!-- Reasons to become a member -->
    <div class="py-16 px-8 bg-navy-900 border-y border-amber-500/10 grid grid-cols-2 justify-center gap-x-8">
      <div class="flex flex-col gap-4 items-start justify-center">
        <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest">Hvorfor?</p>
        <h1 class="text-3xl text-slate-100 leading-tight">3 gode grunde til at være medlem</h1>

        <p class="text-lg text-slate-400 leading-relaxed">
          Stadig i tvivl? Lad os lige skitsere, hvad du får ud af et medlemskab.
        </p>

        <div class="pt-4">
          <button
            class="rounded-lg bg-amber-500 h-12 py-2 px-6 cursor-pointer hover:bg-amber-400 active:bg-amber-600 text-navy-950 font-semibold text-base shadow-lg shadow-amber-500/20 transition-all"
          >
            Bliv medlem
          </button>
        </div>
      </div>

      <div class="flex flex-col gap-4 items-start">
        <div class="pb-4 w-full">
          <div
            class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl flex justify-center p-4 transition-colors"
          >
            <div class="py-2">
              <h2 class="text-xl text-amber-300 pb-2 font-semibold">Sparring</h2>
              <p class="text-slate-400">Diskuter og udveksl ideer med andre</p>
            </div>
          </div>
        </div>

        <div class="pb-4 w-full">
          <div
            class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl flex justify-center p-4 transition-colors"
          >
            <div class="py-2">
              <h2 class="text-xl text-amber-300 pb-2 font-semibold">Netværk</h2>
              <p class="text-slate-400">Få nye forbindelser — og måske venskaber</p>
            </div>
          </div>
        </div>

        <div class="w-full">
          <div
            class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl flex justify-center p-4 transition-colors"
          >
            <div class="py-2">
              <h2 class="text-xl text-amber-300 pb-2 font-semibold">Faglig udvikling</h2>
              <p class="text-slate-400">Styrk kompetencer og opnå ny viden</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Member plans -->
  <div class="py-16">
    <div class="flex justify-center">
      <h1 class="text-4xl text-slate-100">Vores medlemstyper</h1>
    </div>
    <div class="flex justify-center py-12">
      <div class="p-6 border border-amber-500/30 rounded-2xl bg-navy-800 shadow-xl shadow-amber-500/5 w-80">
        <div class="h-1 w-12 bg-amber-500 rounded-full mb-6"></div>
        <div class="pb-6">
          <h2 class="text-2xl text-slate-100">Bliv medlem</h2>
        </div>
        <div class="flex items-end pb-2">
          <h1 class="text-4xl text-amber-400 font-bold">700,-</h1>
          <span class="px-3 pb-1 text-slate-400 text-sm">inkl. moms pr. år</span>
        </div>
        <p class="pt-4 text-slate-400">Giv dig selv de bedste betingelser for et stærkt uddannelsesforløb.</p>
        <p class="pt-2 pb-6 text-slate-400">
          Deltag i studieture, del ud af din viden og styrk dit arbejdsnetværk.
        </p>
        <button
          class="flex justify-center rounded-lg bg-amber-500 h-12 py-2 w-full cursor-pointer hover:bg-amber-400 active:bg-amber-600 text-navy-950 font-semibold text-base shadow-lg shadow-amber-500/20 transition-all"
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

    <!-- News -->
    <div class="py-16 border-t border-amber-500/10">
      <div class="flex justify-center">
        <div>
          <div class="flex justify-center pb-4">
            <p class="text-amber-400 text-sm font-semibold uppercase tracking-widest">Aktiviteter</p>
          </div>
          <div class="flex justify-center pb-6">
            <h1 class="text-4xl text-slate-100">Nyheder og aktiviteter</h1>
          </div>
          <h2 class="text-xl text-slate-400 text-center">Følg med i de seneste nyheder og aktiviteter</h2>
        </div>
      </div>

      <div class="py-12 px-32 gap-6 grid grid-cols-3">
        <div class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl overflow-hidden transition-all hover:-translate-y-1 shadow-lg">
          <img class="w-full object-cover h-48" src="@/assets/stickman-bowling.jpeg" alt="activity picture" />
          <div class="p-4">
            <h1 class="pt-4 pb-2 text-xl text-slate-100">Arrangement titel</h1>
            <h2 class="text-lg pb-2 text-amber-300">Arrangement undertitel</h2>
            <p class="text-sm text-slate-400">Dato</p>
            <span class="text-sm text-slate-400">Lokation</span>
          </div>
        </div>
        <div class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl overflow-hidden transition-all hover:-translate-y-1 shadow-lg">
          <img class="w-full object-cover h-48" src="@/assets/stickman-party.jpeg" alt="activity picture" />
          <div class="p-4">
            <h1 class="pt-4 pb-2 text-xl text-slate-100">Arrangement titel</h1>
            <h2 class="text-lg pb-2 text-amber-300">Arrangement undertitel</h2>
            <p class="text-sm text-slate-400">Dato</p>
            <span class="text-sm text-slate-400">Lokation</span>
          </div>
        </div>
        <div class="bg-navy-800 border border-navy-700 hover:border-amber-500/40 rounded-2xl overflow-hidden transition-all hover:-translate-y-1 shadow-lg">
          <img class="w-full object-cover h-48" src="@/assets/stickmen-driving.jpeg" alt="activity picture" />
          <div class="p-4">
            <h1 class="pt-4 pb-2 text-xl text-slate-100">Arrangement titel</h1>
            <h2 class="text-lg pb-2 text-amber-300">Arrangement undertitel</h2>
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

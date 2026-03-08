<script setup lang="ts"></script>

<template>
  <!-- First section -->
  <div
    class="flex flex-col md:flex-row justify-center items-center gap-x-12 px-4 py-12 sm:py-20 mx-auto"
  >
    <div class="gap-4 w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
      <p class="text-theme-accent text-sm font-semibold uppercase tracking-widest pb-3">
        Medlemskab
      </p>
      <h1 class="text-3xl md:text-4xl pb-6 text-theme-heading leading-tight">
        Bliv medlem af Elektroteknisk forening i dag
      </h1>
      <p class="pb-8 text-theme-text leading-relaxed">
        Udvid dit netværk, opnå nye kompetencer, og accelerér din udvikling som studerende i
        elbranchen med overskuelige priser
      </p>
      <button
        class="flex justify-center rounded-lg bg-amber-600 h-12 px-8 py-2 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/25 transition-all"
      >
        Bliv medlem
      </button>
    </div>

    <div class="w-full md:w-96 px-2 mx-auto">
      <div
        class="bg-theme-mute border border-amber-500/30 rounded-2xl p-6 shadow-xl shadow-amber-500/5"
      >
        <!-- Amber top accent bar -->
        <div class="h-1 w-16 bg-amber-500 rounded-full mb-6"></div>
        <div class="flex justify-center mb-2">
          <h2 class="text-xl font-semibold text-theme-accent">Medlemskab</h2>
        </div>
        <div class="border-t border-theme-border pt-4 pb-4">
          <p class="py-2 text-theme-text">✅ Faglig udvikling</p>
          <p class="pb-2 text-theme-text">✅ Styrk dit netværk</p>
          <p class="pb-2 text-theme-text">✅ Mentorordning</p>
          <p class="pb-2 text-theme-text">✅ Studieture og arrangementer</p>
          <p class="pb-2 text-theme-text">✅ Modtag EL + Energi bladet</p>
        </div>
        <div class="flex justify-center mb-2">
          <h2 class="text-xl font-semibold text-theme-accent">Tilbud til studerende</h2>
        </div>
        <div class="border-t border-theme-border pt-4">
          <p class="py-2 flex items-start gap-2 text-theme-text">
            <span class="text-amber-500 mt-1">•</span> Mentorordning
          </p>
          <p class="pb-2 flex items-start gap-2 text-theme-text">
            <span class="text-amber-500 mt-1">•</span> Fagligt aktuelle netværksmøder
          </p>
          <p class="pb-2 flex items-start gap-2 text-theme-text">
            <span class="text-amber-500 mt-1">•</span> Netværksdatabase til projektskrivning
          </p>
          <p class="pb-2 flex items-start gap-2 text-theme-text">
            <span class="text-amber-500 mt-1">•</span> Måske det er her du finder dit kommende job
          </p>
          <p class="pb-2 flex items-start gap-2 text-theme-text">
            <span class="text-amber-500 mt-1">•</span> Inkl. alle fordele som andre medlemmer
          </p>
        </div>
      </div>
    </div>
  </div>

  <!-- Divider -->
  <div class="w-full border-t border-amber-500/10"></div>

  <!-- Second section -->
  <div
    class="bg-theme-soft flex flex-col md:flex-row justify-center items-center gap-x-12 px-4 py-12 sm:py-20 mx-auto"
  >
    <div class="gap-4 w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
      <p class="text-theme-accent text-sm font-semibold uppercase tracking-widest pb-3">Netværk</p>
      <h1 class="text-2xl sm:text-4xl pb-6 text-theme-heading leading-tight">
        Få adgang til netværksdatabase
      </h1>
      <p class="pb-6 text-theme-text leading-relaxed">
        Søger du specifikke kompetencer og indsigter i elbranchen? Som medlem i Elektroteknisk
        forening har du adgang til en komplet oversigt over andre medlemmer, som du kan søge
        sparring og ekspertise hos.
      </p>
    </div>

    <div class="hidden sm:flex w-96 gap-4 items-start">
      <img
        class="rounded-xl object-cover w-full"
        src="@/assets/advantages1.jpeg"
        alt="member advantages"
      />
    </div>
  </div>

  <!-- Divider -->
  <div class="w-full border-t border-amber-500/10"></div>

  <!-- Third section -->
  <div
    class="flex flex-col md:flex-row justify-center items-center gap-x-12 px-4 py-12 sm:py-20 mx-auto"
  >
    <div class="gap-4 w-full md:w-96 px-2 pb-8 md:pb-0 mx-auto">
      <p class="text-theme-accent text-sm font-semibold uppercase tracking-widest pb-3">
        Arrangementer
      </p>
      <h1 class="text-2xl sm:text-4xl pb-6 text-theme-heading leading-tight">
        Deltag i arrangementer og studieture
      </h1>
      <p class="pb-6 text-theme-text leading-relaxed">
        Vil du styrke din faglighed og få et bredt kendskab til elbranchen? I Elektroteknisk
        forening afholder vi studieture og besøger virksomheder, så du kan udvikle dine kompetencer
        og opnå ny viden.
      </p>
    </div>

    <div class="hidden sm:flex w-96 gap-4 items-start">
      <img
        class="rounded-xl object-cover w-full"
        src="@/assets/advantages2.jpeg"
        alt="member advantages"
      />
    </div>
  </div>

  <!-- Divider -->
  <div class="w-full border-t border-amber-500/10"></div>

  <!-- Fourth section -->
  <div class="bg-theme-soft px-4 py-16">
    <div class="text-center mb-10">
      <p class="text-theme-accent text-sm font-semibold uppercase tracking-widest pb-3">Fordele</p>
      <h1 class="text-2xl sm:text-3xl text-theme-heading">
        Andre gode grunde til at være med i Elektroteknisk forening
      </h1>
    </div>
    <!-- Triple columns -->
    <div class="flex flex-wrap justify-center gap-6 py-4">
      <!-- First column -->
      <div
        class="w-72 bg-theme-mute rounded-2xl border border-theme-border hover:border-amber-500/40 transition-all duration-300 shadow-lg hover:-translate-y-1 overflow-hidden"
      >
        <div class="h-44 overflow-hidden">
          <img
            class="w-full h-full object-cover"
            src="@/assets/advantages3.jpeg"
            alt="member advantages"
          />
        </div>
        <div class="p-5">
          <h2 class="text-lg font-semibold text-amber-300 pb-3">
            Bliv en del af vores mentorordning
          </h2>
          <p class="text-theme-text text-sm leading-relaxed">
            Som erfarent medlem i Elektroteknisk forening kan du dele ud af din viden ved at tage en
            studerende i elbranchen under dine vinger — og som studerende kan du sætte skub på din
            faglige udvikling med en personlig mentor.
          </p>
        </div>
      </div>

      <!-- Second column -->
      <div
        class="w-72 bg-theme-mute rounded-2xl border border-theme-border hover:border-amber-500/40 transition-all duration-300 shadow-lg hover:-translate-y-1 overflow-hidden"
      >
        <div class="h-44 overflow-hidden">
          <img
            class="w-full h-full object-cover"
            src="@/assets/advantages4.jpeg"
            alt="member advantages"
          />
        </div>
        <div class="p-5">
          <h2 class="text-lg font-semibold text-amber-300 pb-3">
            Søg sparring på tværs af branchen
          </h2>
          <p class="text-theme-text text-sm leading-relaxed">
            Lad dig inspirere af andre, og lad andre inspirere af dig. Elektroteknisk forening giver
            dig den ideelle platform til at udveksle og diskutere faglige indsigter med
            branchekollegaer fordelt over et væld af fagnicher.
          </p>
        </div>
      </div>

      <!-- Third column -->
      <div
        class="w-72 bg-theme-mute rounded-2xl border border-theme-border hover:border-amber-500/40 transition-all duration-300 shadow-lg hover:-translate-y-1 overflow-hidden"
      >
        <div class="h-44 overflow-hidden">
          <img
            class="w-full h-full object-cover"
            src="@/assets/advantages5.jpeg"
            alt="member advantages"
          />
        </div>
        <div class="p-5">
          <h2 class="text-lg font-semibold text-amber-300 pb-3">
            Få gratis abonnement på EL+ENERGI
          </h2>
          <p class="text-theme-text text-sm leading-relaxed">
            Som medlem i Elektroteknisk forening modtager du hvert kvartal gratis den seneste udgave
            af fagbladet EL+ENERGI hvori du kan finde nyheder og faglige informationer om alt, der
            rør sig inden for el, energi og installation.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>

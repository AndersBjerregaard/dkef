<script setup lang="ts"></script>

<template>
  <!-- First section -->
  <div class="flex flex-col md:flex-row justify-center items-center gap-x-8 px-4 py-4 sm:py-16 mx-auto">
    <div class="gap-4 w-screen md:w-96 px-2 pb-4 md:py-16 mx-auto">
      <h1 class="text-lg md:text-4xl pb-12">Bliv medlem af Elektroteknisk forening i dag</h1>
      <p class="pb-12">
        Udvid dit netværk, opnå nye kompetencer, og accelerér din udvikling som studerende i elbranchen med overskuelige priser
      </p>
      <button class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer hover:bg-gray-800 sm:text-lg">Bliv medlem</button>
    </div>

    <div class="gap-4 w-96 mx-auto">
      <div class="border bg-gray-700 rounded-2xl p-2">
        <div class="flex justify-center">
          <h2 class="text-xl py-4">Medlemskab</h2>
        </div>
        <div>
          <p class="py-4">✅ Faglig udvikling</p>
          <p class="pb-4">✅ Styrk dit netværk</p>
          <p class="pb-4">✅ Mentorordning</p>
          <p class="pb-4">✅ Studieture og arrangementer</p>
          <p class="pb-4">✅ Modtag EL + Energi bladet</p>
        </div>
        <div class="flex justify-center">
          <h2 class="text-xl py-4">Tilbud til studerende</h2>
        </div>
        <div>
          <p class="py-4">• Mentorordning</p>
          <p class="pb-4">• Fagligt aktuelle netværksmøder</p>
          <p class="pb-4">• Netværksdatabase til projektskrivning</p>
          <p class="pb-4">• Måske det er her du finder dit kommende job</p>
          <p class="pb-4">• Inkl. alle fordele som andre medlemmer</p>
        </div>
      </div>
    </div>
  </div>

  <!-- Second section -->
  <div class="flex justify-center gap-x-8 px-4 py-16">
    <div class="w-96 gap-4 items-start">
      <h1 class="text-4xl pb-12">Få adgang til netværksdatabase</h1>
      <p class="pb-12">
        Søger du specifikke kompetencer og indsigter i elbranchen? Som medlem i Elektroteknisk forening har du agang til en komplet oversigt over andre medlemmer, som du kan søge sparring og ekspertise hos.
      </p>
    </div>

    <div class="w-96 gap-4 items-start">
      <img src="@/assets/advantages1.jpeg" alt="member advantages">
    </div>
  </div>

  <!-- Third section -->
  <div class="flex justify-center gap-x-8 px-4 py-16">
    <div class="w-96 gap-4 items-start">
      <h1 class="text-4xl pb-12">Deltag i arrangementer og studieture</h1>
      <p class="pb-12">
        Vil du styrke din faglighed og få et bredt kendskab til elbranchen? I Elektroteknisk forening afholder vi studieture og besøger virksomheder, så du kan udvikle dine kompetencer og opnå ny viden.
      </p>
    </div>

    <div class="w-96 gap-4 items-start">
      <img src="@/assets/advantages2.jpeg" alt="member advantages">
    </div>
  </div>

  <!-- Fourth section -->
  <div class="justify-center px-4 py-16">
    <div class="text-center">
      <h1 class="text-2xl">Andre gode grunde til at være med i Elektroteknisk forening</h1>
    </div>
    <!-- Triple columns -->
    <div class="flex justify-center gap-x-8 py-4">
      <!-- First column -->
      <div class="gap-4 w-60">
        <div class="h-40">
          <img style="height: 100%; width: 100%; object-fit: fill;" src="@/assets/advantages3.jpeg" alt="member advantages">
        </div>
        <h2 class="text-lg py-4">Bliv en del af vores mentorordning</h2>
        <p>Som erfarent medlem i Elektroteknisk forening kan du dele ud af din viden ved at tage en studerende i elbranchen under dine vinger - og som studerende kan du sætte skub på din faglige udvikling med en personlig mentor.</p>
      </div>

      <!-- Second column -->
      <div class="gap-4 w-60">
        <div class="h-40">
          <img style="height: 100%; width: 100%; object-fit: fill;" src="@/assets/advantages4.jpeg" alt="member advantages">
        </div>
        <h2 class="text-lg py-4">Søg sparring på tværs af branchen</h2>
        <p>Lad dig inspirere af andre, og lad andre inspirere af dig. Elektroteknisk forening giver dig den ideelle platform til at udveksle og diskutere faglige indsigter med branchekollegaer fordelt over et væld af fagnicher.</p>
      </div>

      <!-- Third column -->
      <div class="gap-4 w-60">
        <div class="h-40">
          <img style="height: 100%; width: 100%; object-fit: fill;" src="@/assets/advantages5.jpeg" alt="member advantages">
        </div>
        <h2 class="text-lg py-4">Få gratis abonnoment på EL+ENERGI</h2>
        <p>Som medlem i Elektroteknisk forening modtager du hvert kvrtal gratis den seneste udgave af fagbladet EL+ENERGI hvori du kan finde nyheder og faglige informationer om alt, der rør sig inden for el, energi og installation.</p>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>

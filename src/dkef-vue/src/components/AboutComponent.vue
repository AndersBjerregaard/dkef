<script setup lang="ts">
import ContactFormularComponent from '@/components/ContactFormularComponent.vue'
</script>

<template>
  <div class="py-12">
    <div class="flex-row">
      <div class="flex justify-center items-center pb-4">
        <h1 class="text-4xl">Om Elektroteknisk forening</h1>
      </div>
      <div class="flex justify-center items-center pb-8">
        <h2 class="text-2xl">Vi sætter strøm til elteknik-branchen</h2>
      </div>
    </div>
    <div class="flex justify-center items-center gap-x-4 pb-8">
      <div class="w-[45%]">
        <div class="flex justify-center items-cemter pb-4">
          <h2 class="text-xl">Om foreningen</h2>
        </div>
        <div class="h-80 flex justify-center items-center text-center">
          <p class="text-lg">
            Hos Elektroteknisk forening samler vi elbranchen på tværs af faglige nicher, virksomheder og alle med en
            interesse for området. Vi er en traditionsrig forening, men vi er samtidig et moderne og dynamisk
            samlingspunkt, der nysgerrigt opsøger nye indsigter og perspektiver. Vores mål er at give vores medlemmer
            stærke muligheder for at skabe netværk og udvikle sig fagligt gennem både sparring med branchekollegaer og
            arrangementer.
          </p>
        </div>
      </div>
      <div class="w-[45%]">
        <div class="flex justify-center items-center pb-4">
          <h2 class="text-xl">Sektioner</h2>
        </div>
        <div class="h-80 bg-gray-400">
          <img style="width: 100%; height: 100%; object-fit: contain;" src="@/assets/dkef-sections.png">
        </div>
      </div>
    </div>
    <div class="flex-row">
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">Medlemmer</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-6xl">+550</h1>
      </div>
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">Sektioner</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-6xl">5</h1>
      </div>
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">Siden</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-6xl">1903</h1>
      </div>
      <div class="flex-row py-12">
        <div class="flex justify-center items-center text-center">
          <h1 class="text-9xl font-serif">"</h1>
        </div>
        <div class="flex justify-center items-center text-center pb-6">
          <h1 class="text-4xl">Udvid din faglige horisont, bliv en del af fællesskabet og modtag sparring fra erfarne
            hænder.</h1>
        </div>
        <div class="flex justify-center items-center text-center pb-12">
          <h2 class="text-2xl">Generelsekretær, René Kjemtrup</h2>
        </div>
        <div class="flex justify-center items-center text-center">
          <h1 class="text-9xl font-serif">"</h1>
        </div>
      </div>
    </div>
    <div class="flex justify-center items-center px-4 gap-x-4 pb-24">
      <div class="w-[45%] h-96">
        <div class="flex justify-center items-center text-center">
          <h1 class="text-2xl pb-4">Sekretariat</h1>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">Elektroteknisk forening</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">Elleholmvej 28C</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">5250 Odense SV</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <h3 class="text-xl pb-2">Tlf. 42 57 70 69</h3>
        </div>
        <div class="flex justify-center items-center text-center pb-4">
          <h3 class="text-xl pb-2">Email: info@dkef.dk</h3>
        </div>
        <div class="flex justify-center items-center text-center">
          <div class="h-48 w-48 border-2 rounded-full overflow-hidden">
            <img class="h-full w-full object-cover rounded-full" src="@/assets/secretary.jpg" alt="secretary">
          </div>
        </div>
      </div>
      <ContactFormularComponent />
    </div>
  </div>
</template>

<script setup lang="ts"></script>

<template>
  <div>
    <div class="py-8 px-8 grid grid-cols-2">
      <div>
        <div class="pb-20">
          <h1 class="header1">Dit netværk i elbranchen</h1>
        </div>

        <h2 class="text-xl pt-16 pb-16">Udvid din faglige horisont, blev en del af fællesskabet og modtag sparring fra
          erfarne
          hænder.
        </h2>

        <!-- <button>Medlemsfordele</button> -->

        <!-- <button>Eksisterende Medlemmer</button> -->

        <p class="pt-16 bold">
          ⭐ 4.8/5 stjerner af vores 100+ anmeldelser.
        </p>
      </div>

      <div class="pl-12">
        <img width="400" src="@/assets/el-mast.jpeg" alt="An electrical mast">
      </div>
    </div>

    <div class="bg-gray-600 px-8 py-16">
      <div class="text-4xl pb-8 flex items-center justify-center">
        <h1>Vores medlemsvirksomheder</h1>
      </div>

      <div class="flex justify-between">
        <div class="flex items-center justify-center">
          <img width="75" src="@/assets/cubic.png" alt="CUBIC">
        </div>

        <div class="flex items-center justify-center">
          <img width="200" src="@/assets/ledvance.png" alt="LEDVANCE">
        </div>

        <div class="flex items-center justify-center">
          <img width="125" src="@/assets/wexoe.svg" alt="WEXOE">
        </div>

        <div class="flex items-center justify-center">
          <img width="200" src="@/assets/schneider.png" alt="Schneider">
        </div>

        <div class="flex items-center justify-center">
          <img width="150" src="@/assets/lauritz.svg" alt="Lauritz Knudsen">
        </div>

        <img width="100" src="@/assets/solar.png" alt="Solar">
      </div>
    </div>

    <div class="py-16 px-8">
      <div class="flex justify-center pb-8">
        <h1 class="text-4xl">Kommende begivenheder</h1>
      </div>

      <div class="grid grid-cols-2 gap-10 px-24">
        <div>
          <h2 class="text-3xl">Arrangementer</h2>

          <p>
            Der er ingen planlagte arrangementer foreløbigt.
          </p>
        </div>

        <div>
          <h2 class="text-3xl">Generalforsamlinger</h2>

          <p>
            Der er ingen planlagte generalforsamlinger foreløbigt.
          </p>
        </div>
      </div>
    </div>

    <!-- Info section 1 -->
    <div class="py-32 px-32 flex justify-center">
      <div class="grid grid-cols-2">
        <div>
          <img width="400" src="@/assets/man-studying.png" alt="Man studying">
        </div>

        <div>
          <h1 class="text-3xl py-16">Sparring og relationer på tværs af branchen</h1>

          <p class="text-xl pb-8">
            Med over 550 medlemmer er du garanteret nye forbindelser og indsigter inden for forskellige eltekniske
            områder.
          </p>

          <button
            class="rounded bg-gray-600 h-12 py-2 px-4 cursor-pointer hover:bg-gray-800 text-lg">Medlemsfordele</button>
        </div>
      </div>
    </div>

    <!-- Info section 2 -->
    <div class="py-16 px-32 flex justify-center">
      <div class="grid grid-cols-2">
        <div>
          <h1 class="text-3xl py-16">Fokus på faglig udvikling</h1>

          <p class="text-xl pb-8">Bliv klogere på de nyeste tendenser og andre specialeområder ved at deltage i
            arrangementer og
            virksomhedsbesøg.
          </p>

          <button class="rounded bg-gray-600 h-12 py-2 px-4 cursor-pointer hover:bg-gray-800 text-lg">Bliv
            medlem</button>
        </div>

        <div>
          <img width="400" src="@/assets/corporate-woman.png" alt="Corporate woman">
        </div>
      </div>
    </div>

    <!-- Reasons to become a member -->
    <div>
      <h1>3 gode grunde til at være medlem</h1>

      <p>
        Stadig i tvivl? Lad os lige skitsere, hvad du får ud af et medlemskab.
      </p>

      <button>Bliv medlem</button>

      <h2>Sparring</h2>

      <p>
        Diskuter og udveksl ideer med andre
      </p>

      <h2>Netværk</h2>

      <p>
        Få nye forbindelser - og måske venskaber
      </p>

      <h2>Faglig udvikling</h2>

      <p>
        Styrk kompetencer og opnå ny viden
      </p>
    </div>
  </div>


</template>

<style lang="css" scoped>
.header1 {
  font-size: 2.25rem;
}

.bold {
  font-weight: 800;
}

.semi-bold {
  font-weight: 500;
}
</style>

<script setup lang="ts"></script>

<template>
  <div>
    <div class="grid grid-cols-2 justify-center gap-x-8">
      <!-- Left Column -->
      <div class="flex flex-col gap-4 items-end">
        <div class="w-5/6">
          <h1 class="text-4xl py-12">Dit netværk i elbranchen</h1>

          <h2 class="text-xl py-12">Udvid din faglige horisont, bliv en del af fællesskabet og modtaging sparring fra erfarne hænder.</h2>

          <h2 class="text-xl py-12">⭐ 4.8/5 stjerner af vores 100+ anmeldelser.</h2>
        </div>
      </div>
      <!--Right Column -->
      <div class="flex flex-col gap-4 items-start">
        <div class="w-5/6">
          <img src="@/assets/el-mast.jpeg" alt="An electrical mast">
        </div>
      </div>
    </div>

    <div class="bg-gray-600 px-8 py-16">
      <div class="text-4xl pb-8 flex items-center justify-center">
        <h1>Vores medlemsvirksomheder</h1>
      </div>

      <div class="flex justify-between">
        <div class="flex items-center justify-center w-24 h-24">
          <img src="@/assets/cubic.png" alt="CUBIC">
        </div>

        <div class="flex items-center justify-center w-24 h-24">
          <img src="@/assets/ledvance.png" alt="LEDVANCE">
        </div>

        <div class="flex items-center justify-center w-24 h-24">
          <img src="@/assets/wexoe.svg" alt="WEXOE">
        </div>

        <div class="flex items-center justify-center w-24 h-24">
          <img src="@/assets/schneider.png" alt="Schneider">
        </div>

        <div class="flex items-center justify-center w-24 h-24">
          <img src="@/assets/lauritz.svg" alt="Lauritz Knudsen">
        </div>

        <div class="flex items-center justify-center w-24 h-24">
          <img src="@/assets/solar.png" alt="Solar">
        </div>
      </div>
    </div>

    <div class="py-16 px-8">
      <div class="flex justify-center pb-8">
        <h1 class="text-4xl">Kommende begivenheder</h1>
      </div>

      <div class="grid grid-cols-2 gap-10 px-24">
        <div>
          <h2 class="text-3xl">Arrangementer</h2>

          <div class="py-4">
            <div class="border-1 border-gray-400 rounded-2xl p-4">
              <h3 class="text-xl pb-4">Arrangement titel</h3>
              <div class="p-4 items-center flex justify-between bg-gray-700">
                <span class="text-lg">Dato</span>
                <span class="text-xl">Sektion</span>
                <span class="text-xl">Lokation</span>
              </div>
            </div>
          </div>

          <div class="py-4">
            <div class="border-1 border-gray-400 rounded-2xl p-4">
              <h3 class="text-xl pb-4">Arrangement titel</h3>
              <div class="p-4 items-center flex justify-between bg-gray-700">
                <span class="text-lg">Dato</span>
                <span class="text-xl">Sektion</span>
                <span class="text-xl">Lokation</span>
              </div>
            </div>
          </div>
          <!-- <p>
            Der er ingen planlagte arrangementer foreløbigt.
          </p> -->
        </div>

        <div>
          <h2 class="text-3xl">Generalforsamlinger</h2>

          <div class="pt-8">
            <p class="text-xl">
              Der er ingen planlagte generalforsamlinger foreløbigt.
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Info section 1 -->
    <div class="py-32 px-46 flex justify-center">
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
    <div class="py-16 px-32 flex justify-center">
      <div class="grid grid-cols-2 gap-10">
        <div>
          <h1 class="text-3xl py-16">3 gode grunde til at være medlem</h1>

          <p class="text-lg py-12">
            Stadig i tvivl? Lad os lige skitsere, hvad du får ud af et medlemskab.
          </p>

          <div class="pt-8">
            <button class="rounded bg-gray-600 h-12 py-2 px-4 cursor-pointer hover:bg-gray-800 text-lg">Bliv
              medlem</button>
          </div>
        </div>

        <div>
          <div class="pb-4">
            <div class="border-gray-500 border-1 rounded-2xl flex justify-center">
              <div class="py-8">
                <h2 class="text-2xl pb-4">Sparring</h2>
                <p class="text-lg">
                  Diskuter og udveksl ideer med andre
                </p>
              </div>
            </div>
          </div>

          <div class="pb-4">
            <div class="border-gray-500 border-1 rounded-2xl flex justify-center">
              <div class="py-8">
                <h2 class="text-2xl pb-4">Netværk</h2>
                <p class="text-lg">
                  Få nye forbindelser - og måske venskaber
                </p>
              </div>
            </div>
          </div>

          <div class="border-gray-500 border-1 rounded-2xl flex justify-center">
            <div class="py-8">
              <h2 class="text-2xl pb-4">Faglig udvikling</h2>
              <p class="text-lg">
                Styrk kompetencer og opnå ny viden
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Member plans -->
  <div class="py-16">
    <div class="flex justify-center">
      <h1 class="text-4xl">Vores medlemstyper</h1>
    </div>
    <div class="flex justify-center py-16">
      <div class="p-4 border-1 rounded-2xl border-gray-500">
        <div class="pb-8">
          <h2 class="text-2xl">Bliv medlem</h2>
        </div>
        <div class="flex">
          <h1 class="text-4xl">700,-</h1><span class="px-4 self-center">inkl. moms pr. år</span>
        </div>
        <p class="pt-8">
          Giv dig selv de bedste betingelser for et stærkt uddannelsesforløb.
        </p>
        <p class="pt-4 pb-8">
          Deltag i studieture, del ud af din viden og styrk dit arbejdsnetværk.
        </p>
        <button
          class="flex justify-center rounded bg-gray-600 h-12 py-2 w-full cursor-pointer hover:bg-gray-800 text-lg">Bliv
          medlem</button>
        <div class="pt-4">
          <p class="py-4">✅ Faglig udvikling</p>
          <p class="pb-4">✅ Styrk dit netværk</p>
          <p class="pb-4">✅ Mentorordning</p>
          <p class="pb-4">✅ Studieture og arrangementer</p>
          <p>✅ Modtag EL + Energi bladet</p>
        </div>
      </div>
    </div>

    <!-- News -->
    <div class="py-16">
      <div class="flex justify-center">
        <div>
          <div class="flex justify-center pb-8">
            <h1 class="text-4xl">Nyheder og aktiviteter</h1>
          </div>
          <h2 class="text-2xl">Følg med i de seneste nyheder og aktiviteter</h2>
        </div>
      </div>

      <div class="py-16 px-32 gap-10 grid grid-cols-3">
        <div class="border-1 border-gray-600 rounded-2xl p-4">
          <img width="300" src="@/assets/stickman-bowling.jpeg" alt="activity picture">
          <h1 class="pt-8 pb-4 text-2xl">Arrangement titel</h1>
          <h2 class="text-xl pb-4">Arrangement undertitel</h2>
          <p class="text-lg">Dato</p>
          <span class="text-lg">Lokation</span>
        </div>
        <div class="border-1 border-gray-600 rounded-2xl p-4">
          <img width="300" src="@/assets/stickman-party.jpeg" alt="activity picture">
          <h1 class="pt-8 pb-4 text-2xl">Arrangement titel</h1>
          <h2 class="text-xl pb-4">Arrangement undertitel</h2>
          <p class="text-lg">Dato</p>
          <span class="text-lg">Lokation</span>
        </div>
        <div class="border-1 border-gray-600 rounded-2xl p-4">
          <img width="300" src="@/assets/stickmen-driving.jpeg" alt="activity picture">
          <h1 class="pt-8 pb-4 text-2xl">Arrangement titel</h1>
          <h2 class="text-xl pb-4">Arrangement undertitel</h2>
          <p class="text-lg">Dato</p>
          <span class="text-lg">Lokation</span>
        </div>
      </div>
    </div>
  </div>

  <!-- News letter -->
  <div class="pb-16">
    <div class="grid grid-cols-2 justify-center gap-x-8">
      <!-- Left Column -->
      <div class="flex flex-col gap-4 items-end">
        <div class="w-3/6 h-54">
          <h1 class="text-4xl">Vil du have besked som én af de første når der sker noget nyt?</h1>
        </div>
      </div>

      <!-- Right Column -->
      <div class="flex flex-col gap-4 items-start">
        <div class="w-3/6 h-54">
          <div class="py-4">
            <input id="name_input" name="name_input" placeholder="Navn" class="rounded bg-gray-800 p-2 w-full"
              type="text">
          </div>
          <div>
            <input id="email_input" name="email_input" placeholder="Email" class="rounded bg-gray-800 p-2 w-full"
              type="email">
          </div>
          <div class="py-4">
            <input
              class="flex justify-center rounded bg-gray-600 h-12 py-2 px-8 cursor-pointer hover:bg-gray-800 text-lg"
              type="submit" value="Timeld">
          </div>
        </div>
      </div>
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

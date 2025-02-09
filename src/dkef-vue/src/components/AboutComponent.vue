<script setup lang="ts"></script>

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
        <h1 class="text-4xl">+550</h1>
      </div>
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">Sektioner</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-4xl">5</h1>
      </div>
      <div class="flex justify-center items-center text-center pb-2">
        <h2 class="text-xl">Siden</h2>
      </div>
      <div class="flex justify-center items-center text-center pb-8">
        <h1 class="text-4xl">1903</h1>
      </div>
      <h1>"</h1>
      <h1>Udvid din faglige horisont, bliv en del af fællesskabet og modtag sparring fra erfarne hænder.</h1>
      <h2>Generelsekretær, René Kjemtrup</h2>
      <h1>"</h1>
    </div>
    <div>
      <div>
        <h1>Sekretariat</h1>
        <h1>Kontakt os</h1>
        <h3>Elektroteknisk forening</h3>
        <h3>Elleholmvej 28C</h3>
        <h3>5250 Odense SV</h3>
        <h3>Tlf. 42 57 70 69</h3>
        <h3>Email: info@dkef.dk</h3>
        <img src="@/assets/secretary.jpg" alt="secretary">
      </div>
      <div>
        <h1>Kontakt os</h1>
        <label for="name_input">Navn</label>
        <input id="name_input" name="name_input" placeholder="Navn" type="text">
        <label for="phone_input">Telefon</label>
        <input id="phone_input" name="phone_input" placeholder="Telefon" type="text">
        <label for="email_input">Email</label>
        <input id="email_input" name="email_input" placeholder="Email" type="email">
        <label for="message_input">Besked</label>
        <textarea id="message_input" name="message_input" placeholder="Skriv din besked her"></textarea>
        <input type="submit" value="Send besked">
      </div>
    </div>
  </div>
</template>

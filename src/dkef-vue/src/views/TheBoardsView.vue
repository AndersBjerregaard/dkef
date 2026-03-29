<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Contact, ContactCollection } from '@/types/members'
import { Section, SECTION_DISPLAY_MAP } from '@/types/members'
import api from '@/services/apiservice'
import BoardMemberCard from '@/components/BoardMemberCard.vue'

const boardMembers = ref<Contact[]>([])
const isLoading = ref(true)
const error = ref<string | null>(null)

// Sections in order of display
const sectionOrder = [
  Section.MainAssociation,
  Section.CopenhagenZealand,
  Section.Jutland,
  Section.NorthJutland,
  Section.SouthJutland,
  Section.Funen,
]

// Group members by section
const membersBySection = computed(() => {
  const grouped: Record<Section, Contact[]> = {
    [Section.MainAssociation]: [],
    [Section.CopenhagenZealand]: [],
    [Section.Jutland]: [],
    [Section.NorthJutland]: [],
    [Section.SouthJutland]: [],
    [Section.Funen]: [],
  }

  boardMembers.value.forEach((member) => {
    if (member.primarySection !== null) {
      grouped[member.primarySection].push(member)
    }
    if (member.secondarySection !== null && member.secondarySection !== member.primarySection) {
      grouped[member.secondarySection].push(member)
    }
  })

  return grouped
})

// Get sections that have members
const activeSections = computed(() => {
  return sectionOrder.filter((section) => membersBySection.value[section].length > 0)
})

// Fetch board members from API
async function fetchBoardMembers() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.get<ContactCollection>('/contacts', {
      params: {
        memberType: 1,
        take: 200,
      },
    })
    boardMembers.value = response.data.collection
  } catch (err) {
    console.error('Error fetching board members:', err)
    error.value = err instanceof Error ? err.message : 'Fejl ved hentning af bestyrelsesmedlemmer'
  } finally {
    isLoading.value = false
  }
}

// Scroll to section
function scrollToSection(section: Section) {
  const element = document.getElementById(`section-${section}`)
  if (element) {
    element.scrollIntoView({ behavior: 'smooth' })
  }
}

onMounted(() => {
  fetchBoardMembers()
})
</script>

<template>
  <main class="py-24">
    <div class="flex justify-center items-center mb-8">
      <h1 class="text-4xl">Bestyrelsen</h1>
    </div>

    <!-- Navigation buttons -->
    <div class="flex justify-center mb-12">
      <div class="flex flex-wrap gap-2 justify-center max-w-4xl">
        <button
          v-for="section in activeSections"
          :key="section"
          @click="scrollToSection(section)"
          class="inline-flex justify-center rounded-md border border-transparent bg-amber-600 text-navy-950 px-4 py-2 font-semibold focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 transition-colors cursor-pointer hover:bg-amber-500"
        >
          {{ SECTION_DISPLAY_MAP[section] }}
        </button>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="isLoading" class="flex justify-center items-center py-12">
      <p class="text-lg text-theme-mute">Henter bestyrelsesmedlemmer...</p>
    </div>

    <!-- Error state -->
    <div v-else-if="error" class="flex justify-center items-center py-12">
      <p class="text-lg text-red-500">{{ error }}</p>
    </div>

    <!-- Sections -->
    <div v-else class="max-w-5xl mx-auto px-4">
      <div
        v-for="section in activeSections"
        :key="section"
        :id="`section-${section}`"
        class="mb-12"
      >
        <h2 class="text-2xl font-bold mb-6 pb-2 border-b-2 border-theme-accent">
          {{ SECTION_DISPLAY_MAP[section] }}
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <BoardMemberCard
            v-for="member in membersBySection[section]"
            :key="member.id"
            :member="member"
          />
        </div>
      </div>

      <!-- No members message -->
      <div v-if="activeSections.length === 0" class="flex justify-center items-center py-12">
        <p class="text-lg text-theme-mute">Ingen bestyrelsesmedlemmer fundet</p>
      </div>
    </div>
  </main>
</template>

<style lang="css" scoped></style>

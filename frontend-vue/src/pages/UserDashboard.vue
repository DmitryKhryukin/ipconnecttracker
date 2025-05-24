<template>
  <div class="p-4 max-w-4xl mx-auto">
    <h2 class="text-2xl font-semibold mb-4">User Lookup by IP Prefix</h2>

    <div class="mb-4 flex gap-2">
      <input
        v-model="prefix"
        @keyup.enter="searchAndReset"
        type="text"
        placeholder="Enter IP prefix (e.g., 192.168)"
        class="border px-2 py-1 rounded w-64"
      />
      <button @click="searchAndReset" class="bg-blue-600 text-white px-4 py-1 rounded">Search</button>
    </div>

    <table v-if="userIds.length" class="table-auto w-full border-collapse border mb-4">
      <thead>
        <tr>
          <th class="border px-2 py-1">User ID</th>
          <th class="border px-2 py-1">Last Seen</th>
          <th class="border px-2 py-1">Details</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="userId in userIds" :key="userId">
          <td class="border px-2 py-1">{{ userId }}</td>
          <td class="border px-2 py-1">
            {{
              userConnections.find(c => c.userId === userId)?.timestamp || '—'
            }}
          </td>
          <td class="border px-2 py-1">
            <button @click="toggle(userId)" class="text-blue-600 underline">
              {{ selectedUserId === userId ? 'Hide' : 'Show' }} IPs
            </button>
          </td>
          <tr v-if="selectedUserId === userId" class="bg-gray-50">
          <td colspan="3">
            <UserIpList :userId="userId"/>
          </td>
        </tr>
        </tr>
      </tbody>
    </table>

    <div v-if="userIds.length" class="flex gap-2">
      <button :disabled="skip === 0" @click="prev">Prev</button>
      <span>Showing {{ skip + 1 }} - {{ skip + take }}</span>
      <button @click="next">Next</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { getUserConnectionsByIpPrefix, getUsersLastConnections } from '../services/connectionEventsService'
import UserIpList from '../components/UserIpList.vue'

const prefix = ref('192.')
const userIds = ref<number[]>([])
const userConnections = ref<{ userId: number; ip: string; timestamp: string }[]>([])
const selectedUserId = ref<number | null>(null)

const skip = ref(0)
const take = 20

async function searchAndReset() {
  skip.value = 0
  await loadPage()
}

async function loadPage() {
  const ids = await getUserConnectionsByIpPrefix(prefix.value, skip.value, take)
  userIds.value = ids
  selectedUserId.value = null

  userConnections.value = ids.length ? await getUsersLastConnections(ids) : []
}

function toggle(userId: number) {
  selectedUserId.value = selectedUserId.value === userId ? null : userId
}

async function next() {
  skip.value += take
  await loadPage()
}

async function prev() {
  if (skip.value >= take) {
    skip.value -= take
    await loadPage()
  }
}

// Initial load
searchAndReset()
</script>

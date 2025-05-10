<template>
  <div class="p-4 max-w-4xl mx-auto">
    <h2 class="text-2xl font-semibold mb-4">User Lookup by IP Prefix</h2>

    <div class="mb-4 flex gap-2">
      <input
        v-model="prefix"
        @keyup.enter="search"
        type="text"
        placeholder="Enter IP prefix (e.g., 192.168)"
        class="border px-2 py-1 rounded w-64"
      />
      <button @click="search" class="bg-blue-600 text-white px-4 py-1 rounded">Search</button>
    </div>

    <table v-if="users.length" class="table-auto w-full border-collapse border mb-4">
      <thead>
        <tr>
          <th class="border px-2 py-1">User ID</th>
          <th class="border px-2 py-1">Last Seen</th>
          <th class="border px-2 py-1">Details</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="user in users" :key="user.id">
          <td class="border px-2 py-1">{{ user.id }}</td>
          <td class="border px-2 py-1">
            <!--<UserLastSeen :userId="user.id" />-->
          </td>
          <td class="border px-2 py-1">
            <button @click="toggle(user.id)" class="text-blue-600 underline">
              {{ selectedUserId === user.id ? 'Hide' : 'Show' }} IPs
            </button>
          </td>
        </tr>
        <tr v-if="selectedUserId === user.id" class="bg-gray-50">
          <td colspan="3">
            <!--<UserIpList :userId="user.id" />-->
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="users.length" class="flex gap-2">
      <button :disabled="skip === 0" @click="prev">Prev</button>
      <span>Showing {{ skip + 1 }} - {{ skip + take }}</span>
      <button @click="next">Next</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { getUserConnectionsByIpPrefix } from '../services/userService'
/*import UserLastSeen from '@/components/UserLastSeen.vue'
import UserIpList from '@/components/UserIpList.vue'*/

const prefix = ref('192.')
const users = ref<{ id: number }[]>([])
const selectedUserId = ref<number | null>(null)

const skip = ref(0)
const take = 20

async function search() {
  skip.value = 0
  users.value = await getUserConnectionsByIpPrefix(prefix.value, skip.value, take)
  selectedUserId.value = null
}

function toggle(userId: number) {
  selectedUserId.value = selectedUserId.value === userId ? null : userId
}

async function next() {
  skip.value += take
  users.value = await getUserConnectionsByIpPrefix(prefix.value, skip.value, take)
}

async function prev() {
  if (skip.value >= take) {
    skip.value -= take
    users.value = await getUserConnectionsByIpPrefix(prefix.value, skip.value, take)
  }
}
</script>

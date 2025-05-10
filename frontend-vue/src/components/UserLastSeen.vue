<template>
  <span v-if="data">{{ data.timestamp }}</span>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getUserLastConnection } from '../services/connectionEventsService'

const { userId }  = defineProps<{ userId: number }>()
const data = ref<{ ip: string; timestamp: string } | null>(null)

onMounted(async () => {
  data.value = await getUserLastConnection(userId)
})
</script>

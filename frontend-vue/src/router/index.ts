import { createRouter, createWebHistory } from 'vue-router'
import UserDashboard from '../pages/UserDashboard.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: UserDashboard
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes
})

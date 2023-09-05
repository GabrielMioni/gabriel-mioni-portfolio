import { defineStore } from 'pinia'
import { state } from '@/store/projects/state.js'
import { getters } from '@/store/projects/getters.js'
import { actions } from '@/store/projects/actions.js'

export const useProjectsStore = defineStore('projects', {
  state,
  getters,
  actions,
})

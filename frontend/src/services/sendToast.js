import eventBus from '@/services/EventBus.js'

const messageTypes = {
  success: 'success',
  error: 'error',
  warning: 'warning',
  info: 'info'
}


const sendToast = ({ message, type = '', life = 3000 }) => {
  const severity = type ? messageTypes[type] : 'info'
  const summary = severity.charAt(0).toUpperCase() + severity.slice(1)
  eventBus.$emit('toast', { severity, summary, message, life })
}

export  {
  sendToast,
  messageTypes
}

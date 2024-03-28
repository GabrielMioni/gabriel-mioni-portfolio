import emitter from 'tiny-emitter/instance'

export default {
  $on: (...args) => emitter.on(...args),
  $off: (...args) => emitter.off(...args),
  $emit: (...args) => {
    console.log('EventBus emit:', ...args)
    emitter.emit(...args)
  }
}

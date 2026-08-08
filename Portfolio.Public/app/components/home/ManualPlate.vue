<script setup lang="ts">
const plateCellColumns = 20
const plateCellRows = 20
const plateCellCount = plateCellColumns * plateCellRows
const activePlateCells = ref(new Set<number>())
const plateIsAnimating = ref(false)
const manualPlate = ref<HTMLElement | null>(null)
const manualPlatePattern = ref<HTMLElement | null>(null)
const manualPlateMark = ref<HTMLElement | null>(null)
const plateMarkIsDragging = ref(false)
const plateMarkApex = reactive({ x: 50, y: 42 })

let plateMarkPointerId: number | null = null
let plateMarkDragStart = { pointerX: 0, pointerY: 0, apexX: 50, apexY: 42 }

const plateMarkStyle = computed<Record<string, string>>(() => ({
  '--manual-plate-apex-x': `${plateMarkApex.x}%`,
  '--manual-plate-apex-y': `${plateMarkApex.y}%`
}))

const clamp = (value: number, minimum: number, maximum: number) =>
  Math.min(Math.max(value, minimum), maximum)

const beginPlateMarkDrag = (event: PointerEvent) => {
  if (event.button !== 0) {
    return
  }

  plateMarkPointerId = event.pointerId
  plateMarkIsDragging.value = true
  plateMarkDragStart = {
    pointerX: event.clientX,
    pointerY: event.clientY,
    apexX: plateMarkApex.x,
    apexY: plateMarkApex.y
  }
  manualPlateMark.value?.setPointerCapture(event.pointerId)
}

const movePlateMark = (event: PointerEvent) => {
  const mark = manualPlateMark.value

  if (!mark || event.pointerId !== plateMarkPointerId) {
    return
  }

  const pointerDeltaX = event.clientX - plateMarkDragStart.pointerX
  const pointerDeltaY = event.clientY - plateMarkDragStart.pointerY
  const rotation = Math.PI / 4
  const localDeltaX = Math.cos(rotation) * pointerDeltaX + Math.sin(rotation) * pointerDeltaY
  const localDeltaY = -Math.sin(rotation) * pointerDeltaX + Math.cos(rotation) * pointerDeltaY
  const markSize = mark.offsetWidth

  plateMarkApex.x = clamp(
    plateMarkDragStart.apexX + (localDeltaX / markSize) * 100,
    22,
    78
  )
  plateMarkApex.y = clamp(
    plateMarkDragStart.apexY + (localDeltaY / markSize) * 100,
    20,
    74
  )
}

const endPlateMarkDrag = (event: PointerEvent) => {
  if (event.pointerId !== plateMarkPointerId) {
    return
  }

  if (manualPlateMark.value?.hasPointerCapture(event.pointerId)) {
    manualPlateMark.value.releasePointerCapture(event.pointerId)
  }

  plateMarkPointerId = null
  plateMarkIsDragging.value = false
  plateMarkApex.x = 50
  plateMarkApex.y = 42
}

const waitForPlateStep = () => new Promise(resolve => setTimeout(resolve, 65))

const pulsePlateCell = async (cell: number) => {
  activePlateCells.value = new Set([...activePlateCells.value, cell])

  await waitForPlateStep()

  const remainingCells = new Set(activePlateCells.value)
  remainingCells.delete(cell)
  activePlateCells.value = remainingCells
}

const getVisiblePlateBounds = () => {
  const plateElement = manualPlate.value
  const patternElement = manualPlatePattern.value

  if (!plateElement || !patternElement) {
    return {
      firstColumn: 0,
      lastColumn: plateCellColumns - 1,
      firstRow: 0,
      lastRow: plateCellRows - 1
    }
  }

  const plateBounds = plateElement.getBoundingClientRect()
  const patternBounds = patternElement.getBoundingClientRect()
  const plateStyles = getComputedStyle(plateElement)
  const visiblePlateBounds = {
    top: plateBounds.top + Number.parseFloat(plateStyles.borderTopWidth),
    right: plateBounds.right - Number.parseFloat(plateStyles.borderRightWidth),
    bottom: plateBounds.bottom - Number.parseFloat(plateStyles.borderBottomWidth),
    left: plateBounds.left + Number.parseFloat(plateStyles.borderLeftWidth)
  }
  const cellWidth = patternBounds.width / plateCellColumns
  const cellHeight = patternBounds.height / plateCellRows

  return {
    firstColumn: Math.max(
      0,
      Math.floor((visiblePlateBounds.left - patternBounds.left) / cellWidth)
    ),
    lastColumn: Math.min(
      plateCellColumns - 1,
      Math.ceil((visiblePlateBounds.right - patternBounds.left) / cellWidth) - 1
    ),
    firstRow: Math.max(
      0,
      Math.floor((visiblePlateBounds.top - patternBounds.top) / cellHeight)
    ),
    lastRow: Math.min(
      plateCellRows - 1,
      Math.ceil((visiblePlateBounds.bottom - patternBounds.top) / cellHeight) - 1
    )
  }
}

const choosePlateExitDirection = (
  column: number,
  firstVisibleColumn: number,
  lastVisibleColumn: number
) => {
  const leftDistance = column - firstVisibleColumn
  const rightDistance = lastVisibleColumn - column

  if (leftDistance === rightDistance) {
    return Math.random() < 0.5 ? -1 : 1
  }

  const nearestDirection = leftDistance < rightDistance ? -1 : 1

  return Math.random() < 0.65 ? nearestDirection : nearestDirection * -1
}

const dropPlateCell = async (cell: number) => {
  if (plateIsAnimating.value) {
    return
  }

  if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    activePlateCells.value = new Set([...activePlateCells.value, cell])
    return
  }

  plateIsAnimating.value = true

  const cellIndex = cell - 1
  const column = cellIndex % plateCellColumns
  const startRow = Math.floor(cellIndex / plateCellColumns)
  const visibleBounds = getVisiblePlateBounds()

  for (let row = startRow; row <= visibleBounds.lastRow; row++) {
    await pulsePlateCell(row * plateCellColumns + column + 1)
  }

  const exitDirection = choosePlateExitDirection(
    column,
    visibleBounds.firstColumn,
    visibleBounds.lastColumn
  )

  for (
    let exitColumn = column + exitDirection;
    exitColumn >= visibleBounds.firstColumn && exitColumn <= visibleBounds.lastColumn;
    exitColumn += exitDirection
  ) {
    await pulsePlateCell(visibleBounds.lastRow * plateCellColumns + exitColumn + 1)
  }

  plateIsAnimating.value = false
}

const dropRandomPlateCell = () => {
  const visibleBounds = getVisiblePlateBounds()
  const visibleColumns = visibleBounds.lastColumn - visibleBounds.firstColumn + 1
  const visibleRows = visibleBounds.lastRow - visibleBounds.firstRow + 1
  const randomColumn = visibleBounds.firstColumn + Math.floor(Math.random() * visibleColumns)
  const randomRow = visibleBounds.firstRow + Math.floor(Math.random() * visibleRows)
  const randomCell = randomRow * plateCellColumns + randomColumn + 1

  void dropPlateCell(randomCell)
}
</script>

<template>
  <figure>
    <div
      ref="manualPlate"
      class="manual-plate folio-focus-ring folio-focus-ring--roomy"
      role="button"
      tabindex="0"
      aria-label="Animate technical grid"
      @keydown.enter.prevent="dropRandomPlateCell"
      @keydown.space.prevent="dropRandomPlateCell">
      <span class="manual-plate-label">Exercise A–01</span>
      <div
        ref="manualPlatePattern"
        class="manual-plate-pattern">
        <span
          v-for="cell in plateCellCount"
          :key="cell"
          class="manual-plate-cell"
          :class="{ 'manual-plate-cell--active': activePlateCells.has(cell) }"
          aria-hidden="true"
          @click="dropPlateCell(cell)" />
      </div>
      <div
        ref="manualPlateMark"
        class="manual-plate-mark"
        :class="{ 'manual-plate-mark--dragging': plateMarkIsDragging }"
        :style="plateMarkStyle"
        aria-hidden="true"
        @pointerdown="beginPlateMarkDrag"
        @pointermove="movePlateMark"
        @pointerup="endPlateMarkDrag"
        @pointercancel="endPlateMarkDrag">
        <span class="manual-plate-facet manual-plate-facet--top" />
        <span class="manual-plate-facet manual-plate-facet--right" />
        <span class="manual-plate-facet manual-plate-facet--bottom" />
        <span class="manual-plate-facet manual-plate-facet--left" />
      </div>
    </div>
    <figcaption class="manual-plate-caption">
      <span>Fig. 01 — Elementary signal routing</span>
      <span>Pointer / keyboard input</span>
    </figcaption>
  </figure>
</template>

<style scoped>
.manual-plate {
  --folio-plate-rule: color-mix(in srgb, var(--folio-dark-ink) 22%, transparent);
  --folio-plate-shadow: color-mix(in srgb, var(--folio-dark-ink) 24%, transparent);

  position: relative;
  display: grid;
  width: calc(round(down, calc(100% - 2px), 2.25rem) + 2px);
  height: calc(27rem + 2px);
  margin-inline: auto;
  overflow: hidden;
  border: 1px solid var(--folio-ink);
  background: var(--folio-amber);
  place-items: center;
}

.manual-plate::before,
.manual-plate::after {
  position: absolute;
  width: 65%;
  aspect-ratio: 1;
  clip-path: polygon(50% 0, 100% 100%, 0 100%);
  content: '';
  pointer-events: none;
}

.manual-plate::before {
  z-index: 1;
  right: calc(-12% + 1rem);
  bottom: calc(-25% - .25rem);
  background: var(--folio-plate-shadow);
}

.manual-plate::after {
  z-index: 1;
  right: -12%;
  bottom: -25%;
  border: 1px solid var(--folio-ink);
  background: conic-gradient(
    from 0deg at 50% 0%,
    transparent 0deg 153deg,
    color-mix(in srgb, var(--folio-coral) 96%, var(--folio-paper-raised)) 153deg 175deg,
    color-mix(in srgb, var(--folio-coral) 90%, var(--folio-dark-ink)) 175deg 207deg,
    transparent 207deg 360deg
  ), var(--folio-coral);
}

.manual-plate-pattern {
  position: absolute;
  z-index: 0;
  top: round(nearest, calc((100% - 45rem) / 2), 2.25rem);
  left: round(nearest, calc((100% - 45rem) / 2), 2.25rem);
  display: grid;
  grid-template-columns: repeat(20, 2.25rem);
  grid-template-rows: repeat(20, 2.25rem);
  width: 45rem;
  height: 45rem;
}

.manual-plate-cell {
  min-width: 0;
  padding: 0;
  cursor: pointer;
  border-right: 1px solid var(--folio-plate-rule);
  border-bottom: 1px solid var(--folio-plate-rule);
  background-color: transparent;
  transition: background-color 240ms ease;
}

.manual-plate-cell:hover,
.manual-plate-cell--active {
  background-color: var(--folio-cyan);
}

.manual-plate-cell--active {
  transition-duration: 65ms;
}

.manual-plate-mark {
  position: relative;
  z-index: 1;
  display: grid;
  width: min(48%, 12rem);
  aspect-ratio: 1;
  overflow: hidden;
  border: 1px solid var(--folio-dark-ink);
  background: var(--folio-rust);
  box-shadow: -.45rem .95rem 0 var(--folio-plate-shadow);
  cursor: grab;
  rotate: 45deg;
  touch-action: none;
  user-select: none;
  place-items: center;
  will-change: rotate;
}

.manual-plate-mark--dragging {
  cursor: grabbing;
}

.manual-plate-facet {
  position: absolute;
  inset: 0;
  transition: clip-path 320ms ease-out;
}

.manual-plate-mark--dragging .manual-plate-facet {
  transition: none;
}

.manual-plate-facet--top {
  clip-path: polygon(
    0 0,
    100% 0,
    var(--manual-plate-apex-x) var(--manual-plate-apex-y)
  );
  background: color-mix(in srgb, var(--folio-rust) 92%, var(--folio-paper-raised));
}

.manual-plate-facet--right {
  clip-path: polygon(
    100% 0,
    100% 100%,
    var(--manual-plate-apex-x) var(--manual-plate-apex-y)
  );
  background: color-mix(in srgb, var(--folio-rust) 94%, var(--folio-dark-ink));
}

.manual-plate-facet--bottom {
  clip-path: polygon(
    100% 100%,
    0 100%,
    var(--manual-plate-apex-x) var(--manual-plate-apex-y)
  );
  background: color-mix(in srgb, var(--folio-rust) 88%, var(--folio-dark-ink));
}

.manual-plate-facet--left {
  clip-path: polygon(
    0 100%,
    0 0,
    var(--manual-plate-apex-x) var(--manual-plate-apex-y)
  );
  background: color-mix(in srgb, var(--folio-rust) 96%, var(--folio-paper-raised));
}

.manual-plate-mark:not(.manual-plate-mark--dragging):hover {
  animation: manual-plate-rock 520ms ease-out;
}

@keyframes manual-plate-rock {
  0%,
  100% {
    rotate: 45deg;
  }

  22% {
    rotate: 43.5deg;
  }

  48% {
    rotate: 46deg;
  }

  72% {
    rotate: 44.6deg;
  }
}

.manual-plate-label {
  position: absolute;
  z-index: 2;
  top: 1rem;
  right: 1rem;
  color: var(--folio-dark-ink);
  font-family: var(--folio-font-mono);
  font-size: .65rem;
  font-weight: 700;
  letter-spacing: .12em;
}

.manual-plate-caption {
  display: flex;
  justify-content: space-between;
  width: calc(round(down, calc(100% - 2px), 2.25rem) + 2px);
  gap: 1rem;
  margin-inline: auto;
  padding-top: .6rem;
  font-family: var(--folio-font-mono);
  font-size: .65rem;
  letter-spacing: .08em;
}

@media (max-width: 48rem) {
  .manual-plate {
    height: calc(20.25rem + 2px);
  }

  .manual-plate::before,
  .manual-plate::after {
    width: min(52%, 17rem);
  }

  .manual-plate::before {
    right: calc(-8% + .75rem);
    bottom: calc(-20% - .2rem);
  }

  .manual-plate::after {
    right: -8%;
    bottom: -20%;
  }
}

@media (prefers-reduced-motion: reduce) {
  .manual-plate-mark:not(.manual-plate-mark--dragging):hover {
    animation: none;
  }

  .manual-plate-facet {
    transition: none;
  }
}
</style>

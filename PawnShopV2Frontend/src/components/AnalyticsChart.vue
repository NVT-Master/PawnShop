<script setup>
import { computed } from 'vue'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart, LineChart, PieChart } from 'echarts/charts'
import {
  GridComponent,
  LegendComponent,
  TooltipComponent
} from 'echarts/components'
import VChart from 'vue-echarts'

use([
  CanvasRenderer,
  BarChart,
  LineChart,
  PieChart,
  GridComponent,
  LegendComponent,
  TooltipComponent
])

const props = defineProps({
  option: {
    type: Object,
    default: null
  },
  loading: Boolean,
  hasData: {
    type: Boolean,
    default: true
  },
  emptyMessage: {
    type: String,
    default: 'Chua co du lieu de hien thi'
  },
  height: {
    type: [Number, String],
    default: 320
  }
})

const chartHeight = computed(() => (
  typeof props.height === 'number' ? `${props.height}px` : props.height
))
</script>

<template>
  <div class="analytics-chart-shell" :style="{ minHeight: chartHeight, height: chartHeight }">
    <div v-if="loading" class="analytics-chart-state">
      Dang tai bieu do...
    </div>
    <div v-else-if="!hasData" class="analytics-chart-state">
      {{ emptyMessage }}
    </div>
    <VChart
      v-else
      :option="option"
      autoresize
      :style="{ height: chartHeight, width: '100%' }"
    />
  </div>
</template>

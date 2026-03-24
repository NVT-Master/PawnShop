<script setup>
import { computed, onMounted, ref } from 'vue'
import AnalyticsChart from '../components/AnalyticsChart.vue'
import BaseCard from '../components/BaseCard.vue'
import { contractService } from '../services'
import { formatCurrency, formatDate } from '../utils/format'
import { formatCompactCurrency, formatMonthYear } from '../utils/chartFormat'

const loading = ref(true)
const allContracts = ref([])
const dueSoonContracts = ref([])
const overdueContracts = ref([])

const statusMeta = {
  1: { label: 'Hoat dong', color: '#22c55e' },
  2: { label: 'Qua han', color: '#ef4444' },
  3: { label: 'Da gia han', color: '#3b82f6' },
  4: { label: 'Da dong', color: '#94a3b8' },
  5: { label: 'Da huy', color: '#64748b' },
  6: { label: 'Da tich thu', color: '#f59e0b' }
}

const activeContracts = computed(() => (
  allContracts.value.filter((contract) => [1, 2, 3].includes(contract.status))
))

const stats = computed(() => ({
  activeContracts: activeContracts.value.length,
  totalLoanAmount: activeContracts.value.reduce((sum, contract) => sum + Number(contract.loanAmount || 0), 0),
  dueSoonCount: dueSoonContracts.value.length,
  overdueCount: overdueContracts.value.length
}))

const statusChartData = computed(() => (
  Object.entries(statusMeta)
    .map(([status, meta]) => ({
      name: meta.label,
      value: allContracts.value.filter((contract) => contract.status === Number(status)).length,
      itemStyle: { color: meta.color }
    }))
    .filter((item) => item.value > 0)
))

const loanTrendData = computed(() => {
  const grouped = new Map()

  for (const contract of activeContracts.value) {
    const bucketDate = contract.startDate || contract.createdAt
    if (!bucketDate) continue

    const date = new Date(bucketDate)
    const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
    const existing = grouped.get(key) || {
      label: formatMonthYear(date),
      amount: 0,
      count: 0
    }

    existing.amount += Number(contract.loanAmount || 0)
    existing.count += 1
    grouped.set(key, existing)
  }

  return [...grouped.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .slice(-8)
    .map(([, value]) => value)
})

const hasStatusChartData = computed(() => statusChartData.value.length > 0)
const hasLoanTrendData = computed(() => loanTrendData.value.length > 0)

const statusChartOption = computed(() => ({
  backgroundColor: 'transparent',
  tooltip: {
    trigger: 'item',
    backgroundColor: '#0f172a',
    borderColor: '#334155',
    textStyle: { color: '#e2e8f0' }
  },
  legend: {
    bottom: 0,
    textStyle: { color: '#cbd5e1' }
  },
  series: [
    {
      type: 'pie',
      radius: ['48%', '74%'],
      center: ['50%', '42%'],
      itemStyle: {
        borderColor: '#1f2937',
        borderWidth: 4
      },
      label: {
        color: '#e2e8f0',
        formatter: '{b}\n{c}'
      },
      data: statusChartData.value
    }
  ]
}))

const loanTrendOption = computed(() => ({
  backgroundColor: 'transparent',
  tooltip: {
    trigger: 'axis',
    backgroundColor: '#0f172a',
    borderColor: '#334155',
    textStyle: { color: '#e2e8f0' },
    formatter: (params) => {
      const [amountPoint, countPoint] = params
      return [
        `<strong>${amountPoint.axisValue}</strong>`,
        `Giai ngan: ${formatCurrency(amountPoint.value)}`,
        `So hop dong: ${countPoint.value}`
      ].join('<br/>')
    }
  },
  legend: {
    top: 0,
    textStyle: { color: '#cbd5e1' }
  },
  grid: {
    left: 24,
    right: 24,
    top: 48,
    bottom: 24,
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: loanTrendData.value.map((item) => item.label),
    axisLabel: { color: '#94a3b8' },
    axisLine: { lineStyle: { color: '#334155' } }
  },
  yAxis: [
    {
      type: 'value',
      axisLabel: {
        color: '#94a3b8',
        formatter: (value) => formatCompactCurrency(value)
      },
      splitLine: { lineStyle: { color: 'rgba(148, 163, 184, 0.12)' } }
    },
    {
      type: 'value',
      axisLabel: { color: '#94a3b8' },
      splitLine: { show: false }
    }
  ],
  series: [
    {
      name: 'Giai ngan',
      type: 'bar',
      data: loanTrendData.value.map((item) => item.amount),
      barMaxWidth: 42,
      itemStyle: {
        color: '#38bdf8',
        borderRadius: [10, 10, 0, 0]
      }
    },
    {
      name: 'So hop dong',
      type: 'line',
      yAxisIndex: 1,
      smooth: true,
      symbolSize: 8,
      data: loanTrendData.value.map((item) => item.count),
      lineStyle: { color: '#f59e0b', width: 3 },
      itemStyle: { color: '#f59e0b' }
    }
  ]
}))

onMounted(async () => {
  loading.value = true

  try {
    const [dueSoonResponse, overdueResponse, allContractsResponse] = await Promise.all([
      contractService.getDueSoon(7),
      contractService.getOverdue(),
      contractService.getAll({ page: 1, pageSize: 1000 })
    ])

    dueSoonContracts.value = dueSoonResponse.data?.data || []
    overdueContracts.value = overdueResponse.data?.data || []
    allContracts.value = allContractsResponse.data?.data?.items || []
  } catch (error) {
    console.error('Error loading dashboard charts:', error)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-white mb-6">Bang dieu khien</h1>

    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4 mb-8">
      <BaseCard>
        <div class="text-center">
          <p class="text-3xl font-bold text-blue-400">{{ stats.activeContracts }}</p>
          <p class="text-slate-400 mt-1">Hop dong dang xu ly</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-3xl font-bold text-green-400">{{ formatCurrency(stats.totalLoanAmount) }}</p>
          <p class="text-slate-400 mt-1">Tong tien dang cho vay</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-3xl font-bold text-yellow-400">{{ stats.dueSoonCount }}</p>
          <p class="text-slate-400 mt-1">Sap den han trong 7 ngay</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-3xl font-bold text-red-400">{{ stats.overdueCount }}</p>
          <p class="text-slate-400 mt-1">Hop dong qua han</p>
        </div>
      </BaseCard>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-2 gap-6 mb-8">
      <BaseCard title="Dong giai ngan theo thang">
        <AnalyticsChart
          :loading="loading"
          :has-data="hasLoanTrendData"
          :option="loanTrendOption"
          empty-message="Chua co hop dong du de ve xu huong"
          :height="340"
        />
      </BaseCard>

      <BaseCard title="Co cau trang thai hop dong">
        <AnalyticsChart
          :loading="loading"
          :has-data="hasStatusChartData"
          :option="statusChartOption"
          empty-message="Chua co du lieu trang thai"
          :height="340"
        />
      </BaseCard>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <BaseCard title="Hop dong sap den han">
        <div v-if="loading" class="text-slate-400">Dang tai...</div>
        <div v-else-if="dueSoonContracts.length === 0" class="text-slate-400">
          Khong co hop dong sap den han
        </div>
        <div v-else class="overflow-x-auto">
          <table>
            <thead>
              <tr>
                <th>Ma HD</th>
                <th>Khach hang</th>
                <th>So tien</th>
                <th>Dao han</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="contract in dueSoonContracts.slice(0, 5)" :key="contract.id">
                <td>{{ contract.contractCode }}</td>
                <td>{{ contract.customer?.fullName }}</td>
                <td>{{ formatCurrency(contract.loanAmount) }}</td>
                <td>{{ formatDate(contract.dueDate) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </BaseCard>

      <BaseCard title="Hop dong qua han">
        <div v-if="loading" class="text-slate-400">Dang tai...</div>
        <div v-else-if="overdueContracts.length === 0" class="text-slate-400">
          Khong co hop dong qua han
        </div>
        <div v-else class="overflow-x-auto">
          <table>
            <thead>
              <tr>
                <th>Ma HD</th>
                <th>Khach hang</th>
                <th>So tien</th>
                <th>Dao han</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="contract in overdueContracts.slice(0, 5)"
                :key="contract.id"
                class="text-red-400"
              >
                <td>{{ contract.contractCode }}</td>
                <td>{{ contract.customer?.fullName }}</td>
                <td>{{ formatCurrency(contract.loanAmount) }}</td>
                <td>{{ formatDate(contract.dueDate) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </BaseCard>
    </div>
  </div>
</template>

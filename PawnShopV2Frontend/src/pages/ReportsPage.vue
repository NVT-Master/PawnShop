<script setup>
import { computed, onMounted, ref } from 'vue'
import AnalyticsChart from '../components/AnalyticsChart.vue'
import BaseCard from '../components/BaseCard.vue'
import { contractService } from '../services'
import { formatCurrency, formatDate } from '../utils/format'
import { formatCompactCurrency, formatMonthYear } from '../utils/chartFormat'

const contracts = ref([])
const interestSnapshots = ref([])
const loading = ref(true)

const activeContracts = computed(() => (
  contracts.value.filter((contract) => [1, 2, 3].includes(contract.status))
))

const stats = computed(() => {
  const totalInterest = interestSnapshots.value.reduce(
    (sum, item) => sum + Number(item.regularInterest || 0),
    0
  )
  const totalPenalty = interestSnapshots.value.reduce(
    (sum, item) => sum + Number(item.penaltyInterest || 0),
    0
  )
  const totalLoan = activeContracts.value.reduce(
    (sum, contract) => sum + Number(contract.loanAmount || 0),
    0
  )

  return {
    totalContracts: contracts.value.length,
    totalLoan,
    totalInterest,
    totalPenalty,
    totalReceivable: totalLoan + totalInterest + totalPenalty
  }
})

const receivableBreakdown = computed(() => ([
  { name: 'Von goc', value: stats.value.totalLoan, color: '#38bdf8' },
  { name: 'Lai thuong', value: stats.value.totalInterest, color: '#facc15' },
  { name: 'Phat qua han', value: stats.value.totalPenalty, color: '#f97316' },
  { name: 'Tong phai thu', value: stats.value.totalReceivable, color: '#a855f7' }
]))

const topCustomers = computed(() => {
  const grouped = new Map()

  for (const contract of activeContracts.value) {
    const name = contract.customer?.fullName || 'Khach le'
    grouped.set(name, (grouped.get(name) || 0) + Number(contract.loanAmount || 0))
  }

  return [...grouped.entries()]
    .map(([name, amount]) => ({ name, amount }))
    .sort((left, right) => right.amount - left.amount)
    .slice(0, 6)
    .reverse()
})

const assetCategoryMix = computed(() => {
  const grouped = new Map()

  for (const contract of contracts.value) {
    for (const asset of contract.assets || []) {
      const categoryName = asset.categoryName || 'Khac'
      const existing = grouped.get(categoryName) || {
        name: categoryName,
        value: 0,
        estimatedValue: 0
      }

      existing.value += 1
      existing.estimatedValue += Number(asset.estimatedValue || 0)
      grouped.set(categoryName, existing)
    }
  }

  return [...grouped.values()]
    .sort((left, right) => right.estimatedValue - left.estimatedValue)
    .map((item, index) => ({
      ...item,
      itemStyle: {
        color: ['#38bdf8', '#22c55e', '#f59e0b', '#a855f7', '#ef4444', '#14b8a6'][index % 6]
      }
    }))
})

const monthlyPortfolio = computed(() => {
  const grouped = new Map()

  for (const contract of contracts.value) {
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
    .slice(-10)
    .map(([, value]) => value)
})

const financialChartOption = computed(() => ({
  backgroundColor: 'transparent',
  tooltip: {
    trigger: 'axis',
    backgroundColor: '#0f172a',
    borderColor: '#334155',
    textStyle: { color: '#e2e8f0' },
    formatter: (params) => params
      .map((item) => `${item.marker}${item.name}: ${formatCurrency(item.value)}`)
      .join('<br/>')
  },
  grid: {
    left: 24,
    right: 24,
    top: 18,
    bottom: 24,
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: receivableBreakdown.value.map((item) => item.name),
    axisLabel: { color: '#94a3b8' },
    axisLine: { lineStyle: { color: '#334155' } }
  },
  yAxis: {
    type: 'value',
    axisLabel: {
      color: '#94a3b8',
      formatter: (value) => formatCompactCurrency(value)
    },
    splitLine: { lineStyle: { color: 'rgba(148, 163, 184, 0.12)' } }
  },
  series: [
    {
      type: 'bar',
      barMaxWidth: 56,
      data: receivableBreakdown.value.map((item) => ({
        value: item.value,
        itemStyle: {
          color: item.color,
          borderRadius: [12, 12, 0, 0]
        }
      }))
    }
  ]
}))

const customerChartOption = computed(() => ({
  backgroundColor: 'transparent',
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'shadow' },
    backgroundColor: '#0f172a',
    borderColor: '#334155',
    textStyle: { color: '#e2e8f0' },
    formatter: (params) => {
      const item = params[0]
      return `${item.name}<br/>Du no: ${formatCurrency(item.value)}`
    }
  },
  grid: {
    left: 24,
    right: 24,
    top: 18,
    bottom: 24,
    containLabel: true
  },
  xAxis: {
    type: 'value',
    axisLabel: {
      color: '#94a3b8',
      formatter: (value) => formatCompactCurrency(value)
    },
    splitLine: { lineStyle: { color: 'rgba(148, 163, 184, 0.12)' } }
  },
  yAxis: {
    type: 'category',
    data: topCustomers.value.map((item) => item.name),
    axisLabel: { color: '#cbd5e1' },
    axisLine: { lineStyle: { color: '#334155' } }
  },
  series: [
    {
      type: 'bar',
      data: topCustomers.value.map((item) => item.amount),
      itemStyle: {
        color: '#38bdf8',
        borderRadius: [0, 12, 12, 0]
      },
      barMaxWidth: 28
    }
  ]
}))

const assetMixChartOption = computed(() => ({
  backgroundColor: 'transparent',
  tooltip: {
    trigger: 'item',
    backgroundColor: '#0f172a',
    borderColor: '#334155',
    textStyle: { color: '#e2e8f0' },
    formatter: (item) => [
      `<strong>${item.name}</strong>`,
      `So tai san: ${item.value}`,
      `Tong dinh gia: ${formatCurrency(item.data.estimatedValue)}`
    ].join('<br/>')
  },
  legend: {
    bottom: 0,
    textStyle: { color: '#cbd5e1' }
  },
  series: [
    {
      type: 'pie',
      radius: ['42%', '72%'],
      center: ['50%', '42%'],
      itemStyle: {
        borderColor: '#1f2937',
        borderWidth: 4
      },
      label: {
        color: '#e2e8f0',
        formatter: '{b}\n{c}'
      },
      data: assetCategoryMix.value
    }
  ]
}))

const monthlyChartOption = computed(() => ({
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
        `Tong giai ngan: ${formatCurrency(amountPoint.value)}`,
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
    data: monthlyPortfolio.value.map((item) => item.label),
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
      name: 'Tong giai ngan',
      type: 'line',
      smooth: true,
      symbolSize: 8,
      data: monthlyPortfolio.value.map((item) => item.amount),
      lineStyle: { color: '#22c55e', width: 3 },
      itemStyle: { color: '#22c55e' },
      areaStyle: {
        color: 'rgba(34, 197, 94, 0.18)'
      }
    },
    {
      name: 'So hop dong',
      type: 'bar',
      yAxisIndex: 1,
      data: monthlyPortfolio.value.map((item) => item.count),
      barMaxWidth: 32,
      itemStyle: {
        color: '#a855f7',
        borderRadius: [10, 10, 0, 0]
      }
    }
  ]
}))

const hasFinancialData = computed(() => receivableBreakdown.value.some((item) => item.value > 0))
const hasTopCustomerData = computed(() => topCustomers.value.length > 0)
const hasAssetMixData = computed(() => assetCategoryMix.value.length > 0)
const hasMonthlyData = computed(() => monthlyPortfolio.value.length > 0)

onMounted(async () => {
  loading.value = true

  try {
    const contractsResponse = await contractService.getAll({ page: 1, pageSize: 1000 })
    contracts.value = contractsResponse.data?.data?.items || []

    const activeInterestRequests = activeContracts.value.map(async (contract) => {
      const response = await contractService.calculateInterest(contract.id)
      return response.data?.data
    })

    const settledResults = await Promise.allSettled(activeInterestRequests)
    interestSnapshots.value = settledResults
      .filter((item) => item.status === 'fulfilled' && item.value)
      .map((item) => item.value)
  } catch (error) {
    console.error('Error loading reports charts:', error)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-white mb-6">Bao cao thong ke</h1>

    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-5 gap-4 mb-8">
      <BaseCard>
        <div class="text-center">
          <p class="text-2xl font-bold text-blue-400">{{ stats.totalContracts }}</p>
          <p class="text-slate-400 text-sm mt-1">Tong hop dong</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-2xl font-bold text-green-400">{{ formatCurrency(stats.totalLoan) }}</p>
          <p class="text-slate-400 text-sm mt-1">Tong von dang quay</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-2xl font-bold text-yellow-400">{{ formatCurrency(stats.totalInterest) }}</p>
          <p class="text-slate-400 text-sm mt-1">Tong lai du kien</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-2xl font-bold text-orange-400">{{ formatCurrency(stats.totalPenalty) }}</p>
          <p class="text-slate-400 text-sm mt-1">Tong phat qua han</p>
        </div>
      </BaseCard>
      <BaseCard>
        <div class="text-center">
          <p class="text-2xl font-bold text-fuchsia-400">{{ formatCurrency(stats.totalReceivable) }}</p>
          <p class="text-slate-400 text-sm mt-1">Tong phai thu</p>
        </div>
      </BaseCard>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-2 gap-6 mb-8">
      <BaseCard title="Tong quan khoan phai thu">
        <AnalyticsChart
          :loading="loading"
          :has-data="hasFinancialData"
          :option="financialChartOption"
          empty-message="Chua co du lieu tai chinh"
          :height="320"
        />
      </BaseCard>

      <BaseCard title="Dong hop dong theo thang">
        <AnalyticsChart
          :loading="loading"
          :has-data="hasMonthlyData"
          :option="monthlyChartOption"
          empty-message="Chua co du lieu xu huong theo thang"
          :height="320"
        />
      </BaseCard>
    </div>

    <div class="grid grid-cols-1 xl:grid-cols-2 gap-6 mb-8">
      <BaseCard title="Top khach hang theo du no">
        <AnalyticsChart
          :loading="loading"
          :has-data="hasTopCustomerData"
          :option="customerChartOption"
          empty-message="Chua co khach hang du no"
          :height="340"
        />
      </BaseCard>

      <BaseCard title="Co cau tai san cam co">
        <AnalyticsChart
          :loading="loading"
          :has-data="hasAssetMixData"
          :option="assetMixChartOption"
          empty-message="Chua co tai san de phan tich"
          :height="340"
        />
      </BaseCard>
    </div>

    <BaseCard title="Danh sach hop dong gan day">
      <div v-if="loading" class="text-slate-400">Dang tai...</div>
      <div v-else class="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Ma HD</th>
              <th>Khach hang</th>
              <th>So tien vay</th>
              <th>Lai suat</th>
              <th>Ngay vay</th>
              <th>Trang thai</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="contract in contracts.slice(0, 20)" :key="contract.id">
              <td>{{ contract.contractCode }}</td>
              <td>{{ contract.customer?.fullName }}</td>
              <td>{{ formatCurrency(contract.loanAmount) }}</td>
              <td>{{ contract.interestRate }}%/ngay</td>
              <td>{{ formatDate(contract.startDate) }}</td>
              <td>
                <span
                  :class="[
                    'px-2 py-1 rounded text-xs',
                    contract.status === 1 ? 'bg-green-600' :
                    contract.status === 2 ? 'bg-red-600' :
                    contract.status === 3 ? 'bg-blue-600' :
                    contract.status === 4 ? 'bg-gray-600' :
                    contract.status === 6 ? 'bg-orange-600' : 'bg-slate-600'
                  ]"
                >
                  {{
                    contract.status === 1 ? 'Hoat dong' :
                    contract.status === 2 ? 'Qua han' :
                    contract.status === 3 ? 'Gia han' :
                    contract.status === 4 ? 'Da dong' :
                    contract.status === 6 ? 'Da tich thu' : 'Khac'
                  }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>

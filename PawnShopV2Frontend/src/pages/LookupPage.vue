<script setup>
import { onMounted, ref } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import BaseInput from '../components/BaseInput.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { contractService } from '../services'
import { useAuthStore } from '../store/auth'
import { ContractStatus } from '../utils/enums'
import { formatCurrency, formatDate } from '../utils/format'

const authStore = useAuthStore()
const contracts = ref([])
const loading = ref(true)
const searchCode = ref('')
const searchResult = ref(null)

onMounted(async () => {
  if (authStore.customerId) {
    try {
      const { data } = await contractService.getMyContracts()
      contracts.value = data?.data || []
    } catch (error) {
      console.error('Error:', error)
    } finally {
      loading.value = false
    }
  } else {
    loading.value = false
  }
})

const handleSearch = async () => {
  if (!searchCode.value) return

  try {
    const { data } = await contractService.getByCode(searchCode.value)
    if (data.success) {
      searchResult.value = data.data
    } else {
      searchResult.value = null
    }
  } catch (error) {
    searchResult.value = null
  }
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-white mb-6">Tra Cứu Hợp Đồng</h1>

    <BaseCard class="mb-6">
      <form @submit.prevent="handleSearch" class="flex gap-4">
        <BaseInput
          v-model="searchCode"
          placeholder="Nhập mã hợp đồng..."
          class="flex-1"
        />
        <button type="submit" class="btn btn-primary">Tìm kiếm</button>
      </form>

      <div v-if="searchResult" class="mt-4 bg-slate-700 p-4 rounded-lg">
        <div class="flex items-center gap-3 mb-2">
          <span class="text-lg font-semibold">{{ searchResult.contractCode }}</span>
          <StatusBadge :status="searchResult.status" :status-map="ContractStatus" />
        </div>
        <p class="text-slate-400">Số tiền vay: {{ formatCurrency(searchResult.loanAmount) }}</p>
        <p class="text-slate-400">Ngày vay: {{ formatDate(searchResult.startDate) }}</p>
        <p class="text-slate-400">Ngày đáo hạn: {{ formatDate(searchResult.dueDate) }}</p>
      </div>
    </BaseCard>

    <BaseCard title="Danh sách hợp đồng của bạn">
      <div v-if="loading" class="text-slate-400">Đang tải...</div>
      <div v-else-if="contracts.length === 0" class="text-slate-400">
        Bạn chưa có hợp đồng nào
      </div>
      <div v-else class="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Mã HĐ</th>
              <th>Số tiền vay</th>
              <th>Ngày vay</th>
              <th>Đáo hạn</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in contracts" :key="c.id">
              <td>{{ c.contractCode }}</td>
              <td>{{ formatCurrency(c.loanAmount) }}</td>
              <td>{{ formatDate(c.startDate) }}</td>
              <td>{{ formatDate(c.dueDate) }}</td>
              <td><StatusBadge :status="c.status" :status-map="ContractStatus" /></td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>

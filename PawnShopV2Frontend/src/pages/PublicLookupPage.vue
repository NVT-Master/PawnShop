<script setup>
import { ref } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import BaseInput from '../components/BaseInput.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { contractService } from '../services'
import { useUiStore } from '../store/ui'
import { ContractStatus } from '../utils/enums'
import { formatCurrency, formatDate } from '../utils/format'

const uiStore = useUiStore()

const form = ref({
  citizenId: '',
  phoneNumber: ''
})
const contracts = ref([])
const loading = ref(false)
const searched = ref(false)

const handleSearch = async () => {
  if (!form.value.citizenId || !form.value.phoneNumber) {
    uiStore.showToast('Vui lòng nhập đầy đủ thông tin', 'error')
    return
  }

  loading.value = true
  searched.value = true
  try {
    const { data } = await contractService.publicLookup({
      citizenId: form.value.citizenId,
      phoneNumber: form.value.phoneNumber
    })
    if (data.success) {
      contracts.value = data.data
      if (contracts.value.length === 0) {
        uiStore.showToast('Không tìm thấy hợp đồng nào', 'error')
      }
    } else {
      contracts.value = []
      uiStore.showToast(data.message || 'Không tìm thấy thông tin', 'error')
    }
  } catch (error) {
    contracts.value = []
    uiStore.showToast(error.response?.data?.message || 'Không tìm thấy thông tin', 'error')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-slate-900 py-12 px-4">
    <div class="max-w-4xl mx-auto">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-white">Tra Cứu Hợp Đồng</h1>
        <p class="text-slate-400 mt-2">Nhập thông tin để tra cứu hợp đồng cầm đồ của bạn</p>
      </div>

      <BaseCard>
        <form @submit.prevent="handleSearch" class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <BaseInput
            v-model="form.citizenId"
            label="CCCD/CMND"
            placeholder="Nhập số CCCD/CMND"
            required
          />
          <BaseInput
            v-model="form.phoneNumber"
            label="Số điện thoại"
            placeholder="Nhập số điện thoại"
            required
          />
          <div class="flex items-end">
            <button type="submit" :disabled="loading" class="btn btn-primary w-full">
              {{ loading ? 'Đang tìm...' : 'Tra cứu' }}
            </button>
          </div>
        </form>
      </BaseCard>

      <div v-if="searched && contracts.length > 0" class="mt-8 space-y-4">
        <h2 class="text-xl font-semibold text-white">Danh sách hợp đồng</h2>

        <BaseCard v-for="contract in contracts" :key="contract.id">
          <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <div class="flex items-center gap-3 mb-2">
                <span class="text-lg font-semibold text-white">{{ contract.contractCode }}</span>
                <StatusBadge :status="contract.status" :status-map="ContractStatus" />
              </div>
              <p class="text-slate-400">Số tiền vay: <span class="text-white">{{ formatCurrency(contract.loanAmount) }}</span></p>
              <p class="text-slate-400">Ngày vay: {{ formatDate(contract.startDate) }} - Ngày đáo hạn: {{ formatDate(contract.dueDate) }}</p>
            </div>
          </div>
        </BaseCard>
      </div>

      <div class="mt-8 text-center">
        <router-link to="/login" class="text-blue-400 hover:text-blue-300">
          Đăng nhập hệ thống
        </router-link>
      </div>
    </div>
  </div>
</template>

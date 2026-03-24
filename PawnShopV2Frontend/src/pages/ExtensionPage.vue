<script setup>
import { onMounted, ref } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { contractService } from '../services'
import { ContractStatus } from '../utils/enums'
import { formatCurrency, formatDate } from '../utils/format'

const contracts = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    const { data } = await contractService.getAll({ page: 1, pageSize: 100, status: 1 })
    contracts.value = (data?.data?.items || []).concat(
      ((await contractService.getAll({ page: 1, pageSize: 100, status: 3 })).data?.data?.items || [])
    )
  } catch (error) {
    console.error('Error:', error)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-white mb-6">Danh Sách Gia Hạn</h1>

    <BaseCard>
      <div class="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Mã HĐ</th>
              <th>Khách hàng</th>
              <th>Số tiền vay</th>
              <th>Đáo hạn</th>
              <th>Số lần gia hạn</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="6" class="text-center text-slate-400">Đang tải...</td>
            </tr>
            <tr v-else-if="contracts.length === 0">
              <td colspan="6" class="text-center text-slate-400">Không có hợp đồng nào</td>
            </tr>
            <tr v-else v-for="c in contracts" :key="c.id">
              <td>{{ c.contractCode }}</td>
              <td>{{ c.customer?.fullName }}</td>
              <td>{{ formatCurrency(c.loanAmount) }}</td>
              <td>{{ formatDate(c.dueDate) }}</td>
              <td>{{ c.extensionCount }}</td>
              <td><StatusBadge :status="c.status" :status-map="ContractStatus" /></td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>

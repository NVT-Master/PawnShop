<script setup>
import { onMounted, ref, watch } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import BaseInput from '../components/BaseInput.vue'
import BaseModal from '../components/BaseModal.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { assetCategoryService, contractService, customerService } from '../services'
import { useUiStore } from '../store/ui'
import { ContractStatus } from '../utils/enums'
import { formatCurrency, formatDate } from '../utils/format'

const uiStore = useUiStore()
const contracts = ref([])
const customers = ref([])
const categories = ref([])
const loading = ref(true)
const page = ref(1)
const totalPages = ref(1)
const filterStatus = ref('')

const showModal = ref(false)
const showActionsModal = ref(false)
const selectedContract = ref(null)
const form = ref({
  customerId: '',
  loanAmount: '',
  interestRate: '',
  startDate: new Date().toISOString().split('T')[0],
  dueDate: '',
  notes: '',
  assets: [{ name: '', categoryId: '', estimatedValue: '', description: '' }]
})

const loadData = async () => {
  loading.value = true
  try {
    const [contractsRes, customersRes, categoriesRes] = await Promise.all([
      contractService.getAll({ page: page.value, pageSize: 10, status: filterStatus.value || undefined }),
      customerService.getAll({ page: 1, pageSize: 1000 }),
      assetCategoryService.getActive()
    ])
    contracts.value = contractsRes.data?.data?.items || []
    totalPages.value = contractsRes.data?.data?.totalPages || 1
    customers.value = customersRes.data?.data?.items || []
    categories.value = categoriesRes.data?.data || []
  } catch (error) {
    uiStore.showToast('Lỗi tải dữ liệu', 'error')
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
watch([page, filterStatus], loadData)

const openCreateModal = () => {
  form.value = {
    customerId: '',
    loanAmount: '',
    interestRate: '0.3',
    startDate: new Date().toISOString().split('T')[0],
    dueDate: '',
    notes: '',
    assets: [{ name: '', categoryId: '', estimatedValue: '', description: '' }]
  }
  showModal.value = true
}

const addAsset = () => {
  form.value.assets.push({ name: '', categoryId: '', estimatedValue: '', description: '' })
}

const removeAsset = (index) => {
  if (form.value.assets.length > 1) {
    form.value.assets.splice(index, 1)
  }
}

const handleCreate = async () => {
  try {
    const payload = {
      customerId: parseInt(form.value.customerId),
      loanAmount: parseFloat(form.value.loanAmount),
      interestRate: parseFloat(form.value.interestRate),
      startDate: form.value.startDate,
      dueDate: form.value.dueDate,
      notes: form.value.notes,
      assets: form.value.assets.map(a => ({
        name: a.name,
        categoryId: parseInt(a.categoryId) || 6,
        estimatedValue: parseFloat(a.estimatedValue) || 0,
        description: a.description
      }))
    }
    await contractService.create(payload)
    uiStore.showToast('Tạo hợp đồng thành công')
    showModal.value = false
    loadData()
  } catch (error) {
    uiStore.showToast(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
  }
}

const openActionsModal = (contract) => {
  selectedContract.value = contract
  showActionsModal.value = true
}

const handleExtend = async () => {
  const days = prompt('Nhập số ngày gia hạn:', '30')
  if (!days) return

  try {
    await contractService.extend(selectedContract.value.id, {
      extensionDays: parseInt(days),
      newInterestRate: selectedContract.value.interestRate,
      capitalizeInterest: false,
      notes: 'Gia hạn hợp đồng'
    })
    uiStore.showToast('Gia hạn hợp đồng thành công')
    showActionsModal.value = false
    loadData()
  } catch (error) {
    uiStore.showToast(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
  }
}

const handleRedeem = async () => {
  try {
    const interestRes = await contractService.calculateInterest(selectedContract.value.id)
    const totalPayment = interestRes.data?.data?.totalPayment || 0

    const confirmed = await uiStore.showConfirm({
      title: 'Xác nhận chuộc hàng',
      message: `Tổng số tiền cần thanh toán: ${formatCurrency(totalPayment)}. Xác nhận chuộc hàng?`
    })

    if (confirmed) {
      await contractService.redeem(selectedContract.value.id, {
        paymentAmount: totalPayment,
        notes: 'Chuộc hàng'
      })
      uiStore.showToast('Chuộc hàng thành công')
      showActionsModal.value = false
      loadData()
    }
  } catch (error) {
    uiStore.showToast(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
  }
}

const handleForfeit = async () => {
  const confirmed = await uiStore.showConfirm({
    title: 'Xác nhận tịch thu',
    message: 'Bạn có chắc muốn tịch thu tài sản của hợp đồng này? Hành động này không thể hoàn tác!',
    danger: true
  })

  if (confirmed) {
    try {
      await contractService.forfeit(selectedContract.value.id, { notes: 'Tịch thu do quá hạn' })
      uiStore.showToast('Tịch thu tài sản thành công')
      showActionsModal.value = false
      loadData()
    } catch (error) {
      uiStore.showToast(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
    }
  }
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-white">Quản Lý Hợp Đồng</h1>
      <button @click="openCreateModal" class="btn btn-primary">+ Tạo hợp đồng</button>
    </div>

    <BaseCard>
      <div class="mb-4">
        <select v-model="filterStatus" class="w-full md:w-48">
          <option value="">Tất cả trạng thái</option>
          <option value="1">Hoạt động</option>
          <option value="2">Quá hạn</option>
          <option value="3">Đã gia hạn</option>
          <option value="4">Đã đóng</option>
          <option value="6">Đã tịch thu</option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Mã HĐ</th>
              <th>Khách hàng</th>
              <th>Số tiền vay</th>
              <th>Lãi suất</th>
              <th>Ngày vay</th>
              <th>Đáo hạn</th>
              <th>Trạng thái</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="8" class="text-center text-slate-400">Đang tải...</td>
            </tr>
            <tr v-else-if="contracts.length === 0">
              <td colspan="8" class="text-center text-slate-400">Không có hợp đồng nào</td>
            </tr>
            <tr v-else v-for="contract in contracts" :key="contract.id">
              <td class="font-medium">{{ contract.contractCode }}</td>
              <td>{{ contract.customer?.fullName }}</td>
              <td>{{ formatCurrency(contract.loanAmount) }}</td>
              <td>{{ contract.interestRate }}%/ngày</td>
              <td>{{ formatDate(contract.startDate) }}</td>
              <td>{{ formatDate(contract.dueDate) }}</td>
              <td><StatusBadge :status="contract.status" :status-map="ContractStatus" /></td>
              <td>
                <button
                  v-if="contract.status === 1 || contract.status === 2 || contract.status === 3"
                  @click="openActionsModal(contract)"
                  class="btn btn-secondary text-sm"
                >
                  Thao tác
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="totalPages > 1" class="flex justify-center gap-2 mt-4">
        <button
          v-for="p in totalPages"
          :key="p"
          @click="page = p"
          :class="['px-3 py-1 rounded', page === p ? 'bg-blue-600' : 'bg-slate-700 hover:bg-slate-600']"
        >
          {{ p }}
        </button>
      </div>
    </BaseCard>

    <!-- Create Modal -->
    <BaseModal :show="showModal" title="Tạo hợp đồng mới" size="lg" @close="showModal = false">
      <form @submit.prevent="handleCreate" class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-slate-300 mb-1">Khách hàng *</label>
            <select v-model="form.customerId" required class="w-full">
              <option value="">Chọn khách hàng</option>
              <option v-for="c in customers" :key="c.id" :value="c.id">{{ c.fullName }} - {{ c.citizenId }}</option>
            </select>
          </div>
          <BaseInput v-model="form.loanAmount" type="number" label="Số tiền vay" required />
          <BaseInput v-model="form.interestRate" type="number" step="0.01" label="Lãi suất (%/ngày)" required />
          <BaseInput v-model="form.startDate" type="date" label="Ngày vay" required />
          <BaseInput v-model="form.dueDate" type="date" label="Ngày đáo hạn" required />
        </div>

        <div>
          <div class="flex items-center justify-between mb-2">
            <label class="text-sm font-medium text-slate-300">Tài sản cầm cố</label>
            <button type="button" @click="addAsset" class="text-blue-400 text-sm">+ Thêm tài sản</button>
          </div>
          <div v-for="(asset, index) in form.assets" :key="index" class="bg-slate-700 p-4 rounded-lg mb-2">
            <div class="grid grid-cols-1 md:grid-cols-4 gap-3">
              <BaseInput v-model="asset.name" label="Tên tài sản" required />
              <div>
                <label class="block text-sm font-medium text-slate-300 mb-1">Danh mục</label>
                <select v-model="asset.categoryId" class="w-full">
                  <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                </select>
              </div>
              <BaseInput v-model="asset.estimatedValue" type="number" label="Giá trị" />
              <div class="flex items-end">
                <button
                  v-if="form.assets.length > 1"
                  type="button"
                  @click="removeAsset(index)"
                  class="btn btn-danger w-full"
                >
                  Xóa
                </button>
              </div>
            </div>
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-slate-300 mb-1">Ghi chú</label>
          <textarea v-model="form.notes" rows="2" class="w-full"></textarea>
        </div>

        <div class="flex justify-end gap-3">
          <button type="button" @click="showModal = false" class="btn btn-secondary">Hủy</button>
          <button type="submit" class="btn btn-primary">Tạo hợp đồng</button>
        </div>
      </form>
    </BaseModal>

    <!-- Actions Modal -->
    <BaseModal :show="showActionsModal" title="Thao tác hợp đồng" @close="showActionsModal = false">
      <div v-if="selectedContract" class="space-y-4">
        <div class="bg-slate-700 p-4 rounded-lg">
          <p class="text-lg font-semibold">{{ selectedContract.contractCode }}</p>
          <p class="text-slate-400">Khách hàng: {{ selectedContract.customer?.fullName }}</p>
          <p class="text-slate-400">Số tiền vay: {{ formatCurrency(selectedContract.loanAmount) }}</p>
        </div>

        <div class="grid grid-cols-1 gap-3">
          <button @click="handleExtend" class="btn btn-primary w-full">Gia hạn hợp đồng</button>
          <button @click="handleRedeem" class="btn btn-success w-full">Chuộc hàng</button>
          <button
            v-if="selectedContract.status === 2"
            @click="handleForfeit"
            class="btn btn-danger w-full"
          >
            Tịch thu tài sản
          </button>
        </div>
      </div>
    </BaseModal>
  </div>
</template>

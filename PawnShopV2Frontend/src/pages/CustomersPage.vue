<script setup>
import { onMounted, ref, watch } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import BaseInput from '../components/BaseInput.vue'
import BaseModal from '../components/BaseModal.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { customerService } from '../services'
import { useUiStore } from '../store/ui'
import { CustomerStatus } from '../utils/enums'
import { formatDate } from '../utils/format'

const uiStore = useUiStore()
const customers = ref([])
const loading = ref(true)
const search = ref('')
const page = ref(1)
const totalPages = ref(1)

const showModal = ref(false)
const editingCustomer = ref(null)
const form = ref({
  fullName: '',
  citizenId: '',
  phoneNumber: '',
  address: '',
  email: '',
  notes: ''
})

const loadCustomers = async () => {
  loading.value = true
  try {
    const { data } = await customerService.getAll({
      page: page.value,
      pageSize: 10,
      search: search.value
    })
    if (data.success) {
      customers.value = data.data.items
      totalPages.value = data.data.totalPages
    }
  } catch (error) {
    uiStore.showToast('Lỗi tải danh sách khách hàng', 'error')
  } finally {
    loading.value = false
  }
}

onMounted(loadCustomers)
watch([search, page], loadCustomers)

const openCreateModal = () => {
  editingCustomer.value = null
  form.value = { fullName: '', citizenId: '', phoneNumber: '', address: '', email: '', notes: '' }
  showModal.value = true
}

const openEditModal = (customer) => {
  editingCustomer.value = customer
  form.value = { ...customer }
  showModal.value = true
}

const handleSubmit = async () => {
  try {
    if (editingCustomer.value) {
      await customerService.update(editingCustomer.value.id, {
        ...form.value,
        status: editingCustomer.value.status
      })
      uiStore.showToast('Cập nhật khách hàng thành công')
    } else {
      await customerService.create(form.value)
      uiStore.showToast('Thêm khách hàng thành công')
    }
    showModal.value = false
    loadCustomers()
  } catch (error) {
    uiStore.showToast(error.response?.data?.message || 'Có lỗi xảy ra', 'error')
  }
}

const handleDelete = async (customer) => {
  const confirmed = await uiStore.showConfirm({
    title: 'Xác nhận xóa',
    message: `Bạn có chắc muốn xóa khách hàng "${customer.fullName}"?`,
    danger: true
  })
  if (confirmed) {
    try {
      await customerService.delete(customer.id)
      uiStore.showToast('Xóa khách hàng thành công')
      loadCustomers()
    } catch (error) {
      uiStore.showToast(error.response?.data?.message || 'Không thể xóa khách hàng', 'error')
    }
  }
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-white">Quản Lý Khách Hàng</h1>
      <button @click="openCreateModal" class="btn btn-primary">+ Thêm khách hàng</button>
    </div>

    <BaseCard>
      <div class="mb-4">
        <input
          v-model="search"
          type="text"
          placeholder="Tìm kiếm theo tên, CCCD, SĐT..."
          class="w-full md:w-80"
        />
      </div>

      <div class="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Họ tên</th>
              <th>CCCD</th>
              <th>Số điện thoại</th>
              <th>Trạng thái</th>
              <th>Ngày tạo</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="6" class="text-center text-slate-400">Đang tải...</td>
            </tr>
            <tr v-else-if="customers.length === 0">
              <td colspan="6" class="text-center text-slate-400">Không có khách hàng nào</td>
            </tr>
            <tr v-else v-for="customer in customers" :key="customer.id">
              <td>{{ customer.fullName }}</td>
              <td>{{ customer.citizenId }}</td>
              <td>{{ customer.phoneNumber }}</td>
              <td><StatusBadge :status="customer.status" :status-map="CustomerStatus" /></td>
              <td>{{ formatDate(customer.createdAt) }}</td>
              <td>
                <div class="flex gap-2">
                  <button @click="openEditModal(customer)" class="btn btn-secondary text-sm">Sửa</button>
                  <button @click="handleDelete(customer)" class="btn btn-danger text-sm">Xóa</button>
                </div>
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

    <BaseModal :show="showModal" :title="editingCustomer ? 'Sửa khách hàng' : 'Thêm khách hàng'" @close="showModal = false">
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <BaseInput v-model="form.fullName" label="Họ tên" required />
        <BaseInput v-model="form.citizenId" label="CCCD/CMND" required :disabled="!!editingCustomer" />
        <BaseInput v-model="form.phoneNumber" label="Số điện thoại" required />
        <BaseInput v-model="form.address" label="Địa chỉ" />
        <BaseInput v-model="form.email" label="Email" type="email" />
        <div>
          <label class="block text-sm font-medium text-slate-300 mb-1">Ghi chú</label>
          <textarea v-model="form.notes" rows="3" class="w-full"></textarea>
        </div>
        <div class="flex justify-end gap-3">
          <button type="button" @click="showModal = false" class="btn btn-secondary">Hủy</button>
          <button type="submit" class="btn btn-primary">{{ editingCustomer ? 'Cập nhật' : 'Thêm' }}</button>
        </div>
      </form>
    </BaseModal>
  </div>
</template>

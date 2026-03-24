<script setup>
import { onMounted, ref } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { assetCategoryService, assetService } from '../services'
import { AssetStatus } from '../utils/enums'
import { formatCurrency, formatDate } from '../utils/format'

const assets = ref([])
const categories = ref([])
const loading = ref(true)
const filterStatus = ref('')

const loadData = async () => {
  loading.value = true
  try {
    const [assetsRes, categoriesRes] = await Promise.all([
      assetService.getAll({ page: 1, pageSize: 100, status: filterStatus.value || undefined }),
      assetCategoryService.getAll()
    ])
    assets.value = assetsRes.data?.data?.items || []
    categories.value = categoriesRes.data?.data || []
  } catch (error) {
    console.error('Error loading assets:', error)
  } finally {
    loading.value = false
  }
}

onMounted(loadData)

const getCategoryName = (categoryId) => {
  return categories.value.find(c => c.id === categoryId)?.name || 'Không xác định'
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-white mb-6">Quản Lý Tài Sản</h1>

    <BaseCard>
      <div class="mb-4">
        <select v-model="filterStatus" @change="loadData" class="w-full md:w-48">
          <option value="">Tất cả trạng thái</option>
          <option value="1">Sẵn sàng</option>
          <option value="2">Đang cầm</option>
          <option value="3">Đã chuộc</option>
          <option value="4">Đã tịch thu</option>
          <option value="5">Đã thanh lý</option>
        </select>
      </div>

      <div class="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Tên tài sản</th>
              <th>Danh mục</th>
              <th>Giá trị</th>
              <th>Biển số/IMEI/SN</th>
              <th>Trạng thái</th>
              <th>Ngày tạo</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="6" class="text-center text-slate-400">Đang tải...</td>
            </tr>
            <tr v-else-if="assets.length === 0">
              <td colspan="6" class="text-center text-slate-400">Không có tài sản nào</td>
            </tr>
            <tr v-else v-for="asset in assets" :key="asset.id">
              <td>{{ asset.name }}</td>
              <td>{{ getCategoryName(asset.categoryId) }}</td>
              <td>{{ formatCurrency(asset.estimatedValue) }}</td>
              <td>{{ asset.licensePlate || asset.imei || asset.serialNumber || '-' }}</td>
              <td><StatusBadge :status="asset.status" :status-map="AssetStatus" /></td>
              <td>{{ formatDate(asset.createdAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </BaseCard>
  </div>
</template>

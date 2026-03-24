import api from './api'

export const authService = {
  login: (data) => api.post('/auth/login', data),
  register: (data) => api.post('/auth/register', data),
  changePassword: (data) => api.post('/auth/change-password', data),
  logout: () => api.post('/auth/logout'),
  getMe: () => api.get('/auth/me'),
  getUsers: () => api.get('/auth/users'),
  createUser: (data) => api.post('/auth/users', data)
}

export const customerService = {
  getAll: (params) => api.get('/customers', { params }),
  getById: (id) => api.get(`/customers/${id}`),
  getByCitizenId: (citizenId) => api.get(`/customers/by-citizen-id/${citizenId}`),
  create: (data) => api.post('/customers', data),
  update: (id, data) => api.put(`/customers/${id}`, data),
  delete: (id) => api.delete(`/customers/${id}`)
}

export const assetService = {
  getAll: (params) => api.get('/assets', { params }),
  getById: (id) => api.get(`/assets/${id}`),
  getAvailable: () => api.get('/assets/available'),
  create: (data) => api.post('/assets', data),
  update: (id, data) => api.put(`/assets/${id}`, data),
  delete: (id) => api.delete(`/assets/${id}`)
}

export const assetCategoryService = {
  getAll: () => api.get('/assetcategories'),
  getActive: () => api.get('/assetcategories/active'),
  getById: (id) => api.get(`/assetcategories/${id}`),
  create: (data) => api.post('/assetcategories', data),
  update: (id, data) => api.put(`/assetcategories/${id}`, data),
  delete: (id) => api.delete(`/assetcategories/${id}`)
}

export const contractService = {
  getAll: (params) => api.get('/contracts', { params }),
  getById: (id) => api.get(`/contracts/${id}`),
  getByCode: (code) => api.get(`/contracts/by-code/${code}`),
  getByCustomer: (customerId) => api.get(`/contracts/by-customer/${customerId}`),
  getDueSoon: (days = 7) => api.get('/contracts/due-soon', { params: { days } }),
  getOverdue: () => api.get('/contracts/overdue'),
  publicLookup: (params) => api.get('/contracts/public-lookup', { params }),
  getSoftCopy: (id) => api.get(`/contracts/${id}/soft-copy`),
  getMyContracts: () => api.get('/contracts/my-contracts'),
  calculateInterest: (id) => api.get(`/contracts/${id}/calculate-interest`),
  create: (data) => api.post('/contracts', data),
  update: (id, data) => api.put(`/contracts/${id}`, data),
  delete: (id) => api.delete(`/contracts/${id}`),
  extend: (id, data) => api.post(`/contracts/${id}/extend`, data),
  redeem: (id, data) => api.post(`/contracts/${id}/redeem`, data),
  forfeit: (id, data) => api.post(`/contracts/${id}/forfeit`, data)
}

export function formatCompactCurrency(value) {
  if (value === null || value === undefined) return '0 d'

  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    notation: 'compact',
    maximumFractionDigits: 1
  }).format(value)
}

export function formatMonthYear(date) {
  if (!date) return ''

  return new Date(date).toLocaleDateString('vi-VN', {
    month: 'short',
    year: '2-digit'
  })
}

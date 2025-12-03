import { useLazyQuery } from '@apollo/client'
import { GET_DIAGNOSES } from '@/lib/queries'
import { format } from 'date-fns'
import { cs } from 'date-fns/locale'
import { useState, useEffect, useRef } from 'react'
import Pagination from '@/components/ui/Pagination'
import { PaginationState } from '@/lib/types'

export default function DiagnosesList() {
  const [filters, setFilters] = useState({
    type: '',
    createdAfter: new Date(Date.UTC(new Date().getFullYear(), 0, 1)).toISOString().split('T')[0] + 'T00:00:00Z',
    createdBefore: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().split('T')[0] + 'T23:59:59Z',
    isActive: 'active'
  })
  const [hasFocus, setHasFocus] = useState(false)
  const typeInputRef = useRef<HTMLInputElement>(null)

  const [pagination, setPagination] = useState<PaginationState>({
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  })

  const [getDiagnoses, { loading, error, data }] = useLazyQuery(GET_DIAGNOSES)

  const diagnoses = data?.diagnoses?.nodes || []
  const totalCount = data?.diagnoses?.totalCount || 0
  // pageInfo is available but not currently used: data?.diagnoses?.pageInfo

  // Load data when filters or pagination change
  useEffect(() => {
    // Always load data when filters change, even if they're empty
    getDiagnoses({
      variables: {
        type: filters.type || null,
        createdAfter: filters.createdAfter || null,
        createdBefore: filters.createdBefore || null,
        isActive: filters.isActive === 'active' ? true : filters.isActive === 'inactive' ? false : null,
        skip: (pagination.currentPage - 1) * pagination.pageSize,
        take: pagination.pageSize
      }
    })
  }, [filters, pagination.currentPage, pagination.pageSize])

  // Restore focus after re-renders if input had focus before
  useEffect(() => {
    if (hasFocus && typeInputRef.current) {
      typeInputRef.current.focus()
    }
  })

  // Update pagination state when data changes
  useEffect(() => {
    if (totalCount !== pagination.totalCount) {
      const totalPages = Math.ceil(totalCount / pagination.pageSize)
      setPagination(prev => ({
        ...prev,
        totalCount,
        totalPages
      }))
    }
  }, [totalCount, pagination.pageSize, pagination.totalCount])

  const handleFilterChange = (field: string, value: string) => {
    setFilters(prev => ({ ...prev, [field]: value }))
    // Reset to first page when filter changes
    setPagination(prev => ({ ...prev, currentPage: 1 }))
  }

  const handleFocus = () => {
    setHasFocus(true)
  }

  const handleBlur = () => {
    setHasFocus(false)
  }

  const handlePageChange = (page: number) => {
    setPagination(prev => ({ ...prev, currentPage: page }))
    // The useEffect will automatically trigger the query with new pagination
  }

  const handlePageSizeChange = (pageSize: number) => {
    const newPage = Math.floor(((pagination.currentPage - 1) * pagination.pageSize) / pageSize) + 1
    setPagination(prev => ({
      ...prev,
      pageSize,
      currentPage: newPage
    }))
    // The useEffect will automatically trigger the query with new pagination
  }

  // Only show loading/error if we have data or are currently loading
  if (loading && data) return <div>Loading...</div>
  if (error) return <div>Error: {error.message}</div>

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold">Diagnoses List</h2>
        {totalCount > 0 && (
          <p className="text-sm text-gray-600">
            Showing {diagnoses.length} of {totalCount} diagnoses
          </p>
        )}
      </div>
      
      {/* Filter Section */}
      <div className="mb-6 p-4 bg-gray-50 rounded-lg">
        <h3 className="text-lg font-semibold mb-4">Filters</h3>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Type
            </label>
            <input
              ref={typeInputRef}
              type="text"
              value={filters.type}
              onChange={(e) => handleFilterChange('type', e.target.value)}
              onFocus={handleFocus}
              onBlur={handleBlur}
              placeholder="Filter by diagnosis type..."
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Created After
            </label>
            <input
              type="date"
              value={filters.createdAfter ? filters.createdAfter.split('T')[0] : ''}
              onChange={(e) => handleFilterChange('createdAfter', e.target.value ? e.target.value + 'T00:00:00Z' : '')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Created Before
            </label>
            <input
              type="date"
              value={filters.createdBefore ? filters.createdBefore.split('T')[0] : ''}
              onChange={(e) => handleFilterChange('createdBefore', e.target.value ? e.target.value + 'T23:59:59Z' : '')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select
              value={filters.isActive}
              onChange={(e) => handleFilterChange('isActive', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All Statuses</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </div>
        </div>
      </div>
      
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Patient
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Diagnosis
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Notes
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Date
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {diagnoses.map((diagnosis: any) => (
              <tr key={diagnosis.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {diagnosis.patient?.fullName || 'Unknown'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {diagnosis.diagnosis}
                </td>
                <td className="px-6 py-4 text-sm text-gray-500">
                  {diagnosis.notes}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {format(new Date(diagnosis.timestampUtc), 'd. M. yyyy HH:mm', { locale: cs })}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    diagnosis.isActive
                      ? 'bg-green-100 text-green-800'
                      : 'bg-red-100 text-red-800'
                  }`}>
                    {diagnosis.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {!data && (
        <div className="text-center py-8 text-gray-500">
          Use the filters above to search for diagnoses
        </div>
      )}
      
      {data && diagnoses.length === 0 && !loading && (
        <div className="text-center py-8 text-gray-500">
          No diagnoses found
        </div>
      )}

      {/* Pagination */}
      {totalCount > 0 && (
        <div className="mt-6">
          <Pagination
            currentPage={pagination.currentPage}
            totalPages={pagination.totalPages}
            totalCount={pagination.totalCount}
            pageSize={pagination.pageSize}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
            isLoading={loading}
          />
        </div>
      )}
    </div>
  )
}
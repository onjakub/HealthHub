import { useQuery } from '@apollo/client'
import { GET_DIAGNOSES } from '@/lib/queries'
import { format } from 'date-fns'
import { cs } from 'date-fns/locale'
import { useState } from 'react'

export default function DiagnosesList() {
  const [filters, setFilters] = useState({
    type: '',
    createdAfter: new Date(Date.UTC(new Date().getFullYear(), 0, 1)).toISOString().split('T')[0] + 'T00:00:00Z',
    createdBefore: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().split('T')[0] + 'T23:59:59Z'
  })

  const { loading, error, data, refetch } = useQuery(GET_DIAGNOSES, {
    variables: {
      type: filters.type,
      createdAfter: filters.createdAfter,
      createdBefore: filters.createdBefore,
      skip: 0,
      take: 10
    }
  })

  const handleFilterChange = (field: string, value: string) => {
    setFilters(prev => ({ ...prev, [field]: value }))
  }

  const applyFilters = () => {
    refetch({
      type: filters.type,
      createdAfter: filters.createdAfter,
      createdBefore: filters.createdBefore,
      skip: 0,
      take: 10
    })
  }

  if (loading) return <div>Loading...</div>
  if (error) return <div>Error: {error.message}</div>

  const diagnoses = data?.diagnoses?.nodes || []

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold mb-6">Diagnoses List</h2>
      
      {/* Filter Section */}
      <div className="mb-6 p-4 bg-gray-50 rounded-lg">
        <h3 className="text-lg font-semibold mb-4">Filters</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Type
            </label>
            <input
              type="text"
              value={filters.type}
              onChange={(e) => handleFilterChange('type', e.target.value)}
              placeholder="Filter by type..."
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Created After
            </label>
            <input
              type="date"
              value={filters.createdAfter.split('T')[0]}
              onChange={(e) => handleFilterChange('createdAfter', e.target.value + 'T00:00:00Z')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Created Before
            </label>
            <input
              type="date"
              value={filters.createdBefore.split('T')[0]}
              onChange={(e) => handleFilterChange('createdBefore', e.target.value + 'T23:59:59Z')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>
        <button
          onClick={applyFilters}
          className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          Apply Filters
        </button>
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
                  {diagnosis.patientId}
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

      {diagnoses.length === 0 && (
        <div className="text-center py-8 text-gray-500">
          No diagnoses found
        </div>
      )}
    </div>
  )
}